using System;

namespace TransactionReport.Models
{
    public class TemporaryTransactions
    {
        public string VeApp { get; set; }
        public System.DateTime TransactionDate { get; set; }
        public string Site { get; set; }
        public string OrderNumber { get; set; }
        public string PromoCode { get; set; }
        public string Type { get; set; }
        public string Email { get; set; }
        public Nullable<double> Total { get; set; }
        public string CurrencySymbol { get; set; }
        public Nullable<bool> HasDecimal { get; set; }
        public Nullable<System.DateTime> CheckOutDate { get; set; }
        public string CriteriaGroup { get; set; }
        public string ExtraField1 { get; set; }
        public string ExtraField2 { get; set; }
        public string ExtraField3 { get; set; }
    }
}
