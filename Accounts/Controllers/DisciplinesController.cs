using Accounts.Models;
using Microsoft.AspNet.Identity;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Accounts.Controllers
{
    [Authorize(Roles = "Administrators,Teachers")]
    public class DisciplinesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Disciplines
        public ActionResult Index(string userId)
        {
            if (userId == null)
                userId = User.Identity.GetUserId();

            //получение текущего учебного года
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
            //получаем идентификаторы преподавателя для нагрузки
            var teacherIds = db.TeachersIdDecanat.Where(a => a.SiteId == userId);
            string ids = "dbo.Нагрузка.КодПреподавателя = ";
            foreach (var i in teacherIds)
            {
                if (ids != "dbo.Нагрузка.КодПреподавателя = ") ids += " OR dbo.Нагрузка.КодПреподавателя = ";
                ids += i.DecanatId;
            }

            //получаем нагрузку преподавателя из деканата
            var load = new List<Load>();
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DecanatConnection"].ConnectionString;
            using (var myConnection = new SqlConnection(connectionString))
            //using (var myConnection = new SqlConnection("user id=mih;password=LOR1s2pq;server=192.168.1.14;database=Деканат;"))
            {
                string CommandText = "SELECT TOP 100 PERCENT dbo.Нагрузка.Код AS КодНагрузка, dbo.Преподаватели.Код AS КодПреподавателя, "+
                      "dbo.Нагрузка.КодДисциплины, dbo.Все_Группы.Код AS КодГруппы, dbo.Все_Группы.Название AS Группа, " +
                      "dbo.Нагрузка.Курс, dbo.Нагрузка.Семестр, dbo.Нагрузка.Студентов, CASE WHEN dbo.ПланыДисциплины.Дисциплина IS NULL " +
                      "THEN dbo.Нагрузка.Дисциплина ELSE dbo.ПланыДисциплины.Дисциплина END AS Дисциплина, dbo.Кафедры.Сокращение AS Кафедра, " +
                      "dbo.Нагрузка.ВидЗанятий, dbo.Нагрузка.ВидКонтроля, dbo.Нагрузка.КонтрольныхРабот, dbo.Нагрузка.НагрузкаАуд, " +
                      "dbo.Нагрузка.НагрузкаДр, dbo.Нагрузка.ОписаниеЧасов " +
                      "FROM dbo.Нагрузка INNER JOIN " +
                      "dbo.Преподаватели ON dbo.Нагрузка.КодПреподавателя = dbo.Преподаватели.Код LEFT OUTER JOIN " +
                      "dbo.Кафедры ON dbo.Нагрузка.КодКафедры = dbo.Кафедры.Код LEFT OUTER JOIN " +
                      "dbo.Все_Группы ON dbo.Нагрузка.КодГруппы = dbo.Все_Группы.Код LEFT OUTER JOIN " +
                      "dbo.ПланыДисциплины ON dbo.Нагрузка.КодДисциплины = dbo.ПланыДисциплины.Код " +
                      "WHERE(dbo.Нагрузка.УчебныйГод = '" + currentYear.Value + "') AND (" + ids + ") " +
                      "ORDER BY dbo.Нагрузка.Семестр";

                myConnection.Open(); //Устанавливаем соединение с базой данных.
                var command = new SqlCommand(CommandText, myConnection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var discipline = new Load();
                        discipline.LoadIdDecanat = Convert.ToInt32(reader["КодНагрузка"]);
                        discipline.TeacherId = Convert.ToInt32(reader["КодПреподавателя"]);
                        discipline.DisciplineId = (reader["КодДисциплины"] is DBNull) ? 0 : Convert.ToInt32(reader["КодДисциплины"]);
                        discipline.GroupId = (reader["КодГруппы"] is DBNull) ? 0 : Convert.ToInt32(reader["КодГруппы"]);                        

                        discipline.Group = reader["Группа"].ToString();
                        discipline.Course = reader["Курс"].ToString();
                        discipline.Semester = reader["Семестр"].ToString();
                        discipline.Students = reader["Студентов"].ToString();
                        discipline.Department = reader["Кафедра"].ToString();

                        discipline.Discipline = reader["Дисциплина"].ToString();
                        discipline.TypeLesson = reader["ВидЗанятий"].ToString();
                        discipline.TypeControl = reader["ВидКонтроля"].ToString();
                        discipline.Control = reader["КонтрольныхРабот"].ToString();
                        discipline.LoadClass = reader["НагрузкаАуд"].ToString();
                        discipline.LoadOther = reader["НагрузкаДр"].ToString();
                        discipline.Note = reader["ОписаниеЧасов"].ToString();
                        load.Add(discipline);
                    }
                }
                myConnection.Close();
            }

            return View(load.ToList());
        }

        // GET: Disciplines/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            string userId = User.Identity.GetUserId();
            if (id == null && userId == null && userId == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Discipline discipline = await db.Disciplines.Where(a => a.AuthorId == userId).SingleOrDefaultAsync(m => m.Id == id);
            if (discipline == null)
            {
                return HttpNotFound();
            }
            return View(discipline);
        }

        // GET: Disciplines/Create
        public ActionResult Create()
        {
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false), "Id", "Name");
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false), "Id", "Name");
            ViewBag.ProfileId = new SelectList(db.Profiles.Where(a => a.IsDeleted == false), "Id", "Name");
            return View();
        }

        // POST: Disciplines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,FacultyId,DepartmentId,Name,Hourse,ZET,YearOfStudy,Semester,AcademicYear,FormReporting,ProfileId,TypeTeacherDiscipline,MoodleURL,MoodleNameCourse,TypeTeacherMoodle")] Discipline discipline)
        {
            if (ModelState.IsValid)
            {
                discipline.AuthorId = User.Identity.GetUserId();
                db.Disciplines.Add(discipline);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false), "Id", "Name", discipline.DepartmentId);
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false), "Id", "Name", discipline.FacultyId);
            ViewBag.ProfileId = new SelectList(db.Profiles.Where(a => a.IsDeleted == false), "Id", "Name", discipline.ProfileId);
            return View(discipline);
        }

        // GET: Disciplines/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            string userId = User.Identity.GetUserId();
            if (id == null && userId == null && userId == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Discipline discipline = await db.Disciplines.Where(a => a.AuthorId == userId).SingleOrDefaultAsync(m => m.Id == id);
            if (discipline == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", discipline.DepartmentId);
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", discipline.FacultyId);
            ViewBag.ProfileId = new SelectList(db.Profiles.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", discipline.ProfileId);

            return View(discipline);
        }

        // POST: Disciplines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,FacultyId,DepartmentId,Name,Hourse,ZET,YearOfStudy,Semester,AcademicYear,FormReporting,ProfileId,TypeTeacherDiscipline,MoodleURL,MoodleNameCourse,TypeTeacherMoodle")] Discipline discipline)
        {
            if (ModelState.IsValid)
            {
                discipline.AuthorId = User.Identity.GetUserId();
                db.Entry(discipline).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(a => a.IsDeleted == false), "Id", "Name", discipline.DepartmentId);
            ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false), "Id", "Name", discipline.FacultyId);
            ViewBag.ProfileId = new SelectList(db.Profiles.Where(a => a.IsDeleted == false), "Id", "Name", discipline.ProfileId);

            return View(discipline);
        }

        // GET: Disciplines/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            string userId = User.Identity.GetUserId();
            if (id == null && userId == null && userId == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Discipline discipline = await db.Disciplines.Where(a => a.AuthorId == userId).SingleOrDefaultAsync(m => m.Id == id); ;
            if (discipline == null)
            {
                return HttpNotFound();
            }
            return View(discipline);
        }

        // POST: Disciplines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            string userId = User.Identity.GetUserId();
            if (userId == null && userId == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Discipline discipline = await db.Disciplines.Where(a => a.AuthorId == userId).SingleOrDefaultAsync(m => m.Id == id);
            if (discipline == null)
            {
                return HttpNotFound();
            }
            db.Disciplines.Remove(discipline);
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
