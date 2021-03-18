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
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly MediaDBContext _context;
        private readonly Functions funcs;
        private readonly IMemoryCache _cache;

        public UsersController(MediaDBContext _context, IMemoryCache _cache)
        {
            this._context = _context;
            this._cache = _cache;
            funcs = new Functions();
        }

        [Route("Register")]
        [HttpPost]
        public ActionResult Register(User _userData, string _code) 
        {
            if (!Models.User.ValidateModel(_userData))
            {
                return BadRequest("");
            }

            _userData.Phone = funcs.convertNormalPhoneNumber(_userData.Phone);

            string localCode;
            try
            {
                localCode = _cache.Get<string>(_userData.Phone);
            }
            catch (Exception)
            {
                return BadRequest("Ошибка при извлечении из кэша.");
            }

            if (localCode == null)
            {
                return BadRequest("Устаревший или отсутствующий код.");
            }
            else
            {
                if (localCode != _code)
                {
                    return BadRequest("Ошибка. Получен неверный код. Подтвердите номер еще раз.");
                }
            }

            if (_context.Users.Any(e => e.))
            {
                return BadRequest("Такой номер уже зарегистрирован");
            }
            else
            {
                userCl.CreatedDate = DateTime.UtcNow;
                userCl.UserRole = Models.EnumModels.UserRole.User;
                userCl.Points = 0;

                _context.Users.Add(userCl);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return Forbid();
                }
                return userCl; //без очистки, чтобы заполнить поля в приложении
            }
        }
    }
}
