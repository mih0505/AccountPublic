using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Accounts.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity.SqlServer;
using PagedList;

namespace Accounts.Controllers
{
    public class GIAsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: GIAs
        [Authorize(Roles = "Administrators,FacultiesManagers,SecretariesGIA")]
        public async Task<ActionResult> Index(string sortOrder, int? faculty, string group, string academicYear, int? page)
        {
            // Получение текущего года
            var year = db.Settings.FirstOrDefault(Setting => (Setting.Name == "CurrentYear"));

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

            IQueryable<GIA> gIAs = db.GIAs
                    .Include(g => g.Faculty)
                    .Include(g => g.Group);

            // Получение Cookies
            var cookieGroup = Request.Cookies.Get("gia_group");
            var cookieSortOrder = Request.Cookies.Get("gia_sortOrder");
            var cookieFaculty = Request.Cookies.Get("gia_faculty");
            var cookieAcademicYear = Request.Cookies.Get("gia_academicYear");

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

            if (faculty == null && cookieFaculty != null)
            {
                facultiesView.FirstOrDefault(Faculty => (Faculty.Value == cookieFaculty.Value)).Selected = true;
                faculty = Convert.ToInt32(cookieFaculty.Value);
            }

            if (academicYear == null && cookieAcademicYear != null)
            {
                yearsView.FirstOrDefault(Year => (Year.Value == cookieAcademicYear.Value)).Selected = true;
                academicYear = cookieAcademicYear.Value;
            }


            // Удаление Cookies
            Response.Cookies.Add(new HttpCookie("gia_group")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("gia_sortOrder")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("gia_faculty")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            Response.Cookies.Add(new HttpCookie("gia_academicYear")
            {
                Value = null,
                Expires = DateTime.Now.AddMinutes(-1)
            });

            // Применение фильтров
            // В том числе вывод нужной информации для определенной роли
            if (User.IsInRole("SecretariesGIA") || User.IsInRole("FacultiesManagers"))
            {
                var userId = User.Identity.GetUserId();
                var user = await db.Users.FirstOrDefaultAsync(a => a.Id == userId);
                gIAs = db.GIAs
                    .Include(g => g.Faculty)
                    .Include(g => g.Group)
                    .Where(a => a.FacultyId == user.FacultyId);
            }

            // Применение фильтров
            // В том числе добавление новых Cookies
            if (sortOrder != null)
            {
                Response.Cookies.Add(new HttpCookie("gia_sortOrder")
                {
                    Value = sortOrder.ToString(),
                    Expires = DateTime.Now.AddMinutes(15)
                });
            }

            if (faculty != null)
            {
                Response.Cookies.Add(new HttpCookie("gia_faculty")
                {
                    Value = faculty.ToString(),
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (faculty.Value != -1)
                {
                    gIAs = gIAs.Where(p => p.FacultyId == faculty);
                }
            }

            if (!string.IsNullOrEmpty(group))
            {
                Response.Cookies.Add(new HttpCookie("gia_group")
                {
                    Value = group,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                gIAs = gIAs.Where(p => SqlFunctions.PatIndex("%" + group + "%", p.Group.Name) > 0);
            }

            if (academicYear != null)
            {
                Response.Cookies.Add(new HttpCookie("gia_academicYear")
                {
                    Value = academicYear,
                    Expires = DateTime.Now.AddMinutes(15)
                });

                if (academicYear != (-1).ToString())
                {
                    gIAs = gIAs.Where(p => p.CurrentYear == academicYear);
                }
            }

            // Применение сортировки
            switch (sortOrder)
            {
                case "faculty_asc":
                    gIAs = gIAs.OrderBy(s => s.Faculty.Name);
                    break;
                case "faculty_desc":
                    gIAs = gIAs.OrderByDescending(s => s.Faculty.Name);
                    break;
                case "group_desc":
                    gIAs = gIAs.OrderByDescending(s => s.Group.Name);
                    break;
                default:
                    gIAs = gIAs.OrderBy(s => s.Group.Name);
                    break;
            }

            // Обновление всех ViewBag'ов
            ViewBag.FacultyList = facultiesView;
            ViewBag.Years = yearsView;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.FacultySortParm = sortOrder == "faculty_asc" ? "faculty_desc" : "faculty_asc";
            ViewBag.GroupSortParm = sortOrder == "group_asc" ? "group_desc" : "group_asc";
            ViewBag.GroupFilter = group;
            ViewBag.YearFilter = academicYear;

            return View(gIAs.ToPagedList(page ?? 1, 30));
        }

        [Authorize(Roles = "Administrators,FacultiesManagers,SecretariesGIA")]
        public ActionResult ClearFilter()
        {
            HttpCookie cookieSortOrder = Request.Cookies.Get("gia_sortOrder");
            if (cookieSortOrder != null)
            {
                cookieSortOrder.Value = null;
                cookieSortOrder.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieSortOrder);
            }

            HttpCookie cookieGroup = Request.Cookies.Get("gia_group");
            if (cookieGroup != null)
            {
                cookieGroup.Value = null;
                cookieGroup.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieGroup);
            }

            HttpCookie cookieFaculty = Request.Cookies.Get("gia_faculty");
            if (cookieFaculty != null)
            {
                cookieFaculty.Value = null;
                cookieFaculty.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieFaculty);
            }

            HttpCookie cookieAcademicYear = Request.Cookies.Get("gia_academicYear");
            if (cookieAcademicYear != null)
            {
                cookieAcademicYear.Value = null;
                cookieAcademicYear.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieAcademicYear);
            }

            return RedirectToAction("Index");
        }

        // GET: GIAs/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            GIA gIA = await db.GIAs.Include(a => a.Group).FirstOrDefaultAsync(a => a.Id == id);
            if (gIA == null)
            {
                return HttpNotFound();
            }

            if (gIA.DateEnd.Date < DateTime.Now.Date && !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("AccessDenied");
            }

            ViewBag.Group = gIA.Group.Name;

            var diploms = await db.DiplomWorks
                .Where(a => a.GIAId == id)
                .Include(a => a.GIA)
                .Include(a => a.StudentDiplom)
                .Include(a => a.TeacherDiplom)
                .OrderBy(a => a.StudentDiplom.Lastname)
                .ThenBy(a => a.StudentDiplom.Firstname)
                .ThenBy(a => a.StudentDiplom.Middlename)
                .ToListAsync();

            return View(diploms);
        }

