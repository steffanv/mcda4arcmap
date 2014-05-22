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

        /// <summary>
        /// In case the array contains less than 2 values, 0 is returned
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double SmallestDifference(double[] array)
        {
            Array.Sort(array);

            return array.Count() < 2 ? 0 : SD(array);
        }

        private static double SD(double[] array)
        {

            if (array.Count() == 1)
                return double.MaxValue;

            if (array.Count() == 2)
                return Math.Abs(array[0] - array[1]);

            int cut = array.Count()/2;

            var leftCopy = new double[cut];
            Array.Copy(array,leftCopy,cut);
            double left = SD(leftCopy);

            var rightCopy = new double[array.Count() - cut];
            Array.Copy(array,cut,rightCopy,0,array.Count()-cut);
            double right = SD(rightCopy);

            double middle = Math.Abs(array[cut-1] - array[cut]);

            return Math.Min(Math.Min(left, right), middle);
        }
    }
}
