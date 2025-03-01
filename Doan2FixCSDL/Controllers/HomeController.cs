using Doan2FixCSDL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doan2FixCSDL.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.TopRecipes = data.Recipes
                .Where(r => r.IsApproved == true) // Updated line to ensure it's a non-nullable bool
                .OrderByDescending(r => r.Likes.Count)
                .Take(3)
                .ToList();

            if (Session["Taikhoan"] != null)
            {
                var user = (User)Session["Taikhoan"];
                ViewBag.Notifications = data.Notifications
                    .Where(n => n.UserID == user.UserID && n.IsRead == false) // Ensure proper comparison for nullable bool
                    .OrderByDescending(n => n.CreatedDate)
                    .Take(10)
                    .ToList();
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
