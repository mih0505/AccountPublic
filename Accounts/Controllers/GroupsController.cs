using Accounts.Models;
using Microsoft.AspNet.Identity.Owin;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace Accounts.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class GroupsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public GroupsController()
        {
        }

        public GroupsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

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
        // GET: Groups

        public ActionResult Index(string sortOrder, int? faculty, string name, int? formOfTraining, int? page)
        {
            //получение текущего учебного года
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");

            HttpCookie cookieName = Request.Cookies.Get("group_name");
            HttpCookie cookieFormOfTraining = Request.Cookies.Get("group_formOfTraining");
            HttpCookie cookieSortOrder = Request.Cookies.Get("group_sortOrder");
            HttpCookie cookieFaculty = Request.Cookies.Get("group_faculty");

            if (cookieName != null)
            {
                if (name != null && name != cookieName.Value)
                    cookieName.Value = name;
                else
                    name = cookieName.Value;
            }
            if (cookieSortOrder != null)
            {
                sortOrder = cookieSortOrder.Value;
            }
            if (cookieFaculty != null)
            {
                if (faculty != null && faculty != Convert.ToInt32(cookieFaculty.Value))
                    cookieFaculty.Value = faculty.ToString();
                else
                    faculty = Convert.ToInt32(cookieFaculty.Value);
            }

            if (cookieFormOfTraining != null)
            {
                if (formOfTraining != null && formOfTraining != Convert.ToInt32(cookieFormOfTraining.Value))
                    cookieFormOfTraining.Value = formOfTraining.ToString();
                else
                    formOfTraining = Convert.ToInt32(cookieFormOfTraining.Value);
            }

            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.FacultySortParm = sortOrder == "faculty_asc" ? "faculty_desc" : "faculty_asc";
            ViewBag.FormSortParm = sortOrder == "form_asc" ? "form_desc" : "form_asc";
            ViewBag.CourseSortParm = sortOrder == "course_asc" ? "course_desc" : "course_asc";

            ViewBag.FacultyFilter = faculty;
            ViewBag.NameFilter = name;
            ViewBag.FormFilter = formOfTraining;


            //фильтр групп
            IQueryable<Group> groups = db.Groups.Where(a => a.IsDeleted == false && a.AcademicYear == currentYear.Value).Include(g => g.Faculty).Include(g => g.FormOfTraining)
                .Include(g => g.Profile).OrderBy(c => c.Name);

            if (faculty != null || (faculty != null && faculty.ToString() != cookieFaculty.Value))
            {
                cookieFaculty = new HttpCookie("group_faculty");
                cookieFaculty.Value = faculty.ToString();
                cookieFaculty.Expires = DateTime.Now.AddMinutes(15);
                Response.Cookies.Add(cookieFaculty);
                groups = groups.Where(p => p.FacultyId == faculty);
            }
            if (!String.IsNullOrEmpty(name) || (!String.IsNullOrEmpty(name) && name != cookieName.Value))
            {
                cookieName = new HttpCookie("group_name");
                cookieName.Value = name;
                cookieName.Expires = DateTime.Now.AddMinutes(15);
                Response.Cookies.Add(cookieName);
                groups = groups.Where(p => SqlFunctions.PatIndex("%" + name + "%", p.Name) > 0);
            }

            if (formOfTraining != null || (formOfTraining != null && formOfTraining.ToString() != cookieFormOfTraining.Value))
            {
                cookieFormOfTraining = new HttpCookie("group_formOfTraining");
                cookieFormOfTraining.Value = faculty.ToString();
                cookieFormOfTraining.Expires = DateTime.Now.AddMinutes(15);
                Response.Cookies.Add(cookieFaculty);
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

            ViewBag.FacultyList = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");
            ViewBag.FormOfTraining = new SelectList(db.FormOfTrainings, "Id", "Name");

            int pageSize = 30;
            int pageNumber = (page ?? 1);

            return View(groups.ToPagedList(pageNumber, pageSize));
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

            return RedirectToAction("Index");
        }
               
        
        public ActionResult FromCourseOnCourse()
        {
            Settings currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
            //получение соответствий ид групп в новом учебном году
            var lstAccordanceGroups = GetAccordanceIdGroups(currentYear);
            foreach (var d in lstAccordanceGroups)
            {
                int groupIdSiteFrom = db.Groups.FirstOrDefault(a => a.DecanatID == d.GroupIdDecanatFrom).Id;
                var groupIdSiteIn = db.Groups.FirstOrDefault(a => a.DecanatID == d.GroupIdDecanatIn);
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        //обновление ид групп у студентов
                        db.Database.ExecuteSqlCommand(
                            $"UPDATE AspNetUsers SET GroupId = {groupIdSiteIn.Id}, idGroupDecanat = {d.GroupIdDecanatIn}, " +
                            $"FacultyId = {groupIdSiteIn.FacultyId }, idFacultyDecanat = {d.FacultyIdIn}, idProfileDecanat = {groupIdSiteIn.idPlanDecanat} " +
                            $"WHERE idGroupDecanat = {d.GroupIdDecanatFrom}");
                        //обновление ид групп в ведомостях
                        db.Database.ExecuteSqlCommand(
                            $"UPDATE Statements SET GroupId = {groupIdSiteIn.Id}, GroupIdDecanate = {d.GroupIdDecanatIn}, " +
                            $"ProfileId = {groupIdSiteIn.ProfileId}, FacultyId = {groupIdSiteIn.FacultyId } " +
                            $"WHERE GroupIdDecanate = {d.GroupIdDecanatFrom}");
                        db.Database.ExecuteSqlCommand(
                           $"UPDATE StatementStudents SET GroupIdSite = {groupIdSiteIn.Id}, GroupIdDecanate = {d.GroupIdDecanatIn} " +
                           $"WHERE GroupIdDecanate = {d.GroupIdDecanatFrom}");
                        //обновление ид групп в курсовых
                        db.Database.ExecuteSqlCommand(
                            $"UPDATE Courses SET GroupId = {groupIdSiteIn.Id}, GroupIdDecanate = {d.GroupIdDecanatIn}, " +
                            $"FacultyId = {groupIdSiteIn.FacultyId } " +
                            $"WHERE GroupIdDecanate = {d.GroupIdDecanatFrom}");
                        db.Database.ExecuteSqlCommand(
                            $"UPDATE CourseWorkStudents SET GroupIdSite = {groupIdSiteIn.Id}, GroupIdDecanate = {d.GroupIdDecanatIn} " +
                            $"WHERE GroupIdDecanate = {d.GroupIdDecanatFrom}");
                        //очистка привязки тьюторов к группам


                        db.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                    }
                }
            }

            return View();
        }

        private List<AccordanceGroups> GetAccordanceIdGroups(Settings currentYear)
        {
            var lstAccordanceGroups = new List<AccordanceGroups>();

            using (var connection = new SqlConnection("user id=mih;password=LOR1s2pq;server=192.168.1.14;database=Деканат;"))
            {
                string sql = "SELECT DISTINCT КодГруппыИз, КодГруппыВ, КодФакультетаВ " +
                      "FROM dbo.Перемещения " +
                      "WHERE Тип_Перемещения = 'Перевести с курса на курс' AND Учебный_год = '" + currentYear.Value + "' ";
                var command = new SqlCommand(sql, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lstAccordanceGroups.Add(new AccordanceGroups
                        {
                            GroupIdDecanatFrom = Convert.ToInt32(reader["КодГруппыИз"]),
                            GroupIdDecanatIn = Convert.ToInt32(reader["КодГруппыВ"]),
                            FacultyIdIn = Convert.ToInt32(reader["КодФакультетаВ"])
                        });
                    }
                    connection.Close();
                }
            }
            return lstAccordanceGroups;
        }
                                        
        public async Task<ActionResult> StudentsGroup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var group = db.Groups.Find(id);
            ViewBag.CurrentGroup = group;
            var listStudents = await db.Users.Where(a => a.GroupId == id && a.DateBlocked == null).OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).ThenBy(a => a.Middlename).ToListAsync();
            ViewBag.Count = listStudents.Count;

            return View(listStudents);
        }
                       
        // GET: Groups/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = await db.Groups.FindAsync(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }


        // GET: Groups/Create
        public ActionResult Create()
        {
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false), "Id", "Name");
            ViewBag.FormOfTrainingId = new SelectList(db.FormOfTrainings, "Id", "Name");
            ViewBag.ProfileId = new SelectList(db.Profiles.Where(a => a.IsDeleted == false), "Id", "Name");
            return View();
        }

        // POST: Groups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,DecanatID,MoodleID,PlanId,ProfileId,Course,FacultyId,YearOfReceipt,AcademicYear,Period,FormOfTrainingId,IsDeleted")] Group group)
        {
            if (ModelState.IsValid)
            {
                db.Groups.Add(group);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false), "Id", "Name", group.FacultyId);
            ViewBag.FormOfTrainingId = new SelectList(db.FormOfTrainings, "Id", "Name", group.FormOfTrainingId);
            ViewBag.ProfileId = new SelectList(db.Profiles.Where(a => a.IsDeleted == false), "Id", "Name", group.ProfileId);
            return View(group);
        }

        // GET: Groups/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = await db.Groups.FindAsync(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false), "Id", "Name", group.FacultyId);
            ViewBag.FormOfTrainingId = new SelectList(db.FormOfTrainings, "Id", "Name", group.FormOfTrainingId);
            ViewBag.ProfileId = new SelectList(db.Profiles.Where(a => a.IsDeleted == false), "Id", "Name", group.ProfileId);
            return View(group);
        }

        // POST: Groups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,DecanatID,MoodleID,PlanId,ProfileId,Course,FacultyId,YearOfReceipt,AcademicYear,Period,FormOfTrainingId,IsDeleted")] Group group)
        {
            if (ModelState.IsValid)
            {
                db.Entry(group).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false), "Id", "Name", group.FacultyId);
            ViewBag.FormOfTrainingId = new SelectList(db.FormOfTrainings, "Id", "Name", group.FormOfTrainingId);
            ViewBag.ProfileId = new SelectList(db.Profiles.Where(a => a.IsDeleted == false), "Id", "Name", group.ProfileId);
            return View(group);
        }

        // GET: Groups/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = await db.Groups.FindAsync(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Group group = await db.Groups.FindAsync(id);
            group.IsDeleted = true;
            db.Entry(group).State = EntityState.Modified;

            //блокировка выпускников группы
            var lstStudents = db.Users.Where(a => a.GroupId == group.Id).ToList();
            foreach (var student in lstStudents)
            {
                student.DateBlocked = DateTime.Now;
                student.Email = student.DateBlocked.Value.ToShortDateString() + "___" + student.Email;
                db.Entry(student).State = EntityState.Modified;
            }

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
