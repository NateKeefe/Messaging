using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDK.Objects.EntityInfo
{
    public class EntityInfo
    {
        public virtual List<string> SupportedActions() { return new List<string>(); }

    }
}
