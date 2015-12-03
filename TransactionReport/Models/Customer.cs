using System;

namespace TransactionReport.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMail { get; set; }
        public bool IsAffiliate { get; set; }
        public Nullable<int> SalesManagerId { get; set; }
        public string SalesName { get; set; }
        public string SalesEmail { get; set; }
        public int region { get; set; }
        public string shortName { get; set; }
        public bool IsAds { get; set; }
        public bool ShowEmailInReport { get; set; }
    }
}
