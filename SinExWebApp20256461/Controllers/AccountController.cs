﻿using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SinExWebApp20256461.Models;
using SinExWebApp20256461.ViewModels;
using System.Net;
using System.Data.Entity.Validation;
using System.Net.Mail;

namespace SinExWebApp20256461.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private SinExWebApp20256461Context db = new SinExWebApp20256461Context();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register(string accountType, int? error)
        {
            ViewBag.AccountType = accountType;
            ViewBag.Cities = db.Destinations.Select(a => a.City).Distinct().ToList();
            ViewBag.ProvinceCodes = db.Destinations.Select(a => a.ProvinceCode).Distinct().ToList();
            if(error == 1)
            {
                ViewBag.errorMessage = "the credit card number is invalid";
            }
            else if(error == 2)
            {
                ViewBag.errorMessage = "City and Province must match";
            }
            else if (error == 3)
            {
                ViewBag.errorMessage = "Credit card expire date error";
            }
            return View(new RegisterCustomerViewModel());
        }


        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterCustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.PersonalInformation != null)
                {
                    model.LoginInformation.Email = model.PersonalInformation.EmailAddress;
                    if (!ValidateCard(model.PersonalInformation.CardNumber, model.PersonalInformation.CardType))
                    {
                        return RedirectToAction("Register", "Account", new { accountType = "Personal", error = 1 });
                    }
                    DateTime expireDate = new DateTime(int.Parse(model.PersonalInformation.Year), int.Parse(model.PersonalInformation.Month), 1);
                    if (!isTodayOrLater(expireDate))
                    {
                        return RedirectToAction("Register", "Account", new { accountType = "Personal", error = 3 });
                    }
                }
                else // AccountType = "Business"
                {
                    model.LoginInformation.Email = model.BusinessInformation.EmailAddress;
                    if (!ValidateCard(model.BusinessInformation.CardNumber, model.BusinessInformation.CardType))
                    {
                        return RedirectToAction("Register", "Account", new { accountType = "Business", error = 1 });
                    }
                    DateTime expireDate = new DateTime(int.Parse(model.BusinessInformation.Year), int.Parse(model.BusinessInformation.Month), 1);
                    if (!isTodayOrLater(expireDate))
                    {
                        return RedirectToAction("Register", "Account", new { accountType = "Business", error = 3 });
                    }
                }
                var user = new ApplicationUser { UserName = model.LoginInformation.UserName, Email = model.LoginInformation.Email };
                var result = await UserManager.CreateAsync(user, model.LoginInformation.Password);
                if (result.Succeeded)
                {
                    // Assign user to Customer role.
                    var roleResult = await UserManager.AddToRolesAsync(user.Id, "Customer");
                    if (roleResult.Succeeded)
                    {
                        // Create a shipping account for the customer.
                        int id = db.ShippingAccounts.Count() + 1;
                        string strID = id.ToString().PadLeft(12, '0');
                        ShippingAccount shippingacct = null;
                        if (model.PersonalInformation != null)
                        {
                            model.PersonalInformation.UserName = user.UserName;
                            model.PersonalInformation.ShippingAccountNumber = strID;
                            model.PersonalInformation.ShippingAccountId = id;
                            db.ShippingAccounts.Add(model.PersonalInformation);
                            shippingacct = model.PersonalInformation;
                        }
                        else
                        {
                            model.BusinessInformation.UserName = user.UserName;
                            model.BusinessInformation.ShippingAccountNumber = strID;
                            model.BusinessInformation.ShippingAccountId = id;
                            db.ShippingAccounts.Add(model.BusinessInformation);
                            shippingacct = model.BusinessInformation;
                        }
                        try
                        {
                            if (ValidCity(shippingacct.City))
                            {
                                string ProvinceCode = db.Destinations.SingleOrDefault(a => a.City == shippingacct.City).ProvinceCode;
                                if (ProvinceCode != shippingacct.ProvinceCode)
                                {
                                    ViewBag.errorMessage = "City and Province must match";
                                    // return View(model);
                                    if (shippingacct is PersonalShippingAccount)
                                    {
                                        return RedirectToAction("register", "account", new { accounttype = "personal" , error = 2});
                                    }
                                    else
                                    {
                                        return RedirectToAction("register", "account", new { accounttype = "business", error = 2 });
                                    }

                                }
                            }
                            else
                            {
                                ViewBag.errorMessage = "Please input a valid city";
                                if (shippingacct is PersonalShippingAccount)
                                {
                                    return RedirectToAction("Register", "Account", new { accountType = "Personal" });
                                }
                                else
                                {
                                    return RedirectToAction("Register", "Account", new { accountType = "Business" });
                                }
                                // return View(model);

                            }
                            db.SaveChanges();
                            // send confirmation email
                            MailMessage mailMessage = new MailMessage();
                            //Add recipients
                            ShippingAccount account = null;
                            if (model.PersonalInformation != null)
                            {
                                // personal shipping account
                                account = model.PersonalInformation;
                            }
                            else
                            {
                                // business shipping account
                                account = model.BusinessInformation;
                            }
                            mailMessage.To.Add(account.EmailAddress);

                            //Setting the displayed email address and display name
                            //!!!Do not use this to prank others!!!
                            mailMessage.From = new MailAddress("notification@sinex.com", "SinEx Confirmation");
                            var name = account.UserName;
                            //Subject and content of the email
                            mailMessage.Subject = "Account Confirmation";
                            mailMessage.Body = "Dear " + name + ",\n \n your account with account number " + account.ShippingAccountNumber +
                                " has been created.\n Thank you for choosing SinEx \n\n Best regards,\nSino Express";
                            mailMessage.Priority = MailPriority.Normal;

                            //Instantiate a new SmtpClient instance
                            SmtpClient smtpClient = new SmtpClient("smtp.cse.ust.hk");

                            //WARNING: DO NOT set any credentials and other settings!!!
 
                            //Send
                            try
                            {
                                smtpClient.Send(mailMessage);
                            }
                            catch (Exception e)
                            {
                                ViewBag.errorMessage = e;
                                //return View();
                            }
                        }
                        catch (DbEntityValidationException e)
                        {
                            foreach (var eve in e.EntityValidationErrors)
                            {
                                Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                foreach (var ve in eve.ValidationErrors)
                                {
                                    Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                        ve.PropertyName, ve.ErrorMessage);
                                }
                            }
                            throw;
                        }
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        AddErrors(roleResult);
                    }
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            if (model.PersonalInformation != null)
            {
                return RedirectToAction("Register", "Account", new { accountType = "Personal" });
            }
            else // AccountType = "Business"
            {
                return RedirectToAction("Register", "Account", new { accountType = "Business" });
            }
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
        // GET: /Account/getCustomerRecord
        /*
        public ActionResult GetCustomerRecord()
        {
            string userName = System.Web.HttpContext.Current.User.Identity.Name;
            if (userName == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShippingAccount ShippingAccount = 
        }*/
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}