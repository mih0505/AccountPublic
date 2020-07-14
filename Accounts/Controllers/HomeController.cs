using Accounts.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Accounts.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public async Task<ActionResult> Index()
        {
            ApplicationUser user;
            Group group;
            Profile profile;
            using (var db = new ApplicationDbContext())
            {
                user = await db.Users.FirstOrDefaultAsync(a => a.UserName == User.Identity.Name);

                if (user.Question == null)
                {
                    return RedirectToAction("SecurityQuestion", "Account");
                }

                ViewBag.CountMessages = await CountUnreadMassages();
                
                if (User.IsInRole("Students"))
                {
                    ViewBag.Group = group = db.Groups.FirstOrDefault(a => a.Id == user.GroupId);
                    ViewBag.Faculty = db.Faculties.FirstOrDefault(a => a.Id == user.FacultyId);
                    ViewBag.Profile = profile = db.Profiles.FirstOrDefault(a => a.Id == group.ProfileId);
                    ViewBag.GIA = await db.DiplomWorks.FirstOrDefaultAsync(a => a.StudentId == user.Id);
                }
                if (User.IsInRole("Teachers"))
                {
                    ViewBag.Faculty = db.Faculties.FirstOrDefault(a => a.Id == user.FacultyId);
                    if (user.DepartmentId != null)
                        ViewBag.Department = db.Departments.FirstOrDefault(a => a.Id == user.DepartmentId);
                    else
                        ViewBag.Department = null;
                    ViewBag.CountGroupsTutors = db.Tutors.Where(a => a.UserId == user.Id).Count();
                    ViewBag.CountProjects = db.CourseWorkStudents.Where(a => a.TeacherId == user.Id).Count();
                    var date = DateTime.Now.Date;
                    var statsTeacher = db.StatementStudents.Where(a => a.TeacherStatementId == user.Id).Select(a => a.StatementId).Distinct().ToList();
                    ViewBag.CountStatements = db.Statements.Where(a => (a.TeacherDisciplineId == user.Id || a.TeacherDiscipline2Id == user.Id || a.TeacherDiscipline3Id == user.Id ||
                    a.TeacherDiscipline4Id == user.Id || a.TeacherDiscipline5Id == user.Id || a.TeacherDiscipline6Id == user.Id || a.TeacherDiscipline7Id == user.Id ||
                    statsTeacher.Contains(a.Id)) && a.DateEnd > date).Count();
                }
            }

            ViewBag.User = user;
            return View();
        }

        private async Task<int> CountUnreadMassages()
        {
            var db = new ApplicationDbContext();
            var userId = User.Identity.GetUserId();
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userRoles = userManager.GetRoles(userId).ToList();


            var userMessages = await (from Message in db.Messages
                                     join Role in db.Roles on Message.RoleId equals Role.Id
                                     join UserRole in userRoles on Role.Name equals UserRole
                                     select new 
                                     {
                                         MessageId = Message.Id                                         
                                     }).ToListAsync();

            var userReadedMessages = await (from ReadMessage in db.ReadMessages
                                           join Message in db.Messages on ReadMessage.MessageId equals Message.Id
                                           join Role in db.Roles on Message.RoleId equals Role.Id
                                           where ReadMessage.UserId == userId
                                           select new 
                                           {
                                               MessageId = ReadMessage.MessageId                                               
                                           }).ToListAsync();
            var exceptMessages = userMessages.Except(userReadedMessages).ToList();

            return exceptMessages.Count;
        }


        [Authorize]
        public async Task<ActionResult> UnreadMessages()
        {
            var db = new ApplicationDbContext();

            var userId = User.Identity.GetUserId();
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userRoles = userManager.GetRoles(userId).ToList();


            var userMessages = await (from Message in db.Messages
                                      join Role in db.Roles on Message.RoleId equals Role.Id
                                      join UserRole in userRoles on Role.Name equals UserRole
                                      select new MessageViewModel()
                                      {
                                          MessageId = Message.Id,
                                          Title = Message.Title,
                                          Content = Message.Content,
                                          Date = Message.Date,
                                          RoleId = Role.Id,
                                          RoleName = Role.Name
                                      }).ToListAsync();

            var userReadedMessages = await (from ReadMessage in db.ReadMessages
                                            join Message in db.Messages on ReadMessage.MessageId equals Message.Id
                                            join Role in db.Roles on Message.RoleId equals Role.Id
                                            where ReadMessage.UserId == userId
                                            select new MessageViewModel()
                                            {
                                                MessageId = ReadMessage.MessageId,
                                                Title = Message.Title,
                                                Content = Message.Content,
                                                Date = Message.Date,
                                                RoleId = Role.Id,
                                                RoleName = Role.Name
                                            }).ToListAsync();

            var exceptMessages = (from Message in userMessages
                                  from ReadMessage in userReadedMessages
                                  where Message.MessageId == ReadMessage.MessageId
                                  select Message).ToList();

            foreach (var message in exceptMessages)
            {
                userMessages.Remove(message);
            }
                        
            return PartialView(userMessages);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> UnreadMessages(List<MessageViewModel> models)
        {
            var db = new ApplicationDbContext();

            var userId = User.Identity.GetUserId();

            foreach (var message in models)
            {
                db.ReadMessages.Add(new ReadMessage()
                {
                    MessageId = message.MessageId,
                    UserId = userId
                });
            }

            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public ActionResult Educations()
        {
            return View();
        }

        public ActionResult Questionary()
        {
            return View();
        }

        public ActionResult Plan(int? id, string sortOrder, string name, int? course, int? session, string controls)
        {
            ApplicationUser user;
            Group group;
            int idPlanDecanat;
            Settings currentYear;
            var plan = new List<Plan>();

            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "name_asc";
            ViewBag.CourseSortParm = String.IsNullOrEmpty(sortOrder) ? "course_desc" : "";
            ViewBag.SessionSortParm = String.IsNullOrEmpty(sortOrder) ? "session_desc" : "session_asc";
            ViewBag.ControlsSortParm = String.IsNullOrEmpty(sortOrder) ? "controls_desc" : "controls_asc";

            ViewBag.NameFilter = name;
            ViewBag.CourseFilter = course;
            ViewBag.SessionFilter = session;
            ViewBag.ControlsFilter = controls;

            using (var db = new ApplicationDbContext())
            {
                if (id != null)
                    idPlanDecanat = (int)id;
                else
                {
                    user = db.Users.FirstOrDefault(a => a.UserName == User.Identity.Name);
                    if (user.idPlanDecanat == null)
                    {
                        group = db.Groups.FirstOrDefault(a => a.Id == user.GroupId);
                        idPlanDecanat = group.idPlanDecanat ?? 0;
                    }
                    else idPlanDecanat = user.idPlanDecanat ?? 0;
                }
                currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
            }

            if (idPlanDecanat != 0)
            {
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DecanatConnection"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = "SELECT dbo.ПланыСтроки.ДисциплинаКод, dbo.ПланыСтроки.Дисциплина, dbo.ПланыЧасы.Курс, dbo.ПланыЧасы.Семестр, " +
                      "dbo.ПланыСтроки.ПодлежитИзучениюЧасов, dbo.ПланыСтроки.ЧасовСамостоятельнаяРабота, dbo.ПланыЧасы.Экзамен, dbo.ПланыЧасы.Зачет, " +
                      "dbo.ПланыЧасы.Лекций, dbo.ПланыЧасы.Практик, dbo.ПланыЧасы.Лабораторных, dbo.ПланыЧасы.КСР, dbo.ПланыСтроки.Компетенции " +
                      "FROM dbo.Планы INNER JOIN " +
                      "dbo.Специальности ON dbo.Планы.КодСпециальности = dbo.Специальности.Код INNER JOIN " +
                      "dbo.ПланыСтроки ON dbo.Планы.Код = dbo.ПланыСтроки.КодПлана INNER JOIN " +
                      "dbo.ПланыЧасы ON dbo.ПланыСтроки.Код = dbo.ПланыЧасы.КодСтроки " +
                      "WHERE (dbo.Планы.УчебныйГод = '" + currentYear.Value + "') AND(dbo.Планы.Код = " + idPlanDecanat + ") " +
                      "ORDER BY dbo.ПланыЧасы.Курс, dbo.ПланыСтроки.ДисциплинаКод, dbo.ПланыЧасы.Семестр";
                    var command = new SqlCommand(sql, connection);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var discipline = new Plan();

                            var contr = "";
                            if (Convert.ToBoolean(reader["Экзамен"]))
                                contr = "Экз.";
                            else if (Convert.ToBoolean(reader["Зачет"]))
                                contr = "Зач.";
                            else contr = "-";

                            discipline.Block = reader["ДисциплинаКод"].ToString();
                            discipline.Name = reader["Дисциплина"].ToString();
                            discipline.AllHours = Convert.ToInt32(reader["ПодлежитИзучениюЧасов"]);
                            discipline.IndependentWork = Convert.ToInt32(reader["ЧасовСамостоятельнаяРабота"]);
                            discipline.Course = Convert.ToInt32(reader["Курс"]);
                            discipline.Session = Convert.ToInt32(reader["Семестр"]);
                            discipline.Controls = contr;
                            discipline.Lecture = (reader["Лекций"] is DBNull) ? 0 : Convert.ToInt32(reader["Лекций"]);
                            discipline.PracticalWork = (reader["Практик"] is DBNull) ? 0 : Convert.ToInt32(reader["Практик"]);
                            discipline.LaboratoryWork = (reader["Лабораторных"] is DBNull) ? 0 : Convert.ToInt32(reader["Лабораторных"]);
                            //discipline.CIW = Convert.ToInt32(reader["КСР"]);
                            discipline.Competences = reader["Компетенции"].ToString();
                            plan.Add(discipline);
                        }
                        connection.Close();
                    }
                }
            }

            if (!String.IsNullOrEmpty(name))
            {
                plan = plan.Where(p => p.Name.ToLower().Contains(name.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(controls))
            {
                plan = plan.Where(p => p.Controls == controls).ToList();
            }

            if (course != null)
            {
                plan = plan.Where(p => p.Course == course).ToList();
            }

            if (session != null)
            {
                plan = plan.Where(p => p.Session == session).ToList();
            }

            switch (sortOrder)
            {
                case "course_desc":
                    plan = plan.OrderByDescending(s => s.Course).ThenByDescending(a => a.Session).ToList();
                    break;
                case "name_asc":
                    plan = plan.OrderBy(s => s.Name).ThenBy(a => a.Session).ToList();
                    break;
                case "name_desc":
                    plan = plan.OrderByDescending(s => s.Name).ThenByDescending(a => a.Session).ToList();
                    break;
                case "session_asc":
                    plan = plan.OrderBy(s => s.Session).ToList();
                    break;
                case "session_desc":
                    plan = plan.OrderByDescending(s => s.Session).ToList();
                    break;
                case "controls_asc":
                    plan = plan.OrderBy(s => s.Controls).ThenBy(a => a.Session).ToList();
                    break;
                case "controls_desc":
                    plan = plan.OrderByDescending(s => s.Controls).ThenByDescending(a => a.Session).ToList();
                    break;
                default:
                    plan = plan.OrderBy(s => s.Course).ThenBy(a => a.Session).ToList();
                    break;
            }

            ViewBag.CoursesList = new SelectList(plan.OrderBy(a => a.Course).Select(m => m.Course).Distinct(), "Course");
            ViewBag.SessionList = new SelectList(plan.OrderBy(a => a.Session).Select(m => m.Session).Distinct(), "Session");
            ViewBag.ControlsList = new SelectList(plan.OrderBy(a => a.Controls).Select(m => m.Controls).Distinct(), "Controls");

            return View(plan.ToList());
        }

        public ActionResult DisplayingImage(string id)
        {
            ApplicationUser user;
            string file_type;
            using (var db = new ApplicationDbContext())
            {
                if (id == null)
                    user = db.Users.FirstOrDefault(a => a.UserName == User.Identity.Name);
                else
                    user = db.Users.FirstOrDefault(a => a.Id == id);
                file_type = "application/octet-stream";
            }
            return File(user.Image, file_type);
        }

        public ActionResult Support()
        {
            return View();
        }

        public ActionResult Libraries()
        {
            return View();
        }

        public ActionResult Rules()
        {
            return View();
        }

        public ActionResult Sync()
        {
            return View();
        }

        public ActionResult DocTemplates()
        {
            return View();
        }
    }
}