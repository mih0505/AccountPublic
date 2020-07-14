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
using System.IO;

namespace Accounts.Controllers
{
    public class DiplomWorksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: DiplomWorks
        [Authorize(Roles = "Students,Administrators,SecretariesGIA,FacultiesManagers")]
        public async Task<ActionResult> Index(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var diplomWorks = db.DiplomWorks
                .Include(d => d.StudentDiplom)
                .Include(d => d.TeacherDiplom)
                .Where(a => a.StudentId == id);

            return View(await diplomWorks.ToListAsync());
        }

        public FileResult GetFile(string path, string name)
        {
            byte[] mas = System.IO.File.ReadAllBytes(path);
            string file_path = Server.MapPath("~/DiplomWorks/" + name);
            string file_type = "application/octet-stream";
            return File(mas, file_type, name);
        }

        public async Task<JsonResult> GetTeachers()
        {
            List<TutorsList> teachersList;            
            //получаем список ид кафедр на факультете
            //var departmentsIds = db.Departments.Where(a => a.Id == depId && a.IsDeleted == false).Select(a => a.Id).ToList();
            //получаем список преподавателей кафедр
            teachersList = await db.TeacherDepartments
                    .Include(a => a.Teacher)
                    .Where(a => a.Teacher.DateBlocked == null)
                    .OrderBy(a => a.Teacher.Lastname)
                    .ThenBy(a => a.Teacher.Firstname)
                    .ThenBy(a => a.Teacher.Middlename)
                    .Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." })
                    .ToListAsync();

            return Json(new SelectList(teachersList, "Id", "Name"));
        }

