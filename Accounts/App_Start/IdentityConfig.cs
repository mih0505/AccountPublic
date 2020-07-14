using Accounts.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Accounts
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Подключите здесь службу электронной почты для отправки сообщения электронной почты.
            //return Task.FromResult(0);
            // настройка логина, пароля отправителя
            var from = "sdo.system@strbsu.ru";
            var pass = "SectorDO2020";

            // адрес и порт smtp-сервера, с которого мы и будем отправлять письмо
            SmtpClient client = new SmtpClient("smtp.mail.ru", 25);

            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(from, pass);
            client.EnableSsl = true;

            // создаем письмо: message.Destination - адрес получателя
            var mail = new MailMessage(from, message.Destination);
            mail.Subject = message.Subject;
            mail.Body = message.Body;
            mail.IsBodyHtml = true;

            return client.SendMailAsync(mail);

        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Подключите здесь службу SMS, чтобы отправить текстовое сообщение.
            return Task.FromResult(0);
        }
    }

    // Настройка диспетчера пользователей приложения. UserManager определяется в ASP.NET Identity и используется приложением.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Настройка логики проверки имен пользователей
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Настройка логики проверки паролей
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // Настройка параметров блокировки по умолчанию
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 10;

            // Регистрация поставщиков двухфакторной проверки подлинности. Для получения кода проверки пользователя в данном приложении используется телефон и сообщения электронной почты
            // Здесь можно указать собственный поставщик и подключить его.
            manager.RegisterTwoFactorProvider("Код, полученный по телефону", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Ваш код безопасности: {0}"
            });
            manager.RegisterTwoFactorProvider("Код из сообщения", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Код безопасности",
                BodyFormat = "Ваш код безопасности: {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"))
                    {
                        TokenLifespan = TimeSpan.FromHours(1)
                    };
            }
            return manager;
        }
    }

    public class ApplicationRoleManager : RoleManager<IdentityRole>
    {
        public ApplicationRoleManager(IRoleStore<IdentityRole, string> roleStore)
            : base(roleStore)
        {
        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            return new ApplicationRoleManager(new RoleStore<IdentityRole>(context.Get<ApplicationDbContext>()));
        }
    }

    // This example shows you how to create a new database if the Model changes
    //public class ApplicationDbInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    public class ApplicationDbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            InitializeIdentityForEF(context);
            base.Seed(context);
        }

        public static void InitializeIdentityForEF(ApplicationDbContext db)
        {
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

            //статусы студента            
            var st1 = new Status
            {
                Name = "Академический",
                ShortName = "Академ",
                DecanatID = -1,
                Description = "Находится в академическом отпуске"
            };
            var st2 = new Status
            {
                Name = "Учащийся",
                ShortName = "Уч",
                DecanatID = 1,
                Description = "Учится в университете"
            };
            var st3 = new Status
            {
                Name = "Отчислен",
                ShortName = "Отч",
                DecanatID = 3,
                Description = "Отчислен из университета"
            };
            var st4 = new Status
            {
                Name = "Закончил",
                ShortName = "Зак",
                DecanatID = 4,
                Description = "Закончил обучение"
            };
            var st5 = new Status
            {
                Name = "Призван",
                ShortName = null,
                DecanatID = 5,
                Description = "Призван в армию"
            };
            var st6 = new Status
            {
                Name = "Абитуриент",
                ShortName = "Абит.",
                DecanatID = 6,
                Description = "Поступающий в университет"
            };

            var st7 = new Status
            {
                Name = "БывшАбит",
                ShortName = "Выбыл",
                DecanatID = 7,
                Description = "Выбыл из участия в поступлении"
            };
            var st8 = new Status
            {
                Name = "Зачислен",
                ShortName = "Зачислен",
                DecanatID = 9,
                Description = "Абитуриент, зачисленннный в университет"
            };
            var st9 = new Status
            {
                Name = "Web-Абитуриент",
                ShortName = null,
                DecanatID = 10,
                Description = ""
            };

            if (!db.Statuses.Any())
            {
                db.Statuses.AddOrUpdate(
                  p => p.Name,
                    st1, st2, st3, st4, st5, st6, st7, st8, st9
                );
                db.SaveChanges();
            }


            //основания обучения
            var ce1 = new ConditionsOfEducation
            {
                Name = "ОО",
                Description = "Общие основания"
            };
            var ce2 = new ConditionsOfEducation
            {
                Name = "ЦН",
                Description = "Целевое направление"
            };
            var ce3 = new ConditionsOfEducation
            {
                Name = "Контрактная основа",
                Description = "К"
            };

            if (!db.ConditionsOfEducations.Any())
            {
                db.ConditionsOfEducations.AddOrUpdate(
                  p => p.Name,
                    ce1, ce2, ce3
                );
                db.SaveChanges();
            }

            //факультеты
            var f1 = new Faculty
            {
                Name = "Естественнонаучный факультет",
                AliasFaculty = "ЕНФ",
                DecanatID = 29
            };
            var f2 = new Faculty
            {
                Name = "Факультет математики и информационных технологий",
                AliasFaculty = "ФМиИТ",
                DecanatID = 28
            };
            var f3 = new Faculty
            {
                Name = "Филологический факультет",
                AliasFaculty = "ФФ",
                DecanatID = 30
            };
            var f4 = new Faculty
            {
                Name = "Факультет башкирской и тюркской филологии",
                AliasFaculty = "ФБиТФ",
                DecanatID = 31
            };
            var f5 = new Faculty
            {
                Name = "Исторический факультет",
                AliasFaculty = "ИФ",
                DecanatID = 32
            };
            var f6 = new Faculty
            {
                Name = "Юридический факультет",
                AliasFaculty = "ЮФ",
                DecanatID = 36
            };
            var f7 = new Faculty
            {
                Name = "Факультет педагогики и психологии",
                AliasFaculty = "ФПП",
                DecanatID = 33
            };
            var f8 = new Faculty
            {
                Name = "Экономический факультет",
                AliasFaculty = "ЭФ",
                DecanatID = 34
            };
            var f9 = new Faculty
            {
                Name = "Колледж",
                AliasFaculty = "Колледж СФ БашГУ",
                DecanatID = 37
            };



            if (!db.Faculties.Any())
            {
                db.Faculties.AddOrUpdate(
                  p => p.Name,
                    f1, f2, f3, f4, f5, f6, f7, f8, f9
                );
                db.SaveChanges();
            }

            if (roleManager.FindByName("Administrators") == null)
            {
                roleManager.Create(new IdentityRole("Administrators"));
            }
            if (roleManager.FindByName("Students") == null)
            {
                roleManager.Create(new IdentityRole("Students"));
            }
            if (roleManager.FindByName("DepartmentsManagers") == null)
            {
                roleManager.Create(new IdentityRole("DepartmentsManagers"));
            }
            if (roleManager.FindByName("FacultiesManagers") == null)
            {
                roleManager.Create(new IdentityRole("FacultiesManagers"));
            }
            if (roleManager.FindByName("TutorsManagers") == null)
            {
                roleManager.Create(new IdentityRole("TutorsManagers"));
            }
            if (roleManager.FindByName("Teachers") == null)
            {
                roleManager.Create(new IdentityRole("Teachers"));
            }




            #region InitializingData

            if (!db.Plans.Any())
            {
                db.Plans.Add(new Plan { Name = "План" });
                db.SaveChanges();
            }

            var s1 = new Section
            {
                Name = "Учебная деятельность",
                Alias = "Учебная деятельность",
                Description = "Данный раздел содержит Ваши достижения в учебе. Данный раздел формируется автоматически на основе учебного плана и Вашей успеваемости."
            };
            var s2 = new Section
            {
                Name = "Науч.-исслед. деятельность",
                Alias = "Научно-исследовательская деятельность",
                Description = "Данный раздел содержит Ваши достижения в области науки. Основные категории, по которым Вы можете разместить свои достижения, " +
                "представлены ниже. Для добавления элемента необходимо нажать на соответствующую гиперссылку под названием категории. " +
                "Каждый элемент портфолио должен содержать краткую информацию о Вашем достижении и его подтверждение в виде ссылки, " +
                "на внешний ресурс, или файла (изображение или pdf-документ, размером не более 3 МБ)."
            };
            var s3 = new Section
            {
                Name = "Общественная деятельность",
                Alias = "Общественная деятельность",
                Description = "Данный раздел содержит Ваши достижения в общественной сфере. Основные категории, по которым Вы можете разместить свои достижения, " +
                "представлены ниже. Для добавления элемента необходимо нажать на соответствующую гиперссылку под названием категории. " +
                "Каждый элемент портфолио должен содержать краткую информацию о Вашем достижении и его подтверждение в виде ссылки, " +
                "на внешний ресурс, или файла (изображение или pdf-документ, размером не более 3 МБ)."
            };
            var s4 = new Section
            {
                Name = "Спортивная деятельность",
                Alias = "Спортивная деятельность",
                Description = "Данный раздел содержит Ваши спортивные достижения. Основные категории, по которым Вы можете разместить свои достижения, " +
                "представлены ниже. Для добавления элемента необходимо нажать на соответствующую гиперссылку под названием категории. " +
                "Каждый элемент портфолио должен содержать краткую информацию о Вашем достижении и его подтверждение в виде ссылки, " +
                "на внешний ресурс, или файла (изображение или pdf-документ, размером не более 3 МБ)."
            };
            var s5 = new Section
            {
                Name = "Культ.-твор. деятельность",
                Alias = "Культурно-творческая деятельность",
                Description = "Данный раздел содержит Ваши достижения в культурно-творческой сфере. Основные категории, по которым Вы можете разместить свои достижения, " +
                "представлены ниже. Для добавления элемента необходимо нажать на соответствующую гиперссылку под названием категории. " +
                "Каждый элемент портфолио должен содержать краткую информацию о Вашем достижении и его подтверждение в виде ссылки, " +
                "на внешний ресурс, или файла (изображение или pdf-документ, размером не более 3 МБ)."
            };

            if (!db.Sections.Any())
            {
                db.Sections.AddOrUpdate(
                  p => p.Name,
                    s1, s2, s3, s4, s5
                );
                db.SaveChanges();
            }



            var c1 = new Catigory
            {
                Name = "Статьи",
                IndexSort = 1,
                Section = s2
            };
            var c2 = new Catigory
            {
                Name = "Конкурсы",
                IndexSort = 2,
                Section = s2
            };
            var c3 = new Catigory
            {
                Name = "Выставки",
                IndexSort = 5,
                Section = s2
            };
            var c4 = new Catigory
            {
                Name = "Конференции",
                IndexSort = 3,
                Section = s2
            };
            var c5 = new Catigory
            {
                Name = "Олимпиады",
                IndexSort = 4,
                Section = s2
            };
            var c6 = new Catigory
            {
                Name = "Заявки на грантовую работу",
                IndexSort = 6,
                Section = s2
            };
            var c7 = new Catigory
            {
                Name = "Сведения о выполнении грантов",
                IndexSort = 7,
                Section = s2
            };
            var c8 = new Catigory
            {
                Name = "Заявки на РИД",
                IndexSort = 8,
                Section = s2
            };
            var c9 = new Catigory
            {
                Name = "РИД",
                IndexSort = 9,
                Section = s2
            };
            var c10 = new Catigory
            {
                Name = "Свидетельства о регистрации программы для ЭВМ",
                IndexSort = 10,
                Section = s2
            };
            var c11 = new Catigory
            {
                Name = "Экспонаты",
                IndexSort = 11,
                Section = s2
            };
            var c12 = new Catigory
            {
                Name = "Лицензионные договоры на приобретение объектов интеллектуальной собственности",
                IndexSort = 12,
                Section = s2
            };
            var c13 = new Catigory
            {
                Name = "Иное",
                IndexSort = 13,
                Section = s2
            };


            var c14 = new Catigory
            {
                Name = "Участие в общественной работе факультета, университета",
                IndexSort = 1,
                Section = s3
            };
            var c15 = new Catigory
            {
                Name = "Волонтерская работа",
                IndexSort = 2,
                Section = s3
            };
            var c16 = new Catigory
            {
                Name = "Работа в органах студенческого самоуправления",
                IndexSort = 3,
                Section = s3
            };
            var c17 = new Catigory
            {
                Name = "Участие в работе профсоюзной организации",
                IndexSort = 4,
                Section = s3
            };
            var c18 = new Catigory
            {
                Name = "Работа в общежитии",
                IndexSort = 5,
                Section = s3
            };
            var c19 = new Catigory
            {
                Name = "Участие в работе молодежных организаций города и области",
                IndexSort = 6,
                Section = s3
            };
            var c20 = new Catigory
            {
                Name = "Авторство статей в СМИ",
                IndexSort = 7,
                Section = s3
            };
            var c21 = new Catigory
            {
                Name = "Участие в военно-патриотических мероприятиях",
                IndexSort = 8,
                Section = s3
            };
            var c22 = new Catigory
            {
                Name = "Конкурс общественной направленности",
                IndexSort = 9,
                Section = s3
            };


            var c23 = new Catigory
            {
                Name = "Соревнования",
                IndexSort = 1,
                Section = s4
            };
            var c24 = new Catigory
            {
                Name = "Секции",
                IndexSort = 3,
                Section = s4
            };
            var c25 = new Catigory
            {
                Name = "Спортивная квалификация",
                IndexSort = 5,
                Section = s4
            };
            var c26 = new Catigory
            {
                Name = "Участие в соревнованиях",
                IndexSort = 2,
                Section = s4
            };
            var c27 = new Catigory
            {
                Name = "Занятия в спортивных секциях",
                IndexSort = 4,
                Section = s4
            };
            var c28 = new Catigory
            {
                Name = "Публикации о спортивных достижениях в СМИ",
                IndexSort = 6,
                Section = s4
            };


            var c29 = new Catigory
            {
                Name = "Творческие конкурсы",
                IndexSort = 1,
                Section = s5
            };
            var c30 = new Catigory
            {
                Name = "Занятия в коллективах художественной самодеятельности",
                IndexSort = 2,
                Section = s5
            };

            if (!db.Catigories.Any())
            {
                db.Catigories.AddOrUpdate(
                  p => p.Name,
                    c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15, c16, c17, c18, c19, c20, c21, c22, c23, c24, c25, c26, c27, c28, c29, c30
                );
                db.SaveChanges();
            }


            //var userAdmin = db.Users.FirstOrDefault(a => a.UserName == "admin");
            //var userStudent = db.Users.FirstOrDefault(a => a.UserName == "student");



            //добавление параметров
            var sett1 = new Settings
            {
                Alias = "Текущий учебный год",
                Name = "CurrentYear",
                Value = "2018-2019"
            };

            if (!db.Settings.Any())
            {
                db.Settings.AddOrUpdate(
                  p => p.Name,
                    sett1
                );
                db.SaveChanges();
            }

            //формы обучения
            var form1 = new FormOfTraining
            {
                Name = "Очная"
            };

            var form2 = new FormOfTraining
            {
                Name = "Заочная"
            };

            var form3 = new FormOfTraining
            {
                Name = "Очно-заочная"
            };

            var form4 = new FormOfTraining
            {
                Name = "Дистанционная"
            };

            if (!db.FormOfTrainings.Any())
            {
                db.FormOfTrainings.AddOrUpdate(
                a => a.Name, form1, form2, form3, form4
                );
                db.SaveChanges();
            }

            //кафедры
            var d1 = new Department
            {
                Name = "Кафедра биологии",
                Boss = "Курамшина Зиля Мухтаровна",
                Faculty = f3,
                DecanatID = 29
            };

            var d2 = new Department
            {
                Name = "Кафедра экономики и управления",
                Faculty = f1,
                Boss = "Опарина Татьяна Александровна",
                DecanatID = 31
            };

            if (!db.Departments.Any())
            {
                db.Departments.AddOrUpdate(
                a => a.Name, d2, d1
                );
                db.SaveChanges();
            }

            //направление
            var dt1 = new DirectionOfTraining
            {
                Name = "Биология"
            };

            var dt2 = new DirectionOfTraining
            {
                Name = "Экономика"
            };

            if (!db.DirectionOfTrainings.Any())
            {
                db.DirectionOfTrainings.AddOrUpdate(
                a => a.Name, dt1, dt2
                );
                db.SaveChanges();
            }

            //направленность
            var p1 = new Profile
            {
                Name = "Общая биология",
                DirectionOfTraining = dt1,
                Faculty = f1,
                Department = d1,
                Acceptance = true,
                DecanatID = 256
            };

            var p2 = new Profile
            {
                Name = "Финансы и кредит",
                Faculty = f8,
                DirectionOfTraining = dt2,
                Department = d2,
                Acceptance = true,
                DecanatID = 424
            };

            if (!db.Profiles.Any())
            {
                db.Profiles.AddOrUpdate(
                a => a.Name, p1, p2
                );
                db.SaveChanges();
            }

            //группы
            var g1 = new Group
            {
                Name = "ЕНФ-БИО21",
                DecanatID = 15773,
                Profile = p1,
                Faculty = f1,
                Course = 2,
                YearOfReceipt = "2017",
                AcademicYear = "2018-2019",
                Period = 4,
                FormOfTraining = form1
            };

            var g2 = new Group
            {
                Name = "ЭФ-ФК31",
                DecanatID = 15889,
                Faculty = f8,
                Profile = p2,
                Course = 3,
                YearOfReceipt = "2016",
                AcademicYear = "2018-2019",
                Period = 4,
                FormOfTraining = form1
            };

            if (!db.Groups.Any())
            {
                db.Groups.AddOrUpdate(
                a => a.Name, g1, g2
                );
                db.SaveChanges();
            }

            // создаем пользователей
            var admin = new ApplicationUser { Email = "sspa.sdo@gmail.com", UserName = "mih", Lastname = "Администратор", Firstname = "Михаил", EmailConfirmed = true, Faculty = f1 };
            string password = "LOR1s2pm#";
            var result = userManager.Create(admin, password);

            var teacher = new ApplicationUser { Email = "mih0505@gmail.com", UserName = "mih0505", Lastname = "Преподаватель", Firstname = "Михаил", EmailConfirmed = true, Faculty = f1 };
            string passwordUser = "LOR1s2pm#";
            var resultUser = userManager.Create(teacher, passwordUser);

            var student = new ApplicationUser { Email = "bakses@gmail.com", UserName = "student", Lastname = "Студент", Firstname = "Михаил", StatusId = 1, EmailConfirmed = true, Faculty = f2, Group = g1 };
            string passwordStudent = "LOR1s2pm#";
            var resultStudent = userManager.Create(student, passwordStudent);

            // если создание пользователя прошло успешно
            if (result.Succeeded)
            {
                // добавляем для пользователя роль
                userManager.AddToRole(admin.Id, "Administrators");
            }

            if (resultUser.Succeeded)
            {
                // добавляем для пользователя роль
                userManager.AddToRole(teacher.Id, "Teachers");
                //userManager.AddToRole(teacher.Id, "Tutors");
            }

            if (resultStudent.Succeeded)
            {
                // добавляем для пользователя роль
                userManager.AddToRole(student.Id, "Students");
            }

            var a1 = new Artifact
            {
                Catigory = c4,
                Name = "Конференция Величайшие проблемы науки",
                DateBegin = DateTime.Now.AddMonths(-2),
                DateAdd = DateTime.Now,
                User = student,
                Authors = "Я и тот парень",
                BookTitle = "Сборник статей по проблемам науки",
                AdditionalInformation = "Мега крутая конференция по проблемам науки",
                Organization = "МГУ",
                Location = "Москва"
            };
            var a2 = new Artifact
            {
                Catigory = c4,
                Name = "Конференция ДОТ в ВУЗе",
                DateBegin = DateTime.Now.AddMonths(-2),
                DateAdd = DateTime.Now,
                User = student,
                Authors = "Я",
                BookTitle = "Сборник статей",
                AdditionalInformation = "Еще одна Мега крутая конференция по использованию ДОТ в ВУЗе",
                Organization = "МГУ",
                Location = "Москва"
            };
            var a3 = new Artifact
            {
                Catigory = c4,
                Name = "Конференция ДОТ в ВУЗе ",
                DateBegin = DateTime.Now.AddMonths(-2),
                DateAdd = DateTime.Now,
                User = student,
                Authors = "Кто-то",
                BookTitle = "Сборник",
                AdditionalInformation = "Еще одна Мега крутая конференция по использованию ДОТ в ВУЗе",
                Organization = "СПбГУ",
                Location = "С.-Петербург"
            };

            if (!db.Artifacts.Any())
            {
                db.Artifacts.AddOrUpdate(
                  p => p.Name,
                    a1, a2, a3
                );
                db.SaveChanges();
            }


            #endregion
        }
    }

    // Настройка диспетчера входа для приложения.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
