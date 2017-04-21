using System;
using System.Collections.Generic;
using System.Linq;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Data;
using MCDA.ViewModel;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using ESRI.ArcGIS.ADF;
using MCDA.Misc;

namespace MCDA.Model
{
    internal sealed class LWLCTool : AbstractToolTemplate
    {
        private string _defaultResultColumnName = "LWLCResult";
        private ToolParameterContainer _toolParameterContainer;
        private NormalizationStrategy _tranformationStrategy;
        private IFeatureClass _featureClass;
        private readonly DataTable _dataTable;
        private DataTable _resultDataTable;

        private IDictionary<int, List<Tuple<int, float>>> _dictionaryOfDistances;
        private IDictionary<int, List<int>> _dictionaryOfQueenContiguity, _dictionaryOfRookContiguity;

        private NeighborhoodOptions _neighborhoodOption = NeighborhoodOptions.KNearestNeighbors;
        private int _numberOfKNearestNeighbors = 3;
        private int _numberOfKNearestNeighborsForAutomatic = 3;
        private double _threshold;
        private int _numberOfNeighbors = 20;
        private int _numberOfNeighborsLastRun = 0;

        public LWLCTool(DataTable dataTable, ToolParameterContainer toolParameterContainer, IFeatureClass featureClass)
        {
            _dataTable = dataTable;
            _toolParameterContainer = toolParameterContainer;
            _featureClass = featureClass;

            _resultDataTable = new DataTable();
        }

        protected override void PerformAlgorithm(ProgressHandler childHandler = null)
        {
            ProgressHandler dictionaryHandler = childHandler?.ProvideChildProgressHandler(30);
            if (_featureClass == null)
            {
                return;
            }

            switch (_neighborhoodOption)
            {
                case NeighborhoodOptions.KNearestNeighbors:
                case NeighborhoodOptions.Threshold:
                case NeighborhoodOptions.Automatic:
                    if (_dictionaryOfDistances == null || _numberOfNeighborsLastRun < _numberOfNeighbors)
                        _dictionaryOfDistances = BuildDictionaryOfDistancesByCentroid(dictionaryHandler);
                    break;
                case NeighborhoodOptions.Queen:
                    if (_dictionaryOfQueenContiguity == null)
                        _dictionaryOfQueenContiguity = BuildDictionaryOfQueenContiguity(dictionaryHandler);
                    break;
                case NeighborhoodOptions.Rook:
                    if (_dictionaryOfRookContiguity == null)
                        _dictionaryOfRookContiguity = BuildDictionaryOfRookContiguity(dictionaryHandler);
                    break;
            }

            _resultDataTable = new DataTable();

            _resultDataTable.Columns.Add(_featureClass.OIDFieldName, typeof(FieldTypeOID));
            _resultDataTable.Columns.Add("cluster ids:", typeof(string));

            foreach (var currentToolParameter in _toolParameterContainer.ToolParameter)
            {
                _resultDataTable.Columns.Add("local range: " + currentToolParameter.ColumnName, typeof(double));
                _resultDataTable.Columns.Add("normalized value: " + currentToolParameter.ColumnName, typeof(double));
                _resultDataTable.Columns.Add("local weight: " + currentToolParameter.ColumnName, typeof(double));
            }

            //result column
            _resultDataTable.Columns.Add(_defaultResultColumnName, typeof(double));

            _resultDataTable.BeginLoadData();

            ProgressHandler tableHandler = childHandler?.ProvideChildProgressHandler(50);
            switch (_neighborhoodOption)
            {
                case NeighborhoodOptions.KNearestNeighbors:
                    BuildKNearestNeighborTable(tableHandler);
                    break;
                case NeighborhoodOptions.Threshold:
                    BuildThresholdTable(tableHandler);
                    break;
                case NeighborhoodOptions.Queen:
                    BuildQueenContiguityTable(tableHandler);
                    break;
                case NeighborhoodOptions.Rook:
                    BuildRookContiguityTable(tableHandler);
                    break;
                case NeighborhoodOptions.Automatic:
                    BuildAutomaticTable(tableHandler);
                    break;
            }

            _resultDataTable.EndLoadData();
        }

