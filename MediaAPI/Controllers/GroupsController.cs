using MediaAPI.Controllers.FrequentlyUsed;
using MediaAPI.Models;
using MediaAPI.Models.EnumModels;
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

        //api/Groups
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<KeyValuePair<Genre, IEnumerable<Group>>>> GetPopularGroups()
        {
            int POPULAR_GROUPS_COUNT = 6;

            var allGenres = Enum.GetValues(typeof(Genre));

            var result = new List<KeyValuePair<Genre, IEnumerable<Group>>>();

            foreach (Genre genre in allGenres) 
            {
                var groupsByGenre = _context.GroupGenres.Where(groupGenre => groupGenre.Genre == genre)
                        .Take(POPULAR_GROUPS_COUNT)
                        .Select(groupGenre => groupGenre.Group);

                if (groupsByGenre.Any()) 
                {
                    result.Add(new KeyValuePair<Genre, IEnumerable<Group>>(genre, groupsByGenre));
                } 
            }

            return result;
        }
    }
}
