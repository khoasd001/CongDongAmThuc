using Doan2FixCSDL.Models;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
namespace Doan2FixCSDL.Controllers
{
    public class RecipesController : BaseController
    {
         public ActionResult Index(int? page)
         {
        // Lấy danh sách danh mục từ database
        ViewBag.Categories = data.Categories.ToList();

        // Lấy danh sách công thức đã được duyệt
        var recipes = data.Recipes.Where(r => r.IsApproved ?? false).ToList(); // Thêm chuyển đổi rõ ràng từ bool? sang bool

        // Số lượng công thức mỗi trang
        int pageSize = 10;

        // Số trang hiện tại, nếu không có tham số page thì mặc định là trang 1
        int pageNumber = (page ?? 1);

        // Trả về danh sách công thức đã phân trang vào View
        return View(recipes.ToPagedList(pageNumber, pageSize));
         }
        public ActionResult UserRecipes(int userId, int? page)
        {
            var userRecipes = data.Recipes.Where(r => r.CreatedBy == userId).OrderByDescending(r => r.CreatedDate).ToList();

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            ViewBag.User = data.Users.SingleOrDefault(u => u.UserID == userId);
            return View(userRecipes.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }

            ViewBag.Categories = new SelectList(data.Categories, "CategoryID", "CategoryName");
            return View();
        }
        [HttpPost]
        public ActionResult Create(Recipe recipe, HttpPostedFileBase mainImage, List<string> stepsDescriptions, List<HttpPostedFileBase[]> stepsImages)
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }
            recipe.CreatedBy = ((User)Session["Taikhoan"]).UserID;
            recipe.CreatedDate = DateTime.Now;
            recipe.IsApproved = false;
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            if (mainImage != null && mainImage.ContentLength > 0)
            {
                string fileExtension = Path.GetExtension(mainImage.FileName).ToLower();
                if (allowedExtensions.Contains(fileExtension))
                {
                    string mainImagePath = Server.MapPath("~/Uploads/MainImages/");
                    if (!Directory.Exists(mainImagePath))
                    {
                        Directory.CreateDirectory(mainImagePath);
                    }

                    string uniqueFileName = Guid.NewGuid() + fileExtension;
                    string fullPath = Path.Combine(mainImagePath, uniqueFileName);
                    mainImage.SaveAs(fullPath);
                    recipe.HinhAnh = uniqueFileName;
                }
                else
                {
                    ModelState.AddModelError("", "Định dạng ảnh bìa không hợp lệ.");
                    ViewBag.Categories = new SelectList(data.Categories, "CategoryID", "CategoryName", recipe.CategoryID);
                    return View(recipe);
                }
            }

