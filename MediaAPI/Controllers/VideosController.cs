using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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

        [HttpPost]
        public ActionResult UploadVideo() 
        {
            return Ok();
        }
    }
}
