using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Accounts.Models
{
    public class ExcelStatisticViewModel
    {
        public int Course { get; set; } // Номер курса
        public double StudentsCount { get; set; } // Количество студентов на курсе

        public static double AllStudentsCount { get; set; }

        public static double AllPassedCount { get; set; }
        public static double AllPassedCountPercent
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

        public static double AllPassedOnExcellent { get; set; }
        public static double AllPassedOnExcellentPercent
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

        public static double AllPassedOnVeryWell { get; set; }
        public static double AllPassedOnVeryWellPercent
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

        public static double AllPassedOnGood { get; set; }
        public static double AllPassedOnGoodPercent
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

        public static double AllPassedOnOkey { get; set; }
        public static double AllPassedOnOkeyPercent
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

        public static double AllDebtorsCount { get; set; }
        public static double AllDebtorsCountPercent
        {
            get
            {
                if (AllStudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((AllDebtorsCount / AllStudentsCount) * 100.00, 2);
            }
        }

        public static double AllDebtorsInOne { get; set; }
        public static double AllDebtorsInOnePercent
        {
            get
            {
                if (AllStudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((AllDebtorsInOne / AllStudentsCount) * 100.00, 2);
            }
        }

        public static double AllDebtorsInTwo { get; set; }
        public static double AllDebtorsInTwoPercent
        {
            get
            {
                if (AllStudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((AllDebtorsInTwo / AllStudentsCount) * 100.00, 2);
            }
        }

        public static double AllDebtorsInMoreThree { get; set; }
        public static double AllDebtorsInMoreThreePercent
        {
            get
            {
                if (AllStudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((AllDebtorsInMoreThree / AllStudentsCount) * 100.00, 2);
            }
        }

        public double PassedCount { get; set; } // Всего студентов, сдавших экзамены
        public double PassedCountPercent
        {
            get
            {
                if (StudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((PassedCount / StudentsCount) * 100.00, 2);
            }
        }

        public double PassedOnExcellent { get; set; } // Студенты, сдавшие все экзамены на отлично (только на 5)
        public double PassedOnExcellentPercent
        {
            get
            {
                if (StudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((PassedOnExcellent / StudentsCount) * 100.00, 2);
            }
        }

        public double PassedOnVeryWell { get; set; } // Студенты, сдавшие все экзамены на хорошо и отлично (на 4 и 5)
        public double PassedOnVeryWellPercent
        {
            get
            {
                if (StudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((PassedOnVeryWell / StudentsCount) * 100.00, 2);
            }
        }

        public double PassedOnGood { get; set; } // Студенты, сдавшие все экзамены на хорошо (только 4)
        public double PassedOnGoodPercent
        {
            get
            {
                if (StudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((PassedOnGood / StudentsCount) * 100.00, 2);
            }
        }

        public double PassedOnOkey { get; set; } // Студенты, имеющие по каким либо экзаменам 3-ки
        public double PassedOnOkeyPercent
        {
            get
            {
                if (StudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((PassedOnOkey / StudentsCount) * 100.00, 2);
            }
        }

        public double DebtorsCount { get; set; } // Всего студентов, имеющие задолженности
        public double DebtorsCountPercent
        {
            get
            {
                if (StudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((DebtorsCount / StudentsCount) * 100.00, 2);
            }
        }

        public double DebtorsInOne { get; set; } // Студенты, имеющие задолженности по 1-у предмету
        public double DebtorsInOnePercent
        {
            get
            {
                if (StudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((DebtorsInOne / StudentsCount) * 100.00, 2);
            }
        }

        public double DebtorsInTwo { get; set; } // Студенты, имеющие задолженности по 2-ум предметам
        public double DebtorsInTwoPercent
        {
            get
            {
                if (StudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((DebtorsInTwo / StudentsCount) * 100.00, 2);
            }
        }

        public double DebtorsInMoreThree { get; set; } // Студенты, имеющие задолжености по 3-ем и более предметам
        public double DebtorsInMoreThreePercent
        {
            get
            {
                if (StudentsCount.Equals(0.0))
                {
                    return 0.0;
                }

                return Math.Round((DebtorsInMoreThree / StudentsCount) * 100.00, 2);
            }
        }
    }
}