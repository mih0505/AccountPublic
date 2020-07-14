using Account.DAL.Entities;
using Account.DAL.Interfaces;
using AccountRPD.BL.DTOs;
using AccountRPD.BL.Infrastructure;
using AccountRPD.BL.Interfaces;
using Accounts.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Competence = Account.DAL.Entities.Competence;
using License = Account.DAL.Entities.License;

namespace AccountRPD.BL.Services
{
    public class RPDService : IRPDService
    {
        private readonly IDecanatUnitOfWork decanatUnitOfWork;
        private readonly IEFUnitOfWork efUnitOfWork;

        public RPDService(IDecanatUnitOfWork decanatUnitOfWork, IEFUnitOfWork efUnitOfWork)
        {
            this.decanatUnitOfWork = decanatUnitOfWork;
            this.efUnitOfWork = efUnitOfWork;
        }

        public IEnumerable<TeacherDTO> GetDepartmentTeachers(int departmentId, int? rpdId = null)
        {
            var session = Session.GetSession();
            var departmentTeachers = efUnitOfWork.TeacherDepartments.GetDepartmentTeachers(departmentId);
            var departmentTeachersAsList = departmentTeachers as IList<ApplicationUser>;

            var isContainsCurrentUser = departmentTeachers.FirstOrDefault(Teacher => Teacher.Id.Equals(session.User.Id)) != null;

            if (!isContainsCurrentUser)
            {
                departmentTeachersAsList.Insert(0, session.User);
            }

            if (rpdId != null && rpdId.HasValue)
            {
                var rpdAuthor = efUnitOfWork.RPDs.GetRPDAuthor(rpdId.Value);

                if (!rpdAuthor.Id.Equals(session.User.Id))
                {
                    var isContainsAuthor = departmentTeachers.FirstOrDefault(Teacher => Teacher.Id.Equals(rpdAuthor.Id)) != null;

                    if (!isContainsAuthor)
                    {
                        departmentTeachersAsList.Insert(1, rpdAuthor);
                    }
                }
            }

            var teacherDTOs = departmentTeachers.Select(Teacher => new TeacherDTO()
            {
                Id = Teacher.Id,
                Shortname = $"{Teacher.Lastname} {Teacher.Firstname[0]}. {Teacher.Middlename[0]}"
            });

            return teacherDTOs.ToList();
        }

        public IEnumerable<string> GetRPDTypes()
        {
            var rpdTypesAsLines = new List<string>();
            var rpdTypesAsEnum = Enum.GetValues(typeof(RpdType)).Cast<RpdType>().ToList();

            foreach (var rpdType in rpdTypesAsEnum)
            {
                var description = GetDescriptionByRPDType(rpdType);
                rpdTypesAsLines.Add(description);
            }

            return rpdTypesAsLines;
        }

        public RpdType GetRPDTypeByDescription(string description)
        {
            var rpdTypesAsEnum = Enum.GetValues(typeof(RpdType)).Cast<RpdType>().ToList();

            foreach (var rpdType in rpdTypesAsEnum)
            {
                var rpdTypeDescription = GetDescriptionByRPDType(rpdType);

                if (rpdTypeDescription.Equals(description))
                {
                    return rpdType;
                }
            }

            return default;
        }

        public string GetDescriptionByRPDType(RpdType rpdType)
        {
            var name = rpdType.ToString();
            var field = typeof(RpdType).GetField(name);
            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (!attributes.Length.Equals(0))
            {
                return attributes.First().Description;
            }

            return default;
        }

        public TeacherDTO GetRPDAuthor(int rpdId)
        {
            var teacher = efUnitOfWork.RPDs.GetRPDAuthor(rpdId);

            var teacherDTO = new TeacherDTO()
            {
                Id = teacher.Id,
                Shortname = $"{teacher.Lastname} {teacher.Firstname[0]}. {teacher.Middlename[0]}"
            };

            return teacherDTO;
        }

