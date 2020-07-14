using Accounts.Models;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Accounts.Controllers
{
    [Authorize]
    public class DeclarationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public FileResult GetFile(string path, string name)
        {
            byte[] mas = System.IO.File.ReadAllBytes(path);
            string file_path = Server.MapPath("~/Declarations/" + name);
            string file_type = "application/octet-stream";
            return File(mas, file_type, name);
        }

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
                Text = "Факультеты"
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
            var defaultItem1 = new SelectListItem()
            {
                Value = "-1",
                Text = "Форма обучения"
            };
            var formsOfTrainingList = db.FormOfTrainings.ToList();
            var formsOfTrainingView = new List<SelectListItem>()
            {
                defaultItem1
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
            IQueryable<Group> groups = db.Groups
                .Where(Group => ((Group.IsDeleted == false) && (Group.AcademicYear == year.Value)))
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
                var tutors = db.Tutors
                    .Where(Tutor => (Tutor.UserId == user.Id))
                    .Select(Tutor => Tutor.GroupId)
                    .ToList();
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
        }

        public ActionResult ClearFilter()
        {
            if (Request.Cookies["group_sortOrder"] != null)
            {
                var cookieSortOrder = new HttpCookie("group_sortOrder");
                cookieSortOrder.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieSortOrder);
            }

            if (Request.Cookies["group_name"] != null)
            {
                var cookieName = new HttpCookie("group_name");
                cookieName.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieName);
            }

            if (Request.Cookies["group_faculty"] != null)
            {
                var cookieFaculty = new HttpCookie("group_faculty");
                cookieFaculty.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieFaculty);
            }

            if (Request.Cookies["group_academicYear"] != null)
            {
                var cookieAcademicYear = new HttpCookie("group_academicYear");
                cookieAcademicYear.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieAcademicYear);
            }

            if (Request.Cookies["group_formOfTraining"] != null)
            {
                var cookieFormOfTraining = new HttpCookie("group_formOfTraining");
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

            ViewBag.CurrentGroup = db.Groups.Find(id);
            var declarations = await (from students in db.Users
                                      join decs in db.Declarations on students.DeclarationId equals decs.Id into outer
                                      from decs in outer.DefaultIfEmpty()
                                      where students.GroupId == id && students.DateBlocked == null
                                      orderby students.Lastname, students.Firstname, students.Middlename
                                      select new ListDeclarations
                                      {
                                          Lastname = students.Lastname,
                                          Firstname = students.Firstname,
                                          Middlename = students.Middlename,
                                          Date = decs.DateAdd,
                                          Path = decs.Path,
                                          NameFile = decs.NameFile
                                      }).ToListAsync();

            return View(declarations);
        }

        // GET: 
        public ActionResult Declaration()
        {
            var userId = User.Identity.GetUserId();

            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Find(userId);

                if (user != null)
                {
                    var dec = db.Declarations.FirstOrDefault(a => a.Id == user.DeclarationId);

                    if (dec != null)
                    {
                        return View(dec);
                    }
                }

                return View();
            }
        }

        // POST:         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Declaration(Declaration dec, HttpPostedFileBase NameFile)
        {
            if (NameFile is null)
            {
                return View(dec);
            }

            var declaration = new Declaration();

            if (ModelState.IsValid)
            {
                declaration.UserId = User.Identity.GetUserId();
                declaration.DateAdd = DateTime.Now;

                //Сохраняем изображение
                try
                {
                    if (NameFile != null && NameFile.ContentLength > 31457280 ||
                        (NameFile != null && Path.GetExtension(NameFile.FileName.ToLower()) != ".jpg" && Path.GetExtension(NameFile.FileName.ToLower()) != ".jpeg" &&
                        Path.GetExtension(NameFile.FileName.ToLower()) != ".gif" && Path.GetExtension(NameFile.FileName.ToLower()) != ".pdf" &&
                         Path.GetExtension(NameFile.FileName.ToLower()) != ".zip" && Path.GetExtension(NameFile.FileName.ToLower()) != ".png"))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    if (NameFile != null && NameFile.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(NameFile.FileName);

                        declaration.NameFile = NameFile.FileName;
                        string fileName = Guid.NewGuid().ToString() + extension;
                        declaration.Path = Server.MapPath("~/Declarations/" + fileName);
                        NameFile.SaveAs(declaration.Path);
                        ViewBag.Message = "Загрузка файла выполнена";
                    }
                }
                catch
                {
                    ViewBag.Message = "Ошибка загрузки файла";
                }

                using (var db = new ApplicationDbContext())
                {
                    db.Declarations.Add(declaration);
                    await db.SaveChangesAsync();

                    var user = db.Users.Find(declaration.UserId);
                    user.DeclarationId = declaration.Id;
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index", "Home");
            }

            return View(declaration);
        }
    }
}