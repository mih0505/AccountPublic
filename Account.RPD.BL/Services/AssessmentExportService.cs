using Account.DAL.Entities;
using AccountRPD.BL.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace AccountRPD.BL.Services
{
    public class AssessmentExportService : IAssessmentExportService
    {
        private IAssessmentExportStrategy exportStrategy;

        public IAssessmentExportService SetExportStrategy(IAssessmentExportStrategy exportStrategy)
        {
            this.exportStrategy = exportStrategy;

            return this;
        }

        public IAssessmentExportService Create(string filePath, IEnumerable<RPDItem> assessmentItems)
        {
            exportStrategy.Create(filePath, assessmentItems);

            return this;
        }

        public IAssessmentExportService Include(RPD rpd, DecanatDepartment decanatDepartment, IEnumerable<Member> members)
        {
            exportStrategy.Include(rpd, decanatDepartment, members);

            return this;
        }

        public IAssessmentExportService IncludeTableOfContents()
        {
            exportStrategy.IncludeTableOfContents();

            return this;
        }

        public IAssessmentExportService Include(IDictionary<DecanatCompetence, IEnumerable<CompetenceGrade>> competenceGradesDictionary, IDictionary<DecanatCompetence, Competence> competences)
        {
            exportStrategy.Include(competenceGradesDictionary, competences);

            return this;
        }

        public IAssessmentExportService IncludeAssessmentEquipment(Stream stream)
        {
            exportStrategy.IncludeAssessmentEquipment(stream);

            return this;
        }

        public IAssessmentExportService IncludeAssessmentEquipment(string text)
        {
            exportStrategy.IncludeAssessmentEquipment(text);

            return this;
        }

        public IAssessmentExportService IncludeMaterials(Stream stream, IDictionary<string, string> formsOfControl)
        {
            exportStrategy.IncludeMaterials(stream, formsOfControl);

            return this;
        }

        public IAssessmentExportService IncludeMaterials(string text, IDictionary<string, string> formsOfControl)
        {
            exportStrategy.IncludeMaterials(text, formsOfControl);

            return this;
        }

        public void Export()
        {
            exportStrategy.Export();
        }
    }
}
