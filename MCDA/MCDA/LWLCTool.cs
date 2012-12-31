using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Data;
using System.Collections;
using ESRI.ArcGIS.ADF;

namespace MCDA.Model
{
   public sealed class LWLCTool : AbstractToolTemplate
    {
       private string _defaultResultColumnName = "LWLC Result";
       private ToolParameterContainer _toolParameterContainer;
       private TransformationStrategy _tranformationStrategy;
       private IFeatureClass _featureClass;
       private DataTable _dataTable, _resultDataTable;
       private IDictionary<int, List<Tuple<int, double>>> _dictionaryOfDistances;

       public LWLCTool(DataTable dataTable, ToolParameterContainer toolParameterContainer, IFeatureClass featureClass)
       {
           _dataTable = dataTable;
           _toolParameterContainer = toolParameterContainer;
           _featureClass = featureClass;
       }

        protected override void PerformAlgorithm()
        {
           if(_featureClass == null)
                return;

           if(_dictionaryOfDistances == null)
               _dictionaryOfDistances = BuildDictioninaryOfDistances();

            _resultDataTable = new DataTable();

            _resultDataTable.BeginLoadData();

            _resultDataTable.Columns.Add(_featureClass.OIDFieldName,typeof(FieldTypeOID));

            foreach (IToolParameter _currentToolParameter in _toolParameterContainer.ToolParameter)
            {
                _resultDataTable.Columns.Add("local range " +_currentToolParameter.ColumnName, typeof(double));
                _resultDataTable.Columns.Add("scaled value " + _currentToolParameter.ColumnName, typeof(double));
                _resultDataTable.Columns.Add("weight " + _currentToolParameter.ColumnName, typeof(double));
            }

            _resultDataTable.Columns.Add(_defaultResultColumnName,typeof(double));

            foreach (int currentID in _dictionaryOfDistances.Keys)
            {
                List<Tuple<int,double>> list = new List<Tuple<int,double>>();

                _dictionaryOfDistances.TryGetValue(currentID, out list);
                //todo make sure that n > count
                Cluster c = new Cluster(currentID, list.OrderBy(t => t.Item2).Take(3).Select(t => t.Item1).ToList(), _dataTable, _toolParameterContainer);
                
                _resultDataTable.Rows.Add((c.CalculateLWCL(_resultDataTable.NewRow())));
            }

            _resultDataTable.EndLoadData();
        }

        private IDictionary<int, List<Tuple<int, double>>> BuildDictioninaryOfDistances()
        {
            double[,] centroidArray = new double[_featureClass.FeatureCount(null), 3];

            using (ComReleaser comReleaser = new ComReleaser())
            {
                IFeatureCursor pCursor = (IFeatureCursor)_featureClass.Search(null, false);
                IRow pRow = (IRow)pCursor.NextFeature();
                //IFeatureSelection pFSel = (IFeatureSelection)_featureClass;

                int numberOfFeatures = _featureClass.FeatureCount(null);

                int oidColumn = _featureClass.FindField(_featureClass.OIDFieldName);

                for (int p = 0; p < numberOfFeatures; p++)
                {
                    centroidArray[p, 0] = Convert.ToInt32(_featureClass.GetFeature(p).get_Value(oidColumn));

                    IArea area = (IArea)_featureClass.GetFeature(p).Shape;
                    centroidArray[p, 1] = area.Centroid.X;
                    centroidArray[p, 2] = area.Centroid.Y;
                }
            }

            IDictionary<int, List<Tuple<int, double>>> dictionaryOfDistances = new Dictionary<int, List<Tuple<int, double>>>();

            //Populate the datatable with distances between each polygon pair
            for (int i = 0; i < centroidArray.GetLength(0); i++)
            {
                List<Tuple<int, double>> m = new List<Tuple<int, double>>();

                for (int j = 0; j < centroidArray.GetLength(0); j++)
                {
                    //don t add the distance to iitself
                    if (i == j)
                        continue;
                    //create a distance matrix for each polygon and store in the data table
                    double distance = EuclidianDistance(centroidArray[i, 1], centroidArray[i, 2], centroidArray[j, 1], centroidArray[j, 2]);
                    int id = Convert.ToInt32(centroidArray[j, 0]); //id

                    Tuple<int, double> temp = new Tuple<int, double>(id, distance);

                    m.Add(temp);
                }

                dictionaryOfDistances.Add(Convert.ToInt32(centroidArray[i, 0]), m);

            }

            return dictionaryOfDistances;
        }

        private double EuclidianDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2d) + Math.Pow(y1 - y2, 2d));
        }

        protected override void PerformScaling()
        {
            //todo
        }

        public override string DefaultResultColumnName
        {
            get { return _defaultResultColumnName; }
            set { _defaultResultColumnName = value; }
        }

        public override System.Data.DataTable Data
        {
            get { return _resultDataTable; }
        }

        public override ToolParameterContainer ToolParameterContainer
        {
            get{ return _toolParameterContainer; }
            set {  _toolParameterContainer = value; }
        }

        public override TransformationStrategy TransformationStrategy
        {
            get { return _tranformationStrategy; }
            set {  _tranformationStrategy = value; }
        }
    }
}
