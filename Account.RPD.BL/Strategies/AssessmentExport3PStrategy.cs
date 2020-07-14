using Account.DAL.Entities;
using AccountRPD.BL.Interfaces;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xceed.Document.NET;
using Xceed.Words.NET;
using Font = Xceed.Document.NET.Font;

namespace AccountRPD.BL.Strategies
{
    public class AssessmentExport3PStrategy : IAssessmentExportStrategy
    {
        private DocX document;
        private IEnumerable<RPDItem> assessmentItems;

        private float CentimetersToPoints(float centimeters)
        {
            return centimeters * 28.346f;
        }

        public void Create(string filePath, IEnumerable<RPDItem> assessmentItems)
        {
            document = DocX.Create(filePath, DocumentTypes.Document);
            this.assessmentItems = assessmentItems;

            var font = new Font("Times New Roman");
            document.SetDefaultFont(font, fontSize: 12);

            document.MarginLeft = CentimetersToPoints(3.0f);
            document.MarginRight = CentimetersToPoints(1.5f);
            document.MarginTop = CentimetersToPoints(2.0f);
            document.MarginBottom = CentimetersToPoints(2.0f);
        }

        public void Include(RPD rpd, DecanatDepartment decanatDepartment, IEnumerable<Member> members)
        {
            var headerParagraph = document.InsertParagraph("СТЕРЛИТАМАКСКИЙ ФИЛИАЛ")
                                          .AppendLine("ФЕДЕРАЛЬНОГО ГОСУДАРСТВЕННОГО БЮДЖЕТНОГО ОБРАЗОВАТЕЛЬНОГО")
                                          .AppendLine("УЧРЕЖДЕНИЯ ВЫСШЕГО ОБРАЗОВАНИЯ")
                                          .AppendLine("«БАШКИРСКИЙ ГОСУДАРСТВЕННЫЙ УНИВЕРСИТЕТ»")
                                          .AppendLine();

            headerParagraph.Alignment = Alignment.center;

            var headerTable = document.AddTable(rowCount: 2, columnCount: 2);
            headerTable.Design = TableDesign.TableGrid;

            var emptyBorder = new Border(BorderStyle.Tcbs_none, BorderSize.one, 0, Color.Transparent);
            headerTable.SetBorder(TableBorderType.Left, emptyBorder);
            headerTable.SetBorder(TableBorderType.Top, emptyBorder);
            headerTable.SetBorder(TableBorderType.Right, emptyBorder);
            headerTable.SetBorder(TableBorderType.Bottom, emptyBorder);
            headerTable.SetBorder(TableBorderType.InsideH, emptyBorder);
            headerTable.SetBorder(TableBorderType.InsideV, emptyBorder);

            headerTable.Rows[0].Cells[0].Width = 2.6;
            headerTable.Rows[1].Cells[0].Width = 2.6;

            headerTable.Rows[0].Cells[0].Paragraphs[0].Append("Факультет");

            var stringBuilder = new StringBuilder(decanatDepartment.FacultyTitle);
            stringBuilder.Replace("Факультет", string.Empty).Replace("факультет", string.Empty);

            var facultyName = stringBuilder.ToString().Trim();
            facultyName = char.ToUpper(facultyName[0]) + facultyName.Substring(1);

            headerTable.Rows[0].Cells[1].Paragraphs[0].Append(facultyName)
                                                      .Italic(isItalic: true)
                                                      .Bold(isBold: true);

            headerTable.Rows[1].Cells[0].Paragraphs[0].Append("Кафедра");
            headerTable.Rows[1].Cells[1].Paragraphs[0].Append(decanatDepartment.Title)
                                                      .Italic(isItalic: true)
                                                      .Bold(isBold: true);

            var bottomBorder = new Border(BorderStyle.Tcbs_single, BorderSize.one, space: 0, Color.Black);
            headerTable.Rows[0].Cells[1].SetBorder(TableCellBorderType.Bottom, bottomBorder);
            headerTable.Rows[1].Cells[1].SetBorder(TableCellBorderType.Bottom, bottomBorder);

            headerTable.AutoFit = AutoFit.Window;
            headerParagraph.InsertTableAfterSelf(headerTable);

            document.InsertParagraph()
                    .SpacingAfter(CentimetersToPoints(2.5f));

            var titleParagraph = document.InsertParagraph("Оценочные материалы по дисциплине (модулю)")
                                         .Bold(isBold: true)
                                         .AppendLine();

            titleParagraph.Alignment = Alignment.center;

            var disciplineTable = document.AddTable(1, 2);
            disciplineTable.Design = TableDesign.TableGrid;

            disciplineTable.SetBorder(TableBorderType.Left, emptyBorder);
            disciplineTable.SetBorder(TableBorderType.Top, emptyBorder);
            disciplineTable.SetBorder(TableBorderType.Right, emptyBorder);
            disciplineTable.SetBorder(TableBorderType.Bottom, emptyBorder);
            disciplineTable.SetBorder(TableBorderType.InsideH, emptyBorder);
            disciplineTable.SetBorder(TableBorderType.InsideV, emptyBorder);

            disciplineTable.Rows[0].Cells[0].Paragraphs[0].Append("дисциплина");

            disciplineTable.Rows[0].Cells[1].Paragraphs[0].Append(rpd.DisciplineName)
                                                          .Italic(isItalic: true)
                                                          .Bold(isBold: true)
                                                          .Alignment = Alignment.center;

            disciplineTable.Rows[0].Cells[0].Width = 3.0;
            disciplineTable.AutoFit = AutoFit.Window;

            titleParagraph.InsertTableAfterSelf(disciplineTable);

            var blockParagraph = document.InsertParagraph()
                                         .SpacingAfter(CentimetersToPoints(0.5f));

            blockParagraph.Alignment = Alignment.center;

            var blockTable = document.AddTable(2, 1);
            blockTable.Design = TableDesign.TableGrid;
            blockTable.AutoFit = AutoFit.Window;

            blockTable.SetBorder(TableBorderType.Left, emptyBorder);
            blockTable.SetBorder(TableBorderType.Top, emptyBorder);
            blockTable.SetBorder(TableBorderType.Right, emptyBorder);
            blockTable.SetBorder(TableBorderType.Bottom, emptyBorder);
            blockTable.SetBorder(TableBorderType.InsideV, emptyBorder);

            var part = string.Empty;
            var loopParts = rpd.Block.Split('.');

            if (loopParts[1].Equals("О"))
            {
                part = "основная часть";
            }
            else if (loopParts[1].Equals("Б"))
            {
                part = "базовая часть";
            }
            else if (loopParts[0].Equals("ФТД") || loopParts[1].Equals("В"))
            {
                part = "вариативная часть";
            }
            // факультатив?

            blockTable.Rows[0].Cells[0].Paragraphs[0].Append($"Блок {loopParts[0]}, {part}, {rpd.Block}")
                                                     .Italic(isItalic: true)
                                                     .Bold(isBold: true)
                                                     .Alignment = Alignment.center;

            blockTable.Rows[1].Cells[0].Paragraphs[0].Append("цикл дисциплины и его часть (базовая, вариативная, дисциплина по выбору)")
                                                     .FontSize(10.0)
                                                     .Alignment = Alignment.center;

            blockParagraph.InsertTableAfterSelf(blockTable);

            document.InsertParagraph()
                    .SpacingAfter(CentimetersToPoints(0.5f));

            var profileParagraph = document.InsertParagraph("Направление")
                                           .SpacingAfter(CentimetersToPoints(1.0f));

            profileParagraph.Alignment = Alignment.center;

            var profileTable = document.AddTable(2, 2);
            profileTable.Design = TableDesign.TableGrid;

            profileTable.SetBorder(TableBorderType.Left, emptyBorder);
            profileTable.SetBorder(TableBorderType.Top, emptyBorder);
            profileTable.SetBorder(TableBorderType.Right, emptyBorder);
            profileTable.SetBorder(TableBorderType.Bottom, emptyBorder);
            profileTable.SetBorder(TableBorderType.InsideV, emptyBorder);

            profileTable.Rows[0].Cells[0].Paragraphs[0].Append(rpd.ProfileCode)
                                                       .Italic(isItalic: true)
                                                       .Bold(isBold: true)
                                                       .Alignment = Alignment.center;

            profileTable.Rows[1].Cells[0].Paragraphs[0].Append("код")
                                                       .FontSize(10.0)
                                                       .Alignment = Alignment.center;

            profileTable.Rows[0].Cells[1].Paragraphs[0].Append(rpd.ProfileName)
                                                       .Italic(isItalic: true)
                                                       .Bold(isBold: true)
                                                       .Alignment = Alignment.center;

            profileTable.Rows[1].Cells[1].Paragraphs[0].Append("наименование направления или специальности")
                                                       .FontSize(10.0)
                                                       .Alignment = Alignment.center;

            profileTable.Rows[0].Cells[0].Width = 2.7;
            profileTable.Rows[1].Cells[0].Width = 2.7;
            profileTable.AutoFit = AutoFit.Window;

            profileParagraph.InsertTableAfterSelf(profileTable);

            document.InsertParagraph()
                    .SpacingAfter(CentimetersToPoints(0.5f));

            // Ищет все вхождения после слова «Программа»
            var separator = (rpd.DirectionName.Contains("Программа:")) ? ":" : string.Empty;
            var regex = new Regex($"Программа{separator}\\s+([^\n]+)");

            rpd.DirectionName = rpd.DirectionName.Replace("\"", string.Empty);
            var directionName = string.Empty;

            if (regex.IsMatch(rpd.DirectionName))
            {
                directionName = regex.Match(rpd.DirectionName).Groups[1].Value;
            }

            if (directionName.Contains("\""))
            {
                directionName = directionName.Replace("\"", string.Empty);
            }

            if (directionName.Contains("\r"))
            {
                directionName = directionName.Replace("\r", string.Empty);
            }

            var programParagraph = document.InsertParagraph("Программа")
                                           .SpacingAfter(CentimetersToPoints(1.0f));

            programParagraph.Alignment = Alignment.center;

            var programTable = document.AddTable(3, 1);
            programTable.Design = TableDesign.TableGrid;
            programTable.AutoFit = AutoFit.Window;

            programTable.SetBorder(TableBorderType.Left, emptyBorder);
            programTable.SetBorder(TableBorderType.Top, emptyBorder);
            programTable.SetBorder(TableBorderType.Right, emptyBorder);
            programTable.SetBorder(TableBorderType.InsideV, emptyBorder);

            programTable.Rows[0].Cells[0].Paragraphs[0].Append(directionName)
                                                       .Italic(isItalic: true)
                                                       .Bold(isBold: true)
                                                       .Alignment = Alignment.center;

            programParagraph.InsertTableAfterSelf(programTable);

            document.InsertParagraph()
                    .SpacingAfter(CentimetersToPoints(3.0f));

            var membersCount = members.Count();

            var memberTable = document.AddTable(3, 2);
            memberTable.Design = TableDesign.TableGrid;
            memberTable.AutoFit = AutoFit.Window;

            memberTable.SetBorder(TableBorderType.Left, emptyBorder);
            memberTable.SetBorder(TableBorderType.Top, emptyBorder);
            memberTable.SetBorder(TableBorderType.Right, emptyBorder);
            memberTable.SetBorder(TableBorderType.Bottom, emptyBorder);
            memberTable.SetBorder(TableBorderType.InsideH, emptyBorder);
            memberTable.SetBorder(TableBorderType.InsideV, emptyBorder);

            var memberParagraph = default(Paragraph);

            if (membersCount.Equals(1))
            {
                memberParagraph = document.InsertParagraph("Разработчик (составитель)");

                var member = members.FirstOrDefault();
                var firstnameInitial = member.Firstname.FirstOrDefault();
                var middlenameInitial = member.Middlename.FirstOrDefault();
                var memberShortname = $"{firstnameInitial}. {middlenameInitial}. {member.Lastname}";

                var academicDegree = member.AcademicDegree?.ToLower();
                var position = member.Position?.ToLower();

                memberTable.Rows[0].Cells[0].Paragraphs[0].Append($"{academicDegree}, {position}")
                                                          .Italic(isItalic: true)
                                                          .Bold(isBold: true)
                                                          .Alignment = Alignment.center;

                memberTable.Rows[1].Cells[0].Paragraphs[0].Append(memberShortname)
                                                          .Italic(isItalic: true)
                                                          .Bold(isBold: true)
                                                          .Alignment = Alignment.center;

                memberTable.Rows[2].Cells[0].Paragraphs[0].Append("ученая степень, должность, ФИО")
                                                          .FontSize(10.0)
                                                          .Alignment = Alignment.center;
            }
            else if (membersCount.Equals(2))
            {
                memberParagraph = document.InsertParagraph("Разработчики (составители)");

                var leftMember = members.FirstOrDefault();
                var leftFirstnameInitial = leftMember.Firstname.FirstOrDefault();
                var leftMiddlenameInitial = leftMember.Middlename.FirstOrDefault();
                var leftMemberShortname = $"{leftFirstnameInitial}. {leftMiddlenameInitial}. {leftMember.Lastname}";
                var leftAcademicDegree = leftMember.AcademicDegree?.ToLower();
                var leftPosition = leftMember.Position?.ToLower();

                var rightMember = members.LastOrDefault();
                var rightFirstnameInitial = rightMember.Firstname.FirstOrDefault();
                var rightMiddlenameInitial = rightMember.Middlename.FirstOrDefault();
                var rightMemberShortname = $"{rightFirstnameInitial}. {rightMiddlenameInitial}. {rightMember.Lastname}";
                var rightAcademicDegree = rightMember.AcademicDegree?.ToLower();
                var rightPosition = rightMember.Position?.ToLower();

                memberTable.Rows[0].Cells[0].Paragraphs[0].Append($"{leftAcademicDegree}, {leftPosition} {leftMemberShortname}")
                                                          .Italic(isItalic: true)
                                                          .Bold(isBold: true)
                                                          .Alignment = Alignment.center;

                memberTable.Rows[1].Cells[0].Paragraphs[0].Append($"{rightAcademicDegree}, {rightPosition} {rightMemberShortname}")
                                                          .Italic(isItalic: true)
                                                          .Bold(isBold: true)
                                                          .Alignment = Alignment.center;

                memberTable.Rows[2].Cells[0].Paragraphs[0].Append("ученая степень, должность, ФИО")
                                                          .FontSize(10.0)
                                                          .Alignment = Alignment.center;
            }
            else if (membersCount.Equals(3))
            {
                memberParagraph = document.InsertParagraph("Разработчики (составители)");

                var leftMember = members.FirstOrDefault();
                var leftFirstnameInitial = leftMember.Firstname.FirstOrDefault();
                var leftMiddlenameInitial = leftMember.Middlename.FirstOrDefault();
                var leftMemberShortname = $"{leftFirstnameInitial}. {leftMiddlenameInitial}. {leftMember.Lastname}";
                var leftAcademicDegree = leftMember.AcademicDegree?.ToLower();
                var leftPosition = leftMember.Position?.ToLower();

                var middleMember = (members as IList<Member>).ElementAt(1);
                var middleFirstnameInitial = middleMember.Firstname.FirstOrDefault();
                var middleMiddlenameInitial = middleMember.Middlename.FirstOrDefault();
                var middleMemberShortname = $"{middleFirstnameInitial}. {middleMiddlenameInitial}. {middleMember.Lastname}";
                var middleAcademicDegree = middleMember.AcademicDegree?.ToLower();
                var middlePosition = middleMember.Position?.ToLower();

                var rightMember = members.LastOrDefault();
                var rightFirstnameInitial = rightMember.Firstname.FirstOrDefault();
                var rightMiddlenameInitial = rightMember.Middlename.FirstOrDefault();
                var rightMemberShortname = $"{rightFirstnameInitial}. {rightMiddlenameInitial}. {rightMember.Lastname}";
                var rightAcademicDegree = rightMember.AcademicDegree?.ToLower();
                var rightPosition = rightMember.Position?.ToLower();

                memberTable.InsertRow(3);
                memberTable.Rows[2].Cells[0].SetBorder(TableCellBorderType.Bottom, bottomBorder);

                memberTable.Rows[0].Cells[0].Paragraphs[0].Append($"{leftAcademicDegree}, {leftPosition} {leftMemberShortname}")
                                                          .Italic(isItalic: true)
                                                          .Bold(isBold: true)
                                                          .Alignment = Alignment.center;

                memberTable.Rows[1].Cells[0].Paragraphs[0].Append($"{middleAcademicDegree}, {middlePosition} {middleMemberShortname}")
                                                          .Italic(isItalic: true)
                                                          .Bold(isBold: true)
                                                          .Alignment = Alignment.center;

                memberTable.Rows[2].Cells[0].Paragraphs[0].Append($"{rightAcademicDegree}, {rightPosition} {rightMemberShortname}")
                                                          .Italic(isItalic: true)
                                                          .Bold(isBold: true)
                                                          .Alignment = Alignment.center;

                memberTable.Rows[3].Cells[0].Paragraphs[0].Append("ученая степень, должность, ФИО")
                                                          .FontSize(10.0)
                                                          .Alignment = Alignment.center;
            }

            memberTable.Rows[0].Cells[0].SetBorder(TableCellBorderType.Bottom, bottomBorder);
            memberTable.Rows[1].Cells[0].SetBorder(TableCellBorderType.Bottom, bottomBorder);

            memberParagraph.InsertTableAfterSelf(memberTable);

            document.InsertParagraph()
                        .SpacingAfter(CentimetersToPoints(1.0f));

            var yearParagraph = document.InsertParagraph($"Стерлитамак {rpd.ApprovalDateRPD.Value.Year}");
            yearParagraph.Alignment = Alignment.center;
            yearParagraph.InsertPageBreakAfterSelf();
        }

