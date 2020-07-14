using Accounts.Models;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Accounts.Controllers
{
    [Authorize(Roles = "Administrators,FacultiesManagers")]
    public class TeacherDepartmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: TeacherDepartments
        public async Task<ActionResult> Index()
        {
            //var teacherDepartments = db.TeacherDepartments.Include(d => d.Department).Include(t=>t.Teacher).OrderBy(a=>a.Teacher.Lastname).ThenBy(a=>a.Teacher.Firstname);
            var teacherD = from td in db.TeacherDepartments
                           join d in db.Departments on td.DepartmentId equals d.Id
                           join t in db.Users on td.TeacherId equals t.Id
                           orderby t.Lastname, t.Firstname, t.Middlename
                           select new ListTeachersDepartments
                           {
                               Id = td.Id,
                               TeacherId = t.Id,
                               Lastname = t.Lastname,
                               Firstname = t.Firstname,
                               Middlename = t.Middlename,
                               Department = d.Name,
                               IsManager = td.IsManager
                           };

            return View(await teacherD.ToListAsync());
        }

        //public async Task<ActionResult> Sync()
        //{
        //    var teachers = db.Users.Where(a => a.StatusId == null && a.DepartmentId != null).Include(a => a.Department).ToList();
        //    var teachersDepartments = db.TeacherDepartments.ToList();
        //    ViewBag.Log = "";
        //    foreach (var teacher in teachers)
        //    {
        //        var record = teachersDepartments.FirstOrDefault(a => a.DepartmentId == teacher.DepartmentId && a.TeacherId == teacher.Id);
        //        if (record == null)
        //        {
        //            db.TeacherDepartments.Add(new TeacherDepartment { TeacherId = teacher.Id, DepartmentId = (int)teacher.DepartmentId });
        //            ViewBag.Log += $"<p>Добавлен преподаватель {teacher.LastnameFM} в кафедру {teacher.Department.Name}</p>";
        //        }
        //    }

        //    await db.SaveChangesAsync();
        //    return View();
        //}

        // GET: TeacherDepartments/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TeacherDepartment teacherDepartment = await db.TeacherDepartments.Include(a => a.Department).Include(a => a.Teacher).FirstOrDefaultAsync(a => a.Id == id);
            if (teacherDepartment == null)
            {
                return HttpNotFound();
            }
            return View(teacherDepartment);
        }

        // GET: TeacherDepartments/Create
        public ActionResult Create()
        {
            var teacherId = db.Users.Where(a => a.StatusId == null).OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).
                Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename }).ToList();
            ViewBag.TeacherId = new SelectList(teacherId, "Id", "Name");
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a=>!a.IsDeleted).OrderBy(a=>a.Name), "Id", "Name");

            return View();
        }

        // POST: TeacherDepartments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,TeacherId,DepartmentId,IsManager")] TeacherDepartment teacherDepartment)
        {
            if (ModelState.IsValid)
            {
                db.TeacherDepartments.Add(teacherDepartment);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var teacherId = db.Users.Where(a => a.StatusId == null).OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).
                Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename }).ToList();
            ViewBag.TeacherId = new SelectList(teacherId, "Id", "Name", teacherDepartment.TeacherId);
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => !a.IsDeleted).OrderBy(a => a.Name), "Id", "Name", teacherDepartment.DepartmentId);
            return View(teacherDepartment);
        }

        // GET: TeacherDepartments/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TeacherDepartment teacherDepartment = await db.TeacherDepartments.FindAsync(id);
            if (teacherDepartment == null)
            {
                return HttpNotFound();
            }
            var teacherId = db.Users.Where(a => a.StatusId == null).OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).
                Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename }).ToList();
            ViewBag.TeacherId = new SelectList(teacherId, "Id", "Name", teacherDepartment.TeacherId);
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => !a.IsDeleted).OrderBy(a => a.Name), "Id", "Name", teacherDepartment.DepartmentId);
            return View(teacherDepartment);
        }

        // POST: TeacherDepartments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,TeacherId,DepartmentId,IsManager")] TeacherDepartment teacherDepartment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(teacherDepartment).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            var teacherId = db.Users.Where(a => a.StatusId == null).OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).
                Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename }).ToList();
            ViewBag.TeacherId = new SelectList(teacherId, "Id", "Name", teacherDepartment.TeacherId);
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => !a.IsDeleted).OrderBy(a => a.Name), "Id", "Name", teacherDepartment.DepartmentId);
            return View(teacherDepartment);
        }

        // GET: TeacherDepartments/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TeacherDepartment teacherDepartment = await db.TeacherDepartments.Include(a => a.Department).Include(a => a.Teacher).FirstOrDefaultAsync(a => a.Id == id);
            if (teacherDepartment == null)
            {
                return HttpNotFound();
            }
            return View(teacherDepartment);
        }

        // POST: TeacherDepartments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            TeacherDepartment teacherDepartment = await db.TeacherDepartments.Include(a => a.Department).Include(a => a.Teacher).FirstOrDefaultAsync(a => a.Id == id);
            db.TeacherDepartments.Remove(teacherDepartment);
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
