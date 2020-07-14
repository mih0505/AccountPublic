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
using System.Data.Entity.Validation;

namespace Accounts.Controllers
{
    public class LessonsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Lessons/Create
        public ActionResult Create(int? journalId)
        {
            if (journalId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var lessonFull = new LessonFull();
            lessonFull.Lesson = new Lesson();
            lessonFull.Study = new List<StudiesViewModel>();
            lessonFull.TypeLesson = new SelectList(listTypes, "Value", "Text");
            //получаем текущий журнал
            lessonFull.Lesson.JournalId = (int)journalId;
            lessonFull.Lesson.Date = DateTime.Now;
            lessonFull.Lesson.Journal = db.Journals
                .Include(a => a.Faculty)
                .Include(s => s.Group)
                .Include(s => s.TeacherName)
                .FirstOrDefault(a => a.Id == journalId);

            //получаем список студентов группы
            var students = db.Users
                .Where(a => a.GroupId == lessonFull.Lesson.Journal.GroupId && a.DateBlocked == null)
                .OrderBy(a => a.Lastname)
                .ThenBy(a => a.Firstname)
                .ThenBy(a => a.Middlename)
                .ToList();

            foreach (var s in students)
            {
                lessonFull.Study.Add(new StudiesViewModel
                {
                    Study = new Study { JournalId = journalId, StudentId = s.Id, Student = s },
                    Grades1 = new SelectList(listGrades, "Value", "Text"),
                    Grades2 = new SelectList(listGrades, "Value", "Text"),
                    Grades3 = new SelectList(listGrades, "Value", "Text"),
                });
            }

            return PartialView("Create", lessonFull);
        }

        // POST: Lessons/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(LessonFull lessonFull)
        {
            //if (ModelState.IsValid)
            //{
            if (lessonFull != null)
            {
                //добавление занятия                
                db.Lessons.Add(lessonFull.Lesson);
                await db.SaveChangesAsync();
                //добавление оценок студентов
                foreach (var st in lessonFull.Study)
                {
                    if (st.Study.Grade1 != null || st.Study.Grade2 != null || st.Study.Grade3 != null)
                    {
                        db.Studies.Add(new Study
                        {
                            Grade1 = st.Study.Grade1,
                            Grade2 = st.Study.Grade2,
                            Grade3 = st.Study.Grade3,
                            JournalId = lessonFull.Lesson.JournalId,
                            LessonId = lessonFull.Lesson.Id,
                            StudentId = st.Study.Student.Id,                            
                        });
                    }
                }
                await db.SaveChangesAsync();
            }
            lessonFull.TypeLesson = new SelectList(listTypes, "Value", "Text", lessonFull.Lesson.TypeLesson);
            return RedirectToAction("Details", "Journals", new { id = lessonFull.Lesson.JournalId });
            //}
            //return View(lessonFull);
        }

        public List<SelectListItem> listGrades = new List<SelectListItem>(new[] {
            new SelectListItem
            {
                Text = "н",
                Value = "н"
            },
            new SelectListItem
            {
                Text = "б",
                Value = "б"
            },
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
            },
             new SelectListItem
            {
                Text = "7",
                Value = "7"
            },
             new SelectListItem
            {
                Text = "8",
                Value = "8"
            },
             new SelectListItem
            {
                Text = "9",
                Value = "9"
            },
             new SelectListItem
            {
                Text = "10",
                Value = "10"
            },
             new SelectListItem
            {
                Text = "зачтено",
                Value = "зачтено"
            },
             new SelectListItem
            {
                Text = "не зачтено",
                Value = "не зачтено"
            }
        });

        public List<SelectListItem> listTypes = new List<SelectListItem>(new[] {
            new SelectListItem
            {
                Text = "Урок",
                Value = "Урок"
            },
            new SelectListItem
            {
                Text = "Лекция",
                Value = "Лекция"
            },
            new SelectListItem
            {
                Text = "Практика",
                Value = "Практика"
            },
            new SelectListItem
            {
                Text = "Семинар",
                Value = "Семинар"
            },
            new SelectListItem
            {
                Text = "Лабораторная",
                Value = "Лабораторная"
            },
             new SelectListItem
            {
                Text = "Консультация",
                Value = "Консультация"
            },
             new SelectListItem
            {
                Text = "Итоговая оценка",
                Value = "Итоговая оценка"
            }
        });

        // GET: Lessons/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var lessonFull = new LessonFull();
            lessonFull.Lesson = await db.Lessons.FindAsync(id);
            lessonFull.Study = new List<StudiesViewModel>();
            lessonFull.TypeLesson = new SelectList(listTypes, "Value", "Text", lessonFull.Lesson.TypeLesson);
            //получаем текущий журнал            
            lessonFull.Lesson.Journal = db.Journals
                .Include(a => a.Faculty)
                .Include(s => s.Group)
                .Include(s => s.TeacherName)
                .FirstOrDefault(a => a.Id == lessonFull.Lesson.JournalId);