        private void BuildAutomaticTable(ProgressHandler childHandler = null)
        {
            var lockObject = new object();

            Parallel.ForEach(_dictionaryOfDistances.Keys, currentId =>
                {
                    var list = _dictionaryOfDistances[currentId];

                    int maxElements = list.Count;
                    int tryKNearestNeighbors = _numberOfKNearestNeighborsForAutomatic;

                    var c = new Cluster(currentId, list.OrderBy(t => t.Item2).Take(tryKNearestNeighbors).Select(t => t.Item1).ToList(), _dataTable, _toolParameterContainer, _tranformationStrategy);

                    // lets try another value - IsResultNull also calculates all required stuff for NewRow
                    while (c.IsResultNull() && tryKNearestNeighbors <= maxElements)
                    {
                        tryKNearestNeighbors++;
                        c = new Cluster(currentId, list.OrderBy(t => t.Item2).Take(tryKNearestNeighbors).Select(t => t.Item1).ToList(), _dataTable, _toolParameterContainer, _tranformationStrategy);
                    }

                    lock (lockObject)
                    {
                        childHandler?.OnProgress(_resultDataTable.Rows.Count+1, _dictionaryOfDistances.Keys.Count+1, $"Creating Automatic table\n Calculating row {_resultDataTable.Rows.Count} from {_dictionaryOfDistances.Keys.Count}");
                        _resultDataTable.Rows.Add(c.FillRowWithResults(_resultDataTable.NewRow()));
                    }
                });
        }

        private void BuildRookContiguityTable(ProgressHandler childHandler = null)
        {
            var lockObject = new object();

            Parallel.ForEach(_dictionaryOfRookContiguity.Keys, currentId =>
                {
                    var list = new List<int>(_dictionaryOfRookContiguity[currentId]);

                    var c = new Cluster(currentId, list, _dataTable, _toolParameterContainer, _tranformationStrategy);

                    c.Calculate();

                    lock (lockObject)
                    {
                        childHandler?.OnProgress(_resultDataTable.Rows.Count+1, _dictionaryOfDistances.Keys.Count+1, $"Creating Rook Contiguity table\n Calculating row {_resultDataTable.Rows.Count} from {_dictionaryOfDistances.Keys.Count}");
                        _resultDataTable.Rows.Add(c.FillRowWithResults(_resultDataTable.NewRow()));
                    }

                });
        }

        private void BuildQueenContiguityTable(ProgressHandler childHandler = null)
        {
            var lockObject = new object();

            Parallel.ForEach(_dictionaryOfQueenContiguity.Keys, currentId =>
                {
                    var list = new List<int>(_dictionaryOfQueenContiguity[currentId]);

                    var c = new Cluster(currentId, list, _dataTable, _toolParameterContainer, _tranformationStrategy);

                    c.Calculate();

                    lock (lockObject)
                    {
                        childHandler?.OnProgress(_resultDataTable.Rows.Count+1, _dictionaryOfDistances.Keys.Count+1, $"Creating Queen Contiguity table\nCalculating row {_resultDataTable.Rows.Count} from {_dictionaryOfDistances.Keys.Count}");
                        _resultDataTable.Rows.Add(c.FillRowWithResults(_resultDataTable.NewRow()));
                    }

                });
        }

        private void BuildKNearestNeighborTable(ProgressHandler childHandler = null)
        {
            var lockObject = new object();

            Parallel.ForEach(_dictionaryOfDistances.Keys, currentId =>
                {
                    var list = _dictionaryOfDistances[currentId];

                    var c = new Cluster(currentId,
                                            list.OrderBy(t => t.Item2)
                                                .Take(_numberOfKNearestNeighbors)
                                                .Select(t => t.Item1)
                                                .ToList(), _dataTable, _toolParameterContainer, _tranformationStrategy);

                    c.Calculate();

                    lock (lockObject)
                    {
                        childHandler?.OnProgress(_resultDataTable.Rows.Count+1, _dictionaryOfDistances.Keys.Count+1, $"Creating KNearest neighboor table\nCalculating row {_resultDataTable.Rows.Count} from {_dictionaryOfDistances.Keys.Count}");
                        _resultDataTable.Rows.Add(c.FillRowWithResults(_resultDataTable.NewRow()));
                    }
                });
        }

