using Account.DAL.Entities;
using AccountRPD.BL.Interfaces;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using Xceed.Document.NET;
using Xceed.Words.NET;
using Font = Xceed.Document.NET.Font;

namespace AccountRPD.BL.Strategies
{
    public class RPDExport3PStrategy : IRPDExportStrategy
    {
        private DocX document;
        private IEnumerable<RPDItem> rpdItems;

        private float CentimetersToPoints(float centimeters)
        {
            return centimeters * 28.346f;
        }

        public void Create(string filePath, IEnumerable<RPDItem> rpdItems)
        {
            document = DocX.Create(filePath, DocumentTypes.Document);
            this.rpdItems = rpdItems;

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
                    .SpacingAfter(CentimetersToPoints(3.0f));

            var titleParagraph = document.InsertParagraph("Рабочая программа дисциплины (модуля)")
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

        public void Include(IEnumerable<RPDContent> rpdContents)
        {
            var rpdParentItem = rpdItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("1"));
            var parentItemParagraph = document.InsertParagraph($"{rpdParentItem.Number}. {rpdParentItem.Name}")
                                              .Heading(HeadingType.Heading1)
                                              .Bold(isBold: true)
                                              .Color(Color.Black)
                                              .Font("Times New Roman")
                                              .FontSize(12.0)
                                              .SpacingBefore(0.0)
                                              .SpacingAfter(4.0);

            var rpdItem = rpdItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("1.1"));
            var rpdItemParagraph = document.InsertParagraph($"{rpdItem.Number}. {rpdItem.Name}")
                                           .Heading(HeadingType.Heading3)
                                           .Bold(isBold: true)
                                           .Color(Color.Black)
                                           .Font("Times New Roman")
                                           .FontSize(12.0)
                                           .SpacingBefore(0.0)
                                           .SpacingAfter(4.0);

            var rpdContent = rpdContents.FirstOrDefault(RPDContent => RPDContent.RPDItemId.Equals(rpdItem.Id));
            var rpdContentParagraph = document.InsertParagraph(rpdContent?.Content)
                                              .SpacingAfter(2.0f);
        }

        public void Include(IDictionary<DecanatCompetence, Competence> competences)
        {
            var decanatCompetenceParagraph = document.Paragraphs.LastOrDefault();

            var decanatCompetenceTable = document.AddTable(competences.Keys.Count, 1);
            decanatCompetenceTable.Design = TableDesign.TableGrid;
            decanatCompetenceTable.AutoFit = AutoFit.Window;

            var index = 0;
            foreach (var decanatCompetence in competences.Keys)
            {
                var content = $"{decanatCompetence.Content} ({decanatCompetence.Code})".Replace("\n", string.Empty);
                decanatCompetenceTable.Rows[index++].Cells[0].Paragraphs[0].Append(content);
            }

            decanatCompetenceParagraph.InsertTableAfterSelf(decanatCompetenceTable);

            document.InsertParagraph()
                    .SpacingAfter(4.0);

            var competenceRPDItem = rpdItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("1.2"));
            var competenceRPDItemParagraph = document.InsertParagraph($"{competenceRPDItem.Number}. {competenceRPDItem.Name}")
                                                     .Heading(HeadingType.Heading3)
                                                     .Bold(isBold: true)
                                                     .Color(Color.Black)
                                                     .Font("Times New Roman")
                                                     .FontSize(12.0)
                                                     .SpacingBefore(0.0)
                                                     .SpacingAfter(4.0);

            var competenceRPDItemTable = document.AddTable(1 + competences.Keys.Count * 3, 3);
            competenceRPDItemTable.Design = TableDesign.TableGrid;
            competenceRPDItemTable.AutoFit = AutoFit.Window;

            competenceRPDItemTable.Rows[0].Cells[0].Paragraphs[0].Append("Планируемые результаты освоения образовательной программы (компетенции)")
                                                                 .Bold(isBold: true)
                                                                 .Alignment = Alignment.center;

            competenceRPDItemTable.Rows[0].Cells[1].Paragraphs[0].Append("Этапы формирования компетенции")
                                                                 .Bold(isBold: true)
                                                                 .Alignment = Alignment.center;

            competenceRPDItemTable.Rows[0].Cells[2].Paragraphs[0].Append("Планируемые результаты обучения по дисциплине (модулю)")
                                                                 .Bold(isBold: true)
                                                                 .Alignment = Alignment.center;

