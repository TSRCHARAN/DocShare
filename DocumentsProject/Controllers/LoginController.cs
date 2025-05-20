using DocumentsProject.Core.Domain.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin.Messaging;

namespace DocumentsProject.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILoginService _loginService;
        private readonly IMailService _mailService;
        private readonly IUserService _userService;
        public LoginController(ILoginService loginService, IMailService mailService, IUserService userService)
        {
            _loginService = loginService;
            _mailService = mailService;
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        public async Task<JsonResult> VerifyUser(string useremail, string password, bool rememberMe)
        {
            var isValidUser = await _loginService.VerifyUser(useremail, password);

            if (isValidUser.Success)
            {
                var userDetails = await _loginService.GetUserDetails(useremail);
                // Create claims for the authenticated user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userDetails.Resource.ToString()),
                    new Claim(ClaimTypes.Name, userDetails.Resource.ToString()),
                    new Claim(ClaimTypes.Email, useremail)
                };

                // Create a ClaimsIdentity and sign the user in
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                
                //var authProperties = new AuthenticationProperties
                //{
                //    IsPersistent = true,  // Keep the user logged in even after closing the browser
                //    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10)
                //};

                // Create authentication properties
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = rememberMe,  // Keep the user logged in even after closing the browser
                    ExpiresUtc = rememberMe ? DateTime.UtcNow.AddDays(14) : DateTime.UtcNow.AddMinutes(10) // Expiration period
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return Json(new { success = true, message = "Login successful!" });
            }
            else if (!isValidUser.Success && isValidUser.Message == "User not found")
            {
                return Json(new { success = false, message = "Email Not Found" });
            }
            else
            {
                return Json(new { success = false, message = "Invalid Password" });
            }
        }

        public async Task<JsonResult> SignUpUser(string useremail, string otp, string username, string password)
        {
            var verifyOtp = await VerifyOtpForSignUp(useremail, otp);
            if (verifyOtp == false)
            {
                return Json(new { success = false, message = "OTP was wrong" });
            }

            var createUser = await _loginService.SignUpUser(useremail, username, password);
            if (createUser == null)
            {
                return Json(new { success = false, message = createUser.Message });
            }
            return Json(new { success = true, message = createUser.Message });
        }

        public async Task<JsonResult> SendOtpForForgetPassword(string userEmail)
        {
            var checkIfUserExists = await _userService.CheckIfUserExists(userEmail);
            if (checkIfUserExists == false)
            {
                return Json(new { success = false, message = "Email not found" });
            }
            var res = await _mailService.SendMail(userEmail, "ForgetPassword");
            if (res == false)
            {
                return Json(new { success = false, message = "Failed to send mail" });
            }
            return Json(new { success = true, message = "Successfully set OTP to your Mail" });
        }

        public async Task<JsonResult> SendOtpForSignUp(string userEmail)
        {
            var checkIfUserExists = await _userService.CheckIfUserExists(userEmail);
            if (checkIfUserExists == true)
            {
                return Json(new { success = false, message = "Email already exists" });
            }
            var res = await _mailService.SendMail(userEmail, "SignUp");
            if (res == false)
            {
                return Json(new { success = false, message = "Failed to send mail" });
            }
            return Json(new { success = true, message = "Successfully set OTP to your Mail" });
        }

        public async Task<JsonResult> VerifyOtpForForgetPassword(string userEmail, string otp)
        {
            var res = await _userService.VerifyOtp(userEmail, otp);
            if (res == false)
            {
                return Json(new { success = false, message = "Wrong OTP" });
            }
            return Json(new { success = true, message = "OTP Verified Successful" });
        }

        public async Task<bool> VerifyOtpForSignUp(string userEmail, string otp)
        {
            var res = await _userService.VerifyOtp(userEmail, otp);
            return res;
        }

        public async Task<JsonResult> UpdatePassword(string userEmail, string newPassword)
        {
            var res = await _userService.UpdatePassword(userEmail, newPassword);
            if (res == false)
            {
                return Json(new { success = false, message = "Failed to update password" });
            }
            return Json(new { success = true, message = "Password updated Successfully" });
        }

        public async Task<JsonResult> ChangeUserPassword(string oldPassword, string newPassword)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var checkOldPassword = await _userService.CheckOldPassword(userEmail, oldPassword);
            if(checkOldPassword == false)
            {
                return Json(new { success = false, message = "Entered Old password did not matched with your current password" });
            }
            if(oldPassword == newPassword)
            {
                return Json(new { success = false, message = "New Password can't be same as old password" });
            }
            var res = await _userService.UpdatePassword(userEmail, newPassword);
            if (res == false)
            {
                return Json(new { success = false, message = "Failed to change password" });
            }
            return Json(new { success = true, message = "Password changed Successfully" });
        }
    }
}