        public MemberType GetMemberType(string userId)
        {
            var isManager = efUnitOfWork.UserManager.IsInRole(userId, "DepartmentsManagers");
            return (isManager) ? MemberType.Manager : MemberType.Compiler;
        }

        public KeyValuePair<string, string>[] GetDisciplineHoursDivision(int disciplineId, int planId)
        {
            return decanatUnitOfWork.Disciplines.GetDisciplineHoursDivision(planId, disciplineId).Values.ToArray();
        }

        public DecanatHoursDivision GetDecanatHoursDivision(int planId, int disciplineId)
        {
            return decanatUnitOfWork.Disciplines.GetDisciplineHoursDivision(planId, disciplineId);
        }

        public IEnumerable<int> GetCoursesByPlan(int planId, int disciplineId)
        {
            return decanatUnitOfWork.Disciplines.GetDisciplineCoursesByPlan(planId, disciplineId);
        }

        public IEnumerable<int> GetSemestersByPlan(int planId, int disciplineId)
        {
            return decanatUnitOfWork.Disciplines.GetDisciplineSemestersByPlan(planId, disciplineId);
        }

        public IDictionary<string, string> GetFormsOfControl(int planId, int disciplineId)
        {
            return decanatUnitOfWork.Disciplines.GetDisciplineControlSemesters(planId, disciplineId);
        }

        public IDictionary<DecanatCompetence, Competence> GetCompetences(int disciplineId, int planId)
        {
            var competences = new Dictionary<DecanatCompetence, Competence>();

            var decanatCompetences = decanatUnitOfWork.Competences.GetAllByDiscipline(disciplineId, planId);

            foreach (var decanatCompetence in decanatCompetences)
            {
                var competenceDescription = new Competence
                {
                    DecanatId = decanatCompetence.Id
                };

                competences.Add(decanatCompetence, competenceDescription);
            }

            return competences;
        }

        public IDictionary<DecanatCompetence, Competence> GetCompetencesByRPD(int disciplineId, int planId, int rpdId)
        {
            var competences = new Dictionary<DecanatCompetence, Competence>();

            var decanatCompetences = decanatUnitOfWork.Competences.GetAllByDiscipline(disciplineId, planId);

            foreach (var decanatCompetence in decanatCompetences)
            {
                var competenceDescription = efUnitOfWork.Competences.GetCompetenceDescriptionByRPD(decanatCompetence.Id, rpdId) ?? new Competence();
                competenceDescription.DecanatId = decanatCompetence.Id;
                competences.Add(decanatCompetence, competenceDescription);
            }

            return competences;
        }

        public IDictionary<DecanatCompetence, IEnumerable<CompetenceGrade>> GetCompetenceGrades(int disciplineId, int planId, int educationStandardId)
        {
            var competenceGrades = new Dictionary<DecanatCompetence, IEnumerable<CompetenceGrade>>();

            var decanatCompetences = decanatUnitOfWork.Competences.GetAllByDiscipline(disciplineId, planId);

            foreach (var decanatCompetence in decanatCompetences)
            {
                var competenceGrade = efUnitOfWork.CompetenceGrades.GetCompetenceGrades(decanatCompetence.Id);

                if (competenceGrade.Count().Equals(0))
                {
                    competenceGrade = new BindingList<CompetenceGrade>()
                    {
                        new CompetenceGrade()
                        {
                            DecanatId = decanatCompetence.Id,
                            Stage = (educationStandardId.Equals(1)) ? "1 этап: Знания" : $"{decanatCompetence.Code}.1"
                        },
                        new CompetenceGrade()
                        {
                            DecanatId = decanatCompetence.Id,
                            Stage = (educationStandardId.Equals(1)) ? "2 этап: Умения" : $"{decanatCompetence.Code}.2"
                        },
                        new CompetenceGrade()
                        {
                            DecanatId = decanatCompetence.Id,
                            Stage = (educationStandardId.Equals(1)) ? "3 этап: Владения (навыки / опыт деятельности)" : $"{decanatCompetence.Code}.3"
                        }
                    };
                }

                competenceGrades.Add(decanatCompetence, competenceGrade);
            }

            return competenceGrades;
        }

