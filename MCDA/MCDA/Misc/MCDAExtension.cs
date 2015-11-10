using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using MCDA.Extensions;
using MCDA.Model;
using Feature = MCDA.Model.Feature;
using Field = MCDA.Model.Field;
using MCDA.ViewModel;

namespace MCDA
{
    internal sealed class MCDAExtension : Extension, INotifyPropertyChanged
    {
        private static MCDAExtension _extension;

        private readonly IDictionary<AbstractToolTemplate, Feature> _dictionaryOfLinks =
            new Dictionary<AbstractToolTemplate, Feature>();

        private IActiveViewEvents_Event _activeViewEvents;

        private IWorkspace _shadowWorkspace;

        private readonly IList<PropertyChangedEventHandler> _listOfpropertyChangedEventHandlersForFeatureIsSelected = new List<PropertyChangedEventHandler>();

        public MCDAExtension()
        {
            AvailableFeatures = new ObservableCollection<Feature>();
        }

        #region properties

        /// <summary>
        /// If non is selected the property returns <code>null</code>.
        /// The system ensures that all clients that listen for changes are informed in case the <see cref="SelectedFeature"/> changes.
        /// </summary>
        public Feature SelectedFeature
        {
            get { return AvailableFeatures.FirstOrDefault(l => l.IsSelected); }
        }

        /// <summary>
        ///     Gets the list of <see cref="MCDA.Model.Feature" /> that are available in the context of the add-in.
        /// </summary>
        /// <remarks>
        ///     It is not guaranteed that the features have a <see cref="IFeatureLayer2" />, a <see cref="FeatureClass" /> or both.
        ///     It is also not guaranteed
        ///     that they provide a topological operator. Thus, not all of the <see cref="MCDA.Model.Feature" /> in this list are
        ///     eligible for MCDA.
        /// </remarks>
        public ObservableCollection<Feature> AvailableFeatures { get; private set; }

        /// <summary>
        ///     Gets all <see cref="MCDA.Model.Feature" /> from the in-memory workspace.
        /// </summary>
        public IEnumerable<Feature> FeaturesFromInMemoryWorkspace
        {
            get { return _dictionaryOfLinks.Values.ToList(); }
        }

        public IDictionary<AbstractToolTemplate, Feature> LinkDictionary
        {
            get { return _dictionaryOfLinks; }
        }

        #endregion

        #region extension related methods

        public static MCDAExtension GetExtension()
        {
            if (_extension != null)
            {
                return _extension;
            }

            UID id = new UIDClass();

            id.Value = ThisAddIn.IDs.MCDAExtension;
            ArcMap.Application.FindExtensionByCLSID(id);

            return _extension;
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();

            _extension = null;
        }

        protected override void OnStartup()
        {          
            _extension = this;

            _shadowWorkspace = CreateInMemoryWorkspace();

            AvailableFeatures.CollectionChanged += ListOfAvailableFeaturesChanged;

            var map = ArcMap.Document.ActiveView.FocusMap;

            _activeViewEvents = map as IActiveViewEvents_Event;

            _activeViewEvents.ItemAdded += ArcMapItemAdded;
            _activeViewEvents.ItemDeleted += ArcMapItemDeleted;

            ArcMap.Events.NewDocument += EventsNewDocument;
            //ArcMap.Events.BeforeCloseDocument += new ESRI.ArcGIS.ArcMapUI.IDocumentEvents_BeforeCloseDocumentEventHandler(EventsBeforeCloseDocument); 
            ArcMap.Events.OpenDocument += EventsOpenDocument;

            AddItemsOnStartup(ArcMap.Document.ActiveView);
             
        }

        void ListOfAvailableFeaturesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var currentFeature in AvailableFeatures)
            {
                currentFeature.UnRegisterPropertyHandler(_listOfpropertyChangedEventHandlersForFeatureIsSelected);
            }

