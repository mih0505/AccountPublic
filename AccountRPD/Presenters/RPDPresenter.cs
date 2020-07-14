using Account.DAL.Entities;
using AccountRPD.BL.Interfaces;
using AccountRPD.BL.Strategies;
using AccountRPD.Infrastucture;
using AccountRPD.Interfaces;
using AccountRPD.Interfaces.Presenters;
using AccountRPD.Interfaces.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using License = Account.DAL.Entities.License;

namespace AccountRPD.Presenters
{
    public class RPDPresenter : BasePresenter<IRPDView>, IRPDPresenter
    {
        private readonly IRPDService rpdService;
        private readonly IRPDExportService exportService;
        private readonly IManagerState managerState;

        public DecanatDepartment Department { get; private set; }
        public DecanatDiscipline Discipline { get; private set; }
        public DecanatPlan Plan { get; private set; }
        public RPD RPD { get; set; }

        public RPDPresenter(IRPDView view, IRPDService rpdService, IRPDExportService exportService, DecanatDepartment department, DecanatDiscipline discipline, DecanatPlan plan, IManagerState managerState)
            : base(view)
        {
            this.rpdService = rpdService;
            this.exportService = exportService;
            this.managerState = managerState;

            Department = department;
            Discipline = discipline;
            Plan = plan;

            View.FacultyTitle = Department.FacultyTitle;
            View.DepartmentTitle = Department.Title;
            View.DisciplineTitle = Discipline.Title;
            View.RPDName = $"{View.DisciplineTitle} ({Plan.Filename})";
            View.ProfileTitle = Plan.ProgramRequisites;
            View.DepartmentChief = Department.Chief;
            View.EducationStandardId = Plan.FGOSId.Equals("3,5") ? 1 : 2;

            var member = new Member()
            {
                TeacherId = Controller.Application.Session.User.Id,
                Lastname = Controller.Application.Session.User.Lastname,
                Firstname = Controller.Application.Session.User.Firstname,
                Middlename = Controller.Application.Session.User.Middlename,
                MemberType = rpdService.GetMemberType(Controller.Application.Session.User.Id)
            };

            var membersList = new BindingList<Member> { member };
            View.MembersList = membersList;

            View.LessonsTypes = rpdService.GetLessonsTypes(Discipline.Id, Plan.Id);

            View.Competences = rpdService.GetCompetences(Discipline.Id, Plan.Id);
            View.HoursDivision = rpdService.GetDisciplineHoursDivision(Discipline.Id, Plan.Id);
            View.ThematicPlans = new BindingList<ThematicPlan>();

            View.ThematicContentsDictionary = new Dictionary<string, IEnumerable<ThematicContent>>();

            foreach (var lessonType in View.LessonsTypes)
            {
                var thematicContentsList = new BindingList<ThematicContent>();
                View.ThematicContentsDictionary.Add(lessonType, thematicContentsList);
            }

            View.TotalPlanLectures = rpdService.GetWorkHourDivision(Discipline.Id, Plan.Id, "Лек");
            View.TotalPlanPractices = rpdService.GetWorkHourDivision(Discipline.Id, Plan.Id, "Прак");
            View.TotalPlanLabs = rpdService.GetWorkHourDivision(Discipline.Id, Plan.Id, "Лаб");
            View.TotalPlanIndividualWorks = rpdService.GetWorkHourDivision(Discipline.Id, Plan.Id, "Сам");

            View.BasicLiteratures = new BindingList<BasicLiterature>();
            View.AdditionalLiteratures = new BindingList<AdditionalLiterature>();
            View.LibrarySystems = new BindingList<LibrarySystem>();
            View.InternetResources = new BindingList<InternetResource>();
            View.Licenses = new BindingList<License>();
            View.MaterialBases = new BindingList<MaterialBase>();

            View.CompetenceGrades = rpdService.GetCompetenceGrades(Discipline.Id, Plan.Id, View.EducationStandardId);

            View.DisciplinePlace = (View.EducationStandardId.Equals(1))
                                   ? "Дисциплина реализуется в рамках базовой/вариативной (выбрать) части"
                                   : "1.\n2.\n3.\n...\nДисциплина реализуется в рамках обязательной части/части, формируемой участниками образовательных отношений (выбрать)";

            View.Authors = rpdService.GetDepartmentTeachers(Department.Id);
            View.RPDTypes = rpdService.GetRPDTypes();

            View.DisciplineTitleTextChangedHandler += ViewDisciplineTitleTextChangedExecute;
            View.CompetenceStageBeforeSelectHandler += ViewCompetenceStageBeforeSelectExecute;
            View.CompetenceNameBeforeSelectHandler += ViewCompetenceNameBeforeSelectExecute;
            View.CompetenceSelectionChangedHandler += ViewCompetenceSelectionChanged;
            View.AddSectionHandler += ViewAddSectionExecute;
            View.AddTopicHandler += ViewAddTopicExecute;
            View.RemoveSectionOrTopicHandler += ViewRemoveSectionOrTopicExecute;
            View.ThematicPlanEndEditHandler += ViewThematicPlanEndEditExecute;
            View.LessonTypesSelectHandler += ViewLessonTypesSelectExecute;
            View.ThematicContentEndEditHandler += ViewThematicContentEndEditExecute;
            View.SaveClickHandler += ViewSaveClickExecute;
        }