        public void IncludeTableOfContents()
        {
            var tableOfContentsParagraph = document.InsertParagraph("Оглавление")
                                                   .Bold(isBold: true);

            tableOfContentsParagraph.Alignment = Alignment.center;

            document.InsertTableOfContents(string.Empty, TableOfContentsSwitches.None);

            var emptyParagraph = document.InsertParagraph()
                                         .SpacingAfter(4.0);

            emptyParagraph.InsertPageBreakAfterSelf();
        }

        public void Include(IDictionary<DecanatCompetence, IEnumerable<CompetenceGrade>> competenceGradesDictionary, IDictionary<DecanatCompetence, Competence> competences)
        {
            var assessmentItem = assessmentItems.FirstOrDefault(AssessmentItem => AssessmentItem.Number.Equals("1"));
            var assessmentItemParagraph = document.InsertParagraph($"{assessmentItem.Number}. {assessmentItem.Name}")
                                              .Heading(HeadingType.Heading1)
                                              .Bold(isBold: true)
                                              .Color(Color.Black)
                                              .Font("Times New Roman")
                                              .FontSize(12.0)
                                              .SpacingBefore(0.0)
                                              .SpacingAfter(4.0);

            var competenceGradesTable = document.AddTable(3 + competenceGradesDictionary.Keys.Count * 3, 7);
            competenceGradesTable.Design = TableDesign.TableGrid;
            competenceGradesTable.AutoFit = AutoFit.Window;

            competenceGradesTable.Rows[0].Cells[0].Paragraphs[0].Append("Формируемая компетенция (с указанием кода)")
                                                                .Bold(isBold: true)
                                                                .Alignment = Alignment.center;

            competenceGradesTable.Rows[1].Cells[0].Paragraphs[0].Append("1")
                                                                .Bold(isBold: true)
                                                                .Alignment = Alignment.center;

            competenceGradesTable.Rows[0].Cells[1].Paragraphs[0].Append("Результаты обучения по дисциплине (модулю)")
                                                                .Bold(isBold: true)
                                                                .Alignment = Alignment.center;

            competenceGradesTable.Rows[1].Cells[1].Paragraphs[0].Append("2")
                                                                .Bold(isBold: true)
                                                                .Alignment = Alignment.center;

            competenceGradesTable.Rows[0].MergeCells(startIndex: 2, endIndex: 5);
            competenceGradesTable.Rows[0].Cells[2].Paragraphs[0].Append("Показатели и критерии оценивания результатов обучения по дисциплине (модулю)")
                                                                .Bold(isBold: true)
                                                                .Alignment = Alignment.center;

            competenceGradesTable.Rows[1].MergeCells(startIndex: 2, endIndex: 5);
            competenceGradesTable.Rows[1].Cells[2].Paragraphs[0].Append("3")
                                                                .Bold(isBold: true)
                                                                .Alignment = Alignment.center;

            competenceGradesTable.Rows[2].Cells[2].Paragraphs[0].Append("неуд.")
                                                                .Bold(isBold: true)
                                                                .Alignment = Alignment.center;

            competenceGradesTable.Rows[2].Cells[3].Paragraphs[0].Append("удовл.")
                                                                .Bold(isBold: true)
                                                                .Alignment = Alignment.center;

            competenceGradesTable.Rows[2].Cells[4].Paragraphs[0].Append("хорошо")
                                                                .Bold(isBold: true)
                                                                .Alignment = Alignment.center;

            competenceGradesTable.Rows[2].Cells[5].Paragraphs[0].Append("отлично")
                                                                .Bold(isBold: true)
                                                                .Alignment = Alignment.center;

            competenceGradesTable.Rows[0].Cells[3].Paragraphs[0].Append("Вид оценочного средства")
                                                                .Bold(isBold: true)
                                                                .Alignment = Alignment.center;

            competenceGradesTable.Rows[1].Cells[3].Paragraphs[0].Append("4")
                                                                .Bold(isBold: true)
                                                                .Alignment = Alignment.center;

            var index = 3;
            foreach (var keyValuePair in competenceGradesDictionary)
            {
                competenceGradesTable.MergeCellsInColumn(columnIndex: 0, index, index + 2);
                competenceGradesTable.Rows[index].Cells[0].Paragraphs[0].Append($"{keyValuePair.Key.Content} ({keyValuePair.Key.Code})");
            
                var subIndex = index;
                foreach (var competenceStage in keyValuePair.Value)
                {
                    competenceGradesTable.Rows[subIndex].Cells[1].Paragraphs[0].Append(competenceStage.Stage);

                    competenceGradesTable.Rows[subIndex].Cells[2].Paragraphs[0].Append(competenceStage.BadGrade);

                    competenceGradesTable.Rows[subIndex].Cells[3].Paragraphs[0].Append(competenceStage.TernGrade);

                    competenceGradesTable.Rows[subIndex].Cells[4].Paragraphs[0].Append(competenceStage.WellGrade);

                    competenceGradesTable.Rows[subIndex].Cells[5].Paragraphs[0].Append(competenceStage.PerfectGrade);

                    competenceGradesTable.Rows[subIndex].Cells[6].Paragraphs[0].Append(competenceStage.ValuationType);

                    subIndex++;
                }

                index += 3;
            }

            competenceGradesTable.AutoFit = AutoFit.Contents;
            assessmentItemParagraph.InsertTableAfterSelf(competenceGradesTable);

            document.InsertParagraph()
                    .SpacingAfter(4.0);
        }

