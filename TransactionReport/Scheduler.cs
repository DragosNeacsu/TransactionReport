using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using TransactionReport.Models;

namespace TransactionReport
{
    public partial class Scheduler : ServiceBase
    {
        private Timer Schedular;

        public Scheduler()
        {
            this.InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["EnableScheduler"]))
                ScheduleService();
            else
                RunTransactionReport();
        }

        public void ScheduleService()
        {
            try
            {
                Schedular = new Timer(new TimerCallback(SchedularCallback));

                DateTime scheduledTime = DateTime.Parse(ConfigurationManager.AppSettings["ScheduledTime"]);

                if (DateTime.Now > scheduledTime)
                    scheduledTime = scheduledTime.AddDays(1);   //If Scheduled Time is passed set Schedule for the next day.

                TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);
                Library.WriteErrorLog(string.Format("Service scheduled to run after: {0} day(s) {1} hour(s) {2} minute(s) {3} seconds(s)", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds));

                int dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);  //Get the difference in Minutes between the Scheduled and Current Time.
                Schedular.Change(dueTime, Timeout.Infinite);    //Change the Timer's Due Time.
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog(ex);
            }
        }

        private void SchedularCallback(object e)
        {
            if (DateTime.Now.Day == int.Parse(ConfigurationManager.AppSettings["ScheduledDay"]))
                RunTransactionReport();
            this.ScheduleService();
        }

        private void RunTransactionReport()
        {
            Library.WriteErrorLog(string.Format("Service start runing at: {0}", DateTime.Now.ToString()));
            Library.CleanUpFolder();

            List<Customer> Customers = SqlQueries.GetCustomers();

            List<AccountManager> accountManager = new List<AccountManager>();

            foreach (Customer customer in Customers)
            {
                Library.WriteErrorLog(string.Format("Customer: {0}", customer.CustomerName));

                List<TemporaryTransactions> transactions = SqlQueries.GetTransactions(customer.CustomerId);
                if (customer.IsAds)
                {
                    customer.SalesEmail = ConfigurationManager.AppSettings["VeAdsEmail"];
                    customer.SalesName = "VeAds";
                }

                Library.GenerateExcel(transactions, customer);
                accountManager.Add(new AccountManager() { Email = customer.SalesEmail, Name = customer.SalesName });
            }

            string folderPath = Library.ZipFolders();

            accountManager.GroupBy(m => m.Name).Select(m => m.First()).ToList().ForEach(am =>
                EmailHelper.SendMail(am, Path.Combine(folderPath, string.Format("{0}.zip", am.Name.Trim())))
                );
        }

        protected override void OnStop()
        {
            Library.WriteErrorLog("Windows service stopped");
            if (bool.Parse(ConfigurationManager.AppSettings["EnableScheduler"]))
                this.Schedular.Dispose();
        }
    }
}
