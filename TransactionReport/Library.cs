using DocumentFormat.OpenXml.Spreadsheet;
using Ionic.Zip;
using SpreadsheetLight;
//using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using TransactionReport.Models;

namespace TransactionReport
{
    public class Library
    {
        public static string logFile = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["logFile"]);
        public static string folderPath = string.Format("{0}ExportedFiles", AppDomain.CurrentDomain.BaseDirectory);
        public static string templateFile = string.Format("{0}Helpers\\Template.xlsx", AppDomain.CurrentDomain.BaseDirectory);

        public static string GenerateExcel(List<TemporaryTransactions> transactions, Customer customer)
        {
            Library.WriteErrorLog(string.Format("Generating excel file for - {0}", customer.CustomerName));
            int row = transactions.Count();
            int columns = 14;

            string excelFile = CopyFile(customer);

            SLDocument workbook = new SLDocument(Path.GetFullPath(excelFile), "Sheet1");

            for (int index = 0; index < row; ++index)
            {
                workbook.SetCellValue(index + 10, 1, transactions[index].VeApp);
                workbook.SetCellValue(index + 10, 2, transactions[index].TransactionDate.ToString("dd/MM/yyyy"));
                workbook.SetCellValue(index + 10, 3, transactions[index].Site);
                workbook.SetCellValue(index + 10, 4, IsNull(transactions[index].OrderNumber));
                workbook.SetCellValue(index + 10, 5, transactions[index].PromoCode);
                workbook.SetCellValue(index + 10, 6, transactions[index].Type);
                if (customer.ShowEmailInReport)
                    workbook.SetCellValue(index + 10, 7, transactions[index].Email);
                workbook.SetCellValue(index + 10, 8, transactions[index].CurrencySymbol + " " + string.Format("{0:0.00}", transactions[index].Total));
                workbook.SetCellValue(index + 10, 9, string.Empty);
                workbook.SetCellValue(index + 10, 10, transactions[index].CheckOutDate.ToString());
                workbook.SetCellValue(index + 10, 11, transactions[index].CriteriaGroup);
                workbook.SetCellValue(index + 10, 12, transactions[index].ExtraField1);
                workbook.SetCellValue(index + 10, 13, transactions[index].ExtraField2);
                workbook.SetCellValue(index + 10, 14, transactions[index].ExtraField3);
            }

            if (!customer.ShowEmailInReport)
            {
                workbook.DeleteColumn(7, 1);
                columns = 13;
            }

            SetStyle(workbook, columns, row);
            workbook.Save();
            return excelFile;
        }
        public static string IsNull(string text)
        {
            if (String.IsNullOrEmpty(text))
                return "N/A";
            else
                return text;
        }
        public static void SetStyle(SLDocument workbook, int columns, int bottom)
        {
            workbook.AutoFitColumn(1, columns);
            for (int i = 0; i < columns; i++)
            {
                int columnHeaderIndex = i + 1;
                var width = workbook.GetColumnWidth(columnHeaderIndex);
                if (width < 18)
                    workbook.SetColumnWidth(columnHeaderIndex, 18);
            }
            bottom = bottom + 9;


            SLStyle valueStyle = workbook.CreateStyle(); // HINT: We need to make a new style. We can not do styleValue = style.
            valueStyle.Font.FontName = "Arial";
            valueStyle.Alignment.Horizontal = HorizontalAlignmentValues.Center;
            valueStyle.Alignment.Vertical = VerticalAlignmentValues.Center;
            valueStyle.Font.FontSize = 10;
            valueStyle.Font.FontColor = System.Drawing.Color.DimGray;
            valueStyle.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.White, System.Drawing.Color.GhostWhite);
            valueStyle.Border.LeftBorder.BorderStyle = BorderStyleValues.Thin;
            valueStyle.Border.LeftBorder.Color = System.Drawing.Color.Gray;
            valueStyle.Border.RightBorder.BorderStyle = BorderStyleValues.Thin;
            valueStyle.Border.RightBorder.Color = System.Drawing.Color.Gray;
            valueStyle.Border.TopBorder.BorderStyle = BorderStyleValues.Thin;
            valueStyle.Border.TopBorder.Color = System.Drawing.Color.Gray;
            valueStyle.Border.BottomBorder.BorderStyle = BorderStyleValues.Thin;
            valueStyle.Border.BottomBorder.Color = System.Drawing.Color.Gray;
            workbook.SetCellStyle(10, 1, bottom, columns, valueStyle); // 0 = Values

        }

        #region Logger
        public static void WriteErrorLog(Exception ex)
        {
            try
            {
                StreamWriter streamWriter = new StreamWriter(logFile, true);
                streamWriter.WriteLine(string.Format("{0}: {1}; {2}", DateTime.Now.ToString(), ex.Source.ToString().Trim(), ex.Message.ToString().Trim()));
                streamWriter.Flush();
                streamWriter.Close();
            }
            catch
            {
            }
        }

        public static void WriteErrorLog(string message)
        {
            try
            {
                StreamWriter streamWriter = new StreamWriter(logFile, true);
                streamWriter.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToString(), message));
                streamWriter.Flush();
                streamWriter.Close();
            }
            catch
            {
            }
        }
        #endregion
        #region Txt Reader
        public static string ReadFile(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }
        #endregion

        public static List<T> DataReaderMapToList<T>(IDataReader dr)
        {
            List<T> list = new List<T>();
            T obj = default(T);
            while (dr.Read())
            {
                obj = Activator.CreateInstance<T>();
                foreach (PropertyInfo prop in obj.GetType().GetProperties())
                {
                    if (!object.Equals(dr[prop.Name], DBNull.Value))
                    {
                        prop.SetValue(obj, dr[prop.Name], null);
                    }
                }
                list.Add(obj);
            }
            return list;
        }


        public static void CleanUpFolder()
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            DirectoryInfo directory = new DirectoryInfo(folderPath);
            foreach (DirectoryInfo dir in directory.GetDirectories()) dir.Delete(true);

            foreach (FileInfo file in directory.GetFiles()) file.Delete();
        }

        public static string CopyFile(Customer customer)
        {
            string targetFolder = Path.Combine(folderPath, customer.SalesName.Trim());
            string destFile = Path.Combine(targetFolder, string.Format("{0}.xlsx", customer.CustomerName));

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }
            File.Copy(templateFile, destFile, true);

            return destFile;
        }

        public static string ZipFolders()
        {
            string[] directories = Directory.GetDirectories(folderPath);
            foreach (string directory in directories)
            {
                using (ZipFile zip = new ZipFile())
                {
                    zip.AddDirectory(Path.Combine(folderPath, directory));
                    zip.Save(Path.Combine(folderPath, string.Format("{0}.zip", directory)));
                }
            }
            return folderPath;
        }
    }
}
