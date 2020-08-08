﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Img_socialmedia.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
namespace Img_socialmedia.Controllers
{

    public class HomeController : Controller
    {

        private readonly IConfiguration configuration;
        private readonly db_shutterContext shutterContext;

        public object Id { get; private set; }

        public HomeController(IConfiguration config, db_shutterContext context)
        {
            this.configuration = config;
            shutterContext = context;
        }
        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}

        public IActionResult Index()
        {
            var result = (from photo in shutterContext.Photo
                        join post in shutterContext.Post on photo.Id equals post.PhotoId
                         join user in shutterContext.User on post.UserId equals user.Id
                         select new PostViewModel
                         {
                             Id = post.Id,
                             PhotoId = photo.Id,
                             TotalLike = post.TotalLike,
                             TotalViews = post.TotalViews,
                             CreateAt = post.CreateAt,
                             Tags = post.Tags,
                             UserId = user.Id,
                             User = user,
                             Photo = photo,
                         }).Take(15);
           
            return View(result);
        }

        public ViewResult Privacy()
        {
            return View();
        }
        public ViewResult Upload()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ViewResult About()
        {
            return View();
        }

        public IEnumerable<PostViewModel> TestA()
        {
            var result = from photo in shutterContext.Photo
                         join post in shutterContext.Post on photo.Id equals post.PhotoId
                         join user in shutterContext.User on post.UserId equals user.Id
                         select new PostViewModel
                         {
                             Id = post.Id,
                             PhotoId = photo.Id,
                             TotalLike = post.TotalLike,
                             TotalViews = post.TotalViews,
                             CreateAt = post.CreateAt,
                             Tags = post.Tags,
                             UserId = user.Id,
                             UserImg = user.ProfileImg,
                             Username= user.Lastname +" " + user.Firstname,
                             Photo = photo,
                             
                         };

            return result.ToList();
        }

        [HttpGet]
        public PostViewModel GetDetailPhoto(int? id)
        {
            var result = from photo in shutterContext.Photo
                         join post in shutterContext.Post on photo.Id equals post.PhotoId
                         join user in shutterContext.User on post.UserId equals user.Id
                         where post.Id == id
                         select new PostViewModel
                         {
                             Id = post.Id,
                             PhotoId = photo.Id,
                             TotalLike = post.TotalLike,
                             TotalViews = post.TotalViews,
                             CreateAt = post.CreateAt,
                             Tags = post.Tags,
                             UserId = user.Id,
                             UserImg = user.ProfileImg,
                             Comment = (from comment in shutterContext.Comment
                                        where comment.PostId == id
                                        select new CommentViewModel
                                        {
                                            Id = comment.Id,
                                            PostId = post.Id,
                                            Contents = comment.Contents,
                                            UserId = comment.UserId,
                                            UserImg = shutterContext.User.Where(u => u.Id == comment.UserId).Select(u => u.ProfileImg).FirstOrDefault(),
                                            Username = shutterContext.User.Where(u => u.Id == comment.UserId).Select(u => (u.Lastname + " " + u.Firstname)).FirstOrDefault(),
                                            CreateAt = comment.CreateAt            
                                        }
                                       ).ToList(),

                              Username = user.Lastname + " " + user.Firstname,
                             Photo = photo
                             
                         };
            return result.First();
            //var aa = shutterContext.Post.FromSqlRaw("SELECT post.*,comment.[user_id] as commentuser, [user].firstname,[user].lastname " +
            //                                       "FROM post " +
            //                                       "INNER JOIN dbo.[user] ON post.user_id = dbo.[user].id " +
            //                                       "INNER JOIN comment on post.id = comment.post_id " +
            //                                       "INNER JOIN PHOTO on post.photo_id = photo.id ").ToList();

            //.Where(b=>b.Id == id)

        }


         [HttpGet]
        public IEnumerable<CollectionViewModel> getCollection( )
        {
            var userid = HttpContext.Session.GetInt32("userid");
            var result = (from col in shutterContext.Collection 
                          where  col.UserId == userid
                          select col).ToList();
            
            return result;
        }
        public ActionResult member(){

            member mb = new member();
            mb.collectionView = getCollection();
            //mb.postView = Index();
            return View();
        }
        
    }
}
