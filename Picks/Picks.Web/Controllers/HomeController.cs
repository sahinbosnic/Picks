using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Picks.Web.Models;
using Microsoft.AspNetCore.Http;
using Picks.Dal.Models;
using Picks.Dal.DataAccess;
using Picks.Dal.Services;
using System.IO.Compression;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Picks.Web.Constants;
using System.Globalization;
using Microsoft.Extensions.Caching.Distributed;

namespace Picks.Web.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _ctx;
        private readonly ImageService _imageService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;
        private string CdnUrl = "";

        public HomeController(ApplicationDbContext ctx, ImageService imageService, IHostingEnvironment hostingEnvironment, IConfiguration configuration, IDistributedCache cache)
        {
            _ctx = ctx;
            _imageService = imageService;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _cache = cache;

            if (Convert.ToBoolean(_configuration["CDN:Active"]))
            {
                CdnUrl = _configuration["CDN:Url"];
            }
        }

        public IActionResult Index(string name)
        {
            ViewBag.CDN = CdnUrl;

            //var value = _cache.GetString("the_cache_key");

            //if (value == null)
            //{
            //    value = $"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}";
            //    _cache.SetString("the_cache_string", value);
            //}

            //ViewData["CacheTime"] = $"Cached time {value}";
            //ViewData["CurrentTime"] = $"Current time: {DateTime.Now.ToString(CultureInfo.CurrentCulture)}";

            //var theNameFromSession = HttpContext.Session.Get("name");
            //if (string.IsNullOrEmpty(theNameFromSession))
            //{
                //HttpContext.Session.SetString("name", name);
            //}

            //ViewData["SessionMessage"] = $"theNameFromSession: {theNameFromSession}";

            return View();
        }

        [HttpPost]
        public IActionResult Upload(string tags, params IFormFile[] files)
        {
            var imgList = _imageService.UploadImages(files);

            foreach (var item in imgList)
            {
                item.Tags = tags;
            }
            _ctx.Image.AddRange(imgList);
            _ctx.SaveChanges();
            return Json(imgList);
        }

        [HttpPost]
        public IActionResult GetImages(int count = 50, params string[] search)
        {
            var imageList = new List<Image>();
            if (search.Count() > 0)
            {
                foreach (var item in search)
                {
                    imageList.AddRange(_ctx.Image.Where(x => x.Tags.Contains(item)).ToList());
                }
                imageList = imageList.Distinct().OrderByDescending(x => x.Id).ToList();

            }
            else
            {
                imageList = _ctx.Image.OrderByDescending(x => x.Id).ToList();
            }
            return Json(imageList);
        }

        [HttpGet]
        public IActionResult GetTags()
        {
            var tagList = new List<string>();

            var getTags = _ctx.Image.Select(x => x.Tags).ToList();

            foreach (var item in getTags)
            {
                tagList.AddRange(item.ToLower().Split(","));
            }

            for (int i = 0; i < tagList.Count(); i++)
            {
                tagList[i] = tagList[i].Trim();
            }

            tagList = tagList.Distinct().ToList();

            return Json(tagList);
        }

        [HttpPost]
        public IActionResult getzipurl(params string[] images)
        {
            var imageList = new List<Image>();
            foreach (var item in images)
            {
                if (_ctx.Image.Any(x => x.FileName == item))
                {
                    imageList.Add(_ctx.Image.Where(x => x.FileName == item).First());
                }
            }

            Guid guidName = Guid.NewGuid();
            string returnName = guidName.ToString() + ".zip";
            string rootPath = _hostingEnvironment.WebRootPath + "/uploads/";
            string zipPath = _hostingEnvironment.WebRootPath + "/zip/";

            var zip = ZipFile.Open(zipPath + returnName, ZipArchiveMode.Create);
            foreach (var file in imageList)
            {
                zip.CreateEntryFromFile(rootPath + file.FileName, Path.GetFileName(rootPath + file.FileName), CompressionLevel.Optimal);
            }

            zip.Dispose();


            return Json(returnName);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