        public RPDPresenter(IRPDView view, IRPDService rpdService, IRPDExportService exportService, DecanatDepartment department, DecanatDiscipline discipline, DecanatPlan plan, int rpdId, IManagerState managerState)
            : base(view)
        {
            this.rpdService = rpdService;
            this.exportService = exportService;
            this.managerState = managerState;

            Department = department;
            Discipline = discipline;
            Plan = plan;
            RPD = rpdService.GetRPD(rpdId);

            View.FacultyTitle = Department.FacultyTitle;
            View.DepartmentTitle = Department.Title;
            View.DisciplineTitle = Discipline.Title;
            View.RPDName = $"{View.DisciplineTitle} ({Plan.Filename})";
            View.ProfileTitle = Plan.ProgramRequisites;
            View.DepartmentChief = Department.Chief;
            View.ApprovalDateRPD = RPD.ApprovalDateRPD.Value;
            View.ProtocolNumberRPD = RPD.ProtocolNumberRPD;
            View.EducationStandardId = RPD.EducationStandardId;

            Task.Run(async () =>
            {
                var members = await rpdService.GetRPDsMembersAsync(rpdId) as IList<Member>;
                View.MembersList = new BindingList<Member>(members);

                var rpdItems = await rpdService.GetEducationStandartRPDItemsAsync(RPD.EducationStandardId, 1);
                var rpdContents = await rpdService.GetRPDContentsAsync(rpdId);

                var results = rpdContents.FirstOrDefault(RPDContent => RPDContent.RPDItemId.Equals(rpdItems["rtbResults"]));
                View.Results = results?.Content;

                var disciplinePlace = rpdContents.FirstOrDefault(RPDContent => RPDContent.RPDItemId.Equals(rpdItems["rtbDisciplinePlace"]));
                View.DisciplinePlace = disciplinePlace?.Content;

                var independentWork = rpdContents.FirstOrDefault(RPDContent => RPDContent.RPDItemId.Equals(rpdItems["tpIndependentWork"]));

                if (!string.IsNullOrEmpty(independentWork?.FileName))
                {
                    View.IsIndependentWorkLoadFromWord = true;
                    View.IndependentWorkInsertionFilePath = independentWork?.FileName;
                }
                else if (!string.IsNullOrEmpty(independentWork?.Content))
                {
                    View.IsIndependentWorkInsertText = true;
                    View.IndependentWorkInsertionText = independentWork?.Content;
                }

                var assessmentItemsIds = await rpdService.GetEducationStandartRPDItemsAsync(RPD.EducationStandardId, 2);

                var checkWork = rpdContents.FirstOrDefault(RPDContent => RPDContent.RPDItemId.Equals(assessmentItemsIds["tpCheckWork"]));

                if (!string.IsNullOrEmpty(checkWork?.FileName))
                {
                    View.IsCheckWorkLoadFromWord = true;
                    View.CheckWorkInsertionFilePath = checkWork?.FileName;
                }
                else if (!string.IsNullOrEmpty(checkWork?.Content))
                {
                    View.IsCheckWorkInsertText = true;
                    View.CheckWorkInsertionText = checkWork?.Content;
                }

                var assessment = rpdContents.FirstOrDefault(RPDContent => RPDContent.RPDItemId.Equals(assessmentItemsIds["tpAssessment"]));

                if (!string.IsNullOrEmpty(assessment?.FileName))
                {
                    View.IsAssessmentLoadFromWord = true;
                    View.AssessmentInsertionFilePath = assessment?.FileName;
                }
                else if (!string.IsNullOrEmpty(assessment?.Content))
                {
                    View.IsAssessmentInsertText = true;
                    View.AssessmentInsertionText = assessment?.Content;
                }

                var basicLiteratures = await rpdService.GetRPDsBasicLiteraturesAsync(rpdId) as IList<BasicLiterature>;
                View.BasicLiteratures = new BindingList<BasicLiterature>(basicLiteratures);

                var additionalLiteratures = await rpdService.GetRPDsAdditionalLiteraturesAsync(rpdId) as IList<AdditionalLiterature>;
                View.AdditionalLiteratures = new BindingList<AdditionalLiterature>(additionalLiteratures);

                var librarySystems = await rpdService.GetRPDsLibrarySystemsAsync(rpdId) as IList<LibrarySystem>;
                View.LibrarySystems = new BindingList<LibrarySystem>(librarySystems);

                var internetResources = await rpdService.GetRPDsInternetResourcesAsync(rpdId) as IList<InternetResource>;
                View.InternetResources = new BindingList<InternetResource>(internetResources);

                var licenses = await rpdService.GetRPDsLicencesAsync(rpdId) as IList<License>;
                View.Licenses = new BindingList<License>(licenses);

                var materialBases = await rpdService.GetRPDsMaterialBasesAsync(rpdId) as IList<MaterialBase>;
                View.MaterialBases = new BindingList<MaterialBase>(materialBases);

            }).Wait();

            View.Competences = rpdService.GetCompetencesByRPD(Discipline.Id, Plan.Id, RPD.Id);
            View.CompetenceGrades = rpdService.GetCompetenceGrades(Discipline.Id, Plan.Id, RPD.EducationStandardId);
            View.HoursDivision = rpdService.GetDisciplineHoursDivision(Discipline.Id, Plan.Id);
            View.LessonsTypes = rpdService.GetLessonsTypes(Discipline.Id, Plan.Id);

            var thematicPlans = rpdService.GetThematicPlans(rpdId) as IList<ThematicPlan>;
            View.ThematicPlans = new BindingList<ThematicPlan>(thematicPlans);

            Task.Run(async () =>
            {
                View.ThematicContentsDictionary = new Dictionary<string, IEnumerable<ThematicContent>>();

                foreach (var lessonType in View.LessonsTypes)
                {
                    var thematicContentsList = await rpdService.GetRPDsThematicContentsByLessonTypeAsync(rpdId, lessonType);
                    View.ThematicContentsDictionary.Add(lessonType, thematicContentsList);
                }

            }).Wait();

            View.ThematicContents = View.ThematicContentsDictionary[View.SelectedLessonType];

            var totalHoursForThematicPlans = rpdService.GetTotalHoursForThematicPlans(View.ThematicPlans as IList<ThematicPlan>);
            View.TotalLectures = totalHoursForThematicPlans.lectures;
            View.TotalPractices = totalHoursForThematicPlans.practices;
            View.TotalLabs = totalHoursForThematicPlans.labs;
            View.TotalIndividualWorks = totalHoursForThematicPlans.individualWorks;

            View.TotalPlanLectures = rpdService.GetWorkHourDivision(Discipline.Id, Plan.Id, "Лек");
            View.TotalPlanPractices = rpdService.GetWorkHourDivision(Discipline.Id, Plan.Id, "Прак");
            View.TotalPlanLabs = rpdService.GetWorkHourDivision(Discipline.Id, Plan.Id, "Лаб");
            View.TotalPlanIndividualWorks = rpdService.GetWorkHourDivision(Discipline.Id, Plan.Id, "Сам");

            View.Authors = rpdService.GetDepartmentTeachers(Department.Id, RPD.Id);
            View.RPDTypes = rpdService.GetRPDTypes();

            View.ViewInitializedHandler += ViewInitialized;
            View.DisciplineTitleTextChangedHandler += ViewDisciplineTitleTextChangedExecute;
            View.CompetenceStageBeforeSelectHandler += ViewCompetenceStageBeforeSelectExecute;
            View.CompetenceNameBeforeSelectHandler += ViewCompetenceNameBeforeSelectExecute;
            View.CompetenceSelectionChangedHandler += ViewCompetenceSelectionChanged;
            View.AddSectionHandler += ViewAddSectionExecute;
            View.AddTopicHandler += ViewAddTopicExecute;
            View.RemoveSectionOrTopicHandler += ViewRemoveSectionOrTopicExecute;
            View.ThematicPlanEndEditHandler += ViewThematicPlanEndEditExecute;
            View.LessonTypesSelectHandler += ViewLessonTypesSelectExecute;
            View.ThematicContentEndEditHandler += ViewThematicContentEndEditExecute;
            View.SaveClickHandler += ViewSaveClickExecute;
        }

