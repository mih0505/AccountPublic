using Accounts.Models;
using ClosedXML.Excel;
using Microsoft.AspNet.Identity;
using Spire.Xls;
using Spire.Xls.Charts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Accounts.Controllers
{
    public class ReportsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BranchCharts
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> BranchCharts()
        {
            // Получаем список учебных годов
            var years = db.Statements
                .OrderByDescending(Year => Year.CurrentYear)
                .Select(Year => new { Value = Year.CurrentYear, Text = Year.CurrentYear })
                .Distinct()
                .OrderBy(Year => Year.Text)
                .ToList();

            var yearsView = new List<SelectListItem>();

            foreach (var item in years)
            {
                yearsView.Add(new SelectListItem()
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            // Получаем список форм обучения
            var formsOfTraining = await db.FormOfTrainings.ToListAsync();
            var formsOfTrainingView = new List<SelectListItem>();

            foreach (var item in formsOfTraining)
            {
                formsOfTrainingView.Add(new SelectListItem()
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }

            // Выделяем текущий учебный год в списке учебных годов
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear").Value;
            yearsView.FirstOrDefault(Year => Year.Value == currentYear).Selected = true;

            // Заполняем ViewBag'и содержимым
            ViewBag.YearsList = yearsView;
            ViewBag.FormsOfTrainingList = formsOfTrainingView;
            ViewBag.TermsList = listSessions;

            return View();
        }

        private string GetRGBColors(int elementsCount)
        {
            var random = new Random();

            var red = random.Next(0, 255);
            var green = random.Next(0, 255);
            var blue = random.Next(0, 255);

            var result = $"'rgba({red}, {green}, {blue}, 0.2)'";

            for (int i = 2; i <= elementsCount; i++)
            {
                red = random.Next(0, 255);
                green = random.Next(0, 255);
                blue = random.Next(0, 255);

                result += $", 'rgba({red}, {green}, {blue}, 0.2)'";
            }

            return result;
        }

        private string GetBordersColor(string backgroundColors)
        {
            return backgroundColors.Replace("0.2", "1");
        }

        // POST: BranchCharts
        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> BranchCharts(BranchStatisticViewModel model, string buttonAction)
        {
            if (model.Date.Year == 1)
            {
                return RedirectToAction("BranchCharts");
            }

            // Получение списка факультетов
            var faculties = await db.Faculties.Include("Profiles").Where(Faculty => !Faculty.IsDeleted).ToListAsync();

            // Получение формы обучения
            var formOfTraining = db.FormOfTrainings.FirstOrDefault(FormOfTraining => FormOfTraining.Id == model.FormOfTrainingId);

            // Получаем список учебных годов
            var years = db.Statements.OrderByDescending(Year => Year.CurrentYear).Select(Year => new { Value = Year.CurrentYear, Text = Year.CurrentYear }).Distinct().OrderBy(Year => Year.Text).ToList();
            var yearsView = new List<SelectListItem>();

            foreach (var item in years)
            {
                yearsView.Add(new SelectListItem()
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            // Получаем список форм обучения
            var formsOfTraining = await db.FormOfTrainings.ToListAsync();
            var formsOfTrainingView = new List<SelectListItem>();

            foreach (var item in formsOfTraining)
            {
                formsOfTrainingView.Add(new SelectListItem()
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }

            // Заполняем ViewBag'и содержимым
            ViewBag.YearsList = yearsView;
            ViewBag.FormsOfTrainingList = formsOfTrainingView;
            ViewBag.TermsList = listSessions;

            var facultiesStatistic = new List<ChartFacultyStatisticViewModel>();

            foreach (var faculty in faculties)
            {
                var chartStatistics = new List<ChartProfileStatisticViewModel>();

                foreach (var profile in faculty.Profiles)
                {
                    try
                    {
                        // Выгрузка отсортированных оценок
                        var statementsStudents = GetStatements(model, profile);
                        var excelStatistics = GetStatistic(statementsStudents);

                        chartStatistics.Add(new ChartProfileStatisticViewModel()
                        {
                            ProfileName = profile.ShortName,
                            AllStudentsCount = ExcelStatisticViewModel.AllStudentsCount,
                            AllPassedCount = ExcelStatisticViewModel.AllPassedCount,
                            AllPassedOnExcellent = ExcelStatisticViewModel.AllPassedOnExcellent,
                            AllPassedOnVeryWell = ExcelStatisticViewModel.AllPassedOnVeryWell,
                            AllPassedOnGood = ExcelStatisticViewModel.AllPassedOnGood,
                            AllPassedOnOkey = ExcelStatisticViewModel.AllPassedOnOkey,
                            Perfomance = ExcelStatisticViewModel.AllPassedCount,
                            Quality = ExcelStatisticViewModel.AllPassedOnExcellent + ExcelStatisticViewModel.AllPassedOnVeryWell + ExcelStatisticViewModel.AllPassedOnGood,
                            ExcellentStudents = ExcelStatisticViewModel.AllPassedOnExcellent,
                            PerfomancePercent = ExcelStatisticViewModel.AllPassedCountPercent,
                            QualityPercent = ExcelStatisticViewModel.AllPassedOnExcellentPercent + ExcelStatisticViewModel.AllPassedOnVeryWellPercent + ExcelStatisticViewModel.AllPassedOnGoodPercent,
                            ExcellentStudentsPercent = ExcelStatisticViewModel.AllPassedOnExcellentPercent
                        });
                    }

                    catch (Exception)
                    {
                        continue;
                    }
                }

                var allStudentsCount = 0.0;
                var allPassedCount = 0.0;
                var allPassedOnExcellent = 0.0;
                var allPassedOnVeryWell = 0.0;
                var allPassedOnGood = 0.0;
                var allPassedOnOkey = 0.0;
                var perfomance = 0.0;
                var quality = 0.0;
                var excellentStudent = 0.0;
                var averagePerfomancePercent = 0.0;
                var averageQualityPercent = 0.0;
                var averageExcellentStudentPercent = 0.0;

                if (chartStatistics.Count != 0)
                {
                    allStudentsCount = chartStatistics.Sum(Statistic => Statistic.AllStudentsCount);

                    if (!allStudentsCount.Equals(0.0))
                    {
                        allPassedCount = chartStatistics.Sum(Statistic => Statistic.AllPassedCount);
                        allPassedOnExcellent = chartStatistics.Sum(Statistic => Statistic.AllPassedOnExcellent);
                        allPassedOnVeryWell = chartStatistics.Sum(Statistic => Statistic.AllPassedOnVeryWell);
                        allPassedOnGood = chartStatistics.Sum(Statistic => Statistic.AllPassedOnGood);
                        allPassedOnOkey = chartStatistics.Sum(Statistic => Statistic.AllPassedOnOkey);
                        perfomance = chartStatistics.Sum(Statistic => Statistic.Perfomance);
                        quality = chartStatistics.Sum(Statistic => Statistic.Quality);
                        excellentStudent = chartStatistics.Sum(Statistic => Statistic.ExcellentStudents);
                        averagePerfomancePercent = Math.Round((chartStatistics.Sum(Statistic => Statistic.Perfomance) / chartStatistics.Sum(Statistic => Statistic.AllStudentsCount)) * 100.0, 2);
                        averageQualityPercent = Math.Round((chartStatistics.Sum(Statistic => Statistic.Quality) / chartStatistics.Sum(Statistic => Statistic.AllStudentsCount)) * 100.00, 2);
                        averageExcellentStudentPercent = Math.Round((chartStatistics.Sum(Statistic => Statistic.ExcellentStudents) / chartStatistics.Sum(Statistic => Statistic.AllStudentsCount)) * 100.00, 2);
                    }
                }
                // 413
                facultiesStatistic.Add(new ChartFacultyStatisticViewModel()
                {
                    FacultyShortName = faculty.AliasFaculty,
                    FacultyName = faculty.Name,
                    AllStudentsCount = allStudentsCount,
                    AllPassedCount = allPassedCount,
                    AllPassedOnExcellent = allPassedOnExcellent,
                    AllPassedOnVeryWell = allPassedOnVeryWell,
                    AllPassedOnGood = allPassedOnGood,
                    AllPassedOnOkey = allPassedOnOkey,
                    Perfomance = perfomance,
                    Quality = quality,
                    ExcellentStudents = excellentStudent,
                    PerfomancePercent = averagePerfomancePercent,
                    QualityPercent = averageQualityPercent,
                    ExcellentStudentsPercent = averageExcellentStudentPercent
                });
            }

            if (buttonAction == "Показать")
            {
                try
                {
                    var culture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

                    var chartLabels = facultiesStatistic.Select(Statistic => Statistic.FacultyShortName).ToArray();
                    var labels = $"'{chartLabels[0]}'";
                    for (int i = 1; i < chartLabels.Length; i++)
                    {
                        labels += $",'{chartLabels[i]}'";
                    }

                    var chartPerfomance = facultiesStatistic.Select(Statistic => Statistic.PerfomancePercent).ToArray();
                    var perfomance = chartPerfomance[0].ToString(culture);
                    for (int i = 1; i < chartLabels.Length; i++)
                    {
                        perfomance += $",{chartPerfomance[i].ToString(culture)}";
                    }

                    var chartQuality = facultiesStatistic.Select(Statistic => Statistic.QualityPercent).ToArray();
                    var quality = chartQuality[0].ToString(culture);
                    for (int i = 1; i < chartLabels.Length; i++)
                    {
                        quality += $",{chartQuality[i].ToString(culture)}";
                    }

                    var chartExcellentStudents = facultiesStatistic.Select(Statistic => Statistic.ExcellentStudentsPercent).ToArray();
                    var excellentStudents = chartExcellentStudents[0].ToString(culture);
                    for (int i = 1; i < chartLabels.Length; i++)
                    {
                        excellentStudents += $",{chartExcellentStudents[i].ToString(culture)}";
                    }

                    var branchPerfomance = Math.Round((facultiesStatistic.Sum(Statistic => Statistic.Perfomance) / facultiesStatistic.Sum(Statistic => Statistic.AllStudentsCount)) * 100.00, 2);
                    var branchQuality = Math.Round((facultiesStatistic.Sum(Statistic => Statistic.Quality) / facultiesStatistic.Sum(Statistic => Statistic.AllStudentsCount)) * 100.00, 2);
                    var branchExcellentStudent = Math.Round((facultiesStatistic.Sum(Statistic => Statistic.ExcellentStudents) / facultiesStatistic.Sum(Statistic => Statistic.AllStudentsCount)) * 100.00, 2);

                    ViewBag.FacultiesStatistic = facultiesStatistic;
                    ViewBag.ChartLabels = labels;
                    ViewBag.ChartPerfomance = perfomance;
                    ViewBag.ChartQuality = quality;
                    ViewBag.ChartExcellentStudents = excellentStudents;
                    ViewBag.AveragePerfomance = branchPerfomance;
                    ViewBag.AverageQuality = branchQuality;
                    ViewBag.AverageExcellentStudent = branchExcellentStudent;
                    ViewBag.ChartAverage = $"{branchPerfomance.ToString(culture)},{branchQuality.ToString(culture)},{branchExcellentStudent.ToString(culture)}";

                    var backgroundColors = GetRGBColors(facultiesStatistic.Count);
                    var borderColors = GetBordersColor(backgroundColors);
                    ViewBag.Background = backgroundColors;
                    ViewBag.Border = borderColors;

                    return View(model);
                }

                catch (Exception)
                {
                    return RedirectToAction("BranchCharts");
                }
            }
            else if (buttonAction == "Экспортировать")
            {
                using (var workbook = GetXLBranchCharts(model, facultiesStatistic))
                {
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveToStream(stream, FileFormat.Version2010);
                        var bytes = stream.ToArray();
                        var type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        return File(bytes, type, $"({DateTime.Now.ToShortDateString()}) Успеваемость по филиалу ({formOfTraining.Name} форма обучения, {model.Term.ToLower()}).xlsx");
                    }
                }
            }

            return RedirectToAction("BranchCharts");
        }

        #region Старый код BranchCharts
        //[Authorize(Roles = "Administrators")]
        //public async Task<ActionResult> BranchCharts()
        //{
        //    Получаем список учебных годов
        //    var years = db.Statements.OrderByDescending(Year => Year.CurrentYear).Select(Year => new { Value = Year.CurrentYear, Text = Year.CurrentYear }).Distinct().OrderBy(Year => Year.Text).ToList();
        //    var yearsView = new List<SelectListItem>();

        //    foreach (var year in years)
        //    {
        //        yearsView.Add(new SelectListItem()
        //        {
        //            Text = year.Text,
        //            Value = year.Value
        //        });
        //    }

        //    Получаем список форм обучения
        //    var formsOfTraining = await db.FormOfTrainings.ToListAsync();
        //    var formsOfTrainingView = new List<SelectListItem>();

        //    foreach (var formOfTraining in formsOfTraining)
        //    {
        //        formsOfTrainingView.Add(new SelectListItem()
        //        {
        //            Text = formOfTraining.Name,
        //            Value = formOfTraining.Id.ToString()
        //        });
        //    }

        //    Выделяем текущий учебный год в списке учебных годов
        //    var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear").Value;
        //    yearsView.FirstOrDefault(Year => Year.Value == currentYear).Selected = true;

        //    Заполняем ViewBag'и содержимым
        //    ViewBag.YearsList = yearsView;
        //    ViewBag.FormsOfTrainingList = formsOfTrainingView;
        //    ViewBag.TermsList = listSessions;

        //    return View();
        //}

        //[HttpPost]
        //[Authorize(Roles = "Administrators")]
        //public async Task<ActionResult> BranchCharts(BranchStatisticViewModel model)
        //{
        //    var faculties = await db.Faculties.Include("Profiles").Where(Faculty => !Faculty.IsDeleted).ToListAsync();

        //    Получение формы обучения
        //   var formOfTraining = db.FormOfTrainings.FirstOrDefault(FormOfTraining => FormOfTraining.Id == model.FormOfTrainingId);

        //    Заполнение переменных для вывода в Excel
        //    var termText = null as string;
        //    if (model.Term == "Зимняя сессия")
        //    {
        //        termText = "зимней";
        //    }
        //    else if (model.Term == "Летняя сессия")
        //    {
        //        termText = "летней";
        //    }
        //    else
        //    {
        //        termText = "всех";
        //    }

        //    var years = model.Year.Split('-');
        //    var yearFirst = years[0];
        //    var yearSecond = years[1];

        //    Получаем список учебных годов
        //    var yearsList = db.Statements.OrderByDescending(Year => Year.CurrentYear).Select(Year => new { Value = Year.CurrentYear, Text = Year.CurrentYear }).Distinct().OrderBy(Year => Year.Text).ToList();
        //    var yearsView = new List<SelectListItem>();

        //    foreach (var year in yearsList)
        //    {
        //        yearsView.Add(new SelectListItem()
        //        {
        //            Text = year.Text,
        //            Value = year.Value
        //        });
        //    }

        //    Получаем список форм обучения
        //    var formsOfTrainingForView = await db.FormOfTrainings.ToListAsync();
        //    var formsOfTrainingView = new List<SelectListItem>();

        //    foreach (var formOfTrainingForView in formsOfTrainingForView)
        //    {
        //        formsOfTrainingView.Add(new SelectListItem()
        //        {
        //            Text = formOfTrainingForView.Name,
        //            Value = formOfTrainingForView.Id.ToString()
        //        });
        //    }

        //    Выделяем текущий учебный год в списке учебных годов
        //    var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear").Value;
        //    yearsView.FirstOrDefault(Year => Year.Value == currentYear).Selected = true;

        //    Заполняем ViewBag'и содержимым
        //    ViewBag.YearsList = yearsView;
        //    ViewBag.FormsOfTrainingList = formsOfTrainingView;
        //    ViewBag.TermsList = listSessions;

        //    var facultiesStatistic = new List<ChartFacultyStatisticViewModel>();

        //    foreach (var faculty in faculties)
        //    {
        //        var chartStatistics = new List<ChartProfileStatisticViewModel>();

        //        foreach (var profile in faculty.Profiles)
        //        {
        //            try
        //            {
        //                Выгрузка отсортированных оценок
        //               var statementsStudents = null as IQueryable<StatementStudent>;

        //                if (model.Term == "Зимняя сессия")
        //                {
        //                    statementsStudents = from StatementStudent in db.StatementStudents.Include("Statement")
        //                                         join Statement in db.Statements on StatementStudent.StatementId equals Statement.Id
        //                                         join Group in db.Groups on Statement.GroupId equals Group.Id
        //                                         where (Statement.ProfileId.Value == profile.Id) &&
        //                                         (Statement.CurrentYear == model.Year) &&
        //                                         (Statement.TypeControl == "Экзамен") &&
        //                                         (Group.FormOfTrainingId == formOfTraining.Id) &&
        //                                         (Statement.Semester % 2 == 1) &&
        //                                         ((StatementStudent.Date.Value <= model.Date) || (StatementStudent.Date == null))
        //                                         select StatementStudent;
        //                }
        //                else if (model.Term == "Летняя сессия")
        //                {
        //                    statementsStudents = from StatementStudent in db.StatementStudents.Include("Statement")
        //                                         join Statement in db.Statements on StatementStudent.StatementId equals Statement.Id
        //                                         join Group in db.Groups on Statement.GroupId equals Group.Id
        //                                         where (Statement.ProfileId.Value == profile.Id) &&
        //                                         (Statement.CurrentYear == model.Year) &&
        //                                         (Statement.TypeControl == "Экзамен") &&
        //                                         (Group.FormOfTrainingId == formOfTraining.Id) &&
        //                                         (Statement.Semester % 2 == 0) &&
        //                                         ((StatementStudent.Date.Value <= model.Date) || (StatementStudent.Date == null))
        //                                         select StatementStudent;
        //                }
        //                else
        //                {
        //                    statementsStudents = from StatementStudent in db.StatementStudents.Include("Statement")
        //                                         join Statement in db.Statements on StatementStudent.StatementId equals Statement.Id
        //                                         join Group in db.Groups on Statement.GroupId equals Group.Id
        //                                         where (Statement.ProfileId.Value == profile.Id) &&
        //                                         (Statement.CurrentYear == model.Year) &&
        //                                         (Statement.TypeControl == "Экзамен") &&
        //                                         (Group.FormOfTrainingId == formOfTraining.Id)
        //                                         select StatementStudent;
        //                }

        //                var excelStatistics = new List<ExcelStatisticViewModel>();

        //                Высчитывание номера последнего курса
        //                var maxCourse = statementsStudents.Select(StatementStudent => StatementStudent.Statement).Max(Statement => Statement.Course);

        //                Расчёт статистики
        //                for (int course = 1; course <= maxCourse; course++)
        //                {
        //                    var studentsGrades = statementsStudents.Where(StatementStudent => StatementStudent.Statement.Course == course)
        //                        .GroupBy(StatementStudent => StatementStudent.StudentStatementId)
        //                        .ToList();

        //                    var studentsCount = studentsGrades.Count();

        //                    var excelStatisticViewModel = new ExcelStatisticViewModel()
        //                    {
        //                        Course = course,
        //                        StudentsCount = studentsCount
        //                    };

        //                    foreach (var studentGrades in studentsGrades)
        //                    {
        //                        var gradesCount = studentGrades.Count();

        //                        int excellentGrades = 0,
        //                            goodGrades = 0,
        //                            okeyGrades = 0,
        //                            badGrades = 0;

        //                        foreach (var grade in studentGrades)
        //                        {
        //                            if ((grade.GradeByNumber == 2) || (grade.Grade == "Не явился"))
        //                            {
        //                                badGrades++;
        //                            }
        //                            else if (grade.GradeByNumber == 5)
        //                            {
        //                                excellentGrades++;
        //                            }
        //                            else if (grade.GradeByNumber == 4)
        //                            {
        //                                goodGrades++;
        //                            }
        //                            else if (grade.GradeByNumber == 3)
        //                            {
        //                                okeyGrades++;
        //                            }
        //                        }

        //                        if (badGrades == 0)
        //                        {
        //                            excelStatisticViewModel.PassedCount++;

        //                            if (excellentGrades == gradesCount)
        //                            {
        //                                excelStatisticViewModel.PassedOnExcellent++;
        //                            }
        //                            else if (goodGrades == gradesCount)
        //                            {
        //                                excelStatisticViewModel.PassedOnGood++;
        //                            }
        //                            else if (okeyGrades != 0)
        //                            {
        //                                excelStatisticViewModel.PassedOnOkey++;
        //                            }
        //                            else
        //                            {
        //                                excelStatisticViewModel.PassedOnVeryWell++;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            excelStatisticViewModel.DebtorsCount++;

        //                            if (badGrades == 1)
        //                            {
        //                                excelStatisticViewModel.DebtorsInOne++;
        //                            }
        //                            else if (badGrades == 2)
        //                            {
        //                                excelStatisticViewModel.DebtorsInTwo++;
        //                            }
        //                            else if (badGrades >= 3)
        //                            {
        //                                excelStatisticViewModel.DebtorsInMoreThree++;
        //                            }
        //                        }
        //                    }

        //                    excelStatistics.Add(excelStatisticViewModel);
        //                }

        //                ExcelStatisticViewModel.AllStudentsCount = excelStatistics.Sum(Statistic => Statistic.StudentsCount);
        //                ExcelStatisticViewModel.AllPassedCount = excelStatistics.Sum(Statistic => Statistic.PassedCount);
        //                ExcelStatisticViewModel.AllPassedOnExcellent = excelStatistics.Sum(Statistic => Statistic.PassedOnExcellent);
        //                ExcelStatisticViewModel.AllPassedOnVeryWell = excelStatistics.Sum(Statistic => Statistic.PassedOnVeryWell);
        //                ExcelStatisticViewModel.AllPassedOnGood = excelStatistics.Sum(Statistic => Statistic.PassedOnGood);
        //                ExcelStatisticViewModel.AllPassedOnOkey = excelStatistics.Sum(Statistic => Statistic.PassedOnOkey);
        //                ExcelStatisticViewModel.AllDebtorsCount = excelStatistics.Sum(Statistic => Statistic.DebtorsCount);
        //                ExcelStatisticViewModel.AllDebtorsInOne = excelStatistics.Sum(Statistic => Statistic.DebtorsInOne);
        //                ExcelStatisticViewModel.AllDebtorsInTwo = excelStatistics.Sum(Statistic => Statistic.DebtorsInTwo);
        //                ExcelStatisticViewModel.AllDebtorsInMoreThree = excelStatistics.Sum(Statistic => Statistic.DebtorsInMoreThree);

        //                chartStatistics.Add(new ChartProfileStatisticViewModel()
        //                {
        //                    ProfileName = profile.ShortName,
        //                    Perfomance = ExcelStatisticViewModel.AllPassedCountPercent,
        //                    Quality = ExcelStatisticViewModel.AllPassedOnExcellentPercent + ExcelStatisticViewModel.AllPassedOnVeryWellPercent + ExcelStatisticViewModel.AllPassedOnGoodPercent,
        //                    ExcellentStudents = ExcelStatisticViewModel.AllPassedOnExcellentPercent
        //                });
        //            }

        //            catch (Exception)
        //            {
        //                continue;
        //            }
        //        }

        //        var averagePerfomance = 0.0;
        //        var averageQuality = 0.0;
        //        var averageExcellentStudent = 0.0;

        //        if (chartStatistics.Count != 0)
        //        {
        //            averagePerfomance = Math.Round((chartStatistics.Sum(Statistic => Statistic.Perfomance)) / chartStatistics.Count, 2);
        //            averageQuality = Math.Round((chartStatistics.Sum(Statistic => Statistic.Quality)) / chartStatistics.Count, 2);
        //            averageExcellentStudent = Math.Round((chartStatistics.Sum(Statistic => Statistic.ExcellentStudents)) / chartStatistics.Count, 2);
        //        }

        //        facultiesStatistic.Add(new ChartFacultyStatisticViewModel()
        //        {
        //            FacultyShortName = faculty.AliasFaculty,
        //            FacultyName = faculty.Name,
        //            Perfomance = averagePerfomance,
        //            Quality = averageQuality,
        //            ExcellentStudents = averageExcellentStudent
        //        });
        //    }

        //    var culture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

        //    var chartLabels = facultiesStatistic.Select(Statistic => Statistic.FacultyShortName).ToArray();
        //    var labels = $"'{chartLabels[0]}'";
        //    for (int i = 1; i < chartLabels.Length; i++)
        //    {
        //        labels += $",'{chartLabels[i]}'";
        //    }

        //    var chartPerfomance = facultiesStatistic.Select(Statistic => Statistic.Perfomance).ToArray();
        //    var perfomance = chartPerfomance[0].ToString(culture);
        //    for (int i = 1; i < chartLabels.Length; i++)
        //    {
        //        perfomance += $",{chartPerfomance[i].ToString(culture)}";
        //    }

        //    var chartQuality = facultiesStatistic.Select(Statistic => Statistic.Quality).ToArray();
        //    var quality = chartQuality[0].ToString(culture);
        //    for (int i = 1; i < chartLabels.Length; i++)
        //    {
        //        quality += $",{chartQuality[i].ToString(culture)}";
        //    }

        //    var chartExcellentStudents = facultiesStatistic.Select(Statistic => Statistic.ExcellentStudents).ToArray();
        //    var excellentStudents = chartExcellentStudents[0].ToString(culture);
        //    for (int i = 1; i < chartLabels.Length; i++)
        //    {
        //        excellentStudents += $",{chartExcellentStudents[i].ToString(culture)}";
        //    }

        //    var branchPerfomance = Math.Round((facultiesStatistic.Sum(Statistic => Statistic.Perfomance)) / facultiesStatistic.Count, 2);
        //    var branchQuality = Math.Round((facultiesStatistic.Sum(Statistic => Statistic.Quality)) / facultiesStatistic.Count, 2);
        //    var branchExcellentStudent = Math.Round((facultiesStatistic.Sum(Statistic => Statistic.ExcellentStudents)) / facultiesStatistic.Count, 2);

        //    ViewBag.FacultiesStatistic = facultiesStatistic;
        //    ViewBag.ChartLabels = labels;
        //    ViewBag.ChartPerfomance = perfomance;
        //    ViewBag.ChartQuality = quality;
        //    ViewBag.ChartExcellentStudents = excellentStudents;
        //    ViewBag.ChartAverage = $"{branchPerfomance.ToString(culture)},{branchQuality.ToString(culture)},{branchExcellentStudent.ToString(culture)}";

        //    return View(model);
        //}
        #endregion

        // GET: FacultyCharts
        [Authorize(Roles = "Administrators, FacultiesManagers")]
        public async Task<ActionResult> FacultyCharts()
        {
            // Получаем список направлений (профилей)
            var faculties = await db.Faculties.Where(Faculty => !Faculty.IsDeleted).OrderBy(Faculty => Faculty.Name).ToListAsync();
            var facultiesView = new List<SelectListItem>();

            foreach (var item in faculties)
            {
                facultiesView.Add(new SelectListItem()
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }

            // Получаем список учебных годов
            var years = db.Statements.OrderByDescending(Year => Year.CurrentYear).Select(Year => new { Value = Year.CurrentYear, Text = Year.CurrentYear }).Distinct().OrderBy(Year => Year.Text).ToList();
            var yearsView = new List<SelectListItem>();

            foreach (var item in years)
            {
                yearsView.Add(new SelectListItem()
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            // Получаем список форм обучения
            var formsOfTrainings = await db.FormOfTrainings.ToListAsync();
            var formsOfTrainingView = new List<SelectListItem>();

            foreach (var item in formsOfTrainings)
            {
                formsOfTrainingView.Add(new SelectListItem()
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }

            // Выделяем текущий учебный год в списке учебных годов
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear").Value;
            yearsView.FirstOrDefault(Year => Year.Value == currentYear).Selected = true;

            // Заполняем ViewBag'и содержимым
            ViewBag.FacultiesList = facultiesView;
            ViewBag.YearsList = yearsView;
            ViewBag.FormsOfTrainingList = formsOfTrainingView;
            ViewBag.TermsList = listSessions;

            return View();
        }

        // POST: FacultyCharts
        [HttpPost]
        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public async Task<ActionResult> FacultyCharts(FacultyStatisticViewModel model, string buttonAction)
        {
            // Получаем список направлений (профилей)
            var faculties = await db.Faculties.Where(Faculty => !Faculty.IsDeleted).OrderBy(Faculty => Faculty.Name).ToListAsync();
            var facultiesView = new List<SelectListItem>();

            foreach (var item in faculties)
            {
                facultiesView.Add(new SelectListItem()
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }

            // Получаем список учебных годов
            var years = db.Statements.OrderByDescending(Year => Year.CurrentYear).Select(Year => new { Value = Year.CurrentYear, Text = Year.CurrentYear }).Distinct().OrderBy(Year => Year.Text).ToList();
            var yearsView = new List<SelectListItem>();

            foreach (var item in years)
            {
                yearsView.Add(new SelectListItem()
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            // Получаем список форм обучения
            var formsOfTrainings = await db.FormOfTrainings.ToListAsync();
            var formsOfTrainingView = new List<SelectListItem>();

            foreach (var item in formsOfTrainings)
            {
                formsOfTrainingView.Add(new SelectListItem()
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }

            // Заполняем ViewBag'и содержимым
            ViewBag.FacultiesList = facultiesView;
            ViewBag.YearsList = yearsView;
            ViewBag.FormsOfTrainingList = formsOfTrainingView;
            ViewBag.TermsList = listSessions;

            // Получение факультета
            var faculty = db.Faculties.Include("Profiles").FirstOrDefault(Faculty => Faculty.Id == model.FacultyId);

            // Получение формы обучения
            var formOfTraining = db.FormOfTrainings.FirstOrDefault(FormOfTraining => FormOfTraining.Id == model.FormOfTrainingId);

            var chartStatistics = new List<ChartProfileStatisticViewModel>();

            foreach (var profile in faculty.Profiles)
            {
                try
                {
                    // Выгрузка отсортированных оценок
                    var statementsStudents = GetStatements(model, profile);

                    var excelStatistics = GetStatistic(statementsStudents);

                    chartStatistics.Add(new ChartProfileStatisticViewModel()
                    {
                        ProfileName = profile.ShortName,
                        AllStudentsCount = ExcelStatisticViewModel.AllStudentsCount,
                        AllPassedCount = ExcelStatisticViewModel.AllPassedCount,
                        AllPassedOnExcellent = ExcelStatisticViewModel.AllPassedOnExcellent,
                        AllPassedOnVeryWell = ExcelStatisticViewModel.AllPassedOnVeryWell,
                        AllPassedOnGood = ExcelStatisticViewModel.AllPassedOnGood,
                        AllPassedOnOkey = ExcelStatisticViewModel.AllPassedOnOkey,
                        Perfomance = ExcelStatisticViewModel.AllPassedCount,
                        Quality = ExcelStatisticViewModel.AllPassedOnExcellent + ExcelStatisticViewModel.AllPassedOnVeryWell + ExcelStatisticViewModel.AllPassedOnGood,
                        ExcellentStudents = ExcelStatisticViewModel.AllPassedOnExcellent,
                        PerfomancePercent = ExcelStatisticViewModel.AllPassedCountPercent,
                        QualityPercent = ExcelStatisticViewModel.AllPassedOnExcellentPercent + ExcelStatisticViewModel.AllPassedOnVeryWellPercent + ExcelStatisticViewModel.AllPassedOnGoodPercent,
                        ExcellentStudentsPercent = ExcelStatisticViewModel.AllPassedOnExcellentPercent
                    });
                }

                catch (Exception)
                {
                    continue;
                }
            }

            if (buttonAction == "Показать")
            {
                try
                {
                    var chartViewLabels = chartStatistics.Select(Statistic => Statistic.ProfileName).ToArray();
                    var chartPerfomanceViewData = chartStatistics.Select(Statistic => Statistic.PerfomancePercent).ToArray();
                    var chartQualityViewData = chartStatistics.Select(Statistic => Statistic.QualityPercent).ToArray();
                    var chartExcellentStudentsViewData = chartStatistics.Select(Statistic => Statistic.ExcellentStudentsPercent).ToArray();

                    var culture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

                    var labels = $"'{chartViewLabels[0]}'";

                    for (int i = 1; i < chartViewLabels.Length; i++)
                    {
                        labels += $",'{chartViewLabels[i]}'";
                    }

                    var perfomance = chartPerfomanceViewData[0].ToString(culture);

                    for (int i = 1; i < chartViewLabels.Length; i++)
                    {
                        perfomance += $",{chartPerfomanceViewData[i].ToString(culture)}";
                    }

                    var quality = chartQualityViewData[0].ToString(culture);

                    for (int i = 1; i < chartViewLabels.Length; i++)
                    {
                        quality += $",{chartQualityViewData[i].ToString(culture)}";
                    }

                    var excellentStudents = chartExcellentStudentsViewData[0].ToString(culture);

                    for (int i = 1; i < chartViewLabels.Length; i++)
                    {
                        excellentStudents += $",{chartExcellentStudentsViewData[i].ToString(culture)}";
                    }

                    var averagePerfomance = Math.Round((chartStatistics.Sum(Statistic => Statistic.Perfomance) / chartStatistics.Sum(Statistic => Statistic.AllStudentsCount)) * 100.0, 2);
                    var averageQuality = Math.Round((chartStatistics.Sum(Statistic => Statistic.Quality) / chartStatistics.Sum(Statistic => Statistic.AllStudentsCount)) * 100.00, 2);
                    var averageExcellentStudent = Math.Round((chartStatistics.Sum(Statistic => Statistic.ExcellentStudents) / chartStatistics.Sum(Statistic => Statistic.AllStudentsCount)) * 100.00, 2);

                    ViewBag.ProfilesStatistic = chartStatistics;
                    ViewBag.ChartLabels = labels;
                    ViewBag.ChartPerfomance = perfomance;
                    ViewBag.ChartQuality = quality;
                    ViewBag.ChartExcellentStudents = excellentStudents;
                    ViewBag.AveragePerfomance = averagePerfomance;
                    ViewBag.AverageQuality = averageQuality;
                    ViewBag.AverageExcellentStudent = averageExcellentStudent;
                    ViewBag.ChartAverage = $"{averagePerfomance.ToString(culture)},{averageQuality.ToString(culture)},{averageExcellentStudent.ToString(culture)}";

                    var backgroundColors = GetRGBColors(chartStatistics.Count);
                    var borderColors = GetBordersColor(backgroundColors);
                    ViewBag.Background = backgroundColors;
                    ViewBag.Border = borderColors;

                    return View(model);
                }

                catch (Exception)
                {
                    return RedirectToAction("FacultyCharts");
                }
            }
            else if (buttonAction == "Экспортировать")
            {
                using (var workbook = GetXLFacultyCharts(model, chartStatistics))
                {
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveToStream(stream, FileFormat.Version2010);
                        var bytes = stream.ToArray();
                        var type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        return File(bytes, type, $"({DateTime.Now.ToShortDateString()}) Успеваемость по факультету {faculty.AliasFaculty} ({formOfTraining.Name} форма обучения, {model.Term.ToLower()}).xlsx");
                    }
                }
            }

            return RedirectToAction("FacultyCharts");
        }

        #region Старый код FacultyCharts
        //[Authorize(Roles = "Administrators, FacultiesManagers")]
        //public async Task<ActionResult> FacultyCharts()
        //{
        //    // Получаем список направлений (профилей)
        //    var faculties = await db.Faculties.Where(Faculty => !Faculty.IsDeleted).OrderBy(Faculty => Faculty.Name).ToListAsync();
        //    var facultiesView = new List<SelectListItem>();

        //    foreach (var faculty in faculties)
        //    {
        //        facultiesView.Add(new SelectListItem()
        //        {
        //            Text = faculty.Name,
        //            Value = faculty.Id.ToString()
        //        });
        //    }

        //    // Получаем список учебных годов
        //    var years = db.Statements.OrderByDescending(Year => Year.CurrentYear).Select(Year => new { Value = Year.CurrentYear, Text = Year.CurrentYear }).Distinct().OrderBy(Year => Year.Text).ToList();
        //    var yearsView = new List<SelectListItem>();

        //    foreach (var year in years)
        //    {
        //        yearsView.Add(new SelectListItem()
        //        {
        //            Text = year.Text,
        //            Value = year.Value
        //        });
        //    }

        //    // Получаем список форм обучения
        //    var formsOfTraining = await db.FormOfTrainings.ToListAsync();
        //    var formsOfTrainingView = new List<SelectListItem>();

        //    foreach (var formOfTraining in formsOfTraining)
        //    {
        //        formsOfTrainingView.Add(new SelectListItem()
        //        {
        //            Text = formOfTraining.Name,
        //            Value = formOfTraining.Id.ToString()
        //        });
        //    }

        //    // Выделяем текущий учебный год в списке учебных годов
        //    var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear").Value;
        //    yearsView.FirstOrDefault(Year => Year.Value == currentYear).Selected = true;

        //    // Заполняем ViewBag'и содержимым
        //    ViewBag.FacultiesList = facultiesView;
        //    ViewBag.YearsList = yearsView;
        //    ViewBag.FormsOfTrainingList = formsOfTrainingView;
        //    ViewBag.TermsList = listSessions;

        //    return View();
        //}

        //[HttpPost]
        //[Authorize(Roles = "Administrators,FacultiesManagers")]
        //public async Task<ActionResult> FacultyCharts(FacultyStatisticViewModel model)
        //{
        //    var faculty = db.Faculties.Include("Profiles").FirstOrDefault(Faculty => Faculty.Id == model.FacultyId);

        //    // Получение формы обучения
        //    var formOfTraining = db.FormOfTrainings.FirstOrDefault(FormOfTraining => FormOfTraining.Id == model.FormOfTrainingId);

        //    // Заполнение переменных для вывода в Excel
        //    var termText = null as string;
        //    if (model.Term == "Зимняя сессия")
        //    {
        //        termText = "зимней";
        //    }
        //    else if (model.Term == "Летняя сессия")
        //    {
        //        termText = "летней";
        //    }
        //    else
        //    {
        //        termText = "всех";
        //    }

        //    var years = model.Year.Split('-');
        //    var yearFirst = years[0];
        //    var yearSecond = years[1];

        //    // Получаем список направлений (профилей)
        //    var facultiesForView = await db.Faculties.Where(Faculty => !Faculty.IsDeleted).OrderBy(Faculty => Faculty.Name).ToListAsync();
        //    var facultiesView = new List<SelectListItem>();

        //    foreach (var facultyForView in facultiesForView)
        //    {
        //        facultiesView.Add(new SelectListItem()
        //        {
        //            Text = facultyForView.Name,
        //            Value = facultyForView.Id.ToString()
        //        });
        //    }

        //    // Получаем список учебных годов
        //    var yearsList = db.Statements.OrderByDescending(Year => Year.CurrentYear).Select(Year => new { Value = Year.CurrentYear, Text = Year.CurrentYear }).Distinct().OrderBy(Year => Year.Text).ToList();
        //    var yearsView = new List<SelectListItem>();

        //    foreach (var year in yearsList)
        //    {
        //        yearsView.Add(new SelectListItem()
        //        {
        //            Text = year.Text,
        //            Value = year.Value
        //        });
        //    }

        //    // Получаем список форм обучения
        //    var formsOfTrainingForView = await db.FormOfTrainings.ToListAsync();
        //    var formsOfTrainingView = new List<SelectListItem>();

        //    foreach (var formOfTrainingForView in formsOfTrainingForView)
        //    {
        //        formsOfTrainingView.Add(new SelectListItem()
        //        {
        //            Text = formOfTrainingForView.Name,
        //            Value = formOfTrainingForView.Id.ToString()
        //        });
        //    }

        //    // Выделяем текущий учебный год в списке учебных годов
        //    var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear").Value;
        //    yearsView.FirstOrDefault(Year => Year.Value == currentYear).Selected = true;

        //    // Заполняем ViewBag'и содержимым
        //    ViewBag.FacultiesList = facultiesView;
        //    ViewBag.YearsList = yearsView;
        //    ViewBag.FormsOfTrainingList = formsOfTrainingView;
        //    ViewBag.TermsList = listSessions;

        //    var chartStatistics = new List<ChartProfileStatisticViewModel>();

        //    foreach (var profile in faculty.Profiles)
        //    {
        //        try
        //        {
        //            // Выгрузка отсортированных оценок
        //            var statementsStudents = null as IQueryable<StatementStudent>;

        //            if (model.Term == "Зимняя сессия")
        //            {
        //                statementsStudents = from StatementStudent in db.StatementStudents.Include("Statement")
        //                                     join Statement in db.Statements on StatementStudent.StatementId equals Statement.Id
        //                                     join Group in db.Groups on Statement.GroupId equals Group.Id
        //                                     where (Statement.ProfileId.Value == profile.Id) &&
        //                                     (Statement.CurrentYear == model.Year) &&
        //                                     (Statement.TypeControl == "Экзамен") &&
        //                                     (Group.FormOfTrainingId == formOfTraining.Id) &&
        //                                     (Statement.Semester % 2 == 1) &&
        //                                     ((StatementStudent.Date.Value <= model.Date) || (StatementStudent.Date == null))
        //                                     select StatementStudent;
        //            }
        //            else if (model.Term == "Летняя сессия")
        //            {
        //                statementsStudents = from StatementStudent in db.StatementStudents.Include("Statement")
        //                                     join Statement in db.Statements on StatementStudent.StatementId equals Statement.Id
        //                                     join Group in db.Groups on Statement.GroupId equals Group.Id
        //                                     where (Statement.ProfileId.Value == profile.Id) &&
        //                                     (Statement.CurrentYear == model.Year) &&
        //                                     (Statement.TypeControl == "Экзамен") &&
        //                                     (Group.FormOfTrainingId == formOfTraining.Id) &&
        //                                     (Statement.Semester % 2 == 0) &&
        //                                     ((StatementStudent.Date.Value <= model.Date) || (StatementStudent.Date == null))
        //                                     select StatementStudent;
        //            }
        //            else
        //            {
        //                statementsStudents = from StatementStudent in db.StatementStudents.Include("Statement")
        //                                     join Statement in db.Statements on StatementStudent.StatementId equals Statement.Id
        //                                     join Group in db.Groups on Statement.GroupId equals Group.Id
        //                                     where (Statement.ProfileId.Value == profile.Id) &&
        //                                     (Statement.CurrentYear == model.Year) &&
        //                                     (Statement.TypeControl == "Экзамен") &&
        //                                     (Group.FormOfTrainingId == formOfTraining.Id)
        //                                     select StatementStudent;
        //            }

        //            var excelStatistics = new List<ExcelStatisticViewModel>();

        //            // Высчитывание номера последнего курса
        //            var maxCourse = statementsStudents.Select(StatementStudent => StatementStudent.Statement).Max(Statement => Statement.Course);

        //            // Расчёт статистики
        //            for (int course = 1; course <= maxCourse; course++)
        //            {
        //                var studentsGrades = statementsStudents.Where(StatementStudent => StatementStudent.Statement.Course == course)
        //                    .GroupBy(StatementStudent => StatementStudent.StudentStatementId)
        //                    .ToList();

        //                var studentsCount = studentsGrades.Count();

        //                var excelStatisticViewModel = new ExcelStatisticViewModel()
        //                {
        //                    Course = course,
        //                    StudentsCount = studentsCount
        //                };

        //                foreach (var studentGrades in studentsGrades)
        //                {
        //                    var gradesCount = studentGrades.Count();

        //                    int excellentGrades = 0,
        //                        goodGrades = 0,
        //                        okeyGrades = 0,
        //                        badGrades = 0;

        //                    foreach (var grade in studentGrades)
        //                    {
        //                        if ((grade.GradeByNumber == 2) || (grade.Grade == "Не явился"))
        //                        {
        //                            badGrades++;
        //                        }
        //                        else if (grade.GradeByNumber == 5)
        //                        {
        //                            excellentGrades++;
        //                        }
        //                        else if (grade.GradeByNumber == 4)
        //                        {
        //                            goodGrades++;
        //                        }
        //                        else if (grade.GradeByNumber == 3)
        //                        {
        //                            okeyGrades++;
        //                        }
        //                    }

        //                    if (badGrades == 0)
        //                    {
        //                        excelStatisticViewModel.PassedCount++;

        //                        if (excellentGrades == gradesCount)
        //                        {
        //                            excelStatisticViewModel.PassedOnExcellent++;
        //                        }
        //                        else if (goodGrades == gradesCount)
        //                        {
        //                            excelStatisticViewModel.PassedOnGood++;
        //                        }
        //                        else if (okeyGrades != 0)
        //                        {
        //                            excelStatisticViewModel.PassedOnOkey++;
        //                        }
        //                        else
        //                        {
        //                            excelStatisticViewModel.PassedOnVeryWell++;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        excelStatisticViewModel.DebtorsCount++;

        //                        if (badGrades == 1)
        //                        {
        //                            excelStatisticViewModel.DebtorsInOne++;
        //                        }
        //                        else if (badGrades == 2)
        //                        {
        //                            excelStatisticViewModel.DebtorsInTwo++;
        //                        }
        //                        else if (badGrades >= 3)
        //                        {
        //                            excelStatisticViewModel.DebtorsInMoreThree++;
        //                        }
        //                    }
        //                }

        //                excelStatistics.Add(excelStatisticViewModel);
        //            }

        //            ExcelStatisticViewModel.AllStudentsCount = excelStatistics.Sum(Statistic => Statistic.StudentsCount);
        //            ExcelStatisticViewModel.AllPassedCount = excelStatistics.Sum(Statistic => Statistic.PassedCount);
        //            ExcelStatisticViewModel.AllPassedOnExcellent = excelStatistics.Sum(Statistic => Statistic.PassedOnExcellent);
        //            ExcelStatisticViewModel.AllPassedOnVeryWell = excelStatistics.Sum(Statistic => Statistic.PassedOnVeryWell);
        //            ExcelStatisticViewModel.AllPassedOnGood = excelStatistics.Sum(Statistic => Statistic.PassedOnGood);
        //            ExcelStatisticViewModel.AllPassedOnOkey = excelStatistics.Sum(Statistic => Statistic.PassedOnOkey);
        //            ExcelStatisticViewModel.AllDebtorsCount = excelStatistics.Sum(Statistic => Statistic.DebtorsCount);
        //            ExcelStatisticViewModel.AllDebtorsInOne = excelStatistics.Sum(Statistic => Statistic.DebtorsInOne);
        //            ExcelStatisticViewModel.AllDebtorsInTwo = excelStatistics.Sum(Statistic => Statistic.DebtorsInTwo);
        //            ExcelStatisticViewModel.AllDebtorsInMoreThree = excelStatistics.Sum(Statistic => Statistic.DebtorsInMoreThree);

        //            chartStatistics.Add(new ChartProfileStatisticViewModel()
        //            {
        //                ProfileName = profile.ShortName,
        //                Perfomance = ExcelStatisticViewModel.AllPassedCountPercent,
        //                Quality = ExcelStatisticViewModel.AllPassedOnExcellentPercent + ExcelStatisticViewModel.AllPassedOnVeryWellPercent + ExcelStatisticViewModel.AllPassedOnGoodPercent,
        //                ExcellentStudents = ExcelStatisticViewModel.AllPassedOnExcellentPercent
        //            });
        //        }

        //        catch (Exception)
        //        {
        //            continue;
        //        }
        //    }

        //    var chartViewLabels = chartStatistics.Select(Statistic => Statistic.ProfileName).ToArray();
        //    var chartPerfomanceViewData = chartStatistics.Select(Statistic => Statistic.Perfomance).ToArray();
        //    var chartQualityViewData = chartStatistics.Select(Statistic => Statistic.Quality).ToArray();
        //    var chartExcellentStudentsViewData = chartStatistics.Select(Statistic => Statistic.ExcellentStudents).ToArray();

        //    var culture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

        //    var labels = $"'{chartViewLabels[0]}'";

        //    for (int i = 1; i < chartViewLabels.Length; i++)
        //    {
        //        labels += $",'{chartViewLabels[i]}'";
        //    }

        //    var perfomance = chartPerfomanceViewData[0].ToString(culture);

        //    for (int i = 1; i < chartViewLabels.Length; i++)
        //    {
        //        perfomance += $",{chartPerfomanceViewData[i].ToString(culture)}";
        //    }

        //    var quality = chartQualityViewData[0].ToString(culture);

        //    for (int i = 1; i < chartViewLabels.Length; i++)
        //    {
        //        quality += $",{chartQualityViewData[i].ToString(culture)}";
        //    }

        //    var excellentStudents = chartExcellentStudentsViewData[0].ToString(culture);

        //    for (int i = 1; i < chartViewLabels.Length; i++)
        //    {
        //        excellentStudents += $",{chartExcellentStudentsViewData[i].ToString(culture)}";
        //    }

        //    var averagePerfomance = Math.Round((chartStatistics.Sum(Statistic => Statistic.Perfomance)) / chartStatistics.Count, 2);
        //    var averageQuality = Math.Round((chartStatistics.Sum(Statistic => Statistic.Quality)) / chartStatistics.Count, 2);
        //    var averageExcellentStudent = Math.Round((chartStatistics.Sum(Statistic => Statistic.ExcellentStudents)) / chartStatistics.Count, 2);

        //    ViewBag.ChartLabels = labels;
        //    ViewBag.ChartPerfomance = perfomance;
        //    ViewBag.ChartQuality = quality;
        //    ViewBag.ChartExcellentStudents = excellentStudents;
        //    ViewBag.ChartAverage = $"{averagePerfomance.ToString(culture)},{averageQuality.ToString(culture)},{averageExcellentStudent.ToString(culture)}";

        //    return View(model);
        //}
        #endregion

        // GET: StatisticExport
        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public async Task<ActionResult> StatisticExport()
        {
            // Получаем список направлений (профилей)
            var profiles = await db.Profiles.Where(Profile => !Profile.IsDeleted).OrderBy(Profile => Profile.Name).ToListAsync();
            var profilesView = new List<SelectListItem>();

            foreach (var item in profiles)
            {
                profilesView.Add(new SelectListItem()
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }

            // Получаем список учебных годов
            var years = db.Statements.OrderByDescending(Year => Year.CurrentYear).Select(Year => new { Value = Year.CurrentYear, Text = Year.CurrentYear }).Distinct().OrderBy(Year => Year.Text).ToList();
            var yearsView = new List<SelectListItem>();

            foreach (var item in years)
            {
                yearsView.Add(new SelectListItem()
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            // Получаем список форм обучения
            var formsOfTrainings = await db.FormOfTrainings.ToListAsync();
            var formsOfTrainingView = new List<SelectListItem>();

            foreach (var item in formsOfTrainings)
            {
                formsOfTrainingView.Add(new SelectListItem()
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }

            // Выделяем текущий учебный год в списке учебных годов
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear").Value;
            yearsView.FirstOrDefault(Year => Year.Value == currentYear).Selected = true;

            // Заполняем ViewBag'и содержимым
            ViewBag.ProfilesList = profilesView;
            ViewBag.YearsList = yearsView;
            ViewBag.FormsOfTrainingList = formsOfTrainingView;
            ViewBag.TermsList = listSessions;

            return View();
        }

        // POST: StatisticExport
        [HttpPost]
        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public async Task<ActionResult> StatisticExport(ProfileStatisticViewModel model, string buttonAction)
        {
            try
            {
                // Получение формы обучения
                var formOfTraining = db.FormOfTrainings.FirstOrDefault(FormOfTraining => FormOfTraining.Id == model.FormOfTrainingId);

                // Получение программы обучения
                var profile = db.Profiles.FirstOrDefault(Profile => Profile.Id == model.ProfileId);

                // Выгрузка отсортированных по полям оценок
                var statementsStudents = GetStatements(model);
                var statistics = GetStatistic(statementsStudents);

                if (buttonAction == "Показать")
                {
                    // Получаем список направлений (профилей)
                    var profiles = await db.Profiles.Where(Profile => !Profile.IsDeleted).OrderBy(Profile => Profile.Name).ToListAsync();
                    var profilesView = new List<SelectListItem>();

                    foreach (var item in profiles)
                    {
                        profilesView.Add(new SelectListItem()
                        {
                            Text = item.Name,
                            Value = item.Id.ToString()
                        });
                    }

                    // Получаем список учебных годов
                    var years = db.Statements.OrderByDescending(Year => Year.CurrentYear).Select(Year => new { Value = Year.CurrentYear, Text = Year.CurrentYear }).Distinct().OrderBy(Year => Year.Text).ToList();
                    var yearsView = new List<SelectListItem>();

                    foreach (var item in years)
                    {
                        yearsView.Add(new SelectListItem()
                        {
                            Text = item.Text,
                            Value = item.Value
                        });
                    }

                    // Получаем список форм обучения
                    var formsOfTrainings = await db.FormOfTrainings.ToListAsync();
                    var formsOfTrainingView = new List<SelectListItem>();

                    foreach (var item in formsOfTrainings)
                    {
                        formsOfTrainingView.Add(new SelectListItem()
                        {
                            Text = item.Name,
                            Value = item.Id.ToString()
                        });
                    }

                    // Выделяем текущий учебный год в списке учебных годов
                    var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear").Value;
                    yearsView.FirstOrDefault(Year => Year.Value == currentYear).Selected = true;

                    // Заполняем ViewBag'и содержимым
                    ViewBag.ProfilesList = profilesView;
                    ViewBag.YearsList = yearsView;
                    ViewBag.FormsOfTrainingList = formsOfTrainingView;
                    ViewBag.TermsList = listSessions;
                    ViewBag.StatisticList = statistics;
                    ViewBag.Profile = profile;
                    ViewBag.FormOfTraining = formOfTraining.Name;
                    ViewBag.Term = model.Term;
                    ViewBag.Year = model.Year;

                    return View(model);
                }
                else if (buttonAction == "Экспортировать")
                {
                    // Высчитывание номера последнего курса
                    var maxCourse = statementsStudents.Select(StatementStudent => StatementStudent.Statement).Max(Statement => Statement.Course);

                    using (var workbook = GetXLWorkbook(maxCourse, model, statistics))
                    {
                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            var bytes = stream.ToArray();
                            var type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            return File(bytes, type, $"({DateTime.Now.ToShortDateString()}) Успеваемость по направлению {profile.ShortName} ({formOfTraining.Name} форма обучения, {model.Term.ToLower()}).xlsx");
                        }
                    }
                }

                return RedirectToAction("StatisticExport");
            }

            catch (Exception)
            {
                return RedirectToAction("StatisticExport");
            }
        }

        #region Старый код StatisticExport
        //[HttpPost]
        //[Authorize(Roles = "Administrators,FacultiesManagers")]
        //public async Task<ActionResult> StatisticExport(ProfileStatisticViewModel model, string buttonAction)
        //{
        //    try
        //    {
        //        // Получение формы обучения
        //        var formOfTraining = db.FormOfTrainings.FirstOrDefault(FormOfTraining => FormOfTraining.Id == model.FormOfTrainingId);

        //        // Получение программы обучения
        //        var profile = db.Profiles.FirstOrDefault(Profile => Profile.Id == model.ProfileId);

        //        // Заполнение переменных для вывода в Excel
        //        var termText = null as string;
        //        if (model.Term == "Зимняя сессия")
        //        {
        //            termText = "зимней";
        //        }
        //        else if (model.Term == "Летняя сессия")
        //        {
        //            termText = "летней";
        //        }
        //        else
        //        {
        //            termText = "всех";
        //        }

        //        var years = model.Year.Split('-');
        //        var yearFirst = years[0];
        //        var yearSecond = years[1];

        //        // Выгрузка отсортированных по полям оценок
        //        var statementsStudents = null as IQueryable<StatementStudent>;

        //        if (model.Term == "Зимняя сессия")
        //        {
        //            statementsStudents = from StatementStudent in db.StatementStudents.Include("Statement")
        //                                 join Statement in db.Statements on StatementStudent.StatementId equals Statement.Id
        //                                 join Group in db.Groups on Statement.GroupId equals Group.Id
        //                                 where (Statement.ProfileId.Value == profile.Id) &&
        //                                 (Statement.CurrentYear == model.Year) &&
        //                                 (Statement.TypeControl == "Экзамен") &&
        //                                 (Group.FormOfTrainingId == formOfTraining.Id) &&
        //                                 (Statement.Semester % 2 == 1) &&
        //                                 ((StatementStudent.Date.Value <= model.Date) || (StatementStudent.Date == null))
        //                                 select StatementStudent;
        //        }
        //        else if (model.Term == "Летняя сессия")
        //        {
        //            statementsStudents = from StatementStudent in db.StatementStudents.Include("Statement")
        //                                 join Statement in db.Statements on StatementStudent.StatementId equals Statement.Id
        //                                 join Group in db.Groups on Statement.GroupId equals Group.Id
        //                                 where (Statement.ProfileId.Value == profile.Id) &&
        //                                 (Statement.CurrentYear == model.Year) &&
        //                                 (Statement.TypeControl == "Экзамен") &&
        //                                 (Group.FormOfTrainingId == formOfTraining.Id) &&
        //                                 (Statement.Semester % 2 == 0) &&
        //                                 ((StatementStudent.Date.Value <= model.Date) || (StatementStudent.Date == null))
        //                                 select StatementStudent;
        //        }
        //        else
        //        {
        //            statementsStudents = from StatementStudent in db.StatementStudents.Include("Statement")
        //                                 join Statement in db.Statements on StatementStudent.StatementId equals Statement.Id
        //                                 join Group in db.Groups on Statement.GroupId equals Group.Id
        //                                 where (Statement.ProfileId.Value == profile.Id) &&
        //                                 (Statement.CurrentYear == model.Year) &&
        //                                 (Statement.TypeControl == "Экзамен") &&
        //                                 (Group.FormOfTrainingId == formOfTraining.Id)
        //                                 select StatementStudent;
        //        }

        //        var excelStatistics = new List<ExcelStatisticViewModel>();

        //        // Высчитывание номера последнего курса
        //        var maxCourse = statementsStudents.Select(StatementStudent => StatementStudent.Statement).Max(Statement => Statement.Course);

        //        // Расчёт статистики
        //        for (int course = 1; course <= maxCourse; course++)
        //        {
        //            var studentsGrades = statementsStudents.Where(StatementStudent => StatementStudent.Statement.Course == course)
        //                .GroupBy(StatementStudent => StatementStudent.StudentStatementId)
        //                .ToList();

        //            var studentsCount = studentsGrades.Count();

        //            var excelStatisticViewModel = new ExcelStatisticViewModel()
        //            {
        //                Course = course,
        //                StudentsCount = studentsCount
        //            };

        //            foreach (var studentGrades in studentsGrades)
        //            {
        //                var gradesCount = studentGrades.Count();

        //                int excellentGrades = 0,
        //                    goodGrades = 0,
        //                    okeyGrades = 0,
        //                    badGrades = 0;

        //                foreach (var grade in studentGrades)
        //                {
        //                    if ((grade.GradeByNumber == 2) || (grade.Grade == "Не явился"))
        //                    {
        //                        badGrades++;
        //                    }
        //                    else if (grade.GradeByNumber == 5)
        //                    {
        //                        excellentGrades++;
        //                    }
        //                    else if (grade.GradeByNumber == 4)
        //                    {
        //                        goodGrades++;
        //                    }
        //                    else if (grade.GradeByNumber == 3)
        //                    {
        //                        okeyGrades++;
        //                    }
        //                }

        //                if (badGrades == 0)
        //                {
        //                    excelStatisticViewModel.PassedCount++;

        //                    if (excellentGrades == gradesCount)
        //                    {
        //                        excelStatisticViewModel.PassedOnExcellent++;
        //                    }
        //                    else if (goodGrades == gradesCount)
        //                    {
        //                        excelStatisticViewModel.PassedOnGood++;
        //                    }
        //                    else if (okeyGrades != 0)
        //                    {
        //                        excelStatisticViewModel.PassedOnOkey++;
        //                    }
        //                    else
        //                    {
        //                        excelStatisticViewModel.PassedOnVeryWell++;
        //                    }
        //                }
        //                else
        //                {
        //                    excelStatisticViewModel.DebtorsCount++;

        //                    if (badGrades == 1)
        //                    {
        //                        excelStatisticViewModel.DebtorsInOne++;
        //                    }
        //                    else if (badGrades == 2)
        //                    {
        //                        excelStatisticViewModel.DebtorsInTwo++;
        //                    }
        //                    else if (badGrades >= 3)
        //                    {
        //                        excelStatisticViewModel.DebtorsInMoreThree++;
        //                    }
        //                }
        //            }

        //            excelStatistics.Add(excelStatisticViewModel);
        //        }

        //        ExcelStatisticViewModel.AllStudentsCount = excelStatistics.Sum(Statistic => Statistic.StudentsCount);
        //        ExcelStatisticViewModel.AllPassedCount = excelStatistics.Sum(Statistic => Statistic.PassedCount);
        //        ExcelStatisticViewModel.AllPassedOnExcellent = excelStatistics.Sum(Statistic => Statistic.PassedOnExcellent);
        //        ExcelStatisticViewModel.AllPassedOnVeryWell = excelStatistics.Sum(Statistic => Statistic.PassedOnVeryWell);
        //        ExcelStatisticViewModel.AllPassedOnGood = excelStatistics.Sum(Statistic => Statistic.PassedOnGood);
        //        ExcelStatisticViewModel.AllPassedOnOkey = excelStatistics.Sum(Statistic => Statistic.PassedOnOkey);
        //        ExcelStatisticViewModel.AllDebtorsCount = excelStatistics.Sum(Statistic => Statistic.DebtorsCount);
        //        ExcelStatisticViewModel.AllDebtorsInOne = excelStatistics.Sum(Statistic => Statistic.DebtorsInOne);
        //        ExcelStatisticViewModel.AllDebtorsInTwo = excelStatistics.Sum(Statistic => Statistic.DebtorsInTwo);
        //        ExcelStatisticViewModel.AllDebtorsInMoreThree = excelStatistics.Sum(Statistic => Statistic.DebtorsInMoreThree);

        //        if (buttonAction == "Показать")
        //        {
        //            // Получаем список направлений (профилей)
        //            var profiles = await db.Profiles.Where(Profile => !Profile.IsDeleted).OrderBy(Profile => Profile.Name).ToListAsync();
        //            var profilesView = new List<SelectListItem>();

        //            foreach (var direction in profiles)
        //            {
        //                profilesView.Add(new SelectListItem()
        //                {
        //                    Text = direction.Name,
        //                    Value = direction.Id.ToString()
        //                });
        //            }

        //            // Получаем список учебных годов
        //            var yearsList = db.Statements.OrderByDescending(Year => Year.CurrentYear).Select(Year => new { Value = Year.CurrentYear, Text = Year.CurrentYear }).Distinct().OrderBy(Year => Year.Text).ToList();
        //            var yearsView = new List<SelectListItem>();

        //            foreach (var year in yearsList)
        //            {
        //                yearsView.Add(new SelectListItem()
        //                {
        //                    Text = year.Text,
        //                    Value = year.Value
        //                });
        //            }

        //            // Получаем список форм обучения
        //            var formsOfTraining = await db.FormOfTrainings.ToListAsync();
        //            var formsOfTrainingView = new List<SelectListItem>();

        //            foreach (var form in formsOfTraining)
        //            {
        //                formsOfTrainingView.Add(new SelectListItem()
        //                {
        //                    Text = form.Name,
        //                    Value = form.Id.ToString()
        //                });
        //            }

        //            //// Выделяем текущий учебный год в списке учебных годов
        //            //var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear").Value;
        //            //yearsView.FirstOrDefault(Year => Year.Value == currentYear).Selected = true;

        //            // Заполняем ViewBag'и содержимым
        //            ViewBag.ProfilesList = profilesView;
        //            ViewBag.YearsList = yearsView;
        //            ViewBag.FormsOfTrainingList = formsOfTrainingView;
        //            ViewBag.TermsList = listSessions;
        //            ViewBag.StatisticList = excelStatistics;
        //            ViewBag.Profile = profile;
        //            ViewBag.FormOfTraining = formOfTraining.Name;
        //            ViewBag.Term = model.Term;
        //            ViewBag.Year = model.Year;

        //            return View(model);
        //        }
        //        else if (buttonAction == "Экспортировать")
        //        {
        //            // Формирование Excel файла
        //            using (XLWorkbook workbook = new XLWorkbook())
        //            {
        //                var worksheet = workbook.AddWorksheet(profile.ShortName);

        //                var profileCells = worksheet.Range("D1:K1").Merge();
        //                profileCells.Value = $"{profile.Code1}. {profile.Name}";
        //                profileCells.Style.Alignment.WrapText = true;

        //                worksheet.Range("B1:C1").Merge().Value = "Направление";
        //                worksheet.Range("D2:K2").Merge().Value = $"{formOfTraining.Name.ToLower()} форма обучения";

        //                worksheet.Column("D").Width = 12.71;
        //                worksheet.Column("E").Width = 12.57;
        //                worksheet.Column("F").Width = 17.71;
        //                worksheet.Column("G").Width = 12.00;
        //                worksheet.Column("K").Width = 14.57;
        //                worksheet.Row(1).Height = 41.25;
        //                worksheet.Row(5).Height = 36.75;

        //                worksheet.Cell("D3").Value = "Результаты";

        //                var termCell = worksheet.Cell("E3");
        //                termCell.Style.Font.Bold = true;
        //                termCell.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
        //                termCell.Value = termText;

        //                if (termText == "всех")
        //                {
        //                    worksheet.Cell("F3").Value = "экзаменационных";
        //                    worksheet.Cell("G3").Value = "сессий";
        //                }
        //                else
        //                {
        //                    worksheet.Cell("F3").Value = "экзаменационной";
        //                    worksheet.Cell("G3").Value = "сессии";
        //                }

        //                var yearFirstCell = worksheet.Cell("H3");
        //                yearFirstCell.Style.Font.Bold = true;
        //                yearFirstCell.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
        //                yearFirstCell.Value = yearFirst;

        //                worksheet.Cell("I3").Value = "/";

        //                var yearSecondCell = worksheet.Cell("J3");
        //                yearSecondCell.Style.Font.Bold = true;
        //                yearSecondCell.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
        //                yearSecondCell.Value = yearSecond;

        //                worksheet.Cell("K3").Value = "учебного года";

        //                worksheet.Range("D1:K2").Style.Font.Bold = true;

        //                worksheet.Range("A5:B5").Merge().Value = "Количество студентов";
        //                worksheet.Range("C5:G5").Merge().Value = "Сдали все экзамены";
        //                worksheet.Range("H5:K5").Merge().Value = "Имеют задолженности";
        //                worksheet.Range("A5:K5").Style.Fill.BackgroundColor = XLColor.DeepPeach;

        //                worksheet.Cell("A6").Value = "Курс";
        //                worksheet.Cell("B6").Value = "По списку, чел.";
        //                worksheet.Cell("C6").Value = "Всего, чел., %";
        //                worksheet.Cell("D6").Value = "На отл., чел., %";
        //                worksheet.Cell("E6").Value = "На хор. и отл., чел., %";
        //                worksheet.Cell("F6").Value = "На хор., чел., %";
        //                worksheet.Cell("G6").Value = "Имеют удовлетв., чел., %";
        //                worksheet.Cell("H6").Value = "Всего, чел., %";
        //                worksheet.Cell("I6").Value = "По 1 предм., чел., %";
        //                worksheet.Cell("J6").Value = "По 2 предм., чел., %";
        //                worksheet.Cell("K6").Value = "По 3 и более предм., чел., %";

        //                worksheet.Cell("A7").Value = "1";
        //                worksheet.Cell("B7").Value = "2";
        //                worksheet.Cell("C7").Value = "3";
        //                worksheet.Cell("D7").Value = "4";
        //                worksheet.Cell("E7").Value = "5";
        //                worksheet.Cell("F7").Value = "6";
        //                worksheet.Cell("G7").Value = "7";
        //                worksheet.Cell("H7").Value = "8";
        //                worksheet.Cell("I7").Value = "9";
        //                worksheet.Cell("J7").Value = "10";
        //                worksheet.Cell("K7").Value = "11";
        //                worksheet.Range("A6:K7").Style.Fill.BackgroundColor = XLColor.Peach;

        //                worksheet.Range("A5:K7").Style.Font.Bold = true;

        //                for (int row = 8; row < 8 + maxCourse * 2; row += 2)
        //                {
        //                    worksheet.Cell($"A{row}").Value = excelStatistics[(row - 8) / 2].Course;
        //                    worksheet.Range($"A{row}:A{row + 1}").Merge();

        //                    worksheet.Cell($"B{row}").Value = excelStatistics[(row - 8) / 2].StudentsCount;
        //                    worksheet.Range($"B{row}:B{row + 1}").Merge();

        //                    worksheet.Cell($"C{row}").Value = excelStatistics[(row - 8) / 2].PassedCount;
        //                    worksheet.Cell($"C{row + 1}").SetFormulaA1($"=C{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");

        //                    worksheet.Cell($"D{row}").Value = excelStatistics[(row - 8) / 2].PassedOnExcellent;
        //                    worksheet.Cell($"D{row + 1}").SetFormulaA1($"=D{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");

        //                    worksheet.Cell($"E{row}").Value = excelStatistics[(row - 8) / 2].PassedOnVeryWell;
        //                    worksheet.Cell($"E{row + 1}").SetFormulaA1($"=E{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");

        //                    worksheet.Cell($"F{row}").Value = excelStatistics[(row - 8) / 2].PassedOnGood;
        //                    worksheet.Cell($"F{row + 1}").SetFormulaA1($"=F{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");

        //                    worksheet.Cell($"G{row}").Value = excelStatistics[(row - 8) / 2].PassedOnOkey;
        //                    worksheet.Cell($"G{row + 1}").SetFormulaA1($"=G{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");

        //                    worksheet.Cell($"H{row}").Value = excelStatistics[(row - 8) / 2].DebtorsCount;
        //                    worksheet.Cell($"H{row + 1}").SetFormulaA1($"=H{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");

        //                    worksheet.Cell($"I{row}").Value = excelStatistics[(row - 8) / 2].DebtorsInOne;
        //                    worksheet.Cell($"I{row + 1}").SetFormulaA1($"=I{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");

        //                    worksheet.Cell($"J{row}").Value = excelStatistics[(row - 8) / 2].DebtorsInTwo;
        //                    worksheet.Cell($"J{row + 1}").SetFormulaA1($"=J{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");

        //                    worksheet.Cell($"K{row}").Value = excelStatistics[(row - 8) / 2].DebtorsInMoreThree;
        //                    worksheet.Cell($"K{row + 1}").SetFormulaA1($"=K{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");
        //                }

        //                worksheet.Cell($"A{8 + maxCourse * 2}").Value = "Всего";
        //                worksheet.Range($"A{8 + maxCourse * 2}:A{8 + maxCourse * 2 + 1}").Merge();

        //                worksheet.Cell($"B{8 + maxCourse * 2}").SetFormulaA1($"=SUM(B8:B{8 + maxCourse * 2 - 1})");
        //                worksheet.Range($"B{8 + maxCourse * 2}:B{8 + maxCourse * 2 + 1}").Merge();

        //                worksheet.Cell($"C{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("C", maxCourse)})");
        //                worksheet.Cell($"C{8 + maxCourse * 2 + 1}").SetFormulaA1($"=C{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

        //                worksheet.Cell($"D{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("D", maxCourse)})");
        //                worksheet.Cell($"D{8 + maxCourse * 2 + 1}").SetFormulaA1($"=D{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

        //                worksheet.Cell($"E{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("E", maxCourse)})");
        //                worksheet.Cell($"E{8 + maxCourse * 2 + 1}").SetFormulaA1($"=E{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

        //                worksheet.Cell($"F{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("F", maxCourse)})");
        //                worksheet.Cell($"F{8 + maxCourse * 2 + 1}").SetFormulaA1($"=F{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

        //                worksheet.Cell($"G{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("G", maxCourse)})");
        //                worksheet.Cell($"G{8 + maxCourse * 2 + 1}").SetFormulaA1($"=G{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

        //                worksheet.Cell($"H{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("H", maxCourse)})");
        //                worksheet.Cell($"H{8 + maxCourse * 2 + 1}").SetFormulaA1($"=H{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

        //                worksheet.Cell($"I{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("I", maxCourse)})");
        //                worksheet.Cell($"I{8 + maxCourse * 2 + 1}").SetFormulaA1($"=I{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

        //                worksheet.Cell($"J{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("J", maxCourse)})");
        //                worksheet.Cell($"J{8 + maxCourse * 2 + 1}").SetFormulaA1($"=J{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

        //                worksheet.Cell($"K{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("K", maxCourse)})");
        //                worksheet.Cell($"K{8 + maxCourse * 2 + 1}").SetFormulaA1($"=K{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

        //                var tableCells = worksheet.Range($"A5:K{8 + maxCourse * 2 + 1}");
        //                tableCells.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        //                tableCells.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //                tableCells.Style.Alignment.WrapText = true;

        //                var sheetCells = worksheet.CellsUsed();
        //                sheetCells.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //                sheetCells.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        //                using (var stream = new MemoryStream())
        //                {
        //                    workbook.SaveAs(stream);
        //                    var bytes = stream.ToArray();
        //                    var type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //                    return File(bytes, type, $"({DateTime.Now.ToShortDateString()}) Успеваемость по направлению {profile.ShortName} ({formOfTraining.Name} форма обучения, {model.Term.ToLower()}).xlsx");
        //                }
        //            }
        //        }

        //        return RedirectToAction("StatisticExport");
        //    }

        //    catch (Exception)
        //    {
        //        return RedirectToAction("StatisticExport");
        //    }
        //}
        #endregion

        private IQueryable<StatementStudent> GetStatements(ProfileStatisticViewModel model)
        {
            // Получение формы обучения
            var formOfTraining = db.FormOfTrainings.FirstOrDefault(FormOfTraining => FormOfTraining.Id == model.FormOfTrainingId);

            // Получение программы обучения
            var profile = db.Profiles.FirstOrDefault(Profile => Profile.Id == model.ProfileId);

            // Выгрузка отсортированных по полям оценок
            var statementsStudents = from StatementStudent in db.StatementStudents
                                     join Statement in db.Statements on StatementStudent.StatementId equals Statement.Id
                                     join Group in db.Groups on Statement.GroupId equals Group.Id
                                     where (Statement.ProfileId.Value == profile.Id) &&
                                     (Statement.CurrentYear == model.Year) &&
                                     (Statement.TypeControl == "Экзамен") &&
                                     (Group.FormOfTrainingId == formOfTraining.Id)
                                     select StatementStudent;

            if (model.Term == "Зимняя сессия")
            {
                statementsStudents = statementsStudents.Include("Statement")
                    .Where(StatementStudent => ((StatementStudent.Date.Value <= model.Date) ||
                    (StatementStudent.Date == null)) &&
                    (StatementStudent.Statement.Semester % 2 == 1));
            }
            else if (model.Term == "Летняя сессия")
            {
                statementsStudents = statementsStudents.Include("Statement")
                    .Where(StatementStudent => ((StatementStudent.Date.Value <= model.Date) ||
                    (StatementStudent.Date == null)) &&
                    (StatementStudent.Statement.Semester % 2 == 0));
            }

            return statementsStudents;
        }

        private IQueryable<StatementStudent> GetStatements(FacultyStatisticViewModel model, Profile profile)
        {
            // Получение формы обучения
            var formOfTraining = db.FormOfTrainings.FirstOrDefault(FormOfTraining => FormOfTraining.Id == model.FormOfTrainingId);

            // Выгрузка отсортированных по полям оценок
            var statementsStudents = from StatementStudent in db.StatementStudents
                                     join Statement in db.Statements on StatementStudent.StatementId equals Statement.Id
                                     join Group in db.Groups on Statement.GroupId equals Group.Id
                                     where (Statement.ProfileId.Value == profile.Id) &&
                                     (Statement.CurrentYear == model.Year) &&
                                     (Statement.TypeControl == "Экзамен") &&
                                     (Group.FormOfTrainingId == formOfTraining.Id)
                                     select StatementStudent;

            if (model.Term == "Зимняя сессия")
            {
                statementsStudents = statementsStudents.Include("Statement")
                    .Where(StatementStudent => ((StatementStudent.Date.Value <= model.Date) ||
                    (StatementStudent.Date == null)) &&
                    (StatementStudent.Statement.Semester % 2 == 1));
            }
            else if (model.Term == "Летняя сессия")
            {
                statementsStudents = statementsStudents.Include("Statement")
                    .Where(StatementStudent => ((StatementStudent.Date.Value <= model.Date) ||
                    (StatementStudent.Date == null)) &&
                    (StatementStudent.Statement.Semester % 2 == 0));
            }

            return statementsStudents;
        }

        private IQueryable<StatementStudent> GetStatements(BranchStatisticViewModel model, Profile profile)
        {
            // Получение формы обучения
            var formOfTraining = db.FormOfTrainings.FirstOrDefault(FormOfTraining => FormOfTraining.Id == model.FormOfTrainingId);

            // Выгрузка отсортированных по полям оценок
            var statementsStudents = from StatementStudent in db.StatementStudents
                                     join Statement in db.Statements on StatementStudent.StatementId equals Statement.Id
                                     join Group in db.Groups on Statement.GroupId equals Group.Id
                                     where (Statement.ProfileId.Value == profile.Id) &&
                                     (Statement.CurrentYear == model.Year) &&
                                     (Statement.TypeControl == "Экзамен") &&
                                     (Group.FormOfTrainingId == formOfTraining.Id)
                                     select StatementStudent;

            if (model.Term == "Зимняя сессия")
            {
                statementsStudents = statementsStudents.Include("Statement")
                    .Where(StatementStudent => ((StatementStudent.Date.Value <= model.Date) ||
                    (StatementStudent.Date == null)) &&
                    (StatementStudent.Statement.Semester % 2 == 1));
            }
            else if (model.Term == "Летняя сессия")
            {
                statementsStudents = statementsStudents.Include("Statement")
                    .Where(StatementStudent => ((StatementStudent.Date.Value <= model.Date) ||
                    (StatementStudent.Date == null)) &&
                    (StatementStudent.Statement.Semester % 2 == 0));
            }

            return statementsStudents;
        }

        private List<ExcelStatisticViewModel> GetStatistic(IQueryable<StatementStudent> statementsStudents)
        {
            var excelStatistics = new List<ExcelStatisticViewModel>();

            // Высчитывание номера последнего курса
            var maxCourse = statementsStudents.Select(StatementStudent => StatementStudent.Statement).Max(Statement => Statement.Course);

            // Расчёт статистики
            for (int course = 1; course <= maxCourse; course++)
            {
                var studentsGrades = statementsStudents.Where(StatementStudent => StatementStudent.Statement.Course == course)
                    .GroupBy(StatementStudent => StatementStudent.StudentStatementId)
                    .ToList();

                var studentsCount = studentsGrades.Count();

                var excelStatisticViewModel = new ExcelStatisticViewModel()
                {
                    Course = course,
                    StudentsCount = studentsCount
                };

                foreach (var studentGrades in studentsGrades)
                {
                    var gradesCount = studentGrades.Count();

                    int excellentGrades = 0,
                        goodGrades = 0,
                        okeyGrades = 0,
                        badGrades = 0;

                    foreach (var grade in studentGrades)
                    {
                        if ((grade.GradeByNumber == 2) || (grade.Grade == "Не явился"))
                        {
                            badGrades++;
                        }
                        else if (grade.GradeByNumber == 5)
                        {
                            excellentGrades++;
                        }
                        else if (grade.GradeByNumber == 4)
                        {
                            goodGrades++;
                        }
                        else if (grade.GradeByNumber == 3)
                        {
                            okeyGrades++;
                        }
                    }

                    if (badGrades == 0)
                    {
                        excelStatisticViewModel.PassedCount++;

                        if (excellentGrades == gradesCount)
                        {
                            excelStatisticViewModel.PassedOnExcellent++;
                        }
                        else if (goodGrades == gradesCount)
                        {
                            excelStatisticViewModel.PassedOnGood++;
                        }
                        else if (okeyGrades != 0)
                        {
                            excelStatisticViewModel.PassedOnOkey++;
                        }
                        else
                        {
                            excelStatisticViewModel.PassedOnVeryWell++;
                        }
                    }
                    else
                    {
                        excelStatisticViewModel.DebtorsCount++;

                        if (badGrades == 1)
                        {
                            excelStatisticViewModel.DebtorsInOne++;
                        }
                        else if (badGrades == 2)
                        {
                            excelStatisticViewModel.DebtorsInTwo++;
                        }
                        else if (badGrades >= 3)
                        {
                            excelStatisticViewModel.DebtorsInMoreThree++;
                        }
                    }
                }

                excelStatistics.Add(excelStatisticViewModel);
            }

            ExcelStatisticViewModel.AllStudentsCount = excelStatistics.Sum(Statistic => Statistic.StudentsCount);
            ExcelStatisticViewModel.AllPassedCount = excelStatistics.Sum(Statistic => Statistic.PassedCount);
            ExcelStatisticViewModel.AllPassedOnExcellent = excelStatistics.Sum(Statistic => Statistic.PassedOnExcellent);
            ExcelStatisticViewModel.AllPassedOnVeryWell = excelStatistics.Sum(Statistic => Statistic.PassedOnVeryWell);
            ExcelStatisticViewModel.AllPassedOnGood = excelStatistics.Sum(Statistic => Statistic.PassedOnGood);
            ExcelStatisticViewModel.AllPassedOnOkey = excelStatistics.Sum(Statistic => Statistic.PassedOnOkey);
            ExcelStatisticViewModel.AllDebtorsCount = excelStatistics.Sum(Statistic => Statistic.DebtorsCount);
            ExcelStatisticViewModel.AllDebtorsInOne = excelStatistics.Sum(Statistic => Statistic.DebtorsInOne);
            ExcelStatisticViewModel.AllDebtorsInTwo = excelStatistics.Sum(Statistic => Statistic.DebtorsInTwo);
            ExcelStatisticViewModel.AllDebtorsInMoreThree = excelStatistics.Sum(Statistic => Statistic.DebtorsInMoreThree);

            return excelStatistics;
        }

        private Workbook GetXLBranchCharts(BranchStatisticViewModel model, List<ChartFacultyStatisticViewModel> statistics)
        {
            // Получение формы обучения
            var formOfTraining = db.FormOfTrainings.FirstOrDefault(FormOfTraining => FormOfTraining.Id == model.FormOfTrainingId);

            // Заполнение переменных для вывода в Excel
            var termText = null as string;

            if (model.Term == "Зимняя сессия")
            {
                termText = "зимней";
            }
            else if (model.Term == "Летняя сессия")
            {
                termText = "летней";
            }
            else
            {
                termText = "всех";
            }

            var years = model.Year.Split('-');
            var yearFirst = years[0];
            var yearSecond = years[1];

            var workbook = new Workbook();
            workbook.Worksheets.Clear();
            var worksheet = workbook.Worksheets.Add("Филиал СФ БашГУ");

            worksheet.Range["D1:K1"].Merge();
            worksheet.Range["D1:K1"].Value = "Стерлитамакский филиал Башкирского государственного универсистета";

            worksheet.Range["D2:K2"].Merge();
            worksheet.Range["D2:K2"].Value = $"{formOfTraining.Name.ToLower()} форма обучения";

            worksheet.SetColumnWidth(4, 12.71);
            worksheet.SetColumnWidth(5, 12.57);
            worksheet.SetColumnWidth(6, 17.71);
            worksheet.SetColumnWidth(7, 12.00);
            worksheet.SetColumnWidth(11, 14.57);
            worksheet.SetRowHeight(1, 41.25);
            worksheet.SetRowHeight(5, 25.50);

            worksheet.Range["D3"].Value = "Результаты";

            worksheet.Range["E3"].Style.Font.IsBold = true;
            worksheet.Range["E3"].Borders[BordersLineType.EdgeBottom].LineStyle = LineStyleType.Medium;
            worksheet.Range["E3"].Value = termText;

            if (termText == "всех")
            {
                worksheet.Range["F3"].Value = "экзаменационных";
                worksheet.Range["G3"].Value = "сессий";
            }
            else
            {
                worksheet.Range["F3"].Value = "экзаменационной";
                worksheet.Range["G3"].Value = "сессии";
            }

            worksheet.Range["H3"].Style.Font.IsBold = true;
            worksheet.Range["H3"].Borders[BordersLineType.EdgeBottom].LineStyle = LineStyleType.Medium;
            worksheet.Range["H3"].Value = yearFirst;

            worksheet.Range["I3"].Value = "/";

            worksheet.Range["J3"].Style.Font.IsBold = true;
            worksheet.Range["J3"].Borders[BordersLineType.EdgeBottom].LineStyle = LineStyleType.Medium;
            worksheet.Range["J3"].Value = yearSecond;

            worksheet.Range["K3"].Value = "учебного года";

            worksheet.Range["D1:K2"].Style.Font.IsBold = true;

            worksheet.Range["A5"].Value = "Факультет";
            worksheet.Range["A5"].Style.Font.IsBold = true;
            worksheet.Range["A5:B5"].Merge();

            worksheet.Range["C5"].Value = "Успеваемость, %";
            worksheet.Range["C5"].Style.Font.IsBold = true;
            worksheet.Range["C5:D5"].Merge();

            worksheet.Range["E5"].Value = "Качество образования, %";
            worksheet.Range["E5"].Style.Font.IsBold = true;
            worksheet.Range["E5:F5"].Merge();

            worksheet.Range["G5"].Value = "Отличники, %";
            worksheet.Range["G5"].Style.Font.IsBold = true;
            worksheet.Range["G5:H5"].Merge();

            for (int i = 6; i < statistics.Count + 6; i++)
            {
                worksheet.Range[i, 1].Value = statistics[i - 6].FacultyShortName;
                worksheet.Range[i, 1, i, 2].Merge();

                worksheet.Range[i, 3].Value = (statistics[i - 6].PerfomancePercent / 100.00).ToString();
                worksheet.Range[i, 3].Style.NumberFormat = "0.00%";
                worksheet.Range[i, 3, i, 4].Merge();

                worksheet.Range[i, 5].Value = (statistics[i - 6].QualityPercent / 100.00).ToString();
                worksheet.Range[i, 5].Style.NumberFormat = "0.00%";
                worksheet.Range[i, 5, i, 6].Merge();

                worksheet.Range[i, 7].Value = (statistics[i - 6].ExcellentStudentsPercent / 100.00).ToString();
                worksheet.Range[i, 7].Style.NumberFormat = "0.00%";
                worksheet.Range[i, 7, i, 8].Merge();

            }

            worksheet.Range[$"A{statistics.Count + 6}"].Value = "Средние значения";
            worksheet.Range[$"A{statistics.Count + 6}"].Style.Font.IsBold = true;
            worksheet.Range[$"A{statistics.Count + 6}:B{statistics.Count + 6}"].Merge();

            var averagePerfomancePercent = Math.Round(statistics.Sum(Statistic => Statistic.Perfomance) / statistics.Sum(Statistic => Statistic.AllStudentsCount), 5);
            var averageQualityPercent = Math.Round(statistics.Sum(Statistic => Statistic.Quality) / statistics.Sum(Statistic => Statistic.AllStudentsCount), 5);
            var averageExcellentStudentPercent = Math.Round(statistics.Sum(Statistic => Statistic.ExcellentStudents) / statistics.Sum(Statistic => Statistic.AllStudentsCount), 5);

            worksheet.Range[$"C{statistics.Count + 6}"].Value = averagePerfomancePercent.ToString();
            worksheet.Range[$"C{statistics.Count + 6}"].Style.NumberFormat = "0.00%";
            worksheet.Range[$"C{statistics.Count + 6}:D{statistics.Count + 6}"].Merge();

            worksheet.Range[$"E{statistics.Count + 6}"].Value = averageQualityPercent.ToString();
            worksheet.Range[$"E{statistics.Count + 6}"].Style.NumberFormat = "0.00%";
            worksheet.Range[$"E{statistics.Count + 6}:F{statistics.Count + 6}"].Merge();

            worksheet.Range[$"G{statistics.Count + 6}"].Value = averageExcellentStudentPercent.ToString();
            worksheet.Range[$"G{statistics.Count + 6}"].Style.NumberFormat = "0.00%";
            worksheet.Range[$"G{statistics.Count + 6}:H{statistics.Count + 6}"].Merge();

            worksheet.Range[$"A5:H{statistics.Count + 6}"].BorderAround();
            worksheet.Range[$"A5:H{statistics.Count + 6}"].BorderInside();

            var perfomance = worksheet.Charts.Add(ExcelChartType.ColumnClustered);
            perfomance.DataRange = worksheet.Range[$"C6:C{statistics.Count + 5}"];
            perfomance.SeriesDataFromRange = false;

            perfomance.ChartTitle = "Успеваемость, %";
            perfomance.Legend.Delete();
            perfomance.Series.First().CategoryLabels = worksheet.Range[$"A6:A{statistics.Count + 5}"];
            perfomance.Series.First().DataPoints.DefaultDataPoint.DataLabels.HasValue = true;

            var quality = worksheet.Charts.Add(ExcelChartType.ColumnClustered);
            quality.DataRange = worksheet.Range[$"E6:E{statistics.Count + 5}"];
            quality.SeriesDataFromRange = false;

            quality.ChartTitle = "Качество образования, %";
            quality.Legend.Delete();
            quality.Series.First().CategoryLabels = worksheet.Range[$"A6:A{statistics.Count + 5}"];
            quality.Series.First().DataPoints.DefaultDataPoint.DataLabels.HasValue = true;

            var excellentStudents = worksheet.Charts.Add(ExcelChartType.ColumnClustered);
            excellentStudents.DataRange = worksheet.Range[$"G6:G{statistics.Count + 5}"];
            excellentStudents.SeriesDataFromRange = false;

            excellentStudents.ChartTitle = "Отличники, %";
            excellentStudents.Legend.Delete();
            excellentStudents.Series.First().CategoryLabels = worksheet.Range[$"A6:A{statistics.Count + 5}"];
            excellentStudents.Series.First().DataPoints.DefaultDataPoint.DataLabels.HasValue = true;

            var average = worksheet.Charts.Add(ExcelChartType.ColumnClustered);
            average.SeriesDataFromRange = false;

            average.Series.Add(worksheet.Range[$"C5"].Value).Values = worksheet.Range[$"C{statistics.Count + 6}"];
            average.Series[worksheet.Range[$"C5"].Value].DataPoints.DefaultDataPoint.DataLabels.HasValue = true;

            average.Series.Add(worksheet.Range[$"E5"].Value).Values = worksheet.Range[$"E{statistics.Count + 6}"];
            average.Series[worksheet.Range[$"E5"].Value].DataPoints.DefaultDataPoint.DataLabels.HasValue = true;

            average.Series.Add(worksheet.Range[$"G5"].Value).Values = worksheet.Range[$"G{statistics.Count + 6}"];
            average.Series[worksheet.Range[$"G5"].Value].DataPoints.DefaultDataPoint.DataLabels.HasValue = true;

            average.ChartTitle = "Средние значения по факультету, %";
            average.Legend.Position = LegendPositionType.Bottom;
            average.PrimaryCategoryAxis.Visible = false;

            var perfomanceTop = statistics.Count + 8;
            var perfomanceBottom = perfomanceTop + 21;
            perfomance.LeftColumn = 1;
            perfomance.TopRow = perfomanceTop;
            perfomance.RightColumn = 9;
            perfomance.BottomRow = perfomanceBottom;

            var qualityTop = perfomanceBottom + 1;
            var qualityBottom = qualityTop + 21;
            quality.LeftColumn = 1;
            quality.TopRow = qualityTop;
            quality.RightColumn = 9;
            quality.BottomRow = qualityBottom;

            var excellentStudentsTop = qualityBottom + 1;
            var excellentStudentsBottom = excellentStudentsTop + 21;
            excellentStudents.LeftColumn = 1;
            excellentStudents.TopRow = excellentStudentsTop;
            excellentStudents.RightColumn = 9;
            excellentStudents.BottomRow = excellentStudentsBottom;

            var averageTop = excellentStudentsBottom + 1;
            var averageBottom = averageTop + 21;
            average.LeftColumn = 1;
            average.TopRow = averageTop;
            average.RightColumn = 9;
            average.BottomRow = averageBottom;

            worksheet.CellList.ForEach(Action => Action.Style.HorizontalAlignment = HorizontalAlignType.Center);
            worksheet.CellList.ForEach(Action => Action.Style.VerticalAlignment = VerticalAlignType.Center);
            worksheet.CellList.ForEach(Action => Action.Style.WrapText = true);

            return workbook;
        }

        private Workbook GetXLFacultyCharts(FacultyStatisticViewModel model, List<ChartProfileStatisticViewModel> statistics)
        {
            // Получение факультета
            var faculty = db.Faculties.Include("Profiles").FirstOrDefault(Faculty => Faculty.Id == model.FacultyId);

            // Получение формы обучения
            var formOfTraining = db.FormOfTrainings.FirstOrDefault(FormOfTraining => FormOfTraining.Id == model.FormOfTrainingId);

            // Заполнение переменных для вывода в Excel
            var termText = null as string;

            if (model.Term == "Зимняя сессия")
            {
                termText = "зимней";
            }
            else if (model.Term == "Летняя сессия")
            {
                termText = "летней";
            }
            else
            {
                termText = "всех";
            }

            var years = model.Year.Split('-');
            var yearFirst = years[0];
            var yearSecond = years[1];

            var workbook = new Workbook();
            workbook.Worksheets.Clear();
            var worksheet = workbook.Worksheets.Add(faculty.AliasFaculty);

            worksheet.Range["D1:K1"].Merge();
            worksheet.Range["D1:K1"].Value = faculty.Name;

            worksheet.Range["B1:C1"].Merge();
            worksheet.Range["B1:C1"].Value = "Факультет";
            worksheet.Range["D2:K2"].Merge();
            worksheet.Range["D2:K2"].Value = $"{formOfTraining.Name.ToLower()} форма обучения";

            worksheet.SetColumnWidth(4, 12.71);
            worksheet.SetColumnWidth(5, 12.57);
            worksheet.SetColumnWidth(6, 17.71);
            worksheet.SetColumnWidth(7, 12.00);
            worksheet.SetColumnWidth(11, 14.57);
            worksheet.SetRowHeight(1, 41.25);
            worksheet.SetRowHeight(5, 25.50);

            worksheet.Range["D3"].Value = "Результаты";

            worksheet.Range["E3"].Style.Font.IsBold = true;
            worksheet.Range["E3"].Borders[BordersLineType.EdgeBottom].LineStyle = LineStyleType.Medium;
            worksheet.Range["E3"].Value = termText;

            if (termText == "всех")
            {
                worksheet.Range["F3"].Value = "экзаменационных";
                worksheet.Range["G3"].Value = "сессий";
            }
            else
            {
                worksheet.Range["F3"].Value = "экзаменационной";
                worksheet.Range["G3"].Value = "сессии";
            }

            worksheet.Range["H3"].Style.Font.IsBold = true;
            worksheet.Range["H3"].Borders[BordersLineType.EdgeBottom].LineStyle = LineStyleType.Medium;
            worksheet.Range["H3"].Value = yearFirst;

            worksheet.Range["I3"].Value = "/";

            worksheet.Range["J3"].Style.Font.IsBold = true;
            worksheet.Range["J3"].Borders[BordersLineType.EdgeBottom].LineStyle = LineStyleType.Medium;
            worksheet.Range["J3"].Value = yearSecond;

            worksheet.Range["K3"].Value = "учебного года";

            worksheet.Range["D1:K2"].Style.Font.IsBold = true;

            worksheet.Range["A5"].Value = "Направление (профиль)";
            worksheet.Range["A5"].Style.Font.IsBold = true;
            worksheet.Range["A5:B5"].Merge();

            worksheet.Range["C5"].Value = "Успеваемость, %";
            worksheet.Range["C5"].Style.Font.IsBold = true;
            worksheet.Range["C5:D5"].Merge();

            worksheet.Range["E5"].Value = "Качество образования, %";
            worksheet.Range["E5"].Style.Font.IsBold = true;
            worksheet.Range["E5:F5"].Merge();

            worksheet.Range["G5"].Value = "Отличники, %";
            worksheet.Range["G5"].Style.Font.IsBold = true;
            worksheet.Range["G5:H5"].Merge();

            for (int i = 6; i < statistics.Count + 6; i++)
            {
                worksheet.Range[i, 1].Value = statistics[i - 6].ProfileName;
                worksheet.Range[i, 1, i, 2].Merge();

                worksheet.Range[i, 3].Value = (statistics[i - 6].PerfomancePercent / 100.00).ToString();
                worksheet.Range[i, 3].Style.NumberFormat = "0.00%";
                worksheet.Range[i, 3, i, 4].Merge();

                worksheet.Range[i, 5].Value = (statistics[i - 6].QualityPercent / 100.00).ToString();
                worksheet.Range[i, 5].Style.NumberFormat = "0.00%";
                worksheet.Range[i, 5, i, 6].Merge();

                worksheet.Range[i, 7].Value = (statistics[i - 6].ExcellentStudentsPercent / 100.00).ToString();
                worksheet.Range[i, 7].Style.NumberFormat = "0.00%";
                worksheet.Range[i, 7, i, 8].Merge();

            }

            worksheet.Range[$"A{statistics.Count + 6}"].Value = "Средние значения";
            worksheet.Range[$"A{statistics.Count + 6}"].Style.Font.IsBold = true;
            worksheet.Range[$"A{statistics.Count + 6}:B{statistics.Count + 6}"].Merge();

            var averagePerfomancePercent = Math.Round(statistics.Sum(Statistic => Statistic.Perfomance) / statistics.Sum(Statistic => Statistic.AllStudentsCount), 5);
            var averageQualityPercent = Math.Round(statistics.Sum(Statistic => Statistic.Quality) / statistics.Sum(Statistic => Statistic.AllStudentsCount), 5);
            var averageExcellentStudentPercent = Math.Round(statistics.Sum(Statistic => Statistic.ExcellentStudents) / statistics.Sum(Statistic => Statistic.AllStudentsCount), 5);

            worksheet.Range[$"C{statistics.Count + 6}"].Value = averagePerfomancePercent.ToString();
            worksheet.Range[$"C{statistics.Count + 6}"].Style.NumberFormat = "0.00%";
            worksheet.Range[$"C{statistics.Count + 6}:D{statistics.Count + 6}"].Merge();
            
            worksheet.Range[$"E{statistics.Count + 6}"].Value = averageQualityPercent.ToString();
            worksheet.Range[$"E{statistics.Count + 6}"].Style.NumberFormat = "0.00%";
            worksheet.Range[$"E{statistics.Count + 6}:F{statistics.Count + 6}"].Merge();
            
            worksheet.Range[$"G{statistics.Count + 6}"].Value = averageExcellentStudentPercent.ToString();
            worksheet.Range[$"G{statistics.Count + 6}"].Style.NumberFormat = "0.00%";
            worksheet.Range[$"G{statistics.Count + 6}:H{statistics.Count + 6}"].Merge();
            
            worksheet.Range[$"A5:H{statistics.Count + 6}"].BorderAround();
            worksheet.Range[$"A5:H{statistics.Count + 6}"].BorderInside();

            var perfomance = worksheet.Charts.Add(ExcelChartType.ColumnClustered);
            perfomance.DataRange = worksheet.Range[$"C6:C{statistics.Count + 5}"];
            perfomance.SeriesDataFromRange = false;

            perfomance.ChartTitle = "Успеваемость, %";
            perfomance.Legend.Delete();
            perfomance.Series.First().CategoryLabels = worksheet.Range[$"A6:A{statistics.Count + 5}"];
            perfomance.Series.First().DataPoints.DefaultDataPoint.DataLabels.HasValue = true;

            var quality = worksheet.Charts.Add(ExcelChartType.ColumnClustered);
            quality.DataRange = worksheet.Range[$"E6:E{statistics.Count + 5}"];
            quality.SeriesDataFromRange = false;

            quality.ChartTitle = "Качество образования, %";
            quality.Legend.Delete();
            quality.Series.First().CategoryLabels = worksheet.Range[$"A6:A{statistics.Count + 5}"];
            quality.Series.First().DataPoints.DefaultDataPoint.DataLabels.HasValue = true;

            var excellentStudents = worksheet.Charts.Add(ExcelChartType.ColumnClustered);
            excellentStudents.DataRange = worksheet.Range[$"G6:G{statistics.Count + 5}"];
            excellentStudents.SeriesDataFromRange = false;

            excellentStudents.ChartTitle = "Отличники, %";
            excellentStudents.Legend.Delete();
            excellentStudents.Series.First().CategoryLabels = worksheet.Range[$"A6:A{statistics.Count + 5}"];
            excellentStudents.Series.First().DataPoints.DefaultDataPoint.DataLabels.HasValue = true;

            var average = worksheet.Charts.Add(ExcelChartType.ColumnClustered);
            average.SeriesDataFromRange = false;

            average.Series.Add(worksheet.Range[$"C5"].Value).Values = worksheet.Range[$"C{statistics.Count + 6}"];
            average.Series[worksheet.Range[$"C5"].Value].DataPoints.DefaultDataPoint.DataLabels.HasValue = true;

            average.Series.Add(worksheet.Range[$"E5"].Value).Values = worksheet.Range[$"E{statistics.Count + 6}"];
            average.Series[worksheet.Range[$"E5"].Value].DataPoints.DefaultDataPoint.DataLabels.HasValue = true;

            average.Series.Add(worksheet.Range[$"G5"].Value).Values = worksheet.Range[$"G{statistics.Count + 6}"];
            average.Series[worksheet.Range[$"G5"].Value].DataPoints.DefaultDataPoint.DataLabels.HasValue = true;

            average.ChartTitle = "Средние значения по факультету, %";
            average.Legend.Position = LegendPositionType.Bottom;
            average.PrimaryCategoryAxis.Visible = false;

            var perfomanceTop = statistics.Count + 8;
            var perfomanceBottom = perfomanceTop + 21;
            perfomance.LeftColumn = 1;
            perfomance.TopRow = perfomanceTop;
            perfomance.RightColumn = 9;
            perfomance.BottomRow = perfomanceBottom;

            var qualityTop = perfomanceBottom + 1;
            var qualityBottom = qualityTop + 21;
            quality.LeftColumn = 1;
            quality.TopRow = qualityTop;
            quality.RightColumn = 9;
            quality.BottomRow = qualityBottom;

            var excellentStudentsTop = qualityBottom + 1;
            var excellentStudentsBottom = excellentStudentsTop + 21;
            excellentStudents.LeftColumn = 1;
            excellentStudents.TopRow = excellentStudentsTop;
            excellentStudents.RightColumn = 9;
            excellentStudents.BottomRow = excellentStudentsBottom;

            var averageTop = excellentStudentsBottom + 1;
            var averageBottom = averageTop + 21;
            average.LeftColumn = 1;
            average.TopRow = averageTop;
            average.RightColumn = 9;
            average.BottomRow = averageBottom;

            worksheet.CellList.ForEach(Action => Action.Style.HorizontalAlignment = HorizontalAlignType.Center);
            worksheet.CellList.ForEach(Action => Action.Style.VerticalAlignment = VerticalAlignType.Center);
            worksheet.CellList.ForEach(Action => Action.Style.WrapText = true);

            return workbook;
        }

        private XLWorkbook GetXLWorkbook(int maxCourse, ProfileStatisticViewModel model, List<ExcelStatisticViewModel> statistics)
        {
            // Получение формы обучения
            var formOfTraining = db.FormOfTrainings.FirstOrDefault(FormOfTraining => FormOfTraining.Id == model.FormOfTrainingId);

            // Получение программы обучения
            var profile = db.Profiles.FirstOrDefault(Profile => Profile.Id == model.ProfileId);

            // Заполнение переменных для вывода в Excel
            var termText = null as string;

            if (model.Term == "Зимняя сессия")
            {
                termText = "зимней";
            }
            else if (model.Term == "Летняя сессия")
            {
                termText = "летней";
            }
            else
            {
                termText = "всех";
            }

            var years = model.Year.Split('-');
            var yearFirst = years[0];
            var yearSecond = years[1];

            // Формирование Excel файла
            var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet(profile.ShortName);

            var profileCells = worksheet.Range("D1:K1").Merge();
            profileCells.Value = $"{profile.Code1}. {profile.Name}";
            profileCells.Style.Alignment.WrapText = true;

            worksheet.Range("B1:C1").Merge().Value = "Направление";
            worksheet.Range("D2:K2").Merge().Value = $"{formOfTraining.Name.ToLower()} форма обучения";

            worksheet.Column("D").Width = 12.71;
            worksheet.Column("E").Width = 12.57;
            worksheet.Column("F").Width = 17.71;
            worksheet.Column("G").Width = 12.00;
            worksheet.Column("K").Width = 14.57;
            worksheet.Row(1).Height = 41.25;
            worksheet.Row(5).Height = 36.75;

            worksheet.Cell("D3").Value = "Результаты";

            var termCell = worksheet.Cell("E3");
            termCell.Style.Font.Bold = true;
            termCell.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            termCell.Value = termText;

            if (termText == "всех")
            {
                worksheet.Cell("F3").Value = "экзаменационных";
                worksheet.Cell("G3").Value = "сессий";
            }
            else
            {
                worksheet.Cell("F3").Value = "экзаменационной";
                worksheet.Cell("G3").Value = "сессии";
            }

            var yearFirstCell = worksheet.Cell("H3");
            yearFirstCell.Style.Font.Bold = true;
            yearFirstCell.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            yearFirstCell.Value = yearFirst;

            worksheet.Cell("I3").Value = "/";

            var yearSecondCell = worksheet.Cell("J3");
            yearSecondCell.Style.Font.Bold = true;
            yearSecondCell.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            yearSecondCell.Value = yearSecond;

            worksheet.Cell("K3").Value = "учебного года";

            worksheet.Range("D1:K2").Style.Font.Bold = true;

            worksheet.Range("A5:B5").Merge().Value = "Количество студентов";
            worksheet.Range("C5:G5").Merge().Value = "Сдали все экзамены";
            worksheet.Range("H5:K5").Merge().Value = "Имеют задолженности";
            worksheet.Range("A5:K5").Style.Fill.BackgroundColor = XLColor.DeepPeach;

            worksheet.Cell("A6").Value = "Курс";
            worksheet.Cell("B6").Value = "По списку, чел.";
            worksheet.Cell("C6").Value = "Всего, чел., %";
            worksheet.Cell("D6").Value = "На отл., чел., %";
            worksheet.Cell("E6").Value = "На хор. и отл., чел., %";
            worksheet.Cell("F6").Value = "На хор., чел., %";
            worksheet.Cell("G6").Value = "Имеют удовлетв., чел., %";
            worksheet.Cell("H6").Value = "Всего, чел., %";
            worksheet.Cell("I6").Value = "По 1 предм., чел., %";
            worksheet.Cell("J6").Value = "По 2 предм., чел., %";
            worksheet.Cell("K6").Value = "По 3 и более предм., чел., %";

            worksheet.Cell("A7").Value = "1";
            worksheet.Cell("B7").Value = "2";
            worksheet.Cell("C7").Value = "3";
            worksheet.Cell("D7").Value = "4";
            worksheet.Cell("E7").Value = "5";
            worksheet.Cell("F7").Value = "6";
            worksheet.Cell("G7").Value = "7";
            worksheet.Cell("H7").Value = "8";
            worksheet.Cell("I7").Value = "9";
            worksheet.Cell("J7").Value = "10";
            worksheet.Cell("K7").Value = "11";
            worksheet.Range("A6:K7").Style.Fill.BackgroundColor = XLColor.Peach;

            worksheet.Range("A5:K7").Style.Font.Bold = true;

            for (int row = 8; row < 8 + maxCourse * 2; row += 2)
            {
                worksheet.Cell($"A{row}").Value = statistics[(row - 8) / 2].Course;
                worksheet.Range($"A{row}:A{row + 1}").Merge();

                worksheet.Cell($"B{row}").Value = statistics[(row - 8) / 2].StudentsCount;
                worksheet.Range($"B{row}:B{row + 1}").Merge();

                worksheet.Cell($"C{row}").Value = statistics[(row - 8) / 2].PassedCount;
                worksheet.Cell($"C{row + 1}").SetFormulaA1($"=C{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");

                worksheet.Cell($"D{row}").Value = statistics[(row - 8) / 2].PassedOnExcellent;
                worksheet.Cell($"D{row + 1}").SetFormulaA1($"=D{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");

                worksheet.Cell($"E{row}").Value = statistics[(row - 8) / 2].PassedOnVeryWell;
                worksheet.Cell($"E{row + 1}").SetFormulaA1($"=E{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");

                worksheet.Cell($"F{row}").Value = statistics[(row - 8) / 2].PassedOnGood;
                worksheet.Cell($"F{row + 1}").SetFormulaA1($"=F{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");

                worksheet.Cell($"G{row}").Value = statistics[(row - 8) / 2].PassedOnOkey;
                worksheet.Cell($"G{row + 1}").SetFormulaA1($"=G{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");

                worksheet.Cell($"H{row}").Value = statistics[(row - 8) / 2].DebtorsCount;
                worksheet.Cell($"H{row + 1}").SetFormulaA1($"=H{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");

                worksheet.Cell($"I{row}").Value = statistics[(row - 8) / 2].DebtorsInOne;
                worksheet.Cell($"I{row + 1}").SetFormulaA1($"=I{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");

                worksheet.Cell($"J{row}").Value = statistics[(row - 8) / 2].DebtorsInTwo;
                worksheet.Cell($"J{row + 1}").SetFormulaA1($"=J{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");

                worksheet.Cell($"K{row}").Value = statistics[(row - 8) / 2].DebtorsInMoreThree;
                worksheet.Cell($"K{row + 1}").SetFormulaA1($"=K{row}/B{row}").Style.NumberFormat.SetFormat("0.00%");
            }

            worksheet.Cell($"A{8 + maxCourse * 2}").Value = "Всего";
            worksheet.Range($"A{8 + maxCourse * 2}:A{8 + maxCourse * 2 + 1}").Merge();

            worksheet.Cell($"B{8 + maxCourse * 2}").SetFormulaA1($"=SUM(B8:B{8 + maxCourse * 2 - 1})");
            worksheet.Range($"B{8 + maxCourse * 2}:B{8 + maxCourse * 2 + 1}").Merge();

            worksheet.Cell($"C{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("C", maxCourse)})");
            worksheet.Cell($"C{8 + maxCourse * 2 + 1}").SetFormulaA1($"=C{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

            worksheet.Cell($"D{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("D", maxCourse)})");
            worksheet.Cell($"D{8 + maxCourse * 2 + 1}").SetFormulaA1($"=D{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

            worksheet.Cell($"E{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("E", maxCourse)})");
            worksheet.Cell($"E{8 + maxCourse * 2 + 1}").SetFormulaA1($"=E{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

            worksheet.Cell($"F{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("F", maxCourse)})");
            worksheet.Cell($"F{8 + maxCourse * 2 + 1}").SetFormulaA1($"=F{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

            worksheet.Cell($"G{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("G", maxCourse)})");
            worksheet.Cell($"G{8 + maxCourse * 2 + 1}").SetFormulaA1($"=G{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

            worksheet.Cell($"H{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("H", maxCourse)})");
            worksheet.Cell($"H{8 + maxCourse * 2 + 1}").SetFormulaA1($"=H{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

            worksheet.Cell($"I{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("I", maxCourse)})");
            worksheet.Cell($"I{8 + maxCourse * 2 + 1}").SetFormulaA1($"=I{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

            worksheet.Cell($"J{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("J", maxCourse)})");
            worksheet.Cell($"J{8 + maxCourse * 2 + 1}").SetFormulaA1($"=J{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

            worksheet.Cell($"K{8 + maxCourse * 2}").SetFormulaA1($"=SUM({GetRowsString("K", maxCourse)})");
            worksheet.Cell($"K{8 + maxCourse * 2 + 1}").SetFormulaA1($"=K{8 + maxCourse * 2}/B{8 + maxCourse * 2}").Style.NumberFormat.SetFormat("0.00%");

            var tableCells = worksheet.Range($"A5:K{8 + maxCourse * 2 + 1}");
            tableCells.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            tableCells.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            tableCells.Style.Alignment.WrapText = true;

            var sheetCells = worksheet.CellsUsed();
            sheetCells.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheetCells.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            return workbook;
        }

        private string GetRowsString(string column, int maxCourse)
        {
            var result = $"{column}8";

            for (int i = 10; i < 8 + maxCourse * 2; i += 2)
            {
                result += $",{column}{i}";
            }

            return result;
        }

        // GET: Reports
        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public ActionResult StatementsNullable()
        {
            //var statStud = new List<StatementStudent>();
            var statementsReport = new List<Statement>();

            //List<Statement> statements = new List<Statement>();

            //if (User.IsInRole("Administrators"))
            //    statements = db.Statements
            //    .Include("Faculty")                
            //    .OrderBy(a => a.Faculty.Name)
            //    .ThenBy(a => a.GroupName)
            //    .ToList();
            var statements = from stats in db.Statements
                             join fac in db.Faculties on stats.FacultyId equals fac.Id
                             join teachers in db.Users on stats.TeacherDisciplineId equals teachers.Id into outerTeacher
                             from teachers in outerTeacher.DefaultIfEmpty()
                             join teachers2 in db.Users on stats.TeacherDiscipline2Id equals teachers2.Id into outerTeacher2
                             from teachers2 in outerTeacher2.DefaultIfEmpty()
                             join teachers3 in db.Users on stats.TeacherDiscipline3Id equals teachers3.Id into outerTeacher3
                             from teachers3 in outerTeacher3.DefaultIfEmpty()
                             join teachers4 in db.Users on stats.TeacherDiscipline4Id equals teachers4.Id into outerTeacher4
                             from teachers4 in outerTeacher4.DefaultIfEmpty()
                             join teachers5 in db.Users on stats.TeacherDiscipline5Id equals teachers5.Id into outerTeacher5
                             from teachers5 in outerTeacher5.DefaultIfEmpty()
                             join teachers6 in db.Users on stats.TeacherDiscipline6Id equals teachers6.Id into outerTeacher6
                             from teachers6 in outerTeacher6.DefaultIfEmpty()
                             join teachers7 in db.Users on stats.TeacherDiscipline7Id equals teachers7.Id into outerTeacher7
                             from teachers7 in outerTeacher7.DefaultIfEmpty()
                             orderby fac.Name, stats.GroupName
                             select new StatementNullableViewModel
                             {
                                 Id = stats.Id,
                                 FacultyId = fac.Id,
                                 FacultyName = fac.Name,
                                 Discipline = stats.NameDiscipline,
                                 GroupName = stats.GroupName,
                                 Teacher = teachers.Lastname + " " + teachers.Firstname.Substring(0, 1) + "." + teachers.Middlename.Substring(0, 1) + ". ",
                                 Teacher2 = teachers2 != null ? teachers2.Lastname + " " + teachers2.Firstname.Substring(0, 1) + "." + teachers2.Middlename.Substring(0, 1) + "." : "",
                                 Teacher3 = teachers3 != null ? teachers3.Lastname + " " + teachers3.Firstname.Substring(0, 1) + "." + teachers3.Middlename.Substring(0, 1) + "." : "",
                                 Teacher4 = teachers4 != null ? teachers4.Lastname + " " + teachers4.Firstname.Substring(0, 1) + "." + teachers4.Middlename.Substring(0, 1) + "." : "",
                                 Teacher5 = teachers5 != null ? teachers5.Lastname + " " + teachers5.Firstname.Substring(0, 1) + "." + teachers5.Middlename.Substring(0, 1) + "." : "",
                                 Teacher6 = teachers6 != null ? teachers6.Lastname + " " + teachers6.Firstname.Substring(0, 1) + "." + teachers6.Middlename.Substring(0, 1) + "." : "",
                                 Teacher7 = teachers7 != null ? teachers7.Lastname + " " + teachers7.Firstname.Substring(0, 1) + "." + teachers7.Middlename.Substring(0, 1) + "." : "",
                                 TypeControl = stats.TypeControl,
                                 Course = stats.Course,
                                 Semester = stats.Semester,
                                 Date = stats.DateEnd,
                                 Years = stats.CurrentYear,
                                 ZET = stats.ZET,
                                 Hours = stats.Hours,
                                 CountStudents = db.StatementStudents.Where(a => a.StatementId == stats.Id).Count(),
                                 CountGrades = db.StatementStudents.Where(a => a.StatementId == stats.Id && a.Grade == null).Count()
                             };

            if (User.IsInRole("FacultiesManagers"))
            {
                var userId = User.Identity.GetUserId();
                var user = db.Users.Find(userId);

                statements = statements.Where(a => a.FacultyId == user.FacultyId);
            }

            statements = statements.Where(a => a.CountGrades == a.CountStudents);
            return View(statements.ToList());
        }


        [Authorize(Roles = "Administrators")]
        public ActionResult StatementsStatistic()
        {
            //определение количества ведомостей на факультетах
            var statistic = (from fac in db.Faculties
                             select new StatisticStatement
                             {
                                 FacultyId = fac.Id,
                                 FacultyName = fac.Name,
                                 CountStatementsAll = db.Statements.Where(a => a.FacultyId == fac.Id).Count(),
                                 FillStatements = db.StatementStudents.Where(a => a.Grade != null && a.Statement.FacultyId == fac.Id)
                                 .Select(a => a.StatementId).Distinct().Count()
                             }).ToList();

            return View(statistic);
        }


        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public ActionResult CoursesNullable()
        {
            var courseStud = new List<CourseWorkStudent>();
            var coursesReport = new List<Course>();

            List<Course> courses = new List<Course>();

            if (User.IsInRole("Administrators"))
                courses = db.Courses
                .Include("Faculty")
                .Include("Department")
                .Include("Group")
                .OrderBy(a => a.Faculty.Name)
                .ThenBy(a => a.Group.Name)
                .Where(a => a.CurrentYear != "2018-2019" || (a.CurrentYear == "2018-2019" && a.Semester % 2 != 0)).ToList();

            if (User.IsInRole("FacultiesManagers"))
            {
                var userId = User.Identity.GetUserId();
                var user = db.Users.Find(userId);

                var facultyIdUser =
                courses = db.Courses
                  .Include("Faculty")
                  .Include("Department")
                  .Include("Group")
                  .OrderBy(a => a.Faculty.Name)
                  .ThenBy(a => a.Group.Name)
                  .Where(a => a.FacultyId == user.FacultyId && (a.CurrentYear != "2018-2019" || (a.CurrentYear == "2018-2019" && a.Semester % 2 != 0))).ToList();
            }

            if (courses.Count > 0)
            {
                foreach (var s in courses)
                {
                    bool isNotNull = false;
                    courseStud = db.CourseWorkStudents.Where(a => a.CourseId == s.Id).ToList();
                    foreach (var ss in courseStud)
                    {
                        if (ss.Path != null)
                        {
                            isNotNull = true;
                            break;
                        }
                    }

                    if (!isNotNull)
                    {
                        coursesReport.Add(s);
                    }
                }
            }
            return View(coursesReport);
        }

        [Authorize(Roles = "Administrators")]
        public ActionResult CoursesStatistic()
        {
            //определение количества ведомостей на факультетах
            var statistic = (from fac in db.Faculties
                             select new StatisticStatement
                             {
                                 FacultyId = fac.Id,
                                 FacultyName = fac.Name,
                                 CountStatementsAll = db.Courses.Where(a => a.FacultyId == fac.Id).Count(),
                                 FillStatements = db.CourseWorkStudents.Where(a => a.Path != null && a.Course.FacultyId == fac.Id)
                                 .Select(a => a.CourseId).Distinct().Count()
                             }).ToList();

            return View(statistic);
        }

        private List<ReportProjectsPercent> GetListProjects()
        {
            IQueryable<ReportProjectsPercent> courses = null;

            if (User.IsInRole("Administrators"))
                courses = from cs in db.Courses
                          join f in db.Faculties on cs.FacultyId equals f.Id into ff
                          from f in ff.DefaultIfEmpty()
                          join d in db.Departments on cs.DepartmentId equals d.Id into dd
                          from d in dd.DefaultIfEmpty()
                          join g in db.Groups on cs.GroupId equals g.Id
                          orderby cs.CurrentYear, cs.GroupName
                          select new ReportProjectsPercent
                          {
                              Id = cs.Id,
                              Type = cs.Type,
                              Faculty = f.Name,
                              Department = d.ShortName,
                              Group = cs.GroupName,
                              Course = cs.Cours,
                              Semester = cs.Semester,
                              Year = cs.CurrentYear,
                              CountStudents = db.CourseWorkStudents.Where(a => a.CourseId == cs.Id).Count(),
                              CountProject = db.CourseWorkStudents.Where(a => a.CourseId == cs.Id && a.Path != null).Count(),
                              FillPercent = (db.CourseWorkStudents.Where(a => a.CourseId == cs.Id).Count() != 0) ? db.CourseWorkStudents.Where(a => a.CourseId == cs.Id && a.Path != null).Count()
                              * 100 / db.CourseWorkStudents.Where(a => a.CourseId == cs.Id).Count() : 0,
                          };

            if (User.IsInRole("FacultiesManagers"))
            {
                var userId = User.Identity.GetUserId();
                var user = db.Users.Find(userId);

                courses = from cs in db.Courses
                          join f in db.Faculties on cs.FacultyId equals f.Id into ff
                          from f in ff.DefaultIfEmpty()
                          join d in db.Departments on cs.DepartmentId equals d.Id into dd
                          from d in dd.DefaultIfEmpty()
                          join g in db.Groups on cs.GroupId equals g.Id
                          orderby cs.CurrentYear, cs.GroupName
                          where (cs.FacultyId == user.FacultyId)
                          select new ReportProjectsPercent
                          {
                              Id = cs.Id,
                              Type = cs.Type,
                              Faculty = f.Name,
                              Department = d.ShortName,
                              Group = cs.GroupName,
                              Course = cs.Cours,
                              Semester = cs.Semester,
                              Year = cs.CurrentYear,
                              CountStudents = db.CourseWorkStudents.Where(a => a.CourseId == cs.Id).Count(),
                              CountProject = db.CourseWorkStudents.Where(a => a.CourseId == cs.Id && a.Path != null).Count(),
                              FillPercent = (db.CourseWorkStudents.Where(a => a.CourseId == cs.Id).Count() != 0) ? db.CourseWorkStudents.Where(a => a.CourseId == cs.Id && a.Path != null).Count()
                              * 100 / db.CourseWorkStudents.Where(a => a.CourseId == cs.Id).Count() : 0,
                          };
            }

            return courses.OrderBy(a => a.Faculty).ThenBy(a => a.FillPercent).ToList();
        }

        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public ActionResult CoursesFillPercent()
        {
            return View(GetListProjects());
        }

        [HttpGet]
        public FileResult Export()
        {
            //экспорт списка курсовых
            DataTable dtProjects = new DataTable("Процент публикации работ");
            dtProjects.Columns.AddRange(new DataColumn[10] {
                                            new DataColumn("Вид работы"),
                                            new DataColumn("Факультет"),
                                            new DataColumn("Кафедра"),
                                            new DataColumn("Группа"),
                                            new DataColumn("Курс"),
                                            new DataColumn("Семестр"),
                                            new DataColumn("Год"),
                                            new DataColumn("Количество студентов"),
                                            new DataColumn("Количество работ"),
                                            new DataColumn("Процент заполнения")});

            var courses = GetListProjects();

            foreach (var c in courses)
            {
                dtProjects.Rows.Add(c.Type, c.Faculty, c.Department, c.Group, c.Course,
                    c.Semester, c.Year, c.CountStudents, c.CountProject, c.FillPercent);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dtProjects);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FillProjects.xlsx");
                }
            }
        }


        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public async Task<ActionResult> UpdateGroupNameCourses()
        {
            var courses = db.Courses.Include(a => a.Group).Include(a => a.Faculty).ToList();
            foreach (var course in courses)
            {
                //1. получение курса из названия группы
                int currentCourse = Convert.ToInt32(course.Group.Name.Substring(course.Group.Name.Length - 2, 1));
                // 2.сравниваем совпадают ли введенный курс и курс из названия группы
                if (currentCourse == course.Cours)
                {
                    //course.CurrentYear = currentYear;
                    course.GroupName = course.Group.Name;
                }
                else
                {
                    //если не совпали курсы, то определяем правильный учебный год и меняем название группы и год
                    int delta = currentCourse - course.Cours;
                    //int y = Convert.ToInt32(currentYear.Substring(2, 2)) - delta;
                    //int y1 = Convert.ToInt32(currentYear.Substring(7)) - delta;
                    //course.CurrentYear = String.Format($"20{y}-20{y1}");
                    course.GroupName = course.Group.Name.Substring(0, course.Group.Name.Length - 2) + course.Cours
                                        + course.Group.Name.Substring(course.Group.Name.Length - 1, 1);
                }
                db.Entry(course).State = EntityState.Modified;
            }
            var i = await db.SaveChangesAsync();
            return View();
        }


        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public ActionResult ExportGrades()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(a => a.Id == userId);

            // Получаем список учебных годов
            var years = db.Statements
                .OrderByDescending(Year => Year.CurrentYear)
                .Select(Year => new { Value = Year.CurrentYear, Text = Year.CurrentYear })
                .Distinct()
                .OrderBy(Year => Year.Text).ToList();
            var yearsView = new List<SelectListItem>();

            foreach (var item in years)
            {
                yearsView.Add(new SelectListItem()
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }
            //Добавление пункта Весь период
            yearsView.Insert(0, new SelectListItem()
            {
                Text = "Весь период обучения",
                Value = "Весь период обучения"
            });
            ViewBag.Years = new SelectList(yearsView, "Value", "Text");

            // Выделяем текущий учебный год в списке учебных годов
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear").Value;
            yearsView.FirstOrDefault(Year => Year.Value == currentYear).Selected = true;


            if (!User.IsInRole("Administrators"))
            {
                ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name", user.FacultyId);
                ViewBag.GroupId = new SelectList(db.Groups.Where(a => a.FacultyId == user.FacultyId && a.IsDeleted == false &&
                a.AcademicYear == currentYear).OrderBy(a => a.Name), "Id", "Name");
            }
            else
            {
                ViewBag.FacultyId = new SelectList(db.Faculties.Where(a => a.IsDeleted == false).OrderBy(a => a.Name), "Id", "Name");
                ViewBag.GroupId = new SelectList(db.Groups.Where(a => a.IsDeleted == false &&
                a.AcademicYear == currentYear).OrderBy(a => a.Name), "Id", "Name");
            }
            ViewBag.Courses = new SelectList(listCourses, "Value", "Text");
            ViewBag.Semester = new SelectList(listSessions, "Value", "Text");

            return View();
        }

        // POST: Grade/Create
        [HttpPost]
        [Authorize(Roles = "Administrators,FacultiesManagers")]
        public ActionResult ExportGrades(ReportGrades report)
        {
            if (ModelState.IsValid)
            {                
                //получаем параметры для перебора данных
                var userId = User.Identity.GetUserId();
                var user = db.Users.FirstOrDefault(a => a.Id == userId);
                var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
                                
                //выправляем факультет, чтобы не смотрели чужие ведомости
                if (!User.IsInRole("Administrators"))
                    if (report.FacultyId != user.FacultyId) report.FacultyId = (int)user.FacultyId;
                                
                //получаем список активных групп факультета, если группа не выбрана
                //и список дисциплин для каждой группы
                List<GroupsList> lstDisp;
                List<Group> lstGroups;
                Faculty faculty = new Faculty();
                if (report.GroupId <= 0 || report.GroupId == null)
                {
                    faculty = db.Faculties.FirstOrDefault(a => a.Id == report.FacultyId);
                    lstGroups = db.Groups.Where(a => a.FacultyId == report.FacultyId && a.IsDeleted == false &&
                        a.AcademicYear == currentYear.Value).OrderBy(a => a.Name).ToList();
                }
                else
                {
                    lstGroups = db.Groups.Where(a => a.Id == report.GroupId &&
                    a.AcademicYear == currentYear.Value).ToList();
                }

                using (XLWorkbook wb = new XLWorkbook())
                {
                    foreach (var group in lstGroups)
                    {
                        var dt = GetGradesStudents(group.Id, report.Years, report.Semester, out lstDisp);
                        if (dt.Rows.Count != 0)
                        {
                            var ws = wb.Worksheets.Add(group.Name);
                            int ind = 1;
                            int n = 1;

                            //создание заголовка таблицы
                            ws.Cell("A" + ind).Value = "Учебный год:";
                            ws.Cell("B" + ind++).Value = report.Years;

                            ws.Cell("A" + ind).Value = "Сессия:";
                            ws.Cell("B" + ind++).Value = report.Semester;
                            ws.Range("A1:A2").Style.Font.Bold = true;
                            ind++;

                            //создание шапки таблицы
                            int i = 0;
                            for (int c = 0; c < dt.Columns.Count; c++)
                            {
                                if (c == 0)
                                    ws.Cell(ind, c + 1).Value = "№";
                                else if (c == 1)
                                    ws.Cell(ind, c + 1).Value = "Фамилия";
                                else if (c == 2)
                                    ws.Cell(ind, c + 1).Value = "Имя";
                                else if (c == 3)
                                    ws.Cell(ind, c + 1).Value = "Отчество";
                                else if(c == 4)
                                    ws.Cell(ind, c + 1).Value = "Основание";
                                else
                                {
                                    int id = Convert.ToInt32(dt.Columns[c].ColumnName);
                                    ws.Cell(ind, c + 1).Value = lstDisp.FirstOrDefault(a => a.Id == id).Name;
                                }
                                ws.Cell(ind, c + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                ws.Cell(ind, c + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                ws.Cell(ind, c + 1).Style.Alignment.WrapText = true;
                                ws.Cell(ind, c + 1).Style.Font.Bold = true;
                            }

                            //заполнение таблицы
                            ind++;
                            for (int r = 0; r < dt.Rows.Count; r++)
                            {
                                for (int c = 0; c < dt.Columns.Count; c++)
                                {
                                    if (dt.Columns[c].ColumnName == "Id")
                                    {
                                        ws.Cell((ind + r), c + 1).Value = n + r;
                                        ws.Cell((ind + r), c + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    }
                                    else
                                    {
                                        ws.Cell((ind + r), c + 1).Value = dt.Rows[r][c];
                                    }
                                }

                            }
                        }
                    }
                    using (MemoryStream stream = new MemoryStream())
                    {
                        string fileName = "";
                        if (report.GroupId <= 0 || report.GroupId == null)
                            fileName = faculty.Name ?? "Факультет";
                        else
                            fileName = lstGroups[0].Name;

                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + ".xlsx");
                    }
                }                
            }
            return View(report);
        }



        public DataTable GetGradesStudents(int groupId, string years, string session, out List<GroupsList> lstDispciplines)
        {
            lstDispciplines = GetDisciplines(groupId, years, session);
            var lstStudentsGroup = GetStudentsGroup(groupId);
            DataTable dtJournal = new DataTable();
            if (lstDispciplines.Count != 0 && lstStudentsGroup.Count != 0)
            {
                string sqlCommand = GetSqlCommand(groupId, lstDispciplines);
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();
                    System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(sqlCommand, connection);
                    adapter.Fill(dtJournal);
                }

                return dtJournal;
            }
            else
            {
                return dtJournal;
            }
        }

        public string GetSqlCommand(int groupId, List<GroupsList> st)
        {
            StringBuilder sql = new StringBuilder("WITH Journals AS (" +
                         "SELECT TOP (100) PERCENT dbo.AspNetUsers.Id, dbo.AspNetUsers.Lastname, dbo.AspNetUsers.Firstname, dbo.AspNetUsers.Middlename, " +
                            "dbo.AspNetUsers.Bases, dbo.StatementStudents.StatementId, dbo.StatementStudents.Grade " +
                         "FROM dbo.StatementStudents INNER JOIN " +
                            "dbo.Statements ON dbo.StatementStudents.StatementId = dbo.Statements.Id INNER JOIN " +
                            "dbo.AspNetUsers ON dbo.StatementStudents.StudentStatementId = dbo.AspNetUsers.Id " +
                         "WHERE(dbo.Statements.GroupId = " + groupId + ") AND " +
                            "(dbo.StatementStudents.StatementId IN (");
            for (int n = 0; n < st.Count; n++)
            {
                sql.Append(st[n].Id.ToString() + ", ");
            }

            sql.Remove(sql.Length - 2, 2);
            sql.Append("))) SELECT * FROM Journals PIVOT(MAX(Journals.Grade) for Journals.StatementId in (");

            for (int n = 0; n < st.Count; n++)
            {
                sql.Append("[" + st[n].Id.ToString() + "], ");
            }
            sql.Remove(sql.Length - 2, 2);
            sql.Append(")) AS Journal ORDER BY Lastname, Firstname, Middlename");

            return sql.ToString();
        }

        public List<GroupsList> GetDisciplines(int groupId, string years, string session)
        {
            IQueryable<Statement> lstDisciplines;
            if (years == "1")
            {
                lstDisciplines = db.Statements.Where(a => a.GroupId == groupId);
            }
            else
            {
                lstDisciplines = db.Statements.Where(a => a.GroupId == groupId && a.CurrentYear == years);
            }
            
            if (years != "1")
            {
                if (session == "Зимняя сессия")
                {
                    lstDisciplines = lstDisciplines.Where(a => a.Semester % 2 == 1);
                }
                if (session == "Летняя сессия")
                    lstDisciplines = lstDisciplines.Where(a => a.Semester % 2 == 0);
            }

            lstDisciplines = lstDisciplines.OrderBy(a => a.Semester).ThenBy(a => a.TypeControl).ThenBy(a => a.NameDiscipline);
            var lst = lstDisciplines.Select(a => new GroupsList { Id = a.Id, Name = a.NameDiscipline + " (" + a.Semester + ")" }).ToList();

            return lst;
        }

        public Dictionary<string, string> GetStudentsGroup(int groupId)
        {
            var studentsGroup = db.Users.Where(a => a.GroupId == groupId && a.DateBlocked == null)
                    .OrderBy(a => a.Lastname).ThenBy(a => a.Firstname).ThenBy(a => a.Middlename);

            return studentsGroup.ToDictionary(a => a.Id, a => a.Lastname + " " + a.Firstname + " " + a.Middlename + " (" + a.Bases + ")");
        }

        public List<SelectListItem> listCourses = new List<SelectListItem>(new[] {
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
            }
        });

        public List<SelectListItem> listSessions = new List<SelectListItem>(new[] {
            new SelectListItem
            {
                Text = "Весь учебный год",
                Value = "Весь учебный год"
            },
            new SelectListItem
            {
                Text = "Зимняя сессия",
                Value = "Зимняя сессия"
            },
            new SelectListItem
            {
                Text = "Летняя сессия",
                Value = "Летняя сессия"
            }
        });

        public JsonResult GetGroups(int? id)
        {
            List<GroupsList> groupsList;
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
            if (id == null)
            {
                groupsList = db.Groups.Where(a => a.IsDeleted == false &&
                a.AcademicYear == currentYear.Value).OrderBy(a => a.Name).
                Select(a => new GroupsList { Id = a.Id, Name = a.Name }).ToList();
            }
            else
            {
                int facultyId = Convert.ToInt32(id);
                groupsList = db.Groups.Where(a => a.FacultyId == facultyId && a.IsDeleted == false &&
                a.AcademicYear == currentYear.Value).OrderBy(a => a.Name).
                Select(a => new GroupsList { Id = a.Id, Name = a.Name }).ToList();
            }
            groupsList.Insert(0, new GroupsList { Id = -1, Name = "--Выберите--" });
            return Json(new SelectList(groupsList, "Id", "Name"));
        }

        public JsonResult GetYears(int? id)
        {
            List<TutorsList> years;
            // Получаем список учебных годов
            if (id == null)
            {
                years = db.Statements
                  .OrderByDescending(Year => Year.CurrentYear)
                  .Select(Year => new TutorsList { Id = Year.CurrentYear, Name = Year.CurrentYear })
                  .Distinct().OrderBy(Year => Year.Name)
                  .ToList();
            }
            else
            {
                years = db.Statements
                    .Where(a => a.GroupId == id)
                    .Select(Year => new TutorsList { Id = Year.CurrentYear, Name = Year.CurrentYear })
                    .Distinct().OrderBy(Year => Year.Name)
                    .ToList();
            }

            years.Insert(0, new TutorsList { Id = "-1", Name = "--Выберите--" });
            years.Insert(1, new TutorsList { Id = "1", Name = "Весь период обучения" });
            return Json(new SelectList(years, "Id", "Name"));
        }
    }
}