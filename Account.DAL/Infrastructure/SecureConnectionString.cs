namespace Account.DAL.Infrastructure
{
    public class SecureConnectionString
    {                
        private static SecureConnectionString secureConnectionString { get; set; }

        public string Default { get; set; }
        public string Decanat { get; set; }

        public static SecureConnectionString GetSecureConnectionString()
        {
            if (secureConnectionString is null)
            {
                secureConnectionString = new SecureConnectionString();
            }

            return secureConnectionString;
        }

        public SecureConnectionString()
        {
            //Default = "server=*;database=Accounts;user=*;password=*;MultipleActiveResultSets=True";
            //Decanat = "server=*;database=Деканат;user=*;password=*;MultipleActiveResultSets=True";
            Default = "server=*;database=Accounts;user=*;password=*;";
            Decanat = "server=*;database=Деканат;user=*;password=*;";


        }
    }
}