        public IEnumerable<ThematicPlan> GetUpdatedThematicPlans(IList<ThematicPlan> previousThematicPlans)
        {
            var sectionCounter = 0;
            var topicCounter = 0;

            foreach (var thematicPlan in previousThematicPlans)
            {
                if (thematicPlan.IsSection)
                {
                    sectionCounter++;
                    topicCounter = 0;

                    thematicPlan.Number = sectionCounter.ToString();
                }
                else
                {
                    topicCounter++;

                    thematicPlan.Number = $"{sectionCounter}.{topicCounter}";
                }
            }

            return previousThematicPlans;
        }

        public async Task<double> GetFullnessPercentAsync(int rpdId)
        {
            var rpd = GetRPD(rpdId);

            var lessonsTypes = GetLessonsTypes(rpd.DecanatDisciplineId, rpd.DecanatPlanId);
            var thematicContents = new List<ThematicContent>();

            foreach (var lessonType in lessonsTypes)
            {
                var values = await GetRPDsThematicContentsByLessonTypeAsync(rpd.Id, lessonType);
                thematicContents.AddRange(values);
            }

            var members = await GetRPDsMembersAsync(rpdId);
            var rpdContents = await GetRPDContentsAsync(rpd.Id);
            var competences = GetCompetencesByRPD(rpd.DecanatDisciplineId, rpd.DecanatPlanId, rpdId);
            var basicLiteratures = await GetRPDsBasicLiteraturesAsync(rpdId) as IList<BasicLiterature>;
            var additionalLiteratures = await GetRPDsAdditionalLiteraturesAsync(rpdId) as IList<AdditionalLiterature>;
            var librarySystems = await GetRPDsLibrarySystemsAsync(rpdId) as IList<LibrarySystem>;
            var internetResources = await GetRPDsInternetResourcesAsync(rpdId) as IList<InternetResource>;
            var licenses = await GetRPDsLicencesAsync(rpdId) as IList<License>;
            var materialBases = await GetRPDsMaterialBasesAsync(rpdId) as IList<MaterialBase>;

            var count = 0.0;

            if (!string.IsNullOrEmpty(rpd.DisciplineName))
            {
                count++;
            }

            if (!string.IsNullOrEmpty(rpd.DepartmentManager))
            {
                count++;
            }

            if (!string.IsNullOrEmpty(rpd.ProtocolNumberRPD))
            {
                count++;
            }

            if (!members.Count().Equals(0))
            {
                count++;
            }

            if (!competences.Count().Equals(0))
            {
                count++;
            }

            if (!rpdContents.Count().Equals(0))
            {
                count++;
            }

            if (!thematicContents.Count().Equals(0))
            {
                count++;
            }

            if (!basicLiteratures.Count().Equals(0))
            {
                count++;
            }

            if (!additionalLiteratures.Count().Equals(0))
            {
                count++;
            }

            if (!librarySystems.Count().Equals(0))
            {
                count++;
            }

            if (!internetResources.Count().Equals(0))
            {
                count++;
            }

            if (!licenses.Count().Equals(0))
            {
                count++;
            }

            if (!materialBases.Count().Equals(0))
            {
                count++;
            }

            return count / 13.0;
        }

        public void CommitChanges(RPD rpd)
        {
            if (rpd.Id.Equals(0))
            {
                efUnitOfWork.RPDs.Add(rpd);
            }
            else
            {
                efUnitOfWork.Update(rpd);
            }
        }

