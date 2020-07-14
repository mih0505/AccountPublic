using Accounts.Models;
using Fluentx.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Accounts.Controllers
{

    public class MoodleController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public MoodleController()
        {
        }

        public MoodleController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        private ApplicationUserManager _userManager;
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

        private ApplicationRoleManager _roleManager;
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


        // GET: Moode
        public ActionResult MoodleHome()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            Dictionary<string, object> postData = new Dictionary<string, object>();
            if (user != null)
            {
                //по непонятной причине Moodle перестал авторизовывать студентов по логину, вход работает только по почте
                //и касается это только тех, кто выполнил обновление своего пароля в ЛК

                if (User.IsInRole("Students"))
                    postData.Add("username", user.DecanatId);
                else
                    postData.Add("username", user.Email.ToLower());
                postData.Add("password", user.PassMoodle);
                return this.RedirectAndPost("http://sdo.strbsu.ru/login/index.php", postData);
            }
            return View();
        }

        public ActionResult MoodleMessages()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            Dictionary<string, object> postData = new Dictionary<string, object>();
            if (user != null)
            {
                if (User.IsInRole("Students"))
                    postData.Add("username", user.DecanatId);
                else
                    postData.Add("username", user.Email.ToLower());

                postData.Add("password", user.PassMoodle);
                return this.RedirectAndPost("http://sdo.strbsu.ru/login/index.php", postData);
            }
            return View("MoodleHome");
        }

        public ActionResult UpdateMoodlePassword()
        {
            //обновление пароля мудл на сайте личных кабинетов
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            if (user != null)
                user.PassMoodle = Guid.NewGuid().ToString();
            db.Entry(user).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            //определение ролей пользователя
            var userRoles = UserManager.GetRoles(user.Id);

            //обновление пароля на самом moodle
            using (var myConnection = new NpgsqlConnection("Server=46.191.192.102;Port=5432;User Id=postgres;Password=LOR1s2psql;Database=moodledb;"))
            {
                string hash = "";
                using (MD5 md5Hash = MD5.Create())
                {
                    hash = GetMd5Hash(md5Hash, user.PassMoodle);
                }
                NpgsqlCommand mySQLCommand;
                string CommText = (userRoles.IndexOf("Students") == -1) ? "UPDATE mdl_user SET password='" + hash + "' WHERE email='" + user.Email + "' AND deleted = 0" :
                    "UPDATE mdl_user SET password='" + hash + "', email='" + user.Email + "' WHERE username='" + user.DecanatId + "' AND deleted = 0";
                myConnection.Open();
                mySQLCommand = new NpgsqlCommand(CommText, myConnection);
                int c = mySQLCommand.ExecuteNonQuery();
                myConnection.Close();
            }

            return View();
        }

        public ActionResult UpdateMoodleAllPasswords()
        {
            //получение студентов и текущего учебного года
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
            var users = db.Users.Include(a => a.Group).Where(a => a.Group.IsDeleted == false && a.Group.AcademicYear == currentYear.Value).ToList();

            StringBuilder sb = new StringBuilder(20);
            string guid;
            //csv-файл с обновленными паролями 
            for (int i = 0; i < users.Count; i++)
            {
                if (String.IsNullOrEmpty(users[i].PassMoodle))
                {
                    guid = Guid.NewGuid().ToString();
                    users[i].PassMoodle = guid;
                    db.Entry(users[i]).State = EntityState.Modified;

                    if (sb.Length == 0)
                        sb.AppendLine("Username;Password;Lastname;Firstname;Email");

                    sb.AppendLine($"{users[i].UserName};{users[i].PassMoodle};{users[i].Lastname};{users[i].Firstname};{users[i].Email}");
                }
                else
                {
                    if (sb.Length == 0)
                        sb.AppendLine("Username;Password;Lastname;Firstname;Email");

                    sb.AppendLine($"{users[i].UserName};{users[i].PassMoodle};{users[i].Lastname};{users[i].Firstname};{users[i].Email}");
                }

            }
            //создание csv-файла для обновления паролей студентов
            FileStream fGroup = new FileStream("D:\\WebSites\\Account\\users.csv", FileMode.Append);
            StreamWriter wrGroup = new StreamWriter(fGroup);
            wrGroup.Write(sb);
            wrGroup.Close();
            fGroup.Close();

            //сохранение паролей в бд сайта
            db.SaveChanges();

            return View();
        }

        public bool UpdateMoodlePasswordAndSendEmail(string id)
        {
            //обновление пароля мудл на сайте личных кабинетов
            var user = db.Users.Find(id);
            if (user != null && user.PassMoodle == null)
            {
                user.PassMoodle = Guid.NewGuid().ToString();
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                //обновление пароля на самом moodle
                using (var myConnection = new NpgsqlConnection("Server=46.191.192.102;Port=5432;User Id=postgres;Password=LOR1s2psql;Database=moodledb;"))
                {
                    string hash = "";
                    using (MD5 md5Hash = MD5.Create())
                    {
                        hash = GetMd5Hash(md5Hash, user.PassMoodle);
                    }
                    NpgsqlCommand mySQLCommand;
                    string CommText = "UPDATE mdl_user SET password='" + hash + "' WHERE username='" + user.DecanatId + "'";
                    myConnection.Open();
                    mySQLCommand = new NpgsqlCommand(CommText, myConnection);
                    int c = mySQLCommand.ExecuteNonQuery();
                    myConnection.Close();
                }

                //отправляем письмо
                var dtCheckEmail = new DataTable();
                if (!user.EmailConfirmed)
                {
                    //если почта в ЛК не подтверждена, получаем ее из мудл
                    using (var myConnection = new NpgsqlConnection("Server=46.191.192.102;Port=5432;User Id=postgres;Password=LOR1s2psql;Database=moodledb;"))
                    {
                        //обращаемся к мудл, т.к. в ЛК нет почты пользователя
                        string checkEmail = "SELECT * FROM mdl_user WHERE username='" + user.DecanatId + "'";
                        myConnection.Open(); //Устанавливаем соединение с базой данных.
                        var daCheckEmail = new NpgsqlDataAdapter(checkEmail, myConnection);
                        daCheckEmail.Fill(dtCheckEmail);
                        myConnection.Close();
                    }
                    //если почта в мудле не пустая, сохраняем ее в ЛК и отправляем на нее письмо с паролем к ЛК
                    if (dtCheckEmail.Rows.Count > 0 && !String.IsNullOrEmpty(dtCheckEmail.Rows[0]["email"].ToString()))
                    {
                        //сохранение почты в ЛК
                        user.Email = dtCheckEmail.Rows[0]["email"].ToString();
                        user.EmailConfirmed = true;
                        db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //отправка письма
                        string newPassword = GeneratePassword();
                        string code = UserManager.GeneratePasswordResetToken(user.Id);
                        var result = UserManager.ResetPassword(user.Id, code, newPassword);
                        if (result.Succeeded)
                        {
                            dynamic email = new Postal.Email("SendEmailWithNewPassword");
                            email.From = "system.sdo.strbsu@gmail.com";
                            email.To = user.Email;
                            email.Subject = "Доступ к электронным учебным курсам в СФ БашГУ";
                            email.NewPassword = newPassword;
                            email.UserName = user.Email;
                            email.Send();
                            ViewBag.Result = "Пароль обновлен и отправлен на почту.";
                        }
                        else ViewBag.Result = "Ошибка обновления пароля!!!";
                    }
                }
                else
                {
                    //проверяем одинаковая ли почта в ЛК и мудле, если нет, то заменяем на ту, которая в ЛК
                    using (var myConnection = new NpgsqlConnection("Server=46.191.192.102;Port=5432;User Id=postgres;Password=LOR1s2psql;Database=moodledb;"))
                    {
                        //обращаемся к мудл, т.к. в ЛК нет почты пользователя
                        string checkEmail = "SELECT * FROM mdl_user WHERE username='" + user.DecanatId + "'";
                        myConnection.Open(); //Устанавливаем соединение с базой данных.
                        var daCheckEmail = new NpgsqlDataAdapter(checkEmail, myConnection);
                        daCheckEmail.Fill(dtCheckEmail);
                        myConnection.Close();

                        if (dtCheckEmail.Rows[0]["email"].ToString() != user.Email)
                        {
                            NpgsqlCommand mySQLCommand;
                            string CommText = "UPDATE mdl_user SET email='" + user.Email + "' WHERE username='" + user.DecanatId + "'";
                            myConnection.Open();
                            mySQLCommand = new NpgsqlCommand(CommText, myConnection);
                            int c = mySQLCommand.ExecuteNonQuery();
                        }
                    }

                    dynamic email = new Postal.Email("SendEmailWithoutNewPassword");
                    email.From = "system.sdo.strbsu@gmail.com";
                    email.To = user.Email;
                    email.Subject = "Доступ к электронным учебным курсам в СФ БашГУ";
                    email.Send();
                    ViewBag.Result = "Информационное письмо отправлено";
                }
                return true;
            }
            else
                return false;
        }

        public ActionResult UpdateMoodlePasswordAndSend(string id)
        {
            UpdateMoodlePasswordAndSendEmail(id);
            return View("UpdateMoodlePassword");
        }

        //
        //public ActionResult UpdateMoodlePasswordAndSendGroup(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var group = db.Groups.Find(id);
        //    ViewBag.CurrentGroup = group;
        //    var listStudents = db.Users.Where(a => a.GroupId == id && a.DateBlocked == null).OrderBy(a => a.Lastname).ToList();
        //    foreach(var user in listStudents)
        //        UpdateMoodlePasswordAndSendEmail(user.Id);

        //    return View("UpdateMoodlePassword");
        //}


        //создание хэша пароля
        static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string.
            string hash = sBuilder.ToString();
            return hash;
        }

        private string GeneratePassword()
        {
            string password = "";
            char[] alphabet = "QWERTYUPASDFGHJKLZXCVBNMqwertyuiopasdfghjkzxcvbnm23456789".ToCharArray();
            var r = new Random();
            for (int i = 0; i < 6; i++)
            {
                password += alphabet[r.Next(alphabet.Length)];
            }

            return password;
        }
    }
}