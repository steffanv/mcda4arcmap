using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCDA.Extensions
{
    public static class ExtensionMethods
    {
        public static bool IsNumeric(this DataColumn column)
        {
            if (column == null)
                return false;

            var numericTypes = new[] { typeof(Byte), typeof(Decimal), typeof(Double), typeof(Int16), typeof(Int32), typeof(Int64), typeof(SByte),
                                typeof(Single), typeof(UInt16), typeof(UInt32), typeof(UInt64)};

            return numericTypes.Contains(column.DataType);
        }

        public static PropertyChangedEventHandler RegisterPropertyHandler<T, TProperty>(this T obj, Expression<Func<T, TProperty>> propertyExpression, PropertyChangedEventHandler handlerDelegate)
           where T : INotifyPropertyChanged
        {
            if (obj == null) throw new ArgumentNullException("obj");

            var propertyName = GetPropertyName(propertyExpression);

            PropertyChangedEventHandler handler = (sender, args) =>
            {
                if (args.PropertyName.Equals(propertyName) && handlerDelegate != null)
                    handlerDelegate(sender, args);
            };

            obj.PropertyChanged += handler;

            return handler;
        }
        public static void UnRegisterPropertyHandler<T, TProperty>(this T obj, Expression<Func<T, TProperty>> propertyExpression, PropertyChangedEventHandler handlerDelegate)
            where T : INotifyPropertyChanged
        {
            if (obj == null) throw new ArgumentNullException("obj");

            var propertyName = GetPropertyName(propertyExpression);

            obj.PropertyChanged -= (sender, args) =>
            {
                if (args.PropertyName.Equals(propertyName) && handlerDelegate != null)
                    handlerDelegate(sender, args);
            };
        }

        public static void UnRegisterPropertyHandler<T>(this T obj,  PropertyChangedEventHandler handlerDelegate)
           where T : INotifyPropertyChanged
        {
            if (obj == null) throw new ArgumentNullException("obj");

            obj.PropertyChanged -= handlerDelegate;
        
        }

        private static string GetPropertyName(LambdaExpression propertyExpression)
        {
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
            {
                var unaryExpression = propertyExpression.Body as UnaryExpression;
                if (unaryExpression == null)
                    throw new ArgumentException("Expression must be a UnaryExpression.", "propertyExpression");

                memberExpression = unaryExpression.Operand as MemberExpression;
            }

            if (memberExpression == null)
                throw new ArgumentException("Expression must be a MemberExpression.", "propertyExpression");

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("Expression must be a Property.", "propertyExpression");

            return propertyInfo.Name;
        }

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
