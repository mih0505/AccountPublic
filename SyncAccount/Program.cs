using SyncAccount.Sync;
using System;
using System.Threading.Tasks;

namespace SyncAccount
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== ЗАПУСК СИНХРОНИЗАЦИИ ДАННЫХ С ДЕКАНАТОМ ===");

            //1. синхронизация факультетов
            var syncFaculties = new SyncFaculty();
            syncFaculties.MessageHandler(new SyncFaculty.ProgressMessage(ShowMessage));
            await syncFaculties.SyncFacultiesAsync();
                       

            //2. синхронизация кафедр
            var syncDepartments = new SyncDepartment();
            syncDepartments.MessageHandler(new SyncDepartment.ProgressMessage(ShowMessage));
            await syncDepartments.SyncDepartmentsAsync();


            //3. синхронизация программ
            var syncProfiles = new SyncProfile();
            syncProfiles.MessageHandler(new SyncProfile.ProgressMessage(ShowMessage));
            await syncProfiles.SyncProfilesAsync();


            //4. синхронизация групп
            var syncGroups = new SyncGroup();
            syncGroups.MessageHandler(new SyncGroup.ProgressMessage(ShowMessage));
            await syncGroups.SyncGroupsAsync();


            //5. синхронизация студентов
            var syncStudents = new SyncStudent();
            syncStudents.MessageHandler(new SyncStudent.ProgressMessage(ShowMessage));
            await syncStudents.SyncStudentsAsync();
            
            
            Console.WriteLine("=== СИНХРОНИЗАЦИЯ ДАННЫХ С ДЕКАНАТОМ ЗАВЕРШЕНА ===");
            Console.ReadKey();
        }

        static void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
