using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.DAL.Repositories
{
    public class DecanatCompetenceRepository : IDecanatCompetenceRepository
    {
        private readonly string sqlConnectionString;

        public DecanatCompetenceRepository(string sqlConnectionString)
        {
            this.sqlConnectionString = sqlConnectionString;
        }

        public DecanatCompetence Get(int id)
        {
            var decanatCompetence = new DecanatCompetence();

            var sqlQuery = "SELECT ПланыКомпетенции.Код, ПланыСтроки.КодПлана AS [Код плана], ПланыСтроки.КодДисциплины AS [Код дисциплины], " +
                "ПланыКомпетенции.Номер, ПланыКомпетенции.ШифрКомпетенции AS [Шифр], ПланыКомпетенции.Наименование AS [Содержание] " +
                "FROM ПланыСтроки INNER JOIN " +
                "ПланыКомпетенцииДисциплины ON ПланыСтроки.Код = ПланыКомпетенцииДисциплины.КодСтроки INNER JOIN " +
                "ПланыКомпетенции ON ПланыКомпетенцииДисциплины.КодКомпетенции = ПланыКомпетенции.Код " +
                $"WHERE (ПланыКомпетенции.Код = {id})" +
                "ORDER BY ПланыКомпетенции.Номер";

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    while (commandReader.Read())
                    {
                        decanatCompetence.Id = Convert.ToInt32(commandReader["Код"]);
                        decanatCompetence.PlanId = Convert.ToInt32(commandReader["Код плана"]);
                        decanatCompetence.DisciplineId = Convert.ToInt32(commandReader["Код дисциплины"]);
                        decanatCompetence.Number = Convert.ToInt32(commandReader["Номер"]);
                        decanatCompetence.Code = commandReader["Шифр"].ToString();
                        decanatCompetence.Content = commandReader["Содержание"].ToString();
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return decanatCompetence;
        }

        public IEnumerable<DecanatCompetence> GetAll()
        {
            var decanatCompetences = new List<DecanatCompetence>();

            var sqlQuery = "SELECT ПланыКомпетенции.Код, ПланыСтроки.КодПлана AS [Код плана], ПланыСтроки.КодДисциплины AS [Код дисциплины], " +
                "ПланыКомпетенции.Номер, ПланыКомпетенции.ШифрКомпетенции AS [Шифр], ПланыКомпетенции.Наименование AS [Содержание] " +
                "FROM ПланыСтроки INNER JOIN " +
                "ПланыКомпетенцииДисциплины ON ПланыСтроки.Код = ПланыКомпетенцииДисциплины.КодСтроки INNER JOIN " +
                "ПланыКомпетенции ON ПланыКомпетенцииДисциплины.КодКомпетенции = ПланыКомпетенции.Код " +
                "ORDER BY ПланыКомпетенции.Номер";

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    while (commandReader.Read())
                    {
                        var decanatCompetence = new DecanatCompetence()
                        {
                            Id = Convert.ToInt32(commandReader["Код"]),
                            PlanId = Convert.ToInt32(commandReader["Код плана"]),
                            DisciplineId = Convert.ToInt32(commandReader["Код дисциплины"]),
                            Number = Convert.ToInt32(commandReader["Номер"]),
                            Code = commandReader["Шифр"].ToString(),
                            Content = commandReader["Содержание"].ToString()
                        };

                        decanatCompetences.Add(decanatCompetence);
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return decanatCompetences;
        }

        public IEnumerable<DecanatCompetence> GetAllByDiscipline(int disciplineId, int planId)
        {
            var decanatCompetences = new List<DecanatCompetence>();

            var sqlQuery = "SELECT ПланыКомпетенции.Код, ПланыСтроки.КодПлана AS [Код плана], ПланыСтроки.КодДисциплины AS [Код дисциплины], " +
                "ПланыКомпетенции.Номер, ПланыКомпетенции.ШифрКомпетенции AS [Шифр], ПланыКомпетенции.Наименование AS [Содержание] " +
                "FROM ПланыСтроки INNER JOIN " +
                "ПланыКомпетенцииДисциплины ON ПланыСтроки.Код = ПланыКомпетенцииДисциплины.КодСтроки INNER JOIN " +
                "ПланыКомпетенции ON ПланыКомпетенцииДисциплины.КодКомпетенции = ПланыКомпетенции.Код " +
                $"WHERE (ПланыСтроки.КодДисциплины = {disciplineId}) AND (ПланыКомпетенции.КодПлана = {planId})" +
                "ORDER BY ПланыКомпетенции.Номер";

            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlConnection.Open();

                using (var commandReader = sqlCommand.ExecuteReader())
                {
                    while (commandReader.Read())
                    {
                        var decanatCompetence = new DecanatCompetence()
                        {
                            Id = Convert.ToInt32(commandReader["Код"]),
                            PlanId = Convert.ToInt32(commandReader["Код плана"]),
                            DisciplineId = Convert.ToInt32(commandReader["Код дисциплины"]),
                            Number = Convert.ToInt32(commandReader["Номер"]),
                            Code = commandReader["Шифр"].ToString(),
                            Content = commandReader["Содержание"].ToString()
                        };

                        decanatCompetences.Add(decanatCompetence);
                    }

                    commandReader.Close();
                }

                sqlConnection.Close();
            }

            return decanatCompetences;
        }
    }
}