        public void IncludeAssessmentEquipment(Stream stream)
        {
            var assessmentItem = assessmentItems.FirstOrDefault(AssessmentItem => AssessmentItem.Number.Equals("2"));
            var assessmentItemParagraph = document.InsertParagraph($"{assessmentItem.Number}. {assessmentItem.Name}")
                                                  .Heading(HeadingType.Heading1)
                                                  .Bold(isBold: true)
                                                  .Color(Color.Black)
                                                  .Font("Times New Roman")
                                                  .FontSize(12.0)
                                                  .SpacingBefore(0.0)
                                                  .SpacingAfter(4.0);

            var insertedDocument = DocX.Load(stream);

            insertedDocument.MarginLeft = CentimetersToPoints(3.0f);
            insertedDocument.MarginRight = CentimetersToPoints(1.5f);
            insertedDocument.MarginTop = CentimetersToPoints(2.0f);
            insertedDocument.MarginBottom = CentimetersToPoints(2.0f);

            var font = new Font("Times New Roman");

            foreach (var paragraph in insertedDocument.Paragraphs)
            {
                paragraph.Font(font);
                paragraph.FontSize(fontSize: 12.0);
            }

            document.InsertDocument(insertedDocument);
            document.InsertSection();

            document.InsertParagraph()
                    .SpacingAfter(4.0);
        }

