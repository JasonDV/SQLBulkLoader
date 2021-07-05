using System.Collections.Generic;
using System.Linq;

namespace ivaldez.Sql.SqlBulkLoader.Core
{
    public class ReflectionHelper
    {
        public static TargetProperty[] GetTargetProperties<T>(List<string> propertiesToIgnore,
            Dictionary<string, string> renameFields)
        {
            var ignoreProperties = new HashSet<string>(propertiesToIgnore);

            var targetProperties = typeof(T)
                .GetProperties()
                .Where(x => ignoreProperties.Contains(x.Name) == false)
                .Select(x =>
                {
                    var fieldName = x.Name;

                    if (renameFields.ContainsKey(fieldName))
                    {
                        fieldName = renameFields[fieldName];
                    }

                    return new TargetProperty
                    {
                        Name = fieldName,
                        OriginalName = x.Name,
                        Type = x.PropertyType,
                        PropertyInfo = x
                    };
                }).ToArray();

            return targetProperties;
        }
    }
}