            index = 1;
            foreach (var keyValuePair in competences)
            {
                competenceRPDItemTable.MergeCellsInColumn(columnIndex: 0, index, index + 2);
                competenceRPDItemTable.Rows[index].Cells[0].Paragraphs[0].Append($"{keyValuePair.Key.Content} ({keyValuePair.Key.Code})");

                competenceRPDItemTable.Rows[index].Cells[1].Paragraphs[0].Append("1 этап: Знания");
                competenceRPDItemTable.Rows[index].Cells[2].Paragraphs[0].Append($"Обучающийся должен знать:\n{keyValuePair.Value.Knowledge}");

                index++;

                competenceRPDItemTable.Rows[index].Cells[1].Paragraphs[0].Append("2 этап: Умения");
                competenceRPDItemTable.Rows[index].Cells[2].Paragraphs[0].Append($"Обучающийся должен уметь:\n{keyValuePair.Value.Skill}");

                index++;

                competenceRPDItemTable.Rows[index].Cells[1].Paragraphs[0].Append("3 этап: Владения (навыки / опыт деятельности)");
                competenceRPDItemTable.Rows[index].Cells[2].Paragraphs[0].Append($"Обучающийся должен владеть:\n{keyValuePair.Value.Possession}");

                index++;
            }

            competenceRPDItemTable.AutoFit = AutoFit.Contents;
            competenceRPDItemParagraph.InsertTableAfterSelf(competenceRPDItemTable);

