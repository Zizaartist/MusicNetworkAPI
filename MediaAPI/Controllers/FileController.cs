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
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using MediaAPI.Models.EnumModels;

namespace MediaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : Controller
    {
        private static readonly FormOptions _defaultFormOptions = new FormOptions();
        private readonly long _fileSizeLimit = 100000000000;
        //Костыли
        private string fileName;
        private string extension;

        private readonly IWebHostEnvironment _appEnvironment;

        public FileController(IWebHostEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
            //_defaultFormOptions.
        }

        [Authorize]
        [Route("{folder}/{filename}")]
        public ActionResult GetFile(string folder, string filename)
        {
            var filePath = Path.Combine(_appEnvironment.WebRootPath, "Media", filename);

            if (!System.IO.File.Exists(filePath)) 
            {
                return NotFound();
            }

            FileStream fs = new FileStream(filePath, FileMode.Open);
            string fileType = "video/mp4";

            return File(fs, fileType, filePath);
        }

        //Сам не знаю как работает, будем разжевывать
        [Authorize]
        [HttpPost]
        [DisableRequestSizeLimit]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult<string>> UploadFile()
        {
            //Какая-то проверка
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                ModelState.AddModelError("File",
                    $"The request couldn't be processed (Error 1).");
                // Log error

                return BadRequest(ModelState);
            }

            //Проверяем размер и тип файла из content header-a запроса
            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType),
                _defaultFormOptions.MultipartBoundaryLengthLimit);
            //Создаем читателя и читаем 1й фрагмент
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            //Читаем пока не кончится
            while (section != null)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(
                        section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    // This check assumes that there's a file
                    // present without form data. If form data
                    // is present, this method immediately fails
                    // and returns the model error.
                    if (!MultipartRequestHelper
                        .HasFileContentDisposition(contentDisposition))
                    {
                        ModelState.AddModelError("File",
                            $"The request couldn't be processed (Error 2).");
                        // Log error

                        return BadRequest(ModelState);
                    }
                    else
                    {
                        // Don't trust the file name sent by the client. To display
                        // the file name, HTML-encode the value.
                        var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                                contentDisposition.FileName.Value);
                        var trustedFileNameForFileStorage = Path.GetRandomFileName(); //Удобно, уже предусмотрели :)
                        fileName = trustedFileNameForFileStorage;
                        extension =  Path.GetExtension(contentDisposition.FileName.Value).ToLowerInvariant();

                        // **WARNING!**
                        // In the following example, the file is saved without
                        // scanning the file's contents. In most production
                        // scenarios, an anti-virus/anti-malware scanner API
                        // is used on the file before making the file available
                        // for download or for use by other systems. 
                        // For more information, see the topic that accompanies 
                        // this sample.

                        var streamedFileContent = await FileHelpers.ProcessStreamedFile(
                            section, contentDisposition, ModelState, _fileSizeLimit);

                        ModelState.Values.SelectMany(e => e.Errors).ToList().ForEach(e => Debug.WriteLine(e.ErrorMessage));
                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }

                        using (var targetStream = System.IO.File.Create(
                            Path.Combine(_appEnvironment.WebRootPath, "Media", trustedFileNameForFileStorage)))
                        {
                            await targetStream.WriteAsync(streamedFileContent);
                        }
                    }
                }

                // Drain any remaining section body that hasn't been consumed and
                // read the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            return Json(new { FileName = fileName, Extension = MediaExtensionDictionaries.StringToMediaExtension[extension] });
        }
    }

    public static class MultipartRequestHelper
    {
        // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
        // The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException(
                    $"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary;
        }

        public static bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                   && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="key";
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && string.IsNullOrEmpty(contentDisposition.FileName.Value)
                && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }

        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                    || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
        }
    }
    public static class FileHelpers
    {
        // For more file signatures, see the File Signatures Database (https://www.filesignatures.net/)
        // and the official specifications for the file types you wish to add.

        // **WARNING!**
        // In the following file processing methods, the file's content isn't scanned.
        // In most production scenarios, an anti-virus/anti-malware scanner API is
        // used on the file before making the file available to users or other
        // systems. For more information, see the topic that accompanies this sample
        // app.

        //public static async Task<byte[]> ProcessFormFile<T>(IFormFile formFile,
        //    ModelStateDictionary modelState, string[] permittedExtensions,
        //    long sizeLimit)
        //{
        //    var fieldDisplayName = string.Empty;

        //    // Use reflection to obtain the display name for the model
        //    // property associated with this IFormFile. If a display
        //    // name isn't found, error messages simply won't show
        //    // a display name.
        //    MemberInfo property =
        //        typeof(T).GetProperty(
        //            formFile.Name.Substring(formFile.Name.IndexOf(".",
        //            StringComparison.Ordinal) + 1));

        //    if (property != null)
        //    {
        //        if (property.GetCustomAttribute(typeof(DisplayAttribute)) is
        //            DisplayAttribute displayAttribute)
        //        {
        //            fieldDisplayName = $"{displayAttribute.Name} ";
        //        }
        //    }

        //    // Don't trust the file name sent by the client. To display
        //    // the file name, HTML-encode the value.
        //    var trustedFileNameForDisplay = WebUtility.HtmlEncode(
        //        formFile.FileName);

        //    // Check the file length. This check doesn't catch files that only have 
        //    // a BOM as their content.
        //    if (formFile.Length == 0)
        //    {
        //        modelState.AddModelError(formFile.Name,
        //            $"{fieldDisplayName}({trustedFileNameForDisplay}) is empty.");

        //        return new byte[0];
        //    }

        //    if (formFile.Length > sizeLimit)
        //    {
        //        var megabyteSizeLimit = sizeLimit / 1048576;
        //        modelState.AddModelError(formFile.Name,
        //            $"{fieldDisplayName}({trustedFileNameForDisplay}) exceeds " +
        //            $"{megabyteSizeLimit:N1} MB.");

        //        return new byte[0];
        //    }

        //    try
        //    {
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            await formFile.CopyToAsync(memoryStream);

        //            // Check the content length in case the file's only
        //            // content was a BOM and the content is actually
        //            // empty after removing the BOM.
        //            if (memoryStream.Length == 0)
        //            {
        //                modelState.AddModelError(formFile.Name,
        //                    $"{fieldDisplayName}({trustedFileNameForDisplay}) is empty.");
        //            }

        //            if (!IsValidFileExtensionAndSignature(
        //                formFile.FileName, memoryStream, permittedExtensions))
        //            {
        //                modelState.AddModelError(formFile.Name,
        //                    $"{fieldDisplayName}({trustedFileNameForDisplay}) file " +
        //                    "type isn't permitted or the file's signature " +
        //                    "doesn't match the file's extension.");
        //            }
        //            else
        //            {
        //                return memoryStream.ToArray();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        modelState.AddModelError(formFile.Name,
        //            $"{fieldDisplayName}({trustedFileNameForDisplay}) upload failed. " +
        //            $"Please contact the Help Desk for support. Error: {ex.HResult}");
        //        // Log the exception
        //    }

        //    return new byte[0];
        //}

        public static async Task<byte[]> ProcessStreamedFile(
            MultipartSection section, ContentDispositionHeaderValue contentDisposition,
            ModelStateDictionary modelState, long sizeLimit)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await section.Body.CopyToAsync(memoryStream);

                    // Check if the file is empty or exceeds the size limit.
                    if (memoryStream.Length == 0)
                    {
                        modelState.AddModelError("File", "The file is empty.");
                    }
                    else if (memoryStream.Length > sizeLimit)
                    {
                        var megabyteSizeLimit = sizeLimit / 1048576;
                        modelState.AddModelError("File",
                        $"The file exceeds {megabyteSizeLimit:N1} MB.");
                    }
                    else if (!IsValidFileExtensionAndSignature(
                        contentDisposition.FileName.Value, memoryStream))
                    {
                        modelState.AddModelError("File",
                            "The file type isn't permitted or the file's " +
                            "signature doesn't match the file's extension.");
                    }
                    else
                    {
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                modelState.AddModelError("File",
                    "The upload failed. Please contact the Help Desk " +
                    $" for support. Error: {ex.HResult}");
                // Log the exception
            }

            return new byte[0];
        }

        private static bool IsValidFileExtensionAndSignature(string fileName, Stream data)
        {
            if (string.IsNullOrEmpty(fileName) || data == null || data.Length == 0)
            {
                return false;
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !MediaExtensionDictionaries.StringToMediaExtension.ContainsKey(ext))
            {
                return false;
            }

            data.Position = 0;

            using (var reader = new BinaryReader(data))
            {
                // Uncomment the following code block if you must permit
                // files whose signature isn't provided in the _fileSignature
                // dictionary. We recommend that you add file signatures
                // for files (when possible) for all file types you intend
                // to allow on the system and perform the file signature
                // check.

                if (!MediaExtensionDictionaries.ExtensionToSignature.ContainsKey(ext))
                {
                    return true; //временно раскомментим
                }


                // File signature check
                // --------------------
                // With the file signatures provided in the _fileSignature
                // dictionary, the following code tests the input content's
                // file signature

                //Сверяем сигнатуру файла со значением в словаре
                var signatures = MediaExtensionDictionaries.ExtensionToSignature[ext];
                var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                return signatures.Any(signature =>
                    headerBytes.Take(signature.Length).SequenceEqual(signature));
            }
        }
    }
}
