using Newtonsoft.Json;
using System;

namespace CDK.Objects.Customers
{
    public class Rootobject
    {
        public Customer[] customers { get; set; }
        public Pagecontext pageContext { get; set; }
    }

    public class Pagecontext
    {
        public DateTime timestamp { get; set; }
        public bool hasMoreEntries { get; set; }
        public int pageSize { get; set; }
    }

    public class Customer
    {
        public string id { get; set; }
        public string name { get; set; }
        public string companyName { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string city { get; set; }
        public string county { get; set; }
        public string country { get; set; }
        public string postcode { get; set; }
        public string phoneNumber { get; set; }
        public string creditLimit { get; set; }
        public string availableCredit { get; set; }
        public string merchantType { get; set; }
        public string accountsCode { get; set; }
        public string vatNumber { get; set; }
        public string companyRegNumber { get; set; }
        public string phoneNumber2 { get; set; }
        public string phoneNumber3 { get; set; }
        public string phoneNumber4 { get; set; }
        public string faxNumber { get; set; }
        public string province { get; set; }
        public string priceLevel { get; set; }
        public string hasAccount { get; set; }
        public string mobileNumber { get; set; }
        public string emailAddress { get; set; }
    }

}
