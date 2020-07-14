using Account.DAL.Entities;
using AccountRPD.BL.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountRPD.BL.Interfaces
{
    public interface IRPDService
    {
        IEnumerable<TeacherDTO> GetDepartmentTeachers(int departmentId, int? rpdId = null);
        TeacherDTO GetRPDAuthor(int rpdId);
        IEnumerable<string> GetRPDTypes();
        RpdType GetRPDTypeByDescription(string description);
        string GetDescriptionByRPDType(RpdType rpdType);
        MemberType GetMemberType(string userId);
        KeyValuePair<string, string>[] GetDisciplineHoursDivision(int disciplineId, int planId);
        IDictionary<DecanatCompetence, Competence> GetCompetences(int disciplineId, int planId);
        IDictionary<DecanatCompetence, Competence> GetCompetencesByRPD(int disciplineId, int planId, int rpdId);
        IDictionary<DecanatCompetence, IEnumerable<CompetenceGrade>> GetCompetenceGrades(int disciplineId, int planId, int educationStandardId);
        IEnumerable<ThematicPlan> GetUpdatedThematicPlans(IList<ThematicPlan> previousThematicPlans);
        IEnumerable<ThematicPlan> GetThematicPlans(int RPDId);
        string GetWorkHourDivision(int disciplineId, int planId, string workTypeName);
        (string sectionNumber, double lectures, double practices, double labs, double individualWorks) GetTotalHoursForSection(IList<ThematicPlan> thematicPlans, string sectionNumber);
        (string lectures, string practices, string labs, string individualWorks) GetTotalHoursForThematicPlans(IList<ThematicPlan> thematicPlans);
        IEnumerable<string> GetLessonsTypes(int disciplineId, int planId);
        Task<IEnumerable<Member>> GetRPDsMembersAsync(int RPDId);
        Task<IEnumerable<ThematicContent>> GetRPDsThematicContentsByLessonTypeAsync(int RPDId, string lessonType);
        RPD GetRPD(int RPDId);
        Task<IEnumerable<RPDContent>> GetRPDContentsAsync(int RPDId);
        Task<IDictionary<string, int>> GetEducationStandartRPDItemsAsync(int educationStandartId, int documentTypeId);
        Task<IEnumerable<RPDItem>> GetRPDItemsAsync(int educationStandartId, int documentTypeId);
        DecanatHoursDivision GetDecanatHoursDivision(int planId, int disciplineId);
        IEnumerable<int> GetCoursesByPlan(int planId, int disciplineId);
        IEnumerable<int> GetSemestersByPlan(int planId, int disciplineId);
        IDictionary<string, string> GetFormsOfControl(int planId, int disciplineId);
        Task<IEnumerable<BasicLiterature>> GetRPDsBasicLiteraturesAsync(int RPDId);
        Task<IEnumerable<AdditionalLiterature>> GetRPDsAdditionalLiteraturesAsync(int RPDId);
        Task<IEnumerable<LibrarySystem>> GetRPDsLibrarySystemsAsync(int RPDId);
        Task<IEnumerable<InternetResource>> GetRPDsInternetResourcesAsync(int RPDId);
        Task<IEnumerable<License>> GetRPDsLicencesAsync(int RPDId);
        Task<IEnumerable<MaterialBase>> GetRPDsMaterialBasesAsync(int RPDId);
        Task<RPDContent> GetRPDContentAsync(int RPDId, int rpdItemId);
        Task<double> GetFullnessPercentAsync(int rpdId);
        void CommitChanges(RPD rpd);
        Task CommitChanges(RPD RPD, IEnumerable<Member> members);
        void CommitChanges(RPD RPD, IEnumerable<Competence> competences);
        void CommitChanges(RPD RPD, RPDContent content);
        void CommitChanges(RPD RPD, IEnumerable<ThematicPlan> thematicPlans);
        void CommitChanges(RPD RPD, IEnumerable<ThematicContent> thematicContents);
        Task CommitChanges(RPD RPD, IEnumerable<BasicLiterature> basicLiteratures);
        Task CommitChanges(RPD RPD, IEnumerable<AdditionalLiterature> additionalLiteratures);
        Task CommitChanges(RPD RPD, IEnumerable<LibrarySystem> librarySystems);
        Task CommitChanges(RPD RPD, IEnumerable<InternetResource> internetResources);
        Task CommitChanges(RPD RPD, IEnumerable<License> licenses);
        Task CommitChanges(RPD RPD, IEnumerable<MaterialBase> materialBases);
        void CommitChanges(RPD RPD, IEnumerable<CompetenceGrade> competenceGrades);
        Task SaveAsync();
    }
}