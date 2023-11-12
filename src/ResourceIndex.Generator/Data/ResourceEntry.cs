using System;
using System.Collections.Generic;
using System.Text;

namespace Resource.Index.Data
{
    internal class ResourceEntry
    {
        public string MemberName { get; }
        public string LogicalName { get; }
        
        public ResourceEntry(string memberName, string logicalName)
        {
            MemberName = memberName;
            LogicalName = logicalName;
        }
    }
}