        public void IncludeAssessmentEquipment(string text)
        {
            var assessmentItem = assessmentItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("2"));
            var assessmentItemParagraph = document.InsertParagraph($"{assessmentItem.Number}. {assessmentItem.Name}")
                                                  .Heading(HeadingType.Heading1)
                                                  .Bold(isBold: true)
                                                  .Color(Color.Black)
                                                  .Font("Times New Roman")
                                                  .FontSize(12.0)
                                                  .SpacingBefore(0.0)
                                                  .SpacingAfter(4.0);

            var textParagraph = document.InsertParagraph(text);

            document.InsertParagraph()
                    .SpacingAfter(4.0);
        }

        public void IncludeMaterials(Stream stream, IDictionary<string, string> formsOfControl)
        {
            var assessmentItem = assessmentItems.FirstOrDefault(AssessmentItem => AssessmentItem.Number.Equals("3"));
            var assessmentItemParagraph = document.InsertParagraph($"{assessmentItem.Number}. {assessmentItem.Name}")
                                              .Heading(HeadingType.Heading1)
                                              .Bold(isBold: true)
                                              .Color(Color.Black)
                                              .Font("Times New Roman")
                                              .FontSize(12.0)
                                              .SpacingBefore(0.0)
                                              .SpacingAfter(4.0);

            var insertedDocument = DocX.Load(stream);

            insertedDocument.MarginLeft = CentimetersToPoints(3.0f);
            insertedDocument.MarginRight = CentimetersToPoints(1.5f);
            insertedDocument.MarginTop = CentimetersToPoints(2.0f);
            insertedDocument.MarginBottom = CentimetersToPoints(2.0f);

            var font = new Font("Times New Roman");
            insertedDocument.SetDefaultFont(font, fontSize: 12);

            document.InsertDocument(insertedDocument);
            document.InsertSection();

            document.InsertParagraph("\tРезультаты обучения по дисциплине (модулю) у обучающихся оцениваются по " +
                                     "итогам текущего контроля количественной оценкой, выраженной в рейтинговых баллах. " +
                                     "Оценке подлежит каждое контрольное мероприятие.")
                    .AppendLine("\tПри оценивании сформированности компетенций применяется четырехуровневая " +
                                "шкала «неудовлетворительно», «удовлетворительно», «хорошо», «отлично».")
                    .AppendLine("\tМаксимальный балл по каждому виду оценочного средства определяется в " +
                                "рейтинг-плане и выражает полное (100%) освоение компетенции.")
                    .AppendLine("\tУровень сформированности компетенции «хорошо» устанавливается в случае, " +
                                "когда объем выполненных заданий соответствующего оценочного средства составляет 80-100%; " +
                                "«удовлетворительно» – выполнено 40-80%; «неудовлетворительно» – выполнено 0-40%")
                    .AppendLine("\tРейтинговый балл за выполнение части или полного объема заданий соответствующего " +
                                "оценочного средства выставляется по формуле: ")
                    .SpacingAfter(4.0);

            document.InsertParagraph("Рейтинговый балл = k × Максимальный балл,")
                    .SpacingAfter(4.0)
                    .Alignment = Alignment.center;

            document.InsertParagraph("где k = 0,2 при уровне освоения «неудовлетворительно», k = 0,4 при уровне освоения " +
                                     "«удовлетворительно», k = 0,8 при уровне освоения «хорошо» и k = 1 при уровне освоения " +
                                     "«отлично».")
                    .AppendLine("\tОценка на этапе промежуточной аттестации выставляется согласно Положению о модульно-рейтинговой системе обучения " +
                            "и оценки успеваемости студентов БашГУ: ");

            var primaryControlKey = string.Empty;

            if (formsOfControl.ContainsKey("Экзамен") && formsOfControl.ContainsKey("Дифференцированный зачет"))
            {
                primaryControlKey = "экзамене и дифференцированном зачете";
            }

            if (formsOfControl.ContainsKey("Экзамен") && !formsOfControl.ContainsKey("Дифференцированный зачет"))
            {
                primaryControlKey = "экзамене";
            }

            if (formsOfControl.ContainsKey("Дифференцированный зачет") && !formsOfControl.ContainsKey("Экзамен"))
            {
                primaryControlKey = "дифференцированном зачете";
            }

            if (!string.IsNullOrEmpty(primaryControlKey))
            {
                document.InsertParagraph($"На {primaryControlKey} выставляется оценка:")
                        .AppendLine("• отлично - при накоплении от 80 до 110 рейтинговых баллов (включая 10 " +
                                    "поощрительных баллов),")
                        .AppendLine("• хорошо - при накоплении от 60 до 79 рейтинговых баллов,")
                        .AppendLine("• удовлетворительно - при накоплении от 45 до 59 рейтинговых баллов,")
                        .AppendLine("• неудовлетворительно - при накоплении менее 45 рейтинговых баллов.");
            }

            var secondaryControlKey = string.Empty;

            if (formsOfControl.ContainsKey("Зачет") || formsOfControl.ContainsKey("Зачёт"))
            {
                secondaryControlKey = "зачете";
            }

            if (!string.IsNullOrEmpty(secondaryControlKey))
            {
                document.InsertParagraph($"На {secondaryControlKey} выставляется оценка:")
                        .AppendLine("• зачтено - при накоплении от 60 до 110 рейтинговых баллов (включая 10 " +
                                    "поощрительных баллов),")
                        .AppendLine("• не зачтено - при накоплении от 0 до 59 рейтинговых баллов.");
            }

            document.InsertParagraph("\tПри получении на экзамене оценок «отлично», «хорошо», «удовлетворительно», на " +
                                     "зачёте оценки «зачтено» считается, что результаты обучения по дисциплине (модулю) " +
                                     "достигнуты и компетенции на этапе изучения дисциплины (модуля) сформированы.")
                    .SpacingBefore(4.0);

            document.InsertParagraph()
                    .SpacingAfter(4.0);
        }

