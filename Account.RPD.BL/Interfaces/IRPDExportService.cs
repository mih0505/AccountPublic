using Account.DAL.Entities;
using System.Collections.Generic;
using System.IO;

namespace AccountRPD.BL.Interfaces
{
    public interface IRPDExportService : IExportable
    {
        IRPDExportService SetExportStrategy(IRPDExportStrategy exportStrategy);
        IRPDExportService Create(string filePath, IEnumerable<RPDItem> rpdItems);
        IRPDExportService Include(RPD rpd, DecanatDepartment decanatDepartment, IEnumerable<Member> members);
        IRPDExportService IncludeTableOfContents();
        IRPDExportService Include(IEnumerable<RPDContent> rpdContents);
        IRPDExportService Include(IDictionary<DecanatCompetence, Competence> competences);
        IRPDExportService Include(DecanatPlan plan, IEnumerable<RPDContent> rpdContents, DecanatHoursDivision hoursDivision, IDictionary<string, string> formsOfControl, IEnumerable<int> courses, IEnumerable<int> semesters);
        IRPDExportService Include(IEnumerable<ThematicPlan> thematicPlans, string lectures, string practices, string labs, string individualWorks);
        IRPDExportService Include(IDictionary<string, IEnumerable<ThematicContent>> thematicContents);
        IRPDExportService Include(Stream stream);
        IRPDExportService Include(string text);
        IRPDExportService Include(IEnumerable<BasicLiterature> basicLiteratures, IEnumerable<AdditionalLiterature> additionalLiteratures);
        IRPDExportService Include(IEnumerable<LibrarySystem> librarySystems, IEnumerable<InternetResource> internetResources);
        IRPDExportService Include(IEnumerable<License> licenses);
        IRPDExportService Include(IEnumerable<MaterialBase> materialBases);
    }
}
