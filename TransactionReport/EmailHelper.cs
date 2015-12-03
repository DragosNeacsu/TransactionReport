using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace TransactionReport
{
    class EmailHelper
    {
        public static void SendMail(AccountManager accountManager, string filePath)
        {
            Library.WriteErrorLog("Start sending email to " + accountManager.Email);
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            MailMessage message = new MailMessage();
            SmtpClient smtpClient = new SmtpClient(appSettings["smtpServer"]);
            smtpClient.Port = int.Parse(appSettings["smtpPort"]);
            smtpClient.Credentials = (ICredentialsByHost)new NetworkCredential(appSettings["smtpUsername"], appSettings["smtpPassword"]);
            smtpClient.EnableSsl = Convert.ToBoolean(appSettings["smtpSsl"]);
            message.From = new MailAddress(appSettings["FromEmail"], appSettings["FromName"], Encoding.UTF8);
            if (!bool.Parse(ConfigurationManager.AppSettings["SendEmailToAMs"]))
                accountManager.Email = appSettings["ToEmail"];
            message.To.Add(accountManager.Email);
            if (!String.IsNullOrEmpty(appSettings["CCEmail"]))
                message.CC.Add(appSettings["CCEmail"]);
            message.Subject = string.Format("Transactions for - {0}", DateTime.Now.AddMonths(-1).ToString("MMMM"));
            message.IsBodyHtml = true;
            message.Body = "Account Manager email is: " + accountManager.Email + " account manager name is: " + accountManager.Name;
            Attachment attachment = new Attachment(filePath);
            message.Attachments.Add(attachment);
            smtpClient.Send(message);
        }
    }
}