        private void ViewInitialized(object sender, EventArgs e)
        {
            View.SelectedAuthor = rpdService.GetRPDAuthor(RPD.Id);
            View.SelectedRPDType = rpdService.GetDescriptionByRPDType(RPD.RdpType);

            var userRoles = Controller.Application.Session.Roles;
            View.CanEditAuthor = (userRoles.Contains("DepartmentsManagers") || userRoles.Contains("FacultiesManagers") || userRoles.Contains("Administrators"));
            View.CanEditState = View.CanEditAuthor;
        }

        private async void ViewSaveClickExecute(object sender, EventArgs e)
        {
            var invalidFileCount = 0;

            if (RPD is null)
            {
                RPD = new RPD()
                {
                    AuthorId = View.SelectedAuthor.Id,
                    DecanatDepartmentId = Department.Id,
                    DecanatDisciplineId = Discipline.Id,
                    DecanatPlanId = Plan.Id,
                    EducationStandardId = View.EducationStandardId,
                    DisciplineName = View.DisciplineTitle,
                    PlanFileName = Plan.Filename,
                    ApprovalDateRPD = View.ApprovalDateRPD,
                    ProtocolNumberRPD = View.ProtocolNumberRPD,
                    DirectionName = View.ProfileTitle,
                    ProfileCode = Plan.ProfileCode,
                    ProfileName = Plan.Profile,
                    DepartmentManager = View.DepartmentChief,
                    Block = Plan.Block
                };
            }
            else
            {
                RPD.AuthorId = View.SelectedAuthor.Id;
                RPD.DisciplineName = View.DisciplineTitle;
                RPD.DepartmentManager = View.DepartmentChief;
                RPD.ApprovalDateRPD = View.ApprovalDateRPD;
                RPD.ProtocolNumberRPD = View.ProtocolNumberRPD;
                RPD.EditDate = DateTime.Now;
            }

            rpdService.CommitChanges(RPD);
            await rpdService.CommitChanges(RPD, View.MembersList);

            var competences = new List<Competence>();
            foreach (var keyValuePair in View.Competences)
            {
                competences.Add(keyValuePair.Value);
            }

            rpdService.CommitChanges(RPD, competences);

            var rpdItemsIds = await rpdService.GetEducationStandartRPDItemsAsync(RPD.EducationStandardId, 1);

            if (rpdItemsIds.ContainsKey("rtbResults") && !string.IsNullOrEmpty(View.Results))
            {
                var resultsContent = await rpdService.GetRPDContentAsync(RPD.Id, rpdItemsIds["rtbResults"]);

                if (resultsContent is null)
                {
                    resultsContent = new RPDContent()
                    {
                        RPDItemId = rpdItemsIds["rtbResults"],
                        Content = View.Results
                    };
                }
                else
                {
                    resultsContent.Content = View.Results;
                }

                rpdService.CommitChanges(RPD, resultsContent);
            }

            if (rpdItemsIds.ContainsKey("rtbDisciplinePlace") && !string.IsNullOrEmpty(View.DisciplinePlace))
            {
                var disciplinePlaceContent = await rpdService.GetRPDContentAsync(RPD.Id, rpdItemsIds["rtbDisciplinePlace"]);

                if (disciplinePlaceContent is null)
                {
                    disciplinePlaceContent = new RPDContent()
                    {
                        RPDItemId = rpdItemsIds["rtbDisciplinePlace"],
                        Content = View.DisciplinePlace
                    };
                }
                else
                {
                    disciplinePlaceContent.Content = View.DisciplinePlace;
                }

                rpdService.CommitChanges(RPD, disciplinePlaceContent);
            }

            if (rpdItemsIds.ContainsKey("tpIndependentWork"))
            {
                var fileContent = default(byte[]);
                var fileName = default(string);
                var content = default(string);

                if (View.IsIndependentWorkLoadFromWord && !string.IsNullOrEmpty(View.IndependentWorkInsertionFilePath))
                {
                    if (File.Exists(View.IndependentWorkInsertionFilePath))
                    {
                        try
                        {
                            fileContent = File.ReadAllBytes(View.IndependentWorkInsertionFilePath);
                            fileName = Path.GetFileName(View.IndependentWorkInsertionFilePath);
                        }

                        catch
                        {
                            Controller.MessageService.ShowExclamation("Открытый документ не может быть прикреплен к РПД\nЗакройте его, прежде чем продолжить");
                            return;
                        }
                    }
                    else
                    {
                        invalidFileCount++;
                    }
                }
                else if (View.IsIndependentWorkInsertText && !string.IsNullOrEmpty(View.IndependentWorkInsertionText))
                {
                    content = View.IndependentWorkInsertionText;
                }

                var independentWorkContent = await rpdService.GetRPDContentAsync(RPD.Id, rpdItemsIds["tpIndependentWork"]);

                if (independentWorkContent is null)
                {
                    independentWorkContent = new RPDContent()
                    {
                        RPDItemId = rpdItemsIds["tpIndependentWork"],
                        FileContent = fileContent,
                        FileName = fileName,
                        Content = content
                    };
                }
                else
                {
                    if (View.IsIndependentWorkLoadFromWord && fileContent != null)
                    {
                        independentWorkContent.FileContent = fileContent;
                        independentWorkContent.FileName = fileName;
                        independentWorkContent.Content = null;
                    }
                    else if (View.IsIndependentWorkInsertText && content != null)
                    {
                        independentWorkContent.FileContent = null;
                        independentWorkContent.FileName = null;
                        independentWorkContent.Content = content;
                    }
                }

                if (independentWorkContent.Content != null || independentWorkContent.FileName != null || independentWorkContent.FileContent != null)
                {
                    rpdService.CommitChanges(RPD, independentWorkContent);
                }
            }

            var assessmentItemsIds = await rpdService.GetEducationStandartRPDItemsAsync(RPD.EducationStandardId, 2);

            if (assessmentItemsIds.ContainsKey("tpCheckWork"))
            {
                var fileContent = default(byte[]);
                var fileName = default(string);
                var content = default(string);

                if (View.IsCheckWorkLoadFromWord && !string.IsNullOrEmpty(View.CheckWorkInsertionFilePath))
                {
                    if (File.Exists(View.CheckWorkInsertionFilePath))
                    {
                        try
                        {
                            fileContent = File.ReadAllBytes(View.CheckWorkInsertionFilePath);
                            fileName = Path.GetFileName(View.CheckWorkInsertionFilePath);
                        }

                        catch
                        {
                            Controller.MessageService.ShowExclamation("Открытый документ не может быть прикреплен к РПД\nЗакройте его, прежде чем продолжить");
                            return;
                        }
                    }
                    else
                    {
                        invalidFileCount++;
                    }
                }
                else if (View.IsCheckWorkInsertText && !string.IsNullOrEmpty(View.CheckWorkInsertionText))
                {
                    content = View.CheckWorkInsertionText;
                }

                var checkWorkContent = await rpdService.GetRPDContentAsync(RPD.Id, assessmentItemsIds["tpCheckWork"]);

                if (checkWorkContent is null)
                {
                    checkWorkContent = new RPDContent()
                    {
                        RPDItemId = assessmentItemsIds["tpCheckWork"],
                        FileContent = fileContent,
                        FileName = fileName,
                        Content = content
                    };
                }
                else
                {
                    if (View.IsCheckWorkLoadFromWord && fileContent != null)
                    {
                        checkWorkContent.FileContent = fileContent;
                        checkWorkContent.FileName = fileName;
                        checkWorkContent.Content = null;
                    }
                    else if (View.IsCheckWorkInsertText && content != null)
                    {
                        checkWorkContent.FileContent = null;
                        checkWorkContent.FileName = null;
                        checkWorkContent.Content = content;
                    }
                }

                if (checkWorkContent.Content != null || checkWorkContent.FileName != null || checkWorkContent.FileContent != null)
                {
                    rpdService.CommitChanges(RPD, checkWorkContent);
                }
            }

            if (assessmentItemsIds.ContainsKey("tpAssessment"))
            {
                var fileContent = default(byte[]);
                var fileName = default(string);
                var content = default(string);

                if (View.IsAssessmentLoadFromWord && !string.IsNullOrEmpty(View.AssessmentInsertionFilePath))
                {
                    if (File.Exists(View.AssessmentInsertionFilePath))
                    {
                        try
                        {
                            fileContent = File.ReadAllBytes(View.AssessmentInsertionFilePath);
                            fileName = Path.GetFileName(View.AssessmentInsertionFilePath);
                        }

                        catch
                        {
                            Controller.MessageService.ShowExclamation("Открытый документ не может быть прикреплен к РПД\nЗакройте его, прежде чем продолжить");
                            return;
                        }
                    }
                    else
                    {
                        invalidFileCount++;
                    }
                }
                else if (View.IsAssessmentInsertText && !string.IsNullOrEmpty(View.AssessmentInsertionText))
                {
                    content = View.AssessmentInsertionText;
                }

                var assessmentContent = await rpdService.GetRPDContentAsync(RPD.Id, assessmentItemsIds["tpAssessment"]);

                if (assessmentContent is null)
                {
                    assessmentContent = new RPDContent()
                    {
                        RPDItemId = assessmentItemsIds["tpAssessment"],
                        FileContent = fileContent,
                        FileName = fileName,
                        Content = content
                    };
                }
                else
                {
                    if (View.IsAssessmentLoadFromWord && fileContent != null)
                    {
                        assessmentContent.FileContent = fileContent;
                        assessmentContent.FileName = fileName;
                        assessmentContent.Content = null;
                    }
                    else if (View.IsAssessmentInsertText && content != null)
                    {
                        assessmentContent.FileContent = null;
                        assessmentContent.FileName = null;
                        assessmentContent.Content = content;
                    }
                }

                if (assessmentContent.Content != null || assessmentContent.FileName != null || assessmentContent.FileContent != null)
                {
                    rpdService.CommitChanges(RPD, assessmentContent);
                }
            }

            if (invalidFileCount.Equals(1))
            {
                Controller.MessageService.ShowExclamation("Прикрепленный файл не может быть добавлен в РПД, так как он удален или перемещен");
                return;
            }
            else if (invalidFileCount > 1)
            {
                Controller.MessageService.ShowExclamation("Прикрепленные файлы не могут быть добавлены в РПД, так как они удалены или перемещены");
                return;
            }

            rpdService.CommitChanges(RPD, View.ThematicPlans);

            var thematicPlans = View.ThematicPlans as IList<ThematicPlan>;

            foreach (var lessonType in View.LessonsTypes)
            {
                var thematicContents = View.ThematicContentsDictionary[lessonType] as IList<ThematicContent>;

                foreach (var thematicContent in thematicContents)
                {
                    var thematicContentIndex = thematicContents.IndexOf(thematicContent);
                    thematicContent.ThematicPlan = View.ThematicPlans.FirstOrDefault(ThematicPlan => thematicPlans.IndexOf(ThematicPlan).Equals(thematicContentIndex));
                }

                rpdService.CommitChanges(RPD, thematicContents);
            }

            await rpdService.CommitChanges(RPD, View.BasicLiteratures);
            await rpdService.CommitChanges(RPD, View.AdditionalLiteratures);
            await rpdService.CommitChanges(RPD, View.LibrarySystems);
            await rpdService.CommitChanges(RPD, View.InternetResources);
            await rpdService.CommitChanges(RPD, View.Licenses);
            await rpdService.CommitChanges(RPD, View.MaterialBases);

            foreach (var keyValuePair in View.CompetenceGrades)
            {
                rpdService.CommitChanges(RPD, keyValuePair.Value);
            }

            await rpdService.SaveAsync();

            RPD.FullnessPercent = await rpdService.GetFullnessPercentAsync(RPD.Id);
            RPD.RdpType = rpdService.GetRPDTypeByDescription(View.SelectedRPDType);

            rpdService.CommitChanges(RPD);
            await rpdService.SaveAsync();

            Controller.MessageService.ShowInformation("РПД успешно сохранен. Вы можете продолжить редактирование или закрыть форму");
            managerState.RPDIndex = managerState.LastIndexAtRPDs + 1;
        }

