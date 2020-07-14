using Account.DAL.Entities;
using System.Collections.Generic;
using System.IO;

namespace AccountRPD.BL.Interfaces
{
    public interface IRPDExportStrategy : IExportable
    {
        void Create(string filePath, IEnumerable<RPDItem> rpdItems);
        void Include(RPD rpd, DecanatDepartment decanatDepartment, IEnumerable<Member> members);
        void IncludeTableOfContents();
        void Include(IEnumerable<RPDContent> rpdContents);
        void Include(IDictionary<DecanatCompetence, Competence> competences);
        void Include(DecanatPlan plan, IEnumerable<RPDContent> rpdContents, DecanatHoursDivision hoursDivision, IDictionary<string, string> formsOfControl, IEnumerable<int> courses, IEnumerable<int> semesters);
        void Include(IEnumerable<ThematicPlan> thematicPlans, string lectures, string practices, string labs, string individualWorks);
        void Include(IDictionary<string, IEnumerable<ThematicContent>> thematicContents);
        void Include(Stream stream);
        void Include(string text);
        void Include(IEnumerable<BasicLiterature> basicLiteratures, IEnumerable<AdditionalLiterature> additionalLiteratures);
        void Include(IEnumerable<LibrarySystem> librarySystems, IEnumerable<InternetResource> internetResources);
        void Include(IEnumerable<License> licenses);
        void Include(IEnumerable<MaterialBase> materialBases);
    }
}
