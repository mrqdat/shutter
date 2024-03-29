﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Img_socialmedia.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Img_socialmedia.Controllers
{
    public class PostController : Controller
    {
        private db_shutterContext shutterContext;
        private readonly IHostEnvironment hostEnvironment;
        public PostController(db_shutterContext db_ShutterContext, IHostEnvironment _hostEnvironment)
        {
            shutterContext = db_ShutterContext;
            hostEnvironment = _hostEnvironment;
        }

        [Route("post/{id}",Name = "PostDetail")]
        public IActionResult Index(int id)
        {
            var result = (from photo in shutterContext.Photo
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
                              hasban=post.hasban,
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
                              Photo = photo,
                              RelatedPost = (from pp in shutterContext.Photo
                                             join p in shutterContext.Post on pp.Id equals p.PhotoId
                                             join u in shutterContext.User on p.UserId equals u.Id
                                             select new PostViewModel
                                             {
                                                 Id = p.Id,
                                                 PhotoId = pp.Id,
                                                 TotalLike = p.TotalLike,
                                                 TotalViews = p.TotalViews,
                                                 CreateAt = p.CreateAt,
                                                 Tags = p.Tags,
                                                 UserId = u.Id,
                                                 UserImg = u.ProfileImg,
                                                 Username = u.Lastname + " " + u.Firstname,
                                                 Photo = pp,
                                             }).Take(15).ToList()
                          }).First();
            if (result == null)
            {
                return View("Error");
            }

            if (result.hasban)
            {
                return View("Error");
            }
            bool liked = false;
            if (HttpContext.Session.GetInt32("userid").HasValue)
            {
                string filename = HttpContext.Session.GetInt32("userid").ToString() + ".json";
                JSONReadWrite j = new JSONReadWrite();
                JArray jsonArray = JArray.Parse("[" + j.Read(filename, "json") + "]");
                foreach (var a in jsonArray)
                {  
                    if (Convert.ToInt32(a["id"]).Equals(id))
                    {
                        liked = true;
                    }
                }
            }
            result.TotalViews += 1;
            shutterContext.Post.Update(result);
            shutterContext.SaveChanges();
            result.Liked = liked;
            return View("Index", result);
        }

        public int Likes(int id)
        {
            var result = shutterContext.Post.Find(id);
            return 1;
        }
        public object aa(int id)
        {
            string filename = "1001.json";
            JSONReadWrite j = new JSONReadWrite();
            List<LikeViewModel> like = new List<LikeViewModel>();
         
            JArray jsonArray = JArray.Parse("[" + j.Read(filename,"json") + "]");
            var a = JObject.Parse(jsonArray[0].ToString());
            return a["id"];
        }
        public string b(int id)
        {
            string filename = "1001.json";
            string jsonStr = "{\"id\":"+id+"}";
            //string filename = userid.ToString() + ".json";
            JSONReadWrite j = new JSONReadWrite();
            j.IsFileExist(filename, jsonStr);
            return j.Read(filename, "json");
        }
        // like = 0, unlike = 1
        public IActionResult Like(int id)
        {
            if (!HttpContext.Session.GetInt32("userid").HasValue)
            {
                return Json(new { url = "/account/login" });
            }
            int userid = HttpContext.Session.GetInt32("userid").Value;
            string filename = userid + ".json";
            string jsonStr = "{\"id\":" + id + "}";
            JSONReadWrite j = new JSONReadWrite();
            int a= j.IsFileExist(filename, jsonStr);
            if (a == 0) {
                var total = shutterContext.Post.Find(id);
                total.TotalLike = total.TotalLike + 1;
                shutterContext.SaveChanges();
                return Json(new { status = true }) ;
            }
            else
            {
                var total = shutterContext.Post.Find(id);
                total.TotalLike = total.TotalLike - 1;
                shutterContext.SaveChanges();
                return Json(new { status = false });
            }
        }
        [HttpPost]
        public ActionResult report(int id)
        {
            if(HttpContext.Session.GetInt32("userid").HasValue)
            {
                var userid = HttpContext.Session.GetInt32("userid").Value;
                var rp = shutterContext.Post.Find(id);
                
                rp.triggeredBy = userid;
                shutterContext.SaveChanges();
                //string data = JsonConvert.SerializeObject(rp);
                //System.IO.File.WriteAllText(@"~/rp/report.json", data);

                return Json(new
                {
                    status = true
                });
            }
            else{
                return Json(new{
                    status = false,
                    message = "Your session has expried, please login again to continue working."
                });
            }          
        
        }

        [HttpPost]
        public IActionResult Comment(int id,string content)
        {
            var post = shutterContext.Post.Find(id);
            if (post == null)
            {
                return View("Error");
            }
            if (!HttpContext.Session.GetInt32("userid").HasValue)
            {
                return View("Error");
            }
            if (string.IsNullOrEmpty(content))
            {
                return View("Index",post);
            }

            CommentViewModel comment = new CommentViewModel
            {
                PostId=post.Id,
                UserId=HttpContext.Session.GetInt32("userid").Value,
                Contents = content,
                CreateAt=DateTime.Now,
            };

            shutterContext.Add(comment);
            shutterContext.SaveChanges(); 
            var url = Url.Action("Index", "Post", new { id = id});
            return Redirect(url);
        }
        public IActionResult Search(string tags)
        {
            {
                ViewBag.title = tags;
                if (string.IsNullOrEmpty(tags)) if (HttpContext.Session.GetInt32("userid").HasValue)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                var result = (from photo in shutterContext.Photo
                              join post in shutterContext.Post on photo.Id equals post.PhotoId
                              join user in shutterContext.User on post.UserId equals user.Id
                              where post.Tags.Contains(tags)
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
                              }).Take(15).ToList();
                if (result == null)
                {
                    var result2 = (from photo in shutterContext.Photo
                                   join post in shutterContext.Post on photo.Id equals post.PhotoId
                                   join user in shutterContext.User on post.UserId equals user.Id
                                   orderby Guid.NewGuid()
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
                                   }).Take(15).ToList();
                    return View(result2);
                }
                else if (result.Count < 15)
                {
                    {
                        var result3 = (from photo in shutterContext.Photo
                                       join post in shutterContext.Post on photo.Id equals post.PhotoId
                                       join user in shutterContext.User on post.UserId equals user.Id
                                       orderby Guid.NewGuid()
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
                                       }).Take(15 - result.Count).ToList();
                        result.AddRange(result3);
                    }
                }
                return View(result);
            }
        }

        [HttpPost]
        public ActionResult hidePost(int id)
        {

            return Json(new { status = true });
        }
    }
}
