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
        public ActionResult CreateGroup(Group _groupData) 
        {
            if (!Group.ValidateModel(_groupData)) 
            {
                return BadRequest("Defect model");
            }

            _groupData.OwnerId = Functions.identityToUser(User.Identity, _context).UserId;

            _context.Groups.Add(_groupData);
            _context.SaveChanges();

            return Ok();
        }

        //api/Groups/profile/2
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
            if (!Group.ValidateModel(_groupData))
            {
                return BadRequest();
            }

            var mySelf = Functions.identityToUser(User.Identity, _context);

            var group = _context.Groups.Include(group => group.GroupInstruments)
                                        .Include(group => group.GroupGenres)
                                        .FirstOrDefault(group => group.GroupId == _groupId);

            if (group == null || group.OwnerId != mySelf.UserId) 
            {
                return NotFound();
            }

            group.GroupName = _groupData.GroupName;
            group.Description = _groupData.Description;
            group.ImagePath = _groupData.ImagePath;

            //Может не сработать
            _context.GroupInstruments.RemoveRange(group.GroupInstruments);
            _context.GroupGenres.RemoveRange(group.GroupGenres);

            group.GroupInstruments = _groupData.GroupInstruments;
            group.GroupGenres = _groupData.GroupGenres;

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
            var groups = _context.Groups.Where(group => group.OwnerId == mySelf.UserId);

            if (!groups.Any())
            {
                return NotFound();
            }

            var result = groups.ToList();

            return result;
        }
    }
}
