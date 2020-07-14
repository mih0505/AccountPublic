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
    public  class SyncDepartment
    {
        private  ApplicationDbContext db = new ApplicationDbContext();
        /// <summary>
        /// Получение списка кафедр из деканата
        /// </summary>   
        private  async Task<List<DepartmentViewModel>> GetDepartmentsDecanatAsync()
        {
            string sql = "SELECT dbo.Кафедры.* FROM dbo.Кафедры WHERE Код_Факультета IS NOT NULL ORDER BY Название ASC";

            var lstDepartmentsDecanat = new List<DepartmentViewModel>();
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
                            var dep = new DepartmentViewModel();
                            dep.Name = reader["Название"].ToString();
                            dep.ShortName = reader["Сокращение"].ToString();
                            dep.Boss = reader["ЗавКафедрой"].ToString();
                            dep.DecanatID = Convert.ToInt32(reader["Код"]);
                            dep.Number = Convert.ToInt32(reader["Номер"]);
                            dep.FacultyId = (reader["Код_Факультета"] is DBNull) ? 0 : Convert.ToInt32(reader["Код_Факультета"]);

                            lstDepartmentsDecanat.Add(dep);
                        }
                    }
                }
            }
            return lstDepartmentsDecanat;
        }

        /// <summary>
        /// Получение списка кафедр на сайте
        /// </summary>        
        private  async Task<List<DepartmentViewModel>> GetDepartmentsSiteAsync()
        {
            var lst = db.Departments
                    .Where(a => a.IsDeleted == false && a.FacultyId != null)
                    .Select(a => new DepartmentViewModel
                    {
                        Name = a.Name,
                        ShortName = a.ShortName,
                        Boss = a.Boss,
                        DecanatID = a.DecanatID,
                        FacultyId = a.FacultyId,
                        Number = a.Number
                    })
                    .OrderBy(a => a.Name);
            return await lst.ToListAsync();
        }

        /// <summary>
        /// Удаление кафедр на сайте, которые отсутствуют в деканате
        /// </summary>
        /// <param name="lstDepartmentsSite">список кафедр на сайте</param>
        /// <param name="lstDepartmentsDecanat">список кафедр в деканате</param>
        /// <returns></returns>
        private  async Task DeleteDepartmentsFromSiteAsync(List<DepartmentViewModel> lstDepartmentsSite, List<DepartmentViewModel> lstDepartmentsDecanat)
        {
            var idDepartmentsSite = lstDepartmentsSite.Select(a => a.DecanatID).ToList();
            var idDepartmentsDecanat = lstDepartmentsDecanat.Select(a => a.DecanatID).ToList();
            var result = idDepartmentsSite.Except(idDepartmentsDecanat).ToList();
            foreach (var s in result)
            {
                var faculty = db.Departments.FirstOrDefault(a => a.DecanatID == s && a.IsDeleted == false);
                faculty.IsDeleted = true;
                db.Entry(faculty).State = EntityState.Modified;
                lstDepartmentsSite.RemoveAll(a => a.DecanatID == s);
            }
            int i = await db.SaveChangesAsync();
        }

        /// <summary>
        /// Обновление данных кафедр на сайте, только тех, которые изменились
        /// </summary>
        /// <param name="lstDepartmentsSite">список кафедр на сайте</param>
        /// <param name="lstDepartmentsDecanat">список кафедр в деканате</param>
        /// <returns></returns>
        private  async Task UpdateDepartmentsOnSiteAsync(List<DepartmentViewModel> lstDepartmentsSite, List<DepartmentViewModel> lstDepartmentsDecanat)
        {
            IEnumerable<DepartmentViewModel> exceptSite = lstDepartmentsSite.Except(lstDepartmentsDecanat, new DepartmentComparer());
            foreach (var s in exceptSite)
            {
                //находим кафедру с изменившимися данными на сайтах
                var departmentSite = db.Departments.FirstOrDefault(a => a.DecanatID == s.DecanatID);
                var departmentDecanat = lstDepartmentsDecanat.FirstOrDefault(a => a.DecanatID == s.DecanatID);

                //обновляем данные кафедры                
                departmentSite.Name = departmentDecanat.Name;
                departmentSite.ShortName = departmentDecanat.ShortName;
                departmentSite.Boss = departmentDecanat.Boss;
                departmentSite.DecanatID = departmentDecanat.DecanatID;
                var f = db.Faculties.FirstOrDefault(a => a.DecanatID == departmentDecanat.FacultyId && a.IsDeleted == false);
                if (f != null)
                { 
                    departmentSite.FacultyId = f.Id; 
                }
                departmentSite.Number = departmentDecanat.Number;
                departmentSite.IsDeleted = false;

                db.Entry(departmentSite).State = EntityState.Modified;
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
        /// Добавление недостающих кафедр на сайт
        /// </summary>
        /// <param name="lstDepartmentsSite">список кафедр на сайте</param>
        /// <param name="lstDepartmentsDecanat">список кафедр в деканате</param>
        /// <returns></returns>
        private  async Task AddDepartmentsToSiteAsync(List<DepartmentViewModel> lstDepartmentsSite, List<DepartmentViewModel> lstDepartmentsDecanat)
        {
            var idDepartmentsSite = lstDepartmentsSite.Select(a => a.DecanatID).ToList();
            var idDepartmentsDecanat = lstDepartmentsDecanat.Select(a => a.DecanatID).ToList();
            var result = idDepartmentsDecanat.Except(idDepartmentsSite).ToList();

            foreach (var s in result)
            {
                //сначала поиск кафедры на сайте, среди удаленных
                var Department = db.Departments.FirstOrDefault(a => a.IsDeleted == true && a.DecanatID == s);
                if (Department != null)
                {
                    Department.IsDeleted = false;
                    db.Entry(Department).State = EntityState.Modified;
                }
                else
                {
                    //получаем добавляемую кафедру
                    var newDepartment = lstDepartmentsDecanat.FirstOrDefault(a => a.DecanatID == s);
                    //добавляем в базу
                    if (newDepartment != null)
                    {
                        db.Departments.Add(new Department
                        {
                            DecanatID = newDepartment.DecanatID,
                            Name = newDepartment.Name,
                            ShortName = newDepartment.ShortName,
                            Boss = newDepartment.Boss,
                            FacultyId = db.Faculties.FirstOrDefault(a => a.DecanatID == newDepartment.FacultyId && a.IsDeleted == false).Id,
                            Number = newDepartment.Number
                        });
                    }
                }
            }
            //сохранение кафедры
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
        /// Синхронизация кафедр
        /// </summary>
        /// <returns></returns>
        public  async Task SyncDepartmentsAsync()
        {
            //1. получение списка кафедр из базы сайта
            var lstDepartmentsSite = await GetDepartmentsSiteAsync();
            _pm("Получение списка кафедр с сайта");

            //2. получение списка кафедр из базы деканата
            var lstDepartmentsDecanat = await GetDepartmentsDecanatAsync();
            _pm("Получение списка кафедр из Деканата");

            //3. убираем удаленные кафедры с сайта с помощью сравнения по id
            await DeleteDepartmentsFromSiteAsync(lstDepartmentsSite, lstDepartmentsDecanat);
            _pm("Удаление неактивных кафедр с сайта");

            //4. находим кафедры (на сайте), данные которых изменились в деканате            
            await UpdateDepartmentsOnSiteAsync(lstDepartmentsSite, lstDepartmentsDecanat);
            _pm("Обновление данных актуальных кафедр на сайте");

            //5. добавляем кафедры, которые есть в деканате, но нет на сайте
            await AddDepartmentsToSiteAsync(lstDepartmentsSite, lstDepartmentsDecanat);
            _pm("Добавление новых кафедр на сайт");

            _pm("--- Синхронизация кафедр завершена ---");
        }

        public delegate void ProgressMessage(string msg);
        ProgressMessage _pm;

        public void MessageHandler(ProgressMessage pm)
        {
            _pm = pm;
        }
    }
}
