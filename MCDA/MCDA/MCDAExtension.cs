using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Carto;
using System.ComponentModel;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.ADF.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;
using System.Linq;
using MCDA.Extensions;
using MCDA.Model;
using System.Data;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS;
using ESRI.ArcGIS.ADF;
using MCDA.ViewModel;
using System.Diagnostics.Contracts;

namespace MCDA
{

    internal sealed class MCDAExtension : ESRI.ArcGIS.Desktop.AddIns.Extension, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private static MCDAExtension extension;

        private IList<Model.Feature> listOfAvailableLayer = new List<Model.Feature>();
        private IList<String> _listOfSelectedUniqueLayerNamesForPersistence = new List<string>();

        private IActiveViewEvents_Event activeViewEvents;

        private IDictionary<AbstractToolTemplate, MCDA.Model.Feature> _dictionaryOfLinks = new Dictionary<AbstractToolTemplate, MCDA.Model.Feature>();

        private IWorkspace shadowWorkspace;

        #region properties

        /// <summary>
        /// If non is selected the property returns <code>null</code>.
        /// </summary>
        public MCDA.Model.Feature SelectedFeature
        {
            get { return listOfAvailableLayer.FirstOrDefault(l => l.IsSelected); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="MCDA.Model.Feature"/> that are available in the context of the add-in.
        /// </summary>
        /// <remarks>
        /// It is not guaranteed that the features have a <see cref="IFeatureLayer2"/>, a <see cref="FeatureClass"/> or both. It is also not guaranteed
        /// that they provide a topological operator. Thus, not all of the <see cref="MCDA.Model.Feature"/> in this list are eligible for MCDA.
        /// </remarks>
        public IList<Model.Feature> AvailableFeatures
        {
            get { return listOfAvailableLayer.OrderBy(f => f.LayerName).ToList(); }
            set { PropertyChanged.ChangeAndNotify(ref listOfAvailableLayer, value, () => AvailableFeatures); }
        }

        /// <summary>
        /// Gets a list of <see cref="MCDA.Model.Feature"/> that are eligible for MCDA.
        /// </summary>
        public IList<Model.Feature> EligibleFeaturesForMCDA
        {
            get { return AvailableFeatures.Where(l => l.IsSuitableForMCDA).ToList().OrderBy(f => f.LayerName).ToList(); }
        }

        /// <summary>
        /// Gets all <see cref="MCDA.Model.Feature"/> from the in-memory workspace.
        /// </summary>
        public IList<MCDA.Model.Feature> FeaturesFromInMemoryWorkspace
        {
            get { return _dictionaryOfLinks.Values.ToList(); }
        }

        public IDictionary<AbstractToolTemplate, MCDA.Model.Feature> LinkDictionary
        {
            get { return _dictionaryOfLinks; }
        }
        #endregion

        #region event handling for listOfAvailableLayer
        private void RegisterListenerForEveryMemberOfListOfAvailableLayer()
        {
            //unregister for all member to avoid unnecessary multiple call
            foreach (var currentAvailableLayer in listOfAvailableLayer)
                currentAvailableLayer.PropertyChanged -= new PropertyChangedEventHandler(AvailalbeLayerListMemberPropertyChanged);

            foreach (var currentAvailableLayer in listOfAvailableLayer)
                currentAvailableLayer.PropertyChanged += new PropertyChangedEventHandler(AvailalbeLayerListMemberPropertyChanged);
        }

        /// <summary>
        /// A single feature in the list of <see cref="AvailableFeatures"/> has changed.
        /// This method also ensures that only one feature is selected at every time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AvailalbeLayerListMemberPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach(var currentAvailableLayer in listOfAvailableLayer.Where(l => MCDA.Model.Feature.LastSelectedLayer != l))
                currentAvailableLayer.IsSelected = false;

            PropertyChanged.Notify(() => AvailableFeatures);
        }
        #endregion


        #region extension related methods
        public static MCDAExtension GetExtension()
        {
            if (extension == null)
            {
                UID id = new UIDClass();

                id.Value = ThisAddIn.IDs.MCDAExtension;
                ArcMap.Application.FindExtensionByCLSID(id);
            }

            return extension;
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();

            extension = null;
        }

        protected override void OnStartup()
        {
            extension = this;

            shadowWorkspace = CreateInMemoryWorkspace();

            IMap map = ArcMap.Document.ActiveView.FocusMap;

            activeViewEvents = map as IActiveViewEvents_Event;

            activeViewEvents.ItemAdded += new IActiveViewEvents_ItemAddedEventHandler(ArcMapItemAdded);
            activeViewEvents.ItemDeleted += new IActiveViewEvents_ItemDeletedEventHandler(ArcMapItemDeleted);

            ArcMap.Events.NewDocument += new ESRI.ArcGIS.ArcMapUI.IDocumentEvents_NewDocumentEventHandler(EventsNewDocument);
            //ArcMap.Events.BeforeCloseDocument += new ESRI.ArcGIS.ArcMapUI.IDocumentEvents_BeforeCloseDocumentEventHandler(EventsBeforeCloseDocument); 
            ArcMap.Events.OpenDocument += new ESRI.ArcGIS.ArcMapUI.IDocumentEvents_OpenDocumentEventHandler(EventsOpenDocument);

        }
        #endregion

        #region events
        bool EventsBeforeCloseDocument()
        {
            // Return true to stop document from closing
            ESRI.ArcGIS.Framework.IMessageDialog msgBox = new ESRI.ArcGIS.Framework.MessageDialogClass();
            return msgBox.DoModal("BeforeCloseDocument Event", "Abort closing?", "Yes", "No", ArcMap.Application.hWnd);

        }
        void EventsNewDocument()
        {
            activeViewEvents.ItemAdded -= new IActiveViewEvents_ItemAddedEventHandler(ArcMapItemAdded);
            activeViewEvents.ItemDeleted -= new IActiveViewEvents_ItemDeletedEventHandler(ArcMapItemDeleted);

            IMap map = ArcMap.Document.ActiveView.FocusMap;

            activeViewEvents = map as IActiveViewEvents_Event;

            activeViewEvents.ItemAdded += new IActiveViewEvents_ItemAddedEventHandler(ArcMapItemAdded);
            activeViewEvents.ItemDeleted += new IActiveViewEvents_ItemDeletedEventHandler(ArcMapItemDeleted);

            AvailableFeatures = new List<Model.Feature>();

        }

        void EventsOpenDocument()
        {
            activeViewEvents.ItemAdded -= new IActiveViewEvents_ItemAddedEventHandler(ArcMapItemAdded);
            activeViewEvents.ItemDeleted -= new IActiveViewEvents_ItemDeletedEventHandler(ArcMapItemDeleted);

            IMap map = ArcMap.Document.ActiveView.FocusMap;

            activeViewEvents = map as IActiveViewEvents_Event;

            activeViewEvents.ItemAdded += new IActiveViewEvents_ItemAddedEventHandler(ArcMapItemAdded);
            activeViewEvents.ItemDeleted += new IActiveViewEvents_ItemDeletedEventHandler(ArcMapItemDeleted);

            AvailableFeatures = new List<Model.Feature>();
        }

        void ArcMapItemDeleted(object Item)
        {
            RefreshAvailableLayerListAfterAddOrDelete(ArcMap.Document.ActiveView);

            PropertyChanged.Notify(() => AvailableFeatures);
        }

        private void ArcMapItemAdded(object item)
        {
            RefreshAvailableLayerListAfterAddOrDelete(ArcMap.Document.ActiveView);

            PropertyChanged.Notify(() => AvailableFeatures);
        }
        #endregion

        public ToolParameterContainer GetToolParameterBasedOnSelectedFields()
        {
            IList<IToolParameter> toolParameter = new List<IToolParameter>();

            foreach (var currentAvailableLayer in AvailableFeatures.Where(l => l.IsSelected))
            {
                foreach (var currentField in currentAvailableLayer.Fields.Where(f => f.IsSelected && f.IsNumeric))
		            toolParameter.Add(new ToolParameter(currentField.FieldName));
            }

            return new ToolParameterContainer(toolParameter);
        }

        /// <summary>
        /// Returns the field of the OID for the selected feature. However, it is possible that no feature is selected
        /// or that no OID exists or both. In this the methods returns null.
        /// </summary>
        /// <returns></returns>
        public Model.Field GetOIDFieldFromSelectedFeature()
        {
            Model.Feature selectedFeature = listOfAvailableLayer.FirstOrDefault(f => f.IsSelected);

            if (selectedFeature == null)
                return null;

            return selectedFeature.Fields.FirstOrDefault(f => f.IsOID);

        }

        /// <summary>
        /// The method checks if the preferred name for the result column is already in use and if so 
        /// returns a non used by adding a number.
        /// If the name is not used it returns the preferred name.
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

        public IList<Model.Field> GetFieldsFromSelectedLayerWhichAreNumeric(IList<Model.Feature> layer)
        {
            IList<Model.Field> fieldList = new List<Model.Field>();

            foreach(var currentLayer in layer.Where(l => l.IsSelected)){

                foreach(var currentField in currentLayer.Fields.Where(f => f.IsNumeric))
                    fieldList.Add(currentField);
                
            }

            return fieldList;
        }

        public DataTable GetDataTableForParameterSet<T>(IList<T> toolParameter) where T : IToolParameter
        {
            IList<Model.Field> listOfFields = GetListOfFieldsFromToolParameter(toolParameter);

            DataTable dataTable = new DataTable();

            //add columns
            foreach (Model.Field currentField in listOfFields)
                dataTable.Columns.Add(currentField.FieldName, typeof(double));

            //add the oid column
            Model.Field oidField = GetOIDFieldFromSelectedFeature();

            //we have to take care about FID oid fields http://gis.stackexchange.com/questions/40833/arcobjects-bug-in-oid-field-duplication-while-duplicating-feature-class
            bool isOIDFieldNameFID = false;

            int dataTableOIDOrdinal = 0;
            //in fact no feature can be selected, thus we have to take care about potential null
            if (oidField != null)
            {
                dataTableOIDOrdinal = dataTable.Columns.Add(oidField.FieldName, typeof(FieldTypeOID)).Ordinal;

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
            foreach (Model.Field currentField in listOfFields)
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
                            row[y] = new FieldTypeOID() { OID = (int)tableData[y][i] + 1 };
                            continue;
                        }

                        row[y] = new FieldTypeOID() { OID = (int)tableData[y][i] };
                        continue;
                    }

                    row[y] = tableData[y][i];
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        public IList<double> GetValuesOfField(Model.Field field)
        {
            IList<double> listOfValuesFromField = new List<double>();

            //this should be always the case, as we have only fields from feature feature
            IFeatureLayer2 _featureLayer = field.Feature.FeatureLayer;

            using (ComReleaser comReleaser = new ComReleaser())
            {
                IFeatureCursor featureCursor = _featureLayer.FeatureClass.Search(null, true);

                comReleaser.ManageLifetime(featureCursor);

                int fieldIndex = _featureLayer.FeatureClass.FindField(field.FieldName);

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

        #region private ArcObjects stuff

        private IList<Model.Feature> GetListOfLayerFromActiveView(ESRI.ArcGIS.Carto.IActiveView activeView)
        {
            IList<Model.Feature> layerList = new List<Model.Feature>();

            ESRI.ArcGIS.Carto.IMap map = activeView.FocusMap;

            // Get the number of layers
            int numberOfLayers = map.LayerCount;

            // Loop through the layers and get the correct feature index
            for (int i = 0; i < numberOfLayers; i++)
                layerList.Add(new Model.Feature(map.get_Layer(i)));

            return layerList;
        }

        private IList<Model.Field> GetListOfFieldsFromToolParameter<T>(IList<T> toolParameter) where T : IToolParameter
        {
            IList<Model.Field> listOfFields = new List<Model.Field>();

            foreach (IToolParameter currentToolParameter in toolParameter)
            {
                foreach(var currentAvailableLayer in AvailableFeatures.Where(l => l.IsSelected)){

                    foreach(var currentField in currentAvailableLayer.Fields.Where(f => f.FieldName.Equals(currentToolParameter.ColumnName)))
                        listOfFields.Add(currentField);                
                } 
            }

            return listOfFields;
        }

        /// <summary>
        /// If the user adds or removes items from the TOC the method removes or add the changed items to the available feature list.
        /// </summary>
        /// <param name="activeView"></param>
        private void RefreshAvailableLayerListAfterAddOrDelete(ESRI.ArcGIS.Carto.IActiveView activeView)
        {
            if (listOfAvailableLayer.Any())
                return;

            IList<Model.Feature> layerList = new List<Model.Feature>();

            ESRI.ArcGIS.Carto.IMap map = activeView.FocusMap;

            // Get the number of layers
            int numberOfLayers = map.LayerCount;

            IList<ILayer> newLayerList = new List<ILayer>();

            // Loop through the layers and get the correct feature index
            for (int i = 0; i < numberOfLayers; i++)
            {
                newLayerList.Add(map.get_Layer(i));
            }

            //remove
            for (int i = listOfAvailableLayer.Count - 1; i >= 0; i--)
            {
                if (!newLayerList.Any(l => l == listOfAvailableLayer[i].ESRILayer))
                    listOfAvailableLayer.RemoveAt(i);
            }

            //add
            foreach (ILayer currentNewLayer in newLayerList)
            {
                //is the new feature part in the mcda feature list?
                if (!listOfAvailableLayer.Any(l => l.ESRILayer == currentNewLayer))
                    listOfAvailableLayer.Add(new Model.Feature(currentNewLayer));
            }

            //and do not forget to register the new feature
            RegisterListenerForEveryMemberOfListOfAvailableLayer();

            PropertyChanged.Notify(() => AvailableFeatures);
        }

        private IWorkspace CreateInMemoryWorkspace()
        {
            IWorkspaceFactory newWorkspaceFactory = new InMemoryWorkspaceFactoryClass();
            IWorkspaceName workspaceName = newWorkspaceFactory.Create("", "MCDAWorkspace", null, 0);
            IName name = (IName) workspaceName;
            IWorkspace inMemoryWorkspace = (IWorkspace)name.Open();

            return inMemoryWorkspace;
        }

        private IFeatureClass CopyFeatureClassIntoNewWorkspace(IFeatureClass inFeatureClass, IWorkspace outWorkspace, string newName)
        {
            // get FeatureClassName for input
            IDataset inDataset = inFeatureClass as IDataset;
            IFeatureClassName inFeatureClassName = inDataset.FullName as IFeatureClassName;
            IWorkspace inWorkspace = inDataset.Workspace;

            // get WorkSpaceName for output
            IDataset outDataset = outWorkspace as IDataset;
            IWorkspaceName outWorkspaceName = outDataset.FullName as IWorkspaceName;

            // Create new FeatureClassName
            IFeatureClassName outFeatureClassName = new FeatureClassNameClass();
            // Assign it a name and a workspace
            IDatasetName datasetName = outFeatureClassName as IDatasetName;
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

            IFeatureWorkspace pFeatureWS = (IFeatureWorkspace)outWorkspace;

            return pFeatureWS.OpenFeatureClass(newName);
        }
        #endregion

        /// <summary>
        /// In case the tool has a corresponding <see cref="IFeatureLayer2"/> in the in memory workspace it will be added
        /// to the active view.
        /// </summary>
        /// <param name="tool"></param>
        public void DisplayLink(AbstractToolTemplate tool)
        {
            MCDA.Model.Feature feature;

            if (!_dictionaryOfLinks.TryGetValue(tool, out feature))
                return;

            ArcMap.Document.ActiveView.FocusMap.AddLayer(feature.ESRILayer);
        }

        public void RemoveLink(AbstractToolTemplate tool)
        {
            MCDA.Model.Feature mcdaWorkspaceContainer;

            if (!_dictionaryOfLinks.TryGetValue(tool, out mcdaWorkspaceContainer))
                return;

            _dictionaryOfLinks.Remove(tool);

            PropertyChanged.Notify(() => LinkDictionary);
        }

        public void EstablishLink(AbstractToolTemplate tool)
        {
            IFeatureLayer2 featureLayer = EligibleFeaturesForMCDA.Where(l => l.IsSelected).ToList().First().FeatureLayer;
            IFeatureClass featureClass = featureLayer.FeatureClass;

            IFeatureClass copiedFeatureClass = CopyFeatureClassIntoNewWorkspace(featureClass, shadowWorkspace, tool.ToString() + CreateTimeStamp());

            //MCDA.Model.Feature feature = new MCDAWorkspaceContainer(tool, copiedFeatureClass);

            //IFeatureClass featureClass = feature.FeatureClass;

            //create feature feature to display the result on a map
            FeatureLayerClass newFeatureLayer = new FeatureLayerClass();
            //newFeatureLayer.FeatureClass = feature.FeatureClass;

            newFeatureLayer.Name = CreateLayerName(tool);

            //feature.FeatureLayer = newFeatureLayer;

            MCDA.Model.Feature feature = new Model.Feature(copiedFeatureClass, newFeatureLayer);

            _dictionaryOfLinks.Add(tool, feature);

            PropertyChanged.Notify(() => LinkDictionary);

            //return feature;
        }

        private String CreateLayerName(AbstractToolTemplate tool)
        {
            return tool.ToString() + DateTime.Now.ToString("ddHHmmssffff");
        }

        private String CreateTimeStamp()
        {
            return DateTime.Now.ToString("ddHHmmssffff");
        }

        /// <summary>
        /// Joins the result table of the data table into the feature class of the tool by using the oid/fid.
        /// Afterwards the method updates the view with the new result. This also includes the update of the class breaks or bi polar renderer if set.
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="dataTable"></param>
        public void JoinToolResultByOID(AbstractToolTemplate tool, DataTable dataTable)
        {

            MCDA.Model.Feature feature;
            if (!_dictionaryOfLinks.TryGetValue(tool, out feature))
                return;

            IFeatureClass featureClass = feature.FeatureClass;

            if (featureClass.FindField(tool.DefaultResultColumnName) < 0)
            {

                IField newField = new FieldClass();
                IFieldEdit newFieldEdit = (IFieldEdit)newField;
                newFieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
                newFieldEdit.Name_2 = tool.DefaultResultColumnName;
                newFieldEdit.AliasName_2 = tool.DefaultResultColumnName;

                featureClass.AddField(newField);
            }

            using (ComReleaser comReleaser = new ComReleaser())
            {
                IFeatureCursor featureCursor = featureClass.Update(null, true);

                comReleaser.ManageLifetime(featureCursor);

                IFeature esriFeature = featureCursor.NextFeature();

                int fieldIndex = featureClass.FindField(tool.DefaultResultColumnName);

                int oidIndex = featureClass.FindField(featureClass.OIDFieldName);

                while (esriFeature != null)
                {
                    int oid = Convert.ToInt32(esriFeature.get_Value(oidIndex));
                    EnumerableRowCollection<DataRow> dataRows = dataTable.AsEnumerable().Where(dr => dr.Field<FieldTypeOID>(featureClass.OIDFieldName).OID == oid);

                    DataRow dRow = dataRows.FirstOrDefault();

                    esriFeature.set_Value(fieldIndex, dRow[tool.DefaultResultColumnName]);

                    esriFeature.Store();

                    esriFeature = featureCursor.NextFeature();
                }

            }

            if(feature.SelectedFieldForRendering != null)
                Render(feature.SelectedFieldForRendering.RenderContainer, feature.FeatureLayer);

            PropertyChanged.Notify(() => LinkDictionary);
        }

        /// <summary>
        /// If the optional <see cref="MCDA.Model.ClassBreaksRendererContainer"/> is set and includes all properties the method
        /// creates a new class breaks renderer and performs a partial refresh.
        /// </summary>
        /// <param name="renderContainer"></param>
        /// <param name="featureLayer"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void Render(RendererContainer renderContainer, IFeatureLayer2 featureLayer)
        {
            Contract.Requires<ArgumentNullException>(renderContainer != null);
            Contract.Requires<ArgumentNullException>(featureLayer != null);

            IGeoFeatureLayer geoFeatureLayer = featureLayer as IGeoFeatureLayer;

            //Contract.Requires<ArgumentException>(geoFeatureLayer != null);

            switch (renderContainer.Renderer)
            {
                case Renderer.ClassBreaksRenderer: geoFeatureLayer.Renderer = RendererFactory.NewClassBreaksRenderer(renderContainer);
                    break;
                case Renderer.BiPolarRenderer: geoFeatureLayer.Renderer = RendererFactory.NewUniqueValueRenderer(renderContainer);
                    break;
                case Renderer.None: geoFeatureLayer.Renderer = RendererFactory.NewSimpleRenderer();
                    break;
            }

            PartialRefresh(renderContainer, featureLayer);
        }

        /// <summary>
        /// Performs a partial refresh on the feature feature in the in memory workspace.
        /// </summary>
        /// <param name="renderContainer"></param>
        private void PartialRefresh(RendererContainer renderContainer, IFeatureLayer2 featureLayer)
        {
            IActiveView activeView = (IActiveView)ArcMap.Document.FocusMap;

            activeView.ContentsChanged();
            ArcMap.Document.UpdateContents();

            activeView.PartialRefresh(esriViewDrawPhase.esriViewGeography, featureLayer, null);
        }
    }
}
