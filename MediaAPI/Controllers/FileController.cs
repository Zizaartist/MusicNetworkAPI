using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MediaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _appEnvironment;

        public FileController(IWebHostEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
        }

        [Route("{folder}/{filename}")]
        public ActionResult GetFile(string folder, string filename)
        {
            var filePath = Path.Combine(_appEnvironment.WebRootPath, "Media", folder, filename);

            if (!System.IO.File.Exists(filePath)) 
            {
                return NotFound();
            }

            FileStream fs = new FileStream(filePath, FileMode.Open);
            string fileType = "video/mp4";

            return File(fs, fileType, filePath);
        }

        //public ActionResult UploadFile()
        //{
            
        //}
    }
}
