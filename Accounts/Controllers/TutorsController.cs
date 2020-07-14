using Accounts.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using PagedList;
using ClosedXML.Excel;
using System.IO;

namespace Accounts.Controllers
{
    public class TutorsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tutors
        [Authorize(Roles = "TutorsManagers,Administrators,Teachers")]
        public async Task<ActionResult> Index()
        {
            //получение текущего учебного года
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");

            IQueryable<Tutor> tutorsList = db.Tutors.Include(a => a.User)
                .Include(a => a.Faculty)
                .Include(a=>a.Group)
                .Where(a=>a.Group.AcademicYear == currentYear.Value)
                .OrderBy(a => a.Group.Name);

            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);
            if (User.IsInRole("TutorsManagers"))
            {
                List<string> teachers = db.Users.Where(a => a.DecanatId == null && a.FacultyId == user.FacultyId).Select(a => a.Id).ToList();
                tutorsList = tutorsList.Where(a => teachers.Contains(a.UserId))
                    .Include(a => a.Faculty).OrderBy(a => a.Group.Name);
            }
            else if (User.IsInRole("Teachers"))
            {
                tutorsList = tutorsList.Where(a => a.UserId == user.Id).Include(a => a.User).Include(a => a.Group).Include(a => a.Faculty).OrderBy(a => a.Group.Name);
            }
            else tutorsList = tutorsList.Include(a => a.User).Include(a => a.Group)
                    .Include(a => a.Faculty).OrderBy(a => a.Group.Name);

