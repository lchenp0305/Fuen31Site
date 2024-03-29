﻿using Fuen31Site.Models;
using Fuen31Site.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Eventing.Reader;

namespace Fuen31Site.Controllers
{
    public class ApiController : Controller
    {
        private readonly MyDBContext _dbContext;
        private readonly IWebHostEnvironment _host;
        public ApiController(MyDBContext dbContext, IWebHostEnvironment host)
        {
            _dbContext = dbContext;
            _host = host;
        }

        
        public IActionResult Index()
        {
            System.Threading.Thread.Sleep(10000);
            //return Content("Hello Content");
            //return Content("<h2>Hello Content</h2>", "text/html");
            return Content("Content, 你好","text/plain",System.Text.Encoding.UTF8);
        }

       
        public IActionResult Cities()
        {
            var cities = _dbContext.Addresses.Select(a => a.City).Distinct();
            return Json(cities);
        }
        public IActionResult Districts(string city) {
            var districts = _dbContext.Addresses.Where(a => a.City == city).Select(a => a.SiteId).Distinct();
            return Json(districts);
        }

        public IActionResult Avatar(int id=1) {
            Member? member = _dbContext.Members.Find(id);
            if (member != null)
            {
                byte[] img = member.FileData;
                return File(img, "image/jpeg");
            }

            return NotFound();
        }

        [HttpPost]
        public IActionResult Register(Member member, IFormFile Avatar)
        {
            string fileName = "empty.jpg";
            if (Avatar != null)
            {
                fileName = Avatar.FileName;
            }

            //取得檔案上傳的實際路徑
            string uploadPath = Path.Combine(_host.WebRootPath, "uploads", fileName);
            //檔案上傳
            using (var fileStream = new FileStream(uploadPath, FileMode.Create))
            {
                Avatar?.CopyTo(fileStream);
            }

            //轉成二進位
            byte[]? imgByte = null;
            using (var memoryStream = new MemoryStream())
            {
                Avatar?.CopyTo(memoryStream);
                imgByte = memoryStream.ToArray();
            }

            member.FileName = fileName;
            member.FileData = imgByte;

            //新增
            _dbContext.Members.Add(member);
            _dbContext.SaveChanges();




            return Content("新增成功");

            // return Content($"Hello {_user.Name}, {_user.Age}歲了,電子郵件是{_user.Email}");
            //return Content($"{_user.Avatar?.FileName}-{_user.Avatar?.Length}-{_user.Avatar?.ContentType}");
        }

        // public IActionResult Register(string name, int age = 26)
        //public IActionResult Register(UserDTO _user)
        //{
        //    if (string.IsNullOrEmpty(_user.Name))
        //    {
        //        _user.Name = "Guest";
        //    }
        //    //string uploadPath = @"C:\Users\ispan\Documents\workspace\Fuen31Site\wwwroot\uploads\fileName.jpg";

        //    //todo 檔案存在的處理
        //    //todo 限制上傳的檔案類型
        //    //todo 限制上傳的檔案大小

        //    string fileName = "empty.jpg";
        //    if(_user.Avatar != null)
        //    {
        //        fileName = _user.Avatar.FileName;
        //    }
        //    //取得檔案上傳的實際路徑
        //    string uploadPath = Path.Combine(_host.WebRootPath, "uploads", fileName);
        //    //檔案上傳
        //    using (var fileStream = new FileStream(uploadPath, FileMode.Create))
        //    {
        //        _user.Avatar?.CopyTo(fileStream);
        //    }

        //    //轉成二進位
        //    byte[]? imgByte = null;
        //    using(var memoryStream = new MemoryStream())
        //    {
        //        _user.Avatar?.CopyTo(memoryStream);
        //        imgByte = memoryStream.ToArray();
        //    }


        //    // return Content($"Hello {_user.Name}, {_user.Age}歲了,電子郵件是{_user.Email}");
        //  return Content($"{_user.Avatar?.FileName}-{_user.Avatar?.Length}-{_user.Avatar?.ContentType}");
        //}

        [HttpPost]
        public IActionResult Spots([FromBody]SearchDTO _search) {
            //根據分類編號讀取景點資料
            var spots = _search.categoryId == 0 ? _dbContext.SpotImagesSpots : _dbContext.SpotImagesSpots.Where(s => s.CategoryId == _search.categoryId);

            //根據關鍵字搜尋
            if (!string.IsNullOrEmpty(_search.keyword))
            {
               spots = spots.Where(s => s.SpotTitle.Contains(_search.keyword) || s.SpotDescription.Contains(_search.keyword));
            }

            //排序
            switch (_search.sortBy)
            {
                case "spotTitle":
                    spots = _search.sortType == "asc" ? spots.OrderBy(s=>s.SpotTitle) : spots.OrderByDescending
                        (s=>s.SpotTitle);                        
                    break;
                case "categoryId":
                    spots = _search.sortType == "asc" ? spots.OrderBy(s => s.CategoryId) : spots.OrderByDescending
                       (s => s.CategoryId);
                    break;
                default:
                    spots = _search.sortType == "asc" ? spots.OrderBy(s => s.SpotId) : spots.OrderByDescending
                       (s => s.SpotId);
                    break;
            }

            //分頁
            int TotalCount = spots.Count(); //搜尋出來的資料總共有幾筆
            int pageSize = _search.pageSize ?? 9; //每頁多少筆資料
            int TotalPages =(int)Math.Ceiling((decimal)TotalCount / pageSize); //計算出總共有幾頁
            int page = _search.Page ?? 1; //第幾頁

            //取出分頁資料
            spots = spots.Skip((page - 1) * pageSize).Take(pageSize);

            //設計要回傳的資料格式
            SpotsPagingDTO spotsPaging = new SpotsPagingDTO();
            spotsPaging.TotalPages = TotalPages;
            spotsPaging.SpotsReslut = spots.ToList();

            return Json(spotsPaging);
        }
    }
}