        public async Task CommitChanges(RPD RPD, IEnumerable<Member> members)
        {
            var membersToRemove = new List<Member>();

            if (!RPD.Id.Equals(0))
            {
                var oldMembers = await GetRPDsMembersAsync(RPD.Id);

                foreach (var oldMember in oldMembers as IList<Member>)
                {
                    var isDeleted = members.FirstOrDefault(Member => Member.Id.Equals(oldMember.Id)) is null;

                    if (isDeleted)
                    {
                        membersToRemove.Add(oldMember);
                    }
                }
            }

            efUnitOfWork.Members.RemoveRange(membersToRemove);

            foreach (var member in members)
            {
                if (member.Id.Equals(0))
                {
                    member.RPD = RPD;
                    efUnitOfWork.Members.Add(member);
                }
                else
                {
                    efUnitOfWork.Update(member);
                }
            }
        }

        public void CommitChanges(RPD RPD, IEnumerable<Competence> competences)
        {
            foreach (var competence in competences)
            {
                if (competence.Id.Equals(0))
                {
                    competence.RPD = RPD;
                    efUnitOfWork.Competences.Add(competence);
                }
                else
                {
                    efUnitOfWork.Update(competence);
                }
            }
        }

        public void CommitChanges(RPD RPD, RPDContent content)
        {
            if (content.Id.Equals(0))
            {
                content.RPD = RPD;
                efUnitOfWork.RPDContents.Add(content);
            }
            else
            {
                efUnitOfWork.Update(content);
            }
        }

        public void CommitChanges(RPD RPD, IEnumerable<ThematicPlan> thematicPlans)
        {
            foreach (var thematicPlan in thematicPlans)
            {
                if (thematicPlan.Id.Equals(0))
                {
                    thematicPlan.RPD = RPD;
                    efUnitOfWork.ThematicPlans.Add(thematicPlan);
                }
                else
                {
                    efUnitOfWork.Update(thematicPlan);
                }
            }
        }

        public void CommitChanges(RPD RPD, IEnumerable<ThematicContent> thematicContents)
        {
            foreach (var thematicContent in thematicContents)
            {
                if (thematicContent.Id.Equals(0))
                {
                    thematicContent.RPD = RPD;
                    efUnitOfWork.ThematicContents.Add(thematicContent);
                }
                else
                {
                    efUnitOfWork.Update(thematicContent);
                }
            }
        }

        public async Task CommitChanges(RPD RPD, IEnumerable<BasicLiterature> basicLiteratures)
        {
            var basicLiteraturesToRemove = new List<BasicLiterature>();

            if (!RPD.Id.Equals(0))
            {
                var oldBasicLiteratures = await GetRPDsBasicLiteraturesAsync(RPD.Id);

                foreach (var oldBasicLiterature in oldBasicLiteratures as IList<BasicLiterature>)
                {
                    var isDeleted = basicLiteratures.FirstOrDefault(Member => Member.Id.Equals(oldBasicLiterature.Id)) is null;

                    if (isDeleted)
                    {
                        basicLiteraturesToRemove.Add(oldBasicLiterature);
                    }
                }
            }

            efUnitOfWork.BasicLiteratures.RemoveRange(basicLiteraturesToRemove);

            foreach (var basicLiterature in basicLiteratures)
            {
                if (basicLiterature.Id.Equals(0))
                {
                    basicLiterature.RPD = RPD;
                    efUnitOfWork.BasicLiteratures.Add(basicLiterature);
                }
                else
                {
                    efUnitOfWork.Update(basicLiterature);
                }
            }
        }

