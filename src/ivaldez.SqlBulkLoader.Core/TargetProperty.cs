using System;
using System.Reflection;

namespace ivaldez.Sql.SqlBulkLoader.Core
{
    public class TargetProperty
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public string OriginalName { get; set; }
    }
}