        private void BuildThresholdTable(ProgressHandler childHandler = null)
        {
            var lockObject = new object();

            Parallel.ForEach(_dictionaryOfDistances.Keys, currentId =>
                {
                    var list = new List<Tuple<int, float>>(_dictionaryOfDistances[currentId]);

                    var c = new Cluster(currentId, list.Where(t => t.Item2 <= _threshold).Select(t => t.Item1).ToList(), _dataTable, _toolParameterContainer, _tranformationStrategy);

                    c.Calculate();

                    lock (lockObject)
                    {
                        childHandler?.OnProgress(_resultDataTable.Rows.Count+1, _dictionaryOfDistances.Keys.Count+1, $"Creating Threshold table\nCalculating row {_resultDataTable.Rows.Count} from {_dictionaryOfDistances.Keys.Count}");
                        _resultDataTable.Rows.Add(c.FillRowWithResults(_resultDataTable.NewRow()));
                    }
                });
        }

        private IDictionary<int, List<Tuple<int, float>>> BuildDictionaryOfDistancesByCentroid(ProgressHandler childHandler = null)
        {
            var centroidArray = new double[_featureClass.FeatureCount(null), 3];

            ProgressHandler centroidHandler = childHandler?.ProvideChildProgressHandler(70);
            using (var comReleaser = new ComReleaser())
            {
                var featureCursor = _featureClass.Search(null, false);

                comReleaser.ManageLifetime(featureCursor);

                var oidColumn = _featureClass.FindField(_featureClass.OIDFieldName);

                var zeroOidExist = false;

                var centroidArrayIndex = 0;

                int numberOfFields = _featureClass.FeatureCount(null);
                IFeature currentFeature;
                while ((currentFeature = featureCursor.NextFeature()) != null)
                {
                    centroidHandler?.OnProgress(centroidArrayIndex+1, numberOfFields+1, $"Creating neighborhood\nCalculating centroid {centroidArrayIndex} from {numberOfFields}");

                    var oid = Convert.ToInt32(currentFeature.Value[oidColumn]);
                    centroidArray[centroidArrayIndex, 0] = oid;

                    if (oid == 0)
                    {
                        zeroOidExist = true;
                    }

                    var area = (IArea)currentFeature.Shape;
                    centroidArray[centroidArrayIndex, 1] = area.Centroid.X;
                    centroidArray[centroidArrayIndex, 2] = area.Centroid.Y;

                    centroidArrayIndex++;
                }

                // it is possible that the oid column starts at zero and the other program parts expect it at 1, thus we have to check if the name is FID and one oid is zero
                if ("FID".Equals(_featureClass.OIDFieldName) && zeroOidExist)
                {
                    for (int i = 0; i < centroidArray.GetLength(0); i++)
                    {
                        centroidArray[i, 0]++;
                    }
                }
            }

            IDictionary<int, List<Tuple<int, float>>> dictionaryOfDistances = new ConcurrentDictionary<int, List<Tuple<int, float>>>();

            ProgressHandler distanceHandler = childHandler?.ProvideChildProgressHandler(30);
            int numberOfCalculatedCentroid = 0;
            Parallel.For(0, centroidArray.GetLength(0), i =>
            {
                Interlocked.Increment(ref numberOfCalculatedCentroid);
                distanceHandler?.OnProgress(numberOfCalculatedCentroid + 1, centroidArray.GetLength(0)+1, $"Creating neighborhood\nCalculating centroid distance {numberOfCalculatedCentroid} from {centroidArray.GetLength(0)}");
                var setOfDistances = new SortedSet<Tuple<int, float>>(new TupleComparer());

                for (int j = 0; j < centroidArray.GetLength(0); j++)
                {
                    //do not add the distance too itself
                    if (i == j)
                    {
                    }
                    else
                    {
                        var distance = (float)EuclidianDistance(centroidArray[i, 1], centroidArray[i, 2], centroidArray[j, 1], centroidArray[j, 2]);
                        var id = Convert.ToInt32(centroidArray[j, 0]);

                        setOfDistances.Add(Tuple.Create(id, distance));

                        if (setOfDistances.Count == _numberOfNeighbors + 1)
                        {
                            setOfDistances.Remove(setOfDistances.Last());
                        }
                    }
                }

                dictionaryOfDistances.Add(Convert.ToInt32(centroidArray[i, 0]), setOfDistances.ToList());
            });

            return dictionaryOfDistances;
        }
        
