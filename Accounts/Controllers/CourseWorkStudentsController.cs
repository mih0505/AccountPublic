using Accounts.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Accounts.Controllers
{
    public class CourseWorkStudentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public CourseWorkStudentsController()
        {
        }

        public CourseWorkStudentsController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
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
        // GET: CourseWorkStudents
        [Authorize(Roles = "Students,Administrators,Teachers,DepartmentsManagers,FacultiesManagers")]
        public ActionResult Index(int? id)
        {
            var userId = User.Identity.GetUserId();

            IQueryable<CourseWorkStudent> projects = null;
            if (id == null)
            {
                if (User.IsInRole("Students"))
                    projects = db.CourseWorkStudents.Where(a => a.StudentId == userId).Include(a => a.Student).Include(a => a.Teacher).Include(a => a.Course).OrderByDescending(a => a.Course.DateBegin);
                else if (User.IsInRole("Teachers"))
                {
                    DateTime date = DateTime.Now.Date;
                    projects = db.CourseWorkStudents.Include(a => a.Student).Include(a => a.Teacher).Include(a => a.Course).Include(a => a.Course.Group)
                        .Where(a => (a.TeacherId == userId || a.Teacher2Id == userId || a.Teacher3Id == userId || a.Teacher4Id == userId || a.Teacher5Id == userId)
                        && (a.Course.DateEnd >= date || (a.Course.DateEnd <= date && a.Grade == null && a.Path != null)) && a.Course.Group.IsDeleted != true)
                        .OrderBy(a => a.Student.Lastname).ThenBy(a => a.Student.Firstname).ThenBy(a => a.Student.Middlename);
                }
                else return HttpNotFound();
            }
            if (id != null)
            {
                if (User.IsInRole("DepartmentsManagers"))
                {
                    projects = db.CourseWorkStudents.Include(a => a.Student).Include(a => a.Teacher).Include(a => a.Course)
                          .Where(a => a.Course.CourseWorkCreater == userId && a.Course.GroupId == id).OrderBy(a => a.Student.Lastname)
                          .ThenBy(a => a.Student.Firstname).ThenBy(a => a.Student.Middlename);
                }
                else if (User.IsInRole("FacultiesManagers"))
                {
                    var user = db.Users.FirstOrDefault(a => a.Id == userId);
                    if (user != null)
                    {
                        projects = db.CourseWorkStudents.Include(a => a.Student).Include(a => a.Teacher).Include(a => a.Course)
                              .Where(a => a.Course.FacultyId == user.FacultyId && a.Course.GroupId == id).OrderBy(a => a.Student.Lastname)
                              .ThenBy(a => a.Student.Firstname).ThenBy(a => a.Student.Middlename);
                    }
                }
                else if (User.IsInRole("Administrators"))
                    projects = db.CourseWorkStudents.Include(a => a.Student).Include(a => a.Teacher).Include(a => a.Course)
                        .Where(a => a.Course.GroupId == id).OrderBy(a => a.Student.Lastname).ThenBy(a => a.Student.Firstname).ThenBy(a => a.Student.Middlename);
                else return HttpNotFound();
            }

            //var s = projects.ToList();

            return View(projects.ToList());
        }

        [Authorize(Roles = "Administrators")]
        public ActionResult IndexStudent(string id)
        {
            IQueryable<CourseWorkStudent> projects = null;
            //определенеие ролей пользователя
            var userRoles = UserManager.GetRolesAsync(id);

            projects = db.CourseWorkStudents.Include(a => a.Student).Include(a => a.Teacher).Include(a => a.Course)
                .Where(a => a.StudentId == id).OrderBy(a => a.Course.DateBegin);

            return View(projects.ToList());
        }

        // GET: CourseWorkStudents/Details/5
        [Authorize(Roles = "Students,Administrators,Teachers,DepartmentsManagers,FacultiesManagers")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userId = User.Identity.GetUserId();
            CourseWorkStudent courseWorkStudent = null;
            if (User.IsInRole("Students"))
                courseWorkStudent = db.CourseWorkStudents.Where(a => a.StudentId == userId).Include(a => a.Student)
                    .Include(a => a.Teacher).Include(a => a.Course).FirstOrDefault(a => a.Id == id);
            else if (User.IsInRole("Teachers"))
                courseWorkStudent = db.CourseWorkStudents.Where(a => a.TeacherId == userId).Include(a => a.Student)
                    .Include(a => a.Teacher).Include(a => a.Course).FirstOrDefault(a => a.Id == id);
            else if (User.IsInRole("DepartmentsManagers"))
                courseWorkStudent = db.CourseWorkStudents.Include(a => a.Student).Include(a => a.Teacher)
                    .Include(a => a.Course).Where(a => a.Course.CourseWorkCreater == userId).FirstOrDefault(a => a.Id == id);
            else if (User.IsInRole("FacultiesManagers"))
            {
                var user = db.Users.FirstOrDefault(a => a.Id == userId);
                if (user != null)
                    courseWorkStudent = db.CourseWorkStudents.Include(a => a.Student).Include(a => a.Teacher)
                        .Include(a => a.Course).Where(a => a.Course.FacultyId == user.FacultyId).FirstOrDefault(a => a.Id == id);
            }
            else if (User.IsInRole("Administrators"))
                courseWorkStudent = db.CourseWorkStudents.Include(a => a.Student).Include(a => a.Teacher)
                    .Include(a => a.Course).FirstOrDefault(a => a.Id == id);
            else return HttpNotFound();

            if (courseWorkStudent == null)
            {
                return HttpNotFound();
            }
            return View(courseWorkStudent);
        }

        // GET: CourseWorkStudents/Edit/5
        [Authorize(Roles = "Students,Administrators,Teachers,DepartmentsManagers,FacultiesManagers")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string userId = User.Identity.GetUserId();
            if (id == null && userId == null && userId == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseWorkStudent courseWorkStudent = null;

            if (User.IsInRole("Administrators"))
                courseWorkStudent = db.CourseWorkStudents.Include(a => a.Student).Include(a => a.Teacher).Include(a => a.Teacher2).Include(a => a.Teacher3).Include(a => a.Teacher4).Include(a => a.Teacher5)
                    .Include(a => a.Course).FirstOrDefault(a => a.Id == id);
            else if (User.IsInRole("FacultiesManagers"))
            {
                var user = db.Users.FirstOrDefault(a => a.Id == userId);
                if (user != null)
                    courseWorkStudent = db.CourseWorkStudents.Include(a => a.Student).Include(a => a.Teacher).Include(a => a.Teacher2).Include(a => a.Teacher3).Include(a => a.Teacher4).Include(a => a.Teacher5)
                        .Include(a => a.Course).Where(a => a.Course.FacultyId == user.FacultyId).FirstOrDefault(a => a.Id == id);
            }
            else if (User.IsInRole("DepartmentsManagers"))
                courseWorkStudent = db.CourseWorkStudents.Include(a => a.Student).Include(a => a.Teacher).Include(a => a.Teacher2).Include(a => a.Teacher3).Include(a => a.Teacher4).Include(a => a.Teacher5)
                    .Include(a => a.Course).Where(a => a.Course.CourseWorkCreater == userId || a.TeacherId == userId || a.Teacher2Id == userId || a.Teacher3Id == userId
                    || a.Teacher4Id == userId || a.Teacher5Id == userId).FirstOrDefault(a => a.Id == id);
            else if (User.IsInRole("Teachers"))
                courseWorkStudent = db.CourseWorkStudents.Where(a => a.TeacherId == userId).Include(a => a.Student)
                    .Include(a => a.Teacher).Include(a => a.Teacher2).Include(a => a.Teacher3).Include(a => a.Teacher4).Include(a => a.Teacher5).Include(a => a.Course).FirstOrDefault(a => a.Id == id);
            else if (User.IsInRole("Students"))
                courseWorkStudent = db.CourseWorkStudents.Where(a => a.StudentId == userId).Include(a => a.Student)
                    .Include(a => a.Teacher).Include(a => a.Teacher2).Include(a => a.Teacher3).Include(a => a.Teacher4).Include(a => a.Teacher5).Include(a => a.Course).FirstOrDefault(a => a.Id == id);
            else return HttpNotFound();

            if ((User.IsInRole("Students") && courseWorkStudent.Grade != null) || (User.IsInRole("Teachers") && courseWorkStudent.Grade != null && courseWorkStudent.Course.DateEnd <= DateTime.Now.Date
                && !User.IsInRole("DepartmentsManagers") && !User.IsInRole("FacultiesManagers") && !User.IsInRole("Administrators")))
            {
                return HttpNotFound();
            }

            return View(courseWorkStudent);
        }

        // POST: CourseWorkStudents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Students,Administrators,Teachers,DepartmentsManagers,FacultiesManagers")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,DateUpload,Name,Grade,Path,NameFile,GroupIdSite,GroupIdDecanate,StudentId,TeacherId,IsBlocked,CourseId,IdSudentDecanate")] CourseWorkStudent courseWorkStudent, HttpPostedFileBase File)
        {
            string userId = User.Identity.GetUserId();
            if (userId == null && userId == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (User.IsInRole("Students") && courseWorkStudent.Grade != null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (File != null && File.ContentLength > 52428800 ||
                        (File != null && Path.GetExtension(File.FileName.ToLower()) != ".pdf" && Path.GetExtension(File.FileName.ToLower()) != ".doc"
                        && Path.GetExtension(File.FileName.ToLower()) != ".zip" && Path.GetExtension(File.FileName.ToLower()) != ".docx"))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    if (File != null && File.ContentLength > 0)
                    {
                        if (courseWorkStudent.NameFile != File.FileName)
                        {
                            courseWorkStudent.DateUpload = DateTime.Now;
                        }
                        string extension = Path.GetExtension(File.FileName);
                        courseWorkStudent.NameFile = File.FileName;
                        string fileName = Guid.NewGuid().ToString() + extension;
                        courseWorkStudent.Path = Server.MapPath("~/CourseWorks/" + fileName);
                        File.SaveAs(courseWorkStudent.Path);
                        ViewBag.Message = "Загрузка файла выполнена";
                    }
                }
                catch
                {
                    ViewBag.Message = "Ошибка загрузки файла";
                }

                db.Entry(courseWorkStudent).State = EntityState.Modified;
                await db.SaveChangesAsync();
                if (!User.IsInRole("Students") && (User.IsInRole("Administrators") || User.IsInRole("DepartmentsManagers") || User.IsInRole("FacultiesManagers")))
                    return RedirectToAction("Distribution", "Courses", new { id = courseWorkStudent.CourseId });
                else return RedirectToAction("Index");
            }

            return View(courseWorkStudent);
        }

        public FileResult GetFile(string path, string name)
        {
            byte[] mas = System.IO.File.ReadAllBytes(path);
            string file_path = Server.MapPath("~/CourseWorks/" + name);
            string file_type = "application/octet-stream";
            return File(mas, file_type, name);
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