            data.Recipes.InsertOnSubmit(recipe);
            data.SubmitChanges();
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
                            if (allowedExtensions.Contains(stepFileExtension)) // Sửa lỗi CS0103
                            {
                                string uniqueStepFileName = Guid.NewGuid() + stepFileExtension;
                                string fullStepImagePath = Path.Combine(stepImagePath, uniqueStepFileName);
                                stepImage.SaveAs(fullStepImagePath);
                                images += uniqueStepFileName + ";";
                            }
                        }
                    }
                }

                var step = new RecipeDetail
                {
                    RecipeID = recipe.RecipeID,
                    StepNumber = i + 1,
                    MoTa = stepsDescriptions[i],
                    HinhAnh = images.TrimEnd(';')
                };
                data.RecipeDetails.InsertOnSubmit(step);
            }

            data.SubmitChanges();
            TempData["SuccessMessage"] = "Công thức của bạn đã được tạo và đang chờ phê duyệt!";
            return RedirectToAction("Index");
        }
        public ActionResult Details(int id)
        {
            var recipe = data.Recipes.SingleOrDefault(r => r.RecipeID == id);
            if (recipe == null)
            {
                return HttpNotFound();
            }

            var detailsList = data.RecipeDetails.Where(rd => rd.RecipeID == id).ToList();
            recipe.RecipeDetails = new EntitySet<RecipeDetail>();
            recipe.RecipeDetails.AddRange(detailsList);

            var comments = data.Comments.Where(c => c.RecipeID == id).Select(c => new {
                c,
                User = data.Users.SingleOrDefault(u => u.UserID == c.UserID)
            }).ToList().Select(c => {
                c.c.User = c.User;
                return c.c;
            }).ToList();

            ViewBag.Comments = comments;

            ViewBag.LikeCount = data.Likes.Count(l => l.RecipeID == id);

            return View(recipe);
        }
        [HttpPost]
        public ActionResult FollowUser(int followedId, int recipeId, string returnUrl)
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }

            var userId = ((User)Session["Taikhoan"]).UserID;

            // Kiểm tra xem người dùng đã theo dõi chưa
            var existingFollow = data.UserFollows.SingleOrDefault(f => f.FollowerID == userId && f.FollowedID == followedId);
            if (existingFollow == null)
            {
                var userFollow = new UserFollow
                {
                    FollowerID = userId,
                    FollowedID = followedId,
                    FollowDate = DateTime.Now
                };

                data.UserFollows.InsertOnSubmit(userFollow);
                data.SubmitChanges();
            }

            return Redirect(returnUrl);
        }

        [HttpPost]
        public ActionResult UnfollowUser(int followedId, string returnUrl)
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }

            var userId = ((User)Session["Taikhoan"]).UserID;

            // Kiểm tra xem người dùng có đang theo dõi người này không
            var existingFollow = data.UserFollows.SingleOrDefault(f => f.FollowerID == userId && f.FollowedID == followedId);
            if (existingFollow != null)
            {
                data.UserFollows.DeleteOnSubmit(existingFollow);
                data.SubmitChanges();
            }

            return Redirect(returnUrl);
        }



        [HttpPost]
        public ActionResult Comment(int recipeId, string commentText, HttpPostedFileBase commentImage)
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }

            var user = (User)Session["Taikhoan"];
            var comment = new Comment
            {
                RecipeID = recipeId,
                UserID = user.UserID,
                CommentText = commentText,
                CommentDate = DateTime.Now
            };

            if (commentImage != null && commentImage.ContentLength > 0)
            {
                string directoryPath = Server.MapPath("~/Uploads/CommentImages/");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string uniqueFileName = Guid.NewGuid() + Path.GetExtension(commentImage.FileName);
                string imagePath = Path.Combine(directoryPath, uniqueFileName);
                commentImage.SaveAs(imagePath);
                comment.HinhAnh = uniqueFileName;
            }

            data.Comments.InsertOnSubmit(comment);
            data.SubmitChanges();

            return RedirectToAction("Details", new { id = recipeId });
        }

        [HttpPost]
        public ActionResult Like(int recipeId)
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }

            var user = (User)Session["Taikhoan"];
            var existingLike = data.Likes.SingleOrDefault(l => l.RecipeID == recipeId && l.UserID == user.UserID);

            if (existingLike == null)
            {
                var like = new Like
                {
                    RecipeID = recipeId,
                    UserID = user.UserID,
                    LikeDate = DateTime.Now
                };

                data.Likes.InsertOnSubmit(like);
                data.SubmitChanges();
            }

            return RedirectToAction("Details", new { id = recipeId });
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }

            var recipe = data.Recipes.SingleOrDefault(r => r.RecipeID == id && r.CreatedBy == ((User)Session["Taikhoan"]).UserID);
            if (recipe == null)
            {
                return HttpNotFound();
            }

            ViewBag.Categories = new SelectList(data.Categories, "CategoryID", "CategoryName", recipe.CategoryID);
            ViewBag.RecipeDetails = data.RecipeDetails.Where(rd => rd.RecipeID == id).ToList(); // Make sure to set ViewBag.RecipeDetails
            return View(recipe);
        }

        // Xử lý yêu cầu chỉnh sửa và cập nhật thông tin công thức
        [HttpPost]
        public ActionResult Edit(int id, Recipe model, List<string> stepsDescriptions = null)
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }

            var recipe = data.Recipes.SingleOrDefault(r => r.RecipeID == id && r.CreatedBy == ((User)Session["Taikhoan"]).UserID);
            if (recipe == null)
            {
                return HttpNotFound();
            }

            // Cập nhật thông tin công thức
            recipe.Title = model.Title;
            recipe.MoTa = model.MoTa;
            recipe.CategoryID = model.CategoryID;
            data.SubmitChanges();

            // Cập nhật chi tiết các bước
            var existingDetails = data.RecipeDetails.Where(rd => rd.RecipeID == id).ToList();
            stepsDescriptions = stepsDescriptions ?? new List<string>(); // Initialize stepsDescriptions if null
            for (int i = 0; i < stepsDescriptions.Count; i++)
            {
                if (i < existingDetails.Count)
                {
                    var step = existingDetails[i];
                    step.MoTa = stepsDescriptions[i];
                }
                else
                {
                    var step = new RecipeDetail
                    {
                        RecipeID = id,
                        StepNumber = i + 1,
                        MoTa = stepsDescriptions[i],
                        HinhAnh = existingDetails.ElementAtOrDefault(i)?.HinhAnh
                    };
                    data.RecipeDetails.InsertOnSubmit(step);
                }
            }

            data.SubmitChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Search(string query, int? page)
        {
            ViewBag.Categories = data.Categories.ToList();

            var searchResults = data.Recipes
                .Where(r => r.Title.Contains(query) || r.MoTa.Contains(query))
                .ToList();

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var pagedResults = searchResults.ToPagedList(pageNumber, pageSize);

            return View("Index", pagedResults);
        }

    }
}   

