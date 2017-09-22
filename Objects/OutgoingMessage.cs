using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDK.Objects.Messages
{
    public class Message
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Id { get; set; }
        public string AccountNumber { get; set; }
        public string Account { get; set; }
        public DateTime DateTime { get; set; }
        public int Int { get; set; }
        public string String { get; set; }
    }
}