        public async Task CommitChanges(RPD RPD, IEnumerable<AdditionalLiterature> additionalLiteratures)
        {
            var additionalLiteraturesToRemove = new List<AdditionalLiterature>();

            if (!RPD.Id.Equals(0))
            {
                var oldAdditionalLiteratures = await GetRPDsAdditionalLiteraturesAsync(RPD.Id);

                foreach (var oldAdditionalLiterature in oldAdditionalLiteratures as IList<AdditionalLiterature>)
                {
                    var isDeleted = additionalLiteratures.FirstOrDefault(Member => Member.Id.Equals(oldAdditionalLiterature.Id)) is null;

                    if (isDeleted)
                    {
                        additionalLiteraturesToRemove.Add(oldAdditionalLiterature);
                    }
                }
            }

            efUnitOfWork.AdditionalLiteratures.RemoveRange(additionalLiteraturesToRemove);

            foreach (var additionalLiterature in additionalLiteratures)
            {
                if (additionalLiterature.Id.Equals(0))
                {
                    additionalLiterature.RPD = RPD;
                    efUnitOfWork.AdditionalLiteratures.Add(additionalLiterature);
                }
                else
                {
                    efUnitOfWork.Update(additionalLiterature);
                }
            }
        }

        public async Task CommitChanges(RPD RPD, IEnumerable<LibrarySystem> librarySystems)
        {
            var librarySystemsToRemove = new List<LibrarySystem>();

            if (!RPD.Id.Equals(0))
            {
                var oldLibrarySystems = await GetRPDsLibrarySystemsAsync(RPD.Id);

                foreach (var oldLibrarySystem in oldLibrarySystems as IList<LibrarySystem>)
                {
                    var isDeleted = librarySystems.FirstOrDefault(Member => Member.Id.Equals(oldLibrarySystem.Id)) is null;

                    if (isDeleted)
                    {
                        librarySystemsToRemove.Add(oldLibrarySystem);
                    }
                }
            }

            efUnitOfWork.LibrarySystems.RemoveRange(librarySystemsToRemove);

            foreach (var librarySystem in librarySystems)
            {
                if (librarySystem.Id.Equals(0))
                {
                    librarySystem.RPD = RPD;
                    efUnitOfWork.LibrarySystems.Add(librarySystem);
                }
                else
                {
                    efUnitOfWork.Update(librarySystem);
                }
            }
        }

        public async Task CommitChanges(RPD RPD, IEnumerable<InternetResource> internetResources)
        {
            var internetResourcesToRemove = new List<InternetResource>();

            if (!RPD.Id.Equals(0))
            {
                var oldInternetResources = await GetRPDsInternetResourcesAsync(RPD.Id);

                foreach (var oldInternetResource in oldInternetResources as IList<InternetResource>)
                {
                    var isDeleted = internetResources.FirstOrDefault(Member => Member.Id.Equals(oldInternetResource.Id)) is null;

                    if (isDeleted)
                    {
                        internetResourcesToRemove.Add(oldInternetResource);
                    }
                }
            }

            efUnitOfWork.InternetResources.RemoveRange(internetResourcesToRemove);

            foreach (var internetResource in internetResources)
            {
                if (internetResource.Id.Equals(0))
                {
                    internetResource.RPD = RPD;
                    efUnitOfWork.InternetResources.Add(internetResource);
                }
                else
                {
                    efUnitOfWork.Update(internetResource);
                }
            }
        }

        public async Task CommitChanges(RPD RPD, IEnumerable<License> licenses)
        {
            var licensesToRemove = new List<License>();

            if (!RPD.Id.Equals(0))
            {
                var oldLicenses = await GetRPDsLicencesAsync(RPD.Id);

                foreach (var oldLicense in oldLicenses as IList<License>)
                {
                    var isDeleted = licenses.FirstOrDefault(Member => Member.Id.Equals(oldLicense.Id)) is null;

                    if (isDeleted)
                    {
                        licensesToRemove.Add(oldLicense);
                    }
                }
            }

            efUnitOfWork.Licences.RemoveRange(licensesToRemove);

            foreach (var license in licenses)
            {
                if (license.Id.Equals(0))
                {
                    license.RPD = RPD;
                    efUnitOfWork.Licences.Add(license);
                }
                else
                {
                    efUnitOfWork.Update(license);
                }
            }
        }