        private void ViewThematicContentEndEditExecute(object sender, DataGridViewCellEventArgs e)
        {
            var editedThematicContents = View.ThematicContentsDictionary[View.SelectedLessonType].ElementAtOrDefault(e.RowIndex);
            editedThematicContents.Content = View.ThematicContentValue;
        }

        private void ViewThematicPlanEndEditExecute(object sender, DataGridViewCellEventArgs e)
        {
            var totalHoursForSection = rpdService.GetTotalHoursForSection(View.ThematicPlans as IList<ThematicPlan>, View.SelectedThematicPlan?.Number);
            var section = View.ThematicPlans.FirstOrDefault(ThematicPlan => ThematicPlan.Number.Equals(totalHoursForSection.sectionNumber));

            section.Lecture = totalHoursForSection.lectures;
            section.Practice = totalHoursForSection.practices;
            section.Lab = totalHoursForSection.labs;
            section.IndividualWork = totalHoursForSection.individualWorks;

            var totalHoursForThematicPlans = rpdService.GetTotalHoursForThematicPlans(View.ThematicPlans as IList<ThematicPlan>);
            View.TotalLectures = totalHoursForThematicPlans.lectures;
            View.TotalPractices = totalHoursForThematicPlans.practices;
            View.TotalLabs = totalHoursForThematicPlans.labs;
            View.TotalIndividualWorks = totalHoursForThematicPlans.individualWorks;

            View.ThematicContents = View.ThematicContentsDictionary[View.SelectedLessonType];
        }