        public void IncludeMaterials(string text, IDictionary<string, string> formsOfControl)
        {
            var assessmentItem = assessmentItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("3"));
            var assessmentItemParagraph = document.InsertParagraph($"{assessmentItem.Number}. {assessmentItem.Name}")
                                                  .Heading(HeadingType.Heading1)
                                                  .Bold(isBold: true)
                                                  .Color(Color.Black)
                                                  .Font("Times New Roman")
                                                  .FontSize(12.0)
                                                  .SpacingBefore(0.0)
                                                  .SpacingAfter(4.0);

            var textParagraph = document.InsertParagraph(text);

            document.InsertParagraph()
                    .SpacingAfter(4.0);

            document.InsertParagraph("\tРезультаты обучения по дисциплине (модулю) у обучающихся оцениваются по " +
                                     "итогам текущего контроля количественной оценкой, выраженной в рейтинговых баллах. " +
                                     "Оценке подлежит каждое контрольное мероприятие.")
                    .AppendLine("\tПри оценивании сформированности компетенций применяется четырехуровневая " +
                                "шкала «неудовлетворительно», «удовлетворительно», «хорошо», «отлично».")
                    .AppendLine("\tМаксимальный балл по каждому виду оценочного средства определяется в " +
                                "рейтинг-плане и выражает полное (100%) освоение компетенции.")
                    .AppendLine("\tУровень сформированности компетенции «хорошо» устанавливается в случае, " +
                                "когда объем выполненных заданий соответствующего оценочного средства составляет 80-100%; " +
                                "«удовлетворительно» – выполнено 40-80%; «неудовлетворительно» – выполнено 0-40%")
                    .AppendLine("\tРейтинговый балл за выполнение части или полного объема заданий соответствующего " +
                                "оценочного средства выставляется по формуле: ")
                    .SpacingAfter(4.0);

