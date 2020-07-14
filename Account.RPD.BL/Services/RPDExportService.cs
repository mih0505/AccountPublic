using Account.DAL.Entities;
using AccountRPD.BL.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace AccountRPD.BL.Services
{
    public class RPDExportService : IRPDExportService
    {
        private IRPDExportStrategy exportStrategy;

        public IRPDExportService SetExportStrategy(IRPDExportStrategy exportStrategy)
        {
            this.exportStrategy = exportStrategy;

            return this;
        }

        public IRPDExportService Create(string filePath, IEnumerable<RPDItem> rpdItems)
        {
            exportStrategy.Create(filePath, rpdItems);

            return this;
        }

        public IRPDExportService IncludeTableOfContents()
        {
            exportStrategy.IncludeTableOfContents();

            return this;
        }

        public IRPDExportService Include(RPD rpd, DecanatDepartment decanatDepartment, IEnumerable<Member> members)
        {
            exportStrategy.Include(rpd, decanatDepartment, members);

            return this;
        }

        public IRPDExportService Include(IEnumerable<RPDContent> rpdContents)
        {
            exportStrategy.Include(rpdContents);

            return this;
        }

        public IRPDExportService Include(IDictionary<DecanatCompetence, Competence> competences)
        {
            exportStrategy.Include(competences);

            return this;
        }

        public IRPDExportService Include(DecanatPlan plan, IEnumerable<RPDContent> rpdContents, DecanatHoursDivision hoursDivision, IDictionary<string, string> formsOfControl, IEnumerable<int> courses, IEnumerable<int> semesters)
        {
            exportStrategy.Include(plan, rpdContents, hoursDivision, formsOfControl, courses, semesters);

            return this;
        }

        public IRPDExportService Include(IEnumerable<ThematicPlan> thematicPlans, string lectures, string practices, string labs, string individualWorks)
        {
            exportStrategy.Include(thematicPlans, lectures, practices, labs, individualWorks);

            return this;
        }

        public IRPDExportService Include(IDictionary<string, IEnumerable<ThematicContent>> thematicContents)
        {
            exportStrategy.Include(thematicContents);

            return this;
        }

        public IRPDExportService Include(Stream stream)
        {
            exportStrategy.Include(stream);

            return this;
        }

        public IRPDExportService Include(string text)
        {
            exportStrategy.Include(text);

            return this;
        }

        public IRPDExportService Include(IEnumerable<BasicLiterature> basicLiteratures, IEnumerable<AdditionalLiterature> additionalLiteratures)
        {
            exportStrategy.Include(basicLiteratures, additionalLiteratures);

            return this;
        }

        public IRPDExportService Include(IEnumerable<LibrarySystem> librarySystems, IEnumerable<InternetResource> internetResources)
        {
            exportStrategy.Include(librarySystems, internetResources);

            return this;
        }

        public IRPDExportService Include(IEnumerable<License> licenses)
        {
            exportStrategy.Include(licenses);

            return this;
        }

        public IRPDExportService Include(IEnumerable<MaterialBase> materialBases)
        {
            exportStrategy.Include(materialBases);

            return this;
        }

        public void Export()
        {
            exportStrategy.Export();
        }
    }
}
