using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.DAL.Repositories
{
    public class DecanatDisciplineRepository : IDecanatDisciplineRepository
    {
        private readonly string sqlConnectionString;

        public DecanatDisciplineRepository(string sqlConnectionString)
        {
            this.sqlConnectionString = sqlConnectionString;
        }

        public DecanatDiscipline Get(int id)
        {
            var decanatDiscipline = new DecanatDiscipline();

            var sqlQuery = "SELECT ПланыДисциплины.Код AS [Код дисциплины], Кафедры.Код AS [Код кафедры], " +
                "ПланыДисциплины.Дисциплина AS [Название дисциплины] " +
                "FROM ПланыДисциплины INNER JOIN " +
                "ПланыСтроки ON ПланыДисциплины.Код = ПланыСтроки.КодДисциплины INNER JOIN " +
                "Кафедры ON ПланыСтроки.КодКафедры = Кафедры.Код " +
                $"WHERE (ПланыДисциплины.Код = {id})";

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    while (commandReader.Read())
                    {
                        decanatDiscipline.Id = Convert.ToInt32(commandReader["Код дисциплины"]);
                        decanatDiscipline.DepartmentId = Convert.ToInt32(commandReader["Код кафедры"]);
                        decanatDiscipline.Title = commandReader["Название дисциплины"].ToString();
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return decanatDiscipline;
        }

        public IEnumerable<DecanatDiscipline> GetAll()
        {
            var decanatDisciplines = new List<DecanatDiscipline>();

            var sqlQuery = "SELECT DISTINCT ПланыДисциплины.Код AS [Код дисциплины], Кафедры.Код AS [Код кафедры], " +
                "ПланыДисциплины.Дисциплина AS [Название дисциплины] " +
                "FROM ПланыДисциплины INNER JOIN " +
                "ПланыСтроки ON ПланыДисциплины.Код = ПланыСтроки.КодДисциплины INNER JOIN " +
                "Кафедры ON ПланыСтроки.КодКафедры = Кафедры.Код " +
                "ORDER BY [Название дисциплины]";

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    while (commandReader.Read())
                    {
                        var decanatDiscipline = new DecanatDiscipline()
                        {
                            Id = Convert.ToInt32(commandReader["Код дисциплины"]),
                            DepartmentId = Convert.ToInt32(commandReader["Код кафедры"]),
                            Title = commandReader["Название дисциплины"].ToString()
                        };

                        decanatDisciplines.Add(decanatDiscipline);
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return decanatDisciplines;
        }

        public async Task<IEnumerable<DecanatDiscipline>> GetAllByDepartmentAsync(int departmentId, string year, bool hide)
        {
            List<DecanatDiscipline> decanatDisciplines = new List<DecanatDiscipline>();

            StringBuilder sql = new StringBuilder();
            sql.Append("select  TOP(1) WITH TIES  КодДисциплины, КодКафедры, Дисциплина, ТипОбъекта, SUM(RpdCount) as  RpdCount FROM ");
            sql.Append("(SELECT ПланыСтроки.КодДисциплины, ПланыСтроки.КодКафедры, ПланыСтроки.Дисциплина, ТипОбъекта ");
            sql.Append(",  dbo.GetDiscExistence2(Планы.Код, ПланыСтроки.Код) DiscExists ");
            sql.Append(", (select COUNT(*) from rpTitle (nolock) where ");
            sql.Append("(rpTitle.rupRowID = ПланыСтроки.Код) AND ((rpTitle.hide IS NULL) OR (rpTitle.hide = 0)) AND EXISTS (SELECT rpdId FROM rpContent (nolock) WHERE rpdId = rpTitle.rpdId) ");
            sql.Append(") as  RpdCount ");
            sql.Append("FROM ПланыСтроки (nolock) INNER JOIN Планы (nolock) ON Планы.Код = ПланыСтроки.КодПлана ");
            sql.Append("WHERE ((ПланыСтроки.КодКафедры is null and Планы.КодПрофКафедры = @DepId) OR ПланыСтроки.КодКафедры = @DepId) AND (Планы.УчебныйГод = @AccYear) AND (Планы.СтатусРПД IS NULL) ");
            sql.Append(") as dt ");
            sql.Append("where (ТипОбъекта <> 5 AND ТипОбъекта <> 1) AND ((@hideNotActual = 1 and DiscExists = 1) or (@hideNotActual = 0 and (DiscExists = 0 or DiscExists = 1))) ");
            sql.Append("group by Дисциплина, ТипОбъекта, КодКафедры, КодДисциплины ");
            sql.Append("order by row_number() over (partition by Дисциплина order by КодДисциплины asc)");

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sql.ToString(), sqlConnection);
                sqlCommand.Parameters.AddWithValue("@depId", departmentId);
                sqlCommand.Parameters.AddWithValue("@accYear", year);
                sqlCommand.Parameters.AddWithValue("@hideNotActual", (hide) ? 1 : 0);

                await sqlConnection.OpenAsync();

                using (var commandReader = await sqlCommand.ExecuteReaderAsync())
                {
                    while (await commandReader.ReadAsync())
                    {
                        var decanatDiscipline = new DecanatDiscipline();

                        decanatDiscipline.Id = commandReader.GetInt32(0);
                        decanatDiscipline.DepartmentId = (commandReader["КодКафедры"] != DBNull.Value) ? commandReader.GetInt32(1) : 0;
                        decanatDiscipline.Title = commandReader["Дисциплина"].ToString();
                        decanatDiscipline.TypeObject = commandReader["ТипОбъекта"] != DBNull.Value ? commandReader.GetInt32(3) : -1;

                        decanatDisciplines.Add(decanatDiscipline);
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return decanatDisciplines.OrderBy(a => a.Title).ToList();
        }

        public async Task<IEnumerable<DecanatDiscipline>> GetAllByFilterAsync(int departmentId, string year, bool hide, string filterText)
        {
            List<DecanatDiscipline> decanatDisciplines = new List<DecanatDiscipline>();

            StringBuilder sql = new StringBuilder();
            sql.Append("select  TOP(1) WITH TIES  КодДисциплины, КодКафедры, Дисциплина, ТипОбъекта, SUM(RpdCount) as  RpdCount FROM ");
            sql.Append("(SELECT ПланыСтроки.КодДисциплины, ПланыСтроки.КодКафедры, ПланыСтроки.Дисциплина, ТипОбъекта ");
            sql.Append(",  dbo.GetDiscExistence2(Планы.Код, ПланыСтроки.Код) DiscExists ");
            sql.Append(", (select COUNT(*) from rpTitle (nolock) where ");
            sql.Append("(rpTitle.rupRowID = ПланыСтроки.Код) AND ((rpTitle.hide IS NULL) OR (rpTitle.hide = 0)) AND EXISTS (SELECT rpdId FROM rpContent (nolock) WHERE rpdId = rpTitle.rpdId) ");
            sql.Append(") as  RpdCount ");
            sql.Append("FROM ПланыСтроки (nolock) INNER JOIN Планы (nolock) ON Планы.Код = ПланыСтроки.КодПлана ");
            sql.Append("WHERE ((ПланыСтроки.КодКафедры is null and Планы.КодПрофКафедры = @DepId) OR ПланыСтроки.КодКафедры = @DepId) AND (Планы.УчебныйГод = @AccYear) AND (Планы.СтатусРПД IS NULL) ");
            sql.Append(") as dt ");
            sql.Append($"where (Дисциплина LIKE '%{filterText}%') AND (ТипОбъекта <> 5 AND ТипОбъекта <> 1) AND ((@hideNotActual = 1 and DiscExists = 1) or (@hideNotActual = 0 and (DiscExists = 0 or DiscExists = 1))) ");
            sql.Append("group by Дисциплина, ТипОбъекта, КодКафедры, КодДисциплины ");
            sql.Append("order by row_number() over (partition by Дисциплина order by КодДисциплины asc)");

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sql.ToString(), sqlConnection);
                sqlCommand.Parameters.AddWithValue("@depId", departmentId);
                sqlCommand.Parameters.AddWithValue("@accYear", year);
                sqlCommand.Parameters.AddWithValue("@hideNotActual", (hide) ? 1 : 0);

                await sqlConnection.OpenAsync();

                using (var commandReader = await sqlCommand.ExecuteReaderAsync())
                {
                    while (await commandReader.ReadAsync())
                    {
                        var decanatDiscipline = new DecanatDiscipline();

                        decanatDiscipline.Id = commandReader.GetInt32(0);
                        decanatDiscipline.DepartmentId = (commandReader["КодКафедры"] != DBNull.Value) ? commandReader.GetInt32(1) : 0;
                        decanatDiscipline.Title = commandReader["Дисциплина"].ToString();
                        decanatDiscipline.TypeObject = commandReader["ТипОбъекта"] != DBNull.Value ? commandReader.GetInt32(3) : -1;

                        decanatDisciplines.Add(decanatDiscipline);
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return decanatDisciplines.OrderBy(a => a.Title).ToList();
        }

        public IEnumerable<int> GetDisciplineCoursesByPlan(int planId, int disciplineId)
        {
            var courses = new List<int>();

            var sqlQuery = "SELECT DISTINCT ПланыЧасы.Курс " +
                "FROM ПланыСтроки INNER JOIN " +
                "ПланыЧасы ON ПланыСтроки.Код = ПланыЧасы.КодСтроки " +
                $"WHERE (ПланыСтроки.КодДисциплины = {disciplineId}) AND (ПланыСтроки.КодПлана = {planId}) " +
                "ORDER BY ПланыЧасы.Курс";

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    while (commandReader.Read())
                    {
                        courses.Add(Convert.ToInt32(commandReader["Курс"]));
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return courses;
        }

        public IEnumerable<int> GetDisciplineSemestersByPlan(int planId, int disciplineId)
        {
            var semesters = new List<int>();

            var sqlQuery = "SELECT ПланыЧасы.Семестр " +
                "FROM ПланыСтроки INNER JOIN " +
                "ПланыЧасы ON ПланыСтроки.Код = ПланыЧасы.КодСтроки " +
                $"WHERE (ПланыСтроки.КодДисциплины = {disciplineId}) AND (ПланыСтроки.КодПлана = {planId}) " +
                "ORDER BY ПланыЧасы.Курс";

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    while (commandReader.Read())
                    {
                        semesters.Add(Convert.ToInt32(commandReader["Семестр"]));
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return semesters;
        }

        #region Временное исключение запросов
        //public int GetDisciplineZET(int planId, int disciplineId)
        //{
        //    var ZET = 0;

        //    var sqlQuery = "SELECT ПланыСтроки.ТрудоемкостьКредитов AS [ЗЕТ] " +
        //        "FROM ПланыСтроки INNER JOIN " +
        //        "ПланыЧасы ON ПланыСтроки.Код = ПланыЧасы.КодСтроки " +
        //        $"WHERE (ПланыСтроки.КодПлана = {planId}) AND (ПланыСтроки.КодДисциплины = {disciplineId})";

        //    var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

        //    sqlConnection.Open();

        //    using (var commandReader = sqlCommand.ExecuteReader())
        //    {
        //        if (commandReader.Read())
        //        {
        //            ZET = Convert.ToInt32(commandReader["ЗЕТ"]);
        //        }
        //    }

        //    sqlConnection.Close();

        //    return ZET;
        //}

        //public int GetDisciplineHours(int planId, int disciplineId)
        //{
        //    var hours = 0;

        //    var sqlQuery = "SELECT ПланыСтроки.ЧасовПоПлану AS [Часы] " +
        //        "FROM ПланыСтроки INNER JOIN " +
        //        "ПланыЧасы ON ПланыСтроки.Код = ПланыЧасы.КодСтроки " +
        //        $"WHERE (ПланыСтроки.КодПлана = {planId}) AND (ПланыСтроки.КодДисциплины = {disciplineId})";

        //    var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

        //    sqlConnection.Open();

        //    using (var commandReader = sqlCommand.ExecuteReader())
        //    {
        //        if (commandReader.Read())
        //        {
        //            hours = Convert.ToInt32(commandReader["Часы"]);
        //        }
        //    }

        //    sqlConnection.Close();

        //    return hours;
        //}

        //public int GetDisciplineKSR(int planId, int disciplineId)
        //{
        //    var KSR = 0;

        //    var sqlQuery = "SELECT ПланыЧасы.КСР AS [КСР] " +
        //        "FROM ПланыСтроки INNER JOIN " +
        //        "ПланыЧасы ON ПланыСтроки.Код = ПланыЧасы.КодСтроки " +
        //        $"WHERE (ПланыСтроки.КодПлана = {planId}) AND (ПланыСтроки.КодДисциплины = {disciplineId})";

        //    var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

        //    sqlConnection.Open();

        //    using (var commandReader = sqlCommand.ExecuteReader())
        //    {
        //        if (commandReader.Read())
        //        {
        //            KSR = Convert.ToInt32(commandReader["КСР"]);
        //        }
        //    }

        //    sqlConnection.Close();

        //    return KSR;
        //}
        #endregion

        public DecanatHoursDivision GetDisciplineHoursDivision(int planId, int disciplineId)
        {
            var disciplineHoursDivision = new DecanatHoursDivision();

            var sqlQuery = "SELECT DISTINCT ПланыСтроки.Код, Планы.Код AS [Код плана], ПланыСтроки.КодДисциплины AS [Код дисциплины], Планы.КодФормыОбучения AS [Код формы обучения], " +
                "СправочникВидыРабот.Название AS [Вид работы], SUM(ПланыНовыеЧасы.Количество) AS [Часы по виду работ] " +
                "FROM ПланыНовыеЧасы INNER JOIN " +
                "Планы INNER JOIN " +
                "ПланыСтроки ON Планы.Код = ПланыСтроки.КодПлана ON ПланыНовыеЧасы.КодОбъекта = ПланыСтроки.Код INNER JOIN " +
                "ПланыЧасы ON ПланыСтроки.Код = ПланыЧасы.КодСтроки INNER JOIN " +
                "СправочникВидыРабот ON ПланыНовыеЧасы.КодВидаРаботы = СправочникВидыРабот.Код " +
                $"WHERE(Планы.Код = {planId}) AND (ПланыСтроки.КодДисциплины = {disciplineId}) " +
                "GROUP BY ПланыЧасы.Курс, ПланыЧасы.Семестр, СправочникВидыРабот.Название, Планы.КодФормыОбучения, " +
                "ПланыНовыеЧасы.КодВидаРаботы, ПланыСтроки.Код, ПланыСтроки.КодДисциплины, Планы.Код " +
                "HAVING (ПланыНовыеЧасы.КодВидаРаботы <> 1)";

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    var totalKey = string.Empty;
                    var totalValue = string.Empty;

                    while (commandReader.Read())
                    {
                        var facilityFormId = Convert.ToInt32(commandReader["Код формы обучения"]);
                        disciplineHoursDivision.Id = Convert.ToInt32(commandReader["Код"]);
                        disciplineHoursDivision.PlanId = Convert.ToInt32(commandReader["Код плана"]);
                        disciplineHoursDivision.DisciplineId = Convert.ToInt32(commandReader["Код дисциплины"]);

                        var key = commandReader["Вид работы"].ToString();
                        var hours = Convert.ToDouble(commandReader["Часы по виду работ"]);
                        hours = (facilityFormId.Equals(2)) ? hours / 2.0 : hours;
                        var value = Math.Round(hours, 1).ToString();
                        value = (key.Equals("Зачет") || key.Equals("Зачет с оценкой")) ? "+" : value;

                        if (key.Contains("Итого"))
                        {
                            totalKey = key;
                            totalValue = value;

                            continue;
                        }

                        disciplineHoursDivision.Values.Add(key, value);
                    }

                    disciplineHoursDivision.Values.Add(totalKey, totalValue);

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return disciplineHoursDivision;
        }

        public string GetWorkHourDivision(int planId, int disciplineId, string workTypeName)
        {
            var workHourDivision = string.Empty;

            var sqlQuery = "SELECT DISTINCT ПланыСтроки.Код, Планы.Код AS [Код плана], ПланыСтроки.КодДисциплины AS [Код дисциплины], Планы.КодФормыОбучения AS [Код формы обучения], " +
                "СправочникВидыРабот.Название AS [Вид работы], SUM(ПланыНовыеЧасы.Количество) AS [Часы по виду работ] " +
                "FROM ПланыНовыеЧасы INNER JOIN " +
                "Планы INNER JOIN " +
                "ПланыСтроки ON Планы.Код = ПланыСтроки.КодПлана ON ПланыНовыеЧасы.КодОбъекта = ПланыСтроки.Код INNER JOIN " +
                "ПланыЧасы ON ПланыСтроки.Код = ПланыЧасы.КодСтроки INNER JOIN " +
                "СправочникВидыРабот ON ПланыНовыеЧасы.КодВидаРаботы = СправочникВидыРабот.Код " +
                $"WHERE(Планы.Код = {planId}) AND (ПланыСтроки.КодДисциплины = {disciplineId}) AND (СправочникВидыРабот.Название LIKE '%{workTypeName}%' ) " +
                "GROUP BY ПланыЧасы.Курс, ПланыЧасы.Семестр, СправочникВидыРабот.Название, Планы.КодФормыОбучения, " +
                "ПланыНовыеЧасы.КодВидаРаботы, ПланыСтроки.Код, ПланыСтроки.КодДисциплины, Планы.Код " +
                "HAVING (ПланыНовыеЧасы.КодВидаРаботы <> 1)";

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    if (commandReader.Read())
                    {
                        var facilityFormId = Convert.ToInt32(commandReader["Код формы обучения"]);
                        var key = commandReader["Вид работы"].ToString();
                        var hours = Convert.ToDouble(commandReader["Часы по виду работ"]);
                        hours = (facilityFormId.Equals(2)) ? hours / 2.0 : hours;
                        var value = Math.Round(hours, 1).ToString();
                        value = (key.Equals("Зачет") || key.Equals("Зачет с оценкой")) ? "+" : value;
                        workHourDivision = value;
                    }
                    else
                    {
                        workHourDivision = "0";
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return workHourDivision;
        }

        public IDictionary<string, string> GetDisciplineControlSemesters(int planId, int disciplineId)
        {
            var controlSemesters = new Dictionary<string, string>();

            var sqlQuery = "SELECT dbo.Планы.Код, dbo.СправочникВидыРабот.Название as [Вид работы], dbo.ПланыНовыеЧасы.Курс, dbo.ПланыНовыеЧасы.Семестр " +
                           "FROM dbo.ПланыНовыеЧасы INNER JOIN " +
                           "dbo.Планы INNER JOIN " +
                           "dbo.ПланыСтроки ON dbo.Планы.Код = dbo.ПланыСтроки.КодПлана ON dbo.ПланыНовыеЧасы.КодОбъекта = dbo.ПланыСтроки.Код INNER JOIN " +
                           "dbo.СправочникВидыРабот ON dbo.ПланыНовыеЧасы.КодВидаРаботы = dbo.СправочникВидыРабот.Код " +
                           $"WHERE (dbo.Планы.Код = {planId}) AND (dbo.ПланыСтроки.КодДисциплины = {disciplineId}) AND (dbo.СправочникВидыРабот.Код <= 5)";

            #region Старый запрос, не удалять!
            //var sqlQuery = "SELECT DISTINCT СправочникВидыРабот.Название AS [Вид работы], " +
            //    "COALESCE(STUFF((SELECT ', ' + CAST(ПланыЧасы.Семестр AS VARCHAR(MAX)) AS[text()] " +
            //    "FROM Планы INNER JOIN " +
            //    "ПланыСтроки ON Планы.Код = ПланыСтроки.КодПлана INNER JOIN " +
            //    "ПланыЧасы ON ПланыСтроки.Код = ПланыЧасы.КодСтроки " +
            //    $"WHERE (ПланыСтроки.КодДисциплины = {disciplineId}) AND (Планы.Код = {planId}) " +
            //    "FOR XML PATH(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 2, ''), '') AS [Семестры] " +
            //    "FROM ПланыНовыеЧасы INNER JOIN " +
            //    "Планы INNER JOIN " +
            //    "ПланыСтроки ON Планы.Код = ПланыСтроки.КодПлана ON ПланыНовыеЧасы.КодОбъекта = ПланыСтроки.Код INNER JOIN " +
            //    "СправочникВидыРабот ON ПланыНовыеЧасы.КодВидаРаботы = СправочникВидыРабот.Код " +
            //    $"WHERE (ПланыСтроки.КодДисциплины = {disciplineId}) AND (Планы.Код = {planId}) AND (СправочникВидыРабот.Код <= 5)";
            #endregion

            var semesters = new List<ControlSemesters>();

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    while (commandReader.Read())
                    {
                        var title = commandReader["Вид работы"].ToString();
                        var course = Convert.ToInt32(commandReader["Курс"]);
                        var semester = Convert.ToInt32(commandReader["Семестр"]);
                        semester = (semester % 2 == 0) ? course * semester : course * semester + (course - 1);

                        if (title.Equals("Зачет с оценкой") || title.Equals("Зачёт с оценкой"))
                        {
                            title = "Дифференцированный зачет";
                        }

                        semesters.Add(new ControlSemesters()
                        {
                            Title = title,
                            Course = course,
                            Semester = semester
                        });
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            var groupingSemesters = semesters.GroupBy(Semester => Semester.Title);

            foreach (var list in groupingSemesters)
            {
                var key = list.Key;
                var values = list.FirstOrDefault().Semester.ToString();

                var isFirst = true;
                foreach (var keyValuePair in list)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        continue;
                    }

                    values += $", {keyValuePair.Semester}";
                }

                controlSemesters.Add(key, values);
            }

            return controlSemesters;
        }
    }
}