            document.InsertParagraph("Рейтинговый балл = k × Максимальный балл,")
                    .SpacingAfter(4.0)
                    .Alignment = Alignment.center;

            document.InsertParagraph("где k = 0,2 при уровне освоения «неудовлетворительно», k = 0,4 при уровне освоения " +
                                     "«удовлетворительно», k = 0,8 при уровне освоения «хорошо» и k = 1 при уровне освоения " +
                                     "«отлично».")
                    .AppendLine("\tОценка на этапе промежуточной аттестации выставляется согласно Положению о модульно-рейтинговой системе обучения " +
                            "и оценки успеваемости студентов БашГУ: ");

            var primaryControlKey = string.Empty;

            if (formsOfControl.ContainsKey("Экзамен") && formsOfControl.ContainsKey("Дифференцированный зачет"))
            {
                primaryControlKey = "экзамене и дифференцированном зачете";
            }
            
            if (formsOfControl.ContainsKey("Экзамен") && !formsOfControl.ContainsKey("Дифференцированный зачет"))
            {
                primaryControlKey = "экзамене";
            }
            
            if (formsOfControl.ContainsKey("Дифференцированный зачет") && !formsOfControl.ContainsKey("Экзамен"))
            {
                primaryControlKey = "дифференцированном зачете";
            }

