using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ComponentModel;

namespace MCDA.Extensions
{
    /// <summary>
    /// From http://www.wpftutorial.net/INotifyPropertyChanged.html and http://stackoverflow.com/questions/12274514/why-does-my-property-returns-not-the-value-i-set
    /// </summary>
    internal static class NotifyPropertyChangeExtension
    {
        public static bool ChangeAndNotify<T>(this PropertyChangedEventHandler handler,
        ref T field, T value, Expression<Func<T>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }
            var body = memberExpression.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("Lambda must return a property.");
            }
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;

            var vmExpression = body.Expression as ConstantExpression;
            if (vmExpression != null)
            {
                LambdaExpression lambda = Expression.Lambda(vmExpression);
                Delegate vmFunc = lambda.Compile();
                object sender = vmFunc.DynamicInvoke();

                if (handler != null)
                {
                    handler(sender, new PropertyChangedEventArgs(body.Member.Name));
                }
            }
   
            return true;
        }

        public static void Notify<T>(this PropertyChangedEventHandler handler, Expression<Func<T>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }
            var body = memberExpression.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("Lambda must return a property.");
            }

            var vmExpression = body.Expression as ConstantExpression;
            if (vmExpression != null)
            {
                LambdaExpression lambda = Expression.Lambda(vmExpression);
                Delegate vmFunc = lambda.Compile();
                object sender = vmFunc.DynamicInvoke();

                if (handler != null)
                {
                    handler(sender, new PropertyChangedEventArgs(body.Member.Name));
                }
            }

        }

        public static void Notify(this PropertyChangedEventHandler handler, object sender, string propertyName)
        {
            if(sender == null){

                throw new ArgumentNullException("sender");
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }
           
                if (handler != null)
                {
                    handler(sender, new PropertyChangedEventArgs(propertyName));
                }
            }

        }
    
}
