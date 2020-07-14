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
using System.Data.Entity.SqlServer;
using PagedList;

namespace Accounts.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class ProfilesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Profiles
        public async Task<ActionResult> Index(string sortOrder, string name, int? faculty, int? department, int? page)
        {
            ViewBag.Faculties = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");
            ViewBag.Departments = new SelectList(db.Departments.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");

            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.FacultySortParm = String.IsNullOrEmpty(sortOrder) ? "faculty_desc" : "faculty_asc";
            ViewBag.DepartmentSortParm = String.IsNullOrEmpty(sortOrder) ? "department_desc" : "department_asc";

            ViewBag.NameFilter = name;
            ViewBag.FacultyFilter = faculty;
            ViewBag.DepartmentFilter = department;

            IQueryable<Profile> profiles = db.Profiles
                .Include(p => p.Department)
                .Include(p => p.DirectionOfTraining)
                .Include(p => p.Faculty)
                .OrderBy(a=>a.Name);

            if (!String.IsNullOrEmpty(name))
            {
                profiles = profiles.Where(p => SqlFunctions.PatIndex("%" + name + "%", p.Name) > 0);
            }
            if (faculty != null)
            {
                profiles = profiles.Where(p => p.FacultyId == faculty);
            }
            if (department != null)
            {
                profiles = profiles.Where(p => p.DepartmentId == department);
            }

            switch (sortOrder)
            {
                case "name_desc":
                    profiles = profiles.OrderByDescending(s => s.Name);
                    break;
                case "department_asc":
                    profiles = profiles.OrderBy(s => s.Department.Name);
                    break;
                case "department_desc":
                    profiles = profiles.OrderByDescending(s => s.Department.Name);
                    break;
                case "faculty_asc":
                    profiles = profiles.OrderBy(s => s.Faculty.Name);
                    break;
                case "faculty_desc":
                    profiles = profiles.OrderByDescending(s => s.Faculty.Name);
                    break;
                default:
                    profiles = profiles.OrderBy(s => s.Name);
                    break;
            }

            int pageSize = 30;
            int pageNumber = (page ?? 1);

            return View(profiles.ToPagedList(pageNumber, pageSize));
        }

        // GET: Profiles/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Profile profile = await db.Profiles.FindAsync(id);
            if (profile == null)
            {
                return HttpNotFound();
            }
            return View(profile);
        }

        // GET: Profiles/Create
        public ActionResult Create()
        {
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false), "Id", "Name");
            ViewBag.DirectionOfTrainingId = new SelectList(db.DirectionOfTrainings, "Id", "Name");
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false), "Id", "Name");
            ViewBag.PlanId = new SelectList(db.Plans, "Id", "Name");
            return View();
        }

        // POST: Profiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,ShortName,Code1,Code2,Code3,Period,DirectionOfTrainingId,FacultyId,PlanId,Qualification,DepartmentId,Acceptance,Boss,Secretary,IsDeleted")] Profile profile)
        {
            if (ModelState.IsValid)
            {
                db.Profiles.Add(profile);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false), "Id", "Name", profile.DepartmentId);
            ViewBag.DirectionOfTrainingId = new SelectList(db.DirectionOfTrainings, "Id", "Name", profile.DirectionOfTrainingId);
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false), "Id", "Name", profile.FacultyId);
            ViewBag.PlanId = new SelectList(db.Plans, "Id", "Name", profile.PlanId);
            return View(profile);
        }

        // GET: Profiles/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Profile profile = await db.Profiles.FindAsync(id);
            if (profile == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false), "Id", "Name", profile.DepartmentId);
            ViewBag.DirectionOfTrainingId = new SelectList(db.DirectionOfTrainings, "Id", "Name", profile.DirectionOfTrainingId);
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false), "Id", "Name", profile.FacultyId);
            ViewBag.PlanId = new SelectList(db.Plans, "Id", "Name", profile.PlanId);
            return View(profile);
        }

        // POST: Profiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,ShortName,Code1,Code2,Code3,Period,DirectionOfTrainingId,FacultyId,PlanId,Qualification,DepartmentId,Acceptance,Boss,Secretary,IsDeleted")] Profile profile)
        {
            if (ModelState.IsValid)
            {
                db.Entry(profile).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false), "Id", "Name", profile.DepartmentId);
            ViewBag.DirectionOfTrainingId = new SelectList(db.DirectionOfTrainings, "Id", "Name", profile.DirectionOfTrainingId);
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false), "Id", "Name", profile.FacultyId);
            ViewBag.PlanId = new SelectList(db.Plans, "Id", "Name", profile.PlanId);
            return View(profile);
        }

        // GET: Profiles/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Profile profile = await db.Profiles.FindAsync(id);
            if (profile == null)
            {
                return HttpNotFound();
            }
            return View(profile);
        }

        // POST: Profiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Profile profile = await db.Profiles.FindAsync(id);
            db.Profiles.Remove(profile);
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