            //получаем список студентов группы
            var students = db.Users
                .Where(a => a.GroupId == lessonFull.Lesson.Journal.GroupId && a.DateBlocked == null)
                .OrderBy(a => a.Lastname)
                .ThenBy(a => a.Firstname)
                .ThenBy(a => a.Middlename)
                .ToList();

            //получаем список студентов получивших оценку
            var studies = db.Studies
                .Where(a => a.LessonId == lessonFull.Lesson.Id)
                .Include(a => a.Student)
                .OrderBy(a => a.Student.Lastname)
                .ThenBy(a => a.Student.Firstname)
                .ThenBy(a => a.Student.Middlename)
                .ToList();
            foreach (var s in students)
            {
                var studyStudent = studies.FirstOrDefault(a => a.StudentId == s.Id && (a.Grade1 != null || a.Grade2 != null || a.Grade3 != null));
                if (studyStudent != null)
                {
                    lessonFull.Study.Add(new StudiesViewModel
                    {
                        Study = new Study { JournalId = lessonFull.Lesson.JournalId, StudentId = s.Id, Student = s },
                        Grades1 = new SelectList(listGrades, "Value", "Text", studyStudent.Grade1),
                        Grades2 = new SelectList(listGrades, "Value", "Text", studyStudent.Grade2),
                        Grades3 = new SelectList(listGrades, "Value", "Text", studyStudent.Grade3),
                    });
                }
                else
                {
                    lessonFull.Study.Add(new StudiesViewModel
                    {
                        Study = new Study { JournalId = lessonFull.Lesson.JournalId, StudentId = s.Id, Student = s },
                        Grades1 = new SelectList(listGrades, "Value", "Text"),
                        Grades2 = new SelectList(listGrades, "Value", "Text"),
                        Grades3 = new SelectList(listGrades, "Value", "Text"),
                    });
                }
            }

            return PartialView("Edit", lessonFull);
        }

        // POST: Lessons/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(LessonFull lessonFull)
        {
            if (lessonFull != null)
            {
                //редактирование занятия
                var lesson = db.Lessons.FirstOrDefault(a => a.Id == lessonFull.Lesson.Id);
                if (lesson != null)
                {
                    lesson.Date = lessonFull.Lesson.Date;
                    lesson.HomeWork = lessonFull.Lesson.HomeWork;
                    lesson.Note = lessonFull.Lesson.Note;
                    lesson.Number = lessonFull.Lesson.Number;
                    lesson.Topic = lessonFull.Lesson.Topic;
                    lesson.TypeLesson = lessonFull.Lesson.TypeLesson;
                    db.Entry(lesson).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }

                //сохранение оценок студентов
                //получаем список оценок журнала сохраненных в базе
                var lstGrades = db.Studies.Where(a => a.JournalId == lessonFull.Lesson.JournalId).ToList();

                foreach (var st in lessonFull.Study)
                {
                    var currentGradeStudent = lstGrades.FirstOrDefault(a => a.LessonId == lessonFull.Lesson.Id && a.StudentId == st.Study.Student.Id);
                    if (currentGradeStudent != null)
                    //редактирование старых оценок
                    {
                        if (currentGradeStudent.Grade1 != st.Study.Grade1 ||
                            currentGradeStudent.Grade2 != st.Study.Grade2 ||
                            currentGradeStudent.Grade3 != st.Study.Grade3)
                        {
                            currentGradeStudent.Grade1 = st.Study.Grade1;
                            currentGradeStudent.Grade2 = st.Study.Grade2;
                            currentGradeStudent.Grade3 = st.Study.Grade3;

                            db.Entry(currentGradeStudent).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        //добавление новых оценок
                        db.Studies.Add(new Study
                        {
                            Grade1 = st.Study.Grade1,
                            Grade2 = st.Study.Grade2,
                            Grade3 = st.Study.Grade3,
                            JournalId = lessonFull.Lesson.JournalId,
                            LessonId = lessonFull.Lesson.Id,
                            StudentId = st.Study.Student.Id
                        });

                    }
                }
                try
                {
                    int i = await db.SaveChangesAsync();
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var errors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in errors.ValidationErrors)
                        {
                            string errorMessage = validationError.ErrorMessage;
                        }
                    }
                }
            }
            lessonFull.TypeLesson = new SelectList(listTypes, "Value", "Text", lessonFull.Lesson.TypeLesson);

            return RedirectToAction("Details", "Journals", new { id = lessonFull.Lesson.JournalId });
        }

        // GET: Lessons/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lesson lesson = await db.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return HttpNotFound();
            }
            return View(lesson);
        }

        // POST: Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Lesson lesson = await db.Lessons.FindAsync(id);
            int journalId = lesson.JournalId;
            db.Lessons.Remove(lesson);
            await db.SaveChangesAsync();
            return RedirectToAction("Details", "Journals", new { id = journalId });
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