        private IDictionary<int, List<int>> BuildDictionaryOfQueenContiguity(ProgressHandler childHandler = null)
        {
            IDictionary<int, List<int>> neighborDictionary = new Dictionary<int, List<int>>();

            var zeroOidExist = false;

            using (var comReleaser = new ComReleaser())
            {
                var featureCursor = (IFeatureCursor)_featureClass.Search(null, false);

                comReleaser.ManageLifetime(featureCursor);

                ISpatialFilter spatialFilter = new SpatialFilterClass();
                spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                int numberOfFields = _featureClass.FeatureCount(null);
                IFeature currentFeature;
                while ((currentFeature = featureCursor.NextFeature()) != null)
                {
                    childHandler?.OnProgress(neighborDictionary.Count+1, numberOfFields+1, $"Creating neighborhood\nCalculating Queen Contiguity {neighborDictionary.Count} from {numberOfFields}");
                    spatialFilter.Geometry = currentFeature.Shape;
                    spatialFilter.GeometryField = _featureClass.ShapeFieldName;

                    var selectionSet = _featureClass.Select(spatialFilter,
                        esriSelectionType.esriSelectionTypeIDSet,
                        esriSelectionOption.esriSelectionOptionNormal, null);

                    ISet<int> neighborIDs = new HashSet<int>();

                    var enumIDs = selectionSet.IDs;

                    var id = enumIDs.Next();

                    while (id != -1)
                    {
                        if (id != currentFeature.OID)
                        {
                            neighborIDs.Add(id);
                        }

                        id = enumIDs.Next();
                    }

                    neighborDictionary.Add(currentFeature.OID, neighborIDs.ToList());

                    if (currentFeature.OID == 0)
                    {
                        zeroOidExist = true;
                    }
                }

                // it is possible that the oid column starts at zero and the other program parts expect it at 1, thus we have to check if the name is FID and one oid is zero
                if ("FID".Equals(_featureClass.OIDFieldName) && zeroOidExist)
                {
                    // the easiest way is to build a new dictionary
                    return neighborDictionary.ToDictionary(k => k.Key + 1, v => v.Value.Select(x => x + 1).ToList());
                }
            }

            return neighborDictionary;
        }

        private IDictionary<int, List<int>> BuildDictionaryOfRookContiguity(ProgressHandler childHandler = null)
        {
            IDictionary<int, List<int>> neighborDictionary = new ConcurrentDictionary<int, List<int>>();

            var zeroOidExist = false;

            using (var comReleaser = new ComReleaser())
            {
                var featureCursor = (IFeatureCursor)_featureClass.Search(null, false);

                comReleaser.ManageLifetime(featureCursor);

                ISpatialFilter spatialFilter = new SpatialFilterClass();
                spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                int numberOfFields = _featureClass.FeatureCount(null);
                IFeature currentFeature;
                while ((currentFeature = featureCursor.NextFeature()) != null)
                {
                    childHandler?.OnProgress(neighborDictionary.Count + 1, numberOfFields + 1, $"Creating neighborhood\nCalculating Queen Contiguity {neighborDictionary.Count} from {numberOfFields}");
                    if (currentFeature.OID == 0)
                    {
                        zeroOidExist = true;
                    }

                    spatialFilter.Geometry = currentFeature.Shape;
                    spatialFilter.GeometryField = _featureClass.ShapeFieldName;

                    var selectionSet = _featureClass.Select(spatialFilter,
                        esriSelectionType.esriSelectionTypeIDSet,
                        esriSelectionOption.esriSelectionOptionNormal, null);

                    var topologicalOperator = (ITopologicalOperator)currentFeature.Shape;

                    ISet<int> neighborIDs = new HashSet<int>();

                    var enumIDs = selectionSet.IDs;

                    var id = enumIDs.Next();

                    while (id != -1)
                    {
                        if (id != currentFeature.OID)
                        {
                            // http://resources.arcgis.com/en/help/main/10.1/index.html#//00080000000z000000
                            var polylineCollection =
                                (IGeometryCollection)
                                topologicalOperator.Intersect(_featureClass.GetFeature(id).Shape,
                                                              esriGeometryDimension.esriGeometry1Dimension);

                            // we have one or more polylines in common => add
                            if (polylineCollection.GeometryCount >= 1)
                            {
                                neighborIDs.Add(id);

                                //we can move on, no need to check for 2 dim intersect
                                id = enumIDs.Next();

                                continue;
                            }

                            var polygonCollection =
                                (IGeometryCollection)
                                topologicalOperator.Intersect(_featureClass.GetFeature(id).Shape,
                                                              esriGeometryDimension.esriGeometry2Dimension);

                            // we have one or more polygons in common => add
                            if (polygonCollection.GeometryCount >= 1)
                            {
                                neighborIDs.Add(id);
                            }
                        }

                        id = enumIDs.Next();
                    }

                    neighborDictionary.Add(currentFeature.OID, neighborIDs.ToList());
                }
            }

            // it is possible that the oid column starts at zero and the other program parts expect it at 1, thus we have to check if the name is FID and one oid is zero
            if (_featureClass.OIDFieldName.Equals("FID") && zeroOidExist)
            {
                // the easiest way is to build a new dictionary
                return neighborDictionary.ToDictionary(k => k.Key + 1, v => v.Value.Select(x => x + 1).ToList());
            }

            return neighborDictionary;
        }

