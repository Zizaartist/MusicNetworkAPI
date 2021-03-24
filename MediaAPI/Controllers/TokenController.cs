using MediaAPI.Controllers.FrequentlyUsed;
using MediaAPI.Models;
using MediaAPI.StaticValues;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text;

namespace MediaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : Controller
    {
        private readonly MediaDBContext _context;

        public TokenController(MediaDBContext _context)
        {
            this._context = _context;
        }

        //api/Token?login=123&password=dwwd
        [HttpPost]
        public ActionResult<string> GetToken(string login, string password)
        {
            var identity = GetIdentity(login, password);
            if (identity.Item1 == null)
            {
                return NotFound();
            }

            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Item1.Claims,
                    expires: now.Add(TimeSpan.FromDays(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)); //Не ебу что это
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                userId = identity.Item2.UserId
            };

            return Json(response);
        }

        private (ClaimsIdentity, User) GetIdentity(string login, string password)
        {
            var passHash = Functions.GetHashFromString(password);
            User user = _context.Users.FirstOrDefault(e => e.UserName == login && e.Password == passHash);

            //if user wasn't found or his role is user = ignore
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserId.ToString()),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, "User")
                };
                ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
                return (claimsIdentity, user);
            }

            // если пользователя не найдено
            return (null, null);
        }
    }
}