        private void ViewAddThematicContentByThematicPlan(int index, ThematicPlan thematicPlan)
        {
            foreach (var key in View.LessonsTypes)
            {
                var thematicContents = View.ThematicContentsDictionary[key] as IList<ThematicContent>;

                var thematicContent = new ThematicContent()
                {
                    ThematicPlan = thematicPlan,
                    LessonType = key
                };

                thematicContents.Insert(index, thematicContent);

                View.ThematicContentsDictionary[key] = thematicContents;
            }

            View.ThematicContents = View.ThematicContentsDictionary[View.SelectedLessonType];
        }

        private void ViewAddSectionExecute(object sender, EventArgs e)
        {
            var thematicPlans = View.ThematicPlans as IList<ThematicPlan>;

            var thematicPlan = new ThematicPlan()
            {
                IsSection = true
            };

            var index = thematicPlans.IndexOf(View.SelectedThematicPlan) + 1;
            thematicPlans.Insert(index, thematicPlan);

            View.ThematicPlans = rpdService.GetUpdatedThematicPlans(thematicPlans);

            ViewAddThematicContentByThematicPlan(index, thematicPlans[index]);
        }

        private void ViewAddTopicExecute(object sender, EventArgs e)
        {
            var thematicPlans = View.ThematicPlans as IList<ThematicPlan>;

            if (thematicPlans.Count.Equals(0))
            {
                Controller.MessageService.ShowExclamation("Добавить тему вне раздела нельзя. Сначала добавьте раздел, затем тему");
                return;
            }

            var thematicPlan = new ThematicPlan();

            var index = thematicPlans.IndexOf(View.SelectedThematicPlan) + 1;
            thematicPlans.Insert(index, thematicPlan);

            View.ThematicPlans = rpdService.GetUpdatedThematicPlans(thematicPlans);

            ViewAddThematicContentByThematicPlan(index, thematicPlans[index]);
        }

