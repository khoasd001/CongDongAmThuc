using Doan2FixCSDL.Models;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doan2FixCSDL.Controllers
{
    public class DashboardController : BaseController
    {
        // Kiểm tra xem người dùng đã đăng nhập chưa
        private bool IsUserLoggedIn()
        {
            return Session["Taikhoan"] != null;
        }

        public ActionResult Index()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Dangnhap", "User");
            }

            var userId = ((User)Session["Taikhoan"]).UserID;
            var userRecipes = data.Recipes.Where(r => r.CreatedBy == userId).ToList();
            return View(userRecipes);
        }


        // Phương thức GET: EditProfile
        [HttpGet]
        public ActionResult EditProfile()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Dangnhap", "User");
            }

            var userId = ((User)Session["Taikhoan"]).UserID;
            var user = data.Users.SingleOrDefault(u => u.UserID == userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra và gán avatar mặc định nếu chưa có
            if (string.IsNullOrEmpty(user.Avatar))
            {
                user.Avatar = "default-avatar.png"; // Đặt tên tệp avatar mặc định
            }

            return View(user);
        }

        // Phương thức POST: EditProfile
        [HttpPost]
        public ActionResult EditProfile(User user, HttpPostedFileBase avatar)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Dangnhap", "User");
            }

            var currentUser = data.Users.SingleOrDefault(u => u.UserID == user.UserID);
            if (currentUser != null)
            {
                currentUser.HoTen = user.HoTen;
                currentUser.Ngaysinh = user.Ngaysinh;

                if (avatar != null && avatar.ContentLength > 0)
                {
                    string avatarPath = Path.Combine(Server.MapPath("~/Uploads/"), avatar.FileName);
                    avatar.SaveAs(avatarPath);
                    currentUser.Avatar = avatar.FileName;
                }

                data.SubmitChanges();

                // Cập nhật lại thông tin trong Session
                Session["Taikhoan"] = currentUser;

                // Sử dụng TempData để truyền thông báo thành công
                TempData["SuccessMessage"] = "Lưu thông tin thành công!";
            }
            return RedirectToAction("EditProfile");
        }

        // Phương thức GET: ChangePassword
        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Dangnhap", "User");
            }
            return View();
        }

        // Phương thức POST: ChangePassword
        [HttpPost]
        public ActionResult ChangePassword(string currentPassword, string newPassword)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Dangnhap", "User");
            }

            var userId = ((User)Session["Taikhoan"]).UserID;
            var currentUser = data.Users.SingleOrDefault(u => u.UserID == userId);
            if (currentUser != null)
            {
                if (!string.IsNullOrEmpty(currentPassword) && !string.IsNullOrEmpty(newPassword))
                {
                    if (currentUser.PasswordHash == currentPassword) // Kiểm tra mật khẩu hiện tại
                    {
                        currentUser.PasswordHash = newPassword; // Đổi mật khẩu
                        data.SubmitChanges();
                        // Sử dụng TempData để truyền thông báo thành công
                        TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                    }
                    else
                    {
                        ViewBag.PasswordError = "Mật khẩu hiện tại không đúng.";
                        return View();
                    }
                }
            }
            return RedirectToAction("EditProfile");
        }

        // Phương thức GET: Delete
        public ActionResult Delete(int id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Dangnhap", "User");
            }

            var recipe = data.Recipes.SingleOrDefault(r => r.RecipeID == id && r.CreatedBy == ((User)Session["Taikhoan"]).UserID);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            return View(recipe);
        }

        // Phương thức POST: DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Dangnhap", "User");
            }

            // Mở kết nối nếu nó đã bị đóng
            if (data.Connection.State == System.Data.ConnectionState.Closed)
            {
                data.Connection.Open();
            }

            using (var transaction = data.Connection.BeginTransaction())
            {
                try
                {
                    // Gán transaction cho context
                    data.Transaction = transaction;

                    // Xóa tất cả các bản ghi liên quan trong bảng Likes
                    var relatedLikes = data.Likes.Where(l => l.RecipeID == id).ToList();
                    data.Likes.DeleteAllOnSubmit(relatedLikes);

                    // Xóa tất cả các bản ghi bình luận liên quan
                    var relatedComments = data.Comments.Where(c => c.RecipeID == id).ToList();
                    data.Comments.DeleteAllOnSubmit(relatedComments);

                    // Xóa tất cả các bản ghi chi tiết công thức liên quan
                    var recipeDetails = data.RecipeDetails.Where(rd => rd.RecipeID == id).ToList();
                    data.RecipeDetails.DeleteAllOnSubmit(recipeDetails);

                    // Xóa bản ghi công thức
                    var recipe = data.Recipes.SingleOrDefault(r => r.RecipeID == id && r.CreatedBy == ((User)Session["Taikhoan"]).UserID);
                    if (recipe != null)
                    {
                        data.Recipes.DeleteOnSubmit(recipe);
                    }

                    data.SubmitChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    // Đóng kết nối khi xong việc
                    if (data.Connection.State == System.Data.ConnectionState.Open)
                    {
                        data.Connection.Close();
                    }
                }
            }

            return RedirectToAction("Index");
        }
    }
}
