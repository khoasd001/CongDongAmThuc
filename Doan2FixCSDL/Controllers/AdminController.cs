using Doan2FixCSDL.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Data.Linq;
using PagedList;
namespace Doan2FixCSDL.Controllers
{
    public class AdminController : BaseController
    {

        public ActionResult AdminDashboard()
        {
            if (Session["AdminAccount"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }

            ViewBag.UserCount = data.Users.Count();
            ViewBag.RecipeCount = data.Recipes.Count();
            ViewBag.CommentCount = data.Comments.Count();
            ViewBag.Layout = "~/Views/Shared/AdminLayout.cshtml";
            var recipes = data.Recipes.ToList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int recipeId)
        {
            if (Session["AdminAccount"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }

            var recipe = data.Recipes.SingleOrDefault(r => r.RecipeID == recipeId);
            if (recipe != null)
            {
                recipe.IsApproved = true;
                data.SubmitChanges();

                // Create a notification for the user
                var notification = new Notification
                {
                    UserID = recipe.CreatedBy,
                    Message = $"Công thức '{recipe.Title}' của bạn đã được xét duyệt.",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                };
                data.Notifications.InsertOnSubmit(notification);
                data.SubmitChanges();
            }

            return RedirectToAction("ManageRecipes", "Admin");
        }


        public ActionResult Statistics()
        {
            if (Session["AdminAccount"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }
            // Lấy số liệu người dùng
            int userCount = data.Users.Count();
            int recipeCount = data.Recipes.Count();
            int commentCount = data.Comments.Count();
            int totalAccessCount = data.Users.Sum(u => u.AccessCount ?? 0); // Tổng số lần đăng nhập của tất cả người dùng

            // Truyền dữ liệu vào ViewBag
            ViewBag.UserCount = userCount;
            ViewBag.RecipeCount = recipeCount;
            ViewBag.CommentCount = commentCount;
            ViewBag.TotalAccessCount = totalAccessCount; // Truyền tổng số lần đăng nhập

            return View();
        }

        public ActionResult ManageUsers(int? page)
        {
            if (Session["AdminAccount"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var users = data.Users.ToList().ToPagedList(pageNumber, pageSize);
            return View(users);
        }


        public ActionResult ManageRecipes(int? page)
        {
            if (Session["AdminAccount"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var recipes = data.Recipes.ToList().ToPagedList(pageNumber, pageSize);
            return View(recipes);
        }
          public ActionResult ManageComments(int? page)
        {
            if (Session["AdminAccount"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var comments = data.Comments.OrderByDescending(c => c.CommentDate).ToPagedList(pageNumber, pageSize);
            return View(comments);
        }

        public ActionResult AdminLogout()
        {
            Session["AdminAccount"] = null;
            return RedirectToAction("Dangnhap", "User");
        }

        [HttpPost]
        public ActionResult DeleteUser(int userId)
        {
            if (Session["AdminAccount"] == null)
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
                    var relatedLikes = data.Likes.Where(l => l.UserID == userId).ToList();
                    data.Likes.DeleteAllOnSubmit(relatedLikes);

                    // Xóa tất cả các bản ghi bình luận liên quan
                    var relatedComments = data.Comments.Where(c => c.UserID == userId).ToList();
                    data.Comments.DeleteAllOnSubmit(relatedComments);

                    // Lấy tất cả các bản ghi công thức của người dùng
                    var userRecipes = data.Recipes.Where(r => r.CreatedBy == userId).ToList();

                    foreach (var recipe in userRecipes)
                    {
                        // Xóa tất cả các bản ghi chi tiết công thức liên quan
                        var recipeDetails = data.RecipeDetails.Where(rd => rd.RecipeID == recipe.RecipeID).ToList();
                        data.RecipeDetails.DeleteAllOnSubmit(recipeDetails);

                        // Xóa tất cả các bản ghi liên quan trong bảng Likes
                        var recipeLikes = data.Likes.Where(l => l.RecipeID == recipe.RecipeID).ToList();
                        data.Likes.DeleteAllOnSubmit(recipeLikes);

                        // Xóa tất cả các bản ghi bình luận liên quan
                        var recipeComments = data.Comments.Where(c => c.RecipeID == recipe.RecipeID).ToList();
                        data.Comments.DeleteAllOnSubmit(recipeComments);

                        // Xóa bản ghi công thức
                        data.Recipes.DeleteOnSubmit(recipe);
                    }

                    // Xóa bản ghi người dùng
                    var user = data.Users.SingleOrDefault(u => u.UserID == userId);
                    if (user != null)
                    {
                        data.Users.DeleteOnSubmit(user);
                    }

                    data.SubmitChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
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

            return RedirectToAction("ManageUsers");
        }


        [HttpPost]
        public ActionResult DeleteRecipe(int recipeId)
        {
            if (Session["AdminAccount"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }

            try
            {
                var recipe = data.Recipes.SingleOrDefault(r => r.RecipeID == recipeId);
                if (recipe != null)
                {
        
                    var likes = data.Likes.Where(l => l.RecipeID == recipeId).ToList();
                    data.Likes.DeleteAllOnSubmit(likes);

        
                    var comments = data.Comments.Where(c => c.RecipeID == recipeId).ToList();
                    data.Comments.DeleteAllOnSubmit(comments);

         
                    var recipeDetails = data.RecipeDetails.Where(rd => rd.RecipeID == recipeId).ToList();
                    data.RecipeDetails.DeleteAllOnSubmit(recipeDetails);

                    data.Recipes.DeleteOnSubmit(recipe);
                    data.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                
            }

            return RedirectToAction("ManageRecipes");
        }

        public ActionResult ViewUser(int userId)
        {
            if (Session["AdminAccount"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }
            var user = data.Users.SingleOrDefault(u => u.UserID == userId);
            return View(user);
        }

        public ActionResult ViewRecipe(int recipeId)
        {
            if (Session["AdminAccount"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }
            var recipe = data.Recipes.SingleOrDefault(r => r.RecipeID == recipeId);
            if (recipe == null)
            {
                return HttpNotFound(); 
            }

            var detailsList = data.RecipeDetails.Where(rd => rd.RecipeID == recipeId).ToList();

            EntitySet<RecipeDetail> entitySetDetails = new EntitySet<RecipeDetail>();
            foreach (var detail in detailsList)
            {
                entitySetDetails.Add(detail); 
            }


            recipe.RecipeDetails = entitySetDetails;

            ViewBag.CreatorUsername = data.Users.FirstOrDefault(u => u.UserID == recipe.CreatedBy)?.Username;

            return View(recipe);
        }

      
        [HttpPost]
        public ActionResult DeleteComment(int commentId)
        {
            if (Session["AdminAccount"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }
            var comment = data.Comments.SingleOrDefault(c => c.CommentID == commentId);
            if (comment != null)
            {
                data.Comments.DeleteOnSubmit(comment);
                data.SubmitChanges();
            }
            return RedirectToAction("ManageComments");
        }
        public ActionResult ViewComment(int commentId)
        {
            if (Session["AdminAccount"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }
            var comment = data.Comments.SingleOrDefault(c => c.CommentID == commentId);
            if (comment == null)
            {
                return HttpNotFound();
            }

            var user = data.Users.SingleOrDefault(u => u.UserID == comment.UserID);
            ViewBag.Username = user != null ? user.Username : "Không rõ";

            return View(comment);
        }

        [HttpGet]
        public ActionResult EditRecipeDetails(int recipeId)
        {
            if (Session["AdminAccount"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }

            var recipe = data.Recipes.SingleOrDefault(r => r.RecipeID == recipeId);
            if (recipe == null)
            {
                return HttpNotFound();
            }

            var recipeDetails = data.RecipeDetails.Where(rd => rd.RecipeID == recipeId).ToList();
            ViewBag.Categories = new SelectList(data.Categories, "CategoryID", "CategoryName", recipe.CategoryID);
            recipe.RecipeDetails = new EntitySet<RecipeDetail>();
            recipe.RecipeDetails.AddRange(recipeDetails);

            return View(recipe);
        }

        [HttpPost]
        public ActionResult EditRecipeDetails(Recipe model, List<string> stepsDescriptions, List<HttpPostedFileBase[]> stepsImages)
        {
            if (Session["AdminAccount"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }

            var recipe = data.Recipes.SingleOrDefault(r => r.RecipeID == model.RecipeID);
            if (recipe == null)
            {
                return HttpNotFound();
            }

            recipe.Title = model.Title ?? recipe.Title;
            recipe.MoTa = model.MoTa ?? recipe.MoTa;
            recipe.CategoryID = model.CategoryID != 0 ? model.CategoryID : recipe.CategoryID;

            data.SubmitChanges();

            // Update recipe details
            var existingDetails = data.RecipeDetails.Where(rd => rd.RecipeID == model.RecipeID).ToList();
            stepsDescriptions = stepsDescriptions ?? new List<string>();
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

            for (int i = 0; i < stepsDescriptions.Count; i++)
            {
                string images = "";
                if (stepsImages != null && stepsImages.Count > i && stepsImages[i] != null)
                {
                    string stepImagePath = Server.MapPath("~/Uploads/StepImages/");
                    if (!Directory.Exists(stepImagePath))
                    {
                        Directory.CreateDirectory(stepImagePath);
                    }

                    foreach (var stepImage in stepsImages[i])
                    {
                        if (stepImage != null && stepImage.ContentLength > 0)
                        {
                            string stepFileExtension = Path.GetExtension(stepImage.FileName).ToLower();
                            if (allowedExtensions.Contains(stepFileExtension))
                            {
                                string uniqueStepFileName = Guid.NewGuid() + stepFileExtension;
                                string fullStepImagePath = Path.Combine(stepImagePath, uniqueStepFileName);
                                stepImage.SaveAs(fullStepImagePath);
                                images += uniqueStepFileName + ";";
                            }
                        }
                    }
                }

                if (i < existingDetails.Count)
                {
                    var step = existingDetails[i];
                    step.MoTa = stepsDescriptions[i] ?? step.MoTa;
                    step.HinhAnh = !string.IsNullOrEmpty(images) ? images.TrimEnd(';') : step.HinhAnh;
                }
                else
                {
                    var step = new RecipeDetail
                    {
                        RecipeID = model.RecipeID,
                        StepNumber = i + 1,
                        MoTa = stepsDescriptions[i],
                        HinhAnh = images.TrimEnd(';')
                    };
                    data.RecipeDetails.InsertOnSubmit(step);
                }
            }

            data.SubmitChanges();
            TempData["SuccessMessage"] = "Cập nhật chi tiết công thức thành công!";
            return RedirectToAction("ManageRecipes");
        }



    }
}