        private double EuclidianDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2d) + Math.Pow(y1 - y2, 2d));
        }

        protected override void PerformScaling()
        {
            // done in cluster
        }

        public double ThresholdMin
        {
            get
            {
                if (_featureClass == null || !_toolParameterContainer.ToolParameter.Any())
                {
                    return 0;
                }

                if (_dictionaryOfDistances == null)
                {
                    _dictionaryOfDistances = BuildDictionaryOfDistancesByCentroid();
                }

                return _dictionaryOfDistances.Values.Min(l => l.Min(t => t.Item2));
            }
        }

        public double ThresholdMax
        {
            get
            {
                if (_featureClass == null || !_toolParameterContainer.ToolParameter.Any())
                {
                    return 0;
                }

                if (_dictionaryOfDistances == null)
                {
                    _dictionaryOfDistances = BuildDictionaryOfDistancesByCentroid();
                }

                return _dictionaryOfDistances.Values.Min(l => l.Max(t => t.Item2));
            }
        }

        public IFeatureClass FeatureClass
        {
            get { return _featureClass; }
            set { _featureClass = value; }
        }

        public int NumberOfKNearestNeighborsForAutomatic
        {
            get { return _numberOfKNearestNeighborsForAutomatic; }
            set { _numberOfKNearestNeighborsForAutomatic = value; }
        }

        public int NumberOfKNearestNeighbors
        {
            get { return _numberOfKNearestNeighbors; }
            set { _numberOfKNearestNeighbors = value; }
        }

        public int NumberOfNeighbors
        {
            get { return _numberOfNeighbors; }
            set { _numberOfNeighbors = value; }
        }

        public double Threshold
        {
            get { return _threshold; }
            set { _threshold = value; }
        }

        public NeighborhoodOptions NeighborhoodOptions
        {
            get { return _neighborhoodOption; }
            set { _neighborhoodOption = value; }
        }

        public override string DefaultResultColumnName
        {
            get { return _defaultResultColumnName; }
            set { _defaultResultColumnName = value; }
        }

        public override System.Data.DataTable Data => _resultDataTable;

        public override ToolParameterContainer ToolParameterContainer
        {
            get { return _toolParameterContainer; }
            set { _toolParameterContainer = value; }
        }

        public override NormalizationStrategy TransformationStrategy
        {
            get { return _tranformationStrategy; }
            set { _tranformationStrategy = value; }
        }

        public override string ToString()
        {
            return "LWLC Tool";
        }
    }

    internal class TupleComparer : IComparer<Tuple<int, float>>
    {
        public int Compare(Tuple<int, float> x, Tuple<int, float> y)
        {
            return x.Item2.CompareTo(y.Item2);
        }
    }
}
