using System;
using System.Linq.Expressions;

namespace ivaldez.Sql.SqlBulkLoader.Core
{
    public class ExpressionHelper
    { 
        public static string GetName<T>(Expression<Func<T, object>> expression)
        {
            var body = expression.Body as MemberExpression;

            if (body == null)
            {
                var ubody = (UnaryExpression)expression.Body;
                body = ubody.Operand as MemberExpression;
            }

            var name = body.Member.Name;

            return name;
        }
    }
}
