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
        public string String1 { get; set; }
        public string String2 { get; set; }
        public string String3 { get; set; }
        public string String4 { get; set; }
        public string String5 { get; set; }
        public string String6 { get; set; }
        public string String7 { get; set; }
        public string String8 { get; set; }
        public string String9 { get; set; }
        public string String10 { get; set; }
        public string String11 { get; set; }
        public string String12 { get; set; }
        public string String13 { get; set; }
        public string String14 { get; set; }
        public string String15 { get; set; }
        public string String16 { get; set; }
        public string String17 { get; set; }
        public string String18 { get; set; }
        public string String19 { get; set; }
        public string String20 { get; set; }
    }

}