        private void ViewRemoveSectionOrTopicExecute(object sender, EventArgs e)
        {
            if (View.SelectedThematicPlan is null)
            {
                return;
            }

            var themes = View.ThematicPlans.Where(ThematicPlan => !ThematicPlan.IsSection && ThematicPlan.Number.Split('.')[0].Contains(View.SelectedThematicPlan.Number)).ToList();

            if (View.SelectedThematicPlan.IsSection && !themes.Count().Equals(0))
            {
                Controller.MessageService.ShowExclamation("Раздел с темами удалить нельзя. Сначала удалите темы, затем раздел");
                return;
            }

            var thematicPlans = View.ThematicPlans as IList<ThematicPlan>;

            var index = thematicPlans.IndexOf(View.SelectedThematicPlan);
            foreach (var key in View.LessonsTypes)
            {
                var thematicContents = View.ThematicContentsDictionary[key] as IList<ThematicContent>;
                thematicContents.RemoveAt(index);

                View.ThematicContentsDictionary[key] = thematicContents;
            }

            View.ThematicContents = View.ThematicContentsDictionary[View.SelectedLessonType];

            thematicPlans.RemoveAt(index);

            View.ThematicPlans = rpdService.GetUpdatedThematicPlans(thematicPlans);
        }

        private void ViewDisciplineTitleTextChangedExecute(object sender, EventArgs e)
        {
            View.RPDName = $"{View.DisciplineTitle} ({Plan.Filename})";
        }

