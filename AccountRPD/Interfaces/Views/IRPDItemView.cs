using Account.DAL.Entities;
using System;
using System.Collections.Generic;

namespace AccountRPD.Interfaces.Views
{
    public interface IRPDItemView : IView
    {
        /// <summary>
        /// Порядковый номер пункта
        /// </summary>
        string Number { get; set; }

        /// <summary>
        /// Коллекция пунктов - родителей для текущуго пункта
        /// </summary>
        Dictionary<int, string> Parents { get; set; }

        /// <summary>
        /// Выбранный пункт - родитель
        /// </summary>
        RPDItem SelectedParent { get; }

        /// <summary>
        /// Название пункта
        /// </summary>
        string NameItem { get; set; }

        /// <summary>
        /// Шаблон текста для пункта
        /// </summary>
        string TemplateItem { get; set; }

        event EventHandler SaveRPDItemClick;
    }
}