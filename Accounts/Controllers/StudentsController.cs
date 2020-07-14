using Accounts.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNet.Identity;
using System.IO;

namespace Accounts.Controllers
{
    public class StudentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Students

        [Authorize(Roles = "Administrators,Teachers,FacultiesManagers")]
        public ActionResult ListGroups(string sortOrder, int? faculty, string name, int? formOfTraining, int? page)
        {
            // В ListGroups.cshtml увеличил ширину DropDownList'ов для лучшего отображения содержимого

            // Получение текущего года
            var year = db.Settings.FirstOrDefault(Setting => (Setting.Name == "CurrentYear"));

            // Создание первого (стандартного) элемента (чтобы избежать бага с фильтрацией данных)
            var defaultItem = new SelectListItem()
            {
                Value = "-1",
                Text = "Все факультеты"
            };

            // Заполнение DropDownList'a c факультетами
            var facultiesList = db.Faculties.Where(Faculty => (!Faculty.IsDeleted)).ToList();
            var facultiesView = new List<SelectListItem>()
            {
                defaultItem
            };

            foreach (Faculty item in facultiesList)
            {
                facultiesView.Add(new SelectListItem()
                {
                    Value = item.Id.ToString(),
                    Text = item.Name
                });
            }

            // Заполнение DropDownList'a c формами обучения
            var formsOfTrainingList = db.FormOfTrainings.ToList();
            var formsOfTrainingView = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Value = "-1",
                    Text = "Форма обуч."
                }
            };

            foreach (FormOfTraining item in formsOfTrainingList)
            {
                formsOfTrainingView.Add(new SelectListItem()
                {
                    Value = item.Id.ToString(),
                    Text = item.Name
                });
            }

            // Получение списка всех групп
            IQueryable<Group> groups = db.Groups.Where(a => a.IsDeleted == false && a.AcademicYear == year.Value)
                .Include(Group => Group.Faculty)
                .Include(Group => Group.FormOfTraining)
                .Include(Group => Group.Profile)
                .OrderBy(Group => Group.Name);

            // Получение Cookies
            var cookieSortOrder = Request.Cookies.Get("group_sortOrder");
            var cookieFaculty = Request.Cookies.Get("group_faculty");
            var cookieName = Request.Cookies.Get("group_name");
            var cookieFormOfTraining = Request.Cookies.Get("group_formOfTraining");

            // Применение значений из Cookies
            // В том числе обновление значений в DropDownList согласно полученным Cookies
            if (sortOrder == null && cookieSortOrder != null)
            {
                sortOrder = cookieSortOrder.Value;
            }

            if (faculty == null && cookieFaculty != null)
            {
                facultiesView.FirstOrDefault(Faculty => (Faculty.Value == cookieFaculty.Value)).Selected = true;
                faculty = Convert.ToInt32(cookieFaculty.Value);
            }

            if (name == null && cookieName != null)
            {
                name = cookieName.Value;
            }

            if (formOfTraining == null && cookieFormOfTraining != null)
            {
                formsOfTrainingView.FirstOrDefault(Form => (Form.Value == cookieFormOfTraining.Value)).Selected = true;
                formOfTraining = Convert.ToInt32(cookieFormOfTraining.Value);
            }

            // Удаление Cookies
            Response.Cookies.Add(new HttpCookie("group_sortOrder")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("group_faculty")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("group_name")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("group_formOfTraining")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            // Применение фильтров
            // В том числе вывод нужной информации для определенной роли
            var user = db.Users.FirstOrDefault(ApplicationUser => (ApplicationUser.UserName == User.Identity.Name));
            if (User.IsInRole("FacultiesManagers"))
            {
                groups = groups.Where(Group => (Group.FacultyId == user.FacultyId));
            }
            else if (User.IsInRole("Teachers"))
            {
                var tutors = db.Tutors.Where(Tutor => (Tutor.UserId == user.Id)).Select(Tutor => Tutor.GroupId).ToList();
                groups = groups.Where(Group => tutors.Contains(Group.Id));
            }

            // Применение фильтров
            // В том числе добавление новых Cookies
            if (sortOrder != null)
            {
                Response.Cookies.Add(new HttpCookie("group_sortOrder")
                {
                    Value = sortOrder.ToString(),
                    Expires = DateTime.Now.AddMinutes(15)
                });
            }

            if (faculty != null)
            {
                Response.Cookies.Add(new HttpCookie("group_faculty")
                {
                    Value = faculty.ToString(),
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (faculty.Value != -1)
                {
                    groups = groups.Where(Group => (Group.FacultyId == faculty));
                }
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                Response.Cookies.Add(new HttpCookie("group_name")
                {
                    Value = name,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                groups = groups.Where(Group => (SqlFunctions.PatIndex("%" + name + "%", Group.Name) > 0));
            }

            if (formOfTraining != null)
            {
                Response.Cookies.Add(new HttpCookie("group_formOfTraining")
                {
                    Value = formOfTraining.ToString(),
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (formOfTraining.Value != -1)
                {
                    groups = groups.Where(Group => (Group.FormOfTrainingId == formOfTraining));
                }
            }

            // Применение сортировки
            switch (sortOrder)
            {
                case "name_desc":
                    groups = groups.OrderByDescending(Group => Group.Name);
                    break;
                case "faculty_asc":
                    groups = groups.OrderBy(Group => Group.Faculty.Name);
                    break;
                case "faculty_desc":
                    groups = groups.OrderByDescending(Group => Group.Faculty.Name);
                    break;
                case "form_asc":
                    groups = groups.OrderBy(Group => Group.FormOfTraining.Name);
                    break;
                case "form_desc":
                    groups = groups.OrderByDescending(Group => Group.FormOfTraining.Name);
                    break;
                case "course_asc":
                    groups = groups.OrderBy(Group => Group.Course);
                    break;
                case "course_desc":
                    groups = groups.OrderByDescending(Group => Group.Course);
                    break;
                default:
                    groups = groups.OrderBy(Group => Group.Name);
                    break;
            }

            // Сохранение данных во ViewBag
            ViewBag.FacultyList = facultiesView;
            ViewBag.FormOfTrainingList = formsOfTrainingView;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = (string.IsNullOrWhiteSpace(sortOrder)) ? "name_desc" : null;
            ViewBag.FacultySortParm = (sortOrder == "faculty_asc") ? "faculty_desc" : "faculty_asc";
            ViewBag.FormSortParm = (sortOrder == "form_asc") ? "form_desc" : "form_asc";
            ViewBag.CourseSortParm = (sortOrder == "course_asc") ? "course_desc" : "course_asc";
            ViewBag.FacultyFilter = faculty;
            ViewBag.NameFilter = name;
            ViewBag.FormFilter = formOfTraining;

            // Вывод результата
            return View(groups.ToPagedList(page ?? 1, 30));

            #region Старый код
            //получение текущего учебного года
            //var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");

            //var cookieName = Request.Cookies.Get("group_name");
            //var cookieFormOfTraining = Request.Cookies.Get("group_formOfTraining");
            //var cookieSortOrder = Request.Cookies.Get("group_sortOrder");
            //var cookieFaculty = Request.Cookies.Get("group_faculty");

            //if (cookieName != null)
            //{
            //    if (name != null && name != cookieName.Value)
            //        cookieName.Value = name;
            //    else
            //        name = cookieName.Value;
            //}

            //if (cookieSortOrder != null)
            //{
            //    sortOrder = cookieSortOrder.Value;
            //}

            //if (cookieFaculty != null)
            //{
            //    if (faculty != null && faculty != Convert.ToInt32(cookieFaculty.Value))
            //        cookieFaculty.Value = faculty.ToString();
            //    else
            //        faculty = Convert.ToInt32(cookieFaculty.Value);
            //}

            //if (cookieFormOfTraining != null)
            //{
            //    if (formOfTraining != null && formOfTraining != Convert.ToInt32(cookieFormOfTraining.Value))
            //        cookieFormOfTraining.Value = formOfTraining.ToString();
            //    else
            //        formOfTraining = Convert.ToInt32(cookieFormOfTraining.Value);
            //}

            //ViewBag.CurrentSort = sortOrder;
            //ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            //ViewBag.FacultySortParm = sortOrder == "faculty_asc" ? "faculty_desc" : "faculty_asc";
            //ViewBag.FormSortParm = sortOrder == "form_asc" ? "form_desc" : "form_asc";
            //ViewBag.CourseSortParm = sortOrder == "course_asc" ? "course_desc" : "course_asc";

            //ViewBag.FacultyFilter = faculty;
            //ViewBag.NameFilter = name;
            //ViewBag.FormFilter = formOfTraining;

            ////фильтр групп
            //IQueryable<Group> groups = db.Groups.Where(a => a.IsDeleted == false && a.AcademicYear == currentYear.Value).Include(g => g.Faculty).Include(g => g.FormOfTraining)
            //    .Include(g => g.Profile).OrderBy(c => c.Name);

            //var user = db.Users.FirstOrDefault(a => a.UserName == User.Identity.Name);
            //if (User.IsInRole("FacultiesManagers"))
            //{
            //    groups = groups.Where(a => a.FacultyId == user.FacultyId);
            //}
            //else if (User.IsInRole("Teachers"))
            //{
            //    var lstGroupsOfTutor = db.Tutors.Where(a => a.UserId == user.Id).Select(a => a.GroupId).ToList();
            //    groups = groups.Where(a => lstGroupsOfTutor.Contains(a.Id));
            //}

            //if (faculty != null || (faculty != null && faculty.ToString() != cookieFaculty.Value))
            //{
            //    cookieFaculty = new HttpCookie("group_faculty");
            //    cookieFaculty.Value = faculty.ToString();
            //    cookieFaculty.Expires = DateTime.Now.AddMinutes(15);
            //    Response.Cookies.Add(cookieFaculty);
            //    groups = groups.Where(p => p.FacultyId == faculty);
            //}
            //if (!String.IsNullOrEmpty(name) || (!String.IsNullOrEmpty(name) && name != cookieName.Value))
            //{
            //    cookieName = new HttpCookie("group_name");
            //    cookieName.Value = name;
            //    cookieName.Expires = DateTime.Now.AddMinutes(15);
            //    Response.Cookies.Add(cookieName);
            //    groups = groups.Where(p => SqlFunctions.PatIndex("%" + name + "%", p.Name) > 0);
            //}

            //if (formOfTraining != null || (formOfTraining != null && formOfTraining.ToString() != cookieFormOfTraining.Value))
            //{
            //    cookieFormOfTraining = new HttpCookie("group_formOfTraining");
            //    cookieFormOfTraining.Value = faculty.ToString();
            //    cookieFormOfTraining.Expires = DateTime.Now.AddMinutes(15);
            //    Response.Cookies.Add(cookieFaculty);
            //    groups = groups.Where(p => p.FormOfTrainingId == formOfTraining);
            //}

            //switch (sortOrder)
            //{
            //    case "name_desc":
            //        groups = groups.OrderByDescending(s => s.Name);
            //        break;
            //    case "faculty_asc":
            //        groups = groups.OrderBy(s => s.Faculty.Name);
            //        break;
            //    case "faculty_desc":
            //        groups = groups.OrderByDescending(s => s.Faculty.Name);
            //        break;
            //    case "form_asc":
            //        groups = groups.OrderBy(s => s.FormOfTraining.Name);
            //        break;
            //    case "form_desc":
            //        groups = groups.OrderByDescending(s => s.FormOfTraining.Name);
            //        break;
            //    case "course_asc":
            //        groups = groups.OrderBy(s => s.Course);
            //        break;
            //    case "course_desc":
            //        groups = groups.OrderByDescending(s => s.Course);
            //        break;
            //    default:
            //        groups = groups.OrderBy(s => s.Name);
            //        break;
            //}

            //ViewBag.FacultyList = new SelectList(db.Faculties.Where(a => a.IsDeleted == false), "Id", "Name");
            //ViewBag.FormOfTraining = new SelectList(db.FormOfTrainings, "Id", "Name");

            //int pageSize = 30;
            //int pageNumber = (page ?? 1);

            //return View(groups.ToPagedList(pageNumber, pageSize));
            #endregion
        }

        public ActionResult ClearFilter()
        {
            HttpCookie cookieSortOrder = Request.Cookies.Get("group_sortOrder");
            if (cookieSortOrder != null)
            {
                cookieSortOrder.Value = null;
                cookieSortOrder.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieSortOrder);
            }

            HttpCookie cookieName = Request.Cookies.Get("group_name");
            if (cookieName != null)
            {
                cookieName.Value = null;
                cookieName.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieName);
            }

            HttpCookie cookieFaculty = Request.Cookies.Get("group_faculty");
            if (cookieFaculty != null)
            {
                cookieFaculty.Value = null;
                cookieFaculty.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieFaculty);
            }

            HttpCookie cookieAcademicYear = Request.Cookies.Get("group_academicYear");
            if (cookieAcademicYear != null)
            {
                cookieAcademicYear.Value = null;
                cookieAcademicYear.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieAcademicYear);
            }

            HttpCookie cookieFormOfTraining = Request.Cookies.Get("group_formOfTraining");
            if (cookieFormOfTraining != null)
            {
                cookieFormOfTraining.Value = null;
                cookieFormOfTraining.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieFormOfTraining);
            }

            return RedirectToAction("ListGroups");
        }

        public async Task<ActionResult> StudentsGroup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var group = db.Groups.Find(id);
            ViewBag.CurrentGroup = group;
            var listStudents = await db.Users
                .Where(a => a.GroupId == id && a.DateBlocked == null)
                .OrderBy(a => a.Lastname)
                .ThenBy(a => a.Firstname)
                .ThenBy(a => a.Middlename)
                .ToListAsync();
            ViewBag.Count = listStudents.Count;

            return View(listStudents);
        }

        public async Task<ActionResult> Details(string id)
        {
            ApplicationUser user;
            Group group;
            Profile profile;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //проверяем доступен ли студент для открывающего пользователя
            List<int> lstGroups = new List<int>();
            string userWorkerId = User.Identity.GetUserId();
            if (User.IsInRole("Teachers"))
            {
                var lstGroupsOfTutor = db.Tutors
                    .Where(a => a.UserId == userWorkerId)
                    .Select(a => a.GroupId)
                    .ToList();
                
                lstGroups = db.Groups
                    .Where(a => lstGroupsOfTutor.Contains(a.Id))
                    .Select(a => a.Id)
                    .ToList();
            }
            else if (User.IsInRole("FacultiesManagers"))
            {
                var userWorker = db.Users.FirstOrDefault(a => a.Id == userWorkerId);
                if (userWorker != null)
                {
                    lstGroups = db.Groups
                        .Where(a => a.FacultyId == userWorker.FacultyId)
                        .Select(a => a.Id)
                        .ToList();
                }
            }

            if (!User.IsInRole("Administrators"))
                user = db.Users.FirstOrDefault(a => lstGroups.Contains((int)a.GroupId) && a.Id == id);
            else
                user = db.Users.FirstOrDefault(a => a.Id == id);

            DetailsStudent student = new DetailsStudent();
            //получение данных студента
            if (user != null)
            {
                student.User = user;
                student.Group = group = db.Groups.FirstOrDefault(a => a.Id == user.GroupId);
                student.Faculty = db.Faculties.FirstOrDefault(a => a.Id == user.FacultyId);
                student.Profile = profile = db.Profiles.FirstOrDefault(a => a.Id == group.ProfileId);
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //получение данных об успеваемости
            var lstStatements = db.StatementStudents
                .Where(a => a.StudentStatementId == user.Id)
                .Select(a => a.StatementId)
                .Distinct()
                .ToList();

            if (lstStatements.Count > 0)
            {
                int lastSession = db.StatementStudents
                    .Include(a => a.Statement)
                    .Where(a => a.StudentStatementId == user.Id)
                    .Max(a => a.Statement.Semester);

                ViewBag.CurrentSemester = lastSession;

                student.Grades = (from statStud in db.StatementStudents
                                  join stat in db.Statements on statStud.StatementId equals stat.Id
                                  join students in db.Users on statStud.StudentStatementId equals students.Id
                                  join teachers in db.Users on statStud.TeacherStatementId equals teachers.Id into outerTeacher
                                  from teachers in outerTeacher.DefaultIfEmpty()
                                  join teachers2 in db.Users on stat.TeacherDiscipline2Id equals teachers2.Id into outerTeacher2
                                  from teachers2 in outerTeacher2.DefaultIfEmpty()
                                  join teachers3 in db.Users on stat.TeacherDiscipline3Id equals teachers3.Id into outerTeacher3
                                  from teachers3 in outerTeacher3.DefaultIfEmpty()
                                  join teachers4 in db.Users on stat.TeacherDiscipline4Id equals teachers4.Id into outerTeacher4
                                  from teachers4 in outerTeacher4.DefaultIfEmpty()
                                  join teachers5 in db.Users on stat.TeacherDiscipline5Id equals teachers5.Id into outerTeacher5
                                  from teachers5 in outerTeacher5.DefaultIfEmpty()
                                  join teachers6 in db.Users on stat.TeacherDiscipline6Id equals teachers6.Id into outerTeacher6
                                  from teachers6 in outerTeacher6.DefaultIfEmpty()
                                  join teachers7 in db.Users on stat.TeacherDiscipline7Id equals teachers7.Id into outerTeacher7
                                  from teachers7 in outerTeacher7.DefaultIfEmpty()
                                  join teacherStatement in db.Users on stat.TeacherDisciplineId equals teacherStatement.Id into outerTeacherStatement
                                  from teacherStatement in outerTeacherStatement.DefaultIfEmpty()
                                  where statStud.StudentStatementId == user.Id && stat.Semester == lastSession
                                  orderby stat.TypeControl, stat.Number, statStud.Date
                                  select (new StatementViewModel
                                  {
                                      StatementId = stat.Id,
                                      NameDiscipline = stat.NameDiscipline,
                                      TypeControl = stat.TypeControl,
                                      Hours = stat.ZET +"/"+ stat.Hours,
                                      Teacher = teachers.Lastname + " " + teachers.Firstname.Substring(0, 1) + "." + teachers.Middlename.Substring(0, 1) + ". ",
                                      Teacher2 = teachers2 != null ? teachers2.Lastname + " " + teachers2.Firstname.Substring(0, 1) + "." + teachers2.Middlename.Substring(0, 1) + "." : "",
                                      Teacher3 = teachers3 != null ? teachers3.Lastname + " " + teachers3.Firstname.Substring(0, 1) + "." + teachers3.Middlename.Substring(0, 1) + "." : "",
                                      Teacher4 = teachers4 != null ? teachers4.Lastname + " " + teachers4.Firstname.Substring(0, 1) + "." + teachers4.Middlename.Substring(0, 1) + "." : "",
                                      Teacher5 = teachers5 != null ? teachers5.Lastname + " " + teachers5.Firstname.Substring(0, 1) + "." + teachers5.Middlename.Substring(0, 1) + "." : "",
                                      Teacher6 = teachers6 != null ? teachers6.Lastname + " " + teachers6.Firstname.Substring(0, 1) + "." + teachers6.Middlename.Substring(0, 1) + "." : "",
                                      Teacher7 = teachers7 != null ? teachers7.Lastname + " " + teachers7.Firstname.Substring(0, 1) + "." + teachers7.Middlename.Substring(0, 1) + "." : "",
                                      TeacherStatement = teacherStatement != null ? teacherStatement.Lastname + " " + teacherStatement.Firstname.Substring(0, 1) + "." + teacherStatement.Middlename.Substring(0, 1) + "." : "",
                                      Course = stat.Course,
                                      Semester = stat.Semester,
                                      Date = statStud.Date,
                                      Grade = statStud.Grade,
                                      ParentId = stat.ParentId
                                  })).ToList();
            }

            //получение данных о задолженностях
            var lstArrears = (from statStud in db.StatementStudents
                              join stat in db.Statements on statStud.StatementId equals stat.Id
                              join students in db.Users on statStud.StudentStatementId equals students.Id
                              join teachers in db.Users on statStud.TeacherStatementId equals teachers.Id into outerTeacher
                              from teachers in outerTeacher.DefaultIfEmpty()
                              join teachers2 in db.Users on stat.TeacherDiscipline2Id equals teachers2.Id into outerTeacher2
                              from teachers2 in outerTeacher2.DefaultIfEmpty()
                              join teachers3 in db.Users on stat.TeacherDiscipline3Id equals teachers3.Id into outerTeacher3
                              from teachers3 in outerTeacher3.DefaultIfEmpty()
                              join teachers4 in db.Users on stat.TeacherDiscipline4Id equals teachers4.Id into outerTeacher4
                              from teachers4 in outerTeacher4.DefaultIfEmpty()
                              join teachers5 in db.Users on stat.TeacherDiscipline5Id equals teachers5.Id into outerTeacher5
                              from teachers5 in outerTeacher5.DefaultIfEmpty()
                              join teachers6 in db.Users on stat.TeacherDiscipline6Id equals teachers6.Id into outerTeacher6
                              from teachers6 in outerTeacher6.DefaultIfEmpty()
                              join teachers7 in db.Users on stat.TeacherDiscipline7Id equals teachers7.Id into outerTeacher7
                              from teachers7 in outerTeacher7.DefaultIfEmpty()
                              join teacherStatement in db.Users on stat.TeacherDisciplineId equals teacherStatement.Id into outerTeacherStatement
                              from teacherStatement in outerTeacherStatement.DefaultIfEmpty()
                              where statStud.StudentStatementId == user.Id && stat.GroupId == user.GroupId
                              && ((statStud.Grade == null || statStud.Grade == "Не явился" || statStud.Grade == "Не удовлетворительно" || statStud.Grade == "Не зачтено")
                              || stat.ParentId != null)
                              orderby stat.Semester
                              select (new StatementViewModel
                              {
                                  ParentId = stat.ParentId == null ? stat.Id : stat.ParentId,
                                  NameDiscipline = stat.NameDiscipline,
                                  TypeControl = stat.TypeControl,
                                  Hours = stat.ZET + "/" + stat.Hours,
                                  Teacher = teachers.Lastname + " " + teachers.Firstname.Substring(0, 1) + "." + teachers.Middlename.Substring(0, 1) + ". ",
                                  Teacher2 = teachers2 != null ? teachers2.Lastname + " " + teachers2.Firstname.Substring(0, 1) + "." + teachers2.Middlename.Substring(0, 1) + "." : "",
                                  Teacher3 = teachers3 != null ? teachers3.Lastname + " " + teachers3.Firstname.Substring(0, 1) + "." + teachers3.Middlename.Substring(0, 1) + "." : "",
                                  Teacher4 = teachers4 != null ? teachers4.Lastname + " " + teachers4.Firstname.Substring(0, 1) + "." + teachers4.Middlename.Substring(0, 1) + "." : "",
                                  Teacher5 = teachers5 != null ? teachers5.Lastname + " " + teachers5.Firstname.Substring(0, 1) + "." + teachers5.Middlename.Substring(0, 1) + "." : "",
                                  Teacher6 = teachers6 != null ? teachers6.Lastname + " " + teachers6.Firstname.Substring(0, 1) + "." + teachers6.Middlename.Substring(0, 1) + "." : "",
                                  Teacher7 = teachers7 != null ? teachers7.Lastname + " " + teachers7.Firstname.Substring(0, 1) + "." + teachers7.Middlename.Substring(0, 1) + "." : "",
                                  TeacherStatement = teacherStatement != null ? teacherStatement.Lastname + " " + teacherStatement.Firstname.Substring(0, 1) + "." + teacherStatement.Middlename.Substring(0, 1) + "." : "",
                                  Course = stat.Course,
                                  Semester = stat.Semester,
                                  Date = statStud.Date,
                                  Grade = statStud.Grade,
                              })).ToList();

            var temp = lstArrears
                .Where(a => a.Grade != null && a.Grade != "Не явился" && a.Grade != "Не удовлетворительно" && a.Grade != "Не зачтено" && a.Grade != "Отчислен/Перевод")
                .Select(a => a.ParentId);


            student.Arrears = lstArrears
                .Where(a => !temp.Contains(a.ParentId))
                .ToList();

            //Заполнение данных о курсовых работах
            student.CoursesWork = db.CourseWorkStudents
                .Where(a => a.StudentId == user.Id)
                .Include(a => a.Student)
                .Include(a => a.Teacher)
                .Include(a => a.Course)
                .OrderByDescending(a => a.Course.DateBegin)
                .ToList();

            //получение данных о портфолио
            student.Portfolio = (from students in db.Users
                                 where students.Id == id
                                 orderby students.Lastname, students.Firstname
                                 select new ReportPortfolio
                                 {
                                     Id = students.Id,
                                     Lastname = students.Lastname,
                                     Firstname = students.Firstname,
                                     Middlename = students.Middlename,
                                     //loggedIn = students.EmailConfirmed,
                                     Science = (from sections in db.Sections
                                                join catigories in db.Catigories on sections.Id equals catigories.SectionId
                                                join artifacts in db.Artifacts on catigories.Id equals artifacts.CatigoryId
                                                where sections.Name == "Науч.-исслед. деятельность" && artifacts.UserId == students.Id
                                                select artifacts).Count(),
                                     Social = (from sections in db.Sections
                                               join catigories in db.Catigories on sections.Id equals catigories.SectionId
                                               join artifacts in db.Artifacts on catigories.Id equals artifacts.CatigoryId
                                               where sections.Name == "Общественная деятельность" && artifacts.UserId == students.Id
                                               select artifacts).Count(),
                                     Sports = (from sections in db.Sections
                                               join catigories in db.Catigories on sections.Id equals catigories.SectionId
                                               join artifacts in db.Artifacts on catigories.Id equals artifacts.CatigoryId
                                               where sections.Name == "Спортивная деятельность" && artifacts.UserId == students.Id
                                               select artifacts).Count(),
                                     Cultural = (from sections in db.Sections
                                                 join catigories in db.Catigories on sections.Id equals catigories.SectionId
                                                 join artifacts in db.Artifacts on catigories.Id equals artifacts.CatigoryId
                                                 where sections.Name == "Культ.-твор. деятельность" && artifacts.UserId == students.Id
                                                 select artifacts).Count()
                                 }).ToList();



            return View(student);
        }

        //[HttpPost]
        public async Task<ActionResult> ImageBlocked(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = db.Users.FirstOrDefault(a => a.Id == id);
            if (user.ImageBlocked)
            {
                user.ImageBlocked = false;
            }
            else
            {
                user.ImageBlocked = true;
            }
            db.Entry(user).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = user.Id });
        }

        [HttpPost]
        public async Task<ActionResult> UploadPlan(string id)
        {
            int _maxSize = 50 * 1024 * 1024;
            ApplicationUser user = new ApplicationUser();
            List<string> mimes = new List<string>
                {
                    "application/pdf", "application/zip"
                };

            var result = new ManageController.Result();
            var upload = Request.Files[0];
            using (var db = new ApplicationDbContext())
            {
                if (upload.ContentLength > _maxSize)
                {
                    result.Error = "Размер не более 50 Мб";
                }
                else if (mimes.FirstOrDefault(m => m == upload.ContentType) == null)
                {
                    result.Error = "Недопустимый формат файла";
                }

                if (result.Error == null)
                {
                    if (id == null)
                        new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    else
                    {
                        user = db.Users.FirstOrDefault(a => a.Id == id);
                        //сохранение файла
                        string extension = Path.GetExtension(upload.FileName);
                        user.NamePlan = upload.FileName;
                        string fileName = Guid.NewGuid().ToString() + extension;
                        user.PathPlan = Server.MapPath("~/IndividualPlans/" + fileName);
                        upload.SaveAs(user.PathPlan);

                        db.Entry(user).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                }
            }
            return RedirectToAction("Details", new { id = user.Id });
        }

        public async Task<ActionResult> DeletePlan(string id)
        {
            if (id == null)
                new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ApplicationUser user = db.Users.FirstOrDefault(a => a.Id == id);

            FileInfo fileInf = new FileInfo(user.PathPlan);
            if (fileInf.Exists)
            {
                fileInf.Delete();
            }

            user.NamePlan = user.PathPlan = "";
            db.Entry(user).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("Details", new { id = user.Id });
        }
    }
}