using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDK.Objects.IncomingMessages
{

    public class Rootobject
    {
        public Messages[] Property1 { get; set; }
    }

    public class Messages
    {
        public Message[] Message { get; set; }
    }

    public class Message
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public string AccountNumber { get; set; }
        public string DateTime { get; set; }
        public string Email { get; set; }
        public string Int { get; set; }
        public string String { get; set; }
    }

}
