using Accounts.Models;
using Npgsql;
using SyncAccount.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SyncAccount.Sync
{
    public class SyncGroup
    {
        private readonly string connectionString = "Server=*;Port=5432;User Id=*;Password=*;Database=moodledb;";
        private ApplicationDbContext db = new ApplicationDbContext();
        /// <summary>
        /// Получение списка групп из деканата
        /// </summary>   
        private async Task<List<GroupViewModel>> GetGroupsDecanatAsync(string currentYear)
        {
            string sql = "SELECT TOP 100 PERCENT dbo.Все_Группы.Название, dbo.Все_Группы.Код, dbo.Все_Группы.Код_Специальности, dbo.Все_Группы.Курс, " +
                       "dbo.Все_Группы.Код_Факультета, dbo.Все_Группы.УчебныйГод, dbo.Все_Группы.СрокОбучения, dbo.Все_Группы.Форма_Обучения, " +
                       "dbo.Все_Группы.КодПлана, dbo.Все_Группы.Учебный_План " +
                        "FROM dbo.Все_Студенты INNER JOIN " +
                       "dbo.Все_Группы ON dbo.Все_Студенты.Код_Группы = dbo.Все_Группы.Код " +
                        "GROUP BY dbo.Все_Группы.УчебныйГод, dbo.Все_Группы.Название, dbo.Все_Студенты.Статус, dbo.Все_Группы.Код, " +
                       "dbo.Все_Группы.Код_Специальности, dbo.Все_Группы.Курс, dbo.Все_Группы.Код_Факультета, dbo.Все_Группы.СрокОбучения, " +
                       "dbo.Все_Группы.Форма_Обучения, dbo.Все_Группы.КодПлана, dbo.Все_Группы.Учебный_План " +
                        "HAVING(dbo.Все_Группы.УчебныйГод = '" + currentYear + "') AND (dbo.Все_Студенты.Статус = 1) " +
                        "ORDER BY dbo.Все_Группы.Название";

            var lstGroupsDecanat = new List<GroupViewModel>();
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DecanatConnection"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            GroupViewModel group = new GroupViewModel();
                            group.Name = reader["Название"].ToString();
                            group.DecanatID = Convert.ToInt32(reader["Код"]);
                            group.ProfileId = Convert.ToInt32(reader["Код_Специальности"]);
                            group.Course = Convert.ToInt32(reader["Курс"]);
                            group.FacultyId = Convert.ToInt32(reader["Код_Факультета"]);
                            group.AcademicYear = reader["УчебныйГод"].ToString();
                            group.Period = (reader["СрокОбучения"] is DBNull) ? 0 : Convert.ToDouble(reader["СрокОбучения"]);
                            group.FormOfTrainingId = Convert.ToInt32(reader["Форма_Обучения"]);
                            group.idPlanDecanat = (reader["КодПлана"] is DBNull) ? 0 : Convert.ToInt32(reader["КодПлана"]);
                            group.PlanNameDecanat = reader["Учебный_План"].ToString();
                            lstGroupsDecanat.Add(group);
                        }
                    }
                }
            }
            return lstGroupsDecanat;
        }

        /// <summary>
        /// Получение списка групп на сайте
        /// </summary>        
        private async Task<List<GroupViewModel>> GetGroupsSiteAsync(string currentYear)
        {
            var lst = db.Groups
                    .Where(a => a.AcademicYear == currentYear && a.IsDeleted == false)
                    .Select(a => new GroupViewModel
                    {
                        Name = a.Name,
                        DecanatID = a.DecanatID,
                        ProfileId = a.ProfileId,
                        Course = a.Course,
                        FacultyId = a.FacultyId,
                        AcademicYear = a.AcademicYear,
                        FormOfTrainingId = a.FormOfTrainingId,
                        idPlanDecanat = a.idPlanDecanat,
                        PlanNameDecanat = a.PlanNameDecanat,
                        Period = a.Period
                    })
                    .OrderBy(a => a.Name);
            return await lst.ToListAsync();
        }

        /// <summary>
        /// Получение списка групп Moodle
        /// </summary>        
        private async Task<Dictionary<int, string>> GetGroupsMoodleAsync()
        {
            //c. получение списка групп из moodle
            var groupsMoodle = new Dictionary<int, string>();
            string pg_sql = "SELECT id, name, description FROM mdl_cohort WHERE COALESCE(description, '') NOT ILIKE '%Не учебная%' AND name NOT LIKE '---%' ORDER BY Name";
            using (var myConnection = new NpgsqlConnection(connectionString))
            {
                await myConnection.OpenAsync();
                using (var pg_command = new NpgsqlCommand(pg_sql, myConnection))
                {
                    using (var reader = await pg_command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            groupsMoodle.Add(Convert.ToInt32(reader["id"]), reader["name"].ToString());
                        }
                    }
                }
            }
            return groupsMoodle;
        }

        /// <summary>
        /// Удаление групп на сайте, которые отсутствуют в деканате
        /// </summary>
        /// <param name="lstGroupsSite">список групп на сайте</param>
        /// <param name="lstGroupsDecanat">список групп в деканате</param>
        /// <returns></returns>
        private async Task DeleteGroupsFromSiteAsync(List<GroupViewModel> lstGroupsSite, List<GroupViewModel> lstGroupsDecanat)
        {
            var idGroupsSite = lstGroupsSite.Select(a => a.DecanatID).ToList();
            var idGroupsDecanat = lstGroupsDecanat.Select(a => a.DecanatID).ToList();
            var result = idGroupsSite.Except(idGroupsDecanat).ToList();
            foreach (var s in result)
            {
                var faculty = db.Groups.FirstOrDefault(a => a.DecanatID == s && a.IsDeleted == false);
                faculty.IsDeleted = true;
                db.Entry(faculty).State = EntityState.Modified;
                lstGroupsSite.RemoveAll(a => a.DecanatID == s);
            }
            int i = await db.SaveChangesAsync();
        }

        /// <summary>
        /// Удаление групп в Moodle, которые отсутствуют в деканате
        /// </summary>
        /// <param name="lstGroupsMoodle">список групп в Moodle</param>
        /// <param name="lstGroupsDecanat">список групп в деканате</param>
        /// <returns></returns>
        private async Task DeleteGroupsFromMoodleAsync(Dictionary<int, string> lstGroupsMoodle, List<GroupViewModel> lstGroupsDecanat)
        {
            var nameGroupsMoodle = lstGroupsMoodle.Select(a => a.Value).ToList();
            var nameGroupsDecanat = lstGroupsDecanat.Select(a => a.Name).ToList();
            var result = nameGroupsMoodle.Except(nameGroupsDecanat).ToList();

            using (var myConnection = new NpgsqlConnection(connectionString))
            {
                await myConnection.OpenAsync();
                foreach (var s in result)
                {
                    string pg_sql = "UPDATE mdl_cohort SET name ='---" + s + "' WHERE name = '" + s + "'";

                    using (var pg_command = new NpgsqlCommand(pg_sql, myConnection))
                    {
                        await pg_command.ExecuteNonQueryAsync();
                    }
                }
            }
        }


        /// <summary>
        /// Обновление данных групп на сайте, только тех, которые изменились
        /// </summary>
        /// <param name="lstGroupsSite">список групп на сайте</param>
        /// <param name="lstGroupsDecanat">список групп в деканате</param>
        /// <returns></returns>
        private async Task UpdateGroupsOnSiteAsync(List<GroupViewModel> lstGroupsSite, List<GroupViewModel> lstGroupsDecanat)
        {
            IEnumerable<GroupViewModel> exceptSite = lstGroupsSite.Except(lstGroupsDecanat, new GroupComparer());
            foreach (var s in exceptSite)
            {
                //находим группу с изменившимися данными на сайтах
                var groupSite = db.Groups.FirstOrDefault(a => a.DecanatID == s.DecanatID);
                var groupDecanat = lstGroupsDecanat.FirstOrDefault(a => a.DecanatID == s.DecanatID);

                //обновляем данные группы                
                groupSite.Name = groupDecanat.Name;
                groupSite.DecanatID = groupDecanat.DecanatID;
                var prof = db.Profiles.FirstOrDefault(a => a.DecanatID == groupDecanat.ProfileId && a.IsDeleted == false);
                if (prof != null)
                    groupSite.ProfileId = prof.Id;
                groupSite.Course = groupDecanat.Course;
                var fac = db.Faculties.FirstOrDefault(a => a.DecanatID == groupDecanat.FacultyId && a.IsDeleted == false);
                if (fac != null)
                    groupSite.FacultyId = fac.Id;
                groupSite.AcademicYear = groupDecanat.AcademicYear;
                groupSite.FormOfTrainingId = groupDecanat.FormOfTrainingId;
                groupSite.idPlanDecanat = groupDecanat.idPlanDecanat;
                groupSite.PlanNameDecanat = groupDecanat.PlanNameDecanat;
                groupSite.Period = groupDecanat.Period;
                groupSite.IsDeleted = false;

                db.Entry(groupSite).State = EntityState.Modified;
            }
            //сохранение данных в БД сайта
            try
            {
                int i = await db.SaveChangesAsync();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var errors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in errors.ValidationErrors)
                    {
                        string errorMessage = validationError.ErrorMessage;
                    }
                }
            }
        }


        ///// <summary>
        ///// Обновление данных групп на сайте, только тех, которые изменились
        ///// </summary>
        ///// <param name="groupName">название группы в Moodle</param>        
        ///// <returns></returns>
        //private async Task<Dictionary<int, string>> GetGroupMoodleAsync(string groupName, bool like)
        //{
        //    //c. получение группы из moodle
        //    var groupsMoodle = new Dictionary<int, string>();
        //    string pg_sql = (like) ? $"SELECT id, name FROM mdl_cohort WHERE name NOT LIKE '%{groupName}%'" :
        //        $"SELECT id, name FROM mdl_cohort WHERE name = '{groupName}'";
        //    using (var myConnection = new NpgsqlConnection(connectionString))
        //    {
        //        await myConnection.OpenAsync();
        //        using (var pg_command = new NpgsqlCommand(pg_sql, myConnection))
        //        {
        //            using (var reader = await pg_command.ExecuteReaderAsync())
        //            {
        //                while (await reader.ReadAsync())
        //                {
        //                    groupsMoodle.Add(Convert.ToInt32(reader["id"]), reader["name"].ToString());
        //                }
        //            }
        //        }
        //    }
        //    return groupsMoodle;
        //}


        ///// <summary>
        ///// Обновление данных групп на сайте, только тех, которые изменились
        ///// </summary>
        ///// <param name="lstGroupsMoodle">список групп в Moodle</param>
        ///// <param name="lstGroupsDecanat">список групп в деканате</param>
        ///// <returns></returns>
        //private async Task UpdateGroupsOnMoodleAsync(List<GroupViewModel> lstGroupsMoodle, List<GroupViewModel> lstGroupsDecanat)
        //{
        //    IEnumerable<GroupViewModel> exceptMoodle = lstGroupsMoodle.Except(lstGroupsDecanat, new GroupComparer());
        //    foreach (var s in exceptMoodle)
        //    {
        //        //находим группу с изменившимися данными на сайтах
        //        var groupMoodle = lstGroupsMoodle.FirstOrDefault(a => a.DecanatID == s.DecanatID);
        //        var groupDecanat = lstGroupsDecanat.FirstOrDefault(a => a.DecanatID == s.DecanatID);

        //        //обновляем данные группы                
        //        groupMoodle.Name = groupDecanat.Name;
        //        groupMoodle.DecanatID = groupDecanat.DecanatID;
        //        var prof = db.Profiles.FirstOrDefault(a => a.DecanatID == groupDecanat.ProfileId && a.IsDeleted == false);
        //        if (prof != null)
        //            groupMoodle.ProfileId = prof.Id;
        //        groupMoodle.Course = groupDecanat.Course;
        //        var fac = db.Faculties.FirstOrDefault(a => a.DecanatID == groupDecanat.FacultyId && a.IsDeleted == false);
        //        if (fac != null)
        //            groupMoodle.FacultyId = fac.Id;
        //        groupMoodle.AcademicYear = groupDecanat.AcademicYear;
        //        groupMoodle.FormOfTrainingId = groupDecanat.FormOfTrainingId;
        //        groupMoodle.idPlanDecanat = groupDecanat.idPlanDecanat;
        //        groupMoodle.PlanNameDecanat = groupDecanat.PlanNameDecanat;
        //        groupMoodle.Period = groupDecanat.Period;

        //    }
        //    //сохранение данных в БД сайта
        //    try
        //    {
        //        int i = await db.SaveChangesAsync();
        //    }
        //    catch (DbEntityValidationException ex)
        //    {
        //        foreach (var errors in ex.EntityValidationErrors)
        //        {
        //            foreach (var validationError in errors.ValidationErrors)
        //            {
        //                string errorMessage = validationError.ErrorMessage;
        //            }
        //        }
        //    }
        //}


        /// <summary>
        /// Добавление недостающих групп на сайт
        /// </summary>
        /// <param name="lstGroupsSite">список групп на сайте</param>
        /// <param name="lstGroupsDecanat">список групп в деканате</param>
        /// <returns></returns>
        private async Task AddGroupsToSiteAsync(List<GroupViewModel> lstGroupsSite, List<GroupViewModel> lstGroupsDecanat)
        {
            var idGroupsSite = lstGroupsSite.Select(a => a.DecanatID).ToList();
            var idGroupsDecanat = lstGroupsDecanat.Select(a => a.DecanatID).ToList();
            var result = idGroupsDecanat.Except(idGroupsSite).ToList();

            foreach (var s in result)
            {
                //сначала поиск группы на сайте, среди удаленных
                var group = db.Groups.FirstOrDefault(a => a.IsDeleted == true && a.DecanatID == s);
                if (group != null)
                {
                    group.IsDeleted = false;
                    db.Entry(group).State = EntityState.Modified;
                }
                else
                {
                    //получаем добавляемую группу
                    var newGroup = lstGroupsDecanat.FirstOrDefault(a => a.DecanatID == s);
                    //добавляем в базу
                    if (newGroup != null)
                    {
                        db.Groups.Add(new Group
                        {
                            Name = newGroup.Name,
                            DecanatID = newGroup.DecanatID,
                            ProfileId = db.Profiles.FirstOrDefault(a => a.DecanatID == newGroup.ProfileId && a.IsDeleted == false).Id,
                            Course = newGroup.Course,
                            FacultyId = db.Faculties.FirstOrDefault(a => a.DecanatID == newGroup.FacultyId && a.IsDeleted == false).Id,
                            AcademicYear = newGroup.AcademicYear,
                            FormOfTrainingId = newGroup.FormOfTrainingId,
                            idPlanDecanat = newGroup.idPlanDecanat,
                            PlanNameDecanat = newGroup.PlanNameDecanat,
                            Period = newGroup.Period
                        });
                    }
                }
            }
            //сохранение группы
            try
            {
                int i = await db.SaveChangesAsync();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var errors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in errors.ValidationErrors)
                    {
                        string errorMessage = validationError.ErrorMessage;
                    }
                }
            }
        }

        /// <summary>
        /// Добавление недостающих групп в Moodle
        /// </summary>
        /// <returns></returns>
        private async Task AddGroupsToMoodleAsync(Dictionary<int, string> lstGroupsMoodle, List<GroupViewModel> lstGroupsDecanat)
        {
            var nameGroupsMoodle = lstGroupsMoodle.Select(a => a.Value).ToList();
            var nameGroupsDecanat = lstGroupsDecanat.Select(a => a.Name).ToList();
            var result = nameGroupsDecanat.Except(nameGroupsMoodle).ToList();

            using (var myConnection = new NpgsqlConnection(connectionString))
            {
                await myConnection.OpenAsync();
                foreach (var s in result)
                {
                    int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

                    string idCohortMoodle = "INSERT INTO mdl_cohort (contextid, name, descriptionformat, component, timecreated, timemodified)" +
                                    " VALUES ('1', '" + s + "', '1', '', " + unixTime + ", " + unixTime + ") RETURNING id";

                    using (var myCommand = new NpgsqlCommand(idCohortMoodle, myConnection))
                    {
                        object idNewGroup = myCommand.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Обновление ид групп Moodle в группах на сайте
        /// </summary>
        /// <returns></returns>
        private async Task UpdateGroupsSiteWithMoodleIdAsync(Dictionary<int, string> lstGroupsMoodle, List<GroupViewModel> lstGroupsSite, string currentYear)
        {
            //поиск дубликатов ИД на сайте            
            var groupCount = db.Groups
                .Where(a => a.IsDeleted == false && a.AcademicYear == currentYear)
                .GroupBy(p => p.MoodleID)
                        .Select(g => new { Name = g.Key, Count = g.Count() })
                        .Where(a => a.Count > 1 && a.Name != null).ToList();

            if (groupCount.Count > 0)
            {
                List<int?> lstMoodleGroupsId = groupCount.Select(a => a.Name).ToList();
                var gr = db.Groups.Where(a => lstMoodleGroupsId.Contains(a.MoodleID)).ToList();
                foreach (var g in gr)
                {
                    g.MoodleID = null;
                    db.Entry(g).State = EntityState.Modified;
                }
                await db.SaveChangesAsync();
            }

            //Обновление ИД в группах, где они отсутствуют
            var groups = db.Groups
                .Where(a => a.IsDeleted == false && a.AcademicYear == currentYear && a.MoodleID == null)
                .ToList();
            foreach (var g in groups)
            {
                string pg_sql = "SELECT id FROM mdl_cohort WHERE description NOT LIKE '%Не учебная%' AND name = '" + g.Name + "'";
                using (var myConnection = new NpgsqlConnection(""))
                {
                    await myConnection.OpenAsync();
                    using (var pg_command = new NpgsqlCommand(pg_sql, myConnection))
                    {
                        using (var reader = await pg_command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                g.MoodleID = Convert.ToInt32(reader["id"]);
                            }
                        }
                    }
                }
                db.Entry(g).State = EntityState.Modified;
            }
            await db.SaveChangesAsync();
        }


        /// <summary>
        /// Синхронизация групп
        /// </summary>
        /// <returns></returns>
        public async Task SyncGroupsAsync()
        {
            //получение текущего учебного года
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");

            //1. получение списка групп из базы сайта
            var lstGroupsSite = await GetGroupsSiteAsync(currentYear.Value);
            _pm("Получение списка групп с сайта");

            //2. получение списка групп из базы деканата
            var lstGroupsDecanat = await GetGroupsDecanatAsync(currentYear.Value);
            _pm("Получение списка групп из Деканата");

            //3. получение групп из Moodle
            var lstGroupsMoodle = await GetGroupsMoodleAsync();
            _pm("Получение списка групп из Moodle");

            //4. убираем удаленные группы с сайта с помощью сравнения по id
            await DeleteGroupsFromSiteAsync(lstGroupsSite, lstGroupsDecanat);
            _pm("Удаление неактивных групп с сайта");

            //5. убираем удаленные группы из Moodle
            await DeleteGroupsFromMoodleAsync(lstGroupsMoodle, lstGroupsDecanat);
            _pm("Удаление неактивных групп в Moodle");

            //6.1. получение списка групп из базы сайта
            lstGroupsSite = await GetGroupsSiteAsync(currentYear.Value);
            _pm("Получение списка групп с сайта");

            //6.2. находим группы (на сайте), данные которых изменились в деканате            
            await UpdateGroupsOnSiteAsync(lstGroupsSite, lstGroupsDecanat);
            _pm("Обновление данных актуальных групп на сайте");

            //7. добавляем группы, которые есть в деканате, но нет на сайте
            await AddGroupsToSiteAsync(lstGroupsSite, lstGroupsDecanat);
            _pm("Добавление новых групп на сайт");

            //8. добавляем группы, которые есть в деканате, но нет в Moodle
            await AddGroupsToMoodleAsync(lstGroupsMoodle, lstGroupsDecanat);
            _pm("Добавление новых групп в Moodle");

            //9. повторное получение групп из Moodle, так как список групп мог измениться
            lstGroupsMoodle = await GetGroupsMoodleAsync();
            _pm("Получение списка групп из Moodle");

            //10. повторное получение списка групп из базы сайта
            lstGroupsSite = await GetGroupsSiteAsync(currentYear.Value);
            _pm("Получение списка групп с сайта");

            //11. обновление идентификаторов групп Moodle в группах на сайте 
            await UpdateGroupsSiteWithMoodleIdAsync(lstGroupsMoodle, lstGroupsSite, currentYear.Value);
            _pm("Обновление ИД групп Moodle на сайте");

            _pm("--- Синхронизация групп завершена ---");
        }

        public delegate void ProgressMessage(string msg);
        ProgressMessage _pm;

        public void MessageHandler(ProgressMessage pm)
        {
            _pm = pm;
        }
    }
}