        // GET: GIAs/Show/5
        public async Task<ActionResult> Show(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            GIA gIA = await db.GIAs.Include(a => a.Group).FirstOrDefaultAsync(a => a.Id == id);
            if (gIA == null)
            {
                return HttpNotFound();
            }

            if (gIA.PathVideo == null)
            {
                return RedirectToAction("AccessDenied");
            }

            return View(gIA);
        }


        public ActionResult AccessDenied()
        {
            return View();
        }


        public FileResult GetFile(string path, string name)
        {
            byte[] mas = System.IO.File.ReadAllBytes(path);
            string file_path = Server.MapPath("~/DiplomWorks/" + name);
            string file_type = "application/octet-stream";
            return File(mas, file_type, name);
        }

        // GET: GIAs/Create
        [Authorize(Roles = "Administrators,FacultiesManagers,SecretariesGIA")]
        public ActionResult Create()
        {
            var currentYear = db.Settings.FirstOrDefault(Setting => (Setting.Name == "CurrentYear"));
            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);
            var gia = new GIA();

            if (User.IsInRole("Administrators"))
            {
                ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", user.FacultyId);
                ViewBag.GroupId = new SelectList(db.Groups.Where(a => a.IsDeleted == false &&
                a.AcademicYear == currentYear.Value).OrderBy(a => a.Name), "Id", "Name");
            }
            else if (User.IsInRole("SecretariesGIA") || User.IsInRole("FacultiesManagers"))
            {
                ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");
                ViewBag.GroupId = new SelectList(db.Groups.Where(a => a.FacultyId == user.FacultyId && a.IsDeleted == false &&
                a.AcademicYear == currentYear.Value).OrderBy(a => a.Name), "Id", "Name");
            }

