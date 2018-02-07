using Picks.Dal.Configuration;
using Picks.Dal.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
//using System.Drawing.Primitives;
//using System.Drawing.Drawing2D;
//using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Picks.Dal.Services
{
    public class ImageService
    {

        private IServiceProvider _serviceProvider;
        private IHostingEnvironment _environment;
        private ImageUploadConfiguration _config;

        public ImageService(IOptions<ImageUploadConfiguration> config, IServiceProvider serviceProvider, IHostingEnvironment environment)
        {
            _serviceProvider = serviceProvider;
            _environment = environment;
            _config = config.Value;
        }

        private void OptimizeAndSaveImage(Stream imgStream, string fileName)
        {
            try
            {
                using (var image = new Bitmap(System.Drawing.Image.FromStream(imgStream)))
                {
                    imgStream.Dispose();

                    int height = image.Height,
                        width = image.Width;

                    if (image.Height > image.Width)
                    {
                        if (image.Height > _config.MaxSize)
                        {
                            height = _config.MaxSize;
                            width = Convert.ToInt32(image.Width * ((double)height / (double)image.Height));
                        }
                    }
                    else
                    {
                        if (image.Width > _config.MaxSize)
                        {
                            width = _config.MaxSize;
                            height = Convert.ToInt32(image.Height * ((double)width / (double)image.Width));
                        }
                    }

                    var resized = new Bitmap(width, height);
                    using (var graphics = Graphics.FromImage(resized))
                    {
                        graphics.CompositingQuality = CompositingQuality.HighSpeed;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.CompositingMode = CompositingMode.SourceCopy;

                        graphics.DrawImage(image, 0, 0, width, height);
                        var qualityParamId = System.Drawing.Imaging.Encoder.Quality;
                        var encoderParameters = new EncoderParameters(1);
                        encoderParameters.Param[0] = new EncoderParameter(qualityParamId, _config.Quality);

                        ImageCodecInfo codec = null;
                        switch (fileName.Split(".")[1])
                        {
                            case "png":
                                codec = ImageCodecInfo.GetImageDecoders()
                                    .FirstOrDefault(x => x.FormatID == ImageFormat.Png.Guid);
                                break;
                            case "jpg":
                            case "jpeg":
                                codec = ImageCodecInfo.GetImageDecoders()
                                    .FirstOrDefault(x => x.FormatID == ImageFormat.Jpeg.Guid);
                                break;
                        }

                        using (var fStream = File.Open($"{ _environment.WebRootPath}/uploads/{fileName}", FileMode.Create, FileAccess.Write))
                            resized.Save(fStream, codec, encoderParameters);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                
            }

        }

        public IEnumerable<Models.Image> UploadImages(params IFormFile[] images)
        {
            //var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var fileNames = new List<Models.Image>();

            //var filePath = _environment.WebRootPath + "/uploads/";

            foreach (var image in images)
            {
                var id = Guid.NewGuid();
                var contentType = image.ContentType;

                var fileType = "";
                switch (contentType)
                {
                    case "image/png":
                        fileType = "png";
                        break;
                    case "image/jpg":
                        fileType = "jpg";
                        break;
                    case "image/jpeg":
                        fileType = "jpeg";
                        break;
                    default:
                        continue; //Skip adding because file is not an image
                }

                var fileName = $"{id}.{fileType}";

                /*using (var stream = new FileStream($"{filePath}{fileName}", FileMode.Create))
                {
                    await image.CopyToAsync(stream);

                }*/
                using (var imgStream = image.OpenReadStream())
                {
                    OptimizeAndSaveImage(imgStream, fileName);
                }

                fileNames.Add(new Models.Image { FileName = fileName });
            }

            return fileNames;
        }

        public void RemoveFile(string fileName)
        {
            var filePath = _environment.WebRootPath + "/uploads/";
            filePath = $"{filePath}/{fileName}";
            File.Delete(filePath);
        }


    }
}
