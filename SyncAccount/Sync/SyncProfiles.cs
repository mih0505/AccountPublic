using Accounts.Models;
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
    public class SyncProfile
    {
        public ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// Получение списка программ из деканата
        /// </summary>   
        private async Task<List<ProfileViewModel>> GetProfilesDecanatAsync()
        {
            string sql = "SELECT dbo.Специальности.* FROM dbo.Специальности WHERE Прием = 1 ORDER BY Название_Спец ASC";

            var lstProfilesDecanat = new List<ProfileViewModel>();
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
                            var pro = new ProfileViewModel();
                            pro.Name = reader["Название_Спец"].ToString();
                            pro.ShortName = reader["Название"].ToString();
                            pro.Boss = "";
                            pro.DecanatID = Convert.ToInt32(reader["Код"]);
                            pro.FacultyId = Convert.ToInt32(reader["Код_Факультета"]);
                            pro.Code1 = reader["Специальность"].ToString();
                            pro.Period = Convert.ToDouble(reader["Срок_Обучения"]);
                            pro.DirectionOfTrainingId = 1;
                            pro.Qualification = reader["Квалификация"].ToString();
                            pro.DepartmentId = (reader["Код_Кафедры"] is DBNull) ? 0 : Convert.ToInt32(reader["Код_Кафедры"]);
                            pro.Acceptance = Convert.ToBoolean(reader["Прием"]);

                            lstProfilesDecanat.Add(pro);
                        }
                    }
                }
            }
            return lstProfilesDecanat;
        }

        /// <summary>
        /// Получение списка программ на сайте
        /// </summary>        
        private async Task<List<ProfileViewModel>> GetProfilesSiteAsync()
        {
            var lst = db.Profiles
                    .Where(a => a.IsDeleted == false)
                    .Select(a => new ProfileViewModel
                    {
                        Name = a.Name,
                        ShortName = a.ShortName,
                        Boss = a.Boss,
                        DecanatID = a.DecanatID,
                        FacultyId = a.FacultyId,
                        Code1 = a.Code1,
                        DirectionOfTrainingId = a.DirectionOfTrainingId,
                        Qualification = a.Qualification,
                        DepartmentId = a.DepartmentId,
                        Acceptance = a.Acceptance,
                        Period = a.Period
                    })
                    .OrderBy(a => a.Name);
            return await lst.ToListAsync();
        }

        /// <summary>
        /// Удаление программ на сайте, которые отсутствуют в деканате
        /// </summary>
        /// <param name="lstProfilesSite">список программ на сайте</param>
        /// <param name="lstProfilesDecanat">список программ в деканате</param>
        /// <returns></returns>
        private async Task DeleteProfilesFromSiteAsync(List<ProfileViewModel> lstProfilesSite, List<ProfileViewModel> lstProfilesDecanat)
        {
            var idProfilesSite = lstProfilesSite.Select(a => a.DecanatID).ToList();
            var idProfilesDecanat = lstProfilesDecanat.Select(a => a.DecanatID).ToList();
            var result = idProfilesSite.Except(idProfilesDecanat).ToList();
            foreach (var s in result)
            {
                var profile = db.Profiles.FirstOrDefault(a => a.DecanatID == s && a.IsDeleted == false);
                profile.IsDeleted = true;
                db.Entry(profile).State = EntityState.Modified;
            }

            if (result.Count > 0)
            {
                int i = await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Обновление данных программ на сайте, только тех, которые изменились
        /// </summary>
        /// <param name="lstProfilesSite">список программ на сайте</param>
        /// <param name="lstProfilesDecanat">список программ в деканате</param>
        /// <returns></returns>
        private async Task UpdateProfilesOnSiteAsync(List<ProfileViewModel> lstProfilesSite, List<ProfileViewModel> lstProfilesDecanat)
        {
            IEnumerable<ProfileViewModel> exceptSite = lstProfilesSite.Except(lstProfilesDecanat, new ProfileComparer());
            foreach (var s in exceptSite)
            {
                //находим программу с изменившимися данными на сайтах
                var profileSite = db.Profiles.FirstOrDefault(a => a.DecanatID == s.DecanatID);
                var profileDecanat = lstProfilesDecanat.FirstOrDefault(a => a.DecanatID == s.DecanatID);

                //обновляем данные программы                
                profileSite.Name = profileDecanat.Name;
                profileSite.ShortName = profileDecanat.ShortName;
                profileSite.Boss = profileDecanat.Boss;
                profileSite.DecanatID = profileDecanat.DecanatID;
                var f = db.Faculties.FirstOrDefault(a => a.DecanatID == profileDecanat.FacultyId && a.IsDeleted == false);
                if (f != null)
                    profileSite.FacultyId = f.Id;
                profileSite.Code1 = profileDecanat.Code1;
                profileSite.DirectionOfTrainingId = profileDecanat.DirectionOfTrainingId;
                profileSite.Qualification = profileDecanat.Qualification;
                var d = db.Departments.FirstOrDefault(a => a.DecanatID == profileDecanat.DepartmentId && a.IsDeleted == false);
                if (d != null)
                    profileSite.DepartmentId = d.Id;
                profileSite.Acceptance = profileDecanat.Acceptance;
                profileSite.Period = profileDecanat.Period;
                profileSite.IsDeleted = false;

                db.Entry(profileSite).State = EntityState.Modified;
            }
            //сохранение данных в БД сайта
            if (exceptSite.Count() > 0)
            {
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
        }

        /// <summary>
        /// Добавление недостающих программ на сайт
        /// </summary>
        /// <param name="lstProfilesSite">список программ на сайте</param>
        /// <param name="lstProfilesDecanat">список программ в деканате</param>
        /// <returns></returns>
        private async Task AddProfilesToSiteAsync(List<ProfileViewModel> lstProfilesSite, List<ProfileViewModel> lstProfilesDecanat)
        {
            var idProfilesSite = lstProfilesSite.Select(a => a.DecanatID).ToList();
            var idProfilesDecanat = lstProfilesDecanat.Select(a => a.DecanatID).ToList();
            var result = idProfilesDecanat.Except(idProfilesSite).ToList();

            foreach (var s in result)
            {
                //сначала поиск программы на сайте, среди удаленных
                var Profile = db.Profiles.FirstOrDefault(a => a.IsDeleted == true && a.DecanatID == s);
                if (Profile != null)
                {
                    Profile.IsDeleted = false;
                    db.Entry(Profile).State = EntityState.Modified;
                }
                else
                {
                    //получаем добавляемую программу
                    var newProfile = lstProfilesDecanat.FirstOrDefault(a => a.DecanatID == s);
                    //добавляем в базу
                    if (newProfile != null)
                    {
                        var f = db.Faculties.FirstOrDefault(a => a.DecanatID == newProfile.FacultyId && a.IsDeleted == false);
                        var d = db.Departments.FirstOrDefault(a => a.DecanatID == newProfile.DepartmentId && a.IsDeleted == false);
                        if (f != null && d != null)
                        {
                            db.Profiles.Add(new Profile
                            {
                                DecanatID = newProfile.DecanatID,
                                Name = newProfile.Name,
                                ShortName = newProfile.ShortName,
                                Boss = newProfile.Boss,

                                FacultyId = f.Id,
                                Code1 = newProfile.Code1,
                                DirectionOfTrainingId = newProfile.DirectionOfTrainingId,
                                Qualification = newProfile.Qualification,
                                DepartmentId = d.Id,
                                Acceptance = newProfile.Acceptance,
                                Period = newProfile.Period
                            });
                        }
                        else
                            _pm($"Не найден факультет или кафедра для специальности {newProfile.ShortName} - {newProfile.Name}");
                    }
                }
            }
            //сохранение программы
            if (result.Count > 0)
            {
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
        }

        /// <summary>
        /// Удаление дубликатов программ на сайте 
        /// </summary>
        /// <returns></returns>
        public async Task RemoveDublicateAsync(List<ProfileViewModel> lstProfilesSite)
        {
            //получаем список ИД, которые дублируются на сайте
            var dProfiles = lstProfilesSite.GroupBy(p => p.DecanatID)
                  .Select(g => new { Name = g.Key, Count = g.Count() }).Where(a => a.Count > 1).ToList();
            foreach (var p in dProfiles)
            {
                //получаем список дублирующихся учебных программ
                var currentProfile = db.Profiles.Where(a => a.DecanatID == p.Name && a.IsDeleted == false).ToList();

                //переписываем ИД программы у всех групп закрепленных за этой программой
                int mainProfileId = 0;
                for (var i = 0; i < currentProfile.Count; i++)
                {
                    if (i == 0)
                    {
                        mainProfileId = currentProfile[i].Id;
                    }
                    else
                    {
                        using (var transaction = db.Database.BeginTransaction())
                        {
                            try
                            {
                                //обновление ид программ в группах 
                                db.Database.ExecuteSqlCommand(
                                    $"UPDATE Groups SET ProfileId = {mainProfileId} " +
                                    $"WHERE ProfileId = {currentProfile[i].Id}");
                                //обновление ид групп в ведомостях
                                db.Database.ExecuteSqlCommand(
                                    $"UPDATE Statements SET ProfileId = {mainProfileId} " +
                                    $"WHERE GroupIdDecanate = {currentProfile[i].Id}");
                                await db.SaveChangesAsync();
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                            }
                        }
                        //удаляем дубликат программы с сайта
                        currentProfile[i].IsDeleted = true;
                        db.Entry(currentProfile[i]).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                }
                
            }
        }

        /// <summary>
        /// Исправление привязки групп к профилям в ведомостях 
        /// </summary>
        /// <returns></returns>
        public async Task FixStatementsProfilesAsync()
        {
            //получаем список актуальных групп с профилями    
            var currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");
            var groups = db.Groups.Where(a => !a.IsDeleted && a.AcademicYear == currentYear.Value).Include(a=>a.Profile).ToList();
            int fixStatements = 0;
            //поиск ведомостей, в которых связка группа-профиль отличаются от верных
            foreach (var gr in groups)
            {
                var incorrectsStatements = db.Statements.Where(a => a.GroupId == gr.Id && a.ProfileId != gr.Profile.Id).ToList();

                if (incorrectsStatements.Count != 0)
                {
                    fixStatements += incorrectsStatements.Count;
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            //обновление ид программ в группах 
                            db.Database.ExecuteSqlCommand(
                                $"UPDATE Statements SET ProfileId = {gr.Profile.Id} " +
                                $"WHERE GroupId = {gr.Id} AND ProfileId <> {gr.Profile.Id}");
                            
                            await db.SaveChangesAsync();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            _pm($"Ошибка: {ex.Message}");
                            transaction.Rollback();
                        }
                    }                    
                }
            }
            _pm($"Испарвленно профилей в ведомостях - {fixStatements}");
        }


        /// <summary>
        /// Синхронизация программ
        /// </summary>
        /// <returns></returns>
        public async Task SyncProfilesAsync()
        {
            await FixStatementsProfilesAsync();


            //1. получение списка программ из базы сайта
            var lstProfilesSite = await GetProfilesSiteAsync();
            _pm("Получение списка учебных программ из базы сайта");

            //2. получение списка программ из базы деканата
            var lstProfilesDecanat = await GetProfilesDecanatAsync();
            _pm("Получение списка учебных программ из Деканата");

            //3. убираем удаленные программы с сайта с помощью сравнения по id
            await DeleteProfilesFromSiteAsync(lstProfilesSite, lstProfilesDecanat);
            _pm("Удаление неактивных учебных программ с сайта");

            //4.1. повторное получение списка программ из базы сайта
            lstProfilesSite = await GetProfilesSiteAsync();
            _pm("Получение списка учебных программ из базы сайта");

            //4.2. находим программы (на сайте), данные которых изменились в деканате            
            await UpdateProfilesOnSiteAsync(lstProfilesSite, lstProfilesDecanat);
            _pm("Обновление учебных программ на сайте");

            //5. добавляем программы, которые есть в деканате, но нет на сайте
            await AddProfilesToSiteAsync(lstProfilesSite, lstProfilesDecanat);
            _pm("Добавление новых учебных программ на сайт");

            //6. Поиск и удаление дубликатов
            lstProfilesSite = await GetProfilesSiteAsync();
            _pm("Получение списка учебных программ из базы сайта");
            
            await RemoveDublicateAsync(lstProfilesSite);
            _pm("Дубликаты учебных программ удалены");


            _pm("--- Синхронизация учебных программ завершена ---");
        }

        public delegate void ProgressMessage(string msg);
        ProgressMessage _pm;

        public void MessageHandler(ProgressMessage pm)
        {
            _pm = pm;
        }
    }
}
