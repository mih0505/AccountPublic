using Account.DAL.Infrastructure;
using Account.DAL.Interfaces;
using Account.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace Account.DAL.UnitOfWorks
{
    public class DecanatUnitOfWork : IDecanatUnitOfWork
    {
        private string sqlConnectionString;

        public IDecanatDepartmentRepository Departments { get; set; }
        public IDecanatDisciplineRepository Disciplines { get; set; }
        public IDecanatPlanRepository Plans { get; set; }
        public IDecanatCompetenceRepository Competences { get; set; }

        public DecanatUnitOfWork()
        {
            sqlConnectionString = SecureConnectionString.GetSecureConnectionString().Decanat;
            
            Departments = new DecanatDepartmentRepository(sqlConnectionString);
            Disciplines = new DecanatDisciplineRepository(sqlConnectionString);
            Plans = new DecanatPlanRepository(sqlConnectionString);
            Competences = new DecanatCompetenceRepository(sqlConnectionString);
        }

        public IEnumerable<string> GetAllStudyYears()
        {
            var years = new List<string>();

            var sqlQuery = "SELECT DISTINCT УчебныйГод AS [Учебный год] FROM Планы";

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    while (commandReader.Read())
                    {
                        var year = commandReader["Учебный год"].ToString();

                        years.Add(year);
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return years;
        }

        public string GetStudyYear(DateTime currentDate)
        {
            var studyYears = GetAllStudyYears();
            var currentYear = currentDate.Year.ToString();

            Func<string, bool> predicate = Year => Year.Contains(currentYear);
            return (currentDate.Month < 9) ? studyYears.FirstOrDefault(predicate) : studyYears.LastOrDefault(predicate);
        }

        public void Dispose()
        {
            sqlConnectionString = null;
        }
    }
}
