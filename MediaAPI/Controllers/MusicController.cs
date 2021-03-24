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
    public class MusicController : ControllerBase
    {
        private readonly MediaDBContext _context;

        public MusicController(MediaDBContext _context)
        {
            this._context = _context;
        }

        //api/Musics/
        [Authorize]
        [HttpPost]
        public ActionResult UploadMusic(MusicFile _musicData)
        {
            if (!MusicFile.ValidateModel(_musicData))
            {
                return BadRequest("Defect model");
            }

            _musicData.MediaFile.AuthorId = Functions.identityToUser(User.Identity, _context).UserId;
            _musicData.MediaFile.MediaType = Models.EnumModels.MediaType.music;

            _context.MusicFiles.Add(_musicData);
            _context.SaveChanges();

            return Ok();
        }

        //Добавлять после всех фильтрующих процессов
        private void AddExtraData(ref IQueryable<MusicFile> _initialQuery)
        {
            _initialQuery = _initialQuery.Include(music => music.MediaFile)
                                                .ThenInclude(media => media.Author);
        }
        //Вызывать перед отправкой
        private void CleanUsers(List<MusicFile> _result)
        {
            foreach (var music in _result)
            {
                music.MediaFile.Author = Functions.getCleanUser(music.MediaFile.Author);
            }
        }

        //api/Musics/Popular
        [Route("Popular")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<MusicFile>> GetPopularMusics()
        {
            int LIMIT = 6;

            var music = _context.MusicFiles.Take(LIMIT);

            AddExtraData(ref music);

            if (!music.Any())
            {
                return NotFound();
            }

            var result = music.ToList();
            CleanUsers(result);

            return result;
        }

        //api/Musics/FindByName?limited=false&_nameCriteria=blablah
        [Route("FindByName")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<MusicFile>> GetPopularByName(bool limited, string _nameCriteria)
        {
            int LIMIT = 3;

            IQueryable<MusicFile> music = _context.MusicFiles;

            if (!string.IsNullOrEmpty(_nameCriteria))
            {
                var nameCriteriaCaps = _nameCriteria.ToUpper();
                music = music.Where(music => music.MediaFile.MediaName.ToUpper().Contains(nameCriteriaCaps));
            }

            if (limited) music = music.Take(LIMIT);
            AddExtraData(ref music);

            if (!music.Any())
            {
                return NotFound();
            }

            var result = music.ToList();
            CleanUsers(result);

            return result;
        }

        //api/Musics/GroupedByInstrument
        [Route("GroupedByInstrument")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<KeyValuePair<Instrument, List<MusicFile>>>> GetPopularGroupedByInstrument()
        {
            int LIMIT = 3;

            //https://stackoverflow.com/a/2730206/15358244 - возможное решение, а пока используем дерьмовое

            var musicGroups = new List<KeyValuePair<Instrument, IQueryable<MusicFile>>>();
            foreach (Instrument instrument in Enum.GetValues(typeof(Instrument)))
            {
                //Найти музыку с этим инструментом
                var relatedMusics = _context.MusicFiles.Where(music => music.MediaFile.MediaInstruments.Any(inst => inst.Instrument == instrument));

                //Пустые категории не включаем в список
                if (relatedMusics.Any())
                {
                    AddExtraData(ref relatedMusics);
                    musicGroups.Add(KeyValuePair.Create(instrument, relatedMusics));
                }
            }

            if (!musicGroups.Any())
            {
                return NotFound();
            }

            var result = new List<KeyValuePair<Instrument, List<MusicFile>>>();
            foreach (var musicGroup in musicGroups)
            {
                result.Add(KeyValuePair.Create(musicGroup.Key, musicGroup.Value.ToList()));
                CleanUsers(result.Last().Value);
            }

            return result;
        }

        //api/Musics/GroupedByGenre
        [Route("GroupedByGenre")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<KeyValuePair<Genre, List<MusicFile>>>> GetPopularGroupedByGenre()
        {
            int LIMIT = 3;

            //https://stackoverflow.com/a/2730206/15358244 - возможное решение, а пока используем дерьмовое

            var musicGroups = new List<KeyValuePair<Genre, IQueryable<MusicFile>>>();
            foreach (Genre genre in Enum.GetValues(typeof(Genre)))
            {
                //Найти музыку с этим жанром
                var relatedMusics = _context.MusicFiles.Where(music => music.MediaFile.MediaGenres.Any(inst => inst.Genre == genre));

                //Пустые категории не включаем в список
                if (relatedMusics.Any())
                {
                    AddExtraData(ref relatedMusics);
                    musicGroups.Add(KeyValuePair.Create(genre, relatedMusics));
                }
            }

            if (!musicGroups.Any())
            {
                return NotFound();
            }

            var result = new List<KeyValuePair<Genre, List<MusicFile>>>();
            foreach (var musicGroup in musicGroups)
            {
                result.Add(KeyValuePair.Create(musicGroup.Key, musicGroup.Value.ToList()));
                CleanUsers(result.Last().Value);
            }

            return result;
        }

        //api/Musics/FindByInstrument/2?limited=false
        [Route("FindByInstrument/{_instrumentCriteria}")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<MusicFile>> GetMusicsByInstrument(bool limited, Instrument _instrumentCriteria)
        {
            int LIMIT = 6;

            var music = _context.MusicFiles.Where(music => music.MediaFile.MediaInstruments.Any(inst => inst.Instrument == _instrumentCriteria));
            if (limited) music = music.Take(LIMIT);
            AddExtraData(ref music);

            if (!music.Any())
            {
                return NotFound();
            }

            var result = music.ToList();
            CleanUsers(result);

            return result;
        }

        //api/Musics/FindByGenre/2?limited=false
        [Route("FindByGenre/{_genreCriteria}")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<MusicFile>> GetMusicsByGenre(bool limited, Genre _genreCriteria)
        {
            int LIMIT = 6;

            var music = _context.MusicFiles.Where(music => music.MediaFile.MediaGenres.Any(genre => genre.Genre == _genreCriteria));
            if (limited) music = music.Take(LIMIT); 
            AddExtraData(ref music);

            if (!music.Any())
            {
                return NotFound();
            }

            var result = music.ToList();
            CleanUsers(result);

            return result;
        }

        //api/Musics/MyUploads
        [Route("MyUploads")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<MusicFile>> GetMyUploads()
        {
            var mySelf = Functions.identityToUser(User.Identity, _context);
            var music = _context.MusicFiles.Where(music => music.MediaFile.AuthorId == mySelf.UserId).Include(music => music.MediaFile); //no extra data this time

            if (!music.Any())
            {
                return NotFound();
            }

            var result = music.ToList();

            return result;
        }

        ////WIP
        ////api/Musics/Favorite 
        //[Route("Favorites")]
        //[Authorize]
        //[HttpGet]
        //public ActionResult<IEnumerable<MusicFile>> GetFavorites()
        //{
        //    var mySelf = Functions.identityToUser(User.Identity, _context);
        //    var music = _context.MusicFiles;

        //    if (!music.Any())
        //    {
        //        return NotFound();
        //    }

        //    var result = music.ToList();

        //    return result;
        //}
    }
}
