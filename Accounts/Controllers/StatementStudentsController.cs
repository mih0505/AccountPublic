using Accounts.Models;
using Microsoft.AspNet.Identity;
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
    public class StatementStudentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: StatementStudents
        [Authorize(Roles = "Students,Teachers,Administrators,FacultiesManagers")]
        public async Task<ActionResult> Index(string id)
        {
            string userId;
            if (id == null)
                userId = User.Identity.GetUserId();
            else
                userId = id;
            var user = db.Users.FirstOrDefault(a => a.Id == userId);

            var gradeAndCompetences = new GradesAndCompetences();

            gradeAndCompetences.Grades = await (from statStud in db.StatementStudents
                                                join stat in db.Statements on statStud.StatementId equals stat.Id
                                                join students in db.Users on statStud.StudentStatementId equals students.Id
                                                join teachers in db.Users on statStud.TeacherStatementId equals teachers.Id into outerTeacher
                                                from teachers in outerTeacher.DefaultIfEmpty()
                                                join teachers2 in db.Users on stat.TeacherDiscipline2Id equals teachers2.Id into outerTeacher2
                                                from teachers2 in outerTeacher2.DefaultIfEmpty()
                                                join teachers3 in db.Users on stat.TeacherDiscipline3Id equals teachers3.Id into outerTeacher3
                                                from teachers3 in outerTeacher3.DefaultIfEmpty()
                                                join teachers4 in db.Users on stat.TeacherDiscipline4Id equals teachers4.Id into outerTeacher4
                                                from teachers4 in outerTeacher4.DefaultIfEmpty()
                                                join teachers5 in db.Users on stat.TeacherDiscipline5Id equals teachers5.Id into outerTeacher5
                                                from teachers5 in outerTeacher5.DefaultIfEmpty()
                                                join teachers6 in db.Users on stat.TeacherDiscipline6Id equals teachers6.Id into outerTeacher6
                                                from teachers6 in outerTeacher6.DefaultIfEmpty()
                                                join teachers7 in db.Users on stat.TeacherDiscipline7Id equals teachers7.Id into outerTeacher7
                                                from teachers7 in outerTeacher7.DefaultIfEmpty()
                                                join statements in db.Statements on statStud.StatementId equals statements.Id
                                                join teacherStatement in db.Users on statements.TeacherDisciplineId equals teacherStatement.Id into outerTeacherStatement
                                                from teacherStatement in outerTeacherStatement.DefaultIfEmpty()
                                                where statStud.StudentStatementId == user.Id && stat.GroupId == user.GroupId
                                                && statStud.Grade != null && statStud.Grade != "Не явился" && statStud.Grade != "Не удовлетворительно" && statStud.Grade != "Не зачтено" && statStud.Grade != "Отчислен/Перевод"
                                                orderby stat.Semester, stat.TypeControl, stat.NameDiscipline
                                                select (new StatementViewModel
                                                {
                                                    StatementId = stat.Id,
                                                    NameDiscipline = stat.NameDiscipline,
                                                    TypeControl = stat.TypeControl,
                                                    Hours = stat.ZET + "/" + stat.Hours,
                                                    Teacher = teachers.Lastname + " " + teachers.Firstname.Substring(0, 1) + "." + teachers.Middlename.Substring(0, 1) + ". ",
                                                    Teacher2 = teachers2 != null ? teachers2.Lastname + " " + teachers2.Firstname.Substring(0, 1) + "." + teachers2.Middlename.Substring(0, 1) + "." : "",
                                                    Teacher3 = teachers3 != null ? teachers3.Lastname + " " + teachers3.Firstname.Substring(0, 1) + "." + teachers3.Middlename.Substring(0, 1) + "." : "",
                                                    Teacher4 = teachers4 != null ? teachers4.Lastname + " " + teachers4.Firstname.Substring(0, 1) + "." + teachers4.Middlename.Substring(0, 1) + "." : "",
                                                    Teacher5 = teachers5 != null ? teachers5.Lastname + " " + teachers5.Firstname.Substring(0, 1) + "." + teachers5.Middlename.Substring(0, 1) + "." : "",
                                                    Teacher6 = teachers6 != null ? teachers6.Lastname + " " + teachers6.Firstname.Substring(0, 1) + "." + teachers6.Middlename.Substring(0, 1) + "." : "",
                                                    Teacher7 = teachers7 != null ? teachers7.Lastname + " " + teachers7.Firstname.Substring(0, 1) + "." + teachers7.Middlename.Substring(0, 1) + "." : "",
                                                    TeacherStatement = teacherStatement != null ? teacherStatement.Lastname + " " + teacherStatement.Firstname.Substring(0, 1) + "." + teacherStatement.Middlename.Substring(0, 1) + "." : "",
                                                    Course = stat.Course,
                                                    Semester = stat.Semester,
                                                    Date = statStud.Date,
                                                    Grade = statStud.Grade,
                                                })).ToListAsync();

            var group = db.Groups.FirstOrDefault(a => a.Id == user.GroupId);

            //получение списка компетенций по программе
            var lstCompetences = new List<Competence>();
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DecanatConnection"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT dbo.ПланыСтроки.Дисциплина, dbo.ПланыСтроки.Компетенции, dbo.ПланыКомпетенции.ШифрКомпетенции, " +
                             "dbo.ПланыКомпетенции.Наименование " +
                             "FROM dbo.ПланыСтроки INNER JOIN " +
                             "dbo.ПланыКомпетенцииДисциплины ON dbo.ПланыСтроки.Код = dbo.ПланыКомпетенцииДисциплины.КодСтроки INNER JOIN " +
                             "dbo.ПланыКомпетенции ON dbo.ПланыКомпетенцииДисциплины.КодКомпетенции = dbo.ПланыКомпетенции.Код " +
                             "WHERE(dbo.ПланыСтроки.КодПлана = " + group.idPlanDecanat + ") " +
                             "ORDER BY dbo.ПланыКомпетенции.ШифрКомпетенции, dbo.ПланыСтроки.Дисциплина";
                var command = new SqlCommand(sql, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lstCompetences.Add(new Competence
                        {
                            Discipline = reader["Дисциплина"].ToString(),
                            Competences = reader["Компетенции"].ToString(),
                            Name = reader["Наименование"].ToString(),
                            CodeCompetence = reader["ШифрКомпетенции"].ToString()
                        });
                    }
                    connection.Close();
                }

                //ввод компетенций в списке дисциплин
                foreach (var disp in gradeAndCompetences.Grades)
                {
                    var c = lstCompetences.FirstOrDefault(a => a.Discipline == disp.NameDiscipline);
                    if (c != null)
                        disp.Competences = c.Competences;
                }
            }
            ViewBag.CountSemesters = gradeAndCompetences.Grades.Select(m => m.Semester).Distinct().ToList();

            //получение списка компетенций
            gradeAndCompetences.Codes = lstCompetences.Select(a => new CompetenceResults { Code = a.CodeCompetence, Name = a.Name })
                .OrderBy(a => a.Code).Distinct().ToList();
            

            gradeAndCompetences.Competences = new List<Competence>();
            gradeAndCompetences.Competences.AddRange(lstCompetences);
            foreach (var disp in gradeAndCompetences.Competences)
            {
                var d = gradeAndCompetences.Grades.FirstOrDefault(a => a.NameDiscipline == disp.Discipline);
                if (d != null && !string.IsNullOrEmpty(d.Grade) && d.Grade != "Не явился" && d.Grade != "Неудовлетворительно" && d.Grade != "Не зачтено" && d.Grade != "Отчислен/Перевод")
                {
                    disp.Grade = d.Grade;
                }
            }

            //расчет результата освоения по компетенциям
            foreach (var l in gradeAndCompetences.Codes)
            {
                var countDisciplinesForCompetence = gradeAndCompetences.Competences.Count(a => a.CodeCompetence == l.Code);
                var countDisciplinesWithGradesCompetence = gradeAndCompetences.Competences.Count(a => a.CodeCompetence == l.Code && a.Grade != null);

                if (countDisciplinesWithGradesCompetence == 0)
                {
                    l.Result = "Не освоена";
                }
                else if (countDisciplinesWithGradesCompetence == countDisciplinesForCompetence)
                {
                    l.Result = "Освоена";
                }
                else
                {
                    l.Result = "Частично освоена";
                }
            }

            return View(gradeAndCompetences);
        }

        [Authorize(Roles = "Students")]
        public async Task<ActionResult> Rating()
        {
            var userId = User.Identity.GetUserId();

            var statementStudents = from statStud in db.StatementStudents
                                    join stat in db.Statements on statStud.StatementId equals stat.Id
                                    join students in db.Users on statStud.StudentStatementId equals students.Id
                                    join teachers in db.Users on statStud.TeacherStatementId equals teachers.Id into outerTeacher
                                    from teachers in outerTeacher.DefaultIfEmpty()
                                    join teachers2 in db.Users on stat.TeacherDiscipline2Id equals teachers2.Id into outerTeacher2
                                    from teachers2 in outerTeacher2.DefaultIfEmpty()
                                    join teachers3 in db.Users on stat.TeacherDiscipline3Id equals teachers3.Id into outerTeacher3
                                    from teachers3 in outerTeacher3.DefaultIfEmpty()
                                    join teachers4 in db.Users on stat.TeacherDiscipline4Id equals teachers4.Id into outerTeacher4
                                    from teachers4 in outerTeacher4.DefaultIfEmpty()
                                    join teachers5 in db.Users on stat.TeacherDiscipline5Id equals teachers5.Id into outerTeacher5
                                    from teachers5 in outerTeacher5.DefaultIfEmpty()
                                    join teachers6 in db.Users on stat.TeacherDiscipline6Id equals teachers6.Id into outerTeacher6
                                    from teachers6 in outerTeacher6.DefaultIfEmpty()
                                    join teachers7 in db.Users on stat.TeacherDiscipline7Id equals teachers7.Id into outerTeacher7
                                    from teachers7 in outerTeacher7.DefaultIfEmpty()
                                    join statements in db.Statements on statStud.StatementId equals statements.Id
                                    join teacherStatement in db.Users on statements.TeacherDisciplineId equals teacherStatement.Id into outerTeacherStatement
                                    from teacherStatement in outerTeacherStatement.DefaultIfEmpty()
                                    where students.Id == userId
                                    orderby stat.Semester
                                    select (new StatementViewModel
                                    {
                                        NameDiscipline = stat.NameDiscipline,
                                        TypeControl = stat.TypeControl,
                                        Teacher = teachers != null ? teachers.Lastname + " " + teachers.Firstname.Substring(0, 1) + "." + teachers.Middlename.Substring(0, 1) + "." : "",
                                        Teacher2 = teachers2 != null ? teachers2.Lastname + " " + teachers2.Firstname.Substring(0, 1) + "." + teachers2.Middlename.Substring(0, 1) + "." : "",
                                        Teacher3 = teachers3 != null ? teachers3.Lastname + " " + teachers3.Firstname.Substring(0, 1) + "." + teachers3.Middlename.Substring(0, 1) + "." : "",
                                        Teacher4 = teachers4 != null ? teachers4.Lastname + " " + teachers4.Firstname.Substring(0, 1) + "." + teachers4.Middlename.Substring(0, 1) + "." : "",
                                        Teacher5 = teachers5 != null ? teachers5.Lastname + " " + teachers5.Firstname.Substring(0, 1) + "." + teachers5.Middlename.Substring(0, 1) + "." : "",
                                        Teacher6 = teachers6 != null ? teachers6.Lastname + " " + teachers6.Firstname.Substring(0, 1) + "." + teachers6.Middlename.Substring(0, 1) + "." : "",
                                        Teacher7 = teachers7 != null ? teachers7.Lastname + " " + teachers7.Firstname.Substring(0, 1) + "." + teachers7.Middlename.Substring(0, 1) + "." : "",
                                        TeacherStatement = teacherStatement != null ? teacherStatement.Lastname + " " + teacherStatement.Firstname.Substring(0, 1) + "." + teacherStatement.Middlename.Substring(0, 1) + "." : "",
                                        Course = stat.Course,
                                        Semester = stat.Semester,
                                        Date = statStud.Date,
                                        Grade = statStud.Grade,
                                        TotalPoint = statStud.TotalPoint,
                                        GradeByNumber = statStud.GradeByNumber,
                                    });
            ViewBag.CountSemesters = statementStudents.Select(m => m.Semester).Distinct().ToList();
            if (ViewBag.CountSemesters.Count != 0)
            {
                ViewBag.StudentRating = (double)statementStudents.Sum(a => a.TotalPoint) / 100;
                var r = statementStudents.Where(a => a.GradeByNumber > 0).Select(a => a.GradeByNumber).ToList();
                ViewBag.Avg = (r.Count != 0) ? r.Average() : 0;
            }
            return View(await statementStudents.ToListAsync());
        }

        //[Authorize(Roles = "Administrators,FacultiesManagers,Teachers")]
        //// GET: StatementStudents/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    StatementStudent statementStudent = await db.StatementStudents.FindAsync(id);
        //    if (statementStudent == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(statementStudent);
        //}

        //[Authorize(Roles = "Administrators,FacultiesManagers,Teachers")]
        //// GET: StatementStudents/Create
        //public ActionResult Create()
        //{
        //    ViewBag.StatementId = new SelectList(db.Statements, "Id", "NameDiscipline");
        //    ViewBag.StudentStatementId = new SelectList(db.Users, "Id", "Lastname");
        //    ViewBag.TeacherStatementId = new SelectList(db.Users, "Id", "Lastname");
        //    return View();
        //}

        //// POST: StatementStudents/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "Id,GroupIdSite,GroupIdDecanate,Date,StudentStatementId,TeacherStatementId,IdStudentDecanate,PointSemester,PointAdvanced,PointControl,TotalPoint,Grade,StatementId,IsBlocked")] StatementStudent statementStudent)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.StatementStudents.Add(statementStudent);
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.StatementId = new SelectList(db.Statements, "Id", "NameDiscipline", statementStudent.StatementId);
        //    ViewBag.StudentStatementId = new SelectList(db.Users, "Id", "Lastname", statementStudent.StudentStatementId);
        //    ViewBag.TeacherStatementId = new SelectList(db.Users, "Id", "Lastname", statementStudent.TeacherStatementId);
        //    return View(statementStudent);
        //}

        //[Authorize(Roles = "Administrators,FacultiesManagers,Teachers")]
        //// GET: StatementStudents/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    StatementStudent statementStudent = await db.StatementStudents.FindAsync(id);
        //    if (statementStudent == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.StatementId = new SelectList(db.Statements, "Id", "NameDiscipline", statementStudent.StatementId);
        //    ViewBag.StudentStatementId = new SelectList(db.Users, "Id", "Lastname", statementStudent.StudentStatementId);
        //    ViewBag.TeacherStatementId = new SelectList(db.Users, "Id", "Lastname", statementStudent.TeacherStatementId);
        //    return View(statementStudent);
        //}

        //// POST: StatementStudents/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "Id,GroupIdSite,GroupIdDecanate,Date,StudentStatementId,TeacherStatementId,IdStudentDecanate,PointSemester,PointAdvanced,PointControl,TotalPoint,Grade,StatementId,IsBlocked")] StatementStudent statementStudent)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(statementStudent).State = EntityState.Modified;
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.StatementId = new SelectList(db.Statements, "Id", "NameDiscipline", statementStudent.StatementId);
        //    ViewBag.StudentStatementId = new SelectList(db.Users, "Id", "Lastname", statementStudent.StudentStatementId);
        //    ViewBag.TeacherStatementId = new SelectList(db.Users, "Id", "Lastname", statementStudent.TeacherStatementId);
        //    return View(statementStudent);
        //}

        [Authorize(Roles = "Administrators")]
        // GET: StatementStudents/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StatementStudent statementStudent = await db.StatementStudents.FindAsync(id);
            if (statementStudent == null)
            {
                return HttpNotFound();
            }
            return View(statementStudent);
        }

        // POST: StatementStudents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            StatementStudent statementStudent = await db.StatementStudents.FindAsync(id);
            db.StatementStudents.Remove(statementStudent);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", "Statements");
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
