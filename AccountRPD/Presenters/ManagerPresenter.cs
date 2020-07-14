using Account.DAL.Entities;
using AccountRPD.BL.Interfaces;
using AccountRPD.BL.Strategies;
using AccountRPD.Enums;
using AccountRPD.EventArguments;
using AccountRPD.Extensions;
using AccountRPD.Infrastucture;
using AccountRPD.Interfaces;
using AccountRPD.Interfaces.Presenters;
using AccountRPD.Interfaces.Views;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AccountRPD.Presenters
{
    public class ManagerPresenter : BasePresenter<IManagerView>, IManagerPresenter
    {
        private readonly IManagerService managerService;
        private readonly IRPDService rpdService;
        private readonly IRPDExportService rpdExportService;
        private readonly IAssessmentExportService assessmentExportService;
        private readonly IManagerState managerState;

        public ManagerPresenter(IManagerView view, IManagerService managerService, IRPDService rpdService, IRPDExportService rpdExportService, IAssessmentExportService assessmentExportService, IManagerState managerState)
            : base(view)
        {
            this.managerService = managerService;
            this.rpdService = rpdService;
            this.rpdExportService = rpdExportService;
            this.assessmentExportService = assessmentExportService;
            this.managerState = managerState;

            View.StudyYears = managerService.GetStudyYears();

            View.InitializedHandler += ViewInitialized;
            View.SelectionChangedHandler += ViewSelectionChanged;
            View.PermissionCheckHandler += ViewPermissionCheck;
            View.DisciplineFilterClearedHandler += ViewDisciplineFilterCleared;
            View.RPDCreateHandler += ViewRPDCreate;
            View.RPDEditHandler += ViewRPDEdit;
            View.RPDRemoveHandler += ViewRPDRemove;
            View.RPDExportHandler += ViewRPDExport;
            View.AssessmentExportHandler += ViewAssessmentExport;
            View.ClosingHandler += ViewClosing;
        }

        private void ViewInitialized(object sender, EventArgs eventArgs)
        {
            if (managerState.IsFirstLoad)
            {
                View.SelectedStudyYear = Controller.Application.Session.CurrentStudyYear;
            }
            else
            {
                View.SelectedStudyYearIndex = managerState.StudyYearIndex;
            }
        }

        private void ViewClosing(object sender, EventArgs e)
        {
            managerState.StudyYearIndex = View.SelectedStudyYearIndex;
            managerState.DepartmentIndex = View.SelectedDepartmentIndex;
            managerState.DisciplineIndex = View.SelectedDisciplineIndex;
            managerState.PlanIndex = View.SelectedPlanIndex;
            managerState.RPDIndex = View.SelectedRPDIndex;
            managerState.HideNotActualPlans = View.HideNotActualPlans;
            managerState.FilterText = View.FilterText;

            if (managerState.IsFirstLoad)
            {
                managerState.IsFirstLoad = false;
            }
        }

        private bool CanGetSelectionIndex(int count, int startIndex, int endIndex)
        {
            return !managerState.IsFirstLoad && !startIndex.Equals(-1) && startIndex.Equals(endIndex) && !count.Equals(0);
        }

        private void ViewPermissionCheck(object sender, PermissionCheckEventArgs eventArgs)
        {
            var userDepartments = Controller.Application.Session.DecanatDepartments;
            var userRoles = Controller.Application.Session.Roles;

            if (userRoles.Contains("Administrators"))
            {
                eventArgs.CanRemove = true;
                eventArgs.CanEdit = true;
            }

            var isAttached = default(bool);

            if (userRoles.Contains("DepartmentsManagers"))
            {
                var selectedDepartmentId = View.SelectedDepartment?.Id;
                isAttached = userDepartments.FirstOrDefault(UserDepartment => UserDepartment.Id.Equals(selectedDepartmentId)) != null;
                eventArgs.CanEdit |= selectedDepartmentId.HasValue && isAttached;
                eventArgs.CanRemove |= eventArgs.CanEdit;
            }

            if (userRoles.Contains("FacultiesManagers"))
            {
                var selectedFacultyId = View.SelectedDepartment?.FacultyId;
                isAttached = userDepartments.FirstOrDefault(UserDepartment => UserDepartment.FacultyId.Equals(selectedFacultyId)) != null;
                eventArgs.CanEdit |= selectedFacultyId.HasValue && isAttached;
                eventArgs.CanRemove |= eventArgs.CanEdit;
            }

            if (userRoles.Contains("Teachers"))
            {
                var selectedRPDAuthorId = View.SelectedRPD?.AuthorId;
                eventArgs.CanEdit |= !string.IsNullOrEmpty(selectedRPDAuthorId) && Controller.Application.Session.User.Id.Equals(selectedRPDAuthorId);
            }

            eventArgs.CanEdit &= View.SelectedRPD != null;
            eventArgs.CanRemove &= View.SelectedRPD != null;
        }

        private void ClearDataGrids()
        {
            if (!View.Disciplines.IsEmpty())
            {
                View.Disciplines = null;
            }

            if (!View.Plans.IsEmpty())
            {
                View.Plans = null;
            }

            ClearRPDsDataGrid();
        }

        private void ClearRPDsDataGrid()
        {
            if (!View.RPDs.IsEmpty())
            {
                View.RPDs = null;
            }
        }

        private async void ViewSelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {
            switch (eventArgs.SelectionType)
            {
                case SelectionTypes.StudyYear:
                    ClearDataGrids();

                    View.Departments = managerService.GetAllDepartments();

                    if (CanGetSelectionIndex(View.StudyYears.Count(), View.SelectedStudyYearIndex, managerState.StudyYearIndex) && View.Departments.Contains(managerState.DepartmentIndex))
                    {
                        View.SelectedDepartmentIndex = managerState.DepartmentIndex;
                    }
                    break;

                case SelectionTypes.Department:
                    View.Disciplines = await managerService.GetDisciplinesByDepartmentAsync(View.SelectedDepartment.Id, View.SelectedStudyYear, View.HideNotActualPlans);

                    if (CanGetSelectionIndex(View.Disciplines.Count(), View.SelectedDepartmentIndex, managerState.DepartmentIndex) && View.Disciplines.Contains(managerState.DisciplineIndex))
                    {
                        var isFilterEmpty = string.IsNullOrEmpty(managerState.FilterText);

                        if (isFilterEmpty)
                        {
                            View.SelectedDisciplineIndex = managerState.DisciplineIndex;
                            View.HideNotActualPlans = managerState.HideNotActualPlans;
                        }
                        else
                        {
                            View.FilterText = managerState.FilterText;
                        }
                    }
                    break;

                case SelectionTypes.Filter:
                    View.Disciplines = await managerService.GetDisciplinesByFilterAsync(View.SelectedDepartment.Id, View.SelectedStudyYear, View.HideNotActualPlans, View.FilterText);

                    if (CanGetSelectionIndex(View.Disciplines.Count(), View.SelectedDepartmentIndex, managerState.DepartmentIndex) && View.Disciplines.Contains(managerState.DisciplineIndex))
                    {
                        View.SelectedDisciplineIndex = managerState.DisciplineIndex;
                        View.HideNotActualPlans = managerState.HideNotActualPlans;
                    }
                    break;

                case SelectionTypes.Discipline:
                    ClearRPDsDataGrid();

                    View.Plans = managerService.GetPlansByDiscipline(View.SelectedDiscipline.Title, View.SelectedDepartment.Id, View.SelectedDiscipline.TypeObject, View.SelectedStudyYear, View.HideNotActualPlans);

                    if (CanGetSelectionIndex(View.Plans.Count(), View.SelectedDisciplineIndex, managerState.DisciplineIndex) && View.Plans.Contains(managerState.PlanIndex))
                    {
                        View.SelectedPlanIndex = managerState.PlanIndex;
                    }
                    break;

                case SelectionTypes.Plan:
                    View.RPDs = managerService.GetRPDsByPlan(View.SelectedDiscipline.Id, View.SelectedPlan.Id);

                    if (CanGetSelectionIndex(View.RPDs.Count(), View.SelectedPlanIndex, managerState.PlanIndex) && View.RPDs.Contains(managerState.RPDIndex))
                    {
                        View.SelectedRPDIndex = managerState.RPDIndex;
                    }
                    break;

                case SelectionTypes.RPD:
                case SelectionTypes.Undefined:
                default:
                    return;
            }
        }

        private void ViewDisciplineFilterCleared(object sender, EventArgs e)
        {
            managerState.DisciplineIndex = 0;
            managerState.FilterText = string.Empty;
        }

        private void ViewRPDCreate(object sender, EventArgs eventArgs)
        {
            var department = new ConstructorArgument("department", View.SelectedDepartment);
            var discipline = new ConstructorArgument("discipline", View.SelectedDiscipline);
            var plan = new ConstructorArgument("plan", View.SelectedPlan);

            Controller.Run<IRPDPresenter>(department, discipline, plan);

            managerState.LastIndexAtRPDs = View.RPDs.Count() - 1;

            View.CloseView();
        }

        private void ViewRPDEdit(object sender, EventArgs eventArgs)
        {
            var department = new ConstructorArgument("department", View.SelectedDepartment);
            var discipline = new ConstructorArgument("discipline", View.SelectedDiscipline);
            var plan = new ConstructorArgument("plan", View.SelectedPlan);
            var rpdId = new ConstructorArgument("rpdId", View.SelectedRPD.Id);

            Controller.Run<IRPDPresenter>(department, discipline, plan, rpdId);

            View.CloseView();
        }

        private void ViewRPDRemove(object sender, CancelEventArgs eventArgs)
        {
            var rpdName = $"{View.SelectedRPD.DisciplineName} ({View.SelectedRPD.PlanFileName})";
            var dialogResult = Controller.MessageService.ShowQuestion($"Вы действительно хотите удалить {rpdName}?", "Удаление");

            if (dialogResult.Equals(DialogResult.Yes))
            {
                managerService.RemoveRPD(View.SelectedRPD.Id);
                managerState.RPDIndex = 0;
            }
            else
            {
                eventArgs.Cancel = true;
            }
        }

        private async void ViewRPDExport(object sender, CancelEventArgs eventArgs)
        {
            try
            {
                var rpdExportStrategy = default(IRPDExportStrategy);

                if (View.SelectedRPD.EducationStandardId.Equals(1))
                {
                    rpdExportStrategy = new RPDExport3PStrategy();
                }
                else
                {
                    rpdExportStrategy = new RPDExport3PPStrategy();
                }

                var lessonsTypes = rpdService.GetLessonsTypes(View.SelectedDiscipline.Id, View.SelectedPlan.Id);
                var thematicContentsDictionary = new Dictionary<string, IEnumerable<ThematicContent>>();

                foreach (var lessonType in lessonsTypes)
                {
                    var thematicContentsList = await rpdService.GetRPDsThematicContentsByLessonTypeAsync(View.SelectedRPD.Id, lessonType);
                    thematicContentsDictionary.Add(lessonType, thematicContentsList);
                }

                var rpdName = Path.GetFileName(View.RPDFilePath);
                var hoursDivision = rpdService.GetDecanatHoursDivision(View.SelectedRPD.DecanatPlanId, View.SelectedRPD.DecanatDisciplineId);
                var formsOfControl = rpdService.GetFormsOfControl(View.SelectedRPD.DecanatPlanId, View.SelectedRPD.DecanatDisciplineId);
                var courses = rpdService.GetCoursesByPlan(View.SelectedRPD.DecanatPlanId, View.SelectedRPD.DecanatDisciplineId);
                var semesters = rpdService.GetSemestersByPlan(View.SelectedRPD.DecanatPlanId, View.SelectedRPD.DecanatDisciplineId);
                var thematicPlans = rpdService.GetThematicPlans(View.SelectedRPD.Id);
                var totalHoursForThematicPlans = rpdService.GetTotalHoursForThematicPlans(thematicPlans as IList<ThematicPlan>);
                var competences = rpdService.GetCompetencesByRPD(View.SelectedDiscipline.Id, View.SelectedPlan.Id, View.SelectedRPD.Id);
                var rpdItemsIds = await rpdService.GetEducationStandartRPDItemsAsync(View.SelectedRPD.EducationStandardId, 1);
                var rpdItems = await rpdService.GetRPDItemsAsync(View.SelectedRPD.EducationStandardId, 1);
                var rpdContents = await rpdService.GetRPDContentsAsync(View.SelectedRPD.Id);
                var members = await rpdService.GetRPDsMembersAsync(View.SelectedRPD.Id);
                var basicLiteratures = await rpdService.GetRPDsBasicLiteraturesAsync(View.SelectedRPD.Id);
                var additionalLiteratures = await rpdService.GetRPDsAdditionalLiteraturesAsync(View.SelectedRPD.Id);
                var librarySystems = await rpdService.GetRPDsLibrarySystemsAsync(View.SelectedRPD.Id);
                var internetResources = await rpdService.GetRPDsInternetResourcesAsync(View.SelectedRPD.Id);
                var licenses = await rpdService.GetRPDsLicencesAsync(View.SelectedRPD.Id);
                var materialBases = await rpdService.GetRPDsMaterialBasesAsync(View.SelectedRPD.Id);
                var rpdContent = await rpdService.GetRPDContentAsync(View.SelectedRPD.Id, rpdItemsIds["tpIndependentWork"]);

                try
                {
                    var rpdFileStream = default(Stream);

                    if (rpdContent?.FileContent != null)
                    {
                        File.WriteAllBytes($"Учебно-методическое обеспечение из {rpdName}", rpdContent.FileContent);
                        rpdFileStream = new FileStream($"Учебно-методическое обеспечение из {rpdName}", FileMode.Open);
                    }

                    rpdExportService.SetExportStrategy(rpdExportStrategy)
                                    .Create(View.RPDFilePath, rpdItems)
                                    .Include(View.SelectedRPD, View.SelectedDepartment, members)
                                    .IncludeTableOfContents()
                                    .Include(rpdContents)
                                    .Include(competences)
                                    .Include(View.SelectedPlan, rpdContents, hoursDivision, formsOfControl, courses, semesters)
                                    .Include(thematicPlans, totalHoursForThematicPlans.lectures, totalHoursForThematicPlans.practices, totalHoursForThematicPlans.labs, totalHoursForThematicPlans.individualWorks)
                                    .Include(thematicContentsDictionary);

                    if (rpdContent?.FileContent != null)
                    {
                        rpdExportService.Include(rpdFileStream);
                    }
                    else
                    {
                        rpdExportService.Include(rpdContent?.Content);
                    }

                    rpdExportService.Include(basicLiteratures, additionalLiteratures)
                                    .Include(librarySystems, internetResources)
                                    .Include(licenses)
                                    .Include(materialBases)
                                    .Export();

                    if (rpdFileStream != null)
                    {
                        rpdFileStream.Close();
                    }

                    File.Delete($"Учебно-методическое обеспечение из {rpdName}");
                }

                catch (IOException error)
                {
                    Controller.MessageService.ShowError($"Произошла ошибка экспортирования\n\nБолее подробные сведения:\n{error.Message}");
                    eventArgs.Cancel = true;
                }
            }

            catch
            {
                Controller.MessageService.ShowError("Произошла непредвиденная ошибка при экспорте рабочей программы дисциплины");
                eventArgs.Cancel = true;
            }
        }

        private async void ViewAssessmentExport(object sender, CancelEventArgs eventArgs)
        {
            try
            {
                var assessmentExportStrategy = default(IAssessmentExportStrategy);

                if (View.SelectedRPD.EducationStandardId.Equals(1))
                {
                    assessmentExportStrategy = new AssessmentExport3PStrategy();
                }
                else
                {
                    assessmentExportStrategy = new AssessmentExport3PPStrategy();
                }

                var assessmentName = Path.GetFileName(View.AssessmentFilePath);
                var competenceGradesDictionary = rpdService.GetCompetenceGrades(View.SelectedDiscipline.Id, View.SelectedPlan.Id, View.SelectedRPD.EducationStandardId);
                var competences = rpdService.GetCompetencesByRPD(View.SelectedDiscipline.Id, View.SelectedPlan.Id, View.SelectedRPD.Id);
                var formsOfControl = rpdService.GetFormsOfControl(View.SelectedRPD.DecanatPlanId, View.SelectedRPD.DecanatDisciplineId);
                var assessmentItemsIds = await rpdService.GetEducationStandartRPDItemsAsync(View.SelectedRPD.EducationStandardId, 2);
                var members = await rpdService.GetRPDsMembersAsync(View.SelectedRPD.Id);
                var checkWorkContent = await rpdService.GetRPDContentAsync(View.SelectedRPD.Id, assessmentItemsIds["tpCheckWork"]);
                var materialsContent = await rpdService.GetRPDContentAsync(View.SelectedRPD.Id, assessmentItemsIds["tpAssessment"]);
                var assessmentItems = await rpdService.GetRPDItemsAsync(View.SelectedRPD.EducationStandardId, 2);

                try
                {
                    var checkWorkFileStream = default(Stream);
                    var materialsFileStream = default(Stream);

                    if (checkWorkContent?.FileContent != null)
                    {
                        File.WriteAllBytes($"Контрольные задания из {assessmentName}", checkWorkContent.FileContent);
                        checkWorkFileStream = new FileStream($"Контрольные задания из {assessmentName}", FileMode.Open);
                    }

                    if (materialsContent?.FileContent != null)
                    {
                        File.WriteAllBytes($"Методические материалы из {assessmentName}", materialsContent.FileContent);
                        materialsFileStream = new FileStream($"Методические материалы из {assessmentName}", FileMode.Open);
                    }

                    assessmentExportService.SetExportStrategy(assessmentExportStrategy)
                                           .Create(View.AssessmentFilePath, assessmentItems)
                                           .Include(View.SelectedRPD, View.SelectedDepartment, members)
                                           .IncludeTableOfContents()
                                           .Include(competenceGradesDictionary, competences);

                    if (checkWorkContent?.FileContent != null)
                    {
                        assessmentExportService.IncludeAssessmentEquipment(checkWorkFileStream);
                    }
                    else
                    {
                        assessmentExportService.IncludeAssessmentEquipment(checkWorkContent?.Content);
                    }

                    if (materialsContent?.FileContent != null)
                    {
                        assessmentExportService.IncludeMaterials(materialsFileStream, formsOfControl);
                    }
                    else
                    {
                        assessmentExportService.IncludeMaterials(materialsContent?.Content, formsOfControl);
                    }

                    assessmentExportService.Export();

                    if (checkWorkFileStream != null)
                    {
                        checkWorkFileStream.Close();
                    }

                    File.Delete($"Контрольные задания из {assessmentName}");

                    if (materialsFileStream != null)
                    {
                        materialsFileStream.Close();
                    }

                    File.Delete($"Методические материалы из {assessmentName}");
                }

                catch (IOException error)
                {
                    Controller.MessageService.ShowError($"Произошла ошибка экспортирования\n\nБолее подробные сведения:\n{error.Message}");
                    eventArgs.Cancel = true;
                }
            }

            catch
            {
                Controller.MessageService.ShowError("Произошла непредвиденная ошибка при экспорте оценочных материалов");
                eventArgs.Cancel = true;
            }
        }
    }
}