using MediaAPI.Controllers.FrequentlyUsed;
using MediaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
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

        //api/Users/find/dawdaw
        [Route("find/{_nameCriteria}")]
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsersByName(string _nameCriteria) 
        {
            var nameCriteriaCaps = _nameCriteria.ToUpper();
            var users = _context.Users.Where(user => user.UserName.ToUpper().Contains(nameCriteriaCaps));

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
    }
}
