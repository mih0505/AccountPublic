using Accounts.Models;
using ClosedXML.Excel;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Spire.Xls;

namespace Accounts.Controllers
{
    [Authorize(Roles = "Administrators,FacultiesManagers,Teachers")]
    public class StatementsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Statements2
        public async Task<ActionResult> Index(string sortOrder, string type, int? faculty, int? form, string group, string name, string course, string academicYear, int? page)
        {
            // Получение текущего года
            var year = db.Settings.FirstOrDefault(Setting => (Setting.Name == "CurrentYear"));

            // Создание первого (стандартного) элемента (чтобы избежать бага с фильтрацией данных)
            var defaultItem = new SelectListItem()
            {
                Value = "-1",
                Text = "Все факультеты"
            };

            // Заполнение DropDownList'a c факультетами
            var facultiesList = await db.Faculties
                .Where(a => a.IsDeleted == false)
                .OrderBy(a => a.Name).ToListAsync();

            var facultiesView = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Value = "-1",
                    Text = "Все факультеты"
                }
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
            var formsList = await db.FormOfTrainings
                .OrderBy(a => a.Name).ToListAsync();
            var formsView = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Value = "-1",
                    Text = "Форма обуч."
                }
            };

            foreach (var item in formsList)
            {
                formsView.Add(new SelectListItem()
                {
                    Value = item.Id.ToString(),
                    Text = item.Name
                });
            }

            // Заполнение DropDownList'a c типами
            var typesView = new List<SelectListItem>()
            {
                 new SelectListItem()
                {
                    Value = "-1",
                    Text = "Вид контроля"
                }
            };

            typesView.AddRange(TypeListItems);

            // Заполнение DropDownList'a с номером курса
            var coursesView = new List<SelectListItem>()
            {
                 new SelectListItem()
                {
                    Value = "-1",
                    Text = "Курс"
                }
            };

            coursesView.AddRange(listCourses);

            // Заполнение DropDownList'a с учебными годами
            var yearsList = await db.Statements
                .OrderByDescending(a => a.CurrentYear)
                .Select(a => new { Value = a.CurrentYear, Text = a.CurrentYear })
                .Distinct().OrderBy(a => a.Text).ToListAsync();
            var yearsView = new List<SelectListItem>()
            {
                 new SelectListItem()
                {
                    Value = "-1",
                    Text = "Уч. год"
                }
            };

            foreach (var item in yearsList)
            {
                yearsView.Add(new SelectListItem()
                {
                    Value = item.Value,
                    Text = item.Text
                });
            }

            // Получение списка активных групп учебного года
            List<int> listIdOfCurrentGroups = await db.Groups
                .Where(a => ((a.IsDeleted == false) && (a.AcademicYear == year.Value)))
                .Select(a => a.Id)
                .ToListAsync();

            var statements = from stats in db.Statements
                             join facs in db.Faculties on stats.FacultyId equals facs.Id
                             join grs in db.Groups on stats.GroupId equals grs.Id
                             join profs in db.Profiles on stats.ProfileId equals profs.Id into outerProfs
                             from profs in outerProfs.DefaultIfEmpty()
                             join tds in db.Users on stats.TeacherDisciplineId equals tds.Id into outerTeacher
                             from tds in outerTeacher.DefaultIfEmpty()
                             join tds2 in db.Users on stats.TeacherDiscipline2Id equals tds2.Id into outerTeacher2
                             from tds2 in outerTeacher2.DefaultIfEmpty()
                             join tds3 in db.Users on stats.TeacherDiscipline3Id equals tds3.Id into outerTeacher3
                             from tds3 in outerTeacher3.DefaultIfEmpty()
                             join tds4 in db.Users on stats.TeacherDiscipline4Id equals tds4.Id into outerTeacher4
                             from tds4 in outerTeacher4.DefaultIfEmpty()
                             join tds5 in db.Users on stats.TeacherDiscipline5Id equals tds5.Id into outerTeacher5
                             from tds5 in outerTeacher5.DefaultIfEmpty()
                             join tds6 in db.Users on stats.TeacherDiscipline6Id equals tds6.Id into outerTeacher6
                             from tds6 in outerTeacher6.DefaultIfEmpty()
                             join tds7 in db.Users on stats.TeacherDiscipline7Id equals tds7.Id into outerTeacher7
                             from tds7 in outerTeacher7.DefaultIfEmpty()
                             where stats.ParentId == null && listIdOfCurrentGroups.Contains(stats.GroupId)
                             select new StatementsList
                             {
                                 Id = stats.Id,
                                 FacultyId = stats.FacultyId,
                                 TeacherDisciplineId = stats.TeacherDisciplineId,
                                 TeacherDiscipline2Id = stats.TeacherDiscipline2Id,
                                 TeacherDiscipline3Id = stats.TeacherDiscipline3Id,
                                 TeacherDiscipline4Id = stats.TeacherDiscipline4Id,
                                 TeacherDiscipline5Id = stats.TeacherDiscipline5Id,
                                 TeacherDiscipline6Id = stats.TeacherDiscipline6Id,
                                 TeacherDiscipline7Id = stats.TeacherDiscipline7Id,
                                 AliasFaculty = facs.AliasFaculty,
                                 Faculty = stats.Faculty.Name,
                                 NameDiscipline = stats.NameDiscipline,
                                 GroupName = stats.GroupName,
                                 Teacher = tds != null ? tds.Lastname + " " + tds.Firstname.Substring(0, 1) + "." + (string.IsNullOrEmpty(tds.Middlename) ? "" : tds.Middlename.Substring(0, 1) + ". ") : "",
                                 Teacher2 = tds2 != null ? tds2.Lastname + " " + tds2.Firstname.Substring(0, 1) + "." + (string.IsNullOrEmpty(tds2.Middlename) ? "" : tds2.Middlename.Substring(0, 1) + ". ") : "",
                                 Teacher3 = tds3 != null ? tds3.Lastname + " " + tds3.Firstname.Substring(0, 1) + "." + (string.IsNullOrEmpty(tds3.Middlename) ? "" : tds3.Middlename.Substring(0, 1) + ". ") : "",
                                 Teacher4 = tds4 != null ? tds4.Lastname + " " + tds4.Firstname.Substring(0, 1) + "." + (string.IsNullOrEmpty(tds4.Middlename) ? "" : tds4.Middlename.Substring(0, 1) + ". ") : "",
                                 Teacher5 = tds5 != null ? tds5.Lastname + " " + tds5.Firstname.Substring(0, 1) + "." + (string.IsNullOrEmpty(tds5.Middlename) ? "" : tds5.Middlename.Substring(0, 1) + ". ") : "",
                                 Teacher6 = tds6 != null ? tds6.Lastname + " " + tds6.Firstname.Substring(0, 1) + "." + (string.IsNullOrEmpty(tds6.Middlename) ? "" : tds6.Middlename.Substring(0, 1) + ". ") : "",
                                 Teacher7 = tds7 != null ? tds7.Lastname + " " + tds7.Firstname.Substring(0, 1) + "." + (string.IsNullOrEmpty(tds7.Middlename) ? "" : tds7.Middlename.Substring(0, 1) + ". ") : "",
                                 TypeControl = stats.TypeControl,
                                 Course = stats.Course,
                                 Semester = stats.Semester,
                                 DateEnd = stats.DateEnd,
                                 CurrentYear = stats.CurrentYear,
                                 Form = grs.FormOfTrainingId,
                                 Hours = stats.Hours,
                                 ZET = stats.ZET,
                                 Complete = db.StatementStudents.Where(a => a.StatementId == stats.Id && a.Grade != null).Count(),
                                 CountStudents = db.StatementStudents.Where(a => a.StatementId == stats.Id).Count()
                             };

            // Получение Cookies
            var cookieGroup = Request.Cookies.Get("stat_group");
            var cookieName = Request.Cookies.Get("stat_name");
            var cookieType = Request.Cookies.Get("stat_type");
            var cookieSortOrder = Request.Cookies.Get("stat_sortOrder");
            var cookieFaculty = Request.Cookies.Get("stat_faculty");
            var cookieForm = Request.Cookies.Get("stat_form");
            var cookieAcademicYear = Request.Cookies.Get("stat_academicYear");
            var cookieCourse = Request.Cookies.Get("stat_course");

            // Применение значений из Cookies
            // В том числе обновление значений в DropDownList согласно полученным Cookies
            if (group == null && cookieGroup != null)
            {
                group = cookieGroup.Value;
            }

            if (name == null && cookieName != null)
            {
                name = cookieName.Value;
            }

            if (type == null && cookieType != null)
            {
                typesView.FirstOrDefault(Type => (Type.Value == cookieType.Value)).Selected = true;
                type = cookieType.Value;
            }

            if (sortOrder == null && cookieSortOrder != null)
            {
                sortOrder = cookieSortOrder.Value;
            }

            if (faculty == null && cookieFaculty != null)
            {
                facultiesView.FirstOrDefault(Faculty => (Faculty.Value == cookieFaculty.Value)).Selected = true;
                faculty = Convert.ToInt32(cookieFaculty.Value);
            }

            if (form == null && cookieForm != null)
            {
                formsView.FirstOrDefault(Form => Form.Value == cookieForm.Value).Selected = true;
                form = Convert.ToInt32(cookieForm.Value);
            }

            if (academicYear == null && cookieAcademicYear != null)
            {
                yearsView.FirstOrDefault(Year => (Year.Value == cookieAcademicYear.Value)).Selected = true;
                academicYear = cookieAcademicYear.Value;
            }

            if (course == null && cookieCourse != null)
            {
                coursesView.FirstOrDefault(Course => (Course.Value == cookieCourse.Value)).Selected = true;
                course = cookieCourse.Value;
            }

            // Удаление Cookies
            Response.Cookies.Add(new HttpCookie("stat_group")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("stat_name")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("stat_type")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("stat_sortOrder")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("stat_faculty")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("stat_academicYear")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("stat_course")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });


            // Применение фильтров
            // В том числе вывод нужной информации для определенной роли
            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);
            if (User.IsInRole("FacultiesManagers"))
            {
                var statsTeacher = await db.StatementStudents.Where(a => a.TeacherStatementId == userId).Select(a => a.StatementId).Distinct().ToListAsync();

                var date = DateTime.Now.Date;
                statements = statements.Where(a => (a.FacultyId == user.FacultyId) || ((a.TeacherDisciplineId == user.Id || a.TeacherDiscipline2Id == user.Id || a.TeacherDiscipline3Id == user.Id ||
                a.TeacherDiscipline4Id == user.Id || a.TeacherDiscipline5Id == user.Id || a.TeacherDiscipline6Id == user.Id || a.TeacherDiscipline7Id == user.Id ||
                statsTeacher.Contains(a.Id)) && a.DateEnd > date));
            }
            else if (User.IsInRole("Teachers"))
            {
                // Получение списка идентификаторов ведомостей, где преподаватель выставляет оценки в подгруппе
                var statsTeacher = await db.StatementStudents
                    .Where(a => a.TeacherStatementId == userId)
                    .Select(a => a.StatementId)
                    .Distinct()
                    .ToListAsync();

                // Получение списка идентификаторов ведомостей групп по открытым индивидуальным ведомостям на преподавателя
                List<int> listIdOfIndividualStatements = await db.Statements
                    .Where(a => a.DateEnd >= DateTime.Now && a.ParentId != null && (a.TeacherDisciplineId == user.Id || a.TeacherDiscipline2Id == user.Id ||
                    a.TeacherDiscipline3Id == user.Id || a.TeacherDiscipline4Id == user.Id || a.TeacherDiscipline5Id == user.Id ||
                    a.TeacherDiscipline6Id == user.Id || a.TeacherDiscipline7Id == user.Id))
                    .Select(a => a.ParentId)
                    .Cast<int>()
                    .Distinct()
                    .ToListAsync();

                var date = DateTime.Now.Date;
                statements = statements.Where(a => (a.TeacherDisciplineId == user.Id || a.TeacherDiscipline2Id == user.Id || a.TeacherDiscipline3Id == user.Id ||
                a.TeacherDiscipline4Id == user.Id || a.TeacherDiscipline5Id == user.Id || a.TeacherDiscipline6Id == user.Id || a.TeacherDiscipline7Id == user.Id ||
                statsTeacher.Contains(a.Id)) && (a.DateEnd > date || listIdOfIndividualStatements.Contains(a.Id)));
            }

            // Применение фильтров
            // В том числе добавление новых Cookies
            if (sortOrder != null)
            {
                Response.Cookies.Add(new HttpCookie("stat_sortOrder")
                {
                    Value = sortOrder.ToString(),
                    Expires = DateTime.Now.AddMinutes(15)
                });
            }

            if (faculty != null)
            {
                Response.Cookies.Add(new HttpCookie("stat_faculty")
                {
                    Value = faculty.ToString(),
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (faculty.Value != -1)
                {
                    statements = statements.Where(p => p.FacultyId == faculty);
                }
            }

            if (form != null)
            {
                Response.Cookies.Add(new HttpCookie("stat_form")
                {
                    Value = form.ToString(),
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (form.Value != -1)
                {
                    statements = statements.Where(p => p.Form == form);
                }
            }

            if (!string.IsNullOrEmpty(group))
            {
                Response.Cookies.Add(new HttpCookie("stat_group")
                {
                    Value = group,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                statements = statements.Where(p => SqlFunctions.PatIndex("%" + group + "%", p.GroupName) > 0);
            }

            if (!string.IsNullOrEmpty(name))
            {
                Response.Cookies.Add(new HttpCookie("stat_name")
                {
                    Value = name,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                statements = statements.Where(p => SqlFunctions.PatIndex("%" + name + "%", p.NameDiscipline) > 0);
            }

            if (!string.IsNullOrEmpty(type))
            {
                Response.Cookies.Add(new HttpCookie("stat_type")
                {
                    Value = type,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (type != (-1).ToString())
                {
                    statements = statements.Where(p => SqlFunctions.PatIndex("%" + type + "%", p.TypeControl) > 0);
                }
            }

            if (!string.IsNullOrEmpty(course))
            {
                Response.Cookies.Add(new HttpCookie("stat_course")
                {
                    Value = course,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (course != (-1).ToString())
                {
                    int courseId = Convert.ToInt32(course);
                    statements = statements.Where(p => p.Course == courseId);
                }
            }

            if (academicYear != null)
            {
                Response.Cookies.Add(new HttpCookie("stat_academicYear")
                {
                    Value = academicYear,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (academicYear != (-1).ToString())
                {
                    statements = statements.Where(p => p.CurrentYear == academicYear);
                }
            }

            // Применение сортировки
            switch (sortOrder)
            {
                case "faculty_asc":
                    statements = statements.OrderBy(s => s.AliasFaculty);
                    break;
                case "faculty_desc":
                    statements = statements.OrderByDescending(s => s.AliasFaculty);
                    break;
                case "form_asc":
                    statements = statements.OrderBy(s => s.Form);
                    break;
                case "form_desc":
                    statements = statements.OrderByDescending(s => s.Form);
                    break;
                case "group_asc":
                    statements = statements.OrderBy(s => s.GroupName);
                    break;
                case "group_desc":
                    statements = statements.OrderByDescending(s => s.GroupName);
                    break;
                case "name_asc":
                    statements = statements.OrderBy(s => s.NameDiscipline);
                    break;
                case "name_desc":
                    statements = statements.OrderByDescending(s => s.NameDiscipline);
                    break;
                case "course_asc":
                    statements = statements.OrderBy(s => s.Course);
                    break;
                case "course_desc":
                    statements = statements.OrderByDescending(s => s.Course);
                    break;
                case "type_asc":
                    statements = statements.OrderBy(s => s.TypeControl);
                    break;
                case "type_desc":
                    statements = statements.OrderByDescending(s => s.TypeControl);
                    break;
                default:
                    statements = statements.OrderByDescending(s => s.Id);
                    break;
            }

            // Обновление всех ViewBag'ов
            ViewBag.FacultyList = facultiesView;
            ViewBag.FormList = formsView;
            ViewBag.Types = typesView;
            ViewBag.Courses = coursesView;
            ViewBag.Years = yearsView;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.FacultySortParm = sortOrder == "faculty_asc" ? "faculty_desc" : "faculty_asc";
            ViewBag.FormSortParm = sortOrder == "form_asc" ? "form_desc" : "form_asc";
            ViewBag.GroupSortParm = sortOrder == "group_asc" ? "group_desc" : "group_asc";
            ViewBag.NameSortParm = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.CourseSortParm = sortOrder == "course_asc" ? "course_desc" : "course_asc";
            ViewBag.TypeSortParm = sortOrder == "type_asc" ? "type_desc" : "type_asc";
            ViewBag.FacultyFilter = faculty;
            ViewBag.FormFilter = form;
            ViewBag.GroupFilter = group;
            ViewBag.NameFilter = name;
            ViewBag.TypeFilter = type;
            ViewBag.YearFilter = academicYear;
            ViewBag.CourseFilter = course;

            // Вывод результата
            return View(statements.ToPagedList(page ?? 1, 30));
        }

        public ActionResult ClearFilter()
        {
            HttpCookie cookieSortOrder = Request.Cookies.Get("stat_sortOrder");
            if (cookieSortOrder != null)
            {
                cookieSortOrder.Value = null;
                cookieSortOrder.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieSortOrder);
            }

            HttpCookie cookieGroup = Request.Cookies.Get("stat_group");
            if (cookieGroup != null)
            {
                cookieGroup.Value = null;
                cookieGroup.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieGroup);
            }

            HttpCookie cookieName = Request.Cookies.Get("stat_name");
            if (cookieName != null)
            {
                cookieName.Value = null;
                cookieName.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieName);
            }

            HttpCookie cookieFaculty = Request.Cookies.Get("stat_faculty");
            if (cookieFaculty != null)
            {
                cookieFaculty.Value = null;
                cookieFaculty.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieFaculty);
            }

            HttpCookie cookieAcademicYear = Request.Cookies.Get("stat_academicYear");
            if (cookieAcademicYear != null)
            {
                cookieAcademicYear.Value = null;
                cookieAcademicYear.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieAcademicYear);
            }

            HttpCookie cookieType = Request.Cookies.Get("stat_type");
            if (cookieType != null)
            {
                cookieType.Value = null;
                cookieType.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieType);
            }

            HttpCookie cookieCourse = Request.Cookies.Get("stat_course");
            if (cookieCourse != null)
            {
                cookieCourse.Value = null;
                cookieCourse.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieCourse);
            }

            return RedirectToAction("Index");
        }

        public List<SelectListItem> listCourses = new List<SelectListItem>(new[] {
            new SelectListItem
            {
                Text = "1",
                Value = "1"
            },
            new SelectListItem
            {
                Text = "2",
                Value = "2"
            },
            new SelectListItem
            {
                Text = "3",
                Value = "3"
            },
             new SelectListItem
            {
                Text = "4",
                Value = "4"
            },
             new SelectListItem
            {
                Text = "5",
                Value = "5"
            },
             new SelectListItem
            {
                Text = "6",
                Value = "6"
            }
        });

        [Authorize(Roles = "Administrators,FacultiesManagers,Teachers")]
        public ActionResult StatementsDiscipline(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var lstStatements = db.Statements
                .Where(a => a.Id == id || a.ParentId == id)
                .OrderByDescending(a => a.ParentId)
                .ThenByDescending(a => a.DateBegin)
                .ToList();
            if (lstStatements != null)
                ViewBag.MainStatementId = id;

            return View(lstStatements);
        }

        [Authorize(Roles = "Administrators,FacultiesManagers,Teachers")]
        public async Task<ActionResult> StatementGroup(int? id)
        {
            var statFull = new StatementFull();

            //получаем ведомость
            statFull.Statement = db.Statements
                .Include(a => a.Faculty)
                .Include(s => s.Group)
                .Include(s => s.Profile)
                .Include(s => s.TeacherDiscipline)
                .FirstOrDefault(a => a.Id == id);

            //получаем список студентов группы
            var students = db.StatementStudents
                .Where(a => a.StatementId == id)
                .Include(a => a.StudentStatement)
                .OrderBy(a => a.StudentStatement.Lastname)
                .ThenBy(a => a.StudentStatement.Firstname)
                .ThenBy(a => a.StudentStatement.Middlename)
                .ToList();

            /////получаем список преподавателей факультета//////
            List<TutorsList> lstTeacher;
            //получаем пользователя
            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);

            //получаем список преподавателей
            if (statFull.Statement.DepartmentId == null)
            {
                var depList = db.Departments.Where(a => a.FacultyId == statFull.Statement.FacultyId).Select(a => a.Id).ToList();
                lstTeacher = db.TeacherDepartments.Where(a => depList.Contains(a.DepartmentId)).Include(a => a.Teacher).OrderBy(a => a.Teacher.Lastname)
                  .ThenBy(a => a.Teacher.Firstname).ThenBy(a => a.Teacher.Middlename)
                  .Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." }).ToList();
            }
            else
                lstTeacher = db.TeacherDepartments.Where(a => a.DepartmentId == statFull.Statement.DepartmentId).Include(a => a.Teacher).OrderBy(a => a.Teacher.Lastname)
                    .ThenBy(a => a.Teacher.Firstname).ThenBy(a => a.Teacher.Middlename)
                    .Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." }).ToList();

            //добавляем преподавателя для перезачетов
            var advTeacher = db.Users.FirstOrDefault(a => a.FacultyId == user.FacultyId && a.Lastname == "Перезачет");
            if (advTeacher != null)
                lstTeacher.Insert(0, new TutorsList { Id = advTeacher.Id, Name = advTeacher.Lastname + " " + advTeacher.Firstname.Substring(0, 1) + "." + advTeacher.Middlename.Substring(0, 1) + "." });

            var distVM = new List<StatementDistributionViewModel>();
            //var lstStudentGroup = await db.Users.Where(a => a.GroupId == statFull.Statement.GroupId && a.DateBlocked == null)
            //        .OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).ThenBy(a => a.Middlename).ToListAsync();

            foreach (var s in students)
            {
                distVM.Add(new StatementDistributionViewModel
                {
                    StatementStudent = s,
                    Teachers = new SelectList(lstTeacher, "Id", "Name", s.TeacherStatementId),
                    Grades = new SelectList(listItems, "Value", "Text", s.Grade)
                });
            }

            statFull.StatementDistribution = distVM;

            return PartialView(statFull);
        }

        [HttpPost]
        [Authorize(Roles = "Administrators,FacultiesManagers,Teachers")]
        public ActionResult StatementGroup(StatementFull distribution)
        {
            var stat = db.Statements.Find(distribution.Statement.Id);
            foreach (var dist in distribution.StatementDistribution)
            {
                StatementStudent cws = db.StatementStudents.Find(dist.StatementStudent.Id);
                if (stat.TypeControl == "Зачет" || stat.TypeControl == "Зачет с оценкой" || stat.TypeControl == "Курс. раб." || stat.TypeControl == "Контр. раб." || stat.TypeControl == "Практика")
                {
                    if (dist.StatementStudent.Grade != cws.Grade || cws.Date != dist.StatementStudent.Date || cws.TotalPoint != dist.StatementStudent.TotalPoint)
                    {
                        cws.Date = dist.StatementStudent.Date;
                        cws.Grade = dist.StatementStudent.Grade;
                        cws.TotalPoint = dist.StatementStudent.TotalPoint;
                        cws.TeacherStatementId = (dist.StatementStudent.TeacherStatementId == null) ? stat.TeacherDisciplineId : dist.StatementStudent.TeacherStatementId;
                        cws.GroupIdSite = stat.GroupId;
                        cws.GroupIdDecanate = stat.GroupIdDecanate;
                        cws.StatementId = stat.Id;
                        cws.StudentStatementId = dist.StatementStudent.StudentStatementId;
                        switch (dist.StatementStudent.Grade)
                        {
                            case "Отлично":
                                cws.GradeByNumber = 5;
                                break;
                            case "Хорошо":
                                cws.GradeByNumber = 4;
                                break;
                            case "Удовлетворительно":
                                cws.GradeByNumber = 3;
                                break;
                            case "Не удовлетворительно":
                                cws.GradeByNumber = 2;
                                break;
                            default:
                                cws.GradeByNumber = 0;
                                break;
                        }
                    }
                }

                if (stat.TypeControl == "Экзамен")
                {
                    if (dist.StatementStudent.Grade != cws.Grade || cws.PointSemester != dist.StatementStudent.PointSemester || cws.Date != dist.StatementStudent.Date ||
                        cws.PointAdvanced != dist.StatementStudent.PointAdvanced || cws.PointControl != dist.StatementStudent.PointControl)
                    {
                        cws.Date = dist.StatementStudent.Date;
                        cws.Grade = dist.StatementStudent.Grade;
                        cws.PointSemester = dist.StatementStudent.PointSemester;
                        cws.PointAdvanced = dist.StatementStudent.PointAdvanced;
                        cws.PointControl = dist.StatementStudent.PointControl;
                        cws.TotalPoint = cws.PointSemester + cws.PointAdvanced + cws.PointControl;
                        cws.TeacherStatementId = (dist.StatementStudent.TeacherStatementId == null) ? stat.TeacherDisciplineId : dist.StatementStudent.TeacherStatementId;
                        cws.GroupIdSite = stat.GroupId;
                        cws.GroupIdDecanate = stat.GroupIdDecanate;
                        cws.StatementId = stat.Id;
                        cws.StudentStatementId = dist.StatementStudent.StudentStatementId;
                        switch (dist.StatementStudent.Grade)
                        {
                            case "Отлично":
                                cws.GradeByNumber = 5;
                                break;
                            case "Хорошо":
                                cws.GradeByNumber = 4;
                                break;
                            case "Удовлетворительно":
                                cws.GradeByNumber = 3;
                                break;
                            case "Неудовлетворительно":
                                cws.GradeByNumber = 2;
                                break;
                            default:
                                cws.GradeByNumber = 0;
                                break;
                        }
                    }
                }
                //сохранение преподавателя, если не выставлены оценки
                if (dist.StatementStudent.TeacherStatementId != null)
                    cws.TeacherStatementId = (dist.StatementStudent.TeacherStatementId == null) ? stat.TeacherDisciplineId : dist.StatementStudent.TeacherStatementId;

                db.Entry(cws).State = EntityState.Modified;
            }
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public ActionResult IndividualStatement(int? statementId, string studentId)
        {
            if (statementId == null)
                return HttpNotFound();

            ViewBag.MainStatementId = statementId;

            var currentStatement = db.Statements.Find(statementId);
            if (currentStatement == null)
                return HttpNotFound();

            var newIndividualStatement = new StatementIndividual();
            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);

            //поиск дочерних ведомостей для выяснения следующего номера текущей дочерней ведомости
            int countStatement = db.Statements.Where(a => a.ParentId == currentStatement.Id).Count();
            newIndividualStatement.ParentId = currentStatement.Id;
            newIndividualStatement.Number = currentStatement.Number + "." + (countStatement + 1);
            newIndividualStatement.FacultyId = currentStatement.FacultyId;
            newIndividualStatement.TypeControl = currentStatement.TypeControl;
            newIndividualStatement.CurrentYear = currentStatement.CurrentYear;
            newIndividualStatement.ProfileId = currentStatement.ProfileId;
            newIndividualStatement.DepartmentId = currentStatement.DepartmentId;
            newIndividualStatement.GroupIdDecanate = currentStatement.GroupIdDecanate;
            newIndividualStatement.GroupName = currentStatement.GroupName;
            newIndividualStatement.Course = currentStatement.Course;
            newIndividualStatement.Semester = currentStatement.Semester;
            newIndividualStatement.GroupId = currentStatement.GroupId;
            newIndividualStatement.NameDiscipline = currentStatement.NameDiscipline;
            newIndividualStatement.ZET = currentStatement.ZET;
            newIndividualStatement.Hours = currentStatement.Hours;


            //добавление списка со студентами группы и выбор студента по умолчанию
            var lstStudentGroup = db.Users
                .Where(a => a.GroupId == currentStatement.GroupId && a.DateBlocked == null)
                    .OrderBy(a => a.Lastname)
                    .ThenBy(a => a.Firstname)
                    .ThenBy(a => a.Middlename)
                    .Select(a => new TutorsList
                    {
                        Id = a.Id,
                        Name = a.Lastname + " " + a.Firstname + " " + a.Middlename
                    })
                    .ToList();
            ViewBag.StudentId = new SelectList(lstStudentGroup, "Id", "Name", studentId);

            List<TutorsList> teachersList;
            ////получаем список ид кафедр на факультете
            //var departmentsIds = db.Departments.Where(a => a.FacultyId == newIndividualStatement.FacultyId && a.IsDeleted == false).Select(a => a.Id).ToList();
            ////получаем список преподавателей кафедр
            //teachersList = db.TeacherDepartments.Where(a => departmentsIds.Contains(a.DepartmentId)).Include(a => a.Teacher).OrderBy(a => a.Teacher.Lastname)
            //    .ThenBy(a => a.Teacher.Firstname).ThenBy(a => a.Teacher.Middlename).Select(a => new TutorsList
            //    {
            //        Id = a.TeacherId,
            //        Name = a.Teacher.Lastname
            //    + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "."
            //    }).ToList();

            if (newIndividualStatement.DepartmentId == null)
            {
                var depList = db.Departments.Where(a => a.FacultyId == newIndividualStatement.FacultyId).Select(a => a.Id).ToList();
                teachersList = db.TeacherDepartments
                    .Where(a => depList.Contains(a.DepartmentId))
                    .Include(a => a.Teacher)
                    .Include(a => a.Department)
                    .OrderBy(a => a.Teacher.Lastname)
                  .ThenBy(a => a.Teacher.Firstname)
                  .ThenBy(a => a.Teacher.Middlename)
                  .Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." + " (" + a.Department.ShortName + ")" })
                  .ToList();
            }
            else
                teachersList = db.TeacherDepartments
                    .Where(a => a.DepartmentId == newIndividualStatement.DepartmentId)
                    .Include(a => a.Teacher)
                    .Include(a => a.Department)
                    .OrderBy(a => a.Teacher.Lastname)
                    .ThenBy(a => a.Teacher.Firstname)
                    .ThenBy(a => a.Teacher.Middlename)
                    .Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." + " (" + a.Department.ShortName + ")" })
                    .ToList();


            //добавляем преподавателя для перезачетов
            var advTeacher = db.Users.FirstOrDefault(a => a.FacultyId == user.FacultyId && a.Lastname == "Перезачет");
            if (advTeacher != null)
                teachersList.Insert(0, new TutorsList { Id = advTeacher.Id, Name = advTeacher.Lastname + " " + advTeacher.Firstname.Substring(0, 1) + "." + advTeacher.Middlename.Substring(0, 1) + "." });

            ViewBag.TeacherDisciplineId = new SelectList(teachersList, "Id", "Name", currentStatement.TeacherDisciplineId);

            var teacherAdvList = db.TeacherDepartments.Include(a => a.Teacher).OrderBy(a => a.Teacher.Lastname)
            .ThenBy(a => a.Teacher.Firstname).ThenBy(a => a.Teacher.Middlename)
            .Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." }).ToList();
            ViewBag.TeacherDiscipline2Id = ViewBag.TeacherDiscipline3Id = ViewBag.TeacherDiscipline4Id =
            ViewBag.TeacherDiscipline5Id = ViewBag.TeacherDiscipline6Id = ViewBag.TeacherDiscipline7Id = new SelectList(teacherAdvList, "Id", "Name");

            return View(newIndividualStatement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public async Task<ActionResult> IndividualStatement([Bind(Include = "Id,Number,NameDiscipline,DepartmentId,TeacherDisciplineId,TeacherDiscipline2Id,TeacherDiscipline3Id,TeacherDiscipline4Id,TeacherDiscipline5Id,TeacherDiscipline6Id,TeacherDiscipline7Id,TypeControl,GroupId," +
            "GroupName,GroupIdDecanate,ProfileId,FacultyId,Course,Semester,DateBegin,DateEnd,CreaterStatement,CurrentYear,ZET,Hours,StudentId,ParentId")] StatementIndividual newIndividualStatement)
        {
            string userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");

            if (ModelState.IsValid)
            {
                //добавляем дочернюю (индивидуальную ведомость) 
                newIndividualStatement.CreaterStatement = user.Id;
                var statement = new Statement()
                {
                    ParentId = newIndividualStatement.ParentId,
                    Number = newIndividualStatement.Number,
                    FacultyId = newIndividualStatement.FacultyId,
                    TypeControl = newIndividualStatement.TypeControl,
                    CurrentYear = newIndividualStatement.CurrentYear,
                    ProfileId = newIndividualStatement.ProfileId,
                    DepartmentId = newIndividualStatement.DepartmentId,
                    GroupIdDecanate = newIndividualStatement.GroupIdDecanate,
                    GroupName = newIndividualStatement.GroupName,
                    Course = newIndividualStatement.Course,
                    Semester = newIndividualStatement.Semester,
                    GroupId = newIndividualStatement.GroupId,
                    NameDiscipline = newIndividualStatement.NameDiscipline,
                    ZET = newIndividualStatement.ZET,
                    Hours = newIndividualStatement.Hours,
                    DateBegin = DateTime.Now,
                    DateEnd = (DateTime)newIndividualStatement.DateEnd,
                    TeacherDisciplineId = newIndividualStatement.TeacherDisciplineId,
                    TeacherDiscipline2Id = newIndividualStatement.TeacherDiscipline2Id,
                    TeacherDiscipline3Id = newIndividualStatement.TeacherDiscipline3Id,
                    TeacherDiscipline4Id = newIndividualStatement.TeacherDiscipline4Id,
                    TeacherDiscipline5Id = newIndividualStatement.TeacherDiscipline5Id,
                    TeacherDiscipline6Id = newIndividualStatement.TeacherDiscipline6Id,
                    TeacherDiscipline7Id = newIndividualStatement.TeacherDiscipline7Id,
                };
                db.Statements.Add(statement);

                //закрепляем за индив. ведомостью студента
                var student = db.Users.FirstOrDefault(a => a.Id == newIndividualStatement.StudentId);
                var statementStudent = new StatementStudent
                {
                    GroupIdSite = Convert.ToInt32(student.GroupId),
                    GroupIdDecanate = Convert.ToInt32(student.idGroupDecanat),
                    StudentStatement = student,
                    IdStudentDecanate = Convert.ToInt32(student.DecanatId),
                    StatementId = newIndividualStatement.Id,
                };
                db.StatementStudents.Add(statementStudent);

                await db.SaveChangesAsync();
                return RedirectToAction("StatementsDiscipline", new { Id = newIndividualStatement.ParentId });
            }

            //добавление списка со студентами группы и выбор студента по умолчанию
            var lstStudentGroup = db.Users
                .Where(a => a.GroupId == newIndividualStatement.GroupId && a.DateBlocked == null)
                    .OrderBy(a => a.Lastname)
                    .ThenBy(a => a.Firstname)
                    .ThenBy(a => a.Middlename)
                    .Select(a => new TutorsList
                    {
                        Id = a.Id,
                        Name = a.Lastname + " " + a.Firstname + " " + a.Middlename
                    })
                    .ToList();
            ViewBag.StudentId = new SelectList(lstStudentGroup, "Id", "Name", newIndividualStatement.StudentId);

            List<TutorsList> teachersList;
            //получаем список ид кафедр на факультете
            var departmentsIds = db.Departments.Where(a => a.FacultyId == newIndividualStatement.FacultyId && a.IsDeleted == false).Select(a => a.Id).ToList();
            //получаем список преподавателей кафедр
            teachersList = db.TeacherDepartments.Where(a => departmentsIds.Contains(a.DepartmentId)).Include(a => a.Teacher).OrderBy(a => a.Teacher.Lastname)
                    .ThenBy(a => a.Teacher.Firstname).ThenBy(a => a.Teacher.Middlename).Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." }).ToList();

            //добавляем преподавателя для перезачетов
            var advTeacher = db.Users.FirstOrDefault(a => a.FacultyId == user.FacultyId && a.Lastname == "Перезачет");
            if (advTeacher != null)
                teachersList.Insert(0, new TutorsList { Id = advTeacher.Id, Name = advTeacher.Lastname + " " + advTeacher.Firstname.Substring(0, 1) + "." + advTeacher.Middlename.Substring(0, 1) + "." });

            var teachersAdvList = db.TeacherDepartments.Include(a => a.Teacher).OrderBy(a => a.Teacher.Lastname)
                .ThenBy(a => a.Teacher.Firstname).ThenBy(a => a.Teacher.Middlename)
                .Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." }).ToList();
            ViewBag.TeacherDisciplineId = new SelectList(teachersList, "Id", "Name", newIndividualStatement.TeacherDisciplineId);
            ViewBag.TeacherDiscipline2Id = new SelectList(teachersAdvList, "Id", "Name", newIndividualStatement.TeacherDiscipline2Id);
            ViewBag.TeacherDiscipline3Id = new SelectList(teachersAdvList, "Id", "Name", newIndividualStatement.TeacherDiscipline3Id);
            ViewBag.TeacherDiscipline4Id = new SelectList(teachersAdvList, "Id", "Name", newIndividualStatement.TeacherDiscipline4Id);
            ViewBag.TeacherDiscipline5Id = new SelectList(teachersAdvList, "Id", "Name", newIndividualStatement.TeacherDiscipline5Id);
            ViewBag.TeacherDiscipline6Id = new SelectList(teachersAdvList, "Id", "Name", newIndividualStatement.TeacherDiscipline6Id);
            ViewBag.TeacherDiscipline7Id = new SelectList(teachersAdvList, "Id", "Name", newIndividualStatement.TeacherDiscipline7Id);

            return View(newIndividualStatement);
        }



        public List<SelectListItem> TypeListItems = new List<SelectListItem>(new[] {
            new SelectListItem { Text = "Зачет", Value = "Зачет" },
            new SelectListItem { Text = "Зачет с оценкой", Value = "Зачет с оценкой"},
            new SelectListItem { Text = "Экзамен", Value = "Экзамен"},
            new SelectListItem { Text = "Курс. раб.", Value = "Курс. раб."},
            new SelectListItem { Text = "Контр. раб.", Value = "Контр. раб."},
            new SelectListItem { Text = "Практика", Value = "Практика"}
        });

        public List<SelectListItem> listItems = new List<SelectListItem>(new[] {
            new SelectListItem{ Text= "Отчислен/Перевод", Value = "Отчислен/Перевод" },
            new SelectListItem{ Text= "Не явился", Value = "Не явился" },
            new SelectListItem{ Text= "Зачтено", Value = "Зачтено" },
            new SelectListItem{ Text= "Не зачтено", Value = "Не зачтено" },
            new SelectListItem{ Text= "Неудовлетворительно", Value = "Неудовлетворительно" },
            new SelectListItem{ Text= "Удовлетворительно", Value = "Удовлетворительно" },
            new SelectListItem{ Text= "Хорошо", Value = "Хорошо" },
            new SelectListItem{ Text= "Отлично", Value = "Отлично" }
        });

        public JsonResult GetTeachers(int id)
        {
            List<TutorsList> teachersList;
            int depId = Convert.ToInt32(id);
            //получаем список ид кафедр на факультете
            //var departmentsIds = db.Departments.Where(a => a.Id == depId && a.IsDeleted == false).Select(a => a.Id).ToList();
            //получаем список преподавателей кафедр
            teachersList = db.TeacherDepartments.Where(a => a.DepartmentId == depId)
                .Include(a => a.Teacher)
                .OrderBy(a => a.Teacher.Lastname)
                .ThenBy(a => a.Teacher.Firstname)
                .ThenBy(a => a.Teacher.Middlename)
                .Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." }).ToList();

            teachersList.Insert(0, new TutorsList { Id = null, Name = "--Выберите--" });

            //добавляем преподавателя для перезачетов
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            if (user != null)
            {
                var advTeacher = db.Users.FirstOrDefault(a => a.FacultyId == user.FacultyId && a.Lastname == "Перезачет");
                if (advTeacher != null)
                    teachersList.Insert(1, new TutorsList { Id = advTeacher.Id, Name = advTeacher.Lastname + " " + advTeacher.Firstname.Substring(0, 1) + "." + advTeacher.Middlename.Substring(0, 1) + "." });
            }

            return Json(new SelectList(teachersList, "Id", "Name"));
        }

        public JsonResult GetGroups(int? id)
        {
            List<GroupsList> groupsList;
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
            if (id == null)
            {
                groupsList = db.Groups.Where(a => a.IsDeleted == false &&
                a.AcademicYear == currentYear.Value).OrderBy(a => a.Name).
               Select(a => new GroupsList { Id = a.Id, Name = a.Name }).ToList();
            }
            else
            {
                int facultyId = Convert.ToInt32(id);
                groupsList = db.Groups.Where(a => a.FacultyId == facultyId && a.IsDeleted == false &&
                a.AcademicYear == currentYear.Value).OrderBy(a => a.Name).
               Select(a => new GroupsList { Id = a.Id, Name = a.Name }).ToList();
            }
            groupsList.Insert(0, new GroupsList { Id = -1, Name = "--Выберите--" });
            return Json(new SelectList(groupsList, "Id", "Name"));
        }

        // GET: Statements2/Create
        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public ActionResult Create()
        {
            var statement = new Statement();

            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
            statement.FacultyId = user.FacultyId;
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", user.FacultyId);
            ViewBag.TypeControl = new SelectList(TypeListItems, "Text", "Value");
            ViewBag.GroupId = new SelectList(db.Groups.Where(a => a.FacultyId == user.FacultyId && a.IsDeleted == false &&
                a.AcademicYear == currentYear.Value).OrderBy(a => a.Name), "Id", "Name");
            ViewBag.ProfileId = new SelectList(db.Profiles.Where(a => a.IsDeleted == false), "Id", "Name");
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");

            List<TutorsList> teachersList;
            //получаем список ид кафедр на факультете
            var departmentsIds = db.Departments.Where(a => a.FacultyId == statement.FacultyId && a.IsDeleted == false).Select(a => a.Id).ToList();
            //получаем список преподавателей кафедр
            teachersList = db.TeacherDepartments.Where(a => departmentsIds.Contains(a.DepartmentId)).Include(a => a.Teacher).OrderBy(a => a.Teacher.Lastname)
                .ThenBy(a => a.Teacher.Firstname).ThenBy(a => a.Teacher.Middlename).Select(a => new TutorsList
                {
                    Id = a.TeacherId,
                    Name = a.Teacher.Lastname
                + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "."
                }).ToList();

            //добавляем преподавателя для перезачетов
            var advTeacher = db.Users.FirstOrDefault(a => a.FacultyId == user.FacultyId && a.Lastname == "Перезачет");
            if (advTeacher != null)
                teachersList.Insert(0, new TutorsList { Id = advTeacher.Id, Name = advTeacher.Lastname + " " + advTeacher.Firstname.Substring(0, 1) + "." + advTeacher.Middlename.Substring(0, 1) + "." });

            ViewBag.TeacherDisciplineId = new SelectList(teachersList, "Id", "Name");

            var teacherAdvList = db.TeacherDepartments.Include(a => a.Teacher).OrderBy(a => a.Teacher.Lastname)
            .ThenBy(a => a.Teacher.Firstname).ThenBy(a => a.Teacher.Middlename)
            .Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." }).ToList();
            ViewBag.TeacherDiscipline2Id = ViewBag.TeacherDiscipline3Id = ViewBag.TeacherDiscipline4Id =
            ViewBag.TeacherDiscipline5Id = ViewBag.TeacherDiscipline6Id = ViewBag.TeacherDiscipline7Id = new SelectList(teacherAdvList, "Id", "Name");
            return View();
        }

        // POST: Statements2/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public async Task<ActionResult> Create([Bind(Include = "Id,Number,NameDiscipline,DepartmentId,TeacherDisciplineId,TeacherDiscipline2Id,TeacherDiscipline3Id,TeacherDiscipline4Id,TeacherDiscipline5Id,TeacherDiscipline6Id,TeacherDiscipline7Id,TypeControl,GroupId,GroupName,GroupIdDecanate,ProfileId,FacultyId,Course,Semester,DateBegin,DateEnd,CreaterStatement,CurrentYear,ZET,Hours,AllTeachers")] Statement statement)
        {
            string userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");

            if (ModelState.IsValid)
            {
                statement.CreaterStatement = user.Id;
                statement.DateBegin = DateTime.Now;
                var group = db.Groups.FirstOrDefault(a => a.Id == statement.GroupId);
                var faculty = await db.Faculties.FirstOrDefaultAsync(a => a.Id == statement.FacultyId);
                if (group != null && faculty != null)
                {
                    statement.GroupIdDecanate = Convert.ToInt32(group.DecanatID);
                    statement.ProfileId = group.ProfileId;
                    statement.FacultyId = group.FacultyId;
                    statement.FacultyName = faculty.AliasFullName;
                }
                //определение учебного года и названия группы
                //1. получение курса из названия группы
                int currentCourse = Convert.ToInt32(group.Name.Substring(group.Name.Length - 2, 1));
                //2. сравниваем совпадают ли введенный курс и курс из названия группы
                if (currentCourse == statement.Course)
                {
                    statement.CurrentYear = currentYear.Value;
                    statement.GroupName = group.Name;
                }
                //если не совпали, то определяем правильный учебный год
                else
                {
                    int delta = currentCourse - statement.Course;
                    int y = Convert.ToInt32(currentYear.Value.Substring(2, 2)) - delta;
                    int y1 = Convert.ToInt32(currentYear.Value.Substring(7)) - delta;
                    statement.CurrentYear = String.Format($"20{y}-20{y1}");

                    statement.GroupName = group.Name.Substring(0, group.Name.Length - 2) + statement.Course + group.Name.Substring(group.Name.Length - 1, 1);
                }

                //перезаписываем даты, для исключения влияния времени на закрытие ведомости
                statement.DateBegin = (statement.DateBegin != null) ? statement.DateBegin?.Date : null;
                statement.DateEnd = statement.DateEnd.Date.AddDays(1).AddSeconds(-1);
                
                db.Statements.Add(statement);

                //заполняем ведомость студентами группы
                var lstStudentGroup = db.Users.Where(a => a.GroupId == statement.GroupId && a.DateBlocked == null)
                    .OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).ThenBy(a => a.Middlename).ToList();
                foreach (var s in lstStudentGroup)
                {
                    var student = new StatementStudent
                    {
                        GroupIdSite = Convert.ToInt32(s.GroupId),
                        GroupIdDecanate = Convert.ToInt32(s.idGroupDecanat),
                        StudentStatement = s,
                        IdStudentDecanate = Convert.ToInt32(s.DecanatId),
                        StatementId = statement.Id,
                    };
                    db.StatementStudents.Add(student);
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.TypeControl = new SelectList(TypeListItems, "Text", "Value", statement.TypeControl);
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", statement.FacultyId);
            ViewBag.GroupId = new SelectList(db.Groups.Where(a => a.FacultyId == user.FacultyId && a.IsDeleted == false &&
                a.AcademicYear == currentYear.Value).OrderBy(a => a.Name), "Id", "Name", statement.GroupId);
            ViewBag.ProfileId = new SelectList(db.Profiles, "Id", "Name", statement.ProfileId);
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");

            List<TutorsList> teachersList;
            //получаем список ид кафедр на факультете
            var departmentsIds = db.Departments.Where(a => a.FacultyId == statement.FacultyId && a.IsDeleted == false).Select(a => a.Id).ToList();
            //получаем список преподавателей кафедр
            teachersList = db.TeacherDepartments.Where(a => departmentsIds.Contains(a.DepartmentId)).Include(a => a.Teacher).OrderBy(a => a.Teacher.Lastname)
                .ThenBy(a => a.Teacher.Firstname).ThenBy(a => a.Teacher.Middlename).Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." }).ToList();

            //добавляем преподавателя для перезачетов
            var advTeacher = db.Users.FirstOrDefault(a => a.FacultyId == user.FacultyId && a.Lastname == "Перезачет");
            if (advTeacher != null)
                teachersList.Insert(0, new TutorsList { Id = advTeacher.Id, Name = advTeacher.Lastname + " " + advTeacher.Firstname.Substring(0, 1) + "." + advTeacher.Middlename.Substring(0, 1) + "." });

            var teachersAdvList = db.TeacherDepartments.Include(a => a.Teacher).OrderBy(a => a.Teacher.Lastname)
                .ThenBy(a => a.Teacher.Firstname).ThenBy(a => a.Teacher.Middlename)
                .Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." }).ToList();
            ViewBag.TeacherDisciplineId = new SelectList(teachersList, "Id", "Name", statement.TeacherDisciplineId);
            ViewBag.TeacherDiscipline2Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline2Id);
            ViewBag.TeacherDiscipline3Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline3Id);
            ViewBag.TeacherDiscipline4Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline4Id);
            ViewBag.TeacherDiscipline5Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline5Id);
            ViewBag.TeacherDiscipline6Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline6Id);
            ViewBag.TeacherDiscipline7Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline7Id);

            return View(statement);
        }

        // GET: Statements2/Edit/5
        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public async Task<ActionResult> Edit(int? id)
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");

            if (id == null && user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Statement statement;
            if (User.IsInRole("FacultiesManagers"))
                statement = await db.Statements.Include(a => a.Faculty).Where(a => a.FacultyId == user.FacultyId).SingleOrDefaultAsync(m => m.Id == id);
            else if (User.IsInRole("Administrators"))
                statement = await db.Statements.Include(a => a.Faculty).FirstOrDefaultAsync(a => a.Id == id);
            else statement = null;

            if (statement == null && statement.ParentId != null)
            {
                return HttpNotFound();
            }

            ViewBag.TypeControl = new SelectList(TypeListItems, "Text", "Value", statement.TypeControl);
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", statement.FacultyId);
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", statement.DepartmentId);
            var teacher = db.Users.Find(statement.TeacherDisciplineId);
            if (teacher != null)
            {
                List<TutorsList> teachersList;
                //получаем список преподавателей кафедр
                teachersList = db.TeacherDepartments.Where(a => a.DepartmentId == statement.DepartmentId).Include(a => a.Teacher).OrderBy(a => a.Teacher.Lastname)
                    .ThenBy(a => a.Teacher.Firstname).ThenBy(a => a.Teacher.Middlename).Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." }).ToList();

                var teachersAdvList = db.TeacherDepartments.Include(a => a.Teacher).OrderBy(a => a.Teacher.Lastname)
                .ThenBy(a => a.Teacher.Firstname).ThenBy(a => a.Teacher.Middlename)
                .Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." }).ToList();

                //добавляем преподавателя для перезачетов
                var advTeacher = db.Users.FirstOrDefault(a => a.FacultyId == user.FacultyId && a.Lastname == "Перезачет");
                if (advTeacher != null)
                    teachersList.Insert(0, new TutorsList { Id = advTeacher.Id, Name = advTeacher.Lastname + " " + advTeacher.Firstname.Substring(0, 1) + "." + advTeacher.Middlename.Substring(0, 1) + "." });

                ViewBag.TeacherDisciplineId = new SelectList(teachersList, "Id", "Name", teacher.Id);
                ViewBag.TeacherDiscipline2Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline2Id);
                ViewBag.TeacherDiscipline3Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline3Id);
                ViewBag.TeacherDiscipline4Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline4Id);
                ViewBag.TeacherDiscipline5Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline5Id);
                ViewBag.TeacherDiscipline6Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline6Id);
                ViewBag.TeacherDiscipline7Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline7Id);
            }
            else
            {
                List<TutorsList> teachersList;
                //получаем список преподавателей кафедр
                //получаем список ид кафедр на факультете
                var departmentsIds = db.Departments.Where(a => a.FacultyId == statement.FacultyId && a.IsDeleted == false).Select(a => a.Id).ToList();
                //получаем список преподавателей кафедр
                teachersList = db.TeacherDepartments.Where(a => departmentsIds.Contains(a.DepartmentId)).Include(a => a.Teacher).OrderBy(a => a.Teacher.Lastname)
                    .ThenBy(a => a.Teacher.Firstname).ThenBy(a => a.Teacher.Middlename).Select(a => new TutorsList
                    {
                        Id = a.TeacherId,
                        Name = a.Teacher.Lastname
                    + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "."
                    }).ToList();

                var teachersAdvList = db.TeacherDepartments.Include(a => a.Teacher).OrderBy(a => a.Teacher.Lastname)
                .ThenBy(a => a.Teacher.Firstname).ThenBy(a => a.Teacher.Middlename)
                .Select(a => new TutorsList
                {
                    Id = a.TeacherId,
                    Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1)
                + "." + a.Teacher.Middlename.Substring(0, 1) + "."
                }).ToList();

                //добавляем преподавателя для перезачетов
                var advTeacher = db.Users.FirstOrDefault(a => a.FacultyId == user.FacultyId && a.Lastname == "Перезачет");
                if (advTeacher != null)
                    teachersList.Insert(0, new TutorsList { Id = advTeacher.Id, Name = advTeacher.Lastname + " " + advTeacher.Firstname.Substring(0, 1) + "." + advTeacher.Middlename.Substring(0, 1) + "." });

                ViewBag.TeacherDisciplineId = new SelectList(teachersList, "Id", "Name", statement.TeacherDisciplineId);
                ViewBag.TeacherDiscipline2Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline2Id);
                ViewBag.TeacherDiscipline3Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline3Id);
                ViewBag.TeacherDiscipline4Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline4Id);
                ViewBag.TeacherDiscipline5Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline5Id);
                ViewBag.TeacherDiscipline6Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline6Id);
                ViewBag.TeacherDiscipline7Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline7Id);
            }
            ViewBag.GroupId = new SelectList(db.Groups.Where(a => a.FacultyId == statement.FacultyId && a.IsDeleted == false &&
                a.AcademicYear == currentYear.Value).OrderBy(a => a.Name), "Id", "Name", statement.GroupId);
            ViewBag.ProfileId = new SelectList(db.Profiles, "Id", "Name", statement.ProfileId);

            return View(statement);
        }

        // POST: Statements2/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Number,NameDiscipline,DepartmentId,TeacherDisciplineId,TeacherDiscipline2Id,TeacherDiscipline3Id,TeacherDiscipline4Id,TeacherDiscipline5Id,TeacherDiscipline6Id,TeacherDiscipline7Id,TypeControl,GroupId,GroupName,GroupIdDecanate,ProfileId,FacultyId,Course,Semester,DateBegin,DateEnd,CreaterStatement,CurrentYear,ZET,Hours,AllTeachers")] Statement statement)
        {
            string userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");

            if (ModelState.IsValid)
            {
                statement.CreaterStatement = user.Id;
                var group = db.Groups.FirstOrDefault(a => a.Id == statement.GroupId);
                var faculty = await db.Faculties.FirstOrDefaultAsync(a => a.Id == statement.FacultyId);

                if (group != null && faculty != null)
                {
                    statement.GroupIdDecanate = Convert.ToInt32(group.DecanatID);
                    statement.ProfileId = group.ProfileId;
                    statement.FacultyId = group.FacultyId;
                    statement.FacultyName = faculty.AliasFullName;
                }

                //определение учебного года и названия группы
                //1. получение курса из названия группы
                int currentCourse = Convert.ToInt32(group.Name.Substring(group.Name.Length - 2, 1));
                //2. сравниваем совпадают ли введенный курс и курс из названия группы
                if (currentCourse == statement.Course)
                {
                    statement.CurrentYear = currentYear.Value;
                    statement.GroupName = group.Name;
                }
                //если не совпали, то определяем правильный учебный год
                else
                {
                    int delta = currentCourse - statement.Course;
                    int y = Convert.ToInt32(currentYear.Value.Substring(2, 2)) - delta;
                    int y1 = Convert.ToInt32(currentYear.Value.Substring(7)) - delta;
                    statement.CurrentYear = String.Format($"20{y}-20{y1}");

                    statement.GroupName = group.Name.Substring(0, group.Name.Length - 2) + statement.Course + group.Name.Substring(group.Name.Length - 1, 1);
                }

                //3. обновляем подписи преподавателей внутри ведомости, если его поменяли при редактировании
                if (statement.TeacherDisciplineId != null)
                {
                    var statStuds = db.StatementStudents.Where(a => a.StatementId == statement.Id && a.TeacherStatementId != null).ToList();
                    var doublesTeachers = statStuds.Select(a => a.TeacherStatementId).Distinct().ToList();
                    if (doublesTeachers.Count == 1 && doublesTeachers[0] != statement.TeacherDisciplineId)
                    {
                        foreach (var ss in statStuds)
                        {
                            ss.TeacherStatementId = statement.TeacherDisciplineId;
                            db.Entry(ss).State = EntityState.Modified;
                        }
                    }
                }

                db.Entry(statement).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.TypeControl = new SelectList(TypeListItems, "Text", "Value", statement.TypeControl);
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", statement.FacultyId);
            ViewBag.GroupId = new SelectList(db.Groups.Where(a => a.FacultyId == user.FacultyId && a.IsDeleted == false &&
                a.AcademicYear == currentYear.Value).OrderBy(a => a.Name), "Id", "Name", statement.GroupId);
            ViewBag.ProfileId = new SelectList(db.Profiles.Where(a => a.IsDeleted == false), "Id", "Name", statement.ProfileId);
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", statement.DepartmentId);

            List<TutorsList> teachersList;
            //получаем список преподавателей кафедр
            //получаем список ид кафедр на факультете
            var departmentsIds = db.Departments.Where(a => a.FacultyId == statement.FacultyId && a.IsDeleted == false).Select(a => a.Id).ToList();
            //получаем список преподавателей кафедр
            teachersList = db.TeacherDepartments.Where(a => departmentsIds.Contains(a.DepartmentId)).Include(a => a.Teacher).OrderBy(a => a.Teacher.Lastname)
                .ThenBy(a => a.Teacher.Firstname).ThenBy(a => a.Teacher.Middlename).Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." }).ToList();
            var teachersAdvList = db.TeacherDepartments.Include(a => a.Teacher).OrderBy(a => a.Teacher.Lastname)
                .ThenBy(a => a.Teacher.Firstname).ThenBy(a => a.Teacher.Middlename)
                .Select(a => new TutorsList
                {
                    Id = a.TeacherId,
                    Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1)
                + "." + a.Teacher.Middlename.Substring(0, 1) + "."
                }).ToList();
            //добавляем преподавателя для перезачетов
            var advTeacher = db.Users.FirstOrDefault(a => a.FacultyId == user.FacultyId && a.Lastname == "Перезачет");
            if (advTeacher != null)
                teachersList.Insert(0, new TutorsList { Id = advTeacher.Id, Name = advTeacher.Lastname + " " + advTeacher.Firstname.Substring(0, 1) + "." + advTeacher.Middlename.Substring(0, 1) + "." });

            ViewBag.TeacherDisciplineId = new SelectList(teachersList, "Id", "Name", statement.TeacherDisciplineId);
            ViewBag.TeacherDiscipline2Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline2Id);
            ViewBag.TeacherDiscipline3Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline3Id);
            ViewBag.TeacherDiscipline4Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline4Id);
            ViewBag.TeacherDiscipline5Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline5Id);
            ViewBag.TeacherDiscipline6Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline6Id);
            ViewBag.TeacherDiscipline7Id = new SelectList(teachersAdvList, "Id", "Name", statement.TeacherDiscipline7Id);

            return View(statement);
        }

        // GET: Statements2/Delete/5
        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public async Task<ActionResult> Delete(int? id)
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);

            if (id == null && user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Statement statement;
            if (User.IsInRole("FacultiesManagers"))
            {
                statement = statement = await db.Statements.Where(a => a.FacultyId == user.FacultyId)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("Administrators"))
            {
                statement = await db.Statements.FindAsync(id);
            }
            else statement = null;

            if (statement == null)
            {
                return HttpNotFound();
            }
            return View(statement);
        }

        // POST: Statements2/Delete/5
        [Authorize(Roles = "Administrators,FacultiesManagers")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            string userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);

            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Statement statement;
            List<Statement> statementsChilds = new List<Statement>();
            if (User.IsInRole("FacultiesManagers"))
            {
                //поиск дочерних ведомостей
                statementsChilds.AddRange(db.Statements
                    .Where(a => a.FacultyId == user.FacultyId && a.ParentId == id));

                //поиск групповой ведомости
                statement = await db.Statements
                    .Where(a => a.FacultyId == user.FacultyId)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("Administrators"))
            {
                //поиск дочерних ведомостей
                statementsChilds.AddRange(db.Statements.Where(m => m.ParentId == id));

                //поиск групповой ведомости
                statement = await db.Statements.FindAsync(id);
            }
            else return HttpNotFound();

            //удаление дочерних ведомостей
            var lstIdStatements = statementsChilds.Select(a => a.Id).ToList();
            var statStuds = db.StatementStudents.Where(a => lstIdStatements.Contains((int)a.StatementId)).ToList();
            db.StatementStudents.RemoveRange(statStuds);
            db.Statements.RemoveRange(statementsChilds);
            await db.SaveChangesAsync();

            //удаление родительской ведомости
            statStuds = db.StatementStudents.Where(a => a.StatementId == statement.Id).ToList();
            db.StatementStudents.RemoveRange(statStuds);
            db.Statements.Remove(statement);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<FileResult> ExamStatementGroupPrint(int id)
        {
            //получаем ведомость
            var statement = await db.Statements
                .Include(a => a.Faculty)
                .Include(s => s.Group)
                .Include(s => s.Profile)
                .Include(s => s.TeacherDiscipline)
                .FirstOrDefaultAsync(a => a.Id == id);

            //получаем список студентов группы
            var students = await db.StatementStudents
                .Where(a => a.StatementId == id)
                .Include(a => a.StudentStatement)
                .OrderBy(a => a.StudentStatement.Lastname)
                .ThenBy(a => a.StudentStatement.Firstname)
                .ThenBy(a => a.StudentStatement.Middlename)
                .ToListAsync();

            //========== заполнение ведомости ============//
            string fileName = Server.MapPath("~/DocTemplates/StatTemplates/Exams.xlsx");
            using (var excelWorkbook = new Workbook())
            {
                excelWorkbook.LoadFromFile(fileName);
                //заполнение шапки
                var sheet = excelWorkbook.Worksheets[0];

                if (statement.GroupName.IndexOf("Z") > -1)
                {
                    sheet.Range["H4"].Style.Borders[BordersLineType.EdgeBottom].LineStyle = LineStyleType.Medium;
                }
                else
                {
                    sheet.Range["G4"].Style.Borders[BordersLineType.EdgeBottom].LineStyle = LineStyleType.Medium;
                }

                sheet.Range["K6"].Value = statement.Number;
                sheet.Range["K6"].HorizontalAlignment = HorizontalAlignType.Center;
                sheet.Range["C7"].Value = statement.Faculty.AliasFullName;
                sheet.Range["C8"].Value = statement.Profile.Name;
                sheet.Range["B9"].Value = statement.Course.ToString();
                sheet.Range["E9"].Value = statement.GroupName;
                sheet.Range["I9"].Value = statement.Semester.ToString();
                sheet.Range["K9"].Value = statement.CurrentYear;
                sheet.Range["C10"].Value = statement.NameDiscipline;
                
                string text = (statement.ZET == null) ? statement.Hours.ToString() : statement.Hours.ToString() + @" / " + statement.ZET.ToString();
                sheet.Range["L11"].Text = text;

                string teachers = "";
                if (statement.TeacherDisciplineId != null) teachers = statement.TeacherDiscipline.LastnameFM;
                if (statement.TeacherDiscipline2Id != null) teachers += ", " + statement.TeacherDiscipline2.LastnameFM;
                if (statement.TeacherDiscipline3Id != null) teachers += ", " + statement.TeacherDiscipline3.LastnameFM;
                if (statement.TeacherDiscipline4Id != null) teachers += ", " + statement.TeacherDiscipline4.LastnameFM;
                if (statement.TeacherDiscipline5Id != null) teachers += ", " + statement.TeacherDiscipline5.LastnameFM;
                if (statement.TeacherDiscipline6Id != null) teachers += ", " + statement.TeacherDiscipline6.LastnameFM;
                if (statement.TeacherDiscipline7Id != null) teachers += ", " + statement.TeacherDiscipline7.LastnameFM;
                sheet.Range["G12"].Value = teachers;

                //заполнение таблицы
                var k = 16;

                // подсчет итогов
                int totalFive = 0;
                int totalFour = 0;
                int totalThree = 0;
                int totalTwo = 0;
                int didntShow = 0;

                for (var i = 0; i < students.Count; i++)
                {
                    sheet.InsertRow(k, 1);

                    sheet.Range["A" + k].Value = (i + 1).ToString();
                    sheet.Range["A" + k].HorizontalAlignment = HorizontalAlignType.Center;
                    sheet.Range["B" + k + ":F" + k].Merge();
                    sheet.Range["B" + k].Value = students[i].StudentStatement.Lastname + " " + students[i].StudentStatement.Firstname + " " + students[i].StudentStatement.Middlename;
                    sheet.Range["G" + k].Value = students[i].StudentStatement.NumberOfRecordBook;
                    sheet.Range["G" + k].HorizontalAlignment = HorizontalAlignType.Center;
                    sheet.Range["H" + k].Value = students[i].PointSemester.ToString();
                    sheet.Range["H" + k].HorizontalAlignment = HorizontalAlignType.Center;
                    sheet.Range["I" + k].Value = students[i].PointAdvanced.ToString();
                    sheet.Range["I" + k].HorizontalAlignment = HorizontalAlignType.Center;
                    sheet.Range["J" + k].Value = students[i].PointControl.ToString();
                    sheet.Range["J" + k].HorizontalAlignment = HorizontalAlignType.Center;
                    sheet.Range["K" + k].Value = students[i].TotalPoint.ToString();
                    sheet.Range["K" + k].HorizontalAlignment = HorizontalAlignType.Center;

                    //анализ оценки
                    string grade = students[i].Grade;
                    switch (grade)
                    {
                        case "Отлично":
                            totalFive++;
                            break;
                        case "Хорошо":
                            totalFour++;
                            break;
                        case "Удовлетворительно":
                            grade = "Удовлетв.";
                            totalThree++;
                            break;
                        case "Неудовлетворительно":
                            grade = "Неудовлетв.";
                            totalTwo++;
                            break;
                        case "Не явился":
                            didntShow++;
                            break;
                    }
                    sheet.Range["L" + k].Value = grade;
                    sheet.Range["L" + k].HorizontalAlignment = HorizontalAlignType.Center;
                    sheet.Range["M" + k].Style.NumberFormat = "dd/mm/yyyy";
                    sheet.Range["M" + k].Value = (students[i].Date != null) ? students[i].Date.Value.ToShortDateString() : "";
                    sheet.Range["M" + k].HorizontalAlignment = HorizontalAlignType.Center;


                    k++;
                }

                sheet.Range["A" + (k - students.Count) + ":R" + (k - 1)].Borders[BordersLineType.EdgeBottom].LineStyle = LineStyleType.Thin;
                sheet.Range["A" + (k - students.Count) + ":R" + (k - 1)].Borders[BordersLineType.EdgeLeft].LineStyle = LineStyleType.Thin;
                sheet.Range["A" + (k - students.Count) + ":R" + (k - 1)].Borders[BordersLineType.EdgeRight].LineStyle = LineStyleType.Thin;
                //сводная по ведомости
                k += 1;
                sheet.Range["G" + k].Value = statement.Faculty.AliasBoss;
                k += 3;
                sheet.Range["E" + k].Value = totalFive.ToString();
                k++;
                sheet.Range["E" + k].Value = totalFour.ToString();
                k++;
                sheet.Range["E" + k].Value = totalThree.ToString();
                k++;
                sheet.Range["E" + k].Value = totalTwo.ToString();
                k++;
                sheet.Range["E" + k].Value = didntShow.ToString();
                k += 3;
                sheet.Range["L" + k].Value = statement.Faculty.AliasBoss;


                using (MemoryStream stream = new MemoryStream())
                {
                    excelWorkbook.SaveToStream(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", statement.GroupName.ToString() + ".xlsx");
                }
            }
        }

        public async Task<FileResult> TestStatementGroupPrint(int id)
        {
            //получаем ведомость
            var statement = await db.Statements
                .Include(a => a.Faculty)
                .Include(s => s.Group)
                .Include(s => s.Profile)
                .Include(s => s.TeacherDiscipline)
                .FirstOrDefaultAsync(a => a.Id == id);

            //получаем список студентов группы
            var students = await db.StatementStudents
                .Include(a => a.StudentStatement)
                .Where(a => a.StatementId == id)
                .OrderBy(a => a.StudentStatement.Lastname)
                .ThenBy(a => a.StudentStatement.Firstname)
                .ThenBy(a => a.StudentStatement.Middlename)
                .ToListAsync();

            //========== заполнение ведомости ============//
            string fileName = Server.MapPath("~/DocTemplates/StatTemplates/Tests.xlsx");
            using (var excelWorkbook = new Workbook())
            {

                excelWorkbook.LoadFromFile(fileName);
                //заполнение шапки
                var sheet = excelWorkbook.Worksheets[0];

                if (statement.GroupName.IndexOf("Z") > -1)
                {
                    sheet.Range["H4"].Style.Borders[BordersLineType.EdgeBottom].LineStyle = LineStyleType.Medium;
                }
                else
                {
                    sheet.Range["G4"].Style.Borders[BordersLineType.EdgeBottom].LineStyle = LineStyleType.Medium;
                }

                sheet.Range["J6"].Value = statement.Number;
                sheet.Range["C7"].Value = statement.Faculty.AliasFullName;
                sheet.Range["C8"].Value = statement.Profile.Name;
                sheet.Range["B9"].Value = statement.Course.ToString();
                sheet.Range["E9"].Value = statement.GroupName;
                sheet.Range["I9"].Value = statement.Semester.ToString();
                sheet.Range["L9"].Value = statement.CurrentYear;
                sheet.Range["C10"].Value = statement.NameDiscipline;

                string text = (statement.ZET == null) ? statement.Hours.ToString() : statement.Hours.ToString() + @" / " + statement.ZET.ToString();
                sheet.Range["L11"].Text = text;

                string teachers = "";
                if (statement.TeacherDisciplineId != null) teachers = statement.TeacherDiscipline.LastnameFM;
                if (statement.TeacherDiscipline2Id != null) teachers += ", " + statement.TeacherDiscipline2.LastnameFM;
                if (statement.TeacherDiscipline3Id != null) teachers += ", " + statement.TeacherDiscipline3.LastnameFM;
                if (statement.TeacherDiscipline4Id != null) teachers += ", " + statement.TeacherDiscipline4.LastnameFM;
                if (statement.TeacherDiscipline5Id != null) teachers += ", " + statement.TeacherDiscipline5.LastnameFM;
                if (statement.TeacherDiscipline6Id != null) teachers += ", " + statement.TeacherDiscipline6.LastnameFM;
                if (statement.TeacherDiscipline7Id != null) teachers += ", " + statement.TeacherDiscipline7.LastnameFM;
                sheet.Range["G12"].Value = teachers;

                //заполнение таблицы
                var k = 16;

                // подсчет итогов
                int totalFive = 0;
                int totalFour = 0;
                int totalThree = 0;
                int totalTwo = 0;
                int didntShow = 0;
                int studied = 0;
                int notStudied = 0;

                for (var i = 0; i < students.Count; i++)
                {
                    sheet.InsertRow(k, 1);

                    sheet.Range["A" + k].Value = (i + 1).ToString();
                    sheet.Range["A" + k].HorizontalAlignment = HorizontalAlignType.Center;

                    sheet.Range["B" + k + ":F" + k].Merge();
                    sheet.Range["B" + k].Value = students[i].StudentStatement.Lastname + " " + students[i].StudentStatement.Firstname + " " + students[i].StudentStatement.Middlename;

                    sheet.Range["G" + k].Value = students[i].StudentStatement.NumberOfRecordBook;
                    sheet.Range["G" + k].HorizontalAlignment = HorizontalAlignType.Center;
                    sheet.Range["H" + k + ":J" + k].Merge();
                    sheet.Range["H" + k].Value = students[i].TotalPoint.ToString();
                    sheet.Range["H" + k].HorizontalAlignment = HorizontalAlignType.Center;
                    //анализ оценки
                    string grade = students[i].Grade;
                    switch (grade)
                    {
                        case "Отлично":
                            totalFive++;
                            break;
                        case "Хорошо":
                            totalFour++;
                            break;
                        case "Удовлетворительно":
                            grade = "Удовлетв.";
                            totalThree++;
                            break;
                        case "Неудовлетворительно":
                            grade = "Неудовлетв.";
                            totalTwo++;
                            break;
                        case "Не зачтено":
                            notStudied++;
                            break;
                        case "Зачтено":
                            studied++;
                            break;
                        case "Не явился":
                            didntShow++;
                            break;
                    }
                    sheet.Range["K" + k].Value = (students[i].Grade != "Зачтено" && students[i].Grade != "Не зачтено"
                        && students[i].Grade != "Не явился") ? students[i].GradeByNumber.ToString() : "";
                    sheet.Range["K" + k].HorizontalAlignment = HorizontalAlignType.Center;

                    sheet.Range["L" + k + ":Q" + k].Merge();
                    sheet.Range["L" + k].Value = grade;
                    sheet.Range["L" + k].HorizontalAlignment = HorizontalAlignType.Center;
                    sheet.Range["R" + k].Style.NumberFormat = "dd/mm/yyyy";
                    sheet.Range["R" + k].Value = (students[i].Date != null) ? students[i].Date.Value.ToShortDateString() : "";
                    sheet.Range["R" + k].HorizontalAlignment = HorizontalAlignType.Center;


                    k++;
                }
                sheet.Range["A" + (k - students.Count) + ":S" + (k - 1)].Borders[BordersLineType.EdgeBottom].LineStyle = LineStyleType.Thin;
                sheet.Range["A" + (k - students.Count) + ":S" + (k - 1)].Borders[BordersLineType.EdgeLeft].LineStyle = LineStyleType.Thin;
                sheet.Range["A" + (k - students.Count) + ":S" + (k - 1)].Borders[BordersLineType.EdgeRight].LineStyle = LineStyleType.Thin;

                //сводная по ведомости
                k += 1;
                sheet.Range["G" + k].Value = statement.Faculty.AliasBoss;
                k += 3;
                sheet.Range["E" + k].Value = studied.ToString();
                k++;
                sheet.Range["E" + k].Value = notStudied.ToString();
                k++;
                sheet.Range["E" + k].Value = didntShow.ToString();
                k++;
                sheet.Range["E" + k].Value = totalFive.ToString();
                k++;
                sheet.Range["E" + k].Value = totalFour.ToString();
                k++;
                sheet.Range["E" + k].Value = totalThree.ToString();
                k++;
                sheet.Range["E" + k].Value = totalTwo.ToString();
                k++;
                k += 1;
                sheet.Range["Q" + k].Value = statement.Faculty.AliasBoss;


                using (MemoryStream stream = new MemoryStream())
                {
                    excelWorkbook.SaveToStream(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", statement.GroupName.ToString() + ".xlsx");
                }
            }
        }

        public async Task<FileResult> StatementIndividualPrint(int id)
        {
            //получаем ведомость
            var statement = await db.Statements
                .Include(a => a.Faculty)
                .Include(s => s.Group)
                .Include(s => s.Profile)
                .Include(s => s.TeacherDiscipline)
                .FirstOrDefaultAsync(a => a.Id == id);

            //получаем список студентов группы
            var students = await db.StatementStudents
                .Include(a => a.StudentStatement)
                .Where(a => a.StatementId == id && a.StudentStatement.DateBlocked == null)
                .OrderBy(a => a.StudentStatement.Lastname)
                .ThenBy(a => a.StudentStatement.Firstname)
                .ThenBy(a => a.StudentStatement.Middlename)
                .ToListAsync();

            //========== заполнение ведомости ============//
            string fileName = Server.MapPath("~/DocTemplates/StatTemplates/Second-Exam-test.xlsx");
            using (var excelWorkbook = new XLWorkbook(fileName))
            {
                //заполнение шапки
                var sheet = excelWorkbook.Worksheet(1);

                sheet.Cell("B9").Value = statement.Faculty.AliasFullName;

                sheet.Cell("B10").Value = statement.Course;
                sheet.Cell("E10").Value = statement.GroupName;
                //sheet.Cell("I9").Value = statement.Semester;
                //sheet.Cell("L9").Value = statement.CurrentYear;
                sheet.Cell("C11").Value = statement.NameDiscipline;
                //sheet.Cell("L11").Value = (statement.ZET == null) ? statement.Hours.ToString() : statement.Hours + " / " + statement.ZET;

                sheet.Cell("C16").Value = statement.DateEnd.ToShortDateString();
                sheet.Cell("C17").Value = statement.DateBegin.Value.ToShortDateString();

                string teachers = "";
                if (statement.TeacherDisciplineId != null) teachers = statement.TeacherDiscipline.LastnameFM;
                if (statement.TeacherDiscipline2Id != null) teachers += ", " + statement.TeacherDiscipline2.LastnameFM;
                if (statement.TeacherDiscipline3Id != null) teachers += ", " + statement.TeacherDiscipline3.LastnameFM;
                if (statement.TeacherDiscipline4Id != null) teachers += ", " + statement.TeacherDiscipline4.LastnameFM;
                if (statement.TeacherDiscipline5Id != null) teachers += ", " + statement.TeacherDiscipline5.LastnameFM;
                if (statement.TeacherDiscipline6Id != null) teachers += ", " + statement.TeacherDiscipline6.LastnameFM;
                if (statement.TeacherDiscipline7Id != null) teachers += ", " + statement.TeacherDiscipline7.LastnameFM;
                sheet.Cell("C12").Value = teachers;

                sheet.Cell("C14").Value = students[0].StudentStatement.Lastname + " " + students[0].StudentStatement.Firstname
                    + " " + students[0].StudentStatement.Middlename;

                sheet.Cell("C15").Value = students[0].StudentStatement.NumberOfRecordBook;

                sheet.Cell("B18").Value = (students[0].PointSemester != 0) ? students[0].PointSemester.ToString() : "";
                sheet.Cell("D18").Value = (students[0].PointAdvanced != 0) ? students[0].PointAdvanced.ToString() : "";
                sheet.Cell("B20").Value = (students[0].PointControl != 0) ? students[0].PointControl.ToString() : "";
                sheet.Cell("D20").Value = (students[0].TotalPoint != 0) ? students[0].TotalPoint.ToString() : "";

                //анализ оценки
                string grade = students[0].Grade;
                sheet.Cell("F20").Value = (students[0].Grade != "Зачтено" && students[0].Grade != "Не зачтено"
                    && students[0].Grade != "Не явился") ? students[0].GradeByNumber.ToString() + " / " + grade : grade;

                sheet.Cell("G22").Value = (students[0].Date != null) ? students[0].Date.Value.ToShortDateString() : "";

                sheet.Cell("C19").Value = statement.Faculty.AliasBoss;


                using (MemoryStream stream = new MemoryStream())
                {
                    excelWorkbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Statement.xlsx");
                }
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
