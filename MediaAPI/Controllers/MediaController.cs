using MediaAPI.Controllers.FrequentlyUsed;
using MediaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly MediaDBContext _context;

        public MediaController(MediaDBContext _context)
        {
            this._context = _context;
        }

        //api/Media/FavoriteToggle/3
        [Route("FavoriteToggle/{_mediaFileId}")]
        [Authorize]
        [HttpPut]
        public ActionResult<bool> ToggleFavorite(int _mediaFileId) 
        {
            var mediaFile = _context.MediaFiles.Find(_mediaFileId);
            if (mediaFile == null) 
            {
                return NotFound();
            }

            var mySelf = Functions.identityToUser(User.Identity, _context);
            var existingFavorite = _context.Favourites.FirstOrDefault(fav => fav.UserId == mySelf.UserId && fav.MediaId == mediaFile.MediaId);

            //Существует - удалить
            if (existingFavorite != null)
            {
                _context.Favourites.Remove(existingFavorite);
                _context.SaveChanges();
                return false;
            }
            //Не существует - создать
            else 
            {
                _context.Favourites.Add(new Favourite() 
                {
                    MediaId = mediaFile.MediaId,
                    UserId = mySelf.UserId
                });
                _context.SaveChanges();
                return true;
            }
        }

        //api/Media/FavoriteGet/2
        [Route("FavoriteGet/{_mediaFileId}")]
        [Authorize]
        [HttpGet]
        public ActionResult<bool> GetFavorite(int _mediaFileId)
        {
            var mediaFile = _context.MediaFiles.Find(_mediaFileId);
            if (mediaFile == null)
            {
                return NotFound();
            }

            var mySelf = Functions.identityToUser(User.Identity, _context);
            var existingFavorite = _context.Favourites.FirstOrDefault(fav => fav.UserId == mySelf.UserId && fav.MediaId == mediaFile.MediaId);

            //Существует
            if (existingFavorite != null)
            {
                return false;
            }
            //Не существует
            else
            {
                return true;
            }
        }
    }
}
