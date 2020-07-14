using Accounts.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Accounts.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
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

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
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

        public void Logging(string userId, string message)
        {
            string browser = HttpContext.Request.Browser.Browser;
            string user_agent = HttpContext.Request.UserAgent;
            string url = HttpContext.Request.RawUrl;
            string ip = HttpContext.Request.UserHostAddress;
            string referrer = HttpContext.Request.UrlReferrer == null ? "" : HttpContext.Request.UrlReferrer.AbsoluteUri;

            using (var db = new ApplicationDbContext())
            {
                db.LogLogins.Add(new LogLogins()
                {
                    UserId = userId,
                    TimesLogin = DateTime.Now,
                    IP = ip,
                    Browser = browser,
                    UserAgent = user_agent,
                    Referrer = referrer,
                    Message = message
                });
                db.SaveChanges();
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

            // Сбои при входе не приводят к блокированию учетной записи
            // Чтобы ошибки при вводе пароля инициировали блокирование учетной записи, замените на shouldLockout: true
            ApplicationUser user;
            if (model.Email.Contains("@"))
                user = UserManager.FindByEmail(model.Email);
            else
                user = UserManager.FindByName(model.Email);

            if (user != null && !user.EmailConfirmed)
            {
                Logging(user.Id, "Попытка входа с не подтвержденной почтой");
                ModelState.AddModelError("", "Ваша электронная почта не подтверждена. Пройдите процедуру восстановления доступа!");
                return View(model);
            }

            if (user != null && user.DateBlocked != null)
            {
                Logging(user.Id, "Попытка входа в заблокированную учетную запись");
                ModelState.AddModelError("", "Учетная запись заблокирована, вход не возможен.");
                return View(model);
            }

            SignInStatus result;
            if (user == null)
                result = SignInStatus.Failure;
            else
                result = await SignInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, shouldLockout: true);

            switch (result)
            {
                case SignInStatus.Success:
                    //запись данных о входе
                    if (user.FirstLogin == null)
                        user.FirstLogin = DateTime.Now;
                    user.LastLogin = DateTime.Now;
                    UserManager.Update(user);
                    Logging(user.Id, $"Успешный вход в аккаунт {user.Email}");

                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    if (user != null)
                        Logging(user.Id, $"Неудачная попытка входа в учетную запись {user.Email}");
                    else
                        Logging("55eca6a1-2a5c-42cc-ae77-104f2e79145e", $"Неудачная попытка входа в учетную запись c неизвестной почты {model.Email}");
                    ModelState.AddModelError("", "Неудачная попытка входа.");
                    return View(model);
            }
        }

        public async Task<ActionResult> SecurityQuestion()
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                var model = new SecurityQuestionViewModel
                {
                    Answer = "",
                    Question = user.Question
                };
                return View(model);
            }
            else
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<ActionResult> SecurityQuestion([Bind(Include = "Question,Answer")] SecurityQuestionViewModel sq)
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            user.Question = sq.Question.Trim();
            if (!String.IsNullOrEmpty(sq.Answer))
                user.Answer = sq.Answer.Trim();
            user.Consent = true;

            var result = await UserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.Errors.First());
                return View();
            }

            return RedirectToAction("Index", "Home");
        }


        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Требовать предварительный вход пользователя с помощью имени пользователя и пароля или внешнего имени входа
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

            // Приведенный ниже код защищает от атак методом подбора, направленных на двухфакторные коды. 
            // Если пользователь введет неправильные коды за указанное время, его учетная запись 
            // будет заблокирована на заданный период. 
            // Параметры блокирования учетных записей можно настроить в IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Неправильный код.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        //[AllowAnonymous]
        //public ActionResult Register()
        //{
        //    return View();
        //}

        //
        // POST: /Account/Register
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = new ApplicationUser { Email = model.Email, Lastname = model.Lastname, Firstname = model.Firstname };
        //        var result = await UserManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

        //            // Дополнительные сведения о включении подтверждения учетной записи и сброса пароля см. на странице https://go.microsoft.com/fwlink/?LinkID=320771.
        //            // Отправка сообщения электронной почты с этой ссылкой
        //            string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
        //            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
        //            await UserManager.SendEmailAsync(user.Id, "Подтверждение учетной записи", "Для завершения регистрации перейдите по ссылке:: <a href=\""
        //                                               + callbackUrl + "\">завершить регистрацию</a>");
        //            return View("DisplayEmail");
        //            //return RedirectToAction("Index", "Home");
        //        }
        //        AddErrors(result);
        //    }

        //    // Появление этого сообщения означает наличие ошибки; повторное отображение формы
        //    return View(model);
        //}

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
            Settings currentYear;
            List<Group> groups;
            using (var db = new ApplicationDbContext())
            {
                currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
                groups = db.Groups.Where(a => a.IsDeleted == false && a.AcademicYear == currentYear.Value).OrderBy(a => a.Name).ToList();
            }

            //ViewBag.FacultyId = new SelectList(db.Faculties.OrderBy(a => a.Name), "Id", "Name");
            ViewBag.GroupId = new SelectList(groups, "Id", "Name");
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
                if (model.NumberOfRecordBook == null && model.Answer == null)
                {
                    using (var db = new ApplicationDbContext())
                    {
                        string l = model.Lastname.Trim();
                        string f = model.Firstname.Trim();
                        string m = (model.Middlename != null) ? model.Middlename.Trim() : "";
                        ApplicationUser user;
                        user = await db.Users.FirstOrDefaultAsync(a => a.Email == model.Email);
                        if (user != null)
                        {
                            ViewBag.Status = "FindEmail";
                            //Дополнительные сведения о включении подтверждения учетной записи и сброса пароля см.на странице https://go.microsoft.com/fwlink/?LinkID=320771.
                            //Отправка сообщения электронной почты с этой ссылкой
                            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                            await UserManager.SendEmailAsync(user.Id, "Сброс пароля",
                                "<p>Это автоматическая рассылка, на данное письмо не нужно отвечать.</p><br /><br />" +
                                "<p>На сайте СФ БашГУ был запрошен сброс пароля на Вашу почту. Если Вы этого не делали проигнорируйте это письмо, " +
                                "иначе сбросьте ваш пароль, щелкнув по ссылке <a href=\"" + callbackUrl + "\">" + callbackUrl + "</a>" +
                                "<p>В большинстве браузеров эта ссылка будет подсвечена синим цветом, если это не так просто скопируйте эту ссылку в адресную строку браузера и перейдите по ней.</p>" +
                                "Сброс пароля по этой ссылке необходимо произвести в течении часа. Если, с момента получения письма, прошло более одного часа, пройдите процедуру сброса пароля повторно.");
                            return View("ForgotPasswordConfirmation");
                        }

                        if (model.GroupId != null)
                        {
                            if (m == "")
                                user = await db.Users
                                    .Include("Group")
                                    .FirstOrDefaultAsync(a => a.Lastname.Trim() == l && a.Firstname.Trim() == f && a.GroupId == model.GroupId && a.DateBlocked == null);
                            else
                                user = await db.Users
                                    .Include("Group")
                                    .FirstOrDefaultAsync(a => a.Lastname.Trim() == l && a.Firstname.Trim() == f && a.Middlename.Trim() == m && a.GroupId == model.GroupId && a.DateBlocked == null);
                        }
                        else
                        {
                            user = await db.Users.FirstOrDefaultAsync(a => a.Lastname.Trim() == l && a.Firstname.Trim() == f && a.Middlename.Trim() == m && a.DecanatId == null);
                            if (user.Question == null)
                            {
                                ViewBag.Status = "NotSendEmail";
                                return View("ForgotPasswordConfirmation");
                            }
                        }

                        if (user == null)
                        {
                            ViewBag.Status = "NoFindUsers";
                            return View("ForgotPasswordConfirmation");
                        }
                        else
                        {
                            ViewBag.Status = "FindStudent";
                            //создаем временную модель, для ограничения передаваемой информации
                            var tempModel = new ForgotPasswordViewModel()
                            {
                                Id = user.Id,
                                Lastname = user.Lastname,
                                Firstname = user.Firstname,
                                Middlename = user.Middlename,
                                GroupId = (user.GroupId != null) ? user.GroupId : null,
                                Question = (user.Question != null) ? user.Question : null,
                                Email = model.Email,
                            };
                            return View("ForgotPassword", tempModel);
                        }
                    }
                }
                else
                {
                    ViewBag.Status = "FindStudent";
                    //проверка введенных ответов
                    var user = await UserManager.FindByIdAsync(model.Id);
                    if (user.DecanatId != null)
                    {
                        if ((model.NumberOfRecordBook != null && user.NumberOfRecordBook.Replace(" ", string.Empty).Replace("\r\n", string.Empty).ToUpper() == 
                            model.NumberOfRecordBook.ToString().Replace(" ", string.Empty).Replace("\r\n", string.Empty).ToUpper())
                            || (model.Answer != null && user.Answer.Trim().ToUpper() == model.Answer.Trim().ToUpper()))
                        {
                            user.Email = model.Email;
                            user.EmailConfirmed = true;
                            var result = await UserManager.UpdateAsync(user);
                            if (!result.Succeeded)
                            {
                                ModelState.AddModelError("", result.Errors.First());
                                return View();
                            }

                            //Дополнительные сведения о включении подтверждения учетной записи и сброса пароля см.на странице https://go.microsoft.com/fwlink/?LinkID=320771.
                            //Отправка сообщения электронной почты с этой ссылкой
                            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                            await UserManager.SendEmailAsync(user.Id, "Сброс пароля",
                                "<p>Это автоматическая рассылка, на данное письмо не нужно отвечать.</p><br /><br />" +
                                "<p>На сайте СФ БашГУ был запрошен сброс пароля для на Вашу почту. Если Вы этого не делали проигнорируйте это письмо, " +
                                "иначе сбросьте ваш пароль, щелкнув по ссылке <a href=\"" + callbackUrl + "\">" + callbackUrl + "</a>" +
                                "<p>В большенстве браузеров эта ссылка будет подсвечена синим цветом, если это не так просто скопируйте эту ссылку в адресную строку браузера и перейдите по ней.</p>");
                            return View("ForgotPasswordConfirmation");
                        }
                        else
                        {
                            return View("ForgotPasswordConfirmation");
                        }
                    }
                    else
                    {
                        if (user.Answer.Trim().ToUpper() == model.Answer.Trim().ToUpper())
                        {
                            ViewBag.Status = "FindTeacher";
                            user.Email = model.Email;
                            user.EmailConfirmed = true;
                            var result = await UserManager.UpdateAsync(user);
                            if (!result.Succeeded)
                            {
                                ModelState.AddModelError("", result.Errors.First());
                                return View();
                            }

                            //Дополнительные сведения о включении подтверждения учетной записи и сброса пароля см.на странице https://go.microsoft.com/fwlink/?LinkID=320771.
                            //Отправка сообщения электронной почты с этой ссылкой
                            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                            await UserManager.SendEmailAsync(user.Id, "Сброс пароля",
                                "<p>Это автоматическая рассылка, на данное письмо не нужно отвечать.</p><br /><br />" +
                                "<p>На сайте СФ БашГУ был запрошен сброс пароля для на Вашу почту. Если Вы этого не делали проигнорируйте это письмо, " +
                                "иначе сбросьте ваш пароль, щелкнув по ссылке <a href=\"" + callbackUrl + "\">" + callbackUrl + "</a>" +
                                "<p>В большенстве браузеров эта ссылка будет подсвечена синим цветом, если это не так просто скопируйте эту ссылку в адресную строку браузера и перейдите по ней.</p>");
                            return View("ForgotPasswordConfirmation");
                        }
                        else ViewBag.Status = "NotSendEmail";
                    }
                }
            }

            // Появление этого сообщения означает наличие ошибки; повторное отображение формы
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
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Не показывать, что пользователь не существует
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            user.EmailConfirmed = true;
            await UserManager.UpdateAsync(user);
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                Logging(user.Id, $"Сброс пароля. Аккаунт {user.Email}");
                //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);                
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
            // Запрос перенаправления к внешнему поставщику входа
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

            // Создание и отправка маркера
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
            // Выполнение входа пользователя посредством данного внешнего поставщика входа, если у пользователя уже есть имя входа
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
                    // Если у пользователя нет учетной записи, то ему предлагается создать ее
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
                // Получение сведений о пользователе от внешнего поставщика входа
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
            HttpContext.Session.Clear();
            HttpContext.Session.Abandon();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

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

        #region Вспомогательные приложения
        // Используется для защиты от XSRF-атак при добавлении внешних имен входа
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