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
    public  class SyncFaculty
    {
        private  ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// Получение списка факультетов из деканата
        /// </summary>   
        private  async Task<List<FacultyViewModel>> GetFacultiesDecanatAsync()
        {
            string sql = "SELECT dbo.Факультеты.* FROM dbo.Факультеты WHERE ПК = 1 ORDER BY Факультет ASC";

            var lstFacultiesDecanat = new List<FacultyViewModel>();
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
                            var faculty = new FacultyViewModel();
                            faculty.Name = reader["Факультет"].ToString();
                            faculty.AliasFaculty = reader["Сокращение"].ToString();
                            faculty.Boss = reader["Декан"].ToString();
                            faculty.DecanatID = Convert.ToInt32(reader["Код"]);

                            lstFacultiesDecanat.Add(faculty);
                        }
                    }
                }
            }
            return lstFacultiesDecanat;
        }

        /// <summary>
        /// Получение списка факультетов на сайте
        /// </summary>        
        private  async Task<List<FacultyViewModel>> GetFacultiesSiteAsync()
        {
            var lst = db.Faculties
                    .Where(a => a.IsDeleted == false)
                    .Select(a => new FacultyViewModel
                    {
                        Name = a.Name,
                        AliasFaculty = a.AliasFaculty,
                        Boss = a.Boss,
                        DecanatID = a.DecanatID
                    })
                    .OrderBy(a => a.Name);
            return await lst.ToListAsync();
        }

        /// <summary>
        /// Удаление факультетов на сайте, которые отсутствуют в деканате
        /// </summary>
        /// <param name="lstFacultiesSite">список факультетов на сайте</param>
        /// <param name="lstFacultiesDecanat">список факультетов в деканате</param>
        /// <returns></returns>
        private  async Task DeleteFacultiesFromSiteAsync(List<FacultyViewModel> lstFacultiesSite, List<FacultyViewModel> lstFacultiesDecanat)
        {
            var idFacultiesSite = lstFacultiesSite.Select(a => a.DecanatID).ToList();
            var idFacultiesDecanat = lstFacultiesDecanat.Select(a => a.DecanatID).ToList();
            var result = idFacultiesSite.Except(idFacultiesDecanat).ToList();
            foreach (var s in result)
            {
                var faculty = db.Faculties.FirstOrDefault(a => a.DecanatID == s && a.IsDeleted == false);
                faculty.IsDeleted = true;
                db.Entry(faculty).State = EntityState.Modified;
                //lstFacultiesSite.RemoveAll(a => a.DecanatID == s);
            }
            int i = await db.SaveChangesAsync();
        }

        /// <summary>
        /// Обновление данных факультетов на сайте, только тех, которые изменились
        /// </summary>
        /// <param name="lstFacultiesSite">список факультетов на сайте</param>
        /// <param name="lstFacultiesDecanat">список факультетов в деканате</param>
        /// <returns></returns>
        private  async Task UpdateFacultiesOnSiteAsync(List<FacultyViewModel> lstFacultiesSite, List<FacultyViewModel> lstFacultiesDecanat)
        {
            IEnumerable<FacultyViewModel> exceptSite = lstFacultiesSite.Except(lstFacultiesDecanat, new FacultyComparer());
            foreach (var s in exceptSite)
            {
                //находим факультет с изменившимися данными на сайтах
                var facultySite = db.Faculties.FirstOrDefault(a => a.DecanatID == s.DecanatID);
                var facultyDecanat = lstFacultiesDecanat.FirstOrDefault(a => a.DecanatID == s.DecanatID);

                //обновляем данные факультета                
                facultySite.Name = facultyDecanat.Name;
                facultySite.AliasFaculty = facultyDecanat.AliasFaculty;
                facultySite.Boss = facultyDecanat.Boss;
                facultySite.DecanatID = facultyDecanat.DecanatID;
                facultySite.IsDeleted = false;

                db.Entry(facultySite).State = EntityState.Modified;
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

        /// <summary>
        /// Добавление недостающих факультетов на сайт
        /// </summary>
        /// <param name="lstFacultiesSite">список факультетов на сайте</param>
        /// <param name="lstFacultiesDecanat">список факультетов в деканате</param>
        /// <returns></returns>
        private  async Task AddFacultiesToSiteAsync(List<FacultyViewModel> lstFacultiesSite, List<FacultyViewModel> lstFacultiesDecanat)
        {
            var idFacultiesSite = lstFacultiesSite.Select(a => a.DecanatID).ToList();
            var idFacultiesDecanat = lstFacultiesDecanat.Select(a => a.DecanatID).ToList();
            var result = idFacultiesDecanat.Except(idFacultiesSite).ToList();

            foreach (var s in result)
            {
                //сначала поиск факультета на сайте, среди удаленных
                var faculty = db.Faculties.FirstOrDefault(a => a.IsDeleted == true && a.DecanatID == s);
                if (faculty != null)
                {
                    faculty.IsDeleted = false;
                    db.Entry(faculty).State = EntityState.Modified;
                }
                else
                {
                    //получаем добавляемый факультет
                    var newFaculty = lstFacultiesDecanat.FirstOrDefault(a => a.DecanatID == s);
                    //добавляем в базу
                    if (newFaculty != null)
                    {
                        db.Faculties.Add(new Faculty
                        {
                            DecanatID = newFaculty.DecanatID,
                            Name = newFaculty.Name,
                            AliasFaculty = newFaculty.AliasFaculty,
                            Boss = newFaculty.Boss
                        });
                    }
                }
            }
            //сохранение факультета
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
        /// Синхронизация факультетов
        /// </summary>
        /// <returns></returns>
        public  async Task SyncFacultiesAsync()
        {
            //1. получение списка факультетов из базы сайта
            var lstFacultiesSite = await GetFacultiesSiteAsync();
            _pm("Получение списка факультетов с сайта");

            //2. получение списка факультетов из базы деканата
            var lstFacultiesDecanat = await GetFacultiesDecanatAsync();
            _pm("Получение списка факультетов из Деканата");

            //3. убираем удаленные факультеты с сайта с помощью сравнения по id
            await DeleteFacultiesFromSiteAsync(lstFacultiesSite, lstFacultiesDecanat);
            _pm("Удаление неактивных факультетов с сайта");

            //4.1. получение списка факультетов из базы сайта
            lstFacultiesSite = await GetFacultiesSiteAsync();
            _pm("Получение списка факультетов с сайта");

            //4.2. находим факультеты (на сайте), данные которых изменились в деканате            
            await UpdateFacultiesOnSiteAsync(lstFacultiesSite, lstFacultiesDecanat);
            _pm("Обновление данных актуальных факультетов на сайте");

            //5. добавляем факультеты, которые есть в деканате, но нет на сайте
            await AddFacultiesToSiteAsync(lstFacultiesSite, lstFacultiesDecanat);
            _pm("Добавление новых факультетов на сайт");

            _pm("--- Синхронизация факультетов завершена ---");
        }

        public delegate void ProgressMessage(string msg);
        ProgressMessage _pm;

        public void MessageHandler(ProgressMessage pm)
        {
            _pm = pm;
        }
    }
}