        // GET: DiplomWorks/Edit/5
        [Authorize(Roles = "Students,Administrators,SecretariesGIA,FacultiesManagers")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DiplomWork diplomWork = await db.DiplomWorks
                .Include(a => a.GIA)
                .Include(a => a.StudentDiplom)
                .Include(a => a.TeacherDiplom)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (diplomWork == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId();
            var user = await db.Users.FirstOrDefaultAsync(a => a.Id == userId);

            //проверка на соответствие работы студенту
            if (User.IsInRole("Students") && userId != diplomWork.StudentId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //получаем список преподавателей
            var depList = await db.Departments.Where(a => a.FacultyId == diplomWork.GIA.FacultyId).Select(a => a.Id).ToListAsync();
            List<TutorsList> lstTeacher = await db.TeacherDepartments                    
                    .Include(a => a.Teacher)
                    .Where(a => depList.Contains(a.DepartmentId) && a.Teacher.DateBlocked == null)
                    .OrderBy(a => a.Teacher.Lastname)
                    .ThenBy(a => a.Teacher.Firstname)
                    .ThenBy(a => a.Teacher.Middlename)
                    .Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." })
                    .ToListAsync();

            //проверяем есть ли руководитель в списке факультета, если нет выводим список всех преподавателей
            var cheakTeacher = lstTeacher.FirstOrDefault(a => a.Id == diplomWork.TeacherId);
            if (cheakTeacher != null || diplomWork.TeacherId == null)
            {
                ViewBag.TeacherId = new SelectList(lstTeacher, "Id", "Name", diplomWork.TeacherId);
            }
            else
            {
                lstTeacher = await db.TeacherDepartments
                    .Include(a => a.Teacher)
                    .Where(a => a.Teacher.DateBlocked == null)
                    .OrderBy(a => a.Teacher.Lastname)
                    .ThenBy(a => a.Teacher.Firstname)
                    .ThenBy(a => a.Teacher.Middlename)
                    .Select(a => new TutorsList { Id = a.TeacherId, Name = a.Teacher.Lastname + " " + a.Teacher.Firstname.Substring(0, 1) + "." + a.Teacher.Middlename.Substring(0, 1) + "." })
                    .ToListAsync();
                ViewBag.TeacherId = new SelectList(lstTeacher, "Id", "Name", diplomWork.TeacherId);
            }
            return View(diplomWork);
        }

        // POST: DiplomWorks/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Students,Administrators,SecretariesGIA,FacultiesManagers")]
        public async Task<ActionResult> Edit(DiplomWork diplomWork, IList<HttpPostedFileBase> fileUpload)
        {
            if (ModelState.IsValid)
            {
                var sdg = Request.Files;
                var gia = db.GIAs.Find(diplomWork.GIAId);

                if (fileUpload != null)
                {
                    for (var i = 0; i < fileUpload.Count; i++)
                    {
                        if (fileUpload[i] == null) continue;

                        if (fileUpload[i] != null && fileUpload[i].ContentLength > 52428800 ||
                            (fileUpload[i] != null && Path.GetExtension(fileUpload[i].FileName.ToLower()) != ".pdf" && Path.GetExtension(fileUpload[i].FileName.ToLower()) != ".doc"
                            && Path.GetExtension(fileUpload[i].FileName.ToLower()) != ".zip" && Path.GetExtension(fileUpload[i].FileName.ToLower()) != ".docx"
                            && Path.GetExtension(fileUpload[i].FileName.ToLower()) != ".jpg" && Path.GetExtension(fileUpload[i].FileName.ToLower()) != ".png"))
                        {
                            continue;
                        }

                        string extension = Path.GetExtension(fileUpload[i].FileName);
                        string fileName = Guid.NewGuid().ToString() + extension;
                        string fullPath;
                        switch (i)
                        {
                            case 0:
                                diplomWork.FileNameConducting = fileUpload[i].FileName;
                                diplomWork.ConductingIsBlocked = true;
                                fullPath = AppDomain.CurrentDomain.BaseDirectory + "DiplomWorks/" + fileName;
                                diplomWork.PathConducting = fullPath;
                                if (fileName != null) fileUpload[i].SaveAs(fullPath);
                                break;

                            case 1:
                                diplomWork.FileNameConfirmation = fileUpload[i].FileName;
                                diplomWork.ConfirmationIsBlocked = true;
                                fullPath = AppDomain.CurrentDomain.BaseDirectory + "DiplomWorks/" + fileName;
                                diplomWork.PathConfirmation = fullPath;
                                if (fileName != null) fileUpload[i].SaveAs(fullPath);
                                break;

                            case 2:
                                diplomWork.FileNameDeclaration = fileUpload[i].FileName;
                                diplomWork.DeclarationIsBlocked = true;
                                fullPath = AppDomain.CurrentDomain.BaseDirectory + "DiplomWorks/" + fileName;
                                diplomWork.PathDeclaration = fullPath;
                                if (fileName != null) fileUpload[i].SaveAs(fullPath);
                                break;

                            case 3:
                                diplomWork.FileNameСonsent = fileUpload[i].FileName;
                                diplomWork.СonsentIsBlocked = true;
                                fullPath = AppDomain.CurrentDomain.BaseDirectory + "DiplomWorks/" + fileName;
                                diplomWork.PathСonsent = fullPath;
                                if (fileName != null) fileUpload[i].SaveAs(fullPath);
                                break;

                            case 4:
                                if (Path.GetExtension(fileUpload[i].FileName.ToLower()) != ".doc" && Path.GetExtension(fileUpload[i].FileName.ToLower()) != ".docx")
                                {
                                    continue;
                                }
                                else
                                {
                                    diplomWork.FileNameDiplom = fileUpload[i].FileName;
                                    diplomWork.DiplomIsBlocked = true;
                                    fullPath = AppDomain.CurrentDomain.BaseDirectory + "DiplomWorks/" + fileName;
                                    diplomWork.PathDiplom = fullPath;
                                    if (fileName != null) fileUpload[i].SaveAs(fullPath);
                                }
                                break;

                            case 5:
                                if (Path.GetExtension(fileUpload[i].FileName.ToLower()) != ".pdf")
                                {
                                    continue;
                                }
                                else
                                {
                                    diplomWork.FileNameDiplomPDF = fileUpload[i].FileName;
                                    diplomWork.DiplomIsBlockedPDF = true;
                                    fullPath = AppDomain.CurrentDomain.BaseDirectory + "DiplomWorks/" + fileName;
                                    diplomWork.PathDiplomPDF = fullPath;
                                    if (fileName != null) fileUpload[i].SaveAs(fullPath);
                                }
                                break;

                            case 6:
                                diplomWork.FileNameFeedback = fileUpload[i].FileName;
                                diplomWork.FeedbackIsBlocked = true;
                                fullPath = AppDomain.CurrentDomain.BaseDirectory + "DiplomWorks/" + fileName;
                                diplomWork.PathFeedback = fullPath;
                                if (fileName != null) fileUpload[i].SaveAs(fullPath);
                                break;

                            case 7:
                                diplomWork.FileNamePlagiarism = fileUpload[i].FileName;
                                diplomWork.PlagiarismIsBlocked = true;
                                fullPath = AppDomain.CurrentDomain.BaseDirectory + "DiplomWorks/" + fileName;
                                diplomWork.PathPlagiarism = fullPath;
                                if (fileName != null) fileUpload[i].SaveAs(fullPath);
                                break;

                            case 8:
                                diplomWork.FileNameReview = fileUpload[i].FileName;
                                diplomWork.ReviewIsBlocked = true;
                                fullPath = AppDomain.CurrentDomain.BaseDirectory + "DiplomWorks/" + fileName;
                                diplomWork.PathReview = fullPath;
                                if (fileName != null) fileUpload[i].SaveAs(fullPath);
                                break;

                            case 9:
                                diplomWork.FileNameApplication = fileUpload[i].FileName;
                                diplomWork.ApplicationIsBlocked = true;
                                fullPath = AppDomain.CurrentDomain.BaseDirectory + "DiplomWorks/" + fileName;
                                diplomWork.PathApplication = fullPath;
                                if (fileName != null) fileUpload[i].SaveAs(fullPath);
                                break;

                            case 10:
                                diplomWork.FileNameOther = fileUpload[i].FileName;
                                diplomWork.OtherIsBlocked = true;
                                fullPath = AppDomain.CurrentDomain.BaseDirectory + "DiplomWorks/" + fileName;
                                diplomWork.PathOther = fullPath;
                                if (fileName != null) fileUpload[i].SaveAs(fullPath);
                                break;
                        }
                    }
                }

                db.Entry(diplomWork).State = EntityState.Modified;
                await db.SaveChangesAsync();
                if (User.IsInRole("Students"))
                {
                    return RedirectToAction("Edit", "DiplomWorks", new { id = diplomWork.Id });
                }
                else
                {
                    return RedirectToAction("Details", "GIAs", new { id = gia.Id });
                }
            }

            return View(diplomWork);
        }


        // GET: /Users/Delete/5
        [Authorize(Roles = "Administrators,SecretariesGIA,FacultiesManagers")]
        public async Task<ActionResult> Delete(int? id, string field)
        {
            if (id == null || field == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DiplomWork diplomWork = await db.DiplomWorks
                .Include(a => a.GIA)
                .Include(a => a.StudentDiplom)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (diplomWork == null)
            {
                return HttpNotFound();
            }

            return View(diplomWork);
        }

        //
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators,SecretariesGIA,FacultiesManagers")]
        public async Task<ActionResult> DeleteConfirmed(int? id, string field)
        {
            if (id == null || field == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DiplomWork diplomWork = await db.DiplomWorks
                .Include(a => a.GIA)
                .Include(a => a.StudentDiplom)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (diplomWork == null)
            {
                return HttpNotFound();
            }

            switch (field)
            {
                case "PathСonsent":
                    //удаление файла
                    FileInfo fileConsent = new FileInfo(diplomWork.PathСonsent);
                    if (fileConsent.Exists)
                    {
                        fileConsent.Delete();
                    }

                    //удаление записей о файле
                    diplomWork.PathСonsent = null;
                    diplomWork.FileNameСonsent = null;
                    diplomWork.СonsentIsBlocked = false;
                    break;

                case "PathDeclaration":
                    //удаление файла
                    FileInfo fileDeclaration = new FileInfo(diplomWork.PathDeclaration);
                    if (fileDeclaration.Exists)
                    {
                        fileDeclaration.Delete();
                    }

                    //удаление записей о файле
                    diplomWork.PathDeclaration = null;
                    diplomWork.FileNameDeclaration = null;
                    diplomWork.DeclarationIsBlocked = false;
                    break;

                case "PathDiplom":
                    //удаление файла
                    FileInfo fileDiplom = new FileInfo(diplomWork.PathDiplom);
                    if (fileDiplom.Exists)
                    {
                        fileDiplom.Delete();
                    }

                    //удаление записей о файле
                    diplomWork.PathDiplom = null;
                    diplomWork.FileNameDiplom = null;
                    diplomWork.DiplomIsBlocked = false;
                    break;

                case "PathDiplomPDF":
                    //удаление файла
                    FileInfo fileDiplomPDF = new FileInfo(diplomWork.PathDiplomPDF);
                    if (fileDiplomPDF.Exists)
                    {
                        fileDiplomPDF.Delete();
                    }

                    //удаление записей о файле
                    diplomWork.PathDiplomPDF = null;
                    diplomWork.FileNameDiplomPDF = null;
                    diplomWork.DiplomIsBlockedPDF = false;
                    break;

                case "PathFeedback":
                    //удаление файла
                    FileInfo fileFeedback = new FileInfo(diplomWork.PathFeedback);
                    if (fileFeedback.Exists)
                    {
                        fileFeedback.Delete();
                    }

                    //удаление записей о файле
                    diplomWork.PathFeedback = null;
                    diplomWork.FileNameFeedback = null;
                    diplomWork.FeedbackIsBlocked = false;
                    break;

                case "PathReview":
                    //удаление файла
                    FileInfo fileReview = new FileInfo(diplomWork.PathReview);
                    if (fileReview.Exists)
                    {
                        fileReview.Delete();
                    }

                    //удаление записей о файле
                    diplomWork.PathReview = null;
                    diplomWork.FileNameReview = null;
                    diplomWork.ReviewIsBlocked = false;
                    break;

                case "PathPlagiarism":
                    //удаление файла
                    FileInfo filePlagiarism = new FileInfo(diplomWork.PathPlagiarism);
                    if (filePlagiarism.Exists)
                    {
                        filePlagiarism.Delete();
                    }

                    //удаление записей о файле
                    diplomWork.PathPlagiarism = null;
                    diplomWork.FileNamePlagiarism = null;
                    diplomWork.PlagiarismIsBlocked = false;
                    break;

                case "PathApplication":
                    //удаление файла
                    FileInfo fileApplication = new FileInfo(diplomWork.PathApplication);
                    if (fileApplication.Exists)
                    {
                        fileApplication.Delete();
                    }

                    //удаление записей о файле
                    diplomWork.PathApplication = null;
                    diplomWork.FileNameApplication = null;
                    diplomWork.ApplicationIsBlocked = false;
                    break;

                case "PathConfirmation":
                    //удаление файла
                    FileInfo fileConfirmation = new FileInfo(diplomWork.PathConfirmation);
                    if (fileConfirmation.Exists)
                    {
                        fileConfirmation.Delete();
                    }

                    //удаление записей о файле
                    diplomWork.PathConfirmation = null;
                    diplomWork.FileNameConfirmation = null;
                    diplomWork.ConfirmationIsBlocked = false;
                    break;

                case "PathConducting":
                    //удаление файла
                    FileInfo fileConducting = new FileInfo(diplomWork.PathConducting);
                    if (fileConducting.Exists)
                    {
                        fileConducting.Delete();
                    }

                    //удаление записей о файле
                    diplomWork.PathConducting = null;
                    diplomWork.FileNameConducting = null;
                    diplomWork.ConductingIsBlocked = false;
                    break;

                case "PathOther":
                    //удаление файла
                    FileInfo fileOther = new FileInfo(diplomWork.PathOther);
                    if (fileOther.Exists)
                    {
                        fileOther.Delete();
                    }

                    //удаление записей о файле
                    diplomWork.PathOther = null;
                    diplomWork.FileNameOther = null;
                    diplomWork.OtherIsBlocked = false;
                    break;
            }
            db.Entry(diplomWork).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return RedirectToAction("Details", "GIAs", new { id = diplomWork.GIA.Id });
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
