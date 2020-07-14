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
using ClosedXML.Excel;

namespace Accounts.Controllers
{
    public class EducationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public FileResult GetFile(string path, string name)
        {
            byte[] mas = System.IO.File.ReadAllBytes(path);
            string file_path = Server.MapPath("~/Documents/" + name);
            string file_type = "application/octet-stream";
            return File(mas, file_type, name);
        }

        [HttpGet]
        public FileResult Export()
        {
            //экспорт базового образования
            DataTable dtBasicEdu = new DataTable("Базовое образование");
            dtBasicEdu.Columns.AddRange(new DataColumn[7] { new DataColumn("Факультет"),
                                            new DataColumn("Кафедра"),
                                            new DataColumn("Преподаватель"),
                                            new DataColumn("Образовательное учреждение"),
                                            new DataColumn("Год получения"),
                                            new DataColumn("Специальность"),
                                            new DataColumn("Квалификация") });

            IQueryable<BasicEducationExport> lstBasicEdu = null;
            if (User.IsInRole("Administrators") || User.IsInRole("PersonnelDepartment"))
                lstBasicEdu = from basicEducations in db.BasicEducations
                              join users in db.Users on basicEducations.BasicEducationUserId equals users.Id
                              join departments in db.Departments on users.DepartmentId equals departments.Id
                              join faculties in db.Faculties on users.FacultyId equals faculties.Id
                              orderby users.Lastname, users.Middlename, users.Middlename
                              select new BasicEducationExport
                              {
                                  FacultyName = faculties.Name,
                                  DepartmentName = departments.Name,
                                  Teacher = users.Lastname + " " + users.Firstname + " " + users.Middlename,
                                  EducationalInstitution = basicEducations.EducationalInstitution,
                                  Year = basicEducations.Year,
                                  Specialty = basicEducations.Specialty,
                                  Qualification = basicEducations.Qualification
                              };
            else
            {
                string userId = User.Identity.GetUserId();
                var user = db.Users.Find(userId);
                lstBasicEdu = from basicEducations in db.BasicEducations
                              join users in db.Users on basicEducations.BasicEducationUserId equals users.Id
                              join departments in db.Departments on users.DepartmentId equals departments.Id
                              join faculties in db.Faculties on users.FacultyId equals faculties.Id
                              where users.DepartmentId == user.DepartmentId
                              orderby users.Lastname, users.Middlename, users.Middlename
                              select new BasicEducationExport
                              {
                                  FacultyName = faculties.Name,
                                  DepartmentName = departments.Name,
                                  Teacher = users.Lastname + " " + users.Firstname + " " + users.Middlename,
                                  EducationalInstitution = basicEducations.EducationalInstitution,
                                  Year = basicEducations.Year,
                                  Specialty = basicEducations.Specialty,
                                  Qualification = basicEducations.Qualification
                              };
            }

            foreach (var basic in lstBasicEdu)
            {
                dtBasicEdu.Rows.Add(basic.FacultyName, basic.DepartmentName, basic.Teacher, basic.EducationalInstitution,
                    basic.Year, basic.Specialty, basic.Qualification);
            }


            //экспорт степени/звания
            DataTable dtAcademicDegree = new DataTable("Степень-Звание");
            dtAcademicDegree.Columns.AddRange(new DataColumn[6] { new DataColumn("Факультет"),
                                            new DataColumn("Кафедра"),
                                            new DataColumn("Преподаватель"),
                                            new DataColumn("Год получения"),
                                            new DataColumn("Наименование степени/звания"),
                                            new DataColumn("Реквизиты документа")});

            IQueryable<AcademicDegreeExport> lstAcademicDegree = null;
            if (User.IsInRole("Administrators") || User.IsInRole("PersonnelDepartment"))
                lstAcademicDegree = from academicDegrees in db.AcademicDegrees
                                    join users in db.Users on academicDegrees.AcademicDegreeUserId equals users.Id
                                    join departments in db.Departments on users.DepartmentId equals departments.Id
                                    join faculties in db.Faculties on users.FacultyId equals faculties.Id
                                    orderby users.Lastname, users.Middlename, users.Middlename
                                    select new AcademicDegreeExport
                                    {
                                        FacultyName = faculties.Name,
                                        DepartmentName = departments.Name,
                                        Teacher = users.Lastname + " " + users.Firstname + " " + users.Middlename,
                                        Year = academicDegrees.Year,
                                        Name = academicDegrees.Name,
                                        Requisites = academicDegrees.Requisites
                                    };
            else
            {
                string userId = User.Identity.GetUserId();
                var user = db.Users.Find(userId);
                lstAcademicDegree = from academicDegrees in db.AcademicDegrees
                                    join users in db.Users on academicDegrees.AcademicDegreeUserId equals users.Id
                                    join departments in db.Departments on users.DepartmentId equals departments.Id
                                    join faculties in db.Faculties on users.FacultyId equals faculties.Id
                                    where users.DepartmentId == user.DepartmentId
                                    orderby users.Lastname, users.Middlename, users.Middlename
                                    select new AcademicDegreeExport
                                    {
                                        FacultyName = faculties.Name,
                                        DepartmentName = departments.Name,
                                        Teacher = users.Lastname + " " + users.Firstname + " " + users.Middlename,
                                        Year = academicDegrees.Year,
                                        Name = academicDegrees.Name,
                                        Requisites = academicDegrees.Requisites
                                    };
            }

            foreach (var academicDegree in lstAcademicDegree)
            {
                dtAcademicDegree.Rows.Add(academicDegree.FacultyName, academicDegree.DepartmentName, academicDegree.Teacher,
                    academicDegree.Year, academicDegree.Name, academicDegree.Requisites);
            }


            //экспорт степени/звания
            DataTable dtAdditionalEducation = new DataTable("Дополнительное образование");
            dtAdditionalEducation.Columns.AddRange(new DataColumn[8] {
                                            new DataColumn("Факультет"),
                                            new DataColumn("Кафедра"),
                                            new DataColumn("Преподаватель"),
                                            new DataColumn("Год получения"),
                                            new DataColumn("Вид образования"),
                                            new DataColumn("Наименование"),
                                            new DataColumn("Количество часов"),
                                            new DataColumn("Место прохождения")});

            IQueryable<AdditionalEducationExport> lstAdditionalEducation = null;
            if (User.IsInRole("Administrators") || User.IsInRole("PersonnelDepartment"))
                lstAdditionalEducation = from additionalEducations in db.AdditionalEducations
                                         join users in db.Users on additionalEducations.AdditionalEducationUserId equals users.Id
                                         join departments in db.Departments on users.DepartmentId equals departments.Id
                                         join faculties in db.Faculties on users.FacultyId equals faculties.Id
                                         orderby users.Lastname, users.Middlename, users.Middlename
                                         select new AdditionalEducationExport
                                         {
                                             FacultyName = faculties.Name,
                                             DepartmentName = departments.Name,
                                             Teacher = users.Lastname + " " + users.Firstname + " " + users.Middlename,
                                             Year = additionalEducations.Year,
                                             Type = additionalEducations.Type,
                                             Name = additionalEducations.Name,
                                             Hours = additionalEducations.Hours,
                                             Location = additionalEducations.Location
                                         };
            else
            {
                string userId = User.Identity.GetUserId();
                var user = db.Users.Find(userId);
                lstAdditionalEducation = from additionalEducations in db.AdditionalEducations
                                         join users in db.Users on additionalEducations.AdditionalEducationUserId equals users.Id
                                         join departments in db.Departments on users.DepartmentId equals departments.Id
                                         join faculties in db.Faculties on users.FacultyId equals faculties.Id
                                         where users.DepartmentId == user.DepartmentId
                                         orderby users.Lastname, users.Middlename, users.Middlename
                                         select new AdditionalEducationExport
                                         {
                                             FacultyName = faculties.Name,
                                             DepartmentName = departments.Name,
                                             Teacher = users.Lastname + " " + users.Firstname + " " + users.Middlename,
                                             Year = additionalEducations.Year,
                                             Type = additionalEducations.Type,
                                             Name = additionalEducations.Name,
                                             Hours = additionalEducations.Hours,
                                             Location = additionalEducations.Location
                                         };
            }

            foreach (var additionalEdu in lstAdditionalEducation)
            {
                dtAdditionalEducation.Rows.Add(additionalEdu.FacultyName, additionalEdu.DepartmentName, additionalEdu.Teacher,
                    additionalEdu.Year, additionalEdu.Type, additionalEdu.Name, additionalEdu.Hours, additionalEdu.Location);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dtBasicEdu);
                wb.Worksheets.Add(dtAcademicDegree);
                wb.Worksheets.Add(dtAdditionalEducation);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Educations.xlsx");
                }
            }
        }

        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers,PersonnelDepartment")]
        public ActionResult Index(string teacherId)
        {
            if (teacherId != null)
            {
                ViewBag.UserId = teacherId;
                ViewBag.Teacher = db.Users.FirstOrDefault(a => a.Id == teacherId);
            }
            return View();
        }

        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers,PersonnelDepartment")]
        public ActionResult IndexAll()
        {
            return View();
        }

        // GET: BasicEducations
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers,PersonnelDepartment")]
        public ActionResult IndexBasicEdu(string teacherId)
        {
            IQueryable<BasicEducation> basicEducations = null;
            if (User.IsInRole("Administrators") || User.IsInRole("PersonnelDepartment"))
            {
                if (teacherId != null)
                {
                    var teacher = db.Users.FirstOrDefault(a => a.Id == teacherId);
                    ViewBag.Teacher = teacher;
                    basicEducations = db.BasicEducations.Where(b => b.BasicEducationUserId == teacherId);
                }
                else
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (User.IsInRole("DepartmentsManagers"))
            {
                //получаем зав кафедрой
                var userId = User.Identity.GetUserId();
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем преподавателя
                var teacher = (teacherId != null) ? db.Users.FirstOrDefault(a => a.Id == teacherId) : db.Users.FirstOrDefault(a => a.Id == userId);
                ViewBag.Teacher = teacher;
                //если кафедры совпали, то выдаем данные преподавателя
                if (departmentManager.DepartmentId == teacher.DepartmentId)
                    basicEducations = db.BasicEducations.Include(a => a.User).Where(b => b.BasicEducationUserId == teacher.Id);
            }
            else if (User.IsInRole("Teachers"))
            {
                var userId = User.Identity.GetUserId();
                basicEducations = db.BasicEducations.Where(b => b.BasicEducationUserId == userId);
            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(basicEducations.OrderBy(a => a.Year).ThenBy(a => a.Qualification).ToList());
        }

        // GET: AcademicDegree
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers,PersonnelDepartment")]
        public ActionResult IndexAcademicDegree(string teacherId)
        {
            IQueryable<AcademicDegree> academicDegree = null;
            if (User.IsInRole("Administrators") || User.IsInRole("PersonnelDepartment"))
            {
                if (teacherId != null)
                {
                    var teacher = db.Users.FirstOrDefault(a => a.Id == teacherId);
                    ViewBag.Teacher = teacher;
                    academicDegree = db.AcademicDegrees.Where(b => b.AcademicDegreeUserId == teacherId);
                }
                else
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (User.IsInRole("DepartmentsManagers"))
            {
                //получаем зав кафедрой
                var userId = User.Identity.GetUserId();
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем преподавателя
                var teacher = (teacherId != null) ? db.Users.FirstOrDefault(a => a.Id == teacherId) : db.Users.FirstOrDefault(a => a.Id == userId);
                ViewBag.Teacher = teacher;
                //если кафедры совпали, то выдаем данные преподавателя
                if (departmentManager.DepartmentId == teacher.DepartmentId)
                    academicDegree = db.AcademicDegrees.Include(a => a.User).Where(b => b.AcademicDegreeUserId == teacher.Id); ;
            }
            else if (User.IsInRole("Teachers"))
            {
                var userId = User.Identity.GetUserId();
                academicDegree = db.AcademicDegrees.Where(b => b.AcademicDegreeUserId == userId);
            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(academicDegree.OrderBy(a => a.Year).ThenBy(a => a.Name).ToList());
        }

        // GET: AdditionalEducation
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers,PersonnelDepartment")]
        public ActionResult IndexAdditionalEducation(string teacherId)
        {
            IQueryable<AdditionalEducation> additionalEducation = null;
            if (User.IsInRole("Administrators") || User.IsInRole("PersonnelDepartment"))
            {
                if (teacherId != null)
                {
                    var teacher = db.Users.FirstOrDefault(a => a.Id == teacherId);
                    ViewBag.Teacher = teacher;
                    additionalEducation = db.AdditionalEducations.Where(b => b.AdditionalEducationUserId == teacherId);
                }
                else
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (User.IsInRole("DepartmentsManagers"))
            {
                //получаем зав кафедрой
                var userId = User.Identity.GetUserId();
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем преподавателя
                var teacher = (teacherId != null) ? db.Users.FirstOrDefault(a => a.Id == teacherId) : db.Users.FirstOrDefault(a => a.Id == userId);
                ViewBag.Teacher = teacher;
                //если кафедры совпали, то выдаем данные преподавателя
                if (departmentManager.DepartmentId == teacher.DepartmentId)
                    additionalEducation = db.AdditionalEducations.Include(a => a.User).Where(b => b.AdditionalEducationUserId == teacher.Id); ;
            }
            else if (User.IsInRole("Teachers"))
            {
                var userId = User.Identity.GetUserId();
                additionalEducation = db.AdditionalEducations.Where(b => b.AdditionalEducationUserId == userId);
            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(additionalEducation.OrderBy(a => a.Year).ThenBy(a => a.Name).ToList());
        }

        [Authorize(Roles = "Administrators,DepartmentsManagers,PersonnelDepartment")]
        public ActionResult IndexAllBasicEdu(int? departmentId)
        {
            IQueryable<BasicEducationViewModel> basicEducations = null;
            if (User.IsInRole("Administrators") || User.IsInRole("PersonnelDepartment"))
            {
                if (departmentId != null)
                {
                    //получаем список преподавателей кафедры
                    List<string> teachersDepartment = new List<string>();
                    teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentId).Select(a => a.Id).ToList();
                    //получаем список с базовым образованием
                    basicEducations = from be in db.BasicEducations
                                      join users in db.Users on be.BasicEducationUserId equals users.Id
                                      join dep in db.Departments on users.DepartmentId equals dep.Id
                                      where teachersDepartment.Contains(be.BasicEducationUserId)
                                      orderby users.Lastname, users.Firstname, users.Middlename
                                      select new BasicEducationViewModel
                                      {
                                          Id = be.Id,
                                          UserId = users.Id,
                                          Department = dep.Name,
                                          Teacher = users.Lastname + " " + users.Firstname.Substring(0, 1) + "." + users.Middlename.Substring(0, 1) + ".",
                                          EducationalInstitution = be.EducationalInstitution,
                                          Year = be.Year,
                                          Specialty = be.Specialty,
                                          Qualification = be.Qualification,
                                          Path = be.Path,
                                          FileName = be.FileName
                                      };
                }
                else
                {
                    basicEducations = from be in db.BasicEducations
                                      join users in db.Users on be.BasicEducationUserId equals users.Id
                                      join dep in db.Departments on users.DepartmentId equals dep.Id
                                      orderby users.Lastname, users.Firstname, users.Middlename
                                      select new BasicEducationViewModel
                                      {
                                          Id = be.Id,
                                          UserId = users.Id,
                                          Department = dep.Name,
                                          Teacher = users.Lastname + " " + users.Firstname.Substring(0, 1) + "." + users.Middlename.Substring(0, 1) + ".",
                                          EducationalInstitution = be.EducationalInstitution,
                                          Year = be.Year,
                                          Specialty = be.Specialty,
                                          Qualification = be.Qualification,
                                          Path = be.Path,
                                          FileName = be.FileName
                                      };
                }
            }
            else if (User.IsInRole("DepartmentsManagers"))
            {
                //получаем данные зав кафедрой
                var userId = User.Identity.GetUserId();
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем список преподавателей кафедры
                List<string> teachersDepartment = new List<string>();
                teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentManager.DepartmentId).Select(a => a.Id).ToList();
                //получаем список документов
                basicEducations = from be in db.BasicEducations
                                  join users in db.Users on be.BasicEducationUserId equals users.Id
                                  join dep in db.Departments on users.DepartmentId equals dep.Id
                                  where teachersDepartment.Contains(be.BasicEducationUserId)
                                  orderby users.Lastname, users.Firstname, users.Middlename
                                  select new BasicEducationViewModel
                                  {
                                      Id = be.Id,
                                      UserId = users.Id,
                                      Department = dep.Name,
                                      Teacher = users.Lastname + " " + users.Firstname.Substring(0, 1) + "." + users.Middlename.Substring(0, 1) + ".",
                                      EducationalInstitution = be.EducationalInstitution,
                                      Year = be.Year,
                                      Specialty = be.Specialty,
                                      Qualification = be.Qualification,
                                      Path = be.Path,
                                      FileName = be.FileName
                                  };
            }
            else
            {
                return HttpNotFound();
            }

            return PartialView(basicEducations.OrderBy(be => be.Year).ThenBy(be => be.Qualification).ToList());
        }

        [Authorize(Roles = "Administrators,DepartmentsManagers,PersonnelDepartment")]
        public ActionResult IndexAllAdditionalEducation(int? departmentId)
        {
            IQueryable<AdditionalEducationViewModel> additionalEducation = null;
            if (User.IsInRole("Administrators") || User.IsInRole("PersonnelDepartment"))
            {
                if (departmentId != null)
                {
                    //получаем список преподавателей кафедры
                    List<string> teachersDepartment = new List<string>();
                    teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentId).Select(a => a.Id).ToList();
                    //получаем список с базовым образованием
                    additionalEducation = from ae in db.AdditionalEducations
                                          join users in db.Users on ae.AdditionalEducationUserId equals users.Id
                                          join dep in db.Departments on users.DepartmentId equals dep.Id
                                          where teachersDepartment.Contains(ae.AdditionalEducationUserId)
                                          orderby users.Lastname, users.Firstname
                                          select new AdditionalEducationViewModel
                                          {
                                              Id = ae.Id,
                                              UserId = users.Id,
                                              Department = dep.Name,
                                              Teacher = users.Lastname + " " + users.Firstname.Substring(0, 1) + "." + users.Middlename.Substring(0, 1) + ".",
                                              Type = ae.Type,
                                              Year = ae.Year,
                                              Name = ae.Name,
                                              Hours = ae.Hours,
                                              Location = ae.Location,
                                              Path = ae.Path,
                                              FileName = ae.FileName
                                          };
                }
                else
                {
                    additionalEducation = from ae in db.AdditionalEducations
                                          join users in db.Users on ae.AdditionalEducationUserId equals users.Id
                                          join dep in db.Departments on users.DepartmentId equals dep.Id
                                          orderby users.Lastname, users.Firstname
                                          select new AdditionalEducationViewModel
                                          {
                                              Id = ae.Id,
                                              UserId = users.Id,
                                              Department = dep.Name,
                                              Teacher = users.Lastname + " " + users.Firstname.Substring(0, 1) + "." + users.Middlename.Substring(0, 1) + ".",
                                              Type = ae.Type,
                                              Year = ae.Year,
                                              Name = ae.Name,
                                              Hours = ae.Hours,
                                              Location = ae.Location,
                                              Path = ae.Path,
                                              FileName = ae.FileName
                                          };
                }
            }
            else if (User.IsInRole("DepartmentsManagers"))
            {
                //получаем данные зав кафедрой
                var userId = User.Identity.GetUserId();
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем список преподавателей кафедры
                List<string> teachersDepartment = new List<string>();
                teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentManager.DepartmentId).Select(a => a.Id).ToList();
                //получаем список документов
                additionalEducation = from ae in db.AdditionalEducations
                                      join users in db.Users on ae.AdditionalEducationUserId equals users.Id
                                      join dep in db.Departments on users.DepartmentId equals dep.Id
                                      where teachersDepartment.Contains(ae.AdditionalEducationUserId)
                                      orderby users.Lastname, users.Firstname
                                      select new AdditionalEducationViewModel
                                      {
                                          Id = ae.Id,
                                          UserId = users.Id,
                                          Department = dep.Name,
                                          Teacher = users.Lastname + " " + users.Firstname.Substring(0, 1) + "." + users.Middlename.Substring(0, 1) + ".",
                                          Type = ae.Type,
                                          Year = ae.Year,
                                          Name = ae.Name,
                                          Hours = ae.Hours,
                                          Location = ae.Location,
                                          Path = ae.Path,
                                          FileName = ae.FileName
                                      };
            }
            else
            {
                return HttpNotFound();
            }

            return PartialView(additionalEducation.OrderBy(ae => ae.Year).ThenBy(ae => ae.Name).ToList());
        }

        //get: academicDegree
        [Authorize(Roles = "Administrators,DepartmentsManagers,PersonnelDepartment")]
        public ActionResult IndexAllAcademicDegree(int? departmentId)
        {
            IQueryable<AcademicDegreeViewModel> academicDegree = null;
            if (User.IsInRole("Administrators") || User.IsInRole("PersonnelDepartment"))
            {
                if (departmentId != null)
                {
                    //получаем список преподавателей кафедры
                    List<string> teachersDepartment = new List<string>();
                    teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentId).Select(a => a.Id).ToList();
                    //получаем список с базовым образованием
                    academicDegree = from ad in db.AcademicDegrees
                                     join users in db.Users on ad.AcademicDegreeUserId equals users.Id
                                     join dep in db.Departments on users.DepartmentId equals dep.Id
                                     where teachersDepartment.Contains(ad.AcademicDegreeUserId)
                                     orderby users.Lastname, users.Firstname
                                     select new AcademicDegreeViewModel
                                     {
                                         Id = ad.Id,
                                         UserId = users.Id,
                                         Department = dep.Name,
                                         Teacher = users.Lastname + " " + users.Firstname.Substring(0, 1) + "." + users.Middlename.Substring(0, 1) + ".",
                                         Name = ad.Name,
                                         Year = ad.Year,
                                         Requisites = ad.Requisites,
                                         Path = ad.Path,
                                         FileName = ad.FileName
                                     };
                }
                else
                {
                    academicDegree = from ad in db.AcademicDegrees
                                     join users in db.Users on ad.AcademicDegreeUserId equals users.Id
                                     join dep in db.Departments on users.DepartmentId equals dep.Id
                                     orderby users.Lastname, users.Firstname
                                     select new AcademicDegreeViewModel
                                     {
                                         Id = ad.Id,
                                         UserId = users.Id,
                                         Department = dep.Name,
                                         Teacher = users.Lastname + " " + users.Firstname.Substring(0, 1) + "." + users.Middlename.Substring(0, 1) + ".",
                                         Name = ad.Name,
                                         Year = ad.Year,
                                         Requisites = ad.Requisites,
                                         Path = ad.Path,
                                         FileName = ad.FileName
                                     };
                }
            }
            else if (User.IsInRole("DepartmentsManagers"))
            {
                //получаем данные зав кафедрой
                var userId = User.Identity.GetUserId();
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем список преподавателей кафедры
                List<string> teachersDepartment = new List<string>();
                teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentManager.DepartmentId).Select(a => a.Id).ToList();
                //получаем список документов
                academicDegree = from ad in db.AcademicDegrees
                                 join users in db.Users on ad.AcademicDegreeUserId equals users.Id
                                 join dep in db.Departments on users.DepartmentId equals dep.Id
                                 where teachersDepartment.Contains(ad.AcademicDegreeUserId)
                                 orderby users.Lastname, users.Firstname
                                 select new AcademicDegreeViewModel
                                 {
                                     Id = ad.Id,
                                     UserId = users.Id,
                                     Department = dep.Name,
                                     Teacher = users.Lastname + " " + users.Firstname.Substring(0, 1) + "." + users.Middlename.Substring(0, 1) + ".",
                                     Name = ad.Name,
                                     Year = ad.Year,
                                     Requisites = ad.Requisites,
                                     Path = ad.Path,
                                     FileName = ad.FileName
                                 };
            }
            else
            {
                return HttpNotFound();
            }

            return PartialView(academicDegree.OrderBy(ad => ad.Year).ThenBy(ad => ad.Name).ToList());
        }

        // GET: BasicEducations/Create
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers")]
        public ActionResult CreateBasicEdu()
        {
            if (User.IsInRole("Teachers") && !User.IsInRole("DepartmentsManagers"))
                ViewBag.BasicEducationUserId = User.Identity.GetUserId();
            else if (User.IsInRole("Teachers") && User.IsInRole("DepartmentsManagers"))
            {
                var userId = User.Identity.GetUserId();
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем список преподавателей кафедры
                List<string> teachersDepartment = new List<string>();
                teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentManager.DepartmentId).Select(a => a.Id).ToList();
                ViewBag.BasicEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null && teachersDepartment.Contains(a.Id))
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name");
            }
            else if (User.IsInRole("Administrators"))
            {
                ViewBag.BasicEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null)
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name");
            }
            //ViewBag.BasicEducationUserId = teacherId;
            return View();
        }

        // POST: BasicEducations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers")]
        public async Task<ActionResult> CreateBasicEdu([Bind(Include = "Id,BasicEducationUserId,EducationalInstitution,Year,Specialty,Qualification,Path,FileName")] BasicEducation basicEducation, HttpPostedFileBase NameFile)
        {
            if (ModelState.IsValid)
            {
                //сохраняем автора события
                if (basicEducation.BasicEducationUserId == null)
                    basicEducation.BasicEducationUserId = User.Identity.GetUserId();

                //Сохраняем изображение                
                try
                {
                    if (NameFile != null && NameFile.ContentLength > 3145728 ||
                        (NameFile != null && Path.GetExtension(NameFile.FileName.ToLower()) != ".jpg" && Path.GetExtension(NameFile.FileName.ToLower()) != ".jpeg" &&
                        Path.GetExtension(NameFile.FileName.ToLower()) != ".gif" && Path.GetExtension(NameFile.FileName.ToLower()) != ".pdf" &&
                         Path.GetExtension(NameFile.FileName.ToLower()) != ".zip" && Path.GetExtension(NameFile.FileName.ToLower()) != ".png"))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    if (NameFile != null && NameFile.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(NameFile.FileName);

                        basicEducation.FileName = NameFile.FileName;
                        string fileName = Guid.NewGuid().ToString() + extension;
                        basicEducation.Path = Server.MapPath("~/Documents/" + fileName);
                        NameFile.SaveAs(basicEducation.Path);
                        ViewBag.Message = "Загрузка файла выполнена";
                    }
                }
                catch
                {
                    ViewBag.Message = "Ошибка загрузки файла";
                }

                db.BasicEducations.Add(basicEducation);
                await db.SaveChangesAsync();
                if (!User.IsInRole("Teachers") || basicEducation.BasicEducationUserId != User.Identity.GetUserId())
                    return RedirectToAction("IndexAll");
                else
                    return RedirectToAction("Index");
            }

            if (User.IsInRole("Teachers") && !User.IsInRole("DepartmentsManagers"))
                ViewBag.BasicEducationUserId = User.Identity.GetUserId();
            else if (User.IsInRole("Teachers") && User.IsInRole("DepartmentsManagers"))
            {
                var userId = User.Identity.GetUserId();
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем список преподавателей кафедры
                List<string> teachersDepartment = new List<string>();
                teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentManager.DepartmentId).Select(a => a.Id).ToList();
                ViewBag.BasicEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null && teachersDepartment.Contains(a.Id))
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", basicEducation.BasicEducationUserId);
            }
            else if (User.IsInRole("Administrators"))
            {
                ViewBag.BasicEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null)
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", basicEducation.BasicEducationUserId);
            }
            //ViewBag.BasicEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null).OrderBy(a => a.Lastname), "Id", "Lastname", basicEducation.BasicEducationUserId);
            return View(basicEducation);
        }

        // GET: BasicEducations/Create
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers")]
        public ActionResult CreateAcademicDegree()
        {
            if (User.IsInRole("Teachers") && !User.IsInRole("DepartmentsManagers"))
                ViewBag.AcademicDegreeUserId = User.Identity.GetUserId();
            else if (User.IsInRole("Teachers") && User.IsInRole("DepartmentsManagers"))
            {
                var userId = User.Identity.GetUserId();
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем список преподавателей кафедры
                List<string> teachersDepartment = new List<string>();
                teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentManager.DepartmentId).Select(a => a.Id).ToList();
                ViewBag.AcademicDegreeUserId = new SelectList(db.Users.Where(a => a.DecanatId == null && teachersDepartment.Contains(a.Id))
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name");
            }
            else if (User.IsInRole("Administrators"))
            {
                ViewBag.AcademicDegreeUserId = new SelectList(db.Users.Where(a => a.DecanatId == null)
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name");
            }
            //ViewBag.AcademicDegreeUserId = teacherId;
            return View();
        }

        // POST: BasicEducations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers")]
        public async Task<ActionResult> CreateAcademicDegree([Bind(Include = "Id,AcademicDegreeUserId,Name,Year,Requisites,Path,FileName")] AcademicDegree academicDegree, HttpPostedFileBase NameFile)
        {
            if (ModelState.IsValid)
            {
                //сохраняем автора события
                if (academicDegree.AcademicDegreeUserId == null)
                    academicDegree.AcademicDegreeUserId = User.Identity.GetUserId();

                //Сохраняем изображение                
                try
                {
                    if (NameFile != null && NameFile.ContentLength > 3145728 ||
                        (NameFile != null && Path.GetExtension(NameFile.FileName.ToLower()) != ".jpg" && Path.GetExtension(NameFile.FileName.ToLower()) != ".jpeg" &&
                        Path.GetExtension(NameFile.FileName.ToLower()) != ".gif" && Path.GetExtension(NameFile.FileName.ToLower()) != ".pdf" &&
                         Path.GetExtension(NameFile.FileName.ToLower()) != ".zip" && Path.GetExtension(NameFile.FileName.ToLower()) != ".png"))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    if (NameFile != null && NameFile.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(NameFile.FileName);

                        academicDegree.FileName = NameFile.FileName;
                        string fileName = Guid.NewGuid().ToString() + extension;
                        academicDegree.Path = Server.MapPath("~/Documents/" + fileName);
                        NameFile.SaveAs(academicDegree.Path);
                        ViewBag.Message = "Загрузка файла выполнена";
                    }
                }
                catch
                {
                    ViewBag.Message = "Ошибка загрузки файла";
                }

                db.AcademicDegrees.Add(academicDegree);
                await db.SaveChangesAsync();
                if (!User.IsInRole("Teachers") || academicDegree.AcademicDegreeUserId != User.Identity.GetUserId())
                    return RedirectToAction("IndexAll");
                else
                    return RedirectToAction("Index");
            }

            if (User.IsInRole("Teachers") && !User.IsInRole("DepartmentsManagers"))
                ViewBag.AcademicDegreeUserId = User.Identity.GetUserId();
            else if (User.IsInRole("Teachers") && User.IsInRole("DepartmentsManagers"))
            {
                var userId = User.Identity.GetUserId();
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем список преподавателей кафедры
                List<string> teachersDepartment = new List<string>();
                teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentManager.DepartmentId).Select(a => a.Id).ToList();
                ViewBag.AcademicDegreeUserId = new SelectList(db.Users.Where(a => a.DecanatId == null && teachersDepartment.Contains(a.Id))
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", academicDegree.AcademicDegreeUserId);
            }
            else if (User.IsInRole("Administrators"))
            {
                ViewBag.AcademicDegreeUserId = new SelectList(db.Users.Where(a => a.DecanatId == null)
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", academicDegree.AcademicDegreeUserId);
            }

            return View(academicDegree);
        }

        // GET: BasicEducations/Create
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers")]
        public ActionResult CreateAdditionalEducation()
        {
            if (User.IsInRole("Teachers") && !User.IsInRole("DepartmentsManagers"))
                ViewBag.AdditionalEducationUserId = User.Identity.GetUserId();
            else if (User.IsInRole("Teachers") && User.IsInRole("DepartmentsManagers"))
            {
                var userId = User.Identity.GetUserId();
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем список преподавателей кафедры
                List<string> teachersDepartment = new List<string>();
                teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentManager.DepartmentId).Select(a => a.Id).ToList();
                ViewBag.AdditionalEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null && teachersDepartment.Contains(a.Id))
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name");
            }
            else if (User.IsInRole("Administrators"))
            {
                ViewBag.AdditionalEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null)
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name");
            }
            return View();
        }

        // POST: BasicEducations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers")]
        public async Task<ActionResult> CreateAdditionalEducation([Bind(Include = "Id,AdditionalEducationUserId,Name,Year,Type,Hours,Location,Path,FileName")] AdditionalEducation additionalEducation, HttpPostedFileBase NameFile)
        {
            if (ModelState.IsValid)
            {
                //сохраняем автора события
                if (additionalEducation.AdditionalEducationUserId == null)
                    additionalEducation.AdditionalEducationUserId = User.Identity.GetUserId();

                //Сохраняем изображение                
                try
                {
                    if (NameFile != null && NameFile.ContentLength > 3145728 ||
                        (NameFile != null && Path.GetExtension(NameFile.FileName.ToLower()) != ".jpg" && Path.GetExtension(NameFile.FileName.ToLower()) != ".jpeg" &&
                        Path.GetExtension(NameFile.FileName.ToLower()) != ".gif" && Path.GetExtension(NameFile.FileName.ToLower()) != ".pdf" &&
                         Path.GetExtension(NameFile.FileName.ToLower()) != ".zip" && Path.GetExtension(NameFile.FileName.ToLower()) != ".png"))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    if (NameFile != null && NameFile.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(NameFile.FileName);

                        additionalEducation.FileName = NameFile.FileName;
                        string fileName = Guid.NewGuid().ToString() + extension;
                        additionalEducation.Path = Server.MapPath("~/Documents/" + fileName);
                        NameFile.SaveAs(additionalEducation.Path);
                        ViewBag.Message = "Загрузка файла выполнена";
                    }
                }
                catch
                {
                    ViewBag.Message = "Ошибка загрузки файла";
                }

                db.AdditionalEducations.Add(additionalEducation);
                await db.SaveChangesAsync();
                if (!User.IsInRole("Teachers") || additionalEducation.AdditionalEducationUserId != User.Identity.GetUserId())
                    return RedirectToAction("IndexAll");
                else
                    return RedirectToAction("Index");
            }

            if (User.IsInRole("Teachers") && !User.IsInRole("DepartmentsManagers"))
                ViewBag.AdditionalEducationUserId = User.Identity.GetUserId();
            else if (User.IsInRole("Teachers") && User.IsInRole("DepartmentsManagers"))
            {
                var userId = User.Identity.GetUserId();
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем список преподавателей кафедры
                List<string> teachersDepartment = new List<string>();
                teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentManager.DepartmentId).Select(a => a.Id).ToList();
                ViewBag.AdditionalEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null && teachersDepartment.Contains(a.Id))
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", additionalEducation.AdditionalEducationUserId);
            }
            else if (User.IsInRole("Administrators"))
            {
                ViewBag.AdditionalEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null)
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", additionalEducation.AdditionalEducationUserId);
            }

            return View(additionalEducation);
        }


        // GET: BasicEducations/Edit/5
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers")]
        public async Task<ActionResult> EditBasicEdu(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BasicEducation basicEducation = await db.BasicEducations.FindAsync(id);
            if (basicEducation == null)
            {
                return HttpNotFound();
            }
            if (User.IsInRole("Teachers") && !User.IsInRole("DepartmentsManagers"))
                ViewBag.BasicEducationUserId = User.Identity.GetUserId();
            else if (User.IsInRole("Teachers") && User.IsInRole("DepartmentsManagers"))
            {
                var userId = User.Identity.GetUserId();
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем список преподавателей кафедры
                List<string> teachersDepartment = new List<string>();
                teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentManager.DepartmentId).Select(a => a.Id).ToList();
                ViewBag.BasicEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null && teachersDepartment.Contains(a.Id))
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", basicEducation.BasicEducationUserId);
            }
            else if (User.IsInRole("Administrators"))
            {
                ViewBag.BasicEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null)
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", basicEducation.BasicEducationUserId);
            }
            return View(basicEducation);
        }

        // POST: BasicEducations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers")]
        public async Task<ActionResult> EditBasicEdu([Bind(Include = "Id,BasicEducationUserId,EducationalInstitution,Year,Specialty,Qualification,Path,FileName")] BasicEducation basicEducation, HttpPostedFileBase File)
        {
            string userId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                //сохраняем автора записи                
                if (User.IsInRole("Teachers") && !User.IsInRole("DepartmentsManagers"))
                {
                    basicEducation.BasicEducationUserId = userId;
                }
                try
                {
                    if (File != null && File.ContentLength > 3145728 ||
                        (File != null && Path.GetExtension(File.FileName.ToLower()) != ".jpg" && Path.GetExtension(File.FileName.ToLower()) != ".jpeg" && Path.GetExtension(File.FileName.ToLower()) != ".gif"
                        && Path.GetExtension(File.FileName.ToLower()) != ".pdf" && Path.GetExtension(File.FileName.ToLower()) != ".zip" && Path.GetExtension(File.FileName.ToLower()) != ".png"))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    if (File != null && File.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(File.FileName);
                        basicEducation.FileName = File.FileName;
                        string fileName = Guid.NewGuid().ToString() + extension;
                        basicEducation.Path = Server.MapPath("~/Documents/" + fileName);
                        File.SaveAs(basicEducation.Path);
                        ViewBag.Message = "Загрузка файла выполнена";
                    }
                }
                catch
                {
                    ViewBag.Message = "Ошибка загрузки файла";
                }

                db.Entry(basicEducation).State = EntityState.Modified;
                await db.SaveChangesAsync();
                if (!User.IsInRole("Teachers") || basicEducation.BasicEducationUserId != User.Identity.GetUserId())
                    return RedirectToAction("IndexAll");
                else
                    return RedirectToAction("Index");
            }

            if (User.IsInRole("Teachers") && !User.IsInRole("DepartmentsManagers"))
                ViewBag.BasicEducationUserId = User.Identity.GetUserId();
            else if (User.IsInRole("Teachers") && User.IsInRole("DepartmentsManagers"))
            {
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем список преподавателей кафедры
                List<string> teachersDepartment = new List<string>();
                teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentManager.DepartmentId).Select(a => a.Id).ToList();
                ViewBag.BasicEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null && teachersDepartment.Contains(a.Id))
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", basicEducation.BasicEducationUserId);
            }
            else if (User.IsInRole("Administrators"))
            {
                ViewBag.BasicEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null)
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", basicEducation.BasicEducationUserId);
            }

            return View(basicEducation);
        }

        // GET: AcademicDegree/Edit/5
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers")]
        public async Task<ActionResult> EditAcademicDegree(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AcademicDegree academicDegree = await db.AcademicDegrees.FindAsync(id);
            if (academicDegree == null)
            {
                return HttpNotFound();
            }

            if (User.IsInRole("Teachers") && !User.IsInRole("DepartmentsManagers"))
                ViewBag.AcademicDegreeUserId = User.Identity.GetUserId();
            else if (User.IsInRole("Teachers") && User.IsInRole("DepartmentsManagers"))
            {
                var userId = User.Identity.GetUserId();
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем список преподавателей кафедры
                List<string> teachersDepartment = new List<string>();
                teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentManager.DepartmentId).Select(a => a.Id).ToList();
                ViewBag.AcademicDegreeUserId = new SelectList(db.Users.Where(a => a.DecanatId == null && teachersDepartment.Contains(a.Id))
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", academicDegree.AcademicDegreeUserId);
            }
            else if (User.IsInRole("Administrators"))
            {
                ViewBag.AcademicDegreeUserId = new SelectList(db.Users.Where(a => a.DecanatId == null)
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", academicDegree.AcademicDegreeUserId);
            }
            return View(academicDegree);
        }

        // POST: AcademicDegree/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers")]
        public async Task<ActionResult> EditAcademicDegree([Bind(Include = "Id,AcademicDegreeUserId,Name,Year,Requisites,Path,FileName")] AcademicDegree academicDegree, HttpPostedFileBase File)
        {
            string userId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                //сохраняем автора записи                
                if (User.IsInRole("Teachers") && !User.IsInRole("DepartmentsManagers"))
                {
                    academicDegree.AcademicDegreeUserId = userId;
                }
                try
                {
                    if (File != null && File.ContentLength > 3145728 ||
                        (File != null && Path.GetExtension(File.FileName.ToLower()) != ".jpg" && Path.GetExtension(File.FileName.ToLower()) != ".jpeg" && Path.GetExtension(File.FileName.ToLower()) != ".gif"
                        && Path.GetExtension(File.FileName.ToLower()) != ".pdf" && Path.GetExtension(File.FileName.ToLower()) != ".zip" && Path.GetExtension(File.FileName.ToLower()) != ".png"))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    if (File != null && File.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(File.FileName);
                        academicDegree.FileName = File.FileName;
                        string fileName = Guid.NewGuid().ToString() + extension;
                        academicDegree.Path = Server.MapPath("~/Documents/" + fileName);
                        File.SaveAs(academicDegree.Path);
                        ViewBag.Message = "Загрузка файла выполнена";
                    }
                }
                catch
                {
                    ViewBag.Message = "Ошибка загрузки файла";
                }

                db.Entry(academicDegree).State = EntityState.Modified;
                await db.SaveChangesAsync();
                if (!User.IsInRole("Teachers") || academicDegree.AcademicDegreeUserId != User.Identity.GetUserId())
                    return RedirectToAction("IndexAll");
                else
                    return RedirectToAction("Index");
            }

            if (User.IsInRole("Teachers") && !User.IsInRole("DepartmentsManagers"))
                ViewBag.AcademicDegreeUserId = User.Identity.GetUserId();
            else if (User.IsInRole("Teachers") && User.IsInRole("DepartmentsManagers"))
            {
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем список преподавателей кафедры
                List<string> teachersDepartment = new List<string>();
                teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentManager.DepartmentId).Select(a => a.Id).ToList();
                ViewBag.AcademicDegreeUserId = new SelectList(db.Users.Where(a => a.DecanatId == null && teachersDepartment.Contains(a.Id))
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", academicDegree.AcademicDegreeUserId);
            }
            else if (User.IsInRole("Administrators"))
            {
                ViewBag.AcademicDegreeUserId = new SelectList(db.Users.Where(a => a.DecanatId == null)
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", academicDegree.AcademicDegreeUserId);
            }

            return View(academicDegree);
        }

        // GET: AdditionalEducation/Edit/5
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers")]
        public async Task<ActionResult> EditAdditionalEducation(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdditionalEducation additionalEducation = await db.AdditionalEducations.FindAsync(id);

            if (additionalEducation == null)
            {
                return HttpNotFound();
            }
            if (User.IsInRole("Teachers") && !User.IsInRole("DepartmentsManagers"))
                ViewBag.AdditionalEducationUserId = User.Identity.GetUserId();
            else if (User.IsInRole("Teachers") && User.IsInRole("DepartmentsManagers"))
            {
                var userId = User.Identity.GetUserId();
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем список преподавателей кафедры
                List<string> teachersDepartment = new List<string>();
                teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentManager.DepartmentId).Select(a => a.Id).ToList();
                ViewBag.AdditionalEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null && teachersDepartment.Contains(a.Id))
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", additionalEducation.AdditionalEducationUserId);
            }
            else if (User.IsInRole("Administrators"))
            {
                ViewBag.AdditionalEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null)
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", additionalEducation.AdditionalEducationUserId);
            }

            return View(additionalEducation);
        }

        // POST: AdditionalEducation/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers")]
        public async Task<ActionResult> EditAdditionalEducation([Bind(Include = "Id,AdditionalEducationUserId,Name,Year,Type,Hours,Location,Path,FileName")] AdditionalEducation additionalEducation, HttpPostedFileBase File)
        {
            string userId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                //сохраняем автора записи                
                if (User.IsInRole("Teachers") && !User.IsInRole("DepartmentsManagers"))
                {
                    additionalEducation.AdditionalEducationUserId = userId;
                }
                try
                {
                    if (File != null && File.ContentLength > 3145728 ||
                        (File != null && Path.GetExtension(File.FileName.ToLower()) != ".jpg" && Path.GetExtension(File.FileName.ToLower()) != ".jpeg" && Path.GetExtension(File.FileName.ToLower()) != ".gif"
                        && Path.GetExtension(File.FileName.ToLower()) != ".pdf" && Path.GetExtension(File.FileName.ToLower()) != ".zip" && Path.GetExtension(File.FileName.ToLower()) != ".png"))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    if (File != null && File.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(File.FileName);
                        additionalEducation.FileName = File.FileName;
                        string fileName = Guid.NewGuid().ToString() + extension;
                        additionalEducation.Path = Server.MapPath("~/Documents/" + fileName);
                        File.SaveAs(additionalEducation.Path);
                        ViewBag.Message = "Загрузка файла выполнена";
                    }
                }
                catch
                {
                    ViewBag.Message = "Ошибка загрузки файла";
                }

                db.Entry(additionalEducation).State = EntityState.Modified;
                await db.SaveChangesAsync();
                if (!User.IsInRole("Teachers") || additionalEducation.AdditionalEducationUserId != User.Identity.GetUserId())
                    return RedirectToAction("IndexAll");
                else
                    return RedirectToAction("Index");
            }

            if (User.IsInRole("Teachers") && !User.IsInRole("DepartmentsManagers"))
                ViewBag.AdditionalEducationUserId = User.Identity.GetUserId();
            else if (User.IsInRole("Teachers") && User.IsInRole("DepartmentsManagers"))
            {
                var departmentManager = db.Users.FirstOrDefault(a => a.Id == userId);
                //получаем список преподавателей кафедры
                List<string> teachersDepartment = new List<string>();
                teachersDepartment = db.Users.Where(a => a.DepartmentId == departmentManager.DepartmentId).Select(a => a.Id).ToList();
                ViewBag.AdditionalEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null && teachersDepartment.Contains(a.Id))
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", additionalEducation.AdditionalEducationUserId);
            }
            else if (User.IsInRole("Administrators"))
            {
                ViewBag.AdditionalEducationUserId = new SelectList(db.Users.Where(a => a.DecanatId == null)
                    .Select(a => new TutorsList { Id = a.Id, Name = a.Lastname + " " + a.Firstname + " " + a.Middlename })
                    .OrderBy(a => a.Name), "Id", "Name", additionalEducation.AdditionalEducationUserId);
            }

            return View(additionalEducation);
        }

        // GET: BasicEducations/Delete/5
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers")]
        public ActionResult DeleteBasicEdu(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BasicEducation basicEducation = db.BasicEducations.Include(a => a.User).FirstOrDefault(a => a.Id == id);
            if (basicEducation == null)
            {
                return HttpNotFound();
            }
            return View(basicEducation);
        }

        // POST: BasicEducations/Delete/5
        [HttpPost, ActionName("DeleteBasicEdu")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedBasicEdu(int id)
        {
            BasicEducation basicEducation = null;
            if (User.IsInRole("Teachers"))
            {
                string userId = User.Identity.GetUserId();
                basicEducation = await db.BasicEducations.Where(a => a.BasicEducationUserId == userId).Include(a => a.User)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("DepartmentsManagers") || User.IsInRole("Administrators"))
            {
                basicEducation = await db.BasicEducations.SingleOrDefaultAsync(m => m.Id == id);
            }
            else return HttpNotFound();

            if (basicEducation == null)
                return HttpNotFound();

            db.BasicEducations.Remove(basicEducation);
            await db.SaveChangesAsync();

            //удаление файла подтверждения
            if (basicEducation.Path != null)
                System.IO.File.Delete(basicEducation.Path);

            if (User.IsInRole("Teachers"))
                return RedirectToAction("Index");
            else
                return RedirectToAction("IndexAll");
        }

        // GET: AcademicDegree/Delete/5
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers")]
        public ActionResult DeleteAcademicDegree(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AcademicDegree academicDegree = db.AcademicDegrees.Include(a => a.User).FirstOrDefault(a => a.Id == id);
            if (academicDegree == null)
            {
                return HttpNotFound();
            }
            return View(academicDegree);
        }

        // POST: AcademicDegree/Delete/5
        [HttpPost, ActionName("DeleteAcademicDegree")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAcademicDegree(int id)
        {
            AcademicDegree academicDegree = null;
            if (User.IsInRole("Teachers"))
            {
                string userId = User.Identity.GetUserId();
                academicDegree = await db.AcademicDegrees.Where(a => a.AcademicDegreeUserId == userId).Include(a => a.User)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("DepartmentsManagers") || User.IsInRole("Administrators"))
            {
                academicDegree = await db.AcademicDegrees.Include(a => a.User).SingleOrDefaultAsync(m => m.Id == id);
            }
            else return HttpNotFound();

            if (academicDegree == null)
                return HttpNotFound();

            db.AcademicDegrees.Remove(academicDegree);
            await db.SaveChangesAsync();

            //удаление файла подтверждения
            if (academicDegree.Path != null)
                System.IO.File.Delete(academicDegree.Path);

            if (User.IsInRole("Teachers"))
                return RedirectToAction("Index");
            else
                return RedirectToAction("IndexAll");
        }

        // GET: AdditionalEducation/Delete/5
        [Authorize(Roles = "Administrators,Teachers,DepartmentsManagers")]
        public ActionResult DeleteAdditionalEducation(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdditionalEducation additionalEducation = db.AdditionalEducations.Include(a => a.User).FirstOrDefault(a => a.Id == id);
            if (additionalEducation == null)
            {
                return HttpNotFound();
            }
            return View(additionalEducation);
        }

        // POST: AdditionalEducation/Delete/5
        [HttpPost, ActionName("DeleteAdditionalEducation")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAdditionalEducation(int id)
        {
            AdditionalEducation additionalEducation = null;
            if (User.IsInRole("Teachers"))
            {
                string userId = User.Identity.GetUserId();
                additionalEducation = await db.AdditionalEducations.Where(a => a.AdditionalEducationUserId == userId).Include(a => a.User)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("DepartmentsManagers") || User.IsInRole("Administrators"))
            {
                additionalEducation = await db.AdditionalEducations.Include(a => a.User).SingleOrDefaultAsync(m => m.Id == id);
            }
            else return HttpNotFound();

            if (additionalEducation == null)
                return HttpNotFound();

            db.AdditionalEducations.Remove(additionalEducation);
            await db.SaveChangesAsync();

            //удаление файла подтверждения
            if (additionalEducation.Path != null)
                System.IO.File.Delete(additionalEducation.Path);
            if (User.IsInRole("Teachers"))
                return RedirectToAction("Index");
            else
                return RedirectToAction("IndexAll");
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
