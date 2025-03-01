using Doan2FixCSDL.Models;
using System.Linq;
using System.Web.Mvc;

namespace Doan2FixCSDL.Controllers
{
    public class BaseController : Controller
    {
        protected DataClasses1DataContext data = new DataClasses1DataContext("ADMINISTRATOR");

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (Session["Taikhoan"] != null)
            {
                var currentUser = (User)Session["Taikhoan"];
                var followedUsers = (from follow in data.UserFollows
                                     join user in data.Users on follow.FollowedID equals user.UserID
                                     where follow.FollowerID == currentUser.UserID
                                     select user).ToList();

                ViewBag.FollowedUsers = followedUsers;
            }
        }
    }
}
