using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace MCDA.Model
{
    public sealed class Util
    {
        private Util() { }

        public static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            return (expression.Body as MemberExpression).Member.Name;
        }
    }
}
