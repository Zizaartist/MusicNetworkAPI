using MediaAPI.Controllers.FrequentlyUsed;
using MediaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaAPI.Controllers
{
    /// <summary>
    /// Контроллер пользователей с методами, требующими большей степени осторожности
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly MediaDBContext _context;
        private readonly IMemoryCache _cache;

        public ProfileController(MediaDBContext _context, IMemoryCache _cache)
        {
            this._context = _context;
            this._cache = _cache;
        }

        [Route("Register")]
        [HttpPost]
        public ActionResult Register(User _userData, string _code = null)
        {
            if (!Models.User.ValidateModel(_userData))
            {
                return BadRequest("Defect model");
            }

            _userData.Phone = Functions.convertNormalPhoneNumber(_userData.Phone);

            //string localCode;
            //try
            //{
            //    localCode = _cache.Get<string>(_userData.Phone);
            //}
            //catch (Exception)
            //{
            //    return BadRequest("Cache error");
            //}

            //if (localCode == null)
            //{
            //    return BadRequest("Old code");
            //}
            //else
            //{
            //    if (localCode != _code)
            //    {
            //        return BadRequest("Invalid code");
            //    }
            //}

            if (_context.Users.Any(e => e.UserName == _userData.UserName))
            {
                return BadRequest("Username is taken");
            }
            else if (_context.Users.Any(e => e.Phone == _userData.Phone))
            {
                return BadRequest("Phone is taken");
            }
            else
            {
                _context.Users.Add(_userData);
                _context.SaveChanges();
            }
            return Ok();
        }
    }
}
