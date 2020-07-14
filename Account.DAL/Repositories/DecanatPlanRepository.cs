using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Account.DAL.Repositories
{
    public class DecanatPlanRepository : IDecanatPlanRepository
    {
        private readonly string sqlConnectionString;

        public DecanatPlanRepository(string sqlConnectionString)
        {
            this.sqlConnectionString = sqlConnectionString;
        }

        public DecanatPlan Get(int id)
        {
            var decanatPlan = new DecanatPlan();

            var sqlQuery = "SELECT Планы.Код AS [Код плана], ПланыСтроки.КодДисциплины AS [Код дисциплины], Кафедры.Код AS [Код кафедры], " +
                "Планы.ИмяФайла AS [Имя файла], Планы.Специальность AS [Шифр направления], Планы.Титул, Специальности.Код AS [Код направления], " +
                "Специальности.Название_Спец AS [Название направления], Планы.Квалификация, ФормаОбучения.Код AS [Код формы обучения], " +
                "ФормаОбучения.ФормаОбучения AS [Форма обучения], Планы.УчебныйГод AS [Учебный год], dbo.ПланыСтроки.ДисциплинаКод AS [Блок], " +
                "Планы.ТипГОСа " +
                "FROM ПланыСтроки INNER JOIN " +
                "Планы ON ПланыСтроки.КодПлана = Планы.Код INNER JOIN " +
                "Кафедры ON ПланыСтроки.КодКафедры = Кафедры.Код INNER JOIN " +
                "ФормаОбучения ON Планы.КодФормыОбучения = ФормаОбучения.Код INNER JOIN " +
                "Специальности ON Планы.КодСпециальности = Специальности.Код " +
                $"WHERE (Планы.Код = {id}) " +
                "ORDER BY [Имя файла]";

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    while (commandReader.Read())
                    {
                        decanatPlan.Id = Convert.ToInt32(commandReader["Код плана"]);
                        decanatPlan.DepartmentId = Convert.ToInt32(commandReader["Код кафедры"]);
                        decanatPlan.DisciplineId = Convert.ToInt32(commandReader["Код дисциплины"]);
                        decanatPlan.Filename = commandReader["Имя файла"].ToString();
                        decanatPlan.EducationFormId = Convert.ToInt32(commandReader["Код формы обучения"]);
                        decanatPlan.EducationForm = commandReader["Форма обучения"].ToString();
                        decanatPlan.Qualification = commandReader["Квалификация"].ToString();
                        decanatPlan.FGOSId = commandReader["ТипГОСа"].ToString();
                        decanatPlan.FGOS = decanatPlan.FGOSId.Equals("3,5") ? "ФГОС 3+" : "ФГОС 3++";
                        decanatPlan.ProfileId = Convert.ToInt32(commandReader["Код направления"]);
                        decanatPlan.Profile = commandReader["Название направления"].ToString();
                        decanatPlan.ProgramRequisites = commandReader["Титул"].ToString();
                        decanatPlan.Block = commandReader["Блок"].ToString();
                        decanatPlan.ProfileCode = commandReader["Шифр направления"].ToString();
                        decanatPlan.StudyYear = commandReader["Учебный год"].ToString();
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return decanatPlan;
        }

        public IEnumerable<DecanatPlan> GetAll()
        {
            var decanatPlans = new List<DecanatPlan>();

            var sqlQuery = "SELECT Планы.Код AS [Код плана], ПланыСтроки.КодДисциплины AS [Код дисциплины], Кафедры.Код AS [Код кафедры], " +
                "Планы.ИмяФайла AS [Имя файла], Планы.Специальность AS [Шифр направления], Планы.Титул, Специальности.Код AS [Код направления], " +
                "Специальности.Название_Спец AS [Название направления], Планы.Квалификация, ФормаОбучения.Код AS [Код формы обучения], " +
                "ФормаОбучения.ФормаОбучения AS [Форма обучения], Планы.УчебныйГод AS [Учебный год], dbo.ПланыСтроки.ДисциплинаКод AS [Блок], " +
                "Планы.ТипГОСа " +
                "FROM ПланыСтроки INNER JOIN " +
                "Планы ON ПланыСтроки.КодПлана = Планы.Код INNER JOIN " +
                "Кафедры ON ПланыСтроки.КодКафедры = Кафедры.Код INNER JOIN " +
                "ФормаОбучения ON Планы.КодФормыОбучения = ФормаОбучения.Код INNER JOIN " +
                "Специальности ON Планы.КодСпециальности = Специальности.Код " +
                "ORDER BY [Имя файла]";

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    while (commandReader.Read())
                    {
                        var decanatPlan = new DecanatPlan
                        {
                            Id = Convert.ToInt32(commandReader["Код плана"]),
                            DepartmentId = Convert.ToInt32(commandReader["Код кафедры"]),
                            DisciplineId = Convert.ToInt32(commandReader["Код дисциплины"]),
                            Filename = commandReader["Имя файла"].ToString(),
                            EducationFormId = Convert.ToInt32(commandReader["Код формы обучения"]),
                            EducationForm = commandReader["Форма обучения"].ToString(),
                            Qualification = commandReader["Квалификация"].ToString(),
                            FGOSId = commandReader["ТипГОСа"].ToString(),
                            ProfileId = Convert.ToInt32(commandReader["Код направления"]),
                            Profile = commandReader["Название направления"].ToString(),
                            ProgramRequisites = commandReader["Титул"].ToString(),
                            Block = commandReader["Блок"].ToString(),
                            ProfileCode = commandReader["Шифр направления"].ToString(),
                            StudyYear = commandReader["Учебный год"].ToString()
                        };

                        decanatPlan.FGOS = decanatPlan.FGOSId.Equals("3,5") ? "ФГОС 3+" : "ФГОС 3++";

                        decanatPlans.Add(decanatPlan);
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return decanatPlans;
        }

        public IEnumerable<DecanatPlan> GetAllByDiscipline(string discipline, int departmentId, int typeObj, string year, bool hide)
        {
            var decanatPlans = new List<DecanatPlan>();
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT Планы.Специальность AS [Шифр направления], Планы.Код AS [Код плана], ДисциплиныПланов.Код AS [Код дисциплины], Планы.ИмяФайла AS [Имя Файла], ");
            sql.Append("Планы.Титул, ТипОбъекта, Планы.КодФормыОбучения AS [Код формы обучения], ФормаОбучения.ФормаОбучения AS [Форма обучения], Планы.Квалификация, ");
            sql.Append("ООП.Название AS [Название направления], Планы.КодСпециальности AS [Код направления], Планы.КодПрофКафедры AS [Код кафедры], ");
            sql.Append("Планы.[ТипГОСа],");
            sql.Append("ДисциплиныПланов.ДисциплинаКод AS [Блок], Планы.УчебныйГод AS [Учебный год] ");
            sql.Append("FROM Планы ");
            sql.Append("INNER JOIN ДисциплиныПланов ON Планы.Код = ДисциплиныПланов.КодПлана ");
            sql.Append("LEFT JOIN ООП ON ДисциплиныПланов.КодООП = ООП.Код ");
            sql.Append("INNER JOIN ФормаОбучения ON КодФормыОбучения = ФормаОбучения.Код ");
            sql.Append("INNER JOIN Уровень_образования ON КодУровняОбразования = Уровень_образования.Код_записи ");
            sql.Append("WHERE (");
            sql.Append("ISNULL(ДисциплиныПланов.ТипОбъекта, 0) = @ObjType ");
            sql.Append("AND((Планы.УчебныйГод = @AccYear) AND((Планы.Дистанционное IS NULL) OR(Планы.Дистанционное = 0)) AND(Планы.СтатусРПД IS NULL)  ");
            sql.Append("AND(ДисциплиныПланов.Дисциплина = @DiscName)  AND((ДисциплиныПланов.КодКафедры is null and Планы.КодПрофКафедры = @DepId) OR ДисциплиныПланов.КодКафедры = @DepId)) ");

            if (!hide)
            {
                sql.Append(") AND((EXISTS ");
                sql.Append("(SELECT ПланыЧасы.Код FROM ПланыЧасы WHERE(ПланыЧасы.КодСтроки = ДисциплиныПланов.Код) AND ПланыЧасы.Курс > 0)) ");
                sql.Append("OR (EXISTS (SELECT Код FROM ПланыНовыеЧасы WHERE ПланыНовыеЧасы.КодОбъекта = ДисциплиныПланов.Код AND ПланыНовыеЧасы.Курс > 0))) ");
                sql.Append("AND ((ДисциплиныПланов.ЗЕТфакт - ISNULL(ДисциплиныПланов.ЗЕТизучено, 0)) <> 0 OR ДисциплиныПланов.ЗЕТфакт IS NULL ");
                sql.Append("OR (ДисциплиныПланов.ЗЕТфакт = 0 AND ДисциплиныПланов.ЧасовПоПлану <> 0)) ");
            }
            else
            {
                sql.Append(") AND(EXISTS (SELECT ПланыГрафики.Код FROM ПланыГрафики INNER JOIN ПланыЧасы ON ПланыГрафики.Курс = ПланыЧасы.Курс ");
                sql.Append("WHERE(ПланыГрафики.ЧислоГрупп > 0) AND(ПланыГрафики.КодПлана = ДисциплиныПланов.КодПлана) AND(ПланыЧасы.КодСтроки = ДисциплиныПланов.Код) ");
                sql.Append("AND ПланыЧасы.Курс > 0) OR(EXISTS (SELECT ПланыКонтингент.Код FROM ПланыКонтингент INNER JOIN ПланыНовыеЧасы ON ПланыКонтингент.Курс = ПланыНовыеЧасы.Курс ");
                sql.Append("WHERE(ПланыКонтингент.ЧислоГрупп > 0) AND(ПланыКонтингент.КодПлана = ДисциплиныПланов.КодПлана) AND(ПланыНовыеЧасы.КодОбъекта = ДисциплиныПланов.Код) ");
                sql.Append("AND ПланыНовыеЧасы.Курс > 0))) ");
                sql.Append("AND ((ISNULL(ДисциплиныПланов.ЗЕТфакт, 0) - ISNULL(ДисциплиныПланов.ЗЕТизучено, 0)) <> 0) ");
            }
            sql.Append("GROUP BY ДисциплиныПланов.Код, Планы.Специальность, Планы.ИмяФайла,  Планы.Специальность, Планы.Титул, ТипОбъекта, КодФормыОбучения, КодУровняОбразования, ООП.Название, Планы.КодПрофКафедры, ");
            sql.Append("Планы.ТипГОСа, ДисциплиныПланов.ДисциплинаКод, ФормаОбучения.ФормаОбучения, Планы.Квалификация, Планы.КодСпециальности, Планы.Код, Планы.УчебныйГод ");
            sql.Append("ORDER BY Планы.ИмяФайла ");

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sql.ToString(), sqlConnection);

                sqlCommand.Parameters.AddWithValue("@depId", departmentId);
                sqlCommand.Parameters.AddWithValue("@accYear", year);
                sqlCommand.Parameters.AddWithValue("@discName", discipline);
                sqlCommand.Parameters.AddWithValue("@ObjType", typeObj);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    while (commandReader.Read())
                    {
                        var decanatPlan = new DecanatPlan
                        {
                            Id = Convert.ToInt32(commandReader["Код плана"]),
                            DepartmentId = Convert.ToInt32(commandReader["Код кафедры"]),
                            DisciplineId = Convert.ToInt32(commandReader["Код дисциплины"]),
                            Filename = commandReader["Имя файла"].ToString(),
                            EducationFormId = Convert.ToInt32(commandReader["Код формы обучения"]),
                            EducationForm = commandReader["Форма обучения"].ToString(),
                            Qualification = commandReader["Квалификация"].ToString(),
                            FGOSId = (commandReader["ТипГОСа"] != DBNull.Value) ? commandReader["ТипГОСа"].ToString() : "",
                            ProfileId = Convert.ToInt32(commandReader["Код направления"]),
                            Profile = commandReader["Название направления"].ToString(),
                            ProgramRequisites = commandReader["Титул"].ToString(),
                            Block = commandReader["Блок"].ToString(),
                            ProfileCode = commandReader["Шифр направления"].ToString(),
                            StudyYear = commandReader["Учебный год"].ToString()
                        };

                        decanatPlan.FGOS = decanatPlan.FGOSId.Equals("3,5") ? "ФГОС 3+" : "ФГОС 3++";

                        decanatPlans.Add(decanatPlan);
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return decanatPlans;
        }        
    }
}