        public async Task CommitChanges(RPD RPD, IEnumerable<MaterialBase> materialBases)
        {
            var materialBasesToRemove = new List<MaterialBase>();

            if (!RPD.Id.Equals(0))
            {
                var oldMaterialBases = await GetRPDsMaterialBasesAsync(RPD.Id);

                foreach (var oldMaterialBase in oldMaterialBases as IList<MaterialBase>)
                {
                    var isDeleted = materialBases.FirstOrDefault(Member => Member.Id.Equals(oldMaterialBase.Id)) is null;

                    if (isDeleted)
                    {
                        materialBasesToRemove.Add(oldMaterialBase);
                    }
                }
            }

            efUnitOfWork.MaterialBases.RemoveRange(materialBasesToRemove);

            foreach (var materialBase in materialBases)
            {
                if (materialBase.Id.Equals(0))
                {
                    materialBase.RPD = RPD;
                    efUnitOfWork.MaterialBases.Add(materialBase);
                }
                else
                {
                    efUnitOfWork.Update(materialBase);
                }
            }
        }

        public void CommitChanges(RPD RPD, IEnumerable<CompetenceGrade> competenceGrades)
        {
            foreach (var competenceGrade in competenceGrades)
            {
                if (competenceGrade.Id.Equals(0))
                {
                    competenceGrade.RPD = RPD;
                    efUnitOfWork.CompetenceGrades.Add(competenceGrade);
                }
                else
                {
                    efUnitOfWork.Update(competenceGrade);
                }
            }
        }

        public async Task SaveAsync()
        {
            await efUnitOfWork.SaveAsync();
        }

        public IEnumerable<ThematicPlan> GetThematicPlans(int RPDId)
        {
            return efUnitOfWork.ThematicPlans.GetRPDsThematicPlans(RPDId);
        }

        public string GetWorkHourDivision(int disciplineId, int planId, string workTypeName)
        {
            return decanatUnitOfWork.Disciplines.GetWorkHourDivision(planId, disciplineId, workTypeName);
        }

        public (string sectionNumber, double lectures, double practices, double labs, double individualWorks) GetTotalHoursForSection(IList<ThematicPlan> thematicPlans, string thematicPlanNumber)
        {
            var thematicPlan = thematicPlans.FirstOrDefault(ThematicPlan => ThematicPlan.Number.Equals(thematicPlanNumber));

            var sectionNumber = string.Empty;

            if (thematicPlan.IsSection)
            {
                sectionNumber = thematicPlanNumber;
            }
            else
            {
                sectionNumber = thematicPlanNumber.Split('.')[0];
            }

            var themesInSection = thematicPlans.Where(ThematicPlan => !ThematicPlan.IsSection && ThematicPlan.Number.Split('.')[0].Equals(sectionNumber));

            var tuple =
            (
                sectionNumber: sectionNumber,
                lectures: themesInSection.Sum(Section => Section.Lecture),
                practices: themesInSection.Sum(Section => Section.Practice),
                labs: themesInSection.Sum(Section => Section.Lab),
                individualWorks: themesInSection.Sum(Section => Section.IndividualWork)
            );

            return tuple;
        }

        public (string lectures, string practices, string labs, string individualWorks) GetTotalHoursForThematicPlans(IList<ThematicPlan> thematicPlans)
        {
            var sections = thematicPlans.Where(ThematicPlan => ThematicPlan.IsSection);

            var tuple =
            (
                lectures: sections.Sum(Section => Section.Lecture).ToString("0.#"),
                practices: sections.Sum(Section => Section.Practice).ToString("0.#"),
                labs: sections.Sum(Section => Section.Lab).ToString("0.#"),
                individualWorks: sections.Sum(Section => Section.IndividualWork).ToString("0.#")
            );

            return tuple;
        }

