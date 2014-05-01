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
        ///     If non is selected the property returns <code>null</code>.
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
        public IList<Feature> FeaturesFromInMemoryWorkspace
        {
            get { return _dictionaryOfLinks.Values.ToList(); }
        }

        public IDictionary<AbstractToolTemplate, Feature> LinkDictionary
        {
            get { return _dictionaryOfLinks; }
        }

        #endregion

        #region event handling for listOfAvailableFeatures

        //private void RegisterListenerForEveryMemberOfListOfAvailableLayer()
        //{
        //    //unregister for all member to avoid unnecessary multiple call
        //    foreach (Feature currentAvailableLayer in listOfAvailableFeatures)
        //        currentAvailableLayer.PropertyChanged -= AvailalbeLayerListMemberPropertyChanged;

        //    foreach (Feature currentAvailableLayer in listOfAvailableFeatures)
        //        currentAvailableLayer.PropertyChanged += AvailalbeLayerListMemberPropertyChanged;
        //}

        /// <summary>
        ///     A single feature in the list of <see cref="AvailableFeatures" /> has changed.
        ///     This method also ensures that only one feature is selected at every time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void AvailalbeLayerListMemberPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    foreach (Feature currentAvailableLayer in listOfAvailableFeatures.Where(l => Feature.LastSelectedLayer != l))
        //        currentAvailableLayer.IsSelected = false;

        //    PropertyChanged.Notify(() => AvailableFeatures);
        //}

        #endregion

        #region extension related methods

        public static MCDAExtension GetExtension()
        {
            if (_extension == null)
            {
                UID id = new UIDClass();

                id.Value = ThisAddIn.IDs.MCDAExtension;
                ArcMap.Application.FindExtensionByCLSID(id);
            }

            return _extension;
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();

            _extension = null;
        }

        protected override void OnStartup()
        {
            Assembly assembly = Assembly.LoadFrom("HistogramControl.dll");

            _extension = this;

            _shadowWorkspace = CreateInMemoryWorkspace();

            AvailableFeatures.CollectionChanged += ListOfAvailableFeaturesChanged;

            IMap map = ArcMap.Document.ActiveView.FocusMap;

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
                currentFeature.UnRegisterPropertyHandler(_listOfpropertyChangedEventHandlersForFeatureIsSelected);

            foreach (var currentFeature in AvailableFeatures)
                _listOfpropertyChangedEventHandlersForFeatureIsSelected.Add(currentFeature.RegisterPropertyHandler(f => f.IsSelected, IsSelectedChanged));
        }

        public void IsSelectedChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged.Notify(() => SelectedFeature);
        }

        #endregion

        #region events

        private bool EventsBeforeCloseDocument()
        {
            // Return true to stop document from closing
            IMessageDialog msgBox = new MessageDialogClass();
            return msgBox.DoModal("BeforeCloseDocument Event", "Abort closing?", "Yes", "No", ArcMap.Application.hWnd);
        }

        private void EventsNewDocument()
        {
            _activeViewEvents.ItemAdded -= ArcMapItemAdded;
            _activeViewEvents.ItemDeleted -= ArcMapItemDeleted;

            IMap map = ArcMap.Document.ActiveView.FocusMap;

            _activeViewEvents = map as IActiveViewEvents_Event;

            _activeViewEvents.ItemAdded += ArcMapItemAdded;
            _activeViewEvents.ItemDeleted += ArcMapItemDeleted;

            //AvailableFeatures = new List<Feature>();
        }

        private void EventsOpenDocument()
        {
            _activeViewEvents.ItemAdded -= ArcMapItemAdded;
            _activeViewEvents.ItemDeleted -= ArcMapItemDeleted;

            IMap map = ArcMap.Document.ActiveView.FocusMap;

            _activeViewEvents = map as IActiveViewEvents_Event;

            _activeViewEvents.ItemAdded += ArcMapItemAdded;
            _activeViewEvents.ItemDeleted += ArcMapItemDeleted;

            //AvailableFeatures = new List<Feature>();
        }

        private void AddItemsOnStartup(ESRI.ArcGIS.Carto.IActiveView activeView)
        {
            ESRI.ArcGIS.Carto.IMap map = activeView.FocusMap;

            int numberOfLayers = map.LayerCount;

            for (int i = 0; i < numberOfLayers; i++)
                ArcMapItemAdded(map.Layer[i]);
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
           ILayer2 newLayer = item as ILayer2;

           if (newLayer != null)
               AvailableFeatures.Add(new Feature(newLayer));

            //PropertyChanged.Notify(() => AvailableFeatures);
            //PropertyChanged.Notify(() => EligibleFeaturesForMCDA);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public ToolParameterContainer GetToolParameterBasedOnSelectedFields()
        {
            IList<IToolParameter> toolParameter = new List<IToolParameter>();

            foreach (Feature currentAvailableLayer in AvailableFeatures.Where(l => l.IsSelected))
            {
                foreach (Field currentField in currentAvailableLayer.Fields.Where(f => f.IsSelected && f.IsNumeric))
                    toolParameter.Add(new ToolParameter(currentField.FieldName));
            }

            return new ToolParameterContainer(toolParameter);
        }

        /// <summary>
        ///     Returns the field of the OID for the selected feature. However, it is possible that no feature is selected
        ///     or that no OID exists or both. In this the methods returns null.
        /// </summary>
        /// <returns></returns>
        public Field GetOIDFieldFromSelectedFeature()
        {
            Feature selectedFeature = AvailableFeatures.FirstOrDefault(f => f.IsSelected);

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

            int extension = 0;

            while (SelectedFeature.Fields.Any(f => f.FieldName.Equals(preferredName)))
            {
                //remove what we tried before
                if (extension > 0)
                    preferredName = preferredName.Remove(preferredName.Length - (extension).ToString().Length);

                extension++;
                preferredName += extension;
            }

            return preferredName;
        }

        public IList<Field> GetFieldsFromSelectedLayerWhichAreNumeric(IList<Feature> layer)
        {
            return layer.Where(l => l.IsSelected).SelectMany(currentLayer => currentLayer.Fields.Where(f => f.IsNumeric)).ToList();
        }

        public DataTable GetDataTableForParameterSet<T>(IList<T> toolParameter) where T : IToolParameter
        {
            IList<Field> listOfFields = GetListOfFieldsFromToolParameter(toolParameter);

            var dataTable = new DataTable();

            //add columns
            foreach (Field currentField in listOfFields)
                dataTable.Columns.Add(currentField.FieldName, typeof (double));

            //add the oid column
            Field oidField = GetOIDFieldFromSelectedFeature();

            //we have to take care about FID oid fields http://gis.stackexchange.com/questions/40833/arcobjects-bug-in-oid-field-duplication-while-duplicating-feature-class
            bool isOIDFieldNameFID = false;

            int dataTableOIDOrdinal = 0;
            //in fact no feature can be selected, thus we have to take care about potential null
            if (oidField != null)
            {
                dataTableOIDOrdinal = dataTable.Columns.Add(oidField.FieldName, typeof (FieldTypeOID)).Ordinal;

                //and to make it easier add the oidField to the rest of the fields
                // it is important to take care of the order of the columns! later we depend if we add data
                listOfFields.Insert(0, oidField);

                if (oidField.FieldName.Equals("FID"))
                    isOIDFieldNameFID = true;

                //make the oid column the first column
                dataTable.Columns[dataTableOIDOrdinal].SetOrdinal(0);
                //should be 0...
                dataTableOIDOrdinal = 0;
            }

            //get data for rows and store it in a list of list like a table
            IList<IList<double>> tableData = new List<IList<double>>();

            bool isOIDFieldStartsByZero = false;

            int expectedNumbersOfRow = 0;
            foreach (Field currentField in listOfFields)
            {
                IList<double> column = GetValuesOfField(currentField);
                tableData.Add(column);

                //each column has the data of all rows in the column
                expectedNumbersOfRow = column.Count;

                if (currentField.IsOID && column.Contains(0d))
                    isOIDFieldStartsByZero = true;
            }

            // add rows
            // the table data list has the same order as the datatable columns!
            for (int i = 0; i < expectedNumbersOfRow; i++)
            {
                DataRow row = dataTable.NewRow();

                for (int y = 0; y < tableData.Count; y++)
                {
                    //we have the oid column
                    if (y == dataTableOIDOrdinal)
                    {
                        //if the column is FID we have to change the ids 0...x to 1...x+1 because this change will be made after the featureclass is copied into the in memory workspace
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

        public IList<double> GetValuesOfField(Field field)
        {
            IList<double> listOfValuesFromField = new List<double>();

            //this should be always the case, as we have only fields from feature feature
            IFeatureLayer2 featureLayer = field.Feature.FeatureLayer;

            using (var comReleaser = new ComReleaser())
            {
                IFeatureCursor featureCursor = featureLayer.FeatureClass.Search(null, true);

                comReleaser.ManageLifetime(featureCursor);

                int fieldIndex = featureLayer.FeatureClass.FindField(field.FieldName);

                IFeature currentFeature;
                while ((currentFeature = featureCursor.NextFeature()) != null)
                {
                    //we have to cast explicitly ... https://connect.microsoft.com/VisualStudio/feedback/details/534288/ilist-dynamic-cannot-call-a-method-add-without-casting
                    listOfValuesFromField.Add(Convert.ToDouble(currentFeature.get_Value(fieldIndex)));
                }
            }

            return listOfValuesFromField;
        }

        public IList<IField> GetListOfFieldsFromFeatureClass(IFeatureClass featureClass)
        {
            IList<IField> fieldsList = new List<IField>();

            IFields fields = featureClass.Fields;

            for (int i = 0; i <= fields.FieldCount - 1; i++)
            {
                if (fields.get_Field(i).Type <= esriFieldType.esriFieldTypeDouble)
                    fieldsList.Add(fields.get_Field(i));
            }

            return fieldsList;
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
                return;

            ArcMap.Document.ActiveView.FocusMap.AddLayer((ILayer)feature.ESRILayer);
        }

        public void RemoveLink(AbstractToolTemplate tool)
        {
            Feature mcdaWorkspaceContainer;

            if (!_dictionaryOfLinks.TryGetValue(tool, out mcdaWorkspaceContainer))
                return;

            _dictionaryOfLinks.Remove(tool);

            PropertyChanged.Notify(() => LinkDictionary);
        }

        public void EstablishLink(AbstractToolTemplate tool)
        {
            IFeatureLayer2 featureLayer = AvailableFeatures.Where(l => l.IsSelected && l.IsSuitableForMCDA).ToList().First().FeatureLayer;
            IFeatureClass featureClass = featureLayer.FeatureClass;

            IFeatureClass copiedFeatureClass = CopyFeatureClassIntoNewWorkspace(featureClass, _shadowWorkspace,
                tool + CreateTimeStamp());

            //MCDA.Model.Feature feature = new MCDAWorkspaceContainer(tool, copiedFeatureClass);

            //IFeatureClass featureClass = feature.FeatureClass;

            //create feature feature to display the result on a map
            var newFeatureLayer = new FeatureLayerClass();
            //newFeatureLayer.FeatureClass = feature.FeatureClass;

            newFeatureLayer.Name = CreateLayerName(tool);

            //feature.FeatureLayer = newFeatureLayer;

            var feature = new Feature(copiedFeatureClass, newFeatureLayer);

            _dictionaryOfLinks.Add(tool, feature);

            PropertyChanged.Notify(() => LinkDictionary);

            //return feature;
        }

        private String CreateLayerName(AbstractToolTemplate tool)
        {
            return tool + DateTime.Now.ToString("ddHHmmssffff");
        }

        private String CreateTimeStamp()
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
                return;

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
            }

            using (var comReleaser = new ComReleaser())
            {
                var featureCursor = featureClass.Update(null, true);

                comReleaser.ManageLifetime(featureCursor);

                var esriFeature = featureCursor.NextFeature();

                var fieldIndex = featureClass.FindField(tool.DefaultResultColumnName);

                var oidIndex = featureClass.FindField(featureClass.OIDFieldName);

                while (esriFeature != null)
                {
                    var oid = Convert.ToInt32(esriFeature.get_Value(oidIndex));

                    var dataRows = dataTable.AsEnumerable().Where(dr => dr.Field<FieldTypeOID>(featureClass.OIDFieldName).OID == oid);

                    DataRow dRow = dataRows.FirstOrDefault();

                    esriFeature.set_Value(fieldIndex, dRow[tool.DefaultResultColumnName]);

                    esriFeature.Store();

                    esriFeature = featureCursor.NextFeature();
                }
            }

            if (feature.SelectedFieldForRendering != null)
                ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<RendererContainer, IFeatureLayer2>)Render, feature.SelectedFieldForRendering.RenderContainer, feature.FeatureLayer);
                //Render(feature.SelectedFieldForRendering.RenderContainer, feature.FeatureLayer);

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

            PartialRefresh(renderContainer, featureLayer);
        }

        /// <summary>
        ///     Performs a partial refresh on the feature feature in the in memory workspace.
        /// </summary>
        /// <param name="renderContainer"></param>
        private void PartialRefresh(RendererContainer renderContainer, IFeatureLayer2 featureLayer)
        {
            var activeView = (IActiveView) ArcMap.Document.FocusMap;

            activeView.ContentsChanged();
            ArcMap.Document.UpdateContents();

            activeView.PartialRefresh(esriViewDrawPhase.esriViewGeography, featureLayer, null);
        }

        #region private ArcObjects stuff

        private IList<Feature> GetListOfLayerFromActiveView(IActiveView activeView)
        {
            IList<Feature> layerList = new List<Feature>();

            IMap map = activeView.FocusMap;

            // Get the number of layers
            int numberOfLayers = map.LayerCount;

            // Loop through the layers and get the correct feature index
            for (int i = 0; i < numberOfLayers; i++)
                layerList.Add(new Feature( (ILayer2) map.get_Layer(i)));

            return layerList;
        }

        private IList<Field> GetListOfFieldsFromToolParameter<T>(IList<T> toolParameter) where T : IToolParameter
        {
            IList<Field> listOfFields = new List<Field>();

            foreach (IToolParameter currentToolParameter in toolParameter)
            {
                foreach (Feature currentAvailableLayer in AvailableFeatures.Where(l => l.IsSelected))
                {
                    foreach (
                        Field currentField in
                            currentAvailableLayer.Fields.Where(f => f.FieldName.Equals(currentToolParameter.ColumnName))
                        )
                        listOfFields.Add(currentField);
                }
            }

            return listOfFields;
        }

        /// <summary>
        ///     If the user adds or removes items from the TOC the method removes or add the changed items to the available feature
        ///     list.
        /// </summary>
        /// <param name="activeView"></param>
        //private void RefreshAvailableLayerListAfterAddOrDelete(IActiveView activeView)
        //{
        //    if (listOfAvailableFeatures.Any())
        //        return;

        //    IList<Feature> layerList = new List<Feature>();

        //    IMap map = activeView.FocusMap;

        //    // Get the number of layers
        //    int numberOfLayers = map.LayerCount;

        //    IList<ILayer> newLayerList = new List<ILayer>();

        //    // Loop through the layers and get the correct feature index
        //    for (int i = 0; i < numberOfLayers; i++)
        //    {
        //        newLayerList.Add(map.get_Layer(i));
        //    }

        //    //remove
        //    for (int i = listOfAvailableFeatures.Count - 1; i >= 0; i--)
        //    {
        //        if (!newLayerList.Any(l => l == listOfAvailableFeatures[i].ESRILayer))
        //            listOfAvailableFeatures.RemoveAt(i);
        //    }

        //    //add
        //    foreach (ILayer currentNewLayer in newLayerList)
        //    {
        //        //is the new feature part in the mcda feature list?
        //        if (!listOfAvailableFeatures.Any(l => l.ESRILayer == currentNewLayer))
        //            listOfAvailableFeatures.Add(new Feature(currentNewLayer));
        //    }

        //    //and do not forget to register the new feature
        //    RegisterListenerForEveryMemberOfListOfAvailableLayer();

        //    PropertyChanged.Notify(() => AvailableFeatures);
        //}

        private IWorkspace CreateInMemoryWorkspace()
        {
            IWorkspaceFactory newWorkspaceFactory = new InMemoryWorkspaceFactoryClass();
            IWorkspaceName workspaceName = newWorkspaceFactory.Create("", "MCDAWorkspace", null, 0);
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
            IWorkspace inWorkspace = inDataset.Workspace;

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
            // Check enumFieldError for field naming confilcts

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