            document.InsertParagraph()
                    .SpacingAfter(4.0);
        }

        public void Include(DecanatPlan plan, IEnumerable<RPDContent> rpdContents, DecanatHoursDivision hoursDivision, IDictionary<string, string> formsOfControl, IEnumerable<int> courses, IEnumerable<int> semesters)
        {
            var coursesList = courses as IList<int>;
            var semestersList = semesters as IList<int>;

            var disciplinePlaceItem = rpdItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("2"));
            var disciplinePlaceParagraph = document.InsertParagraph($"{disciplinePlaceItem.Number}. {disciplinePlaceItem.Name}")
                                                   .Heading(HeadingType.Heading1)
                                                   .Bold(isBold: true)
                                                   .Color(Color.Black)
                                                   .Font("Times New Roman")
                                                   .FontSize(12.0)
                                                   .SpacingBefore(0.0)
                                                   .SpacingAfter(4.0);

            var disciplinePlaceContent = rpdContents.FirstOrDefault(RPDContent => RPDContent.RPDItemId.Equals(disciplinePlaceItem.Id));

            var coursesLine = coursesList.FirstOrDefault().ToString();
            for (int index = 1; index < coursesList.Count; index++)
            {
                coursesLine += $", {coursesList[index]}";
            }

            var semestersLine = semestersList.FirstOrDefault().ToString();
            for (int index = 1; index < semestersList.Count; index++)
            {
                semestersLine += $", {semestersList[index]}";
            }

            var courseCase = (!coursesList.Count.Equals(1)) ? "ах" : "e";
            var semesterCase = (!semestersList.Count.Equals(1)) ? "ах" : "e";

            document.InsertParagraph(disciplinePlaceContent?.Content)
                    .AppendLine()
                    .AppendLine($"Дисциплина изучается на {coursesLine} курс{courseCase} в {semestersLine} семестр{semesterCase}");

            document.InsertParagraph()
                    .SpacingAfter(4.0);

            var hoursDivisionItem = rpdItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("3"));
            var hoursDivisionParagraph = document.InsertParagraph($"{hoursDivisionItem.Number}. {hoursDivisionItem.Name}")
                                                 .Heading(HeadingType.Heading1)
                                                 .Bold(isBold: true)
                                                 .Color(Color.Black)
                                                 .Font("Times New Roman")
                                                 .FontSize(12.0)
                                                 .SpacingBefore(0.0)
                                                 .SpacingAfter(4.0);

            var totalZET = hoursDivision.Values.FirstOrDefault(KeyValuePair => KeyValuePair.Key.Contains("ЗЕТ")).Value;
            var totalHours = hoursDivision.Values.FirstOrDefault(KeyValuePair => KeyValuePair.Key.Contains("Итого часов")).Value;

            var totalContentParagraph = document.InsertParagraph($"Общая трудоемкость (объем) дисциплины составляет {totalZET} зачетных единиц (з.е.), {totalHours} академических часов")
                                                .SpacingAfter(2.0f);

            var hoursDivisionTable = document.AddTable(6, 2);
            hoursDivisionTable.Design = TableDesign.TableGrid;

            foreach (var row in hoursDivisionTable.Rows)
            {
                row.Cells[1].Width = 3.7;
            }

            hoursDivisionTable.AutoFit = AutoFit.Window;

            hoursDivisionTable.MergeCellsInColumn(columnIndex: 0, startRow: 0, endRow: 1);

            hoursDivisionTable.Rows[0].Cells[0].VerticalAlignment = VerticalAlignment.Center;
            hoursDivisionTable.Rows[0].Cells[0].Paragraphs[0].Append("Объем дисциплины")
                                                             .Bold(isBold: true)
                                                             .Alignment = Alignment.center;

            hoursDivisionTable.Rows[0].Cells[1].Paragraphs[0].Append("Всего часов")
                                                             .Bold(isBold: true)
                                                             .Alignment = Alignment.center;

            hoursDivisionTable.Rows[1].Cells[1].Paragraphs[0].Append($"{plan.EducationForm} обучения")
                                                             .Bold(isBold: true)
                                                             .Alignment = Alignment.center;

            hoursDivisionTable.Rows[2].Cells[0].Paragraphs[0].Append("Общая трудоемкость дисциплины");

            hoursDivisionTable.Rows[2].Cells[1].Paragraphs[0].Append(totalHours)
                                                             .Alignment = Alignment.center;

            hoursDivisionTable.Rows[3].Cells[0].Paragraphs[0].Append("Учебных часов на контактную работу с преподавателем:");

            var lectureHours = hoursDivision.Values.FirstOrDefault(KeyValuePair => KeyValuePair.Key.Contains("Лек")).Value;
            hoursDivisionTable.Rows[4].Cells[0].Paragraphs[0].Append("\tлекций");

            hoursDivisionTable.Rows[4].Cells[1].Paragraphs[0].Append(lectureHours)
                                                             .Alignment = Alignment.center;

            var practiceHours = hoursDivision.Values.FirstOrDefault(KeyValuePair => KeyValuePair.Key.Contains("Прак")).Value;
            hoursDivisionTable.Rows[5].Cells[0].Paragraphs[0].Append("\tпрактических (семинарских)");

            hoursDivisionTable.Rows[5].Cells[1].Paragraphs[0].Append(practiceHours)
                                                             .Alignment = Alignment.center;

            var rowIndex = 6;
            var labHours = hoursDivision.Values.FirstOrDefault(KeyValuePair => KeyValuePair.Key.Contains("Лаб")).Value;

            if (!string.IsNullOrEmpty(labHours))
            {
                var labHoursRow = hoursDivisionTable.InsertRow(rowIndex++);
                labHoursRow.Cells[0].Paragraphs[0].Append("\tлабораторных");

                labHoursRow.Cells[1].Paragraphs[0].Append(labHours)
                                                  .Alignment = Alignment.center;
            }

            var contactWorkHours = hoursDivision.Values.FirstOrDefault(KeyValuePair => KeyValuePair.Key.Contains("контактной")).Value;

            if (!string.IsNullOrEmpty(contactWorkHours))
            {
                var contactWorkRow = hoursDivisionTable.InsertRow(rowIndex++);
                contactWorkRow.Cells[0].Paragraphs[0].Append("\tдругие формы контактной работы (ФКР)");

                contactWorkRow.Cells[1].Paragraphs[0].Append(contactWorkHours)
                                                     .Alignment = Alignment.center;
            }

            var controlHours = hoursDivision.Values.FirstOrDefault(KeyValuePair => KeyValuePair.Key.Contains("Часы на контроль")).Value;

            var controlHoursRow = hoursDivisionTable.InsertRow(rowIndex++);
            controlHoursRow.Cells[0].Paragraphs[0].Append("Учебных часов на контроль (включая часы подготовки):");

            controlHoursRow.Cells[1].Paragraphs[0].Append(controlHours)
                                                  .Alignment = Alignment.center;

            foreach (var formOfControl in formsOfControl.Keys)
            {
                var formOfControlRow = hoursDivisionTable.InsertRow(rowIndex++);

                var content = formOfControl.ToLower();
                formOfControlRow.Cells[0].Paragraphs[0].Append($"\t{content}");
            }

            var independentWorkHours = hoursDivision.Values.FirstOrDefault(KeyValuePair => KeyValuePair.Key.Contains("Самостоятельная работа")).Value;

            var independentWorkRow = hoursDivisionTable.InsertRow(rowIndex++);
            independentWorkRow.Cells[0].Paragraphs[0].Append("Учебных часов на самостоятельную работу обучающихся (СР)");

            independentWorkRow.Cells[1].Paragraphs[0].Append(independentWorkHours)
                                                     .Alignment = Alignment.center;

            var courseWorkHours = hoursDivision.Values.FirstOrDefault(KeyValuePair => KeyValuePair.Key.Contains("Курсовая работа")).Value;

            if (!string.IsNullOrEmpty(courseWorkHours))
            {
                independentWorkRow.Cells[0].Paragraphs[0].Append(":");

                var courseWorkHoursRow = hoursDivisionTable.InsertRow(rowIndex++);
                courseWorkHoursRow.Cells[0].Paragraphs[0].Append("\tкурсовая работа");
            }

            totalContentParagraph.InsertTableAfterSelf(hoursDivisionTable);

            document.InsertParagraph()
                    .SpacingAfter(4.0);

            var dormsOfControlParagraph = document.InsertParagraph();

            var formsOfControlTable = document.AddTable(1 + formsOfControl.Count, 2);
            formsOfControlTable.Design = TableDesign.TableGrid;
            formsOfControlTable.AutoFit = AutoFit.Window;

            formsOfControlTable.Rows[0].Cells[0].Paragraphs[0].Append("Формы контроля")
                                                              .Bold(isBold: true)
                                                              .Alignment = Alignment.center;

            formsOfControlTable.Rows[0].Cells[1].Paragraphs[0].Append("Семестры")
                                                              .Bold(isBold: true)
                                                              .Alignment = Alignment.center;

            rowIndex = 1;
            foreach (var keyValuePair in formsOfControl)
            {
                var content = keyValuePair.Key.ToLower();
                formsOfControlTable.Rows[rowIndex].Cells[0].Paragraphs[0].Append(content);

                formsOfControlTable.Rows[rowIndex++].Cells[1].Paragraphs[0].Append(keyValuePair.Value)
                                                                         .Alignment = Alignment.center;
            }

            dormsOfControlParagraph.InsertTableAfterSelf(formsOfControlTable);

            document.InsertParagraph()
                    .SpacingAfter(4.0);
        }

        public void Include(IEnumerable<ThematicPlan> thematicPlans, string lectures, string practices, string labs, string individualWorks)
        {
            var rpdParentItem = rpdItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("4"));
            var parentItemParagraph = document.InsertParagraph($"{rpdParentItem.Number}. {rpdParentItem.Name}")
                                              .Heading(HeadingType.Heading1)
                                              .Bold(isBold: true)
                                              .Color(Color.Black)
                                              .Font("Times New Roman")
                                              .FontSize(12.0)
                                              .SpacingBefore(0.0)
                                              .SpacingAfter(4.0);

            var rpdItem = rpdItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("4.1"));
            var rpdItemParagraph = document.InsertParagraph($"{rpdItem.Number}. {rpdItem.Name}")
                                           .Heading(HeadingType.Heading3)
                                           .Bold(isBold: true)
                                           .Color(Color.Black)
                                           .Font("Times New Roman")
                                           .FontSize(12.0)
                                           .SpacingBefore(0.0)
                                           .SpacingAfter(4.0);

            var thematicPlansTable = document.AddTable(4 + thematicPlans.Count(), columnCount: 6);
            thematicPlansTable.Design = TableDesign.TableGrid;
            thematicPlansTable.AutoFit = AutoFit.Window;

            thematicPlansTable.MergeCellsInColumn(columnIndex: 0, startRow: 0, endRow: 2);
            thematicPlansTable.MergeCellsInColumn(columnIndex: 1, startRow: 0, endRow: 2);

            thematicPlansTable.Rows[0].Cells[0].VerticalAlignment = VerticalAlignment.Center;

            thematicPlansTable.Rows[0].Cells[0].Paragraphs[0].Append("№ п/п")
                                                             .Bold(isBold: true)
                                                             .Alignment = Alignment.center;

            thematicPlansTable.Rows[0].Cells[0].Width = 2.0;

            thematicPlansTable.Rows[0].Cells[1].VerticalAlignment = VerticalAlignment.Center;

            thematicPlansTable.Rows[0].Cells[1].Paragraphs[0].Append("Наименование раздела / темы дисциплины")
                                                             .Bold(isBold: true)
                                                             .Alignment = Alignment.center;

            thematicPlansTable.Rows[0].MergeCells(startIndex: 2, endIndex: 5);

            thematicPlansTable.Rows[0].Cells[2].VerticalAlignment = VerticalAlignment.Center;

            thematicPlansTable.Rows[0].Cells[2].Paragraphs[0].Append("Виды учебных занятий, включая самостоятельную работу обучающихся и трудоемкость (в часах)")
                                                             .Bold(isBold: true)
                                                             .Alignment = Alignment.center;

            thematicPlansTable.Rows[1].MergeCells(startIndex: 2, endIndex: 4);

            thematicPlansTable.Rows[1].Cells[2].Paragraphs[0].Append("Контактная работа с преподавателем")
                                                             .Bold(isBold: true)
                                                             .Alignment = Alignment.center;

            thematicPlansTable.Rows[2].Cells[2].Paragraphs[0].Append("Лек")
                                                             .Bold(isBold: true)
                                                             .Alignment = Alignment.center;

            thematicPlansTable.Rows[2].Cells[3].Paragraphs[0].Append("Пр/Сем")
                                                             .Bold(isBold: true)
                                                             .Alignment = Alignment.center;

            thematicPlansTable.Rows[2].Cells[4].Paragraphs[0].Append("Лаб")
                                                             .Bold(isBold: true)
                                                             .Alignment = Alignment.center;

            thematicPlansTable.MergeCellsInColumn(columnIndex: 5, startRow: 1, endRow: 2);

            thematicPlansTable.Rows[1].Cells[3].Paragraphs[0].Append("СР")
                                                             .Bold(isBold: true)
                                                             .Alignment = Alignment.center;

            thematicPlansTable.Rows[1].Cells[3].VerticalAlignment = VerticalAlignment.Center;

            var rowIndex = 3;
            foreach (var thematicPlan in thematicPlans)
            {
                thematicPlansTable.Rows[rowIndex].Cells[0].Paragraphs[0].Append(thematicPlan.Number);
                thematicPlansTable.Rows[rowIndex].Cells[0].Width = 2.0;
                thematicPlansTable.Rows[rowIndex].Cells[1].Paragraphs[0].Append(thematicPlan.Topic);

                var lectureHours = thematicPlan.Lecture.ToString("0.#");
                thematicPlansTable.Rows[rowIndex].Cells[2].Paragraphs[0].Append(lectureHours)
                                                                        .Alignment = Alignment.center;

                var practiceHours = thematicPlan.Practice.ToString("0.#");
                thematicPlansTable.Rows[rowIndex].Cells[3].Paragraphs[0].Append(practiceHours)
                                                                        .Alignment = Alignment.center;

                var labHours = thematicPlan.Lab.ToString("0.#");
                thematicPlansTable.Rows[rowIndex].Cells[4].Paragraphs[0].Append(labHours)
                                                                        .Alignment = Alignment.center;

                var individualWork = thematicPlan.IndividualWork.ToString("0.#");
                thematicPlansTable.Rows[rowIndex++].Cells[5].Paragraphs[0].Append(individualWork)
                                                                          .Alignment = Alignment.center;
            }

            var lastThematicPlansRow = thematicPlansTable.Rows.Last();
            lastThematicPlansRow.Cells[1].Paragraphs[0].Append("Итого")
                                                       .Bold(isBold: true);

            lastThematicPlansRow.Cells[2].Paragraphs[0].Append(lectures)
                                                       .Bold(isBold: true)
                                                       .Alignment = Alignment.center;

            lastThematicPlansRow.Cells[3].Paragraphs[0].Append(practices)
                                                       .Bold(isBold: true)
                                                       .Alignment = Alignment.center;

            lastThematicPlansRow.Cells[4].Paragraphs[0].Append(labs)
                                                       .Bold(isBold: true)
                                                       .Alignment = Alignment.center;

            lastThematicPlansRow.Cells[5].Paragraphs[0].Append(individualWorks)
                                                       .Bold(isBold: true)
                                                       .Alignment = Alignment.center;

            thematicPlansTable.AutoFit = AutoFit.Contents;
            rpdItemParagraph.InsertTableAfterSelf(thematicPlansTable);

            document.InsertParagraph()
                    .SpacingAfter(4.0);
        }

        public void Include(IDictionary<string, IEnumerable<ThematicContent>> thematicContents)
        {
            var rpdItem = rpdItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("4.2"));
            var rpdItemParagraph = document.InsertParagraph($"{rpdItem.Number}. {rpdItem.Name}")
                                           .Heading(HeadingType.Heading3)
                                           .Bold(isBold: true)
                                           .Color(Color.Black)
                                           .Font("Times New Roman")
                                           .FontSize(12.0)
                                           .SpacingBefore(0.0)
                                           .SpacingAfter(4.0);

            foreach (var keyValuePair in thematicContents)
            {
                var lessonTypeParagraph = document.InsertParagraph(keyValuePair.Key)
                                                  .SpacingAfter(4.0);

                var lessonTypeTable = document.AddTable(1 + keyValuePair.Value.Count(), columnCount: 3);
                lessonTypeTable.Design = TableDesign.TableGrid;
                lessonTypeTable.AutoFit = AutoFit.Window;

                lessonTypeTable.Rows[0].Cells[0].Paragraphs[0].Append("№ п/п")
                                                              .Bold(isBold: true)
                                                              .Alignment = Alignment.center;

                lessonTypeTable.Rows[0].Cells[0].Width = 2.0;

                lessonTypeTable.Rows[0].Cells[1].Paragraphs[0].Append("Наименование раздела / темы дисциплины")
                                                              .Bold(isBold: true)
                                                              .Alignment = Alignment.center;

                lessonTypeTable.Rows[0].Cells[2].Paragraphs[0].Append("Содержание")
                                                              .Bold(isBold: true)
                                                              .Alignment = Alignment.center;

                var rowIndex = 1;
                foreach (var thematicContent in keyValuePair.Value)
                {
                    if (thematicContent.ThematicPlan.IsSection)
                    {
                        lessonTypeTable.Rows[rowIndex].MergeCells(startIndex: 1, endIndex: 2);
                    }
                    else
                    {
                        lessonTypeTable.Rows[rowIndex].Cells[2].Paragraphs[0].Append(thematicContent.Content);
                    }

                    lessonTypeTable.Rows[rowIndex].Cells[0].Paragraphs[0].Append(thematicContent.ThematicPlan.Number)
                                                                         .Alignment = Alignment.center;

                    lessonTypeTable.Rows[rowIndex].Cells[0].Width = 2.0;

                    lessonTypeTable.Rows[rowIndex].Cells[1].Paragraphs[0].Append(thematicContent.ThematicPlan.Topic);

                    rowIndex++;
                }

                lessonTypeParagraph.InsertTableAfterSelf(lessonTypeTable);

                document.InsertParagraph()
                        .SpacingAfter(4.0);
            }
        }

        public void Include(Stream stream)
        {
            var rpdItem = rpdItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("5"));
            var rpdItemParagraph = document.InsertParagraph($"{rpdItem.Number}. {rpdItem.Name}")
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

        public void Include(string text)
        {
            var rpdItem = rpdItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("5"));
            var rpdItemParagraph = document.InsertParagraph($"{rpdItem.Number}. {rpdItem.Name}")
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

        public void Include(IEnumerable<BasicLiterature> basicLiteratures, IEnumerable<AdditionalLiterature> additionalLiteratures)
        {
            var rpdParentItem = rpdItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("6"));
            var rpdParentItemParagraph = document.InsertParagraph($"{rpdParentItem.Number}. {rpdParentItem.Name}")
                                                 .Heading(HeadingType.Heading1)
                                                 .Bold(isBold: true)
                                                 .Color(Color.Black)
                                                 .Font("Times New Roman")
                                                 .FontSize(12.0)
                                                 .SpacingBefore(0.0)
                                                 .SpacingAfter(4.0);

            var rpdItem = rpdItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("6.1"));
            var rpdItemParagraph = document.InsertParagraph($"{rpdItem.Number}. {rpdItem.Name}")
                                           .Heading(HeadingType.Heading3)
                                           .Bold(isBold: true)
                                           .Color(Color.Black)
                                           .Font("Times New Roman")
                                           .FontSize(12.0)
                                           .SpacingBefore(0.0)
                                           .SpacingAfter(4.0);

            var basicLiteraturesParagraph = document.InsertParagraph($"Основная учебная литература:")
                                                     .Bold(isBold: true)
                                                     .SpacingAfter(4.0);

            var basicLiteraturesList = document.AddList();

            foreach (var basicLiterature in basicLiteratures)
            {
                document.AddListItem(basicLiteraturesList, basicLiterature.Title);
            }

            basicLiteraturesParagraph.InsertListAfterSelf(basicLiteraturesList);

            document.InsertParagraph()
                    .SpacingAfter(4.0);

            var additionalLiteraturesParagraph = document.InsertParagraph($"Дополнительная учебная литература:")
                                                     .Bold(isBold: true)
                                                     .SpacingAfter(4.0);

            var additionalLiteraturesList = document.AddList();

            foreach (var additionalLiterature in additionalLiteratures)
            {
                document.AddListItem(additionalLiteraturesList, additionalLiterature.Title);
            }

            additionalLiteraturesParagraph.InsertListAfterSelf(additionalLiteraturesList);

            document.InsertParagraph()
                    .SpacingAfter(4.0);
        }

        public void Include(IEnumerable<LibrarySystem> librarySystems, IEnumerable<InternetResource> internetResources)
        {
            var rpdItem = rpdItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("6.2"));
            var rpdItemParagraph = document.InsertParagraph($"{rpdItem.Number}. {rpdItem.Name}")
                                           .Heading(HeadingType.Heading3)
                                           .Bold(isBold: true)
                                           .Color(Color.Black)
                                           .Font("Times New Roman")
                                           .FontSize(12.0)
                                           .SpacingBefore(0.0)
                                           .SpacingAfter(4.0);

            var librarySystemsTable = document.AddTable(1 + librarySystems.Count(), 3);
            librarySystemsTable.Design = TableDesign.TableGrid;

            librarySystemsTable.Rows[0].Cells[0].Paragraphs[0].Append("№ п/п")
                                                              .Bold(isBold: true)
                                                              .Alignment = Alignment.center;

            librarySystemsTable.Rows[0].Cells[0].Width = 2.0;

            librarySystemsTable.Rows[0].Cells[1].Paragraphs[0].Append("Наименование документа с указанием реквизитов")
                                                              .Bold(isBold: true)
                                                              .Alignment = Alignment.center;

            librarySystemsTable.Rows[0].Cells[2].Paragraphs[0].Append("Срок действия документов")
                                                              .Bold(isBold: true)
                                                              .Alignment = Alignment.center;

            var index = 1;
            foreach (var librarySystem in librarySystems)
            {
                var number = index.ToString();
                librarySystemsTable.Rows[index].Cells[0].Paragraphs[0].Append(number)
                                                                      .Alignment = Alignment.center;

                librarySystemsTable.Rows[index].Cells[0].Width = 2.0;

                librarySystemsTable.Rows[index].Cells[1].Paragraphs[0].Append(librarySystem.NameWithRequisites)
                                                                      .Alignment = Alignment.center;

                librarySystemsTable.Rows[index++].Cells[2].Paragraphs[0].Append(librarySystem.Validity)
                                                                        .Alignment = Alignment.center;
            }

            librarySystemsTable.AutoFit = AutoFit.Window;
            rpdItemParagraph.InsertTableAfterSelf(librarySystemsTable);

            document.InsertParagraph()
                    .SpacingAfter(4.0);

            var internetResourcesParagraph = document.InsertParagraph($"Перечень ресурсов информационно-телекоммуникационной сети «Интернет» (далее - сеть «Интернет»)")
                                                     .Bold(isBold: true)
                                                     .SpacingAfter(4.0);

            var internetResourcesTable = document.AddTable(1 + internetResources.Count(), 3);
            internetResourcesTable.Design = TableDesign.TableGrid;

            internetResourcesTable.Rows[0].Cells[0].Paragraphs[0].Append("№ п/п")
                                                                 .Bold(isBold: true)
                                                                 .Alignment = Alignment.center;

            internetResourcesTable.Rows[0].Cells[0].Width = 2.0;

            internetResourcesTable.Rows[0].Cells[1].Paragraphs[0].Append("Адрес (URL)")
                                                                 .Bold(isBold: true)
                                                                 .Alignment = Alignment.center;

            internetResourcesTable.Rows[0].Cells[2].Paragraphs[0].Append("Описание страницы")
                                                                 .Bold(isBold: true)
                                                                 .Alignment = Alignment.center;

            index = 1;
            foreach (var internetResource in internetResources)
            {
                var number = index.ToString();
                internetResourcesTable.Rows[index].Cells[0].Paragraphs[0].Append(number)
                                                                      .Alignment = Alignment.center;

                internetResourcesTable.Rows[index].Cells[0].Width = 2.0;

                internetResourcesTable.Rows[index].Cells[1].Paragraphs[0].Append(internetResource.URL)
                                                                      .Alignment = Alignment.center;

                internetResourcesTable.Rows[index++].Cells[2].Paragraphs[0].Append(internetResource.Description)
                                                                      .Alignment = Alignment.center;
            }

            internetResourcesTable.AutoFit = AutoFit.Window;
            internetResourcesParagraph.InsertTableAfterSelf(internetResourcesTable);

            document.InsertParagraph()
                    .SpacingAfter(4.0);
        }

        public void Include(IEnumerable<License> licenses)
        {
            var rpdItem = rpdItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("6.3"));
            var rpdItemParagraph = document.InsertParagraph($"{rpdItem.Number}. {rpdItem.Name}")
                                           .Heading(HeadingType.Heading3)
                                           .Bold(isBold: true)
                                           .Color(Color.Black)
                                           .Font("Times New Roman")
                                           .FontSize(12.0)
                                           .SpacingBefore(0.0)
                                           .SpacingAfter(4.0);

            var licensesTable = document.AddTable(1 + licenses.Count(), 1);
            licensesTable.Design = TableDesign.TableGrid;

            licensesTable.Rows[0].Cells[0].Paragraphs[0].Append("Наименование программного обеспечения")
                                                        .Bold(isBold: true)
                                                        .Alignment = Alignment.center;

            var index = 1;
            foreach (var license in licenses)
            {
                licensesTable.Rows[index++].Cells[0].Paragraphs[0].Append(license.ProgramName)
                                                                .Alignment = Alignment.center;
            }

            licensesTable.AutoFit = AutoFit.Window;
            rpdItemParagraph.InsertTableAfterSelf(licensesTable);

            document.InsertParagraph()
                    .SpacingAfter(4.0);
        }

        public void Include(IEnumerable<MaterialBase> materialBases)
        {
            var rpdItem = rpdItems.FirstOrDefault(RPDItem => RPDItem.Number.Equals("7"));
            var rpdItemParagraph = document.InsertParagraph($"{rpdItem.Number}. {rpdItem.Name}")
                                           .Heading(HeadingType.Heading1)
                                           .Bold(isBold: true)
                                           .Color(Color.Black)
                                           .Font("Times New Roman")
                                           .FontSize(12.0)
                                           .SpacingBefore(0.0)
                                           .SpacingAfter(4.0);

            var materialBasesTable = document.AddTable(1 + materialBases.Count(), 2);
            materialBasesTable.Design = TableDesign.TableGrid;

            materialBasesTable.Rows[0].Cells[0].Paragraphs[0].Append("Тип учебной аудитории")
                                                             .Bold(isBold: true)
                                                             .Alignment = Alignment.center;

            materialBasesTable.Rows[0].Cells[1].Paragraphs[0].Append("Оснащенность учебной аудитории")
                                                             .Bold(isBold: true)
                                                             .Alignment = Alignment.center;

            var index = 1;
            foreach (var materialBase in materialBases)
            {
                materialBasesTable.Rows[index].Cells[0].Paragraphs[0].Append(materialBase.Type)
                                                                     .Alignment = Alignment.center;

                materialBasesTable.Rows[index++].Cells[1].Paragraphs[0].Append(materialBase.Equipment)
                                                                       .Alignment = Alignment.center;
            }

            materialBasesTable.AutoFit = AutoFit.Window;
            rpdItemParagraph.InsertTableAfterSelf(materialBasesTable);

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

            document.Save();
        }
    }
}