        public IEnumerable<string> GetLessonsTypes(int disciplineId, int planId)
        {
            var lessonsTypes = new List<string>();

            var lecture = GetWorkHourDivision(disciplineId, planId, "Лек");

            if (!lecture.Equals("0"))
            {
                lessonsTypes.Add("Курс лекционных занятий");
            }

            var practice = GetWorkHourDivision(disciplineId, planId, "Прак");

            if (!practice.Equals("0"))
            {
                lessonsTypes.Add("Курс практических/семинарских занятий");
            }

            var lab = GetWorkHourDivision(disciplineId, planId, "Лаб");

            if (!lab.Equals("0"))
            {
                lessonsTypes.Add("Курс лабораторных занятий");
            }

            return lessonsTypes;
        }

        public async Task<IEnumerable<Member>> GetRPDsMembersAsync(int RPDId)
        {
            return await efUnitOfWork.Members.GetRPDsMembersAsync(RPDId);
        }

        public async Task<IEnumerable<RPDContent>> GetRPDContentsAsync(int RPDId)
        {
            return await efUnitOfWork.RPDContents.GetRPDContentsAsync(RPDId);
        }

        public async Task<RPDContent> GetRPDContentAsync(int RPDId, int rpdItemId)
        {
            return await efUnitOfWork.RPDContents.GetRPDContentAsync(RPDId, rpdItemId);
        }

        public async Task<IDictionary<string, int>> GetEducationStandartRPDItemsAsync(int educationStandartId, int documentTypeId)
        {
            var rpdItems = await efUnitOfWork.RPDItems.GetEducationStandartRPDItemsAsync(educationStandartId, documentTypeId);
            var rpdItemsIds = new Dictionary<string, int>();

            foreach (var rpdItem in rpdItems)
            {
                if (!string.IsNullOrWhiteSpace(rpdItem.ControlName))
                {
                    rpdItemsIds.Add(rpdItem.ControlName, rpdItem.Id);
                }
            }

            return rpdItemsIds;
        }

        public async Task<IEnumerable<RPDItem>> GetRPDItemsAsync(int educationStandartId, int documentTypeId)
        {
            return await efUnitOfWork.RPDItems.GetEducationStandartRPDItemsAsync(educationStandartId, documentTypeId);
        }

        public RPD GetRPD(int RPDId)
        {
            return efUnitOfWork.RPDs.Get(RPDId);
        }

        public async Task<IEnumerable<ThematicContent>> GetRPDsThematicContentsByLessonTypeAsync(int RPDId, string lessonType)
        {
            return await efUnitOfWork.ThematicContents.GetRPDsThematicContentsByLessonType(RPDId, lessonType);
        }

        public async Task<IEnumerable<BasicLiterature>> GetRPDsBasicLiteraturesAsync(int RPDId)
        {
            return await efUnitOfWork.BasicLiteratures.GetRPDsBasicLiteraturesAsync(RPDId);
        }

        public async Task<IEnumerable<AdditionalLiterature>> GetRPDsAdditionalLiteraturesAsync(int RPDId)
        {
            return await efUnitOfWork.AdditionalLiteratures.GetRPDsAdditionalLiteraturesAsync(RPDId);
        }

        public async Task<IEnumerable<LibrarySystem>> GetRPDsLibrarySystemsAsync(int RPDId)
        {
            return await efUnitOfWork.LibrarySystems.GetRPDsLibrarySystemsAsync(RPDId);
        }

        public async Task<IEnumerable<InternetResource>> GetRPDsInternetResourcesAsync(int RPDId)
        {
            return await efUnitOfWork.InternetResources.GetRPDsInternetResourcesAsync(RPDId);
        }

        public async Task<IEnumerable<License>> GetRPDsLicencesAsync(int RPDId)
        {
            return await efUnitOfWork.Licences.GetRPDsLicensesAsync(RPDId);
        }

        public async Task<IEnumerable<MaterialBase>> GetRPDsMaterialBasesAsync(int RPDId)
        {
            return await efUnitOfWork.MaterialBases.GetRPDsMaterialBasesAsync(RPDId);
        }
    }
}
