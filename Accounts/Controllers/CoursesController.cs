using Accounts.Models;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Accounts.Controllers
{
    [Authorize(Roles = "Administrators,DepartmentsManagers,FacultiesManagers")]
    public class CoursesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CourseWorks
        public async Task<ActionResult> Index(string sortOrder, string type, int? faculty, int? department, string group, string course, string academicYear, int? page)
        {
            // Получение текущего года
            var year = db.Settings.FirstOrDefault(Setting => (Setting.Name == "CurrentYear"));

            // Создание первого (стандартного) элемента (чтобы избежать бага с фильтрацией данных)
            var defaultItem = new SelectListItem()
            {
                Value = "-1",
                Text = "Все"
            };

            // Заполнение DropDownList'a c типами
            var typesView = new List<SelectListItem>()
            {
                defaultItem
            };

            typesView.AddRange(listItems);

            // Заполнение DropDownList'a c факультетами
            var facultiesList = await db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name).ToListAsync();
            var facultiesView = new List<SelectListItem>()
            {
                defaultItem
            };

            foreach (var item in facultiesList)
            {
                facultiesView.Add(new SelectListItem()
                {
                    Value = item.Id.ToString(),
                    Text = item.Name
                });
            }

            // Заполнение DropDownList'a c кафедрами
            var departmentsList = await db.Departments.Where(a => a.IsDeleted == false).OrderBy(a => a.Name).ToListAsync();
            var departmentsView = new List<SelectListItem>()
            {
                defaultItem
            };

            foreach (var item in departmentsList)
            {
                departmentsView.Add(new SelectListItem()
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

            // Заполнение DropDownList'a с учебными годами
            var yearsList = db.Statements.OrderByDescending(a => a.CurrentYear).Select(a => new { Value = a.CurrentYear, Text = a.CurrentYear }).Distinct().OrderBy(a => a.Text).ToList();
            var yearsView = new List<SelectListItem>()
            {
                defaultItem
            };

            foreach (var item in yearsList)
            {
                yearsView.Add(new SelectListItem()
                {
                    Value = item.Value,
                    Text = item.Text
                });
            }

            // Получение списка курсовых/практик
            var listIdOfCurrentGroups = await db.Groups.Where(a => !a.IsDeleted && a.AcademicYear == year.Value).Select(a => a.Id).ToListAsync();
            var courses = db.Courses.Include(c => c.Group).Include(a => a.Faculty).Include(a => a.User).Include(a => a.Department).Where(a => listIdOfCurrentGroups.Contains(a.GroupId));
            courses = courses.OrderByDescending(a => a.DateBegin);

            var cookieGroup = Request.Cookies.Get("courses_group");
            var cookieType = Request.Cookies.Get("courses_type");
            var cookieSortOrder = Request.Cookies.Get("courses_sortOrder");
            var cookieFaculty = Request.Cookies.Get("courses_faculty");
            var cookieDepartment = Request.Cookies.Get("courses_department");
            var cookieAcademicYear = Request.Cookies.Get("courses_academicYear");
            var cookieCourseNumber = Request.Cookies.Get("courses_courseNumber");

            // Применение значений из Cookies
            // В том числе обновление значений в DropDownList согласно полученным Cookies
            if (group == null && cookieGroup != null)
            {
                group = cookieGroup.Value;
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

            if (department == null && cookieDepartment != null)
            {
                departmentsView.FirstOrDefault(Department => (Department.Value == cookieDepartment.Value)).Selected = true;
                department = Convert.ToInt32(cookieDepartment.Value);
            }

            if (academicYear == null && cookieAcademicYear != null)
            {
                yearsView.FirstOrDefault(Year => (Year.Value == cookieAcademicYear.Value)).Selected = true;
                academicYear = cookieAcademicYear.Value;
            }

            if (course == null && cookieCourseNumber != null)
            {
                coursesView.FirstOrDefault(Course => (Course.Value == cookieCourseNumber.Value)).Selected = true;
                course = cookieCourseNumber.Value;
            }

            // Удаление Cookies
            Response.Cookies.Add(new HttpCookie("courses_group")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("courses_type")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("courses_sortOrder")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("courses_faculty")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("courses_department")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("courses_academicYear")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("courses_courseNumber")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);
            if (User.IsInRole("FacultiesManagers"))
            {
                courses = courses.Where(a => a.FacultyId == user.FacultyId).Include(a => a.Group).Include(a => a.Department).OrderByDescending(a => a.DateBegin);
            }
            else if (User.IsInRole("DepartmentsManagers"))
            {
                // Определяем какой кафедрой/-ами заведует пользователь
                var lstDep = db.TeacherDepartments.Where(a => a.TeacherId == user.Id && a.IsManager == true).Select(a => a.DepartmentId).ToList();

                // Получаем список Id преподавателей, в которых руководит заведующий
                var teachersDepartmentId = db.TeacherDepartments.Where(a => lstDep.Contains(a.DepartmentId)).Select(a => a.TeacherId).ToList();

                // Получаем список проектов, в которых руководят преподаватели полученных выше кафедр
                var projectsDepartment = db.CourseWorkStudents.Where(a => teachersDepartmentId.Contains(a.TeacherId)).Select(a => a.CourseId).ToList();

                courses = courses.Where(a => a.CourseWorkCreater == userId || projectsDepartment.Contains(a.Id)).OrderByDescending(a => a.DateBegin);
            }

            // Применение фильтров
            // В том числе добавление новых Cookies
            if (sortOrder != null)
            {
                Response.Cookies.Add(new HttpCookie("courses_sortOrder")
                {
                    Value = sortOrder.ToString(),
                    Expires = DateTime.Now.AddMinutes(15)
                });
            }

            if (faculty != null)
            {
                Response.Cookies.Add(new HttpCookie("courses_faculty")
                {
                    Value = faculty.ToString(),
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (faculty.Value != -1)
                {
                    courses = courses.Where(p => p.FacultyId == faculty.Value);
                }
            }

            if (department != null)
            {
                Response.Cookies.Add(new HttpCookie("courses_department")
                {
                    Value = department.ToString(),
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (department.Value != -1)
                {
                    courses = courses.Where(p => p.DepartmentId == department.Value);
                }
            }

            if (!string.IsNullOrEmpty(group))
            {
                Response.Cookies.Add(new HttpCookie("courses_group")
                {
                    Value = group,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                courses = courses.Where(p => SqlFunctions.PatIndex("%" + group + "%", p.GroupName) > 0);
            }

            if (!string.IsNullOrEmpty(type))
            {
                Response.Cookies.Add(new HttpCookie("courses_type")
                {
                    Value = type,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (type != (-1).ToString())
                {
                    courses = courses.Where(p => SqlFunctions.PatIndex("%" + type + "%", p.Type) > 0);
                }
            }

            if (!string.IsNullOrEmpty(course))
            {
                Response.Cookies.Add(new HttpCookie("courses_courseNumber")
                {
                    Value = course,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (course != (-1).ToString())
                {
                    int courseId = Convert.ToInt32(course);
                    courses = courses.Where(p => p.Cours == courseId);
                }
            }

            if (academicYear != null)
            {
                Response.Cookies.Add(new HttpCookie("courses_academicYear")
                {
                    Value = academicYear,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (academicYear != (-1).ToString())
                {
                    courses = courses.Where(p => p.CurrentYear == academicYear);
                }
            }

            switch (sortOrder)
            {
                case "faculty_desc":
                    courses = courses.OrderByDescending(s => s.Faculty.AliasFaculty);
                    break;
                case "department_asc":
                    courses = courses.OrderBy(s => s.Department.ShortName);
                    break;
                case "department_desc":
                    courses = courses.OrderByDescending(s => s.Department.ShortName);
                    break;
                case "group_asc":
                    courses = courses.OrderBy(s => s.Group.Name);
                    break;
                case "group_desc":
                    courses = courses.OrderByDescending(s => s.Group.Name);
                    break;
                case "course_asc":
                    courses = courses.OrderBy(s => s.Cours);
                    break;
                case "course_desc":
                    courses = courses.OrderByDescending(s => s.Cours);
                    break;
                case "type_asc":
                    courses = courses.OrderBy(s => s.Type);
                    break;
                case "type_desc":
                    courses = courses.OrderByDescending(s => s.Type);
                    break;
                default:
                    courses = courses.OrderBy(s => s.Faculty.AliasFaculty);
                    break;
            }

            // Фильтрация от завершивших и удаленных групп
            if (!User.IsInRole("Administrators"))
            {
                courses = courses.Where(a => a.Group.IsDeleted != true);
            }

            ViewBag.FacultyList = facultiesView;
            ViewBag.DepartmentList = departmentsView;
            ViewBag.Years = yearsView;
            ViewBag.Courses = coursesView;
            ViewBag.Types = typesView;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.FacultySortParm = String.IsNullOrEmpty(sortOrder) ? "faculty_desc" : "";
            ViewBag.DepartmentSortParm = sortOrder == "department_asc" ? "department_desc" : "department_asc";
            ViewBag.GroupSortParm = sortOrder == "group_asc" ? "group_desc" : "group_asc";
            ViewBag.CourseSortParm = sortOrder == "course_asc" ? "course_desc" : "course_asc";
            ViewBag.TypeSortParm = sortOrder == "type_asc" ? "type_desc" : "type_asc";
            ViewBag.FacultyFilter = faculty;
            ViewBag.DepartmentFilter = department;
            ViewBag.GroupFilter = group;
            ViewBag.TypeFilter = type;
            ViewBag.YearFilter = academicYear;
            ViewBag.CourseFilter = course;

            // Вывод результата
            return View(courses.ToPagedList(page ?? 1, 30));
        }

        public ActionResult ClearFilter()
        {
            HttpCookie cookieSortOrder = Request.Cookies.Get("courses_sortOrder");
            if (cookieSortOrder != null)
            {
                cookieSortOrder.Value = null;
                cookieSortOrder.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieSortOrder);
            }

            HttpCookie cookieGroup = Request.Cookies.Get("courses_group");
            if (cookieGroup != null)
            {
                cookieGroup.Value = null;
                cookieGroup.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieGroup);
            }

            HttpCookie cookieFaculty = Request.Cookies.Get("courses_faculty");
            if (cookieFaculty != null)
            {
                cookieFaculty.Value = null;
                cookieFaculty.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieFaculty);
            }

            HttpCookie cookieDepartment = Request.Cookies.Get("courses_department");
            if (cookieDepartment != null)
            {
                cookieDepartment.Value = null;
                cookieDepartment.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieDepartment);
            }

            HttpCookie cookieAcademicYear = Request.Cookies.Get("courses_academicYear");
            if (cookieAcademicYear != null)
            {
                cookieAcademicYear.Value = null;
                cookieAcademicYear.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieAcademicYear);
            }

            HttpCookie cookieType = Request.Cookies.Get("courses_type");
            if (cookieType != null)
            {
                cookieType.Value = null;
                cookieType.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieType);
            }

            HttpCookie cookieCourse = Request.Cookies.Get("courses_course");
            if (cookieCourse != null)
            {
                cookieCourse.Value = null;
                cookieCourse.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieCourse);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Distribution(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //проверка существует ли распределение студентов по преподавателям
            var courseWork = db.Courses.FirstOrDefault(a => a.Id == id);
            ViewBag.CountTeachers = courseWork.CountTeachers;
            var distribution = db.CourseWorkStudents
                .Where(a => a.CourseId == id)
                .Include(a => a.Student)
                .OrderBy(a => a.Student.Lastname)
                .ThenBy(a => a.Student.Firstname)
                .ToList();

            //////получаем список руководителей///////
            List<TutorsList> lstTeacher;
            if (courseWork.DepartmentId != null)
            {
                lstTeacher = db.TeacherDepartments
                    .Where(a => a.DepartmentId == courseWork.DepartmentId)
                    .Include(a => a.Teacher)
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
                //получение кафедр факультета
                var depList = db.Departments.Where(a => a.FacultyId == courseWork.FacultyId).Select(a => a.Id).ToList();
                //получение списка преподавателей факультета
                lstTeacher = db.TeacherDepartments
                    .Where(a => depList.Contains(a.DepartmentId))
                    .Include(a => a.Teacher)
                    .OrderBy(a => a.Teacher.Lastname)
                    .ThenBy(a => a.Teacher.Firstname)
                    .ThenBy(a => a.Teacher.Middlename)
                    .Select(a => new TutorsList
                    {
                        Id = a.TeacherId,
                        Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "."
                    }).ToList();
            }
            var distVM = new List<DistributionViewModel>();
            var lstStudentGroup = db.Users
                .Where(a => a.GroupId == courseWork.GroupId && a.DateBlocked == null)
                .OrderBy(a => a.Lastname).ThenBy(a => a.Firstname)
                .ThenBy(a => a.Middlename)
                .ToList();

            //проверяем численность студентов в ведомости и группе, на случай отчисления или восстановления
            var idStudentsGroup = lstStudentGroup
                .Where(u => u.DecanatId != null)
                .Select(u => u.DecanatId.Value)
                .ToList();
            var idDistributionStudents = distribution
                .Select(a => a.IdSudentDecanate);
            var distinct = idStudentsGroup
                .Except(idDistributionStudents).ToList();

            //редактируем распределение студентов
            //проверяем численность студентов в ведомости и группе, на случай отчисления или восстановления
            if (distribution.Count != lstStudentGroup.Count || distinct.Count != 0)
            {
                foreach (var d in distinct)
                {
                    var s = lstStudentGroup.First(a => a.DecanatId == d);
                    var student = new CourseWorkStudent
                    {
                        GroupIdSite = Convert.ToInt32(s.GroupId),
                        GroupIdDecanate = Convert.ToInt32(s.idGroupDecanat),
                        Student = s,
                        IdSudentDecanate = Convert.ToInt32(s.DecanatId),
                        CourseId = courseWork.Id,
                    };
                    db.CourseWorkStudents.Add(student);
                }
                db.SaveChanges();
                distribution = db.CourseWorkStudents.Where(a => a.CourseId == id).Include(a => a.Student).OrderBy(a => a.Student.Lastname).ThenBy(a => a.Student.Firstname).ToList();
            }
            //загрузка списка 
            foreach (var s in distribution)
            {
                distVM.Add(new DistributionViewModel
                {
                    CourseWorkStudent = s,
                    Teachers = new SelectList(lstTeacher, "Id", "Name", s.TeacherId),
                    Teachers2 = new SelectList(lstTeacher, "Id", "Name", s.Teacher2Id),
                    Teachers3 = new SelectList(lstTeacher, "Id", "Name", s.Teacher3Id),
                    Teachers4 = new SelectList(lstTeacher, "Id", "Name", s.Teacher4Id),
                    Teachers5 = new SelectList(lstTeacher, "Id", "Name", s.Teacher5Id)
                });
            }
            return View(distVM);
        }

        [HttpPost]
        public ActionResult Distribution(List<DistributionViewModel> distribution)
        {
            foreach (var dist in distribution)
            {
                CourseWorkStudent cws = db.CourseWorkStudents.Find(dist.CourseWorkStudent.Id);
                cws.TeacherId = dist.CourseWorkStudent.TeacherId;
                cws.Teacher2Id = dist.CourseWorkStudent.Teacher2Id;
                cws.Teacher3Id = dist.CourseWorkStudent.Teacher3Id;
                cws.Teacher4Id = dist.CourseWorkStudent.Teacher4Id;
                cws.Teacher5Id = dist.CourseWorkStudent.Teacher5Id;
                db.Entry(cws).State = EntityState.Modified;
            }
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: CourseWorks/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            string userId = User.Identity.GetUserId();
            if (id == null && userId == null && userId == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Course courseWork = null;
            if (User.IsInRole("DepartmentsManagers"))
            {
                courseWork = await db.Courses.Where(a => a.CourseWorkCreater == userId)
                    .Include(a => a.Group).Include(a => a.Faculty).Include(a => a.Department).Include(a => a.User).Include(a => a.Department)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("FacultiesManagers"))
            {
                var user = db.Users.FirstOrDefault(a => a.Id == userId);
                courseWork = await db.Courses.Where(a => a.FacultyId == user.FacultyId).Include(a => a.Group).Include(a => a.Faculty).Include(a => a.Department).Include(a => a.User)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("Administrators"))
            {
                courseWork = await db.Courses.Include(a => a.Group).Include(a => a.Faculty).Include(a => a.User).Include(a => a.Department)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else return HttpNotFound();

            return View(courseWork);
        }

        public List<SelectListItem> listItems = new List<SelectListItem>(new[] {
            new SelectListItem
            {
                Text = "Курсовая",
                Value = "Курсовая"
            },
            new SelectListItem
            {
                Text = "Дипломная",
                Value = "Дипломная"
            },
            new SelectListItem
            {
                Text = "Практика",
                Value = "Практика"
            },
            new SelectListItem
            {
                Text = "НИР",
                Value = "НИР"
            }});

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

        // GET: CourseWorks/Create
        public ActionResult Create()
        {
            ViewBag.Type = new SelectList(listItems, "Text", "Value");

            var courseWork = new Course();

            courseWork.Group = null;
            courseWork.DateBegin = courseWork.DateEnd = DateTime.Now;
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");

            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");
            ViewBag.GroupId = new SelectList(db.Groups.Where(a => a.IsDeleted == false &&
                a.AcademicYear == currentYear.Value).OrderBy(a => a.Name), "Id", "Name");

            return View(courseWork);
        }

        // POST: CourseWorks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,GroupId,Type,FacultyId,DepartmentId,Cours,Semester,DateBegin,DateEnd,CountTeachers")] Course courseWork)
        {
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
            if (ModelState.IsValid)
            {
                string userId = User.Identity.GetUserId();
                var user = db.Users.FirstOrDefault(a => a.Id == userId);
                courseWork.CourseWorkCreater = user.Id;
                var group = db.Groups.FirstOrDefault(a => a.Id == courseWork.GroupId);
                if (group != null)
                    courseWork.GroupIdDecanate = Convert.ToInt32(group.DecanatID);

                courseWork.CurrentYear = currentYear.Value;

                //определение учебного года и названия группы
                //1. получение курса из названия группы
                int currentCourse = Convert.ToInt32(group.Name.Substring(group.Name.Length - 2, 1));
                //2. сравниваем совпадают ли введенный курс и курс из названия группы
                if (currentCourse == courseWork.Cours)
                {
                    courseWork.CurrentYear = currentYear.Value;
                    courseWork.GroupName = group.Name;
                }
                //если не совпали, то определяем правильный учебный год
                else
                {
                    int delta = currentCourse - courseWork.Cours;
                    int y = Convert.ToInt32(currentYear.Value.Substring(2, 2)) - delta;
                    int y1 = Convert.ToInt32(currentYear.Value.Substring(7)) - delta;
                    courseWork.CurrentYear = String.Format($"20{y}-20{y1}");

                    courseWork.GroupName = group.Name.Substring(0, group.Name.Length - 2) + courseWork.Cours + group.Name.Substring(group.Name.Length - 1, 1);
                }

                db.Courses.Add(courseWork);

                //заполняем ведомость студентами группы
                var lstStudentGroup = db.Users.Where(a => a.GroupId == courseWork.GroupId && a.DateBlocked == null)
                    .OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).ThenBy(a => a.Middlename).ToList();
                foreach (var s in lstStudentGroup)
                {
                    var student = new CourseWorkStudent
                    {
                        GroupIdSite = Convert.ToInt32(s.GroupId),
                        GroupIdDecanate = Convert.ToInt32(s.idGroupDecanat),
                        Student = s,
                        IdSudentDecanate = Convert.ToInt32(s.DecanatId),
                        CourseId = courseWork.Id,
                    };
                    db.CourseWorkStudents.Add(student);
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");
            ViewBag.GroupId = new SelectList(db.Groups.Where(a => a.IsDeleted == false && a.AcademicYear == currentYear.Value).OrderBy(a => a.Name), "Id", "Name", courseWork.GroupId);
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", courseWork.DepartmentId);
            ViewBag.Type = new SelectList(listItems, "Text", "Value", courseWork.Type);

            return View(courseWork);
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
            return Json(new SelectList(groupsList, "Id", "Name"));
        }

        //public JsonResult GetDepartments(int? id)
        //{
        //    List<DepartmentsList> departmentsList;            
        //    if (id == null)
        //    {
        //        departmentsList = db.Departments.Where(a => a.IsDeleted == false).OrderBy(a => a.Name).
        //       Select(a => new DepartmentsList { Id = a.Id, Name = a.Name }).ToList();
        //    }
        //    else
        //    {
        //        int facultyId = Convert.ToInt32(id);
        //        departmentsList = db.Departments.Where(a => a.FacultyId == facultyId && a.IsDeleted == false).OrderBy(a => a.Name).
        //       Select(a => new DepartmentsList { Id = a.Id, Name = a.Name }).ToList();
        //    }
        //    return Json(new SelectList(departmentsList, "Id", "Name"));
        //}


        // GET: CourseWorks/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string userId = User.Identity.GetUserId();
            ApplicationUser user = db.Users.FirstOrDefault(a => a.Id == userId);
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
            Course courseWork = null;
            //if (User.IsInRole("FacultiesManagers") || User.IsInRole("DepartmentsManagers"))
            //{
            //    courseWork = await db.Courses.Where(a => a.CourseWorkCreater == userId)
            //        .Include(a => a.Group).SingleOrDefaultAsync(a => a.Id == id);
            //    var groups = db.Groups.Where(a => a.FacultyId == user.FacultyId && a.IsDeleted == false &&
            //    a.AcademicYear == currentYear.Value);
            //    ViewBag.GroupId = new SelectList(groups, "Id", "Name", courseWork.GroupId);
            //}
            //else if (User.IsInRole("Administrators"))
            //{
            courseWork = await db.Courses.Include(a => a.Group).SingleOrDefaultAsync(a => a.Id == id);
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", courseWork.FacultyId);
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", courseWork.DepartmentId);
            ViewBag.GroupId = new SelectList(db.Groups.Where(a => a.IsDeleted == false &&
            a.AcademicYear == currentYear.Value).OrderBy(a => a.Name), "Id", "Name", courseWork.GroupId);
            //}
            //else return HttpNotFound();

            ViewBag.Type = new SelectList(listItems, "Text", "Value", courseWork.Type);

            return View(courseWork);
        }

        // POST: CourseWorks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,GroupId,Type,FacultyId,DepartmentId,GroupIdDecanate,Semester,DateBegin,DateEnd,Cours,CourseWorkCreater,CountTeachers")] Course courseWork)
        {
            string userId = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                //courseWork.CourseWorkCreater = userId;
                var group = db.Groups.FirstOrDefault(a => a.Id == courseWork.GroupId);
                if (group != null)
                    courseWork.GroupIdDecanate = Convert.ToInt32(group.DecanatID);
                var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
                courseWork.CurrentYear = currentYear.Value;

                //определение учебного года и названия группы
                //1. получение курса из названия группы
                int currentCourse = Convert.ToInt32(group.Name.Substring(group.Name.Length - 2, 1));
                //2. сравниваем совпадают ли введенный курс и курс из названия группы
                if (currentCourse == courseWork.Cours)
                {
                    courseWork.CurrentYear = currentYear.Value;
                    courseWork.GroupName = group.Name;
                }
                //если не совпали, то определяем правильный учебный год
                else
                {
                    int delta = currentCourse - courseWork.Cours;
                    int y = Convert.ToInt32(currentYear.Value.Substring(2, 2)) - delta;
                    int y1 = Convert.ToInt32(currentYear.Value.Substring(7)) - delta;
                    courseWork.CurrentYear = String.Format($"20{y}-20{y1}");

                    courseWork.GroupName = group.Name.Substring(0, group.Name.Length - 2) + courseWork.Cours + group.Name.Substring(group.Name.Length - 1, 1);
                }

                db.Entry(courseWork).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.GroupId = new SelectList(db.Groups, "Id", "Name", courseWork.GroupId);
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", courseWork.DepartmentId);
            ViewBag.Type = new SelectList(listItems, "Text", "Value", courseWork.Type);
            return View(courseWork);
        }

        // GET: CourseWorks/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string userId = User.Identity.GetUserId();
            Course courseWork = null;
            if (User.IsInRole("DepartmentsManagers"))
            {
                courseWork = await db.Courses.Where(a => a.CourseWorkCreater == userId)
                    .Include(a => a.Group).Include(a => a.Faculty).Include(a => a.Department).Include(a => a.User)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("FacultiesManagers"))
            {
                var user = db.Users.FirstOrDefault(a => a.Id == userId);
                courseWork = await db.Courses.Where(a => a.FacultyId == user.FacultyId).Include(a => a.Group).Include(a => a.Faculty).Include(a => a.Department).Include(a => a.User)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("Administrators"))
            {
                courseWork = await db.Courses.Include(a => a.Group).Include(a => a.Faculty).Include(a => a.Department).Include(a => a.User)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else return HttpNotFound();
            return View(courseWork);
        }

        // POST: CourseWorks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Course courseWork;
            List<CourseWorkStudent> lstSdudents;
            var userId = User.Identity.GetUserId();
            if (User.IsInRole("DepartmentsManagers"))
            {
                courseWork = db.Courses.Where(a => a.CourseWorkCreater == userId)
                    .SingleOrDefault(m => m.Id == id);
                lstSdudents = db.CourseWorkStudents.Where(a => a.CourseId == courseWork.Id).ToList();
            }
            else if (User.IsInRole("FacultiesManagers"))
            {
                var user = db.Users.FirstOrDefault(a => a.Id == userId);
                courseWork = db.Courses.Where(a => a.FacultyId == user.FacultyId)
                    .SingleOrDefault(m => m.Id == id);
                lstSdudents = db.CourseWorkStudents.Where(a => a.CourseId == courseWork.Id).ToList();
            }
            else if (User.IsInRole("Administrators"))
            {
                courseWork = db.Courses
                    .SingleOrDefault(m => m.Id == id);
                lstSdudents = db.CourseWorkStudents.Where(a => a.CourseId == courseWork.Id).ToList();
            }
            else return HttpNotFound();

            db.CourseWorkStudents.RemoveRange(lstSdudents);
            db.Courses.Remove(courseWork);
            db.SaveChanges();
            return RedirectToAction("Index", "Courses");
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
