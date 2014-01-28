using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using MCDA.Extensions;

namespace MCDA.Model
{
    internal static class Util
    {
        public static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            object o = expression.Body as MemberExpression;

            return (expression.Body as MemberExpression).Member.Name;
        }

        public static string GetPropertyValue<T>(Expression<Func<T>> expression)
        {
            return GetValue(expression.Body as MemberExpression).ToString();
        }

        private static object GetValue(MemberExpression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }
    }
}
