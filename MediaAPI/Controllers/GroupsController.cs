using MediaAPI.Controllers.FrequentlyUsed;
using MediaAPI.Models;
using MediaAPI.Models.EnumModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {

        private readonly MediaDBContext _context;

        public GroupsController(MediaDBContext _context)
        {
            this._context = _context;
        }

        //api/Groups
        [Authorize]
        [HttpPost]
        public ActionResult<int> CreateGroup(Group _groupData) 
        {
            if (!Group.ValidateModel(_groupData)) 
            {
                return BadRequest("Defect model");
            }

            _groupData.OwnerId = Functions.identityToUser(User.Identity, _context).UserId;

            //Проверяем все ли являются подписчиками
            foreach (var friend in _groupData.GroupMembers)
            {
                if (!_context.Subscriptions.Any(sub => sub.SubscriberId == _groupData.OwnerId && sub.ProviderId == friend.MemberId)) 
                {
                    return BadRequest(); //не является подписчиком
                }
                friend.Role = Role.user; //Перезаписываем
            }

            _context.Groups.Add(_groupData);
            _context.SaveChanges();

            return _groupData.GroupId;
        }

        //api/Groups/info/2
        [Route("info/{_groupId}")]
        [Authorize]
        [HttpGet]
        public ActionResult<Group> GetGroupInfo(int _groupId)
        {
            var group = _context.Groups.Include(group => group.GroupInstruments)
                                        .Include(group => group.GroupGenres)
                                        .FirstOrDefault(group => group.GroupId == _groupId);

            if (group == null)
            {
                return NotFound();
            }

            return group;
        }

        //api/Groups/edit/2
        [Route("edit/{_groupId}")]
        [HttpPut]
        public ActionResult EditGroupInfo(Group _groupData, int _groupId)
        {
            if (_groupData == null ||
                string.IsNullOrEmpty(_groupData.GroupName))
            {
                return BadRequest();
            }

            var mySelf = Functions.identityToUser(User.Identity, _context);

            var group = _context.Groups
                                        //.Include(group => group.GroupInstruments)
                                        //.Include(group => group.GroupGenres)
                                        .FirstOrDefault(group => group.GroupId == _groupId);

            if (group == null || group.OwnerId != mySelf.UserId) 
            {
                return NotFound();
            }

            group.GroupName = _groupData.GroupName;
            group.Description = _groupData.Description;
            if(!string.IsNullOrEmpty(_groupData.ImagePath)) group.ImagePath = _groupData.ImagePath;

            //Пока не трогаем
            //_context.GroupInstruments.RemoveRange(group.GroupInstruments);
            //_context.GroupGenres.RemoveRange(group.GroupGenres);

            //group.GroupInstruments = _groupData.GroupInstruments;
            //group.GroupGenres = _groupData.GroupGenres;

            _context.SaveChanges();

            return Ok();
        }

        //Добавлять после всех фильтрующих процессов
        private void AddExtraData(ref IQueryable<Group> _initialQuery)
        {
            _initialQuery = _initialQuery.Include(group => group.GroupInstruments)
                                            .Include(group => group.GroupGenres);
        }

        //api/Groups/Popular
        [Route("Popular")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<Group>> GetPopularGroups()
        {
            int LIMIT = 6;

            var groups = _context.Groups.Take(LIMIT);

            AddExtraData(ref groups);

            if (!groups.Any())
            {
                return NotFound();
            }

            var result = groups.ToList();

            return result;
        }

        //api/Groups/GroupedByGenre
        [Route("GroupedByGenre")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<KeyValuePair<Genre, List<Group>>>> GetPopularGroupedByGenre()
        {
            int LIMIT = 6;

            var groupGroups = new List<KeyValuePair<Genre, IQueryable<Group>>>();
            foreach (Genre genre in Enum.GetValues(typeof(Genre)))
            {
                //Найти видео с этим инструментом
                var relatedGroups = _context.Groups.Where(group => group.GroupGenres.Any(inst => inst.Genre == genre));

                //Пустые категории не включаем в список
                if (relatedGroups.Any())
                {
                    groupGroups.Add(KeyValuePair.Create(genre, relatedGroups));
                }
            }

            if (!groupGroups.Any())
            {
                return NotFound();
            }

            var result = new List<KeyValuePair<Genre, List<Group>>>();
            foreach (var groupGroup in groupGroups)
            {
                result.Add(KeyValuePair.Create(groupGroup.Key, groupGroup.Value.ToList()));
            }

            return result;
        }

        //api/Groups/FindByInstrument/2?limited=false
        [Route("FindByInstrument/{_instrumentCriteria}")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<Group>> GetGroupsByInstrument(bool limited, Instrument _instrumentCriteria)
        {
            var LIMIT = 3;

            var groups = _context.Groups.Where(group => group.GroupInstruments.Any(inst => inst.Instrument == _instrumentCriteria));
            if (limited) groups = groups.Take(LIMIT);
            AddExtraData(ref groups);

            if (!groups.Any())
            {
                return NotFound();
            }

            var result = groups.ToList();

            return result;
        }

        //api/Groups/FindByGenre/2?limited=false
        [Route("FindByGenre/{_genreCriteria}")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<Group>> GetGroupsByGenre(bool limited, Genre _genreCriteria)
        {
            var LIMIT = 3;

            var groups = _context.Groups.Where(group => group.GroupGenres.Any(genre => genre.Genre == _genreCriteria));
            if (limited) groups = groups.Take(LIMIT);
            AddExtraData(ref groups);

            if (!groups.Any())
            {
                return NotFound();
            }

            var result = groups.ToList();

            return result;
        }

        //api/Groups/MyGroups
        [Route("MyGroups")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<Group>> GetMyGroups()
        {
            var mySelf = Functions.identityToUser(User.Identity, _context);
            var groups = _context.Groups.Where(group => group.OwnerId == mySelf.UserId || group.GroupMembers.Any(mem => mem.MemberId == mySelf.UserId));

            if (!groups.Any())
            {
                return NotFound();
            }

            var result = groups.ToList();

            return result;
        }

        //api/Groups/members/2
        [Route("members/{_groupId}")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetGroupMembers(int _groupId) 
        {
            var group = _context.Groups.Include(group => group.GroupMembers)
                                        .ThenInclude(member => member.Member)
                                        .AsNoTracking()
                                        .FirstOrDefault(group => group.GroupId == _groupId);

            if (group == null) 
            {
                return NotFound();
            }

            var result = new List<User>();
            foreach (var friend in group.GroupMembers.Select(member => member.Member).ToList()) 
            {
                result.Add(Functions.getCleanUser(friend));
            }

            return result;
        }

        //api/Groups/friends/2
        [Route("friends/{_groupId}")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetFriends(int _groupId)
        {
            var mySelf = Functions.identityToUser(User.Identity, _context);

            var group = _context.Groups.Include(group => group.GroupMembers)
                                            .ThenInclude(member => member.Member)
                                            .AsNoTracking()
                                        .FirstOrDefault(group => group.GroupId == _groupId);

            if (group == null)
            {
                return NotFound();
            }

            //Если не владелец, то хотя бы должен быть модером
            if (group.OwnerId != mySelf.UserId) 
            {
                var myMembership = _context.GroupMembers.FirstOrDefault(mem => mem.MemberId == mySelf.UserId);

                var myRole = myMembership?.Role;

                if (myRole == null || myRole == Role.user) 
                {
                    return NotFound();
                }
            }

            var friendsQuery = _context.Subscriptions.Where(sub => sub.SubscriberId == mySelf.UserId)
                                                .Include(sub => sub.Provider)
                                                .Select(sub => sub.Provider);

            if (!friendsQuery.Any()) 
            {
                return NotFound();
            }

            var friends = friendsQuery.ToList();

            var members = group.GroupMembers.Select(member => member.Member).ToList();

            var memberless = friends.Where(friend => !members.Any(mem => mem.UserId == friend.UserId)).ToList();

            var result = new List<User>();
            foreach (var friend in memberless)
            {
                result.Add(Functions.getCleanUser(friend));
            }

            return result;
        }

        //api/Groups/toggleMembership/2/3
        [Route("toggleMembership/{_groupId}/{_userId}")]
        [Authorize]
        [HttpPut]
        public ActionResult<bool> ToggleMembership(int _groupId, int _userId) //membershrimp 🦈
        {
            var mySelf = Functions.identityToUser(User.Identity, _context);

            var group = _context.Groups.Find(_groupId);

            var member = _context.Users.Find(_userId);

            if (group == null || member == null || group.OwnerId != mySelf.UserId)
            {
                return NotFound();
            }

            var friendship = _context.Subscriptions.FirstOrDefault(sub => sub.SubscriberId == mySelf.UserId && sub.ProviderId == member.UserId);

            if (friendship == null) 
            {
                return Forbid();
            }

            var membership = _context.GroupMembers.FirstOrDefault(mem => mem.GroupId == group.GroupId && mem.MemberId == member.UserId);

            if (membership != null)
            {
                _context.GroupMembers.Remove(membership);
                _context.SaveChanges();
                return false;
            }
            else 
            {
                _context.GroupMembers.Add(new GroupMember() 
                {
                    GroupId = group.GroupId,
                    MemberId = member.UserId,
                    Role = Role.user
                });
                _context.SaveChanges();
                return true;
            }
        }

        //api/Groups/addMemberships/2
        [Route("addMemberships/{_groupId}")]
        [Authorize]
        [HttpPost]
        public ActionResult AddMemberships(int _groupId, IEnumerable<int> _userIds) 
        {
            var mySelf = Functions.identityToUser(User.Identity, _context);

            var group = _context.Groups.Find(_groupId);

            var friends = _context.Users.AsNoTracking().Where(user => _userIds.Any(id => id == user.UserId)).ToList();

            if (group == null || !friends.Any() || group.OwnerId != mySelf.UserId)
            {
                return NotFound();
            }

            foreach (var friend in friends) 
            {
                //if friendship exists
                if (!_context.Subscriptions.Any(sub => sub.SubscriberId == mySelf.UserId && sub.ProviderId == friend.UserId)) 
                {
                    return Forbid();
                }
            }

            var listofids = friends.Select(fr => fr.UserId);

            //get existing memberships/ костыль
            var memberships = _context.GroupMembers.Where(mem => mem.GroupId == group.GroupId && listofids.Any(fr => fr == (mem.MemberId ?? 0))).ToList();

            var nonmembers = friends.Where(fr => !memberships.Any(mem => (mem.MemberId ?? 0) == fr.UserId));

            foreach (var nonmember in nonmembers)
            {
                _context.GroupMembers.Add(new GroupMember()
                {
                    GroupId = group.GroupId,
                    MemberId = nonmember.UserId,
                    Role = Role.user
                });
            }

            _context.SaveChanges();
            return Ok();
        }

        //api/Groups/videos/2
        [Route("videos/{_groupId}")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<VideoFile>> GetVideos(int _groupId)
        {
            var group = _context.Groups.Find(_groupId);

            if (group == null) 
            {
                return NotFound();
            }

            var favourites = _context.GroupFavourites.Where(fav => fav.MediaFile.MediaType == MediaType.video && fav.GroupId == group.GroupId)
                                                    .Include(fav => fav.MediaFile)
                                                        .ThenInclude(media => media.VideoFile)
                                                    .Select(fav => fav.MediaFile);

            if (!favourites.Any()) 
            {
                return NotFound();
            }

            var mediaFiles = favourites.ToList();

            var result = new List<VideoFile>();
            foreach (var media in mediaFiles) 
            {
                var video = media.VideoFile;
                video.MediaFile = media;
                video.MediaFile.VideoFile = null;
                result.Add(video);
            }

            return result;
        }

        //api/Groups/music/2
        [Route("music/{_groupId}")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<MusicFile>> GetMusic(int _groupId)
        {
            var group = _context.Groups.Find(_groupId);

            if (group == null)
            {
                return NotFound();
            }

            var favourites = _context.GroupFavourites.Where(fav => fav.MediaFile.MediaType == MediaType.music && fav.GroupId == group.GroupId)
                                                    .Include(fav => fav.MediaFile)
                                                        .ThenInclude(media => media.MusicFile)
                                                    .Select(fav => fav.MediaFile);

            if (!favourites.Any())
            {
                return NotFound();
            }

            var mediaFiles = favourites.ToList();

            var result = new List<MusicFile>();
            foreach (var media in mediaFiles)
            {
                var music = media.MusicFile;
                music.MediaFile = media;
                music.MediaFile.MusicFile = null;
                result.Add(music);
            }

            return result;
        }

        //api/Groups/addFavourites/1/1
        [Route("addFavourites/{_groupId}/{_mediaType}")]
        [Authorize]
        [HttpPost]
        public ActionResult AddFavourites(int _groupId, List<int> _mediaIds, MediaType _mediaType) 
        {
            var mySelf = Functions.identityToUser(User.Identity, _context);

            var group = _context.Groups.Find(_groupId);

            if (group == null || group.OwnerId != mySelf.UserId) 
            {
                return Forbid();
            }

            IQueryable<MediaFile> mediaFiles;
            if (_mediaType == MediaType.video)
            {
                mediaFiles = _context.VideoFiles.Include(video => video.MediaFile)
                                            .Where(video => _mediaIds.Contains(video.VideoFileId))
                                            .Select(vid => vid.MediaFile);
            }
            else
            {
                mediaFiles = _context.MusicFiles.Include(music => music.MediaFile)
                                            .Where(music => _mediaIds.Contains(music.MusicFileId))
                                            .Select(vid => vid.MediaFile);
            }

            if (!mediaFiles.Any()) 
            {
                return NotFound();
            }

            var newFavourites = mediaFiles.ToList();
            var newFavIds = newFavourites.Select(fav => fav.MediaId);

            var existingFavourites = _context.GroupFavourites.Where(fav => newFavIds.Any(id => id == (fav.MediaFileId ?? 0)) && fav.GroupId == group.GroupId);

            if (existingFavourites.Any()) 
            {
                return Forbid();
            }

            foreach (var newFavourite in newFavourites) 
            {
                _context.GroupFavourites.Add(new GroupFavourite() 
                {
                    GroupId = group.GroupId,
                    MediaFileId = newFavourite.MediaId
                });
            }

            _context.SaveChanges();
            return Ok();
        }

        //api/Groups/toggleMedia/1/2
        [Route("toggleMusic/{_groupId}/{_mediaId}")]
        [Authorize]
        [HttpPut]
        public ActionResult<bool> ToggleMusic(int _groupId, int _mediaId, bool _isVideo) 
        {
            var media = _context.MediaFiles.Find(_mediaId);

            var group = _context.Groups.Find(_groupId);

            if (media == null || group == null) 
            {
                return NotFound();
            }

            var existingFavourite = _context.GroupFavourites.FirstOrDefault(fav => fav.GroupId == group.GroupId && fav.MediaFileId == media.MediaId);

            if (existingFavourite != null)
            {
                _context.GroupFavourites.Remove(existingFavourite);
                _context.SaveChanges();
                return false;
            }
            else
            {
                _context.GroupFavourites.Add(new GroupFavourite() 
                { 
                    GroupId = group.GroupId,
                    MediaFileId = media.MediaId
                });
                _context.SaveChanges();
                return true;
            }
        }

        //api/Groups/FindMusicByName/3?_nameCriteria=blablah
        [Route("FindMusicByName/{_groupId}")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<MusicFile>> FindMusicByName(int _groupId, string _nameCriteria)
        {
            var group = _context.Groups.Find(_groupId);

            IQueryable<MusicFile> music = _context.MusicFiles;

            if (!string.IsNullOrEmpty(_nameCriteria))
            {
                var nameCriteriaCaps = _nameCriteria.ToUpper();
                music = music.Where(music => music.MediaFile.MediaName.ToUpper().Contains(nameCriteriaCaps));
            }

            var favouritesIds = _context.GroupFavourites.Where(fav => fav.GroupId == group.GroupId && fav.MediaFile.MediaType == MediaType.music)
                                                        .Select(fav => fav.MediaFile.MediaId).ToList();

            music = music.Where(music => !favouritesIds.Contains(music.MediaFile.MediaId))
                            .Include(music => music.MediaFile)
                                .ThenInclude(media => media.Author);

            if (!music.Any())
            {
                return NotFound();
            }

            var result = music.ToList();

            //Костыль 
            foreach (var file in result) 
            {
                file.MediaFile.MusicFile = null;
                file.MediaFile.Author.Password = null;
                file.MediaFile.Author.Phone = null;
            }

            return result;
        }


        //api/Groups/FindVideosByName/3?_nameCriteria=blablah
        [Route("FindVideosByName/{_groupId}")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<VideoFile>> FindVideosByName(int _groupId, string _nameCriteria)
        {
            var group = _context.Groups.Find(_groupId);

            IQueryable<VideoFile> video = _context.VideoFiles;

            if (!string.IsNullOrEmpty(_nameCriteria))
            {
                var nameCriteriaCaps = _nameCriteria.ToUpper();
                video = video.Where(video => video.MediaFile.MediaName.ToUpper().Contains(nameCriteriaCaps));
            }

            var favouritesIds = _context.GroupFavourites.Where(fav => fav.GroupId == group.GroupId && fav.MediaFile.MediaType == MediaType.video)
                                                        .Select(fav => fav.MediaFile.MediaId).ToList();

            video = video.Where(video => !favouritesIds.Contains(video.MediaFile.MediaId))
                            .Include(video => video.MediaFile)
                                .ThenInclude(media => media.Author)
                            .Include(video => video.MediaFile)
                                .ThenInclude(media => media.MediaGenres);

            if (!video.Any())
            {
                return NotFound();
            }

            var result = video.ToList();

            //Костыль 
            foreach (var file in result)
            {
                file.MediaFile.VideoFile = null;
                file.MediaFile.Author.Password = null;
                file.MediaFile.Author.Phone = null;
            }

            return result;
        }
    }
}