            foreach (var currentFeature in AvailableFeatures)
            {
                _listOfpropertyChangedEventHandlersForFeatureIsSelected.Add(
                    currentFeature.RegisterPropertyHandler(f => f.IsSelected, IsSelectedChanged));
            }
        }

        private void IsSelectedChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged.Notify(() => SelectedFeature);
        }

        #endregion

        #region events

        private void EventsNewDocument()
        {
            _activeViewEvents.ItemAdded -= ArcMapItemAdded;
            _activeViewEvents.ItemDeleted -= ArcMapItemDeleted;

            var map = ArcMap.Document.ActiveView.FocusMap;

            _activeViewEvents = map as IActiveViewEvents_Event;

            _activeViewEvents.ItemAdded += ArcMapItemAdded;
            _activeViewEvents.ItemDeleted += ArcMapItemDeleted;
        }

        private void EventsOpenDocument()
        {
            _activeViewEvents.ItemAdded -= ArcMapItemAdded;
            _activeViewEvents.ItemDeleted -= ArcMapItemDeleted;

            var map = ArcMap.Document.ActiveView.FocusMap;

            _activeViewEvents = map as IActiveViewEvents_Event;

            _activeViewEvents.ItemAdded += ArcMapItemAdded;
            _activeViewEvents.ItemDeleted += ArcMapItemDeleted;

            AddItemsOnStartup(ArcMap.Document.ActiveView);
        }

        private void AddItemsOnStartup(ESRI.ArcGIS.Carto.IActiveView activeView)
        {
            ESRI.ArcGIS.Carto.IMap map = activeView.FocusMap;

            var numberOfLayers = map.LayerCount;

            for (var i = 0; i < numberOfLayers; i++)
            {
                ArcMapItemAdded(map.Layer[i]);
            }
        }

        private void ArcMapItemDeleted(object item)
        {
            var featureToRemove = AvailableFeatures.FirstOrDefault(f => f.ESRILayer == item);

            if (featureToRemove != null && featureToRemove.IsSelected) { 

                featureToRemove.IsSelected = false;
                PropertyChanged.Notify(() => SelectedFeature);
            }

            AvailableFeatures.Remove(featureToRemove);         
        }

        private void ArcMapItemAdded(object item)
        {
           var newLayer = item as ILayer2;

            if (newLayer != null)
            {
                AvailableFeatures.Add(new Feature(newLayer));
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public ToolParameterContainer GetToolParameterBasedOnSelectedFields()
        {
            IList<IToolParameter> toolParameter = new List<IToolParameter>();

            foreach (var currentAvailableLayer in AvailableFeatures.Where(l => l.IsSelected))
            {
                foreach (var currentField in currentAvailableLayer.Fields.Where(f => f.IsSelected))
                {
                    toolParameter.Add(new ToolParameter(currentField.FieldName));
                }
            }

            return new ToolParameterContainer(toolParameter);
        }

        /// <summary>
        ///     Returns the field of the OID for the selected feature. However, it is possible that no feature is selected
        ///     or that no OID exists or both. In this the methods returns null.
        /// </summary>
        /// <returns></returns>
        private Field GetOIDFieldFromSelectedFeature()
        {
            var selectedFeature = AvailableFeatures.FirstOrDefault(f => f.IsSelected);

            return selectedFeature == null ? null : selectedFeature.Fields.FirstOrDefault(f => f.IsOID);
        }

        /// <summary>
        ///     The method checks if the preferred name for the result column is already in use and if so
        ///     returns a non used by adding a number.
        ///     If the name is not used it returns the preferred name.
        /// </summary>
        /// <param name="preferredName"></param>
        /// <returns></returns>
        public string GetSuggestNameForResultColumn(string preferredName)
        {
            if (SelectedFeature == null || !SelectedFeature.Fields.Any(f => f.FieldName.Equals(preferredName)))
                return preferredName;

            var extension = 0;

            while (SelectedFeature.Fields.Any(f => f.FieldName.Equals(preferredName)))
            {
                //remove what we tried before
                if (extension > 0)
                {
                    preferredName = preferredName.Remove(preferredName.Length - (extension).ToString().Length);
                }

                extension++;
                preferredName += extension;
            }

            return preferredName;
        }

        public DataTable GetDataTableForParameterSet<T>(IEnumerable<T> toolParameter) where T : IToolParameter
        {
            var listOfFields = GetListOfFieldsFromToolParameter(toolParameter);

            var dataTable = new DataTable();

            //add columns
            foreach (var currentField in listOfFields)
            {
                dataTable.Columns.Add(currentField.FieldName, typeof(double));
            }

            //add the oid column
            var oidField = GetOIDFieldFromSelectedFeature();

            //we have to take care about FID oid fields http://gis.stackexchange.com/questions/40833/arcobjects-bug-in-oid-field-duplication-while-duplicating-feature-class
            var isOIDFieldNameFID = false;

            int dataTableOIDOrdinal = 0;
            //in fact no feature can be selected, thus we have to take care about potential null
            if (oidField != null)
            {
                dataTableOIDOrdinal = dataTable.Columns.Add(oidField.FieldName, typeof (FieldTypeOID)).Ordinal;

                //and to make it easier add the oidField to the rest of the fields
                // it is important to take care of the order of the columns! later we depend on it if we add data
                listOfFields.Insert(0, oidField);

                if (oidField.FieldName.Equals("FID"))
                {
                    isOIDFieldNameFID = true;
                }

                //make the oid column the first column
                dataTable.Columns[dataTableOIDOrdinal].SetOrdinal(0);
                //should be 0...
                dataTableOIDOrdinal = 0;
            }

            //get data for rows and store it in a list of list like a table
            IList<IList<double>> tableData = new List<IList<double>>();

            var isOIDFieldStartsByZero = false;

            var expectedNumbersOfRow = 0;
            foreach (var currentField in listOfFields)
            {
                IList<double> column = currentField.GetFieldData().ToList();
                tableData.Add(column);

                //each column has the data of all rows in the column
                expectedNumbersOfRow = column.Count;

                if (currentField.IsOID && column.Contains(0d))
                {
                    isOIDFieldStartsByZero = true;
                }
            }

            // add rows
            // the table data list has the same order as the data table columns!
            for (var i = 0; i < expectedNumbersOfRow; i++)
            {
                var row = dataTable.NewRow();

                for (var y = 0; y < tableData.Count; y++)
                {
                    //we have the oid column
                    if (y == dataTableOIDOrdinal)
                    {
                        //if the column is FID we have to change the ids 0...x to 1...x+1 because this change will be made after the feature class is copied into the in memory workspace
                        if (isOIDFieldNameFID && isOIDFieldStartsByZero)
                        {
                            row[y] = new FieldTypeOID {OID = (int) tableData[y][i] + 1};
                            continue;
                        }

                        row[y] = new FieldTypeOID {OID = (int) tableData[y][i]};
                        continue;
                    }

                    row[y] = tableData[y][i];
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        /// <summary>
        ///     In case the tool has a corresponding <see cref="IFeatureLayer2" /> in the in memory workspace it will be added
        ///     to the active view.
        /// </summary>
        /// <param name="tool"></param>
        public void DisplayLink(AbstractToolTemplate tool)
        {
            Feature feature;

            if (!_dictionaryOfLinks.TryGetValue(tool, out feature))
            {
                return;
            }

            ArcMap.Document.ActiveView.FocusMap.AddLayer((ILayer)feature.ESRILayer);
        }

        public void RemoveLink(AbstractToolTemplate tool)
        {
            Feature mcdaWorkspaceContainer;

            if (!_dictionaryOfLinks.TryGetValue(tool, out mcdaWorkspaceContainer))
            {
                return;
            }

            _dictionaryOfLinks.Remove(tool);

            PropertyChanged.Notify(() => LinkDictionary);
        }

        public void EstablishLink(AbstractToolTemplate tool)
        {
            var featureLayer = AvailableFeatures.First(l => l.IsSelected && l.IsSuitableForMCDA).FeatureLayer;
            var featureClass = featureLayer.FeatureClass;

            var copiedFeatureClass = CopyFeatureClassIntoNewWorkspace(featureClass, _shadowWorkspace,tool + CreateTimeStamp());

            var newFeatureLayer = new FeatureLayerClass { Name = CreateLayerName(tool) };

            var feature = new Feature(copiedFeatureClass, newFeatureLayer);

            _dictionaryOfLinks.Add(tool, feature);

            PropertyChanged.Notify(() => LinkDictionary);

        }

        private static String CreateLayerName(AbstractToolTemplate tool)
        {
            return tool + DateTime.Now.ToString("ddHHmmssffff");
        }

        private static String CreateTimeStamp()
        {
            return DateTime.Now.ToString("ddHHmmssffff");
        }

        /// <summary>
        ///     Joins the result table of the data table into the feature class of the tool by using the oid/fid.
        ///     Afterwards the method updates the view with the new result. This also includes the update of the class breaks or bi
        ///     polar renderer if set.
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="dataTable"></param>
        public void JoinToolResultByOID(AbstractToolTemplate tool, DataTable dataTable)
        {
            Feature feature;
            if (!_dictionaryOfLinks.TryGetValue(tool, out feature))
            {
                return;
            }

            var featureClass = feature.FeatureClass;

            if (featureClass.FindField(tool.DefaultResultColumnName) < 0)
            {
                IField newField = new FieldClass();
                var newFieldEdit = (IFieldEdit) newField;
                newFieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
                newFieldEdit.Name_2 = tool.DefaultResultColumnName;
                newFieldEdit.AliasName_2 = tool.DefaultResultColumnName;

                featureClass.AddField(newField);

                feature.UpdateFieldsProperty();
                feature.SetToolField(newField);
            }

            using (var comReleaser = new ComReleaser())
            {
                var featureCursor = featureClass.Update(null, true);

                comReleaser.ManageLifetime(featureCursor);

                var esriFeature = featureCursor.NextFeature();

                var fieldIndex = featureClass.FindField(feature.Fields.First(f => f.IsToolField).FieldName);

                var oidIndex = featureClass.FindField(featureClass.OIDFieldName);

                while (esriFeature != null)
                {
                    var oid = Convert.ToInt32(esriFeature.Value[oidIndex]);

                    var dataRows = dataTable.AsEnumerable().Where(dr => dr.Field<FieldTypeOID>(featureClass.OIDFieldName).OID == oid);

                    var dRow = dataRows.FirstOrDefault();

                    if (dRow != null) esriFeature.Value[fieldIndex] = dRow[tool.DefaultResultColumnName];

                    esriFeature.Store();

                    esriFeature = featureCursor.NextFeature();
                }
            }

            if (feature.SelectedFieldForRendering != null)
            {
                switch (ConfigSingleton.Instance.SelectedRenderoption)
                {
                    case RenderOption.AfterSliderDrag:
                        ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<RendererContainer, IFeatureLayer2>)Render, feature.SelectedFieldForRendering.RenderContainer, feature.FeatureLayer);
                        break;
                    case RenderOption.AnimationLike: case RenderOption.Realtime: default:
                        Render(feature.SelectedFieldForRendering.RenderContainer, feature.FeatureLayer);
                        break;
                }
            }
                
            PropertyChanged.Notify(() => LinkDictionary);
        }

        /// <summary>
        ///     If the optional <see cref="MCDA.Model.ClassBreaksRendererContainer" /> is set and includes all properties the
        ///     method
        ///     creates a new class breaks renderer and performs a partial refresh.
        /// </summary>
        /// <param name="renderContainer"></param>
        /// <param name="featureLayer"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void Render(RendererContainer renderContainer, IFeatureLayer2 featureLayer)
        {
            var geoFeatureLayer = featureLayer as IGeoFeatureLayer;

            switch (renderContainer.Renderer)
            {
                case Renderer.ClassBreaksRenderer:
                    geoFeatureLayer.Renderer = RendererFactory.NewClassBreaksRenderer(renderContainer);
                    break;
                case Renderer.BiPolarRenderer:
                    geoFeatureLayer.Renderer = RendererFactory.NewUniqueValueRenderer(renderContainer);
                    break;
                case Renderer.None:
                    geoFeatureLayer.Renderer = RendererFactory.NewSimpleRenderer();
                    break;
            }

            switch (ConfigSingleton.Instance.SelectedRenderoption)
            {
                case RenderOption.AfterSliderDrag:
                    PartialRefresh(renderContainer, featureLayer);
                    break;
                case RenderOption.AnimationLike: case RenderOption.Realtime: default:
                    PartialRefresh(renderContainer, featureLayer, false);
                    break;
            }
            
        }

        /// <summary>
        ///     Performs a partial refresh on the feature feature in the in memory workspace.
        /// </summary>
        /// <param name="renderContainer"></param>
        /// <param name="featureLayer"></param>
        /// <param name="updateTOC"></param>
        private static void PartialRefresh(RendererContainer renderContainer, IFeatureLayer2 featureLayer, bool updateTOC = true)
        {
            var activeView = (IActiveView) ArcMap.Document.FocusMap;

            if (updateTOC)
            {
                activeView.ContentsChanged();
                ArcMap.Document.UpdateContents();
            }

            activeView.PartialRefresh(esriViewDrawPhase.esriViewGeography, featureLayer, null);
        }

        #region private ArcObjects stuff

        private IList<Feature> GetListOfLayerFromActiveView(IActiveView activeView)
        {
            IList<Feature> layerList = new List<Feature>();

            var map = activeView.FocusMap;

            // Get the number of layers
            var numberOfLayers = map.LayerCount;

            // Loop through the layers and get the correct feature index
            for (var i = 0; i < numberOfLayers; i++)
            {
                layerList.Add(new Feature((ILayer2) map.Layer[i]));
            }

            return layerList;
        }

        private IList<Field> GetListOfFieldsFromToolParameter<T>(IEnumerable<T> toolParameter) where T : IToolParameter
        {
            IList<Field> listOfFields = new List<Field>();

            foreach (var currentToolParameter in toolParameter)
            {
                foreach (var currentAvailableLayer in AvailableFeatures.Where(l => l.IsSelected))
                {
                    var parameter = currentToolParameter;
                    foreach (var currentField in currentAvailableLayer.Fields.Where(f => f.FieldName.Equals(parameter.ColumnName)))
                    {
                        listOfFields.Add(currentField);
                    }
                }
            }

            return listOfFields;
        }

        private IWorkspace CreateInMemoryWorkspace()
        {
            var newWorkspaceFactory = new InMemoryWorkspaceFactoryClass();
            var workspaceName = newWorkspaceFactory.Create("", "MCDAWorkspace", null, 0);
            var name = (IName) workspaceName;
            var inMemoryWorkspace = (IWorkspace) name.Open();

            return inMemoryWorkspace;
        }

        private IFeatureClass CopyFeatureClassIntoNewWorkspace(IFeatureClass inFeatureClass, IWorkspace outWorkspace,
            string newName)
        {
            // get FeatureClassName for input
            var inDataset = inFeatureClass as IDataset;
            var inFeatureClassName = inDataset.FullName as IFeatureClassName;
            var inWorkspace = inDataset.Workspace;

            // get WorkSpaceName for output
            var outDataset = outWorkspace as IDataset;
            var outWorkspaceName = outDataset.FullName as IWorkspaceName;

            // Create new FeatureClassName
            IFeatureClassName outFeatureClassName = new FeatureClassNameClass();
            // Assign it a name and a workspace
            var datasetName = outFeatureClassName as IDatasetName;
            datasetName.Name = newName == String.Empty ? (inFeatureClassName as IDatasetName).Name : newName;
            datasetName.WorkspaceName = outWorkspaceName;

            // Check for field conflicts.
            IFieldChecker fieldChecker = new FieldCheckerClass();
            IFields inFields = inFeatureClass.Fields;
            IFields outFields;
            IEnumFieldError enumFieldError;
            fieldChecker.InputWorkspace = inWorkspace;
            fieldChecker.ValidateWorkspace = outWorkspace;
            fieldChecker.Validate(inFields, out enumFieldError, out outFields);
            // Check enumFieldError for field naming conflicts

            //Convert the data.
            IFeatureDataConverter featureDataConverter = new FeatureDataConverterClass();
            featureDataConverter.ConvertFeatureClass(inFeatureClassName, null, null,
                outFeatureClassName, null, outFields, "", 100, 0);

            var pFeatureWS = (IFeatureWorkspace) outWorkspace;

            return pFeatureWS.OpenFeatureClass(newName);
        }

        #endregion
    }
}