            return View(await tutorsList.ToListAsync());
        }

        // GET: Tutors/Details/5
        [Authorize(Roles = "TutorsManagers,Administrators")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tutor tutor = await db.Tutors.Include(a => a.User).Include(a => a.Group).Include(a => a.Faculty).FirstOrDefaultAsync(a => a.Id == id);
            if (tutor == null)
            {
                return HttpNotFound();
            }
            return View(tutor);
        }

        // GET: Tutors/Create
        [Authorize(Roles = "TutorsManagers,Administrators")]
        public ActionResult Create()
        {
            var tutor = new TutorViewModel();
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");
            
            if (User.IsInRole("Administrators"))
            {
                tutor.TutorsList = db.Users.Where(a => a.StatusId == null).OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).
                Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename }).ToList();
                tutor.Groups = db.Groups.Where(a => a.IsDeleted == false && a.AcademicYear == currentYear.Value).OrderBy(a => a.Name).
                    Select(a => new GroupsList { Id = a.Id, Name = a.Name }).ToList();
            }
            else
            {
                tutor.TutorsList = null;
                tutor.Groups = null;
            }
            return View(tutor);
        }

        // POST: Tutors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "TutorsManagers,Administrators")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,UserId,GroupId,FacultyId")] TutorViewModel tutor)
        {
            if (tutor.FacultyId == null)
            {
                var group = db.Groups.FirstOrDefault(a => a.Id == tutor.GroupId);
                tutor.FacultyId = group.FacultyId;
            }
            if (ModelState.IsValid || User.IsInRole("Administrators"))
            {
                db.Tutors.Add(new Tutor { UserId = tutor.UserId, GroupId = tutor.GroupId, FacultyId = tutor.FacultyId });
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", tutor.FacultyId);
            return View(tutor);
        }

        public JsonResult GetUsers(string id)
        {
            List<TutorsList> tutorList;
            if ((id == null || id == "") && User.IsInRole("Administrators"))
            {
                tutorList = db.Users.Where(a => a.StatusId == null).OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).
                Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename }).ToList();
            }
            else
            {
                int facultyId = Convert.ToInt32(id);
                tutorList = db.Users.Where(a => a.StatusId == null && a.FacultyId == facultyId).OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).
                Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename }).ToList();
            }
            return Json(new SelectList(tutorList, "Id", "Name"));
        }

        public JsonResult GetGroups(int? id)
        {
            List<GroupsList> groupsList;
            if (id == null && User.IsInRole("Administrators"))
            {
                var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
                groupsList = db.Groups.Where(a => a.IsDeleted == false &&
                    a.AcademicYear == currentYear.Value).OrderBy(a => a.Name).
                Select(a => new GroupsList { Id = a.Id, Name = a.Name }).ToList();
            }
            else
            {
                int facultyId = Convert.ToInt32(id);
                var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
                groupsList = db.Groups.Where(a => a.FacultyId == facultyId && a.IsDeleted == false &&
                    a.AcademicYear == currentYear.Value).OrderBy(a => a.Name).
                Select(a => new GroupsList { Id = a.Id, Name = a.Name }).ToList();
            }
            return Json(new SelectList(groupsList, "Id", "Name"));
        }

        [Authorize(Roles = "TutorsManagers,Administrators,Teachers")]
        public async Task<ActionResult> StudentsGroup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var group = db.Groups.Find(id);
            ViewBag.CurrentGroup = group;
            var userId = User.Identity.GetUserId();
            if (User.IsInRole("Teachers") && !User.IsInRole("TutorsManagers"))
            {
                var validationTutor = db.Tutors.FirstOrDefault(a => a.UserId == userId && a.GroupId == group.Id);

                if (validationTutor == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }

            var listStudents = from students in db.Users
                               where students.GroupId == id && students.DateBlocked == null
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
                               };
            return View(await listStudents.ToListAsync());
        }

        [Authorize(Roles = "TutorsManagers,Administrators,DepartmentsManagers,FacultiesManagers")]
        public async Task<ActionResult> TeachersPortfolio()
        {
            var userId = User.Identity.GetUserId();
            var user = await db.Users.FirstAsync(a => a.Id == userId);

            IQueryable<ReportPortfolio> listTeachers;
            if (User.IsInRole("TutorsManagers") || User.IsInRole("FacultiesManagers"))
            {
                //получаем список преподавателей факультета
                listTeachers = from teachers in db.Users
                               join dep in db.Departments on teachers.DepartmentId equals dep.Id
                               join fac in db.Faculties on teachers.FacultyId equals fac.Id
                               where teachers.FacultyId == user.FacultyId && teachers.DecanatId == null && teachers.Employer != true
                               orderby teachers.Lastname, teachers.Firstname
                               select new ReportPortfolio
                               {
                                   Id = teachers.Id,
                                   Lastname = teachers.Lastname,
                                   Firstname = teachers.Firstname,
                                   Middlename = teachers.Middlename,
                                   Science = (from sections in db.Sections
                                              join catigories in db.Catigories on sections.Id equals catigories.SectionId
                                              join artifacts in db.Artifacts on catigories.Id equals artifacts.CatigoryId
                                              where sections.Name == "Науч.-исслед. деятельность" && artifacts.UserId == teachers.Id
                                              select artifacts).Count(),
                                   Faculty = fac.AliasFaculty,
                                   Department = dep.ShortName,
                               };
            }
            else if (User.IsInRole("DepartmentsManagers"))
            {
                //получаем список преподавателей куфедры             
                listTeachers = from teachers in db.Users
                               join dep in db.Departments on teachers.DepartmentId equals dep.Id
                               join fac in db.Faculties on teachers.FacultyId equals fac.Id
                               where teachers.DepartmentId == user.DepartmentId && teachers.DecanatId == null && teachers.Employer != true
                               orderby teachers.Lastname, teachers.Firstname
                               select new ReportPortfolio
                               {
                                   Id = teachers.Id,
                                   Lastname = teachers.Lastname,
                                   Firstname = teachers.Firstname,
                                   Middlename = teachers.Middlename,
                                   Science = (from sections in db.Sections
                                              join catigories in db.Catigories on sections.Id equals catigories.SectionId
                                              join artifacts in db.Artifacts on catigories.Id equals artifacts.CatigoryId
                                              where sections.Name == "Науч.-исслед. деятельность" && artifacts.UserId == teachers.Id
                                              select artifacts).Count(),
                                   Faculty = fac.AliasFaculty,
                                   Department = dep.ShortName,
                               };
            }
            else
            {
                //получаем список преподавателей кафедры             
                listTeachers = from teachers in db.Users
                               join dep in db.Departments on teachers.DepartmentId equals dep.Id
                               join fac in db.Faculties on teachers.FacultyId equals fac.Id
                               where teachers.DecanatId == null && teachers.Employer != true
                               orderby teachers.Lastname, teachers.Firstname
                               select new ReportPortfolio
                               {
                                   Id = teachers.Id,
                                   Lastname = teachers.Lastname,
                                   Firstname = teachers.Firstname,
                                   Middlename = teachers.Middlename,
                                   Science = (from sections in db.Sections
                                              join catigories in db.Catigories on sections.Id equals catigories.SectionId
                                              join artifacts in db.Artifacts on catigories.Id equals artifacts.CatigoryId
                                              where sections.Name == "Науч.-исслед. деятельность" && artifacts.UserId == teachers.Id
                                              select artifacts).Count(),
                                   Faculty = fac.AliasFaculty,
                                   Department = dep.ShortName,
                               };
            }
            return View(await listTeachers.ToListAsync());
        }

        [HttpGet]
        public FileResult Export()
        {
            //экспорт базового образования
            DataTable dtPortfolio = new DataTable("Портфолио преподавателя");
            dtPortfolio.Columns.AddRange(new DataColumn[4] {
                                            new DataColumn("Преподаватель"),
                                            new DataColumn("Факультет"),
                                            new DataColumn("Кафедра"),
                                            new DataColumn("Достижений")});
            string userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);

            IQueryable<ReportPortfolio> listTeachers = null;
            if (User.IsInRole("TutorsManagers") || User.IsInRole("FacultiesManagers"))
            {
                //получаем список преподавателей факультета             
                listTeachers = from teachers in db.Users
                               join dep in db.Departments on teachers.DepartmentId equals dep.Id
                               join fac in db.Faculties on teachers.FacultyId equals fac.Id
                               where teachers.FacultyId == user.FacultyId && teachers.DecanatId == null && teachers.Employer != true
                               orderby teachers.Lastname, teachers.Firstname
                               select new ReportPortfolio
                               {
                                   Id = teachers.Id,
                                   Lastname = teachers.Lastname,
                                   Firstname = teachers.Firstname,
                                   Middlename = teachers.Middlename,
                                   Science = (from sections in db.Sections
                                              join catigories in db.Catigories on sections.Id equals catigories.SectionId
                                              join artifacts in db.Artifacts on catigories.Id equals artifacts.CatigoryId
                                              where sections.Name == "Науч.-исслед. деятельность" && artifacts.UserId == teachers.Id
                                              select artifacts).Count(),
                                   Faculty = fac.AliasFaculty,
                                   Department = dep.ShortName,
                               };
            }
            else if (User.IsInRole("DepartmentsManagers"))
            {
                //получаем список преподавателей куфедры             
                listTeachers = from teachers in db.Users
                               join dep in db.Departments on teachers.DepartmentId equals dep.Id
                               join fac in db.Faculties on teachers.FacultyId equals fac.Id
                               where teachers.DepartmentId == user.DepartmentId && teachers.DecanatId == null && teachers.Employer != true
                               orderby teachers.Lastname, teachers.Firstname
                               select new ReportPortfolio
                               {
                                   Id = teachers.Id,
                                   Lastname = teachers.Lastname,
                                   Firstname = teachers.Firstname,
                                   Middlename = teachers.Middlename,
                                   Science = (from sections in db.Sections
                                              join catigories in db.Catigories on sections.Id equals catigories.SectionId
                                              join artifacts in db.Artifacts on catigories.Id equals artifacts.CatigoryId
                                              where sections.Name == "Науч.-исслед. деятельность" && artifacts.UserId == teachers.Id
                                              select artifacts).Count(),
                                   Faculty = fac.AliasFaculty,
                                   Department = dep.ShortName,
                               };
            }
            else
            {
                //получаем список преподавателей куфедры             
                listTeachers = from teachers in db.Users
                               join dep in db.Departments on teachers.DepartmentId equals dep.Id
                               join fac in db.Faculties on teachers.FacultyId equals fac.Id
                               where teachers.DecanatId == null && teachers.Employer != true
                               orderby teachers.Lastname, teachers.Firstname
                               select new ReportPortfolio
                               {
                                   Id = teachers.Id,
                                   Lastname = teachers.Lastname,
                                   Firstname = teachers.Firstname,
                                   Middlename = teachers.Middlename,
                                   Science = (from sections in db.Sections
                                              join catigories in db.Catigories on sections.Id equals catigories.SectionId
                                              join artifacts in db.Artifacts on catigories.Id equals artifacts.CatigoryId
                                              where sections.Name == "Науч.-исслед. деятельность" && artifacts.UserId == teachers.Id
                                              select artifacts).Count(),
                                   Faculty = fac.AliasFaculty,
                                   Department = dep.ShortName,
                               };
            }

            var abc = listTeachers.ToList();
            foreach (var teacher in listTeachers)
            {
                dtPortfolio.Rows.Add(teacher.Lastname + " " + teacher.Firstname + " " + teacher.Middlename, teacher.Faculty, teacher.Department, teacher.Science);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dtPortfolio);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Portfolio.xlsx");
                }
            }
        }

        public FileResult GetFile(string path, string name)
        {
            byte[] mas = System.IO.File.ReadAllBytes(path);
            string file_path = Server.MapPath("~/Portfolio/" + name);
            string file_type = "application/octet-stream";
            return File(mas, file_type, name);
        }

        [Authorize(Roles = "TutorsManagers,Administrators,Teachers")]
        public async Task<ActionResult> GetArtifacts(int? sectionId, string userId)
        {
            if (userId != null && sectionId != null)
            {
                var user = await db.Users.FirstAsync(a => a.Id == userId);
                var section = await db.Sections.FindAsync(sectionId);
                ViewBag.Section = section;

                //получение списка категорий
                var catigories = await db.Catigories.Where(a => a.SectionId == sectionId).OrderBy(a => a.IndexSort).ToListAsync();
                ViewBag.Catigories = catigories;

                if (userId == null && userId == "" && catigories == null && section == null)
                    return HttpNotFound();

                //получение списка достижений
                var artifacts = from arts in db.Artifacts
                                join cats in db.Catigories on arts.CatigoryId equals cats.Id
                                where cats.SectionId == sectionId && arts.User.Id == userId
                                orderby cats.Name, arts.DateBegin
                                select arts;

                ViewBag.CurrentUser = user;

                return View(await artifacts.ToListAsync());
            }
            else return HttpNotFound();
        }

        public ActionResult ListGroups(string sortOrder, int? faculty, string name, int? formOfTraining, int? page)
        {
            //получение текущего учебного года
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");

            ViewBag.CurrentSort = sortOrder;
            ViewBag.FacultySortParm = String.IsNullOrEmpty(sortOrder) ? "faculty_desc" : "faculty_asc";
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.FormSortParm = String.IsNullOrEmpty(sortOrder) ? "form_desc" : "form_asc";
            ViewBag.CourseSortParm = String.IsNullOrEmpty(sortOrder) ? "course_desc" : "course_asc";

            ViewBag.FacultyFilter = faculty;
            ViewBag.NameFilter = name;
            ViewBag.FormFilter = formOfTraining;            

            //фильтр групп
            IQueryable<Group> groups = null;
            if (User.IsInRole("TutorsManagers"))
            {
                string userId = User.Identity.GetUserId();
                var user = db.Users.Find(userId);
                groups = db.Groups.Where(a => a.IsDeleted == false &&
                a.AcademicYear == currentYear.Value).Include(g => g.Faculty).Include(g => g.FormOfTraining)
                .Include(g => g.Profile).Where(a => a.FacultyId == user.FacultyId).OrderBy(c => c.Name);
            }
            else
                groups = db.Groups.Where(a => a.IsDeleted == false &&
                   a.AcademicYear == currentYear.Value).Include(g => g.Faculty).Include(g => g.FormOfTraining)
                   .Include(g => g.Profile).OrderBy(c => c.Name);
                                    
            if (faculty != null)
            {
                groups = groups.Where(p => p.FacultyId == faculty);
            }
            if (!String.IsNullOrEmpty(name))
            {
                groups = groups.Where(p => SqlFunctions.PatIndex("%" + name + "%", p.Name) > 0);
            }

            if (formOfTraining != null)
            {
                groups = groups.Where(p => p.FormOfTrainingId == formOfTraining);
            }

            switch (sortOrder)
            {
                case "name_desc":
                    groups = groups.OrderByDescending(s => s.Name);
                    break;
                case "faculty_asc":
                    groups = groups.OrderBy(s => s.Faculty.Name);
                    break;
                case "faculty_desc":
                    groups = groups.OrderByDescending(s => s.Faculty.Name);
                    break;
                case "form_asc":
                    groups = groups.OrderBy(s => s.FormOfTraining.Name);
                    break;
                case "form_desc":
                    groups = groups.OrderByDescending(s => s.FormOfTraining.Name);
                    break;
                default:
                    groups = groups.OrderBy(s => s.Name);
                    break;
            }

            ViewBag.FacultyList = new SelectList(db.Faculties.Where(a => a.IsDeleted == false), "Id", "Name");            
            ViewBag.FormOfTraining = new SelectList(db.FormOfTrainings, "Id", "Name");

            int pageSize = 30;
            int pageNumber = (page ?? 1);

            return View(groups.ToPagedList(pageNumber, pageSize));
        }

        // GET: Tutors/Edit/5
        [Authorize(Roles = "TutorsManagers,Administrators")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
            Tutor t = await db.Tutors.FindAsync(id);
            TutorViewModel tutor = new TutorViewModel
            {
                Id = t.Id,
                GroupId = t.GroupId,
                FacultyId = t.FacultyId,
                UserId = t.UserId,
                TutorsList = db.Users.Where(a => a.StatusId == null && a.FacultyId == t.FacultyId).OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).
                Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename }).OrderBy(a => a.Name).ToList(),
                Groups = db.Groups.Where(a => a.FacultyId == t.FacultyId && a.IsDeleted == false &&
                a.AcademicYear == currentYear.Value).OrderBy(a => a.Name).
                Select(a => new GroupsList { Id = a.Id, Name = a.Name }).ToList()
            };

            if (tutor == null)
            {
                return HttpNotFound();
            }
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", tutor.FacultyId);
            return View(tutor);
        }

        // POST: Tutors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TutorsManagers,Administrators")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,UserId,GroupId,FacultyId")] TutorViewModel tutor)
        {
            if (ModelState.IsValid)
            {
                var t = new Tutor();
                t.Id = tutor.Id;
                t.UserId = tutor.UserId;
                t.GroupId = tutor.GroupId;
                t.FacultyId = tutor.FacultyId;

                db.Entry(t).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a=>a.IsDeleted != false).OrderBy(a => a.Name), "Id", "Name", tutor.FacultyId);

            return View(tutor);
        }

        // GET: Tutors/Delete/5
        [Authorize(Roles = "TutorsManagers,Administrators")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tutor tutor = await db.Tutors.FindAsync(id);
            if (tutor == null)
            {
                return HttpNotFound();
            }
            return View(tutor);
        }

        // POST: Tutors/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "TutorsManagers,Administrators")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Tutor tutor = await db.Tutors.FindAsync(id);
            db.Tutors.Remove(tutor);
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
