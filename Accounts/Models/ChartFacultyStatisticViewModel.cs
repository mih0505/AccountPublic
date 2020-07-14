using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Accounts.Models
{
    public class ChartFacultyStatisticViewModel
    {
        public string FacultyName { get; set; }
        public string FacultyShortName { get; set; }

        public double AllStudentsCount { get; set; }

        public double AllPassedCount { get; set; }
        public double AllPassedCountPercent
        {
            get
            {
                if (AllStudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((AllPassedCount / AllStudentsCount) * 100.00, 2);
            }
        }

        public double AllPassedOnExcellent { get; set; }
        public double AllPassedOnExcellentPercent
        {
            get
            {
                if (AllStudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((AllPassedOnExcellent / AllStudentsCount) * 100.00, 2);
            }
        }

        public double AllPassedOnVeryWell { get; set; }
        public double AllPassedOnVeryWellPercent
        {
            get
            {
                if (AllStudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((AllPassedOnVeryWell / AllStudentsCount) * 100.00, 2);
            }
        }

        public double AllPassedOnGood { get; set; }
        public double AllPassedOnGoodPercent
        {
            get
            {
                if (AllStudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((AllPassedOnGood / AllStudentsCount) * 100.00, 2);
            }
        }

        public double AllPassedOnOkey { get; set; }
        public double AllPassedOnOkeyPercent
        {
            get
            {
                if (AllStudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((AllPassedOnOkey / AllStudentsCount) * 100.00, 2);
            }
        }

        public double Perfomance { get; set; }
        public double Quality { get; set; }
        public double ExcellentStudents { get; set; }
        public double PerfomancePercent { get; set; }
        public double QualityPercent { get; set; }
        public double ExcellentStudentsPercent { get; set; }
    }
}