        private void ViewLessonTypesSelectExecute(object sender, EventArgs e)
        {
            View.ThematicContents = View.ThematicContentsDictionary[View.SelectedLessonType];
        }

        private void ViewCompetenceStageBeforeSelectExecute(object sender, EventArgs e)
        {
            if (View.SelectedCompetence is null)
            {
                return;
            }

            var decanatCompetenceId = View.SelectedCompetenceNode.Parent?.Tag as int?;

            if (decanatCompetenceId.HasValue && decanatCompetenceId != null)
            {
                var decanatCompetence = View.Competences.FirstOrDefault(DecanatCompetence => DecanatCompetence.Key.Id.Equals(decanatCompetenceId)).Key;

                var competence = View.SelectedCompetence;

                if (View.SelectedCompetenceNode.Text.Equals("Знать") || View.SelectedCompetenceNode.Text.Equals($"{decanatCompetence.Code}.1"))
                {
                    competence.Knowledge = View.CompetenceStage;
                }
                else if (View.SelectedCompetenceNode.Text.Equals("Уметь") || View.SelectedCompetenceNode.Text.Equals($"{decanatCompetence.Code}.2"))
                {
                    competence.Skill = View.CompetenceStage;
                }
                else if (View.SelectedCompetenceNode.Text.Equals("Владеть") || View.SelectedCompetenceNode.Text.Equals($"{decanatCompetence.Code}.3"))
                {
                    competence.Possession = View.CompetenceStage;
                }

                View.SelectedCompetence = competence;
            }
        }

