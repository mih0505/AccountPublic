using AccountRPD.BL.Infrastructure;
using System;

namespace AccountRPD.BL.Interfaces
{
    public interface ILoginService : IDisposable
    {
        /// <summary>
        /// Авторизирует пользователя
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="password">Введенный пользователем пароль</param>
        /// <returns></returns>
        Session Login(string login, string password);  
    }
}
