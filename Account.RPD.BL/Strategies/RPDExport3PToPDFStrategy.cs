using Account.DAL.Entities;
using AccountRPD.BL.Interfaces;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace AccountRPD.BL.Strategies
{
    public class RPDExport3PToPDFStrategy : IRPDExportStrategy
    {
        private string filePath;
        private XFont font;
        private PdfDocument document;
        private IEnumerable<RPDItem> rpdItems;

        private PdfPage CreatePage()
        {
            var page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            page.TrimMargins.Left = XUnit.FromCentimeter(3.0f);
            page.TrimMargins.Right = XUnit.FromCentimeter(1.5f);
            page.TrimMargins.Top = XUnit.FromCentimeter(2.0f);
            page.TrimMargins.Bottom = XUnit.FromCentimeter(2.0f);

            return page;
        }

        public void Create(string filePath, IEnumerable<RPDItem> rpdItems)
        {
            this.filePath = filePath;
            document = new PdfDocument();
            this.rpdItems = rpdItems;
            font = new XFont("Times New Roman", 12.0, XFontStyle.Regular);
        }

        public void Include(RPD rpd, DecanatDepartment decanatDepartment, IEnumerable<Member> members)
        {
            var page = CreatePage();
            var gfx = XGraphics.FromPdfPage(page);

            gfx.DrawString("0", font, XBrushes.Black, new XRect(0.0, 0.0, page.Width, 12.0));
        }

        public void IncludeTableOfContents()
        {
            throw new System.NotImplementedException();
        }

        public void Include(IEnumerable<RPDContent> rpdContents)
        {
            throw new System.NotImplementedException();
        }

        public void Include(IDictionary<DecanatCompetence, Competence> competences)
        {
            throw new System.NotImplementedException();
        }

        public void Include(DecanatPlan plan, IEnumerable<RPDContent> rpdContents, DecanatHoursDivision hoursDivision, IDictionary<string, string> formsOfControl, IEnumerable<int> courses, IEnumerable<int> semesters)
        {
            throw new System.NotImplementedException();
        }

        public void Include(IEnumerable<ThematicPlan> thematicPlans, string lectures, string practices, string labs, string individualWorks)
        {
            throw new System.NotImplementedException();
        }

        public void Include(IDictionary<string, IEnumerable<ThematicContent>> thematicContents)
        {
            throw new System.NotImplementedException();
        }

        public void Include(Stream stream)
        {
            throw new System.NotImplementedException();
        }

        public void Include(string text)
        {
            throw new System.NotImplementedException();
        }

        public void Include(IEnumerable<BasicLiterature> basicLiteratures, IEnumerable<AdditionalLiterature> additionalLiteratures)
        {
            throw new System.NotImplementedException();
        }

        public void Include(IEnumerable<LibrarySystem> librarySystems, IEnumerable<InternetResource> internetResources)
        {
            throw new System.NotImplementedException();
        }

        public void Include(IEnumerable<License> licenses)
        {
            throw new System.NotImplementedException();
        }

        public void Include(IEnumerable<MaterialBase> materialBases)
        {
            throw new System.NotImplementedException();
        }

        public void Export()
        {
            document.Save(filePath);
        }
    }
}
