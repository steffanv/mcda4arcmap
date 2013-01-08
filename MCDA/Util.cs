using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using MCDA.Extensions;

namespace MCDA.Model
{
    public sealed class Util
    {
        private Util() { }

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

        //seriously ESRI? I need the ArcGIS Military Analyst? 

        /////<summary>Get the geodetically correct Rhumb Line distance between two points.</summary>
        ///// 
        /////<param name="fromPoint">An IPoint interface that is the start (or from) location</param>
        /////<param name="toPoint">An IPoint interface that is the end (or to) location</param>
        /////<param name="spatialReference">An esriSRGeoCSType enum that is a predefined geographic coordinate system. Example: ESRI.ArcGIS.Geometry.esriSRGeoCSType.esriSRGeoCS_NAD1983</param>
        /////  
        /////<returns>A System.Double representing true distance</returns>
        /////  
        /////<remarks></remarks>
        //public static System.Double GetDistanceFromTwoPoints(ESRI.ArcGIS.Geometry.IPoint fromPoint, ESRI.ArcGIS.Geometry.IPoint toPoint, ESRI.ArcGIS.Geometry.esriSRGeoCSType spatialReference)
        //{
        //    // Define the spatial reference of the rhumb line. 
        //    ESRI.ArcGIS.Geometry.ISpatialReferenceFactory2 spatialReferenceFactory2 = new ESRI.ArcGIS.Geometry.SpatialReferenceEnvironmentClass();
        //    ESRI.ArcGIS.Geometry.ISpatialReference2 spatialReference2 = (ESRI.ArcGIS.Geometry.ISpatialReference2)spatialReferenceFactory2.CreateSpatialReference((System.Int16)spatialReference);

        //    // Initialize the MeasurementTool and define the properties of the line.
        //    // These properties include the line type, which is a rhumb line in this case, and the 
        //    // spatial reference of the line.   
        //    ESRI.ArcGIS.DefenseSolutions.IMeasurementTool measurementTool = new ESRI.ArcGIS.DefenseSolutions.MeasurementToolClass();
        //    measurementTool.SpecialGeolineType = ESRI.ArcGIS.DefenseSolutions.cjmtkSGType.cjmtkSGTRhumbLine;
        //    measurementTool.SpecialSpatialReference = spatialReference2;

        //    // Determine the distance and azimuth of the rhumb line based on the start and end point coordinates.   
        //    measurementTool.ConstructByPoints(fromPoint, toPoint);

        //    // Return the Distance. 
        //    return measurementTool.Distance;
        //}

    }
}
