using Accounts.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Npgsql;
using SyncAccount.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncAccount.Sync
{
    public class SyncStudent
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
        private readonly string connectionString = "Server=*;Port=5432;User Id=*;Password=*;Database=moodledb;";

        //создание хэша пароля
        string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string.
            string hash = sBuilder.ToString();
            return hash;
        }

        void FileCSV(string str)
        {
            string path = @"D:\\WebSites\\Account\\users.csv";
            FileInfo fileInf = new FileInfo(path);

            using (var sw = new StreamWriter("D:\\WebSites\\Account\\users.csv", true))
            {
                if (fileInf.Length == 0)
                {
                    sw.WriteLineAsync("Username;Password;Firstname;Middlename;Lastname;Email;City;Country;Cohort1;Deleted");
                    sw.WriteLineAsync(str);
                }
                else
                {
                    sw.WriteLineAsync(str);
                }
            }
        }

        /// <summary>
        /// Получение списка студентов из деканата
        /// </summary>   
        private async Task<List<UserViewModel>> GetStudentsDecanatAsync(string currentYear)
        {
            var lstStudentsDecanat = new List<UserViewModel>();
            string sql = "SELECT dbo.Специальности.Код AS КодСпециальности, dbo.Специальности.Название_Спец, dbo.Все_Группы.Код AS КодГруппы, dbo.УсловияОбучения.Сокращение, " +
                      "dbo.Все_Группы.Название AS Группа, dbo.Все_Группы.Курс, dbo.Факультеты.Факультет, dbo.Все_Студенты.*, dbo.Все_Группы.Код_Факультета " +
                      "FROM dbo.Специальности INNER JOIN " +
                      "dbo.Все_Группы ON dbo.Специальности.Код = dbo.Все_Группы.Код_Специальности INNER JOIN " +
                      "dbo.Факультеты ON dbo.Все_Группы.Код_Факультета = dbo.Факультеты.Код INNER JOIN " +
                      "dbo.Все_Студенты ON dbo.Все_Группы.Код = dbo.Все_Студенты.Код_Группы INNER JOIN " +
                      "dbo.УсловияОбучения ON dbo.Все_Студенты.УслОбучения = dbo.УсловияОбучения.Код " +
                      "WHERE dbo.Все_Студенты.Статус = 1 AND dbo.Все_Группы.УчебныйГод = '" + currentYear + "' " +
                      "ORDER BY dbo.Все_Студенты.Код ASC";

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
                            var student = new UserViewModel();
                            student.DecanatId = Convert.ToInt32(reader["Код"]);
                            student.Lastname = reader["Фамилия"].ToString();
                            student.Firstname = reader["Имя"].ToString();
                            student.Middlename = reader["Отчество"].ToString();
                            student.BirthDate = (reader["Дата_Рождения"] is DBNull) ? DateTime.Now : Convert.ToDateTime(reader["Дата_Рождения"]);
                            
                            student.Bases = reader["Сокращение"].ToString();
                            student.idGroupDecanat = Convert.ToInt32(reader["Код_Группы"]);
                            student.idFacultyDecanat = Convert.ToInt32(reader["Код_Факультета"]);
                            student.UserName = student.DecanatId.ToString();
                            student.NumberOfRecordBook = reader["Номер_Зачетной_Книжки"].ToString();
                            student.Sex = (reader["Пол"].ToString() == "Муж") ? true : false;
                            student.GroupName = reader["Группа"].ToString();

                            lstStudentsDecanat.Add(student);
                        }
                    }
                }
            }
            return lstStudentsDecanat;
        }

        /// <summary>
        /// Получение списка студентов из moodle
        /// </summary> 
        private async Task<List<UserViewModel>> GetStudentsMoodleAsync()
        {
            var groups = db.Groups.Where(a => a.IsDeleted != true).ToList();
            string pg_sql = "SELECT c.id, username, lastname, firstname, middlename, deleted, password, c.email, a.name AS group " +
                            "FROM mdl_cohort a, mdl_cohort_members b, mdl_user c " +
                            "WHERE b.cohortid = a.id AND b.userid = c.id AND c.deleted = 0 AND (";
            foreach (var g in groups)
            {
                pg_sql += "a.name = '" + g.Name + "' OR ";
            }
            pg_sql = pg_sql.Substring(0, pg_sql.Length - 4) + ") ORDER BY lastname, firstname, middlename ASC";

            var lstStudentsMoodle = new List<UserViewModel>();
            using (var myConnection = new NpgsqlConnection(connectionString))
            {
                await myConnection.OpenAsync();
                using (var pg_command = new NpgsqlCommand(pg_sql, myConnection))
                {
                    using (var reader = await pg_command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var student = new UserViewModel();
                            student.DecanatId = Convert.ToInt32(reader["username"]);
                            student.Lastname = reader["lastname"].ToString();
                            student.Firstname = reader["firstname"].ToString();
                            student.Middlename = reader["middlename"].ToString();
                            student.idGroupDecanat = null;
                            student.idFacultyDecanat = null;
                            student.UserName = reader["id"].ToString();//записываем сюда id студента в moodle, так проще обновить его данные
                            student.GroupName = reader["group"].ToString();//записываем сюда название группы, так проще сравнивать, где студент учится
                            student.Sex = true;
                            student.Email = reader["email"].ToString().ToLower();
                            lstStudentsMoodle.Add(student);
                        }
                    }
                }
            }
            return lstStudentsMoodle;
        }

        /// <summary>
        /// Получение списка студентов из сайта
        /// </summary> 
        private async Task<List<UserViewModel>> GetStudentsSiteAsync()
        {

            return db.Users.Include(a => a.Group).Include(a => a.Faculty)
                .Where(a => a.DecanatId != null && a.DateBlocked == null)
                .OrderBy(a => a.DecanatId)
                .Select(a => new UserViewModel
                {
                    DecanatId = a.DecanatId,
                    Lastname = a.Lastname,
                    Firstname = a.Firstname,
                    Middlename = a.Middlename,
                    idGroupDecanat = a.Group.DecanatID,
                    idFacultyDecanat = a.Faculty.DecanatID,
                    UserName = a.UserName,
                    NumberOfRecordBook = a.NumberOfRecordBook,
                    Sex = a.Sex,
                    Bases = a.Bases,
                    DateBlocked = a.DateBlocked,
                    Email = a.Email,
                    GroupName = a.Group.Name,
                    BirthDate = a.BirthDay
                }).ToList();
        }

        /// <summary>
        /// Удаление студентов на сайте, которые отсутствуют в деканате
        /// </summary>
        /// <param name="lstStudentsSite">список студентов на сайте</param>
        /// <param name="lstStudentsDecanat">список студентов в деканате</param>
        /// <returns></returns>
        private async Task DeleteStudentsFromSiteAsync(List<UserViewModel> lstStudentsSite, List<UserViewModel> lstStudentsDecanat)
        {
            var idStudentsSite = lstStudentsSite.Select(a => a.DecanatId).ToList();
            var idStudentsDecanat = lstStudentsDecanat.Select(a => a.DecanatId).ToList();
            var result = idStudentsSite.Except(idStudentsDecanat).ToList();
            //блокируем отчисленных студентов на сайте
            foreach (var s in result)
            {
                var stud = db.Users.FirstOrDefault(a => a.DecanatId == s && a.DateBlocked == null);
                stud.DateBlocked = DateTime.Now;
                stud.Email = stud.DateBlocked.Value.ToShortDateString() + "___" + stud.Email;
                db.Entry(stud).State = EntityState.Modified;
                lstStudentsSite.RemoveAll(a => a.DecanatId == s);
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
        /// Удаление студентов, которые отсутствуют в деканате, из Moodle.
        /// Так как нет соответствующего api, используем csv-файл для групповых 
        /// операций над пользователями
        /// </summary>
        /// <param name="lstStudentsMoodle">список студентов в Moodle</param>
        /// <param name="lstStudentsDecanat">список студентов в деканате</param>
        /// <returns></returns>
        private async Task DeleteStudentsFromMoodleAsync(List<UserViewModel> lstStudentsMoodle, List<UserViewModel> lstStudentsDecanat)
        {
            var idStudentsMoodle = lstStudentsMoodle.Select(a => a.DecanatId).ToList();
            var idStudentsDecanat = lstStudentsDecanat.Select(a => a.DecanatId).ToList();
            var resultMoodle = idStudentsMoodle.Except(idStudentsDecanat).ToList();

            //удаляем отчисленных студентов в moodle
            foreach (var stud in resultMoodle)
            {
                FileCSV(stud.ToString() + ";;;;;;;;;1");
            }
        }

        /// <summary>
        /// Обновление данных студентов на сайте, только тех, которые изменились
        /// </summary>
        /// <param name="lstStudentsSite">список групп на сайте</param>
        /// <param name="lstStudentsDecanat">список групп в деканате</param>
        /// <returns></returns>
        private async Task UpdateStudentsOnSiteAsync(List<UserViewModel> lstStudentsSite, List<UserViewModel> lstStudentsDecanat)
        {
            IEnumerable<UserViewModel> exceptSite = lstStudentsSite.Except(lstStudentsDecanat, new StudentComparer());
            foreach (var s in exceptSite)
            {
                //находим студента с изменившимися данными на сайтах
                var studSite = db.Users.FirstOrDefault(a => a.DecanatId == s.DecanatId && a.DateBlocked == null);
                var studDecanat = lstStudentsDecanat.FirstOrDefault(a => a.DecanatId == s.DecanatId);

                //обновляем ФИО если они не совпадают
                if (studSite.Lastname != studDecanat.Lastname || studSite.Firstname != studDecanat.Firstname || studSite.Middlename != studDecanat.Middlename)
                {
                    studSite.Lastname = studDecanat.Lastname;
                    studSite.Firstname = studDecanat.Firstname;
                    studSite.Middlename = studDecanat.Middlename;
                }

                //обновляем группу, если она не совпадает
                if (studSite.idGroupDecanat != studDecanat.idGroupDecanat)
                {
                    studSite.idGroupDecanat = studDecanat.idGroupDecanat;
                    var groupStudent = db.Groups.FirstOrDefault(a => a.DecanatID == studDecanat.idGroupDecanat && a.IsDeleted == false);
                    studSite.GroupId = groupStudent.Id;
                }

                //обновляем остальные данные
                studSite.idFacultyDecanat = studDecanat.idFacultyDecanat;
                var facultyStudent = db.Faculties.FirstOrDefault(a => a.DecanatID == studDecanat.idFacultyDecanat && a.IsDeleted == false);
                studSite.FacultyId = facultyStudent.Id;
                studSite.NumberOfRecordBook = studDecanat.NumberOfRecordBook;
                studSite.Sex = studDecanat.Sex;
                studSite.Bases = studDecanat.Bases;
                studSite.UserName = studDecanat.UserName;
                studSite.BirthDay = studDecanat.BirthDate;

                db.Entry(studSite).State = EntityState.Modified;
            }
            //сохранение данных в БД сайта
            try
            {
                var i = db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var errors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in errors.ValidationErrors)
                    {
                        // get the error message 
                        string errorMessage = validationError.ErrorMessage;
                    }
                }
            }
        }

        /// <summary>
        /// Обновление данных студентов в Moodle
        /// </summary>
        /// <param name="lstStudentsSite">список студентов на сайте</param>
        /// <param name="lstStudentsSite">список студентов в Moodle</param>
        /// <returns></returns>
        private async Task UpdateStudentsToMoodleAsync(List<UserViewModel> lstStudentsMoodle, List<UserViewModel> lstStudentsSite, string currentYear)
        {
            //ведем подсчет количества отчислений и зачислений в группы, чтобы не открывалось много вкладок в браузере сразу
            int i = 0;

            IEnumerable<UserViewModel> exceptMoodle = lstStudentsMoodle.Except(lstStudentsSite, new StudentComparerMoodle());            
            foreach (var s in exceptMoodle)
            {
                var studSite = lstStudentsSite.FirstOrDefault(a => a.DecanatId == s.DecanatId);
                var studMoodle = lstStudentsMoodle.FirstOrDefault(a => a.DecanatId == s.DecanatId);

                if (studSite != null && studMoodle != null)
                {
                    if (studMoodle.Lastname != studSite.Lastname || studMoodle.Firstname != studSite.Firstname
                      || studMoodle.Middlename != studSite.Middlename || studMoodle.Email != studSite.Email)
                    {
                        studMoodle.Lastname = studSite.Lastname;
                        studMoodle.Firstname = studSite.Firstname;
                        studMoodle.Middlename = studSite.Middlename;
                        studMoodle.Email = studSite.Email;

                        //обновление ФИО в moodle
                        string CommText = "UPDATE mdl_user SET email='" + studMoodle.Email +
                            "', middlename='" + studSite.Middlename + "', firstname='" + studSite.Firstname +
                            "', lastname='" + studSite.Lastname + "' WHERE username = '" + studSite.DecanatId + "'";
                        using (var myConnection = new NpgsqlConnection(connectionString))
                        {
                            await myConnection.OpenAsync();
                            using (var pg_command = new NpgsqlCommand(CommText, myConnection))
                            {
                                object c = pg_command.ExecuteNonQuery();
                            }
                        }
                    }

                    //обновляем группу, если она не совпадает
                    if (studMoodle.GroupName != studSite.GroupName)
                    {
                        //поиск ИД новой группы студента в Moodle
                        int idMoodleGroup = 0;
                        string pg_sql = $"SELECT id, name FROM mdl_cohort WHERE Name = '{studSite.GroupName}'";
                        using (var myConnection = new NpgsqlConnection(connectionString))
                        {
                            await myConnection.OpenAsync();
                            using (var pg_command = new NpgsqlCommand(pg_sql, myConnection))
                            {
                                using (var reader = await pg_command.ExecuteReaderAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        idMoodleGroup = Convert.ToInt32(reader["id"]);
                                    }
                                }
                            }
                        }

                        //проверка найден ли ИД новой группы
                        if (idMoodleGroup == 0)
                        {
                            //создаем недостающую группу в Moodle, если она не была найдена, и получаем ее ИД
                            using (var myConnection = new NpgsqlConnection(connectionString))
                            {
                                await myConnection.OpenAsync();
                                int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

                                string idCohortMoodle = "INSERT INTO mdl_cohort (contextid, name, descriptionformat, component, timecreated, timemodified)" +
                                                " VALUES ('1', '" + s + "', '1', '', " + unixTime + ", " + unixTime + ") RETURNING id";

                                using (var myCommand = new NpgsqlCommand(idCohortMoodle, myConnection))
                                {
                                    object idNewGroup = myCommand.ExecuteScalarAsync();
                                    idMoodleGroup = Convert.ToInt32(idNewGroup);
                                }
                            }
                        }


                        //Получаем id глоб. групп, в которых записан студент Moodle, удаляем его от туда и записываем в правильную группу
                        pg_sql = $"SELECT cohortid FROM mdl_cohort_members WHERE userid = {studMoodle.UserName}";
                        using (var myConnection = new NpgsqlConnection(connectionString))
                        {
                            await myConnection.OpenAsync();
                            using (var pg_command = new NpgsqlCommand(pg_sql, myConnection))
                            {
                                using (var reader = await pg_command.ExecuteReaderAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        System.Diagnostics.Process.Start("http://sdo.strbsu.ru/cohort/removeusercohort.php?cohortid="
                                            + Convert.ToInt32(reader["cohortid"]) + "&userid=" + studMoodle.UserName);
                                        i++;
                                    }
                                }
                            }
                        }
                        //запись в правильную группу
                        System.Diagnostics.Process.Start("http://sdo.strbsu.ru/cohort/addusercohort.php?cohortid=" + idMoodleGroup + "&userid=" + studMoodle.UserName);
                        i++;
                    }
                }

                if (i >= 10)
                {
                    i = 0;
                    Thread.Sleep(90000);
                }
            }
        }

        /// <summary>
        /// Добавление недостающих студентов на сайт
        /// </summary>
        /// <param name="lstStudentsSite">список студентов на сайте</param>
        /// <param name="lstStudentsDecanat">список студентов в деканате</param>
        /// <returns></returns>
        private async Task AddStudentsToSiteAsync(List<UserViewModel> lstStudentsSite, List<UserViewModel> lstStudentsDecanat, string currentYear)
        {
            var idStudentsSite = lstStudentsSite.Select(a => a.DecanatId).ToList();
            var idStudentsDecanat = lstStudentsDecanat.Select(a => a.DecanatId).ToList();
            var result = idStudentsDecanat.Except(idStudentsSite).ToList();

            foreach (var s in result)
            {
                //сначала поиск студента на сайте, среди заблокированных
                var student = db.Users.Include(a => a.Group).Include(a => a.Faculty).FirstOrDefault(a => a.DecanatId != null && a.DecanatId == s);
                if (student != null)
                {
                    student.Email = student.Email.Substring(student.Email.IndexOf("___") + 3);
                    student.DateBlocked = null;
                    db.Entry(student).State = EntityState.Modified;
                }
                else
                {
                    //если студент не найден, то получаем данные о нем из деканата и добавляем на сайт
                    var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DecanatConnection"].ConnectionString;
                    string sql = "SELECT dbo.Специальности.Код AS КодСпециальности, dbo.Специальности.Название_Спец, dbo.Все_Группы.Код AS КодГруппы, dbo.УсловияОбучения.Сокращение, " +
                                 "dbo.Все_Группы.Название AS Группа, dbo.Все_Группы.Курс, dbo.Факультеты.Факультет, dbo.Все_Студенты.*, dbo.Все_Группы.Код_Факультета  " +
                                 "FROM dbo.Специальности INNER JOIN " +
                                 "dbo.Все_Группы ON dbo.Специальности.Код = dbo.Все_Группы.Код_Специальности INNER JOIN " +
                                 "dbo.Факультеты ON dbo.Все_Группы.Код_Факультета = dbo.Факультеты.Код INNER JOIN " +
                                 "dbo.Все_Студенты ON dbo.Все_Группы.Код = dbo.Все_Студенты.Код_Группы INNER JOIN " +
                                 "dbo.УсловияОбучения ON dbo.Все_Студенты.УслОбучения = dbo.УсловияОбучения.Код " +
                                 "WHERE dbo.Все_Студенты.Статус = 1 AND dbo.Все_Группы.УчебныйГод = '" + currentYear + "' AND dbo.Все_Студенты.Код = " + s.ToString();
                    var studentDecanat = new ApplicationUser();

                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        using (var command = new SqlCommand(sql, connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                string newPass = Guid.NewGuid().ToString();
                                while (await reader.ReadAsync())
                                {
                                    studentDecanat.DecanatId = Convert.ToInt32(reader["Код"]);
                                    studentDecanat.Lastname = reader["Фамилия"].ToString();
                                    studentDecanat.Firstname = reader["Имя"].ToString();
                                    studentDecanat.Middlename = reader["Отчество"].ToString();
                                    studentDecanat.Email = studentDecanat.DecanatId.ToString() + "@strbsu1.ru";
                                    studentDecanat.BirthDay = (reader["Дата_Рождения"] is DBNull) ? DateTime.Now : Convert.ToDateTime(reader["Дата_Рождения"]);
                                    studentDecanat.idGroupDecanat = Convert.ToInt32(reader["КодГруппы"]);
                                    studentDecanat.idFacultyDecanat = Convert.ToInt32(reader["Код_Факультета"]);
                                    studentDecanat.idProfileDecanat = Convert.ToInt32(reader["КодСпециальности"]);
                                    studentDecanat.MoodleId = studentDecanat.DecanatId;
                                    studentDecanat.UserName = studentDecanat.DecanatId.ToString();
                                    studentDecanat.StatusId = Convert.ToInt32(reader["Статус"]);
                                    studentDecanat.NumberOfRecordBook = reader["Номер_Зачетной_Книжки"].ToString();
                                    studentDecanat.YearOfReceipt = (reader["Год_Поступления"] is DBNull) ? (DateTime.Now.Year - Convert.ToInt32(reader["Курс"]) + 1) : Convert.ToInt32(reader["Год_Поступления"]);
                                    studentDecanat.Sex = (reader["Пол"].ToString() == "Муж") ? true : false;
                                    studentDecanat.OrderNumber = (reader["НомерПриказаОЗачислении"] is DBNull) ? "" : reader["НомерПриказаОЗачислении"].ToString();
                                    studentDecanat.ConditionsOfEducationId = (reader["УслОбучения"] is DBNull) ? 1 : Convert.ToInt32(reader["УслОбучения"]);
                                    studentDecanat.DateEnrollment = (reader["ДатаЗачисления"] is DBNull) ? DateTime.Now : Convert.ToDateTime(reader["ДатаЗачисления"]);
                                    studentDecanat.HouseApartment = reader["Дом_Кв_ПП"].ToString();
                                    studentDecanat.ExpirationDate = (reader["ДатаОкончания"] is DBNull) ? DateTime.Now : Convert.ToDateTime(reader["ДатаОкончания"]);
                                    studentDecanat.Village = (reader["ДатаОкончания"] is DBNull) ? true : Convert.ToBoolean(reader["Село"]);
                                    studentDecanat.ContractNumber = reader["Номер_договора"].ToString();
                                    studentDecanat.Country = reader["Страна_ПП"].ToString();
                                    studentDecanat.Language = reader["Изучаемый_Язык"].ToString();
                                    studentDecanat.Bases = reader["Сокращение"].ToString();
                                    studentDecanat.Nationality = reader["Национальность"].ToString();
                                    studentDecanat.School = reader["Уч_Заведение"].ToString();
                                    studentDecanat.SchoolLocation = reader["Где_Находится_УЗ"].ToString();
                                    studentDecanat.YearOfEndingSchool = (reader["Год_Окончания_УЗ"] is DBNull) ? studentDecanat.YearOfReceipt : Convert.ToInt32(reader["Год_Окончания_УЗ"]);
                                    studentDecanat.HighAchiever = (reader["Отличник_УЗ"].ToString() != "") ? true : false;
                                    studentDecanat.Region = reader["Регион_ПП"].ToString();
                                    studentDecanat.City = reader["Город_ПП"].ToString();
                                    studentDecanat.Postcode = reader["Индекс_ПП"].ToString();
                                    studentDecanat.Street = reader["Улица_ПП"].ToString();
                                    studentDecanat.PassMoodle = newPass;
                                }
                            }
                        }
                        var group = db.Groups.FirstOrDefault(a => a.DecanatID == studentDecanat.idGroupDecanat && a.IsDeleted == false);
                        studentDecanat.GroupId = group.Id;
                        studentDecanat.FacultyId = group.FacultyId;

                        var createResult = userManager.Create(studentDecanat, "ProG12#");
                        //добавляем роль студента
                        if (createResult.Succeeded)
                        {
                            var r = userManager.AddToRole(studentDecanat.Id, "Students");
                        }
                    }
                }
            }

            try
            {
                var i = db.SaveChanges();
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
        /// Добавление недостающих Студентов в Moodle
        /// </summary>
        /// <returns></returns>
        private async Task AddStudentsToMoodleAsync(List<UserViewModel> lstStudentsSite, List<UserViewModel> lstStudentsDecanat)
        {
            var idStudentsMoodle = lstStudentsSite.Select(a => a.DecanatId).ToList();
            var idStudentsDecanat = lstStudentsDecanat.Select(a => a.DecanatId).ToList();
            var result = idStudentsDecanat.Except(idStudentsMoodle).ToList();
            foreach (var s in result)
            {
                //поиск студента в мудл, на тот случай если он не числится в группе, но есть на сайте
                //поиск среди пользователей
                int idStudDec = s.Value;

                string pg_sql = "SELECT public.mdl_user.id, username, lastname, firstname, middlename, deleted, password, email, public.mdl_cohort.name AS group " +
                                "FROM public.mdl_cohort, public.mdl_cohort_members, public.mdl_user " +
                                "WHERE mdl_cohort_members.cohortid = mdl_cohort.id AND mdl_user.id = mdl_cohort_members.userid AND " +
                                "deleted = 0 AND username = '" + idStudDec + "'";
                var student = new UserViewModel();
                using (var myConnection = new NpgsqlConnection(connectionString))
                {
                    await myConnection.OpenAsync();
                    using (var pg_command = new NpgsqlCommand(pg_sql, myConnection))
                    {
                        using (var reader = await pg_command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                student.DecanatId = Convert.ToInt32(reader["username"]);
                                student.Lastname = reader["lastname"].ToString();
                                student.Firstname = reader["firstname"].ToString();
                                student.Middlename = reader["middlename"].ToString();
                                student.idGroupDecanat = null;
                                student.idFacultyDecanat = null;
                                student.UserName = reader["id"].ToString();//записываем сюда id студента в moodle, так проще обновить его данные
                                student.GroupName = reader["group"].ToString();//записываем сюда название группы, так проще сравнивать, где студент учится
                                student.Sex = true;
                                student.Email = reader["email"].ToString();
                            }
                        }
                    }
                }

                //студент найден среди пользователей, на всякий случай удаляем его из групп
                if (!String.IsNullOrEmpty(student.UserName))
                {
                    FindStudentAndUpdateGroupMoodle(student.UserName, lstStudentsDecanat, idStudDec);
                }
                else
                {
                    //добавляем студента отсутствующего в moodle   
                    //получаем группу в moodle, в которую зачислен студент в деканате       
                    var studentDecanat = lstStudentsDecanat.FirstOrDefault(a => a.DecanatId == idStudDec);
                    //поиск ИД новой группы студента в Moodle
                    int idMoodleGroup = 0;
                    pg_sql = $"SELECT id, name FROM mdl_cohort WHERE name = '{studentDecanat.GroupName}'";
                    using (var myConnection = new NpgsqlConnection(connectionString))
                    {
                        await myConnection.OpenAsync();
                        using (var pg_command = new NpgsqlCommand(pg_sql, myConnection))
                        {
                            using (var reader = await pg_command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    idMoodleGroup = Convert.ToInt32(reader["id"]);
                                }
                            }
                        }
                    }

                    //проверка найден ли ИД новой группы
                    if (idMoodleGroup == 0)
                    {
                        //создаем недостающую группу в Moodle, если она не была найдена, и получаем ее ИД
                        using (var myConnection = new NpgsqlConnection(connectionString))
                        {
                            await myConnection.OpenAsync();
                            int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

                            string idCohortMoodle = "INSERT INTO mdl_cohort (contextid, name, descriptionformat, component, timecreated, timemodified)" +
                                            " VALUES ('1', '" + s + "', '1', '', " + unixTime + ", " + unixTime + ") RETURNING id";

                            using (var myCommand = new NpgsqlCommand(idCohortMoodle, myConnection))
                            {
                                object idNewGroup = myCommand.ExecuteScalarAsync();
                                idMoodleGroup = Convert.ToInt32(idNewGroup);
                            }
                        }
                    }
                    //получаем пароль от мудл для пользователя
                    var currentStudentSite = db.Users.FirstOrDefault(a => a.DecanatId == idStudDec && a.DateBlocked == null);
                    string newPass = "";
                    if (String.IsNullOrEmpty(currentStudentSite.PassMoodle))
                    {
                        newPass = Guid.NewGuid().ToString();
                        currentStudentSite.PassMoodle = newPass;
                        db.Entry(currentStudentSite).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        newPass = currentStudentSite.PassMoodle;
                    }

                    //уточняем почту, приоритет отдаем почте из ЛК
                    string email = studentDecanat.DecanatId + "@strbsu1.ru;";
                    if (email != currentStudentSite.Email)
                        email = currentStudentSite.Email;

                    string fileCSV = "";
                    //создание csv-файла группы для добавления новых студентов                    
                    fileCSV += studentDecanat.DecanatId + ";";
                    fileCSV += newPass + ";";
                    fileCSV += studentDecanat.Firstname + ";" + studentDecanat.Middlename + ";";
                    fileCSV += studentDecanat.Lastname + ";";
                    fileCSV += email + ";";
                    fileCSV += "Стерлитамак;RU;" + idMoodleGroup;
                    fileCSV += ";0";

                    //запись студента в файл для групповых операций
                    FileCSV(fileCSV);
                }
            }
        }

        /// <summary>
        /// Поиск студента в группах Moodle, удаление его от туда и запись в правильную группу
        /// </summary>
        /// <param name="userName">ИД студента в Moodle</param>
        /// <param name="lstStudentsDecanat">Список студентов Деканата</param>
        /// <param name="idStudDec">ИД студента в деканате</param>
        private async void FindStudentAndUpdateGroupMoodle(string userName, List<UserViewModel> lstStudentsDecanat, int idStudDec)
        {
            var pg_sql = $"SELECT cohortid FROM mdl_cohort_members WHERE userid = {userName}";
            using (var myConnection = new NpgsqlConnection(connectionString))
            {
                await myConnection.OpenAsync();
                using (var pg_command = new NpgsqlCommand(pg_sql, myConnection))
                {
                    using (var reader = await pg_command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var groupMoodleId = reader["cohortid"].ToString();
                            System.Diagnostics.Process.Start("http://sdo.strbsu.ru/cohort/removeusercohort.php?cohortid="
                                + groupMoodleId + "&userid=" + userName);
                        }
                    }
                }
            }

            //добавляем найденного студента в нужную группу
            var studentDecanat = lstStudentsDecanat.FirstOrDefault(a => a.DecanatId == idStudDec);
            if (studentDecanat != null)
            {
                var groupMoodle = db.Groups.FirstOrDefault(a => a.DecanatID == studentDecanat.idGroupDecanat && a.IsDeleted == false);
                System.Diagnostics.Process.Start("http://sdo.strbsu.ru/cohort/addusercohort.php?cohortid=" + groupMoodle.MoodleID + "&userid=" + userName);
            }
        }

        /// <summary>
        /// Синхронизация студентов
        /// </summary>
        /// <returns></returns>
        public async Task SyncStudentsAsync()
        {
            Settings currentYear = db.Settings.FirstOrDefault(a => a.Name == "CurrentYear");

            //1. получение списка студентов из базы сайта
            var lstStudentsSite = await GetStudentsSiteAsync();
            _pm("Получен список студентов из базы сайта");

            //2. получение списка студентов из деканата
            var lstStudentsDecanat = await GetStudentsDecanatAsync(currentYear.Value);
            _pm("Получен список студентов из Деканата");

            //3. получение списка студентов из Moodle
            var lstStudentsMoodle = await GetStudentsMoodleAsync();
            _pm("Получен список студентов из Moodle");

            //4. убираем удаленных студентов с сайта с помощью сравнения по id
            await DeleteStudentsFromSiteAsync(lstStudentsSite, lstStudentsDecanat);
            _pm("Заблокированы отчисленные студенты на сайте");

            //5. убираем удаленных студентов из Moodle
            await DeleteStudentsFromMoodleAsync(lstStudentsMoodle, lstStudentsDecanat);
            _pm("Удалены отчисленные студенты в Moodle");

            //6.1. получение списка студентов из базы сайта
            lstStudentsSite = await GetStudentsSiteAsync();

            //6.2. находим студентов (на сайте), данные которых изменились в деканате            
            await UpdateStudentsOnSiteAsync(lstStudentsSite, lstStudentsDecanat);
            _pm("Обновлены данные студентов на сайте");

            //6.3. получение списка студентов из Moodle
            lstStudentsMoodle = await GetStudentsMoodleAsync();
            _pm("Получен список студентов из Moodle");

            //6.4. находим студентов (в Moodle), данные которых изменились в деканате            
            await UpdateStudentsToMoodleAsync(lstStudentsMoodle, lstStudentsSite, currentYear.Value);
            _pm("Обновлены данные студентов в Moodle");

            //7. добавляем студентов, которые есть в деканате, но нет на сайте
            await AddStudentsToSiteAsync(lstStudentsSite, lstStudentsDecanat, currentYear.Value);
            _pm("Добавлены данные новых студентов на сайт");

            //8. добавляем студентов, которые есть в деканате, но нет в Moodle
            await AddStudentsToMoodleAsync(lstStudentsMoodle, lstStudentsDecanat);
            _pm("Добавлены данные новых студенты в Moodle");

            _pm("--- Синхронизация студентов завершена ---");
        }

        public delegate void ProgressMessage(string msg);
        ProgressMessage _pm;

        public void MessageHandler(ProgressMessage pm)
        {
            _pm = pm;
        }
    }
}
