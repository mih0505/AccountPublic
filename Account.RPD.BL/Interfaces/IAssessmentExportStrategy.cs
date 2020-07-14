using Account.DAL.Entities;
using System.Collections.Generic;
using System.IO;

namespace AccountRPD.BL.Interfaces
{
    public interface IAssessmentExportStrategy : IExportable
    {
        void Create(string filePath, IEnumerable<RPDItem> assessmentItems);
        void Include(RPD rpd, DecanatDepartment decanatDepartment, IEnumerable<Member> members);
        void IncludeTableOfContents();
        void Include(IDictionary<DecanatCompetence, IEnumerable<CompetenceGrade>> competenceGradesDictionary, IDictionary<DecanatCompetence, Competence> competences);
        void IncludeAssessmentEquipment(Stream stream);
        void IncludeAssessmentEquipment(string text);
        void IncludeMaterials(Stream stream, IDictionary<string, string> formsOfControl);
        void IncludeMaterials(string text, IDictionary<string, string> formsOfControl);
    }
}
