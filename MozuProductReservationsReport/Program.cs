using System;

namespace MozuProductReservationsReport
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // stop the logging spam
                global::Mozu.Api.Logging.LogManager.LoggingService = new MozuQuietLogger();

                int tenantId = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["tenantId"]);
                int siteId = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["siteId"]);
                string mozuApplicationId = System.Configuration.ConfigurationManager.AppSettings["mozuApplicationId"];
                string mozuApplicationSecret = System.Configuration.ConfigurationManager.AppSettings["mozuApplicationSecret"];

                Console.WriteLine("tenantId = {1}{0}siteId = {2}{0}mozuApplicationId = {3}{0}mozuApplicationSecret = {4}{0}{0}", Environment.NewLine, tenantId, siteId, mozuApplicationId ?? "[null]", mozuApplicationSecret ?? "[null]");
                Console.WriteLine("Check the appSettings in the application config file to change the settings above.  Press enter to continue.");
                Console.ReadLine();

                MozuContext.AuthApp(mozuApplicationId: mozuApplicationId, mozuApplicationSecret: mozuApplicationSecret);
                MozuContext mozuContext = MozuContext.GetMozuContext(tenantId, siteId);

                string reportFilename = String.Format("ProductReservationsReport_t{0}_{1}.csv", tenantId, DateTime.UtcNow.ToString("yyyyMMddHHmm"));

                Console.WriteLine("Generating report, please wait.");
                new ReservationsReport().GenerateReport(mozuContext, reportFilename);
                Console.WriteLine("Wrote report to file {0}", reportFilename);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.WriteLine("{0}Finished.  Press enter to exit.", Environment.NewLine);
                Console.ReadLine();
            }
        }
    }
}
