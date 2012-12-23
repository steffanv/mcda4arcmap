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

namespace MCDA
{
    /// <summary>
    /// The s
    /// </summary>
    public class MCDAExtension : ESRI.ArcGIS.Desktop.AddIns.Extension, INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private static MCDAExtension _extension;

        private IList<Model.Layer> _listOfAvailableLayer = new List<Model.Layer>();
        private IList<String> _listOfSelectedUniqueLayerNamesForPersistence = new List<string>();

        private IActiveViewEvents_Event _activeViewEvents;

        private IDictionary<AbstractToolTemplate,MCDAWorkspaceContainer> _dictionaryOfLinks= new Dictionary<AbstractToolTemplate,MCDAWorkspaceContainer>();

        private IWorkspace _shadowWorkspace;

        #region properties

        /// <summary>
        /// If non is selected the property returns null.
        /// </summary>
        public Layer SelectedLayer
        {
            get { return _listOfAvailableLayer.FirstOrDefault(l => l.IsSelected); }
        }

        public IList<Model.Layer> AvailableLayer
        {
            get { return _listOfAvailableLayer.OrderBy(f => f.LayerName).ToList(); }
            set { PropertyChanged.ChangeAndNotify(ref _listOfAvailableLayer, value, () => AvailableLayer); /*RegisterListenerForEveryMemberOfListOfAvailableLayer();*/ }
        }

        public IList<Model.Layer> AvailableFeatureLayer
        {
            get { return AvailableLayer.Where(l => l.IsFeatureLayer).ToList().OrderBy(f => f.LayerName).ToList(); }
        }

        public IDictionary<AbstractToolTemplate, MCDAWorkspaceContainer> LinkDictionary
        {
            get { return _dictionaryOfLinks; }
        }
        #endregion

        #region eventhandling for _listOfAvailableLayer
        private void RegisterListenerForEveryMemberOfListOfAvailableLayer()
        {
            //unregister for all member to avoid unessary multiple call
            _listOfAvailableLayer.ForEach(l => l.PropertyChanged -= new PropertyChangedEventHandler(AvailalbeLayerListMemberPropertyChanged));
            _listOfAvailableLayer.ForEach(l => l.PropertyChanged +=new PropertyChangedEventHandler(AvailalbeLayerListMemberPropertyChanged));
        }

        /// <summary>
        /// A single layer in the list of "AvailableLayer" has changed.
        /// This method also ensures that only one layer is selected at every time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AvailalbeLayerListMemberPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _listOfAvailableLayer.Where(l => Layer.LastSelectedLayer != l).ForEach(l => l.IsSelected = false);