            if (!string.IsNullOrEmpty(primaryControlKey))
            {
                document.InsertParagraph($"На {primaryControlKey} выставляется оценка:")
                        .AppendLine("• отлично - при накоплении от 80 до 110 рейтинговых баллов (включая 10 " +
                                    "поощрительных баллов),")
                        .AppendLine("• хорошо - при накоплении от 60 до 79 рейтинговых баллов,")
                        .AppendLine("• удовлетворительно - при накоплении от 45 до 59 рейтинговых баллов,")
                        .AppendLine("• неудовлетворительно - при накоплении менее 45 рейтинговых баллов.");
            }

            var secondaryControlKey = string.Empty;

            if (formsOfControl.ContainsKey("Зачет") || formsOfControl.ContainsKey("Зачёт"))
            {
                secondaryControlKey = "зачете";
            }

            if (!string.IsNullOrEmpty(secondaryControlKey))
            {
                document.InsertParagraph($"На {secondaryControlKey} выставляется оценка:")
                        .AppendLine("• зачтено - при накоплении от 60 до 110 рейтинговых баллов (включая 10 " +
                                    "поощрительных баллов),")
                        .AppendLine("• не зачтено - при накоплении от 0 до 59 рейтинговых баллов.");
            }

            document.InsertParagraph("\tПри получении на экзамене оценок «отлично», «хорошо», «удовлетворительно», на " +
                                     "зачёте оценки «зачтено» считается, что результаты обучения по дисциплине (модулю) " +
                                     "достигнуты и компетенции на этапе изучения дисциплины (модуля) сформированы.")
                    .SpacingBefore(4.0);

            document.InsertParagraph()
                    .SpacingAfter(4.0);
        }

        public void Export()
        {
            document.AddFooters();

            document.DifferentFirstPage = true;
            document.DifferentOddAndEvenPages = true;

            document.Footers.First = null;
            document.Footers.Even.PageNumbers = true;
            document.Footers.Odd.PageNumbers = true;

            var font = new Font("Times New Roman");

            foreach (var paragraph in document.Paragraphs)
            {
                paragraph.Font(font);
                paragraph.FontSize(fontSize: 12.0);
            }

            document.Save();
        }
    }
}
