using Doan2FixCSDL.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Doan2FixCSDL.Controllers
{
    public class UserController : BaseController
    {
        // GET: User

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Dangnhap()
        {
            return View();
        }

        public ActionResult Dangxuat()
        {
            Session["Taikhoan"] = null; // Xóa session người dùng
            return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chủ
        }

        [HttpPost]
        public ActionResult Dangnhap(FormCollection collection)
        {
            var tendn = collection["Useraname"];
            var matkhau = collection["PasswordHash"];
            if (String.IsNullOrEmpty(tendn))
            {
                ViewData["Loi1"] = "Phải nhập tên đăng nhập";
            }
            else if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Phải nhập mật khẩu";
            }
            else
            {
                // Kiểm tra tài khoản quản trị viên
                Admin admin = data.Admins.SingleOrDefault(a => a.Username == tendn && a.PasswordHash == matkhau);
                if (admin != null)
                {
                    Session["AdminAccount"] = admin;
                    return RedirectToAction("AdminDashboard", "Admin"); // Chuyển hướng đến trang quản trị
                }

                // Kiểm tra tài khoản người dùng
                User user = data.Users.SingleOrDefault(n => n.Username == tendn && n.PasswordHash == matkhau);
                if (user != null)
                {
                    // Tăng số lần đăng nhập
                    user.AccessCount = (user.AccessCount ?? 0) + 1;

                    // Lưu thay đổi vào cơ sở dữ liệu
                    data.SubmitChanges();

                    // Cập nhật avatar mặc định nếu chưa có
                    if (string.IsNullOrEmpty(user.Avatar))
                    {
                        user.Avatar = "default-avatar.png"; // Đặt tên tệp avatar mặc định
                    }

                    ViewBag.Thongbao = "Chúc mừng đăng nhập thành công";
                    Session["Taikhoan"] = user;
                    return RedirectToAction("Index", "Home"); // Chuyển hướng đến Dashboard
                }
                else
                {
                    ViewBag.Thongbao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult Signup()
        {
            return View();
        }

        [HttpPost]

        public ActionResult SignUp(FormCollection collection, User user)
        {
            // Gán các giá trị người dùng nhập liệu cho các biến
            var hoten = collection["HoTen"];
            var tendn = collection["Useraname"];
            var matkhau = collection["PasswordHash"];
            var matkhaunhaplai = collection["Matkhaunhaplai"];
            var email = collection["Email"];
            var ngaysinh = String.Format("{0:MM/dd/yyyy}", collection["NgaySinh"]);

            // Kiểm tra hợp lệ của các trường dữ liệu
            if (String.IsNullOrEmpty(hoten))
                ViewData["Loi1"] = "Họ tên khách hàng không được để trống";
            else if (String.IsNullOrEmpty(tendn))
                ViewData["Loi2"] = "Phải nhập tên đăng nhập";
            else if (String.IsNullOrEmpty(matkhau))
                ViewData["Loi3"] = "Phải nhập mật khẩu";
            else if (String.IsNullOrEmpty(matkhaunhaplai))
                ViewData["Loi4"] = "Phải nhập lại mật khẩu";
            if (String.IsNullOrEmpty(email))
            {
                ViewData["Loi5"] = "Email không được bỏ trống";
            }
            else
            {
                // Kiểm tra xem tên đăng nhập có bị trùng không
                User existingUser = data.Users.SingleOrDefault(n => n.Username == tendn);
                if (existingUser != null)
                {
                    ViewData["Loi2"] = "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.";
                }
                else
                {
                    // Gán giá trị cho đối tượng được tạo mới (user)
                    user.HoTen = hoten;
                    user.Username = tendn;
                    user.PasswordHash = matkhau; // Nên mã hóa mật khẩu
                    user.Email = email;
                    user.Ngaysinh = DateTime.Parse(ngaysinh);
                    try
                    {
                        data.Users.InsertOnSubmit(user);
                        data.SubmitChanges();
                        return RedirectToAction("Dangnhap"); // Chuyển hướng về trang đăng nhập
                    }
                    catch (Exception ex)
                    {
                        // Log lỗi (nếu có)
                        // ViewData["Loi5"] = "Có lỗi xảy ra khi tạo tài khoản. Vui lòng thử lại.";
                    }
                }
            }
            return View();
        }
    }
}
