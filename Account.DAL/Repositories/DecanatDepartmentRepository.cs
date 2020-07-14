using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Account.DAL.Repositories
{
    class DecanatDepartmentRepository : IDecanatDepartmentRepository
    {
        private readonly string sqlConnectionString;

        public DecanatDepartmentRepository(string sqlConnectionString)
        {
            this.sqlConnectionString = sqlConnectionString;
        }

        public DecanatDepartment Get(int id)
        {
            var decanatDepartment = new DecanatDepartment();

            var sqlQuery = "SELECT Кафедры.Код, Факультеты.Код AS [Код факультета], Факультеты.Факультет, " +
                "Кафедры.Название AS [Кафедра], Кафедры.ЗавКафедрой AS [Заведующий кафедрой] " +
                "FROM Кафедры INNER JOIN " +
                "Факультеты ON Кафедры.Код_Факультета = Факультеты.Код " +
                $"WHERE (Факультеты.Код IS NOT NULL) AND (Кафедры.Код = {id}) " +
                "ORDER BY Кафедра";

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    while (commandReader.Read())
                    {
                        decanatDepartment.Id = Convert.ToInt32(commandReader["Код"]);
                        decanatDepartment.FacultyId = Convert.ToInt32(commandReader["Код факультета"]);
                        decanatDepartment.FacultyTitle = commandReader["Факультет"].ToString();
                        decanatDepartment.Title = commandReader["Кафедра"].ToString();
                        decanatDepartment.Chief = commandReader["Заведующий кафедрой"].ToString();
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return decanatDepartment;
        }

        public IEnumerable<DecanatDepartment> GetAll()
        {
            var decanatDepartments = new List<DecanatDepartment>();

            var sqlQuery = "SELECT Кафедры.Код, Факультеты.Код AS [Код факультета], Факультеты.Факультет, " +
                "Кафедры.Название AS [Кафедра], Кафедры.ЗавКафедрой AS [Заведующий кафедрой] " +
                "FROM Кафедры INNER JOIN " +
                "Факультеты ON Кафедры.Код_Факультета = Факультеты.Код " +
                $"WHERE (Факультеты.Код IS NOT NULL) " +
                "ORDER BY Кафедра";

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    while (commandReader.Read())
                    {
                        var decanatDepartment = new DecanatDepartment()
                        {
                            Id = Convert.ToInt32(commandReader["Код"]),
                            FacultyId = Convert.ToInt32(commandReader["Код факультета"]),
                            FacultyTitle = commandReader["Факультет"].ToString(),
                            Title = commandReader["Кафедра"].ToString(),
                            Chief = commandReader["Заведующий кафедрой"].ToString()
                        };

                        decanatDepartments.Add(decanatDepartment);
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return decanatDepartments;
        }

        public IEnumerable<DecanatDepartment> GetUserDepartments(List<int> departmentsId)
        {
            var decanatDepartments = new List<DecanatDepartment>();

            if (departmentsId.Count.Equals(0))
            {
                return decanatDepartments;
            }

            string filter = departmentsId[0].ToString();

            for (int i = 1; i < departmentsId.Count; i++)
            {
                filter += $", {departmentsId[i].ToString()}";
            }

            var sqlQuery = "SELECT Кафедры.Код, Факультеты.Код AS [Код факультета], Факультеты.Факультет, " +
                "Кафедры.Название AS [Кафедра], Кафедры.ЗавКафедрой AS [Заведующий кафедрой] " +
                "FROM Кафедры INNER JOIN " +
                "Факультеты ON Кафедры.Код_Факультета = Факультеты.Код " +
                $"WHERE (Факультеты.Код IS NOT NULL) AND (Кафедры.Код IN ({filter})) " +
                "ORDER BY Кафедра";

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    while (commandReader.Read())
                    {
                        var decanatDepartment = new DecanatDepartment()
                        {
                            Id = Convert.ToInt32(commandReader["Код"]),
                            FacultyId = Convert.ToInt32(commandReader["Код факультета"]),
                            FacultyTitle = commandReader["Факультет"].ToString(),
                            Title = commandReader["Кафедра"].ToString(),
                            Chief = commandReader["Заведующий кафедрой"].ToString()
                        };

                        decanatDepartments.Add(decanatDepartment);
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return decanatDepartments;
        }
    }
}
