using MediaAPI.Controllers.FrequentlyUsed;
using MediaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly MediaDBContext _context;

        public UsersController(MediaDBContext _context)
        {
            this._context = _context;
        }

        //api/Users/profile/2
        [Route("profile/{_userId}")]
        [Authorize]
        [HttpGet]
        public ActionResult<User> GetProfileInfo(int _userId) 
        {
            var user = _context.Users.Find(_userId);

            if (user == null) 
            {
                return NotFound();
            }

            var result = Functions.getCleanUser(user);

            return result;
        }

        //api/Users/edit
        [Route("edit")]
        [HttpPut]
        public ActionResult EditProfileInfo(User _userData) 
        {
            if (string.IsNullOrEmpty(_userData.UserName)) 
            {
                return BadRequest();
            }

            var mySelf = _context.Users.First(user => user.UserId == int.Parse(User.Identity.Name)); //не используем functions т.к. нам нужен tracking
            mySelf.UserName = _userData.UserName;
            mySelf.AvatarPath = _userData.AvatarPath;
            mySelf.Status = _userData.Status;
            mySelf.DateOfBirth = _userData.DateOfBirth;

            _context.SaveChanges();

            return Ok();
        }

        //api/Users/find/dawdaw?limited=true
        [Route("find")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsersByName(bool limited, string _nameCriteria) 
        {
            var LIMIT = 3;

            IQueryable<User> users = _context.Users;

            if (!string.IsNullOrEmpty(_nameCriteria))
            {
                var nameCriteriaCaps = _nameCriteria.ToUpper();
                users = users.Where(user => user.UserName.ToUpper().Contains(nameCriteriaCaps));
            }
            if (limited) users = users.Take(LIMIT);

            if (!users.Any()) 
            {
                return NotFound();
            }

            var result = new List<User>();
            foreach (var user in users.ToList()) 
            {
                result.Add(Functions.getCleanUser(user));
            }

            return result;
        }

        //api/Users/wall/3
        [Route("wall/{_userId}")]
        [Authorize]
        [HttpGet]
        public ActionResult<string> GetUploadedShit(int _userId) 
        {
            var user = _context.Users.AsNoTracking().First(user => user.UserId == _userId);

            if (user == null) 
            {
                return NotFound();
            }

            var videoFiles = _context.VideoFiles.Where(vid => vid.MediaFile.AuthorId == user.UserId)
                                                .Include(vid => vid.MediaFile)
                                                    .ThenInclude(media => media.MediaInstruments)
                                                .Include(vid => vid.MediaFile)
                                                    .ThenInclude(media => media.MediaGenres);

            var musicFiles = _context.MusicFiles.Where(music => music.MediaFile.AuthorId == user.UserId)
                                                .Include(music => music.MediaFile);

            var groups = _context.Groups.Where(group => group.OwnerId == user.UserId);

            if (!videoFiles.Any() && !musicFiles.Any() && !groups.Any()) 
            {
                return NotFound();
            }

            var result = new
            {
                resultVids = videoFiles.ToList(),
                resultMusic = musicFiles.ToList(),
                resultGroups = groups.ToList()
            };

            return Json(result);
        }
    }
}
