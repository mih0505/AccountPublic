using Accounts.Models;
using ClosedXML.Excel;
using Microsoft.AspNet.Identity;
using PagedList;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Accounts.Controllers
{
    public class JournalsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Journals
        [Authorize(Roles = "Administrators,FacultiesManagers,Teachers,TutorsManagers")]
        public async Task<ActionResult> Index(string sortOrder, int? faculty, string group, string discipline, string course, string teacher, int? page)
        {
            // Получение текущего учебного года
            var year = db.Settings.FirstOrDefault(Setting => (Setting.Name == "CurrentYear"));

            var userId = User.Identity.GetUserId();
            ViewBag.TeacherId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);

            // Создание первого (стандартного) элемента (чтобы избежать бага с фильтрацией данных)
            var defaultItem = new SelectListItem()
            {
                Value = "-1",
                Text = "Все"
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

            // Заполнение DropDownList'a с номером курса
            var coursesView = new List<SelectListItem>()
            {
                defaultItem
            };
            coursesView.AddRange(listCourses);

            // Получение списка журналов
            var listIdOfCurrentGroups = db.Groups.Where(a => a.IsDeleted == false && a.AcademicYear == year.Value);
            var listIdOfGroups = new List<int>();
            bool isTutor = false; //определяет является ли преподаватель куратором


            //получение списка журналов
            if (User.IsInRole("Administrators"))
            {
                listIdOfGroups = listIdOfCurrentGroups.Select(a => a.Id).ToList();
            }
            else if (User.IsInRole("FacultiesManagers") || User.IsInRole("Teachers"))
            {
                listIdOfGroups = listIdOfCurrentGroups
                    .Where(a => a.FacultyId == (int)user.FacultyId)
                    .Select(a => a.Id)
                    .ToList();
            }

            var journals = db.Journals
                .Include(j => j.Group)
                .Where(a => a.Group.AcademicYear == year.Value)
                .Include(j => j.TeacherName)
                .Include(j => j.Faculty);


            // В том числе вывод нужной информации для определенной роли
            //получение курируемых групп
            //получение групп куратора
            var groupsTutor = db.Tutors
                .Where(a => a.UserId == user.Id)
                .Select(a => a.GroupId)
                .ToList();
            if (groupsTutor.Count > 0)
            {
                isTutor = true;
                //ViewBag.TeacherId = userId;
            }

            if (User.IsInRole("Teachers") && !User.IsInRole("FacultiesManagers") && !isTutor)
            {
                journals = journals.Where(a => a.TeacherNameId == userId && !a.IsDeleted);
            }
            else if (User.IsInRole("Teachers") && !User.IsInRole("FacultiesManagers") && isTutor)
            {
                journals = journals.Where(a => (groupsTutor.Contains(a.GroupId) || a.TeacherNameId == userId) && !a.IsDeleted);
            }
            else
            {
                journals = journals.Where(a => listIdOfGroups.Contains(a.GroupId) && !a.IsDeleted);
            }


            // Получение Cookies
            var cookieSortOrder = Request.Cookies.Get("jour_sortOrder");
            var cookieGroup = Request.Cookies.Get("jour_group");
            var cookieTeacher = Request.Cookies.Get("jour_teacher");
            var cookieDiscipline = Request.Cookies.Get("jour_discipline");
            var cookieCourse = Request.Cookies.Get("jour_course");
            var cookieFaculty = Request.Cookies.Get("jour_faculty");

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

            if (group == null && cookieGroup != null)
            {
                group = cookieGroup.Value;
            }

            if (discipline == null && cookieDiscipline != null)
            {
                discipline = cookieDiscipline.Value;
            }

            if (course == null && cookieCourse != null)
            {
                coursesView.FirstOrDefault(Course => (Course.Value == cookieCourse.Value)).Selected = true;
                course = cookieCourse.Value;
            }

            if (teacher == null && cookieTeacher != null)
            {
                teacher = cookieTeacher.Value;
            }

            // Удаление Cookies
            Response.Cookies.Add(new HttpCookie("jour_sortOrder")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("jour_faculty")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("jour_group")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("jour_discipline")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("jour_teacher")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("jour_course")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });


            // Применение фильтров и сортировок
            if (sortOrder != null)
            {
                Response.Cookies.Add(new HttpCookie("jour_sortOrder")
                {
                    Value = sortOrder.ToString(),
                    Expires = DateTime.Now.AddMinutes(15)
                });
            }

            if (!string.IsNullOrEmpty(group))
            {
                Response.Cookies.Add(new HttpCookie("jour_group")
                {
                    Value = group,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                journals = journals.Where(p => SqlFunctions.PatIndex("%" + group + "%", p.GroupName) > 0);
            }

            if (teacher != null)
            {
                Response.Cookies.Add(new HttpCookie("jour_teacher")
                {
                    Value = teacher,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (teacher != null)
                {
                    var teach = db.Users
                        .Where(a => a.DecanatId == null && a.DateBlocked == null && SqlFunctions.PatIndex("%" + teacher + "%", a.Lastname) > 0)
                        .Select(a => a.Id)
                        .ToList();
                    journals = journals.Where(p => teach.Contains(p.TeacherNameId));
                }
            }

            if (faculty != null)
            {
                Response.Cookies.Add(new HttpCookie("jour_faculty")
                {
                    Value = faculty.ToString(),
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (faculty.Value != -1)
                {
                    journals = journals.Where(p => p.FacultyId == faculty);
                }
            }

            if (!string.IsNullOrEmpty(discipline))
            {
                Response.Cookies.Add(new HttpCookie("jour_discipline")
                {
                    Value = discipline,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                journals = journals.Where(p => SqlFunctions.PatIndex("%" + discipline + "%", p.Discipline) > 0);
            }

            if (!string.IsNullOrEmpty(course))
            {
                Response.Cookies.Add(new HttpCookie("jour_course")
                {
                    Value = course,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (course != (-1).ToString())
                {
                    int courseId = Convert.ToInt32(course);
                    var semesterSecond = courseId * 2;
                    var semesterFirst = semesterSecond - 1;
                    journals = journals.Where(p => p.Semester == semesterFirst || p.Semester == semesterSecond);
                }
            }


            // Применение сортировки
            switch (sortOrder)
            {
                case "group_desc":
                    journals = journals.OrderByDescending(s => s.GroupName);
                    break;
                case "discipline_asc":
                    journals = journals.OrderBy(s => s.Discipline);
                    break;
                case "discipline_desc":
                    journals = journals.OrderByDescending(s => s.Discipline);
                    break;
                case "course_asc":
                    journals = journals.OrderBy(s => s.Group.Course);
                    break;
                case "course_desc":
                    journals = journals.OrderByDescending(s => s.Group.Course);
                    break;
                default:
                    journals = journals.OrderBy(s => s.GroupName);
                    break;
            }

            // Обновление всех ViewBag'ов
            ViewBag.FacultyList = facultiesView;

            ViewBag.Courses = coursesView;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.GroupSortParm = sortOrder == "group_asc" ? "group_desc" : "group_asc";
            ViewBag.DisciplineSortParm = sortOrder == "discipline_asc" ? "discipline_desc" : "discipline_asc";
            ViewBag.CourseSortParm = sortOrder == "course_asc" ? "course_desc" : "course_asc";
            ViewBag.FacultyFilter = faculty;
            ViewBag.GroupFilter = group;
            ViewBag.TeacherFilter = teacher;
            ViewBag.Discipline = discipline;
            ViewBag.CourseFilter = course;

            // Вывод результата
            return View(journals.ToPagedList(page ?? 1, 30));
        }


        public ActionResult ClearFilter()
        {
            HttpCookie cookieSortOrder = Request.Cookies.Get("jour_sortOrder");
            if (cookieSortOrder != null)
            {
                cookieSortOrder.Value = null;
                cookieSortOrder.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieSortOrder);
            }

            HttpCookie cookieFaculty = Request.Cookies.Get("jour_faculty");
            if (cookieFaculty != null)
            {
                cookieFaculty.Value = null;
                cookieFaculty.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieFaculty);
            }

            HttpCookie cookieGroup = Request.Cookies.Get("jour_group");
            if (cookieGroup != null)
            {
                cookieGroup.Value = null;
                cookieGroup.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieGroup);
            }

            HttpCookie cookieDiscipline = Request.Cookies.Get("jour_discipline");
            if (cookieDiscipline != null)
            {
                cookieDiscipline.Value = null;
                cookieDiscipline.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieDiscipline);
            }

            HttpCookie cookieTeacher = Request.Cookies.Get("jour_teacher");
            if (cookieTeacher != null)
            {
                cookieTeacher.Value = null;
                cookieTeacher.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieTeacher);
            }

            HttpCookie cookieCourse = Request.Cookies.Get("jour_course");
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

        /// <summary>
        /// Получение списка преподавателей подразделения (факультета)
        /// </summary>
        /// <param name="Id">Идентификатор факультета</param>
        /// <returns>Список преподавателей факультета</returns>
        public JsonResult GetTeachers(int? Id)
        {
            var facultyId = Convert.ToInt32(Id);

            var teachersList = db.TeacherDepartments
                                 .Include(a => a.Teacher)
                                 .Include(a => a.Department)
                                 .Where(a => a.Department.FacultyId == facultyId)
                                 .OrderBy(a => a.Teacher.Lastname)
                                 .ThenBy(a => a.Teacher.Firstname)
                                 .ThenBy(a => a.Teacher.Middlename)
                                 .Select(a => new TutorsList
                                 {
                                     Id = a.TeacherId,
                                     Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "."
                                 })
                                 .ToList();

            var data = new SelectList(teachersList, "Id", "Name");

            return Json(data);
        }

        /// <summary>
        /// Получение списка групп факультета, текущего учебного года
        /// </summary>
        /// <param name="Id">Идентификатор факультета</param>
        /// <returns>Список групп факультета</returns>
        public JsonResult GetGroups(int? id)
        {
            List<GroupsList> groupsList;
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
            if (id == null)
            {
                groupsList = db.Groups.Where(a => !a.IsDeleted &&
                a.AcademicYear == currentYear.Value).OrderBy(a => a.Name).
               Select(a => new GroupsList { Id = a.Id, Name = a.Name }).ToList();
            }
            else
            {
                int facultyId = Convert.ToInt32(id);
                groupsList = db.Groups.Where(a => a.FacultyId == facultyId && !a.IsDeleted &&
                a.AcademicYear == currentYear.Value).OrderBy(a => a.Name).
               Select(a => new GroupsList { Id = a.Id, Name = a.Name }).ToList();
            }
            return Json(new SelectList(groupsList, "Id", "Name"));
        }

        [Authorize(Roles = "Students,Teachers,Administrators,FacultiesManagers")]
        public async Task<ActionResult> DisciplinesStudent(string id)
        {
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
            string userId;
            if (id == null)
            {
                userId = User.Identity.GetUserId();
            }
            else
            {
                userId = id;
            }
            var user = await db.Users.FirstOrDefaultAsync(a => a.Id == userId);
            ViewBag.UserId = userId;
            //определяем список журналов студента            
            var js = await db.Journals
                .Include(a => a.TeacherName)
                .Where(a => a.GroupId == user.GroupId && a.Year == currentYear.Value)
                .OrderBy(a => a.Discipline)
                .ThenBy(a => a.Semester)
                .ToListAsync();

            return View(js);
        }

        [Authorize(Roles = "Students,Administrators,FacultiesManagers,Teachers")]
        public async Task<ActionResult> DisciplineGrades(int? id, string studentId)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            string userId;
            if (studentId == null)
            {
                userId = User.Identity.GetUserId();
            }
            else
            {
                userId = studentId;
            }
            var user = await db.Users.FirstOrDefaultAsync(a => a.Id == userId);

            //получаем список оценок студента в журнале
            var listGrades = await db.Studies
                .Include(a => a.Lesson)
                .Where(a => a.JournalId == id && a.StudentId == user.Id)
                .OrderBy(a => a.Lesson.Date)
                .ToListAsync();

            if (listGrades != null && listGrades.Count > 0)
            {
                var journalId = listGrades[0].JournalId;
                ViewBag.Discipline = db.Journals.FirstOrDefault(a => a.Id == journalId).Discipline;
            }
            else
            {
                ViewBag.Discipline = "Данные по дисциплине отсутствуют";
            }

            return View(listGrades);
        }

        // GET: Journals/Details/5
        [Authorize(Roles = "Administrators,FacultiesManagers,Teachers")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //получаем данные о журнале если есть права на него
            JournalFull journal = new JournalFull();
            var userId = User.Identity.GetUserId();
            if (User.IsInRole("Teachers") && !User.IsInRole("FacultiesManagers"))
            {
                //получение групп куратора
                var groupsTutor = db.Tutors
                    .Where(a => a.UserId == userId)
                    .Select(a => a.GroupId)
                    .ToList();

                if (groupsTutor.Count > 0)
                {
                    journal.Journal = await db.Journals
                  .Include(j => j.Group)
                  .Include(j => j.Faculty)
                  .Where(a => groupsTutor.Contains(a.GroupId) || a.TeacherNameId == userId)
                  .SingleOrDefaultAsync(m => m.Id == id);
                }
                else
                {
                    journal.Journal = await db.Journals
                  .Include(j => j.Group)
                  .Include(j => j.Faculty)
                  .Where(a => a.TeacherNameId == userId)
                  .SingleOrDefaultAsync(m => m.Id == id);
                }
            }
            else if (User.IsInRole("FacultiesManagers"))
            {
                var user = db.Users.Find(userId);
                journal.Journal = await db.Journals
                  .Include(j => j.Group)
                  .Include(j => j.Faculty)
                  .Where(a => a.FacultyId == user.FacultyId)
                  .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("Administrators"))
            {
                journal.Journal = await db.Journals
                    .Include(j => j.Group)
                    .Include(j => j.Faculty)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else
            {
                return HttpNotFound();
            }

            //проверка существования журнала
            if (journal == null)
            {
                return HttpNotFound();
            }

            //список студентов группы
            var studentsGroup = await db.Users
                .Where(a => a.GroupId == journal.Journal.GroupId && a.DateBlocked == null)
                .OrderBy(a => a.Lastname)
                .ThenBy(a => a.Firstname)
                .ThenBy(a => a.Middlename)
                .ToListAsync();

            //список занятий
            var listLessons = db.Lessons
                .Where(a => a.JournalId == journal.Journal.Id)
                .OrderBy(a => a.Date)
                .ToList();

            //заполнение строк столбцов № и ФИО с шапкой
            journal.Grades = new string[studentsGroup.Count + 2, listLessons.Count + 6];
            for (int i = 0; i < journal.Grades.GetLength(0); i++)
            {
                if (i > 1)
                {
                    journal.Grades[i, 0] = studentsGroup[i - 2].Id;
                    journal.Grades[i, 1] = (i - 1).ToString();
                    journal.Grades[i, 2] = String.Format($"{studentsGroup[i - 2].Lastname} {studentsGroup[i - 2].Firstname} {studentsGroup[i - 2].Middlename}");
                }
                else if (i == 0)
                {
                    journal.Grades[i, 0] = "id";
                    journal.Grades[i, 1] = "№";
                    journal.Grades[i, 2] = "ФИО";
                    journal.Grades[i, journal.Grades.GetLength(1) - 3] = "Пропуски";
                    journal.Grades[i, journal.Grades.GetLength(1) - 2] = "Среднее";
                    journal.Grades[i, journal.Grades.GetLength(1) - 1] = "Сумма";
                }
            }

            //заполнение списка занятий в шапке 
            for (int i = 3; i < journal.Grades.GetLength(1) - 3; i++)
            {
                journal.Grades[0, i] = (listLessons[i - 3].TypeLesson == "Итоговая оценка") ? "Итог" : String.Format($"{listLessons[i - 3].Date.ToString("dd.MM")}");
                journal.Grades[1, i] = listLessons[i - 3].Id.ToString();
            }

            //заполнение массива оценками
            var listGrades = await db.Studies
                                     .Where(a => a.JournalId == journal.Journal.Id)
                                     .ToListAsync();

            foreach (var grade in listGrades)
            {
                bool ok = false;
                int b = journal.Grades.GetLength(0);
                for (int i = 2; i < b; i++)
                {
                    if (journal.Grades[i, 0] == grade.StudentId)
                    {
                        int a = journal.Grades.GetLength(1);
                        for (int j = 3; j < a - 3; j++)
                        {
                            if (int.Parse(journal.Grades[1, j]) == grade.LessonId)
                            {
                                if (!string.IsNullOrEmpty(grade.Grade1))
                                {
                                    journal.Grades[i, j] = grade.Grade1;
                                }

                                if (!string.IsNullOrEmpty(grade.Grade2))
                                {
                                    journal.Grades[i, j] += @"/" + grade.Grade2;
                                }
                                if (!string.IsNullOrEmpty(grade.Grade3))
                                {
                                    journal.Grades[i, j] += @"/" + grade.Grade3;
                                }
                                ok = true;
                            }
                            if (ok) break;
                        }
                    }
                    if (ok) break;
                }
            }

            //расчет итогов
            for (int i = 2; i < journal.Grades.GetLength(0); i++)
            {
                int sumGrades = 0, absence = 0;
                double countGrades = 0;

                for (int j = 3; j < journal.Grades.GetLength(1) - 3; j++)
                {
                    if (journal.Grades[i, j] != null)
                    {
                        if (journal.Grades[i, j].IndexOf('/') == -1)
                        {
                            if (journal.Grades[i, j] != "н" && journal.Grades[i, j] != "б"
                                && journal.Grades[i, j] != "зачтено" && journal.Grades[i, j] != "не зачтено")
                            {
                                sumGrades += int.Parse(journal.Grades[i, j]);
                                countGrades++;
                            }
                            else
                            {
                                absence++;
                            }
                        }
                        else
                        {
                            string[] g = journal.Grades[i, j].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var s in g)
                            {
                                if (s != "н" && s != "б" && s != "зачтено" && s != "не зачтено")
                                {
                                    sumGrades += int.Parse(s);
                                    countGrades++;
                                }
                            }
                        }
                    }
                    journal.Grades[i, journal.Grades.GetLength(1) - 3] = absence.ToString();
                    journal.Grades[i, journal.Grades.GetLength(1) - 2] = (countGrades != 0) ? string.Format("{0:f2}", ((double)sumGrades / countGrades)) : "0";
                    journal.Grades[i, journal.Grades.GetLength(1) - 1] = sumGrades.ToString();
                }
            }
            return View(journal);
        }

        [Authorize(Roles = "Administrators,FacultiesManagers,Teachers")]
        public async Task<ActionResult> JournalPrint(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //получаем данные о журнале если есть права на него
            JournalFull journal = new JournalFull();
            var userId = User.Identity.GetUserId();
            if (User.IsInRole("Teachers") && !User.IsInRole("FacultiesManagers"))
            {
                //получение групп куратора
                var groupsTutor = db.Tutors
                    .Where(a => a.UserId == userId)
                    .Select(a => a.GroupId)
                    .ToList();

                if (groupsTutor.Count > 0)
                {
                    journal.Journal = await db.Journals
                  .Include(j => j.Group)
                  .Include(j => j.Faculty)
                  .Where(a => groupsTutor.Contains(a.GroupId) || a.TeacherNameId == userId)
                  .SingleOrDefaultAsync(m => m.Id == id);
                }
                else
                {
                    journal.Journal = await db.Journals
                  .Include(j => j.Group)
                  .Include(j => j.Faculty)
                  .Where(a => a.TeacherNameId == userId)
                  .SingleOrDefaultAsync(m => m.Id == id);
                }
            }
            else if (User.IsInRole("FacultiesManagers"))
            {
                var user = db.Users.Find(userId);
                journal.Journal = await db.Journals
                  .Include(j => j.Group)
                  .Include(j => j.Faculty)
                  .Where(a => a.FacultyId == user.FacultyId)
                  .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("Administrators"))
            {
                journal.Journal = await db.Journals
                    .Include(j => j.Group)
                    .Include(j => j.Faculty)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else
            {
                return HttpNotFound();
            }

            //проверка существования журнала
            if (journal == null)
            {
                return HttpNotFound();
            }

            //список студентов группы
            var studentsGroup = await db.Users
                .Where(a => a.GroupId == journal.Journal.GroupId && a.DateBlocked == null)
                .OrderBy(a => a.Lastname)
                .ThenBy(a => a.Firstname)
                .ThenBy(a => a.Middlename)
                .ToListAsync();

            //список занятий
            var listLessons = db.Lessons
                .Where(a => a.JournalId == journal.Journal.Id)
                .OrderBy(a => a.Date)
                .ToList();

            //заполнение строк столбцов № и ФИО с шапкой
            journal.Grades = new string[studentsGroup.Count + 2, listLessons.Count + 6];
            for (int i = 0; i < journal.Grades.GetLength(0); i++)
            {
                if (i > 1)
                {
                    journal.Grades[i, 0] = studentsGroup[i - 2].Id;
                    journal.Grades[i, 1] = (i - 1).ToString();
                    journal.Grades[i, 2] = String.Format($"{studentsGroup[i - 2].Lastname} {studentsGroup[i - 2].Firstname} {studentsGroup[i - 2].Middlename}");
                }
                else if (i == 0)
                {
                    journal.Grades[i, 0] = "id";
                    journal.Grades[i, 1] = "№";
                    journal.Grades[i, 2] = "ФИО";
                    journal.Grades[i, journal.Grades.GetLength(1) - 3] = "Пропуски";
                    journal.Grades[i, journal.Grades.GetLength(1) - 2] = "Среднее";
                    journal.Grades[i, journal.Grades.GetLength(1) - 1] = "Сумма";
                }
            }

            //заполнение списка занятий в шапке 
            for (int i = 3; i < journal.Grades.GetLength(1) - 3; i++)
            {
                journal.Grades[0, i] = (listLessons[i - 3].TypeLesson == "Итоговая оценка") ? "Итог" : String.Format($"{listLessons[i - 3].Date.ToString("dd.MM")}");
                journal.Grades[1, i] = listLessons[i - 3].Id.ToString();
            }

            //заполнение массива оценками
            var listGrades = await db.Studies
                                     .Where(a => a.JournalId == journal.Journal.Id)
                                     .ToListAsync();

            foreach (var grade in listGrades)
            {
                bool ok = false;
                int b = journal.Grades.GetLength(0);
                for (int i = 2; i < b; i++)
                {
                    if (journal.Grades[i, 0] == grade.StudentId)
                    {
                        int a = journal.Grades.GetLength(1);
                        for (int j = 3; j < a - 3; j++)
                        {
                            if (int.Parse(journal.Grades[1, j]) == grade.LessonId)
                            {
                                if (!string.IsNullOrEmpty(grade.Grade1))
                                {
                                    journal.Grades[i, j] = grade.Grade1;
                                }

                                if (!string.IsNullOrEmpty(grade.Grade2))
                                {
                                    journal.Grades[i, j] += @"/" + grade.Grade2;
                                }
                                if (!string.IsNullOrEmpty(grade.Grade3))
                                {
                                    journal.Grades[i, j] += @"/" + grade.Grade3;
                                }
                                ok = true;
                            }
                            if (ok) break;
                        }
                    }
                    if (ok) break;
                }
            }

            //расчет итогов
            for (int i = 2; i < journal.Grades.GetLength(0); i++)
            {
                int sumGrades = 0, absence = 0;
                double countGrades = 0;

                for (int j = 3; j < journal.Grades.GetLength(1) - 3; j++)
                {
                    if (journal.Grades[i, j] != null)
                    {
                        if (journal.Grades[i, j].IndexOf('/') == -1)
                        {
                            if (journal.Grades[i, j] != "н" && journal.Grades[i, j] != "б"
                                && journal.Grades[i, j] != "зачтено" && journal.Grades[i, j] != "не зачтено")
                            {
                                sumGrades += int.Parse(journal.Grades[i, j]);
                                countGrades++;
                            }
                            else
                            {
                                absence++;
                            }
                        }
                        else
                        {
                            string[] g = journal.Grades[i, j].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var s in g)
                            {
                                if (s != "н" && s != "б" && s != "зачтено" && s != "не зачтено")
                                {
                                    sumGrades += int.Parse(s);
                                    countGrades++;
                                }
                            }
                        }
                    }
                    journal.Grades[i, journal.Grades.GetLength(1) - 3] = absence.ToString();
                    journal.Grades[i, journal.Grades.GetLength(1) - 2] = (countGrades != 0) ? string.Format("{0:f2}", ((double)sumGrades / countGrades)) : "0";
                    journal.Grades[i, journal.Grades.GetLength(1) - 1] = sumGrades.ToString();
                }
            }


            using (var workbook = new Workbook())
            {
                workbook.CreateEmptySheets(1);
                Worksheet sheet = workbook.Worksheets[0];

                int b = journal.Grades.GetLength(0);
                for (int i = 0; i < b; i++)
                {
                    if (i != 1)
                    {
                        int a = journal.Grades.GetLength(1);
                        for (int j = 1; j < a; j++)
                        {
                            string value = journal.Grades[i, j] ?? "";
                            if (value.IndexOf('/') > -1)
                            {
                                sheet.Range[i + 1, j + 1].Text = value;
                            }
                            else
                            {
                                sheet.Range[i + 1, j + 1].Value = value;
                            }
                        }
                    }
                    //else
                    //{
                    //    if(i > 2)
                    //        sheet.Range[i + 1, j + 1].Value = j;
                    //}
                }
                sheet.AllocatedRange.AutoFitColumns();


                using (var stream = new MemoryStream())
                {
                    workbook.SaveToStream(stream, FileFormat.Version2010);
                    var bytes = stream.ToArray();
                    var type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return File(bytes, type, $"{journal.Journal.GroupName} - {journal.Journal.Discipline}.xlsx");
                }
            }
        }

        // GET: Journals/Create
        [Authorize(Roles = "Administrators,FacultiesManagers,Teachers")]
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");

            var journal = new Journal()
            {
                Year = currentYear.Value
            };

            if (User.IsInRole("Administrators"))
            {
                var facultiesList = db.Faculties
                                      .Where(a => !a.IsDeleted)
                                      .OrderBy(a => a.Name);

                ViewBag.FacultyId = new SelectList(facultiesList, "Id", "Name", user.FacultyId);

                var groupsList = db.Groups
                                   .Where(a => !a.IsDeleted && a.AcademicYear == currentYear.Value)
                                   .OrderBy(a => a.Name);

                ViewBag.GroupId = new SelectList(groupsList, "Id", "Name");

                var teachersList = db.TeacherDepartments
                                 .Include(a => a.Teacher)
                                 .Include(a => a.Department)
                                 .Where(a => a.Department.FacultyId == user.FacultyId)
                                 .OrderBy(a => a.Teacher.Lastname)
                                 .ThenBy(a => a.Teacher.Firstname)
                                 .ThenBy(a => a.Teacher.Middlename)
                                 .Select(a => new TutorsList
                                 {
                                     Id = a.TeacherId,
                                     Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "."
                                 })
                                 .ToList();

                ViewBag.TeacherNameId = new SelectList(teachersList, "Id", "Name");
            }
            else if (User.IsInRole("FacultiesManagers"))
            {
                journal.FacultyId = Convert.ToInt32(user.FacultyId);

                var groupsList = db.Groups
                                   .Where(a => a.FacultyId == journal.FacultyId && !a.IsDeleted && a.AcademicYear == currentYear.Value)
                                   .OrderBy(a => a.Name);

                ViewBag.GroupId = new SelectList(groupsList, "Id", "Name");

                var teachersList = db.TeacherDepartments
                                 .Include(a => a.Teacher)
                                 .Include(a => a.Department)
                                 .Where(a => a.Department.FacultyId == user.FacultyId)
                                 .OrderBy(a => a.Teacher.Lastname)
                                 .ThenBy(a => a.Teacher.Firstname)
                                 .ThenBy(a => a.Teacher.Middlename)
                                 .Select(a => new TutorsList
                                 {
                                     Id = a.TeacherId,
                                     Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "."
                                 })
                                 .ToList();

                ViewBag.TeacherNameId = new SelectList(teachersList, "Id", "Name");
            }
            else
            {
                journal.FacultyId = Convert.ToInt32(user.FacultyId);
                journal.TeacherNameId = user.Id;

                var groupsList = db.Groups
                                   .Where(a => a.FacultyId == journal.FacultyId && !a.IsDeleted && a.AcademicYear == currentYear.Value)
                                   .OrderBy(a => a.Name);

                ViewBag.GroupId = new SelectList(groupsList, "Id", "Name");
            }

            return View(journal);
        }

        // POST: Journals/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators,FacultiesManagers,Teachers")]
        public async Task<ActionResult> Create([Bind(Include = "Id,GroupId,Discipline,Semester,Year,TypeControl,TeacherNameId,FacultyId,GroupName,GroupIdDecanat")] Journal journal)
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");

            if (ModelState.IsValid)
            {
                //отдельно сохраняем текущее название группы и ее код в деканате
                var group = db.Groups.FirstOrDefault(a => a.Id == journal.GroupId);

                if (group != null)
                {
                    journal.GroupName = group.Name;
                    journal.GroupIdDecanat = (int)group.DecanatID;
                    journal.Year = currentYear.Value;
                }

                db.Journals.Add(journal);
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            if (User.IsInRole("Administrators"))
            {
                var facultiesList = db.Faculties
                                      .Where(a => !a.IsDeleted)
                                      .OrderBy(a => a.Name);

                ViewBag.FacultyId = new SelectList(facultiesList, "Id", "Name", user.FacultyId);

                var groupsList = db.Groups
                                   .Where(a => !a.IsDeleted && a.AcademicYear == currentYear.Value)
                                   .OrderBy(a => a.Name);

                ViewBag.GroupId = new SelectList(groupsList, "Id", "Name");

                var teachersList = db.TeacherDepartments
                                 .Include(a => a.Teacher)
                                 .Include(a => a.Department)
                                 .Where(a => a.Department.FacultyId == journal.FacultyId)
                                 .OrderBy(a => a.Teacher.Lastname)
                                 .ThenBy(a => a.Teacher.Firstname)
                                 .ThenBy(a => a.Teacher.Middlename)
                                 .Select(a => new TutorsList
                                 {
                                     Id = a.TeacherId,
                                     Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "."
                                 })
                                 .ToList();

                ViewBag.TeacherNameId = new SelectList(teachersList, "Id", "Name");
            }
            else if (User.IsInRole("FacultiesManagers"))
            {
                journal.FacultyId = Convert.ToInt32(user.FacultyId);

                var groupsList = db.Groups
                                   .Where(a => a.FacultyId == journal.FacultyId && !a.IsDeleted && a.AcademicYear == currentYear.Value)
                                   .OrderBy(a => a.Name);

                ViewBag.GroupId = new SelectList(groupsList, "Id", "Name");

                var teachersList = db.TeacherDepartments
                                 .Include(a => a.Teacher)
                                 .Include(a => a.Department)
                                 .Where(a => a.Department.FacultyId == journal.FacultyId)
                                 .OrderBy(a => a.Teacher.Lastname)
                                 .ThenBy(a => a.Teacher.Firstname)
                                 .ThenBy(a => a.Teacher.Middlename)
                                 .Select(a => new TutorsList
                                 {
                                     Id = a.TeacherId,
                                     Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "."
                                 })
                                 .ToList();

                ViewBag.TeacherNameId = new SelectList(teachersList, "Id", "Name");
            }
            else
            {
                journal.FacultyId = Convert.ToInt32(user.FacultyId);
                journal.TeacherNameId = user.Id;

                var groupsList = db.Groups
                                   .Where(a => a.FacultyId == journal.FacultyId && !a.IsDeleted && a.AcademicYear == currentYear.Value)
                                   .OrderBy(a => a.Name);

                ViewBag.GroupId = new SelectList(groupsList, "Id", "Name");
            }

            return View(journal);
        }

        // GET: Journals/Edit/5
        [Authorize(Roles = "Administrators,FacultiesManagers,Teachers")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
            Journal journal;
            if (User.IsInRole("Teachers") && !User.IsInRole("FacultiesManagers"))
            {
                journal = await db.Journals
                  .Include(j => j.Group)
                  .Include(j => j.Faculty)
                  .Where(a => a.TeacherNameId == userId)
                  .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("FacultiesManagers"))
            {
                journal = await db.Journals
                  .Include(j => j.Group)
                  .Include(j => j.Faculty)
                  .Where(a => a.FacultyId == user.FacultyId)
                  .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("Administrators"))
            {
                journal = await db.Journals
                    .Include(j => j.Group)
                    .Include(j => j.Faculty)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else
            {
                return HttpNotFound();
            }
            List<TutorsList> teachersList = new List<TutorsList>();
            if (User.IsInRole("Administrators"))
            {
                var facultiesList = db.Faculties
                                      .Where(a => !a.IsDeleted)
                                      .OrderBy(a => a.Name);

                ViewBag.FacultyId = new SelectList(facultiesList, "Id", "Name", journal.FacultyId);

                teachersList = db.TeacherDepartments
                                 .Include(a => a.Teacher)
                                 .Include(a => a.Department)
                                 .Where(a => a.Department.FacultyId == user.FacultyId)
                                 .OrderBy(a => a.Teacher.Lastname)
                                 .ThenBy(a => a.Teacher.Firstname)
                                 .ThenBy(a => a.Teacher.Middlename)
                                 .Select(a => new TutorsList
                                 {
                                     Id = a.TeacherId,
                                     Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "."
                                 })
                                 .ToList();
            }
            else if (User.IsInRole("FacultiesManagers"))
            {
                teachersList = db.TeacherDepartments
                                 .Include(a => a.Teacher)
                                 .Include(a => a.Department)
                                 .Where(a => a.Department.FacultyId == user.FacultyId)
                                 .OrderBy(a => a.Teacher.Lastname)
                                 .ThenBy(a => a.Teacher.Firstname)
                                 .ThenBy(a => a.Teacher.Middlename)
                                 .Select(a => new TutorsList
                                 {
                                     Id = a.TeacherId,
                                     Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "."
                                 })
                                 .ToList();
            }
            else
            {
                teachersList = db.TeacherDepartments
                                 .Include(a => a.Teacher)
                                 .Include(a => a.Department)
                                 .Where(a => a.TeacherId == journal.TeacherNameId)
                                 .OrderBy(a => a.Teacher.Lastname)
                                 .ThenBy(a => a.Teacher.Firstname)
                                 .ThenBy(a => a.Teacher.Middlename)
                                 .Select(a => new TutorsList
                                 {
                                     Id = a.TeacherId,
                                     Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "."
                                 })
                                 .ToList();
            }
            ViewBag.TeacherNameId = new SelectList(teachersList, "Id", "Name", journal.TeacherNameId);
            return View(journal);
        }

        // POST: Journals/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators,FacultiesManagers,Teachers")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,GroupId,Discipline,Semester,Year,TypeControl,TeacherNameId,FacultyId,GroupName,GroupIdDecanat")] Journal journal)
        {
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");

            if (ModelState.IsValid)
            {
                db.Entry(journal).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            if (User.IsInRole("Administrators"))
            {
                var facultiesList = db.Faculties
                                      .Where(a => !a.IsDeleted)
                                      .OrderBy(a => a.Name);

                ViewBag.FacultyId = new SelectList(facultiesList, "Id", "Name", journal.FacultyId);

                var teachersList = db.TeacherDepartments
                                 .Include(a => a.Teacher)
                                 .Include(a => a.Department)
                                 .Where(a => a.Department.FacultyId == journal.FacultyId)
                                 .OrderBy(a => a.Teacher.Lastname)
                                 .ThenBy(a => a.Teacher.Firstname)
                                 .ThenBy(a => a.Teacher.Middlename)
                                 .Select(a => new TutorsList
                                 {
                                     Id = a.TeacherId,
                                     Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "."
                                 })
                                 .ToList();

                ViewBag.TeacherNameId = new SelectList(teachersList, "Id", "Name", journal.TeacherNameId);
            }
            else if (User.IsInRole("FacultiesManagers"))
            {
                var teachersList = db.TeacherDepartments
                                 .Include(a => a.Teacher)
                                 .Include(a => a.Department)
                                 .Where(a => a.Department.FacultyId == journal.FacultyId)
                                 .OrderBy(a => a.Teacher.Lastname)
                                 .ThenBy(a => a.Teacher.Firstname)
                                 .ThenBy(a => a.Teacher.Middlename)
                                 .Select(a => new TutorsList
                                 {
                                     Id = a.TeacherId,
                                     Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "."
                                 })
                                 .ToList();

                ViewBag.TeacherNameId = new SelectList(teachersList, "Id", "Name", journal.TeacherNameId);
            }

            return View(journal);
        }

        // GET: Journals/Delete/5
        [Authorize(Roles = "Administrators,FacultiesManagers,Teachers")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);
            Journal journal;
            if (User.IsInRole("Teachers") && !User.IsInRole("FacultiesManagers"))
            {
                journal = await db.Journals
                  .Include(j => j.Group)
                  .Include(j => j.Faculty)
                  .Where(a => a.TeacherNameId == userId)
                  .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("FacultiesManagers"))
            {
                journal = await db.Journals
                  .Include(j => j.Group)
                  .Include(j => j.Faculty)
                  .Where(a => a.FacultyId == user.FacultyId)
                  .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("Administrators"))
            {
                journal = await db.Journals
                    .Include(j => j.Group)
                    .Include(j => j.Faculty)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else
            {
                return HttpNotFound();
            }

            if (journal == null)
            {
                return HttpNotFound();
            }
            return View(journal);
        }

        // POST: Journals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators,FacultiesManagers,Teachers")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);
            Journal journal;
            if (User.IsInRole("Teachers") && !User.IsInRole("FacultiesManagers"))
            {
                journal = await db.Journals
                  .Include(j => j.Group)
                  .Include(j => j.Faculty)
                  .Where(a => a.TeacherNameId == userId)
                  .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("FacultiesManagers"))
            {
                journal = await db.Journals
                  .Include(j => j.Group)
                  .Include(j => j.Faculty)
                  .Where(a => a.FacultyId == user.FacultyId)
                  .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("Administrators"))
            {
                journal = await db.Journals
                    .Include(j => j.Group)
                    .Include(j => j.Faculty)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else
            {
                return HttpNotFound();
            }

            if (journal == null)
            {
                return HttpNotFound();
            }

            journal.IsDeleted = true;
            db.Entry(journal).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public async Task<ActionResult> ExportGrades()
        {
            var userId = User.Identity.GetUserId();
            var user = await db.Users.FirstOrDefaultAsync(a => a.Id == userId);
            var currentYear = await db.Settings.FirstOrDefaultAsync(a => a.Name == "CurrentYear");

            // Получаем список учебных годов
            var years = db.Journals
                .OrderByDescending(Year => Year.Year)
                .Select(Year => new { Value = Year.Year, Text = Year.Year })
                .Distinct()
                .OrderBy(Year => Year.Text).ToList();
            
            var yearsView = new List<SelectListItem>();
            foreach (var item in years)
            {
                yearsView.Add(new SelectListItem()
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }
            ViewBag.Years = new SelectList(yearsView, "Value", "Text");
            yearsView.FirstOrDefault(Year => Year.Value == currentYear.Value).Selected = true;

            var facultiesList = await db.Faculties
                               .Where(a => !a.IsDeleted)
                               .OrderBy(a => a.Name)
                               .ToListAsync();
            ViewBag.FacultyId = new SelectList(facultiesList, "Id", "Name", user.FacultyId);

            var groupsList = await db.Groups
                               .Where(a => !a.IsDeleted && a.AcademicYear == currentYear.Value)
                               .OrderBy(a => a.Name)
                               .ToListAsync();
            ViewBag.GroupId = new SelectList(groupsList, "Id", "Name");

            ViewBag.Semester = new SelectList(listSessions, "Value", "Text");

            return View();
        }

        [Authorize(Roles = "Administrators,FacultiesManagers")]
        [HttpPost]
        public async Task<ActionResult> ExportGrades(ReportGrades report)
        {
            if (ModelState.IsValid)
            {
                //получаем параметры для перебора данных
                var userId = User.Identity.GetUserId();
                var user = db.Users.FirstOrDefault(a => a.Id == userId);
                var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");

                //выправляем факультет, чтобы не смотрели чужие журналы
                if (!User.IsInRole("Administrators"))
                    if (report.FacultyId != user.FacultyId) report.FacultyId = (int)user.FacultyId;

                //получаем список активных групп факультета, если группа не выбрана
                //и список дисциплин для каждой группы
                List<GroupsList> lstDisp;
                List<Group> lstGroups;
                Faculty faculty = new Faculty();
                if (report.GroupId <= 0 || report.GroupId == null)
                {
                    faculty = db.Faculties.FirstOrDefault(a => a.Id == report.FacultyId);
                    lstGroups = db.Groups.Where(a => a.FacultyId == report.FacultyId && a.IsDeleted == false &&
                        a.AcademicYear == currentYear.Value).OrderBy(a => a.Name).ToList();
                }
                else
                {
                    lstGroups = db.Groups.Where(a => a.Id == report.GroupId &&
                    a.AcademicYear == currentYear.Value).ToList();
                }

                using (XLWorkbook wb = new XLWorkbook())
                {
                    foreach (var group in lstGroups)
                    {
                        var dt = GetGradesStudents(group.Id, report.Years, report.Semester, out lstDisp);
                        if (dt.Rows.Count != 0)
                        {
                            var ws = wb.Worksheets.Add(group.Name);
                            int ind = 1;
                            int n = 1;

                            //создание заголовка таблицы
                            ws.Cell("A" + ind).Value = "Учебный год:";
                            ws.Cell("B" + ind++).Value = report.Years;

                            ws.Cell("A" + ind).Value = "Сессия:";
                            ws.Cell("B" + ind++).Value = report.Semester;
                            ws.Range("A1:A2").Style.Font.Bold = true;
                            ind++;

                            //создание шапки таблицы
                            int i = 0;
                            for (int c = 0; c < dt.Columns.Count; c++)
                            {
                                if (c == 0)
                                    ws.Cell(ind, c + 1).Value = "№";
                                else if (c == 1)
                                    ws.Cell(ind, c + 1).Value = "Фамилия";
                                else if (c == 2)
                                    ws.Cell(ind, c + 1).Value = "Имя";
                                else if (c == 3)
                                    ws.Cell(ind, c + 1).Value = "Отчество";
                                else if (c == 4)
                                    ws.Cell(ind, c + 1).Value = "Основание";
                                else
                                {
                                    int id = Convert.ToInt32(dt.Columns[c].ColumnName);
                                    ws.Cell(ind, c + 1).Value = lstDisp.FirstOrDefault(a => a.Id == id).Name;
                                }
                                ws.Cell(ind, c + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                ws.Cell(ind, c + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                ws.Cell(ind, c + 1).Style.Alignment.WrapText = true;
                                ws.Cell(ind, c + 1).Style.Font.Bold = true;
                            }

                            //заполнение таблицы
                            ind++;
                            for (int r = 0; r < dt.Rows.Count; r++)
                            {
                                for (int c = 0; c < dt.Columns.Count; c++)
                                {
                                    if (dt.Columns[c].ColumnName == "Id")
                                    {
                                        ws.Cell((ind + r), c + 1).Value = n + r;
                                        ws.Cell((ind + r), c + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    }
                                    else
                                    {
                                        ws.Cell((ind + r), c + 1).Value = dt.Rows[r][c];
                                    }
                                }

                            }
                        }
                    }
                    using (MemoryStream stream = new MemoryStream())
                    {
                        string fileName = "";
                        if (report.GroupId <= 0 || report.GroupId == null)
                            fileName = faculty.Name ?? "Факультет";
                        else
                            fileName = lstGroups[0].Name;

                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + ".xlsx");
                    }
                }
            }
            return View(report);
        }

        public DataTable GetGradesStudents(int groupId, string years, string session, out List<GroupsList> lstDispciplines)
        {
            lstDispciplines = GetDisciplines(groupId, years, session);
            var lstStudentsGroup = GetStudentsGroup(groupId);
            DataTable dtJournal = new DataTable();
            if (lstDispciplines.Count != 0 && lstStudentsGroup.Count != 0)
            {
                string sqlCommand = GetSqlCommand(groupId, lstDispciplines);
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();
                    System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(sqlCommand, connection);
                    adapter.Fill(dtJournal);
                }

                return dtJournal;
            }
            else
            {
                return dtJournal;
            }
        }

        public string GetSqlCommand(int groupId, List<GroupsList> st)
        {
            StringBuilder sql = new StringBuilder("WITH JournalsTemp AS ( " +
                    "SELECT TOP(100) PERCENT dbo.AspNetUsers.Id, dbo.AspNetUsers.Lastname, dbo.AspNetUsers.Firstname, dbo.AspNetUsers.Middlename, " +
                    "dbo.AspNetUsers.Bases, dbo.Lessons.JournalId, IIF(dbo.Studies.Grade2 IS NOT NULL, CONCAT(dbo.Studies.Grade1, '/', dbo.Studies.Grade2) , dbo.Studies.Grade1) AS Grade1 " +
                    "FROM dbo.Journals INNER JOIN " +
                    "dbo.Lessons ON dbo.Journals.Id = dbo.Lessons.JournalId INNER JOIN " +
                    "dbo.Studies ON dbo.Lessons.Id = dbo.Studies.LessonId INNER JOIN " +
                    "dbo.AspNetUsers ON dbo.Studies.StudentId = dbo.AspNetUsers.Id " +
                    "WHERE(dbo.Journals.GroupId = " + groupId + ") AND dbo.AspNetUsers.DateBlocked IS NULL AND  dbo.Lessons.TypeLesson = 'Итоговая оценка' AND " +
                            "(dbo.Lessons.[JournalId] IN (");
            for (int n = 0; n < st.Count; n++)
            {
                sql.Append(st[n].Id.ToString() + ", ");
            }

            sql.Remove(sql.Length - 2, 2);
            sql.Append("))) SELECT * FROM JournalsTemp PIVOT(MAX(JournalsTemp.Grade1) for JournalsTemp.[JournalId] in (");

            for (int n = 0; n < st.Count; n++)
            {
                sql.Append("[" + st[n].Id.ToString() + "], ");
            }
            sql.Remove(sql.Length - 2, 2);
            sql.Append(")) AS Journal ORDER BY Lastname, Firstname, Middlename");

            return sql.ToString();
        }

        public Dictionary<string, string> GetStudentsGroup(int groupId)
        {
            var studentsGroup = db.Users.Where(a => a.GroupId == groupId && a.DateBlocked == null)
                    .OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).ThenBy(a => a.Middlename);

            return studentsGroup.ToDictionary(a => a.Id, a => a.Lastname + " " + a.Firstname + " " + a.Middlename + " (" + a.Bases + ")");
        }

        public List<GroupsList> GetDisciplines(int groupId, string years, string session)
        {
            IQueryable<Journal> lstDisciplines;
            if (years == "1")
            {
                lstDisciplines = db.Journals.Where(a => a.GroupId == groupId);
            }
            else
            {
                lstDisciplines = db.Journals.Where(a => a.GroupId == groupId && a.Year == years);
            }

            if (years != "1")
            {
                if (session == "Зимняя сессия")
                {
                    lstDisciplines = lstDisciplines.Where(a => a.Semester % 2 == 1);
                }
                if (session == "Летняя сессия")
                    lstDisciplines = lstDisciplines.Where(a => a.Semester % 2 == 0);
            }

            lstDisciplines = lstDisciplines.OrderBy(a => a.Semester).ThenBy(a => a.Discipline);
            var lst = lstDisciplines.Select(a => new GroupsList { Id = a.Id, Name = a.Discipline + " (" + a.Semester + ")" }).ToList();

            return lst;
        }


        public List<SelectListItem> listSessions = new List<SelectListItem>(new[] {
            new SelectListItem
            {
                Text = "Весь учебный год",
                Value = "Весь учебный год"
            },
            new SelectListItem
            {
                Text = "Зимняя сессия",
                Value = "Зимняя сессия"
            },
            new SelectListItem
            {
                Text = "Летняя сессия",
                Value = "Летняя сессия"
            }
        });

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