        private void ViewCompetenceNameBeforeSelectExecute(object sender, EventArgs e)
        {
            if (View.SelectedCompetence is null)
            {
                return;
            }

            var decanatCompetenceId = View.SelectedCompetenceNode.Parent?.Tag as int?;

            if (decanatCompetenceId.HasValue && decanatCompetenceId != null)
            {
                var decanatCompetence = View.Competences.FirstOrDefault(DecanatCompetence => DecanatCompetence.Key.Id.Equals(decanatCompetenceId)).Key;

                var competence = View.SelectedCompetence;

                if (View.SelectedCompetenceNode.Text.Equals("Знать") || View.SelectedCompetenceNode.Text.Equals($"{decanatCompetence.Code}.1"))
                {
                    competence.KnowledgeName = View.CompetenceName;
                }
                else if (View.SelectedCompetenceNode.Text.Equals("Уметь") || View.SelectedCompetenceNode.Text.Equals($"{decanatCompetence.Code}.2"))
                {
                    competence.SkillName = View.CompetenceName;
                }
                else if (View.SelectedCompetenceNode.Text.Equals("Владеть") || View.SelectedCompetenceNode.Text.Equals($"{decanatCompetence.Code}.3"))
                {
                    competence.PossessionName = View.CompetenceName;
                }

                View.SelectedCompetence = competence;
            }
        }

        private void ViewCompetenceSelectionChanged(object sender, TreeViewEventArgs e)
        {
            var decanatCompetenceId = View.SelectedCompetenceNode.Parent?.Tag as int?;

            if (decanatCompetenceId.HasValue && decanatCompetenceId != null)
            {
                var decanatCompetence = View.Competences.FirstOrDefault(DecanatCompetence => DecanatCompetence.Key.Id.Equals(decanatCompetenceId)).Key;

                if (View.SelectedCompetenceNode.Text.Equals("Знать") || View.SelectedCompetenceNode.Text.Equals($"{decanatCompetence.Code}.1"))
                {
                    View.CompetenceName = View.SelectedCompetence.KnowledgeName;
                    View.CompetenceStage = View.SelectedCompetence.Knowledge;
                }
                else if (View.SelectedCompetenceNode.Text.Equals("Уметь") || View.SelectedCompetenceNode.Text.Equals($"{decanatCompetence.Code}.2"))
                {
                    View.CompetenceName = View.SelectedCompetence.SkillName;
                    View.CompetenceStage = View.SelectedCompetence.Skill;
                }
                else if (View.SelectedCompetenceNode.Text.Equals("Владеть") || View.SelectedCompetenceNode.Text.Equals($"{decanatCompetence.Code}.3"))
                {
                    View.CompetenceName = View.SelectedCompetence.PossessionName;
                    View.CompetenceStage = View.SelectedCompetence.Possession;
                }
            }
        }
    }
}