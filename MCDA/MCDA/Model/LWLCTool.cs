﻿using System;
using System.Collections.Generic;
using System.Linq;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Data;
using ESRI.ArcGIS.ADF;
using MCDA.ViewModel;
using System.Threading.Tasks;
using System.Collections.Concurrent;

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

        protected override void PerformAlgorithm()
        {
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
                        _dictionaryOfDistances = BuildDictionaryOfDistancesByCentroid();
                    break;
                case NeighborhoodOptions.Queen:
                    if (_dictionaryOfQueenContiguity == null)
                        _dictionaryOfQueenContiguity = BuildDictionaryOfQueenContiguity();
                    break;
                case NeighborhoodOptions.Rook:
                    if (_dictionaryOfRookContiguity == null)
                        _dictionaryOfRookContiguity = BuildDictionaryOfRookContiguity();
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

            switch (_neighborhoodOption)
            {
                case NeighborhoodOptions.KNearestNeighbors:
                    BuildKNearestNeighborTable();
                    break;
                case NeighborhoodOptions.Threshold:
                    BuildThresholdTable();
                    break;
                case NeighborhoodOptions.Queen:
                    BuildQueenContiguityTable();
                    break;
                case NeighborhoodOptions.Rook:
                    BuildRookContiguityTable();
                    break;
                case NeighborhoodOptions.Automatic:
                    BuildAutomaticTable();
                    break;
            }

            _resultDataTable.EndLoadData();
        }

        private void BuildAutomaticTable()
        {
            var lockObject = new object();

            Parallel.ForEach(_dictionaryOfDistances.Keys, currentID =>
                {
                    var list = _dictionaryOfDistances[currentID];

                    int maxElements = list.Count();
                    int tryKNearestNeighbors = _numberOfKNearestNeighborsForAutomatic;

                    var c = new Cluster(currentID, list.OrderBy(t => t.Item2).Take(tryKNearestNeighbors).Select(t => t.Item1).ToList(), _dataTable, _toolParameterContainer, _tranformationStrategy);

                    // lets try another value - IsResultNull also calculates all required stuff for NewRow
                    while (c.IsResultNull() && tryKNearestNeighbors <= maxElements)
                    {
                        tryKNearestNeighbors++;
                        c = new Cluster(currentID, list.OrderBy(t => t.Item2).Take(tryKNearestNeighbors).Select(t => t.Item1).ToList(), _dataTable, _toolParameterContainer, _tranformationStrategy);
                    }

                    lock (lockObject)
                    {
                        _resultDataTable.Rows.Add((c.FillRowWithResults(_resultDataTable.NewRow())));
                    }
                });
        }

        private void BuildRookContiguityTable()
        {
            var lockObject = new object();

            Parallel.ForEach(_dictionaryOfRookContiguity.Keys, currentID =>
                {
                    var list = new List<int>(_dictionaryOfRookContiguity[currentID]);

                    var c = new Cluster(currentID, list, _dataTable, _toolParameterContainer, _tranformationStrategy);

                    c.Calculate();

                    lock (lockObject)
                    {
                        _resultDataTable.Rows.Add((c.FillRowWithResults(_resultDataTable.NewRow())));
                    }

                });
        }

        private void BuildQueenContiguityTable()
        {
            var lockObject = new object();

            Parallel.ForEach(_dictionaryOfQueenContiguity.Keys, currentID =>
                {
                    var list = new List<int>(_dictionaryOfQueenContiguity[currentID]);

                    var c = new Cluster(currentID, list, _dataTable, _toolParameterContainer, _tranformationStrategy);

                    c.Calculate();

                    lock (lockObject)
                    {
                        _resultDataTable.Rows.Add((c.FillRowWithResults(_resultDataTable.NewRow())));
                    }

                });
        }

        private void BuildKNearestNeighborTable()
        {
            var lockObject = new object();

            Parallel.ForEach(_dictionaryOfDistances.Keys, currentID =>
                {
                    var list = _dictionaryOfDistances[currentID];

                    var c = new Cluster(currentID,
                                            list.OrderBy(t => t.Item2)
                                                .Take(_numberOfKNearestNeighbors)
                                                .Select(t => t.Item1)
                                                .ToList(), _dataTable, _toolParameterContainer, _tranformationStrategy);

                    c.Calculate();

                    lock (lockObject)
                    {
                        _resultDataTable.Rows.Add((c.FillRowWithResults(_resultDataTable.NewRow())));
                    }
                });
        }

        private void BuildThresholdTable()
        {
            var lockObject = new object();

            Parallel.ForEach(_dictionaryOfDistances.Keys, currentID =>
                {
                    var list = new List<Tuple<int, float>>(_dictionaryOfDistances[currentID]);

                    var c = new Cluster(currentID, list.Where(t => t.Item2 <= _threshold).Select(t => t.Item1).ToList(), _dataTable, _toolParameterContainer, _tranformationStrategy);

                    c.Calculate();

                    lock (lockObject)
                    {
                        _resultDataTable.Rows.Add((c.FillRowWithResults(_resultDataTable.NewRow())));
                    }
                });
        }

        private IDictionary<int, List<Tuple<int, float>>> BuildDictionaryOfDistancesByCentroid()
        {
            var centroidArray = new double[_featureClass.FeatureCount(null), 3];

            using (var comReleaser = new ComReleaser())
            {
                var featureCursor = (IFeatureCursor)_featureClass.Search(null, false);

                comReleaser.ManageLifetime(featureCursor);

                var numberOfFeatures = _featureClass.FeatureCount(null);

                var oidColumn = _featureClass.FindField(_featureClass.OIDFieldName);

                var zeroOIDExist = false;

                var centroidArrayIndex = 0;

                IFeature currentFeature;
                while ((currentFeature = featureCursor.NextFeature()) != null)
                {
                    var oid = Convert.ToInt32(currentFeature.Value[oidColumn]);
                    centroidArray[centroidArrayIndex, 0] = oid;

                    if (oid == 0)
                    {
                        zeroOIDExist = true;
                    }

                    var area = (IArea)currentFeature.Shape;
                    centroidArray[centroidArrayIndex, 1] = area.Centroid.X;
                    centroidArray[centroidArrayIndex, 2] = area.Centroid.Y;

                    centroidArrayIndex++;
                }

                // it is possible that the oid column starts at zero and the other program parts expect it at 1, thus we have to check if the name is FID and one oid is zero
                if (_featureClass.OIDFieldName.Equals("FID") && zeroOIDExist)
                {

                    for (int i = 0; i < centroidArray.GetLength(0); i++)
                    {
                        centroidArray[i, 0]++;
                    }
                }
            }

            IDictionary<int, List<Tuple<int, float>>> dictionaryOfDistances = new ConcurrentDictionary<int, List<Tuple<int, float>>>();

            Parallel.For(0, centroidArray.GetLength(0), i =>
            {
                var setOfDistances = new SortedSet<Tuple<int, float>>(new TupleComparer());

                for (int j = 0; j < centroidArray.GetLength(0); j++)
                {
                    //do not add the distance too itself
                    if (i == j)
                    {
                        continue;
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

        private IDictionary<int, List<int>> BuildDictionaryOfQueenContiguity()
        {
            IDictionary<int, List<int>> neighborDictionary = new Dictionary<int, List<int>>();

            var zeroOIDExist = false;

            using (var comReleaser = new ComReleaser())
            {
                var featureCursor = (IFeatureCursor)_featureClass.Search(null, false);

                comReleaser.ManageLifetime(featureCursor);

                ISpatialFilter spatialFilter = new SpatialFilterClass();
                spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                IFeature currentFeature;
                while ((currentFeature = featureCursor.NextFeature()) != null)
                {

                    spatialFilter.Geometry = currentFeature.Shape;
                    spatialFilter.GeometryField = _featureClass.ShapeFieldName;

                    var selectionSet = _featureClass.Select(spatialFilter,
                        esriSelectionType.esriSelectionTypeIDSet,
                        esriSelectionOption.esriSelectionOptionNormal, null);

                    ISet<int> neighborIDs = new HashSet<int>();

                    var enumIDs = selectionSet.IDs;

                    var ID = enumIDs.Next();

                    while (ID != -1)
                    {
                        if (ID != currentFeature.OID)
                        {
                            neighborIDs.Add(ID);
                        }

                        ID = enumIDs.Next();
                    }

                    neighborDictionary.Add(currentFeature.OID, neighborIDs.ToList());

                    if (currentFeature.OID == 0)
                    {
                        zeroOIDExist = true;
                    }
                }

                // it is possible that the oid column starts at zero and the other program parts expect it at 1, thus we have to check if the name is FID and one oid is zero
                if (_featureClass.OIDFieldName.Equals("FID") && zeroOIDExist)
                {
                    // the easiest way is to build a new dictionary
                    return neighborDictionary.ToDictionary(k => k.Key + 1, v => v.Value.Select(x => x + 1).ToList());
                }
            }

            return neighborDictionary;
        }

        private IDictionary<int, List<int>> BuildDictionaryOfRookContiguity()
        {
            IDictionary<int, List<int>> neighborDictionary = new ConcurrentDictionary<int, List<int>>();

            var zeroOIDExist = false;

            using (var comReleaser = new ComReleaser())
            {
                var featureCursor = (IFeatureCursor)_featureClass.Search(null, false);

                comReleaser.ManageLifetime(featureCursor);

                ISpatialFilter spatialFilter = new SpatialFilterClass();
                spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                IFeature currentFeature;
                while ((currentFeature = featureCursor.NextFeature()) != null)
                {
                    if (currentFeature.OID == 0)
                    {
                        zeroOIDExist = true;
                    }

                    spatialFilter.Geometry = currentFeature.Shape;
                    spatialFilter.GeometryField = _featureClass.ShapeFieldName;

                    var selectionSet = _featureClass.Select(spatialFilter,
                        esriSelectionType.esriSelectionTypeIDSet,
                        esriSelectionOption.esriSelectionOptionNormal, null);

                    var topologicalOperator = (ITopologicalOperator)currentFeature.Shape;

                    ISet<int> neighborIDs = new HashSet<int>();

                    var enumIDs = selectionSet.IDs;

                    var ID = enumIDs.Next();

                    while (ID != -1)
                    {
                        if (ID != currentFeature.OID)
                        {
                            // http://resources.arcgis.com/en/help/main/10.1/index.html#//00080000000z000000
                            var polylineCollection =
                                (IGeometryCollection)
                                topologicalOperator.Intersect(_featureClass.GetFeature(ID).Shape,
                                                              esriGeometryDimension.esriGeometry1Dimension);

                            // we have one or more polylines in common => add
                            if (polylineCollection.GeometryCount >= 1)
                            {
                                neighborIDs.Add(ID);

                                //we can move on, no need to check for 2 dim intersect
                                ID = enumIDs.Next();

                                continue;
                            }

                            var polygonCollection =
                                (IGeometryCollection)
                                topologicalOperator.Intersect(_featureClass.GetFeature(ID).Shape,
                                                              esriGeometryDimension.esriGeometry2Dimension);

                            // we have one or more polygons in common => add
                            if (polygonCollection.GeometryCount >= 1)
                            {
                                neighborIDs.Add(ID);
                            }
                        }

                        ID = enumIDs.Next();
                    }

                    neighborDictionary.Add(currentFeature.OID, neighborIDs.ToList());
                }
            }

            // it is possible that the oid column starts at zero and the other program parts expect it at 1, thus we have to check if the name is FID and one oid is zero
            if (_featureClass.OIDFieldName.Equals("FID") && zeroOIDExist)
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

        public override System.Data.DataTable Data
        {
            get { return _resultDataTable; }
        }

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

    class TupleComparer : IComparer<Tuple<int, float>>
    {
        public int Compare(Tuple<int, float> x, Tuple<int, float> y)
        {
            return x.Item2.CompareTo(y.Item2);
        }
    }
}
