using MediaAPI.Controllers.FrequentlyUsed;
using MediaAPI.Models;
using MediaAPI.Models.EnumModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MediaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideosController : ControllerBase
    {
        private readonly MediaDBContext _context;

        public VideosController(MediaDBContext _context)
        {
            this._context = _context;
        }

        //api/Videos/
        [Authorize]
        [HttpPost]
        public ActionResult UploadVideo(VideoFile _videoData)
        {
            if (!VideoFile.ValidateModel(_videoData)) 
            {
                return BadRequest("Defect model");
            }

            _videoData.MediaFile.AuthorId = Functions.identityToUser(User.Identity, _context).UserId;
            _videoData.MediaFile.MediaType = Models.EnumModels.MediaType.video;

            _context.VideoFiles.Add(_videoData);
            _context.SaveChanges();

            return Ok();
        }

        //Добавлять после всех фильтрующих процессов
        private void AddExtraData(ref IQueryable<VideoFile> _initialQuery) 
        {
            _initialQuery = _initialQuery.Include(video => video.MediaFile)
                                                .ThenInclude(media => media.MediaGenres)
                                            .Include(video => video.MediaFile)
                                                .ThenInclude(media => media.MediaInstruments)
                                            .Include(video => video.MediaFile)
                                                .ThenInclude(media => media.Author);
        }
        //Вызывать перед отправкой
        private void CleanUsers(List<VideoFile> _result)
        {
            foreach (var video in _result)
            {
                video.MediaFile.Author = Functions.getCleanUser(video.MediaFile.Author);
            }
        }

        //api/Videos/Popular
        [Route("Popular")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<VideoFile>> GetPopularVideos()
        {
            int LIMIT = 6;

            var videos = _context.VideoFiles.Take(LIMIT);

            AddExtraData(ref videos);

            if (!videos.Any())
            {
                return NotFound();
            }

            var result = videos.ToList();
            CleanUsers(result);

            return result;
        }

        //api/Videos/FindByName?limited=false&_nameCriteria=blablah
        [Route("FindByName")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<VideoFile>> GetPopularByName(bool limited, string _nameCriteria)
        {
            int LIMIT = 3;

            IQueryable<VideoFile> videos = _context.VideoFiles;

            if (!string.IsNullOrEmpty(_nameCriteria))
            {
                var nameCriteriaCaps = _nameCriteria.ToUpper();
                videos = videos.Where(video => video.MediaFile.MediaName.ToUpper().Contains(nameCriteriaCaps));
            }

            if (limited) videos = videos.Take(LIMIT);
            AddExtraData(ref videos);

            if (!videos.Any())
            {
                return NotFound();
            }

            var result = videos.ToList();
            CleanUsers(result);

            return result;
        }

        //api/Videos/GroupedByInstrument
        [Route("GroupedByInstrument")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<KeyValuePair<Instrument, List<VideoFile>>>> GetPopularGroupedByInstrument()
        {
            int LIMIT = 3;

            //https://stackoverflow.com/a/2730206/15358244 - возможное решение, а пока используем дерьмовое

            var videoGroups = new List<KeyValuePair<Instrument, IQueryable<VideoFile>>>();
            foreach (Instrument instrument in Enum.GetValues(typeof(Instrument))) 
            {
                //Найти видео с этим инструментом
                var relatedVideos = _context.VideoFiles.Where(video => video.MediaFile.MediaInstruments.Any(inst => inst.Instrument == instrument));

                //Пустые категории не включаем в список
                if (relatedVideos.Any())
                {
                    AddExtraData(ref relatedVideos);
                    videoGroups.Add(KeyValuePair.Create(instrument, relatedVideos));
                }
            }

            if (!videoGroups.Any())
            {
                return NotFound();
            }

            var result = new List<KeyValuePair<Instrument, List<VideoFile>>>();
            foreach (var videoGroup in videoGroups) 
            {
                result.Add(KeyValuePair.Create(videoGroup.Key, videoGroup.Value.ToList()));
                CleanUsers(result.Last().Value);
            }

            return result;
        }

        //api/Videos/FindByInstrument/2
        [Route("FindByInstrument/{_instrumentCriteria}")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<VideoFile>> GetVideosByInstrument(Instrument _instrumentCriteria)
        {
            var videos = _context.VideoFiles.Where(video => video.MediaFile.MediaInstruments.Any(inst => inst.Instrument == _instrumentCriteria));
            AddExtraData(ref videos);

            if (!videos.Any())
            {
                return NotFound();
            }

            var result = videos.ToList();
            CleanUsers(result);

            return result;
        }

        //api/Videos/FindByGenre/2
        [Route("FindByGenre/{_genreCriteria}")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<VideoFile>> GetVideosByGenre(Genre _genreCriteria)
        {
            var videos = _context.VideoFiles.Where(video => video.MediaFile.MediaGenres.Any(genre => genre.Genre == _genreCriteria));
            AddExtraData(ref videos);

            if (!videos.Any())
            {
                return NotFound();
            }

            var result = videos.ToList();
            CleanUsers(result);

            return result;
        }

        //api/Videos/MyUploads
        [Route("MyUploads")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<VideoFile>> GetMyUploads()
        {
            var mySelf = Functions.identityToUser(User.Identity, _context);
            var videos = _context.VideoFiles.Where(video => video.MediaFile.AuthorId == mySelf.UserId).Include(video => video.MediaFile); //no extra data this time
            
            if (!videos.Any())
            {
                return NotFound();
            }

            var result = videos.ToList();

            return result;
        }

        //WIP
        //api/Videos/Favorite 
        [Route("Favorites")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<VideoFile>> GetFavorites()
        {
            var mySelf = Functions.identityToUser(User.Identity, _context);
            var videos = _context.VideoFiles;

            if (!videos.Any())
            {
                return NotFound();
            }

            var result = videos.ToList();

            return result;
        }
    }
}
