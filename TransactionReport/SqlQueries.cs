using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using TransactionReport.Models;
namespace TransactionReport
{
    public static class SqlQueries
    {
        public static SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TransactionReport"].ConnectionString);
        public static string getCustomersFilePath= string.Format("{0}Helpers\\Get_Customers.txt", AppDomain.CurrentDomain.BaseDirectory);
        public static string getTransactionsFilePath = string.Format("{0}Helpers\\Get_TemporaryTransactions.txt", AppDomain.CurrentDomain.BaseDirectory);

        public static List<Customer> GetCustomers()
        {
            SqlCommand cmd = new SqlCommand();
            #region GetCustomers
            cmd.CommandText = string.Format(Library.ReadFile(getCustomersFilePath), ConfigurationManager.AppSettings["countries"].ToString());
            #endregion

            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection;

            sqlConnection.Open();
            List<Customer> customers = Library.DataReaderMapToList<Customer>(cmd.ExecuteReader());
            sqlConnection.Close();
            return customers;
        }
        public static List<TemporaryTransactions> GetTransactions(int customerId)
        {
            SqlCommand cmd = new SqlCommand();

            #region GetTransactions
            cmd.CommandText = string.Format(Library.ReadFile(getTransactionsFilePath), customerId);
            #endregion

            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection;

            sqlConnection.Open();
            List<TemporaryTransactions> transactions = Library.DataReaderMapToList<TemporaryTransactions>(cmd.ExecuteReader());
            sqlConnection.Close();
            return transactions;
        }
    }
}
