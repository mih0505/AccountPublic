using Account.DAL.Entities;
using Accounts.Models;
using System.Collections.Generic;

namespace AccountRPD.BL.Infrastructure
{
    public class Session
    {
        private static Session session;
        
        public static Session GetSession()
        {
            if (session == null)
            {
                session = new Session();
            }

            return session;
        }

        /// <summary>
        /// Текущий пользователь
        /// </summary>
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Роли текущего пользователя
        /// </summary>
        public IEnumerable<string> Roles { get; set; }

        /// <summary>
        /// Кафедры текущего пользователя
        /// </summary>
        public IEnumerable<Department> Departments { get; set; }

        /// <summary>
        /// Кафедры пользователя, синхронизированные с Деканатом
        /// </summary>
        public IEnumerable<DecanatDepartment> DecanatDepartments { get; set; }

        /// <summary>
        /// Текущий учебный год
        /// </summary>
        public string CurrentStudyYear { get; set; }

        /// <summary>
        /// Детали авторизации
        /// </summary>
        public OperationDetails Details { get; set; }

        public Session()
        {
            Roles = new List<string>();
            Departments = new List<Department>();
            DecanatDepartments = new List<DecanatDepartment>();
        }
    }
}
