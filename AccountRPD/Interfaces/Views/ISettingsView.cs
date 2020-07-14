using Account.DAL.Entities;
using System;
using System.Collections.Generic;

namespace AccountRPD.Interfaces.Views
{
    public interface ISettingsView : IView
    {
        //IEnumerable<RPDItem> ItemsList { get; set; }
        //RPDItem SelectedRPD { get; }
        //EducationStandard SelectedStandardInComboBox { get; }
        //EducationStandard SelectedStandardInTable { get; }
        //IEnumerable<EducationStandard> StandardSelectList { get; set; }
        //IEnumerable<EducationStandard> StandardsList { get; set; }

        //event EventHandler AddItemHandler;
        //event EventHandler AddStandardHandler;
        //event EventHandler DeleteItemHandler;
        //event EventHandler DeleteStandardHandler;
        //event EventHandler EditItemHandler;
        //event EventHandler EditStandardHandler;
        //event EventHandler StandardValuesChangedHandler;
        
            
        /// <summary>
        /// Список пунктов содержания рабочей программы
        /// </summary>
        IEnumerable<RPDItem> ItemsList { get; set; }

        /// <summary>
        /// Выбранный пункт содержания РПД
        /// </summary>
        RPDItem SelectedItem { get; }

        /// <summary>
        /// Выбранный стандарт в фильтре
        /// </summary>
        EducationStandard SelectedStandardInFilter { get; }

        /// <summary>
        /// Выбранный стандарт в списке стандартов
        /// </summary>
        EducationStandard SelectedStandard { get; }

        /// <summary>
        /// Список стандартов для фильтра
        /// </summary>
        IEnumerable<EducationStandard> StandardSelectList { get; set; }

        /// <summary>
        /// Список стандартов
        /// </summary>
        IEnumerable<EducationStandard> StandardsList { get; set; }


        /// <summary>
        /// Событие, добавление пункта в содержание РПД
        /// </summary>
        event EventHandler AddItemHandler;

        /// <summary>
        /// Событие, добавление нового стандарта образования
        /// </summary>
        event EventHandler AddStandardHandler;

        /// <summary>
        /// Событие, удаление пункта из содержания РПД
        /// </summary>
        event EventHandler DeleteItemHandler;

        /// <summary>
        /// Событие, удаление стандарта образования
        /// </summary>
        event EventHandler DeleteStandardHandler;

        /// <summary>
        /// Событие, редактировать пункт содержания РПД
        /// </summary>
        event EventHandler EditItemHandler;

        /// <summary>
        /// Событие, редактирование образовательного стандарта
        /// </summary>
        event EventHandler EditStandardHandler;

        /// <summary>
        /// Событие, выбор образовательного стандарта в фильтре
        /// </summary>
        event EventHandler StandardValuesChangedHandler;
    }
}