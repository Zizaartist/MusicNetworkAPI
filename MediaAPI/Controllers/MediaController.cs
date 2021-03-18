using MediaAPI.Controllers.FrequentlyUsed;
using MediaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class MediaController : ControllerBase
    {
        private readonly MediaDBContext _context;

        public MediaController(MediaDBContext _context)
        {
            this._context = _context;
        }

        //public ActionResult<IEnumerable<MediaFile>> 
    }
}
