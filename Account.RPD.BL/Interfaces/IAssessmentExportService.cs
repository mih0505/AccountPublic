using Account.DAL.Entities;
using System.Collections.Generic;
using System.IO;

namespace AccountRPD.BL.Interfaces
{
    public interface IAssessmentExportService : IExportable
    {
        IAssessmentExportService SetExportStrategy(IAssessmentExportStrategy exportStrategy);
        IAssessmentExportService Create(string filePath, IEnumerable<RPDItem> assessmentItems);
        IAssessmentExportService Include(RPD rpd, DecanatDepartment decanatDepartment, IEnumerable<Member> members);
        IAssessmentExportService IncludeTableOfContents();
        IAssessmentExportService Include(IDictionary<DecanatCompetence, IEnumerable<CompetenceGrade>> competenceGradesDictionary, IDictionary<DecanatCompetence, Competence> competences);
        IAssessmentExportService IncludeAssessmentEquipment(Stream stream);
        IAssessmentExportService IncludeAssessmentEquipment(string text);
        IAssessmentExportService IncludeMaterials(Stream stream, IDictionary<string, string> formsOfControl);
        IAssessmentExportService IncludeMaterials(string text, IDictionary<string, string> formsOfControl);
    }
}
