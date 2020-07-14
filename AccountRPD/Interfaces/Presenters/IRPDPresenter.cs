using Account.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountRPD.Interfaces.Presenters
{
    public interface IRPDPresenter : IPresenter
    {
        DecanatDepartment Department { get; }
        DecanatDiscipline Discipline { get; }
        DecanatPlan Plan { get; }
    }
}
