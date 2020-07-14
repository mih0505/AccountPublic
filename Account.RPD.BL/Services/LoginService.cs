using Account.DAL.Interfaces;
using AccountRPD.BL.Infrastructure;
using AccountRPD.BL.Interfaces;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;

namespace AccountRPD.BL.Services
{
    public class LoginService : ILoginService
    {
        private readonly IEFUnitOfWork efUnitOfWork;
        private readonly IDecanatUnitOfWork decanatUnitOfWork;

        public LoginService(IEFUnitOfWork efUnitOfWork, IDecanatUnitOfWork decanatUnitOfWork, IManagerService managerService)
        {
            this.efUnitOfWork = efUnitOfWork;
            this.decanatUnitOfWork = decanatUnitOfWork;
        }

        public Session Login(string login, string password)
        {
            var verificationResult = PasswordVerificationResult.Failed;

            /* Внимание! Не использовать асинхронный способ получения сущности пользователя (зависает) */
            var user = efUnitOfWork.UserManager.FindByEmail(login);

            var session = Session.GetSession();
            session.User = user;

            var isStudent = false;

            if (session.User != null)
            {
                verificationResult = efUnitOfWork.UserManager.PasswordHasher.VerifyHashedPassword(session.User.PasswordHash, password);
                isStudent = efUnitOfWork.UserManager.IsInRole(session.User.Id, "Students") && !efUnitOfWork.UserManager.IsInRole(session.User.Id, "Administrators");
            }

            if (isStudent)
            {
                var message = "Студент не может иметь доступ к Электронному РПД";
                session.Details = new OperationDetails(false, message, nameof(LoginService.Login));
            }
            else if (verificationResult.Equals(PasswordVerificationResult.Success))
            {
                session.Roles = efUnitOfWork.UserManager.GetRoles(session.User.Id);

                var userDepartmentsIds = efUnitOfWork.TeacherDepartments.GetAllIdDepartmentsOfUser(session.User.Id);
                session.Departments = efUnitOfWork.Departments.GetAllDepartmentOfUser(userDepartmentsIds);

                if (!efUnitOfWork.UserManager.IsInRole(session.User.Id, "Administrators"))
                {
                    var decanatDepartmentsIds = efUnitOfWork.TeacherDepartments.GetAllUserDecanatDepartmentsIds(session.User.Id).ToList();
                    session.DecanatDepartments = decanatUnitOfWork.Departments.GetUserDepartments(decanatDepartmentsIds);
                }
                else
                {
                    session.DecanatDepartments = decanatUnitOfWork.Departments.GetAll();
                }

                var currentDate = DateTime.Now.Date;
                var studyYear = decanatUnitOfWork.GetStudyYear(currentDate);
                session.CurrentStudyYear = studyYear;

                var isDataLoad = session.Roles != null && session.Departments != null && !string.IsNullOrWhiteSpace(session.CurrentStudyYear);

                var message = (isDataLoad) ? "Данные пользователя получены" : "Ошибка получения данных о пользователе";
                session.Details = new OperationDetails(isDataLoad, message, nameof(LoginService.Login));
            }
            else
            {
                var message = "Неудачная попытка входа";
                session.Details = new OperationDetails(false, message, nameof(LoginService.Login));
            }

            return session;
        }

        public void Dispose()
        {
            efUnitOfWork.Dispose();
            decanatUnitOfWork.Dispose();
        }
    }
}