            return View();
        }

        // POST: GIAs/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators,FacultiesManagers,SecretariesGIA")]
        public async Task<ActionResult> Create(GIA gIA)
        {
            string userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);
            var currentYear = db.Settings.FirstOrDefault(Setting => (Setting.Name == "CurrentYear"));

            if (!User.IsInRole("Administrators"))
                if (gIA.FacultyId != user.FacultyId) gIA.FacultyId = (int)user.FacultyId;

            if (ModelState.IsValid)
            {
                gIA.CurrentYear = currentYear.Value;
                gIA.IsBlocked = false;
                gIA.Id = Guid.NewGuid().ToString();
                db.GIAs.Add(gIA);
                await db.SaveChangesAsync();

                //заполняем ведомость студентами группы
                var lstStudentGroup = db.Users.Where(a => a.GroupId == gIA.GroupId && a.DateBlocked == null)
                    .OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).ThenBy(a => a.Middlename).ToList();
                foreach (var s in lstStudentGroup)
                {
                    var student = new DiplomWork
                    {
                        GIAId = gIA.Id,
                        StudentDiplom = s
                    };
                    db.DiplomWorks.Add(student);
                }
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false)
                .OrderBy(a => a.Name), "Id", "Name", gIA.FacultyId);
            ViewBag.GroupId = new SelectList(db.Groups.Where(a => a.FacultyId == user.FacultyId && a.IsDeleted == false &&
                a.AcademicYear == currentYear.Value).OrderBy(a => a.Name), "Id", "Name", gIA.GroupId);

            return View(gIA);
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

        // GET: GIAs/Edit/5
        [Authorize(Roles = "Administrators,FacultiesManagers,SecretariesGIA")]
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GIA gIA = await db.GIAs.FindAsync(id);
            if (gIA == null)
            {
                return HttpNotFound();
            }
            ViewBag.FacultyId = new SelectList(db.Faculties, "Id", "Name", gIA.FacultyId);
            ViewBag.GroupId = new SelectList(db.Groups, "Id", "Name", gIA.GroupId);

            return View(gIA);
        }

        // POST: GIAs/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators,FacultiesManagers,SecretariesGIA")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,FacultyId,GroupId,DateEnd,IsBlocked,CurrentYear,Link,PathVideo")] GIA gIA)
        {
            if (ModelState.IsValid)
            {
                db.Entry(gIA).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.FacultyId = new SelectList(db.Faculties, "Id", "Name", gIA.FacultyId);
            ViewBag.GroupId = new SelectList(db.Groups, "Id", "Name", gIA.GroupId);

            return View(gIA);
        }

        // GET: GIAs/Delete/5
        [Authorize(Roles = "Administrators,FacultiesManagers,SecretariesGIA")]
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            GIA gIA = await db.GIAs.Include(a => a.Group).FirstOrDefaultAsync(a => a.Id == id);
            if (!string.IsNullOrEmpty(gIA.PathVideo))
            {
                return RedirectToAction("AccessDenied");
            }

            ViewBag.Diploms = db.DiplomWorks.Where(a => a.GIAId == id &&
            (!string.IsNullOrEmpty(a.PathConducting) || !string.IsNullOrEmpty(a.PathDiplom) || !string.IsNullOrEmpty(a.PathFeedback) ||
             !string.IsNullOrEmpty(a.PathApplication) || !string.IsNullOrEmpty(a.PathPlagiarism) || !string.IsNullOrEmpty(a.PathReview) ||
             !string.IsNullOrEmpty(a.PathConfirmation) || !string.IsNullOrEmpty(a.PathDeclaration) || !string.IsNullOrEmpty(a.PathСonsent) ||
             !string.IsNullOrEmpty(a.PathDiplomPDF) || !string.IsNullOrEmpty(a.PathOther))).Count();
            if (gIA == null)
            {
                return HttpNotFound();
            }

            return View(gIA);
        }

        // POST: GIAs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators,FacultiesManagers,SecretariesGIA")]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            GIA gIA = await db.GIAs.FindAsync(id);
            var lstSdudents = db.DiplomWorks.Where(a => a.GIAId == gIA.Id).ToList();

            db.DiplomWorks.RemoveRange(lstSdudents);
            db.GIAs.Remove(gIA);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
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
