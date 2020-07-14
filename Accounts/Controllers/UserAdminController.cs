using Accounts.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Npgsql;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Accounts.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class UsersAdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public UsersAdminController()
        {
        }

        public UsersAdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
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

        //
        // GET: /Users/
        public ActionResult Index(string sortOrder, string lastname, string email, string group, string role, int? page)
        {
            // Создание первого (стандартного) элемента (чтобы избежать бага с фильтрацией данных)
            var defaultItem = new SelectListItem()
            {
                Value = "-1",
                Text = "Все"
            };

            // Заполнение DropDownList с ролями
            var rolesList = RoleManager.Roles;
            var rolesView = new List<SelectListItem>()
            {
                defaultItem
            };

            foreach (Microsoft.AspNet.Identity.EntityFramework.IdentityRole item in rolesList)
            {
                rolesView.Add(new SelectListItem()
                {
                    Value = item.Id.ToString(),
                    Text = item.Name
                });
            }

            // Получение списка всех пользователей
            IQueryable<ApplicationUser> usersList = UserManager.Users.Include("Group").Include("Roles").OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).ThenBy(a => a.Middlename);

            // Получение Cookies
            var cookieGroup = Request.Cookies.Get("user_group");
            var cookieSortOrder = Request.Cookies.Get("user_sortOrder");
            var cookieLastname = Request.Cookies.Get("user_lastname");
            var cookieEmail = Request.Cookies.Get("user_email");
            var cookieRole = Request.Cookies.Get("user_role");

            // Применение значений из Cookies
            // В том числе обновление значений в DropDownList согласно полученным Cookies
            if (group == null && cookieGroup != null)
            {
                group = cookieGroup.Value;
            }

            if (sortOrder == null && cookieSortOrder != null)
            {
                sortOrder = cookieSortOrder.Value;
            }

            if (lastname == null && cookieLastname != null)
            {
                lastname = cookieLastname.Value;
            }

            if (email == null && cookieEmail != null)
            {
                email = cookieEmail.Value;
            }

            if (role == null && cookieRole != null)
            {
                rolesView.FirstOrDefault(Role => (Role.Value == cookieRole.Value)).Selected = true;
                role = cookieRole.Value;
            }

            // Удаление Cookies
            Response.Cookies.Add(new HttpCookie("user_group")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("user_sortOrder")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("user_lastname")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("user_email")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("user_role")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            // Применение фильтров
            // В том числе добавление новых Cookies
            if (sortOrder != null)
            {
                Response.Cookies.Add(new HttpCookie("user_sortOrder")
                {
                    Value = sortOrder.ToString(),
                    Expires = DateTime.Now.AddMinutes(15)
                });
            }

            if (!String.IsNullOrEmpty(lastname))
            {
                Response.Cookies.Add(new HttpCookie("user_lastname")
                {
                    Value = lastname,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                usersList = usersList.Where(p => SqlFunctions.PatIndex("%" + lastname + "%", p.Lastname) > 0);
            }

            if (!String.IsNullOrEmpty(email))
            {
                Response.Cookies.Add(new HttpCookie("user_email")
                {
                    Value = email,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                usersList = usersList.Where(p => SqlFunctions.PatIndex("%" + email + "%", p.Email) > 0);
            }

            if (!String.IsNullOrEmpty(group))
            {
                Response.Cookies.Add(new HttpCookie("user_group")
                {
                    Value = group,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                usersList = usersList.Where(p => SqlFunctions.PatIndex("%" + group + "%", p.Group.Name) > 0);
            }

            if (!String.IsNullOrEmpty(role))
            {
                Response.Cookies.Add(new HttpCookie("user_role")
                {
                    Value = role,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (role != (-1).ToString())
                {
                    usersList = usersList.Where(x => x.Roles.Select(y => y.RoleId).Contains(role));
                }
            }

            // Применение сортировки
            switch (sortOrder)
            {
                case "lastname_desc":
                    usersList = usersList.OrderByDescending(s => s.Lastname).ThenByDescending(a => a.Firstname).ThenByDescending(a => a.Middlename);
                    break;
                case "email_asc":
                    usersList = usersList.OrderBy(s => s.Email);
                    break;
                case "email_desc":
                    usersList = usersList.OrderByDescending(s => s.Email);
                    break;
                case "group_asc":
                    usersList = usersList.OrderBy(s => s.Group.Name);
                    break;
                case "group_desc":
                    usersList = usersList.OrderByDescending(s => s.Group.Name);
                    break;
                default:
                    usersList = usersList.OrderBy(s => s.Lastname).ThenBy(a => a.Firstname).ThenBy(a => a.Middlename);
                    break;
            }

            // Сохранение данных во ViewBag
            ViewBag.CurrentSort = sortOrder;
            ViewBag.RoleId = rolesView;
            ViewBag.LastnameSortParm = String.IsNullOrEmpty(sortOrder) ? "lastname_desc" : "";
            ViewBag.EmailSortParm = sortOrder == "email_asc" ? "email_desc" : "email_asc";
            ViewBag.GroupSortParm = sortOrder == "group_asc" ? "group_desc" : "group_asc";
            ViewBag.LastnameFilter = lastname;
            ViewBag.EmailFilter = email;
            ViewBag.GroupFilter = group;
            ViewBag.RoleFilter = role;

            // Вывод результата
            return View(usersList.ToPagedList(page ?? 1, 30));

            #region Старый код
            //HttpCookie cookieGroup = Request.Cookies.Get("user_group");
            //HttpCookie cookieSortOrder = Request.Cookies.Get("user_sortOrder");
            //HttpCookie cookieLastname = Request.Cookies.Get("user_lastname");
            //HttpCookie cookieEmail = Request.Cookies.Get("user_email");
            //HttpCookie cookieRole = Request.Cookies.Get("user_role");

            //if (cookieGroup != null)
            //{
            //    if (group != null && group != cookieGroup.Value)
            //        cookieGroup.Value = group;
            //    else
            //        group = cookieGroup.Value;
            //}
            //if (cookieSortOrder != null)
            //{
            //    sortOrder = cookieSortOrder.Value;
            //}
            //if (cookieLastname != null)
            //{
            //    if (lastname != null && lastname != cookieLastname.Value)
            //        cookieLastname.Value = lastname;
            //    else
            //        lastname = cookieLastname.Value;
            //}
            //if (cookieEmail != null)
            //{
            //    if (email != null && email != cookieEmail.Value)
            //        cookieEmail.Value = email;
            //    else
            //        email = cookieEmail.Value;
            //}
            //if (cookieRole != null)
            //{
            //    if (role != null && role != cookieRole.Value)
            //        cookieRole.Value = role.ToString();
            //    else
            //        role = cookieRole.Value;
            //}

            //ViewBag.CurrentSort = sortOrder;
            //ViewBag.RoleId = new SelectList(RoleManager.Roles, "Id", "Name");
            //ViewBag.LastnameSortParm = String.IsNullOrEmpty(sortOrder) ? "lastname_desc" : "";
            //ViewBag.EmailSortParm = sortOrder == "email_asc" ? "email_desc" : "email_asc";
            //ViewBag.GroupSortParm = sortOrder == "group_asc" ? "group_desc" : "group_asc";

            //ViewBag.LastnameFilter = lastname;
            //ViewBag.EmailFilter = email;
            //ViewBag.GroupFilter = group;
            //ViewBag.RoleFilter = role;

            //IQueryable<ApplicationUser> lstUsers = UserManager.Users.Include("Group").Include("Roles").OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).ThenBy(a => a.Middlename);

            //if (!String.IsNullOrEmpty(lastname))
            //{
            //    cookieLastname = new HttpCookie("user_lastname");
            //    cookieLastname.Value = lastname;
            //    cookieLastname.Expires = DateTime.Now.AddMinutes(15);
            //    Response.Cookies.Add(cookieLastname);
            //    lstUsers = lstUsers.Where(p => SqlFunctions.PatIndex("%" + lastname + "%", p.Lastname) > 0);
            //}

            //if (!String.IsNullOrEmpty(email))
            //{
            //    cookieEmail = new HttpCookie("user_email");
            //    cookieEmail.Value = email;
            //    cookieEmail.Expires = DateTime.Now.AddMinutes(15);
            //    Response.Cookies.Add(cookieEmail);
            //    lstUsers = lstUsers.Where(p => SqlFunctions.PatIndex("%" + email + "%", p.Email) > 0);
            //}

            //if (!String.IsNullOrEmpty(group))
            //{
            //    cookieGroup = new HttpCookie("user_group");
            //    cookieGroup.Value = group;
            //    cookieGroup.Expires = DateTime.Now.AddMinutes(15);
            //    Response.Cookies.Add(cookieGroup);
            //    lstUsers = lstUsers.Where(p => SqlFunctions.PatIndex("%" + group + "%", p.Group.Name) > 0);
            //}

            //if (!String.IsNullOrEmpty(role))
            //{
            //    cookieRole = new HttpCookie("user_role");
            //    cookieRole.Value = role;
            //    cookieRole.Expires = DateTime.Now.AddMinutes(15);
            //    Response.Cookies.Add(cookieRole);
            //    lstUsers = lstUsers.Where(x => x.Roles.Select(y => y.RoleId).Contains(role));
            //}

            //switch (sortOrder)
            //{
            //    case "lastname_desc":
            //        lstUsers = lstUsers.OrderByDescending(s => s.Lastname).ThenByDescending(a => a.Firstname).ThenByDescending(a => a.Middlename);
            //        break;
            //    case "email_asc":
            //        lstUsers = lstUsers.OrderBy(s => s.Email);
            //        break;
            //    case "email_desc":
            //        lstUsers = lstUsers.OrderByDescending(s => s.Email);
            //        break;
            //    case "group_asc":
            //        lstUsers = lstUsers.OrderBy(s => s.Group.Name);
            //        break;
            //    case "group_desc":
            //        lstUsers = lstUsers.OrderByDescending(s => s.Group.Name);
            //        break;
            //    default:
            //        lstUsers = lstUsers.OrderBy(s => s.Lastname).ThenBy(a => a.Firstname).ThenBy(a => a.Middlename);
            //        break;
            //}

            //int pageSize = 30;
            //int pageNumber = (page ?? 1);

            //return View(lstUsers.ToPagedList(pageNumber, pageSize));
            #endregion
        }

        public ActionResult ClearFilter()
        {
            HttpCookie cookieSortOrder = Request.Cookies.Get("user_sortOrder");
            if (cookieSortOrder != null)
            {
                cookieSortOrder.Value = null;
                cookieSortOrder.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieSortOrder);
            }

            HttpCookie cookieGroup = Request.Cookies.Get("user_group");
            if (cookieGroup != null)
            {
                cookieGroup.Value = null;
                cookieGroup.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieGroup);
            }

            HttpCookie cookieLastname = Request.Cookies.Get("user_lastname");
            if (cookieLastname != null)
            {
                cookieLastname.Value = null;
                cookieLastname.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieLastname);
            }

            HttpCookie cookieEmail = Request.Cookies.Get("user_email");
            if (cookieEmail != null)
            {
                cookieEmail.Value = null;
                cookieEmail.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieEmail);
            }

            HttpCookie cookieRole = Request.Cookies.Get("user_role");
            if (cookieRole != null)
            {
                cookieRole.Value = null;
                cookieRole.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieRole);
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> SyncTeachers()
        {
            Settings currentYear;
            using (var db = new ApplicationDbContext())
            {
                //получение текущего учебного года
                currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
            }
            ViewBag.Log = "";
            //получение списка преподавателей из moodle, для заполнения эл. почты
            var teachersMoodle = new List<TeachersFromMoodle>();
            using (var myConnection = new NpgsqlConnection("Server=46.191.192.102;Port=5432;User Id=postgres;Password=LOR1s2psql;Database=moodledb;"))
            {
                string CommandText = "SELECT mdl_user.id, mdl_user.username, trim(trim(mdl_user.lastname) || ' ' || trim(mdl_user.firstname) || ' ' " +
                    "|| trim(COALESCE(mdl_user.middlename, ' '))) AS FIO, email " +
                    "FROM mdl_user INNER JOIN mdl_cohort_members ON (mdl_user.id = mdl_cohort_members.userid)" +
                    "WHERE mdl_cohort_members.cohortid = 18" +
                    "ORDER BY FIO";
                myConnection.Open(); //Устанавливаем соединение с базой данных.
                var command = new NpgsqlCommand(CommandText, myConnection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var teacher = new TeachersFromMoodle();
                        teacher.Id = Convert.ToInt32(reader["id"]);
                        teacher.UserName = reader["username"].ToString();
                        teacher.FIO = reader["FIO"].ToString();
                        teacher.Email = reader["email"].ToString();
                        teachersMoodle.Add(teacher);
                    }
                }
                myConnection.Close();
            }

            //a. получение списка преподавателей текущего уч. года из деканата
            var lstTeachersDecanat = new List<ApplicationUser>();
            using (var connection = new SqlConnection("user id=mih;password=LOR1s2pq;server=192.168.1.14;database=Деканат;"))
            {
                string sql = "SELECT TOP 100 PERCENT dbo.Преподаватели.Фамилия, dbo.Преподаватели.Имя, dbo.Преподаватели.Отчество, dbo.Нагрузка.КодКафедры, " +
                    "dbo.Преподаватели.Звание, dbo.Преподаватели.Степень FROM dbo.Нагрузка INNER JOIN " +
                      "dbo.Преподаватели ON dbo.Нагрузка.КодПреподавателя = dbo.Преподаватели.Код " +
                    "WHERE(dbo.Нагрузка.УчебныйГод = '" + currentYear.Value + "') " +
                    "GROUP BY dbo.Преподаватели.Фамилия, dbo.Преподаватели.Имя, dbo.Преподаватели.Отчество, dbo.Нагрузка.КодКафедры, " +
                    "dbo.Преподаватели.Звание, dbo.Преподаватели.Степень " +
                    "ORDER BY dbo.Преподаватели.Фамилия, dbo.Преподаватели.Имя";
                var command = new SqlCommand(sql, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    int i = 1;
                    while (reader.Read())
                    {
                        var teacher = new ApplicationUser();
                        teacher.Lastname = reader["Фамилия"].ToString().Trim();
                        teacher.Firstname = reader["Имя"].ToString().Trim();
                        teacher.Middlename = reader["Отчество"].ToString().Trim();
                        if (reader["Отчество"].ToString().Trim().Length > 1)
                            teacher.Sex = (reader["Отчество"].ToString().Trim().Substring(reader["Отчество"].ToString().Trim().Length - 2) == "ич") ? true : false;
                        else
                            teacher.Sex = (reader["Фамилия"].ToString().Trim().Substring(reader["Фамилия"].ToString().Trim().Length - 1) == "а") ? false : true;
                        teacher.Rank = reader["Звание"].ToString().Trim();
                        teacher.Power = reader["Степень"].ToString().Trim();
                        //вносим данные о кафедре и факультете преподавателя
                        int idDep = Convert.ToInt32(reader["КодКафедры"]);
                        var dep = db.Departments.FirstOrDefault(a => a.DecanatID == idDep && a.IsDeleted == false);
                        if (dep != null)
                        {
                            teacher.DepartmentId = dep.Id;
                            teacher.FacultyId = dep.FacultyId;
                        }

                        //почта                        
                        var teach = teachersMoodle.FirstOrDefault(a => a.FIO == teacher.Lastname.Trim() + " " + teacher.Firstname + " " + teacher.Middlename.Trim());
                        if (teach != null)
                        {
                            teacher.Email = (teach.Email != null && teach.Email != "") ? teach.Email : teach.UserName + "@strbsu1.ru";
                            teacher.MoodleId = teach.Id;
                            teacher.UserName = teach.UserName;
                            teacher.EmailConfirmed = (teach.Email != null && teach.Email != "") ? true : false;
                        }
                        else
                        {
                            teacher.UserName = teacher.Email = "t" + i.ToString() + "@strbsu1.ru";
                        }
                        lstTeachersDecanat.Add(teacher);
                        i++;
                    }
                    connection.Close();
                }
            }

            //b. получение списка преподавателей из базы сайта 
            var role = RoleManager.FindByName("Teachers");
            var lstTeachersSite = db.Users.Where(x => x.Roles.Select(y => y.RoleId).Contains(role.Id)).OrderBy(a => a.Lastname)
            .ThenBy(a => a.Firstname).ToList();
            //поиск преподавателей на сайте
            foreach (ApplicationUser teacher in lstTeachersDecanat)
            {
                var findTeacherSite = lstTeachersSite.FirstOrDefault(a => a.Lastname == teacher.Lastname && a.Firstname == teacher.Firstname && a.Middlename == teacher.Middlename);
                if (findTeacherSite != null)
                {
                    //преподаватель найден на сайте
                    //проверяем не удален ли этот препод, если удален восстанавливаем
                    bool editTeacher = false;
                    if (findTeacherSite.DateBlocked != null)
                    {
                        findTeacherSite.DateBlocked = null;
                        editTeacher = true;
                        ViewBag.Log += "<p>Восстановлен преподаватель " + findTeacherSite.Lastname + " " + findTeacherSite.Firstname + " " + findTeacherSite.Middlename + "</p>";                        //await db.SaveChangesAsync();
                    }

                    //проверка и обновление кафедры и факультета преподавателя в случае несоответствия                    
                    if (findTeacherSite.DepartmentId != teacher.DepartmentId) { findTeacherSite.DepartmentId = teacher.DepartmentId; editTeacher = true; }
                    if (findTeacherSite.Rank != teacher.Rank) { findTeacherSite.Rank = teacher.Rank; editTeacher = true; }
                    if (findTeacherSite.Power != teacher.Power) { findTeacherSite.Power = teacher.Power; editTeacher = true; }
                    if (findTeacherSite.FacultyId != teacher.FacultyId) { findTeacherSite.FacultyId = teacher.FacultyId; editTeacher = true; }
                    if (editTeacher)
                    {
                        db.Entry(findTeacherSite).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }

                    //проверяем все ли id-шники дубликатов преподавателя прописаны на сайте
                    //получаем id-шники преподов с сайта
                    var lstIdTeacher = db.TeachersIdDecanat.Where(a => a.SiteId == findTeacherSite.Id).OrderBy(a => a.DecanatId).ToList();

                    //получаем id-шники преподов с деканата                    
                    using (var connection = new SqlConnection("user id=mih;password=LOR1s2pq;server=192.168.1.14;database=Деканат;"))
                    {
                        string CommandText = "SELECT TOP 100 PERCENT dbo.Преподаватели.Код, dbo.Преподаватели.Фамилия, dbo.Преподаватели.Имя, dbo.Преподаватели.Отчество, " +
                        "COUNT(dbo.Нагрузка.Дисциплина) AS КоличествоДисциплин " +
                        "FROM dbo.Нагрузка INNER JOIN " +
                        "dbo.Преподаватели ON dbo.Нагрузка.КодПреподавателя = dbo.Преподаватели.Код " +
                        "WHERE(dbo.Нагрузка.УчебныйГод = '2018-2019') " +
                        "GROUP BY dbo.Преподаватели.Фамилия, dbo.Преподаватели.Имя, dbo.Преподаватели.Отчество, dbo.Преподаватели.Код " +
                        "HAVING (dbo.Преподаватели.Фамилия = '" + teacher.Lastname + "') AND (dbo.Преподаватели.Имя = '" + teacher.Firstname + "') AND (dbo.Преподаватели.Отчество = '" + teacher.Middlename + "') " +
                        "ORDER BY dbo.Преподаватели.Код";
                        connection.Open(); //Устанавливаем соединение с базой данных.
                        var command = new SqlCommand(CommandText, connection);
                        using (var reader = command.ExecuteReader())
                        {
                            int i = 0;
                            while (reader.Read())
                            {
                                //id-шники преподов совпали на сайте и в деканате --> удаляем из проверяемых id-шников
                                int idDec = Convert.ToInt32(reader["Код"]);
                                if (lstIdTeacher[i].DecanatId == idDec)
                                {
                                    lstIdTeacher.RemoveAt(i);
                                }
                                else if (lstIdTeacher[i].DecanatId < idDec)
                                {
                                    //id-шник на сайте меньше чем в деканате 
                                    lstIdTeacher.RemoveAt(i);
                                    db.TeachersIdDecanat.Remove(lstIdTeacher[i]);
                                    await db.SaveChangesAsync();
                                    ViewBag.Log += "<p>Удален идентификатор " + findTeacherSite.Lastname + " " + findTeacherSite.Firstname + " " + findTeacherSite.Middlename + "</p>";
                                }
                                else if (lstIdTeacher[i].DecanatId > idDec)
                                {
                                    db.TeachersIdDecanat.Add(new TeacherIdDecanat { SiteId = lstIdTeacher[i].SiteId, DecanatId = idDec });
                                    ViewBag.Log += "<p>Добавлен идентификатор " + findTeacherSite.Lastname + " " + findTeacherSite.Firstname + " " + findTeacherSite.Middlename + "</p>";
                                    await db.SaveChangesAsync();
                                }
                            }
                        }
                        connection.Close();
                    }
                    //после сравнения всех id-шников препода, удаляем его из списка преподавателей на сайте, чтобы в конце остались преподаватели, которых нужно удалить
                    lstTeachersSite.Remove(findTeacherSite);
                }
                else
                {
                    //преподаватель не найден на сайте
                    //добавляем его на сайт                    
                    string lastname = teacher.Lastname;
                    var result = UserManager.Create(teacher, "LOR1s2pq");
                    if (result.Succeeded)
                    {
                        // добавляем для пользователя роль
                        UserManager.AddToRole(teacher.Id, "Teachers");
                        ViewBag.Log += "<p>Добавлен преподаватель " + teacher.Lastname + " " + teacher.Firstname + " " + teacher.Middlename + "</p>";
                        //добавляем ему id-шники из базы деканата
                        using (var connection = new SqlConnection("user id=mih;password=LOR1s2pq;server=192.168.1.14;database=Деканат;"))
                        {
                            string CommandText = "SELECT TOP 100 PERCENT dbo.Преподаватели.Код, dbo.Преподаватели.Фамилия, dbo.Преподаватели.Имя, dbo.Преподаватели.Отчество, " +
                            "COUNT(dbo.Нагрузка.Дисциплина) AS КоличествоДисциплин " +
                            "FROM dbo.Нагрузка INNER JOIN " +
                            "dbo.Преподаватели ON dbo.Нагрузка.КодПреподавателя = dbo.Преподаватели.Код " +
                            "WHERE(dbo.Нагрузка.УчебныйГод = '2018-2019') " +
                            "GROUP BY dbo.Преподаватели.Фамилия, dbo.Преподаватели.Имя, dbo.Преподаватели.Отчество, dbo.Преподаватели.Код " +
                            "HAVING (dbo.Преподаватели.Фамилия = '" + teacher.Lastname + "') AND (dbo.Преподаватели.Имя = '" + teacher.Firstname + "') AND (dbo.Преподаватели.Отчество = '" + teacher.Middlename + "') " +
                            "ORDER BY dbo.Преподаватели.Код";
                            connection.Open(); //Устанавливаем соединение с базой данных.
                            var command = new SqlCommand(CommandText, connection);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    db.TeachersIdDecanat.Add(new TeacherIdDecanat { SiteId = teacher.Id, DecanatId = Convert.ToInt32(reader["Код"]) });
                                }
                                await db.SaveChangesAsync();
                            }
                            connection.Close();
                        }
                    }
                }
            }
            if (lstTeachersSite.Count > 0)
            {
                foreach (var t in lstTeachersSite)
                {
                    if (t.DateBlocked == null)
                        t.DateBlocked = DateTime.Now;
                    ViewBag.Log += "<p>Удален преподаватель " + t.Lastname + " " + t.Firstname + " " + t.Middlename + "</p>";
                    db.Entry(t).State = EntityState.Modified;
                }
                await db.SaveChangesAsync();
            }
            return View();
        }

        //
        // GET: /Users/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await db.Users.Include(a => a.Faculty).Include(a => a.Department).FirstOrDefaultAsync(a => a.Id == id);

            ViewBag.RoleNames = await UserManager.GetRolesAsync(user.Id);

            return View(user);
        }

        //
        // GET: /Users/Create
        public async Task<ActionResult> Create()
        {
            //Get the list of Roles
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");
            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
            return View();
        }

        //
        // POST: /Users/Create
        [HttpPost]
        public async Task<ActionResult> Create(RegisterViewModel userViewModel, params string[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = userViewModel.UserName,
                    Email = userViewModel.Email,
                    EmailConfirmed = true,
                    Firstname = userViewModel.Firstname,
                    Lastname = userViewModel.Lastname,
                    Middlename = userViewModel.Middlename,
                    FacultyId = userViewModel.FacultyId,
                    DepartmentId = userViewModel.DepartmentId,
                    DecanatId = userViewModel.DecanatId,
                    Employer = userViewModel.Employer,
                };
                var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);

                //Add User to the selected Roles 
                if (adminresult.Succeeded)
                {
                    if (selectedRoles != null)
                    {
                        var result = await UserManager.AddToRolesAsync(user.Id, selectedRoles);
                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError("", result.Errors.First());
                            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
                            return View();
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", adminresult.Errors.First());
                    ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
                    ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");
                    ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");
                    return View();

                }
                return RedirectToAction("Index");
            }
            return View();
        }


        //
        // GET: /Users/Edit/1
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", user.FacultyId);
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", user.DepartmentId);
            var userRoles = await UserManager.GetRolesAsync(user.Id);

            return View(new EditUserViewModel()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Lastname = user.Lastname,
                Firstname = user.Firstname,
                Middlename = user.Middlename,
                FacultyId = user.FacultyId,
                DepartmentId = user.DepartmentId,
                DecanatId = user.DecanatId,
                DateBlocked = user.DateBlocked,
                BlockingReason = user.BlockingReason,
                Employer = user.Employer,
                RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
                {
                    Selected = userRoles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                })
            });
        }

        //
        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Email,Id,UserName,Lastname,Firstname,Middlename,FacultyId,DepartmentId,DecanatId,DateBlocked,BlockingReason,Employer")] EditUserViewModel editUser, string drowssap, params string[] selectedRole)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(editUser.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                user.UserName = editUser.UserName;
                if (user.Email != editUser.Email) user.EmailConfirmed = true;
                if (drowssap != "")
                {
                    UserManager.RemovePassword(user.Id);
                    UserManager.AddPassword(user.Id, drowssap);
                }
                user.Email = editUser.Email;
                user.Lastname = editUser.Lastname;
                user.Firstname = editUser.Firstname;
                user.Middlename = editUser.Middlename;
                user.FacultyId = editUser.FacultyId;
                user.DepartmentId = editUser.DepartmentId;
                user.DecanatId = editUser.DecanatId;
                user.DateBlocked = editUser.DateBlocked;
                user.BlockingReason = editUser.BlockingReason;
                user.Employer = editUser.Employer;

                var userRoles = await UserManager.GetRolesAsync(user.Id);
                selectedRole = selectedRole ?? new string[] { };
                var result = await UserManager.AddToRolesAsync(user.Id, selectedRole.Except(userRoles).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                result = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.Except(selectedRole).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Что-то пошло не так");
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");
            return View();
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

        public ActionResult SendNewPassword(string id)
        {
            var user = db.Users.Include("Roles").FirstOrDefault(a => a.Id == id);
            if (user != null)
            {
                //обновление пароля мудл на сайте личных кабинетов
                user.PassMoodle = Guid.NewGuid().ToString();
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                //определение ролей пользователя
                var userRoles = UserManager.GetRoles(user.Id);


                //обновление пароля и почты (для студента) на самом moodle
                using (var myConnection = new NpgsqlConnection("Server=46.191.192.102;Port=5432;User Id=postgres;Password=LOR1s2psql;Database=moodledb;"))
                {
                    string hash = "";
                    using (MD5 md5Hash = MD5.Create())
                    {
                        hash = GetMd5Hash(md5Hash, user.PassMoodle);
                    }
                    NpgsqlCommand mySQLCommand;
                    string CommText = (userRoles.IndexOf("Students") == -1) ? "UPDATE mdl_user SET password='" + hash + "' WHERE email='" + user.Email + "' AND deleted = 0" :
                        "UPDATE mdl_user SET password='" + hash + "', email='" + user.Email + "' WHERE username='" + user.UserName + "' AND deleted = 0";
                    myConnection.Open();
                    mySQLCommand = new NpgsqlCommand(CommText, myConnection);
                    int c = mySQLCommand.ExecuteNonQuery();
                    myConnection.Close();
                }

                string newPassword = GeneratePassword();
                string code = UserManager.GeneratePasswordResetToken(user.Id);
                var result = UserManager.ResetPassword(user.Id, code, newPassword);
                if (result.Succeeded)
                {
                    dynamic email = new Postal.Email("SendEmailPassword");
                    email.From = "sdo.system@strbsu.ru";
                    email.To = user.Email;
                    email.Subject = "Логин и пароль от личного кабинета СФ БашГУ";
                    email.NewPassword = newPassword;
                    email.UserName = user.Email;
                    email.Send();
                    ViewBag.Result = "Пароль обновлен и отправлен на почту.";
                }
                else ViewBag.Result = "Ошибка обновления пароля!!!";
            }
            return View();
        }

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


        //
        // GET: /Users/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                //удаление данных о попытках входа на сайт
                var logs = db.LogLogins.Where(a => a.UserId == user.Id).ToList();
                var res0 = db.LogLogins.RemoveRange(logs);
                //удаление из таблицы связей преподавателя и идентификаторов деканата
                var teacherRows = db.TeachersIdDecanat.Where(a => a.SiteId == user.Id).ToList();
                var res = db.TeachersIdDecanat.RemoveRange(teacherRows);
                //удаление из таблиц об образовании
                var advEduRows = db.AdditionalEducations.Where(a => a.AdditionalEducationUserId == user.Id).ToList();
                var res1 = db.AdditionalEducations.RemoveRange(advEduRows);
                var basEduRows = db.BasicEducations.Where(a => a.BasicEducationUserId == user.Id).ToList();
                var res2 = db.BasicEducations.RemoveRange(basEduRows);
                var acadEduRows = db.AcademicDegrees.Where(a => a.AcademicDegreeUserId == user.Id).ToList();
                var res3 = db.AcademicDegrees.RemoveRange(acadEduRows);
                db.SaveChanges();

                //удаление пользователя
                var result = await UserManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