            PropertyChanged.Notify(() => AvailableLayer);
        }
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
            _extension = this;

            _shadowWorkspace = CreateInMemoryWorkspace();

            IMap map = ArcMap.Document.ActiveView.FocusMap;

            _activeViewEvents = map as IActiveViewEvents_Event;

            _activeViewEvents.ItemAdded += new IActiveViewEvents_ItemAddedEventHandler(ArcMap_ItemAdded);
            _activeViewEvents.ItemDeleted += new IActiveViewEvents_ItemDeletedEventHandler(ArcMap_ItemDeleted);

            ArcMap.Events.NewDocument += new ESRI.ArcGIS.ArcMapUI.IDocumentEvents_NewDocumentEventHandler(Events_NewDocument);
            //ArcMap.Events.BeforeCloseDocument += new ESRI.ArcGIS.ArcMapUI.IDocumentEvents_BeforeCloseDocumentEventHandler(Events_BeforeCloseDocument); 
            ArcMap.Events.OpenDocument += new ESRI.ArcGIS.ArcMapUI.IDocumentEvents_OpenDocumentEventHandler(Events_OpenDocument);
  
        }
        #endregion

        #region events
        bool Events_BeforeCloseDocument()
        {
                // Return true to stop document from closing
                ESRI.ArcGIS.Framework.IMessageDialog msgBox = new ESRI.ArcGIS.Framework.MessageDialogClass();
                return msgBox.DoModal("BeforeCloseDocument Event", "Abort closing?", "Yes", "No", ArcMap.Application.hWnd);
           
        }
        void Events_NewDocument()
        {
            _activeViewEvents.ItemAdded -= new IActiveViewEvents_ItemAddedEventHandler(ArcMap_ItemAdded);
            _activeViewEvents.ItemDeleted -= new IActiveViewEvents_ItemDeletedEventHandler(ArcMap_ItemDeleted);

            IMap map = ArcMap.Document.ActiveView.FocusMap;

            _activeViewEvents = map as IActiveViewEvents_Event;

            _activeViewEvents.ItemAdded += new IActiveViewEvents_ItemAddedEventHandler(ArcMap_ItemAdded);
            _activeViewEvents.ItemDeleted += new IActiveViewEvents_ItemDeletedEventHandler(ArcMap_ItemDeleted);

            AvailableLayer = new List<Model.Layer>();

        }

        void Events_OpenDocument()
        {
            _activeViewEvents.ItemAdded -= new IActiveViewEvents_ItemAddedEventHandler(ArcMap_ItemAdded);
            _activeViewEvents.ItemDeleted -= new IActiveViewEvents_ItemDeletedEventHandler(ArcMap_ItemDeleted);

            IMap map = ArcMap.Document.ActiveView.FocusMap;

            _activeViewEvents = map as IActiveViewEvents_Event;

            _activeViewEvents.ItemAdded += new IActiveViewEvents_ItemAddedEventHandler(ArcMap_ItemAdded);
            _activeViewEvents.ItemDeleted += new IActiveViewEvents_ItemDeletedEventHandler(ArcMap_ItemDeleted);

            AvailableLayer = new List<Model.Layer>();
        }

        void ArcMap_ItemDeleted(object Item)
        {
           
            RefreshAvailableLayerListAfterAddOrDelete(ArcMap.Document.ActiveView);

            PropertyChanged.Notify(() => AvailableLayer);

        }

        private void ArcMap_ItemAdded(object item)
        {
            RefreshAvailableLayerListAfterAddOrDelete(ArcMap.Document.ActiveView);

            PropertyChanged.Notify(() => AvailableLayer);
        }
        #endregion

        public ToolParameterContainer GetToolParameterBasedOnSelectedFields()
        {
            IList<IToolParameter> toolParameter = new List<IToolParameter>();
            
            AvailableLayer.Where(l => l.IsSelected).ForEach(l => l.Fields.Where(f => f.IsSelected && f.IsNumber).ForEach(f => toolParameter.Add(new WLCToolParameter(f.FieldName))));

            return new ToolParameterContainer(toolParameter);
        }

        /// <summary>
        /// Returns the field of the OID for the selected feature. However, it is possible that no feature is selected
        /// or that no OID exists or both. In this the methods retuns null.
        /// </summary>
        /// <returns></returns>
        public Model.Field GetOIDFieldFromSelectedFeature()
        {

            Model.Layer selectedFeature = _listOfAvailableLayer.FirstOrDefault(f => f.IsSelected);

            if (selectedFeature == null)
                return null;

            return selectedFeature.Fields.FirstOrDefault(f => f.IsOID);

        }

        /// <summary>
        /// The method checks if the preferred name for the result column is alreadz in use and if so 
        /// returns a non used by adding a number.
        /// If the name is not used it returns the preferred name.
        /// </summary>
        /// <param name="preferredName"></param>
        /// <returns></returns>
        public string GetSuggestNameForResultColumn(string preferredName)
        {
            if (SelectedLayer == null || !SelectedLayer.Fields.Any(f => f.FieldName.Equals(preferredName)))
                return preferredName;

            int extension = 0;

            while(SelectedLayer.Fields.Any(f => f.FieldName.Equals(preferredName)))
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

        public IList<Model.Field> GetFieldsFromSelectedLayerWhichAreNumeric(IList<Model.Layer> layer)
        {
            IList<Model.Field> fieldList = new List<Model.Field>();

            layer.Where(l => l.IsSelected).ForEach(l => l.Fields.Where(f => f.IsNumber).ForEach(f => fieldList.Add(f)));

            return fieldList;
        }

        public DataTable GetDataTableForParameterSet<T>(IList<T> toolParameter) where T : IToolParameter
        {
           IList<Model.Field> listOfFields = GetListOfFieldsFromToolParameter(toolParameter);

           DataTable dataTable = new DataTable();

            //add columns
           foreach (Model.Field currentField in listOfFields)
           {
               dataTable.Columns.Add(currentField.FieldName, typeof(double));
           }

            //add the oid column
            Model.Field oidField = GetOIDFieldFromSelectedFeature();

            //we have to take care about FID oid fields http://gis.stackexchange.com/questions/40833/arcobjects-bug-in-oid-field-duplication-while-duplicating-feature-class
            bool isOIDFieldNameFID = false;
            //in fact no feature can be selected, thus we have to take care about potential null
            if (oidField != null)
            {
                dataTable.Columns.Add(oidField.FieldName, typeof(FieldTypeOID));

                //and to make it easier add the oidField to the rest of the fields
                listOfFields.Add(oidField);

                if(oidField.FieldName.Equals("FID"))
                    isOIDFieldNameFID = true;
            }

            //get data for rows
           IList<IList<double>> tableData = new List<IList<double>>();

           int expectedNumbersOfRow = 0;
           foreach (Model.Field currentField in listOfFields)
           {
               IList<double> column = GetValuesOfField(currentField);
               tableData.Add(column);

               //each column has the data of all rows in the column
               expectedNumbersOfRow = column.Count;
           }

           bool isOIDFieldStartsByZero = false;

           //add row
           for (int i = 0; i < expectedNumbersOfRow; i++)
           {
               DataRow row = dataTable.NewRow();

               for (int y = 0; y < tableData.Count; y++)
               {
                   //we have the oid column
                   if (dataTable.Columns[y].DataType == typeof(FieldTypeOID))
                   {
                       //the first row
                       if (i == 0)
                       {
                           if((int)tableData[y][i] ==0)
                             isOIDFieldStartsByZero = true;
                       }
                       //if the column is FID we have to change the ids 0...x to 1...x+1 because this change will be made after the featureclass is copied into the in memory workspace
                       if (isOIDFieldNameFID && isOIDFieldStartsByZero)
                       {
                           row[y] = new FieldTypeOID() { OID = (int)tableData[y][i]+1 };
                           continue;
                       }

                       row[y] = new FieldTypeOID() { OID = (int) tableData[y][i] };
                         continue;
                   }

                   row[y] = tableData[y][i];
               }

               dataTable.Rows.Add(row);
           }

           return dataTable;

        }

        public IList<MCDAWorkspaceContainer> GetAllMCDAWorkspaceContainerFromShadowWorkspace()
        {
            return _dictionaryOfLinks.Values.ToList();
        }

        public IList<double> GetValuesOfField(Model.Field field)
        {
            IList<double> listOfValuesFromField = new List<double>();

            //this should be always the case, as we have only fields from feature layer
            IFeatureLayer2 _featureLayer = field.Layer.FeatureLayer;

            using (ComReleaser comReleaser = new ComReleaser())
            {
          
            IFeatureCursor featureCursor = _featureLayer.FeatureClass.Search(null, true);

            IFeature feature = featureCursor.NextFeature();

                int fieldIndex = feature.Table.FindField(field.FieldName);

                ICursor cursor = feature.Table.Search(null, true);

                IRow row = null;
            
                while ((row = cursor.NextRow()) != null)
                {
                    //we have to cast explicitly ... https://connect.microsoft.com/VisualStudio/feedback/details/534288/ilist-dynamic-cannot-call-a-method-add-without-casting
                    listOfValuesFromField.Add(Convert.ToDouble(row.get_Value(fieldIndex)));
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
                {
                    fieldsList.Add(fields.get_Field(i));
                }
            }

            return fieldsList;
        }

        #region private ArcObjects stuff

        private IList<Model.Layer>  GetListOfLayerFromActiveView(ESRI.ArcGIS.Carto.IActiveView activeView)
        {

            IList<Model.Layer> layerList = new List<Model.Layer>();

            ESRI.ArcGIS.Carto.IMap map = activeView.FocusMap;
           
            // Get the number of layers
            int numberOfLayers = map.LayerCount;

            // Loop through the layers and get the correct layer index
            for (System.Int32 i = 0; i < numberOfLayers; i++)
            {

                layerList.Add(new Model.Layer(map.get_Layer(i)));
            }
            
            return layerList;
        }

        private IList<Model.Field> GetListOfFieldsFromToolParameter<T>(IList<T> toolParameter) where T : IToolParameter
        {
            IList<Model.Field> listOfFields = new List<Model.Field>();

            foreach (IToolParameter currentToolParameter in toolParameter)
            {
                AvailableLayer.Where(l => l.IsSelected).ForEach(l => l.Fields.Where(f => f.FieldName.Equals(currentToolParameter.ColumnName)).ForEach(f => listOfFields.Add(f)));
            }

            return listOfFields;
        }

        /// <summary>
        /// If the user adds or removes items from the TOC the method removes or add the changed items to the available layer list.
        /// </summary>
        /// <param name="activeView"></param>
        private void RefreshAvailableLayerListAfterAddOrDelete(ESRI.ArcGIS.Carto.IActiveView activeView)
        {
            if (_listOfAvailableLayer == null)
                return;

            IList<Model.Layer> layerList = new List<Model.Layer>();

            ESRI.ArcGIS.Carto.IMap map = activeView.FocusMap;

            // Get the number of layers
            int numberOfLayers = map.LayerCount;

            IList<ILayer> newLayerList = new List<ILayer>();

            // Loop through the layers and get the correct layer index
            for (int i = 0; i < numberOfLayers; i++)
            {
                newLayerList.Add(map.get_Layer(i));
            }

            //remove
            for(int i = _listOfAvailableLayer.Count -1; i >= 0; i--)
            {
                if (!newLayerList.Any(l => l == _listOfAvailableLayer[i].ESRILayer))
                    _listOfAvailableLayer.RemoveAt(i);   
            }

            //add
            foreach (ILayer currentNewLayer in newLayerList)
            {
                //is the new layer part in the mcda layer list?
                if(!_listOfAvailableLayer.Any(l => l.ESRILayer == currentNewLayer))
                    _listOfAvailableLayer.Add(new Model.Layer(currentNewLayer));
            }

            //and do not forget to register the new layer
            RegisterListenerForEveryMemberOfListOfAvailableLayer();

            PropertyChanged.Notify(() => AvailableLayer);
        }

        private IWorkspace CreateInMemoryWorkspace()
        {
            IWorkspaceFactory newWorkspaceFactory = new InMemoryWorkspaceFactoryClass();
            IWorkspaceName wName = newWorkspaceFactory.Create("", "MCDAWorkspace", null, 0);
            IName name = (IName)wName;
            IWorkspace inmemWor = (IWorkspace)name.Open();

            return inmemWor;
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
        /// In case the tool has a corresponding feature class in the in memory workspace it will be added
        /// to the active view.
        /// </summary>
        /// <param name="tool"></param>
        public void DisplayLink(AbstractToolTemplate tool)
        {
            MCDAWorkspaceContainer mcdaWorkspaceContainer;
           
            if (!_dictionaryOfLinks.TryGetValue(tool, out mcdaWorkspaceContainer))
                return;

           ArcMap.Document.ActiveView.FocusMap.AddLayer(mcdaWorkspaceContainer.FeatureLayer);
        }

        public void RemoveLink(AbstractToolTemplate tool)
        {
            MCDAWorkspaceContainer mcdaWorkspaceContainer;
            if (!_dictionaryOfLinks.TryGetValue(tool, out mcdaWorkspaceContainer))
                return;

            _dictionaryOfLinks.Remove(tool);

            PropertyChanged.Notify(() => LinkDictionary);
        }

        public void EstablishLink(AbstractToolTemplate tool)
        {
            IFeatureLayer2 fl = AvailableFeatureLayer.Where(l => l.IsSelected).ToList().FirstOrDefault().FeatureLayer;
            IFeatureClass fc = fl.FeatureClass;

            IFeatureClass fcCopy = CopyFeatureClassIntoNewWorkspace(fc, _shadowWorkspace, tool.ToString() + CreateTimeStamp());

            MCDAWorkspaceContainer mcdaWorkspaceContainer = new MCDAWorkspaceContainer(tool, fcCopy);

            //IFeatureClass fc = mcdaWorkspaceContainer.FeatureClass;

            //createn feature layer to display the result on a map
            IFeatureLayer newfl = new FeatureLayerClass();
            newfl.FeatureClass = mcdaWorkspaceContainer.FeatureClass;

            newfl.Name = CreateLayerName(tool);

            mcdaWorkspaceContainer.FeatureLayer = newfl;

            _dictionaryOfLinks.Add(tool, mcdaWorkspaceContainer);

            PropertyChanged.Notify(() => LinkDictionary);

            //return fcCopy;
        }

        private String CreateLayerName(AbstractToolTemplate tool)
        {
            return tool.ToString() + DateTime.Now.ToString("MMddHHmmssffff");
        }

        private String CreateTimeStamp()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssffff");
        }

        /// <summary>
        /// Joins the result table of the data table into the feature class of the tool by using the oid/fid.
        /// Afterwards the method updates the view with the new result. This also includes the update of the class breaks renderer if set.
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="dataTable"></param>
        public void JoinToolResultByOID(AbstractToolTemplate tool, DataTable dataTable)
        {
            
            MCDAWorkspaceContainer mcdaWorkspaceContainer;
            if (!_dictionaryOfLinks.TryGetValue(tool, out mcdaWorkspaceContainer))
                return;

            IFeatureClass fc = mcdaWorkspaceContainer.FeatureClass;

            using (ComReleaser comReleaser = new ComReleaser())
            {

                if (fc.FindField(tool.DefaultResultColumnName) < 0)
                {

                    IField newField = new FieldClass();
                    IFieldEdit newFieldEdit = (IFieldEdit)newField;
                    newFieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
                    newFieldEdit.Name_2 = tool.DefaultResultColumnName;
                    newFieldEdit.AliasName_2 = tool.DefaultResultColumnName;

                    fc.AddField(newField);
                }

                // StartEditing(_shadowWorkspace);
                // _editor.StartOperation();

                IFeatureCursor featureCursor = fc.Update(null, true);

                IFeature feature = null;

                feature = featureCursor.NextFeature();

                int fieldIndex = fc.FindField(tool.DefaultResultColumnName);

                int oidIndex = fc.FindField(fc.OIDFieldName);

                while (feature != null)
                {

                    int oid = Convert.ToInt32(feature.get_Value(oidIndex));
                    EnumerableRowCollection<DataRow> dataRows = dataTable.AsEnumerable().Where(dr => dr.Field<FieldTypeOID>(fc.OIDFieldName).OID == oid);

                    DataRow dRow = dataRows.FirstOrDefault();

                    feature.set_Value(fieldIndex, dRow[tool.DefaultResultColumnName]);

                    feature.Store();
                    
                    feature = featureCursor.NextFeature();
                }

            }

           Render(mcdaWorkspaceContainer);

           PropertyChanged.Notify(() => LinkDictionary);
        }

        /// <summary>
        /// If the optional <see cref="MCDA.Model.ClassBreaksRendererContainer"/> is set and includes all properties the method
        /// creates a new class breaks renderer and performs a partial refresh.
        /// </summary>
        /// <param name="renderContainer"></param>
        public void Render(IRenderContainer renderContainer)
        {
            if (renderContainer != null)
            {
                IGeoFeatureLayer geoFeatureLayer = renderContainer.FeatureLayer as IGeoFeatureLayer;

                switch(renderContainer.Renderer){

                    case Renderer.ClassBreaksRenderer: geoFeatureLayer.Renderer = RendererFactory.NewClassBreaksRenderer( renderContainer);
                        break;
                    case Renderer.BiPolarRenderer: geoFeatureLayer.Renderer = RendererFactory.NewUniqueValueRenderer(renderContainer);
                        //one could argue that this has not to be done in the model, however, this is the place where the decision takes places
                        ProgressDialog.ShowProgressDialog("Creating Unique Renderer", PartialRefresh, renderContainer);
                        return;
                    case Renderer.None: geoFeatureLayer.Renderer = RendererFactory.NewSimpleRenderer();
                        break;
                }
                
                PartialRefresh(renderContainer);
            }
        }

        /// <summary>
        /// Performs a partial refresh on the feature layer in the in memory workspace.
        /// </summary>
        /// <param name="renderContainer"></param>
        private void PartialRefresh(IRenderContainer renderContainer)
        {  
            IActiveView av = (IActiveView)ArcMap.Document.FocusMap;

            av.ContentsChanged();
            ArcMap.Document.UpdateContents();

            av.PartialRefresh(esriViewDrawPhase.esriViewGeography, renderContainer.FeatureLayer, null);
 
        }

        #region workspace editing
        /*
        private bool StartEditing(ESRI.ArcGIS.Geodatabase.IWorkspace workspaceToEdit)
        {
            //Get a reference to the editor.
            UID uid = new UIDClass();
            uid.Value = "esriEditor.Editor";
            IEditor _editor = ArcMap.Application.FindExtensionByCLSID(uid) as IEditor;

            //Check to see if a workspace is already being edited.
            if (_editor.EditState == esriEditState.esriStateNotEditing)
            {
                _editor.StartEditing(workspaceToEdit);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void StopEditing(ESRI.ArcGIS.Geodatabase.IWorkspace workspaceToEdit)
        {
            _editor.StopEditing(true);
        }
        */
        #endregion

        #region persistence
        protected override void OnSave(Stream outStrm)
        { 
            var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            //persist only the unique layer names of selected layer
            _listOfSelectedUniqueLayerNamesForPersistence = _listOfAvailableLayer.Where(l => l.IsSelected).Select(l => l.UniqueLayerName).ToList();
             
            bf.Serialize(outStrm, _listOfSelectedUniqueLayerNamesForPersistence);
        }

        protected override void OnLoad(Stream inStrm)
        {
            var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            //bf.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;

            bf.Binder = new MCDADeserializationBinder();

            _listOfSelectedUniqueLayerNamesForPersistence = null;
            _listOfSelectedUniqueLayerNamesForPersistence = bf.Deserialize(inStrm) as List<string>;
        }

    }

    sealed class MCDADeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return Type.GetType(typeName + "," + Assembly.GetExecutingAssembly().FullName);
        }
    }
     #endregion  

}
