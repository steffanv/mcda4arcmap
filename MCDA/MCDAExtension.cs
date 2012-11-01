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
using MCDA.Entity;
using ESRI.ArcGIS.DataSourcesGDB;

namespace MCDA
{
    public class MCDAExtension : ESRI.ArcGIS.Desktop.AddIns.Extension, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler ItemAdded;
        public event PropertyChangedEventHandler ItemDeleted;

        private static MCDAExtension _extension;

        private IList<MCDA.Model.Feature> _listOfAvailableLayer = new List<MCDA.Model.Feature>();
        private IList<String> _listOfSelectedUniqueLayerNamesForPersistence = new List<string>();

        private IActiveViewEvents_Event _activeViewEvents;


        public IList<MCDA.Model.Feature> AvailableLayer
        {
            get { return _listOfAvailableLayer; }
            set { PropertyChanged.ChangeAndNotify(ref _listOfAvailableLayer, value, () => AvailableLayer); RegisterListenerForEveryMemberOfListOfAvailableLayer(); }
        }

        #region eventhandling for _listOfAvailableLayer
        public void RegisterListenerForEveryMemberOfListOfAvailableLayer()
        {
            //unregister for all member to avoid unessary multiple call
            _listOfAvailableLayer.ForEach(l => l.PropertyChanged -= new PropertyChangedEventHandler(l_PropertyChanged));
            _listOfAvailableLayer.ForEach(l => l.PropertyChanged +=new PropertyChangedEventHandler(l_PropertyChanged));
        }

        private void l_PropertyChanged(object sender, PropertyChangedEventArgs e)
        { 
            PropertyChanged.Notify(() => AvailableLayer);
        }
        #endregion

        public IList<MCDA.Model.Feature> AvailableFeatureLayer
        {
            get { return AvailableLayer.Where(l => l.IsFeatureLayer).ToList(); }
           // set { ;/* PropertyChanged.Notify(() => AvailableFeatureLayer);*/ }
        }

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

            IMap map = ArcMap.Document.ActiveView.FocusMap;

            _activeViewEvents = map as IActiveViewEvents_Event;

            _activeViewEvents.ItemAdded += new IActiveViewEvents_ItemAddedEventHandler(ArcMap_ItemAdded);
            _activeViewEvents.ItemDeleted += new IActiveViewEvents_ItemDeletedEventHandler(ArcMap_ItemDeleted);

            ArcMap.Events.NewDocument += new ESRI.ArcGIS.ArcMapUI.IDocumentEvents_NewDocumentEventHandler(Events_NewDocument);
            //ArcMap.Events.BeforeCloseDocument += new ESRI.ArcGIS.ArcMapUI.IDocumentEvents_BeforeCloseDocumentEventHandler(Events_BeforeCloseDocument); 
            ArcMap.Events.OpenDocument += new ESRI.ArcGIS.ArcMapUI.IDocumentEvents_OpenDocumentEventHandler(Events_OpenDocument);
  
        }

       

        private void Reset()
        {
           
            AvailableLayer = new List<MCDA.Model.Feature>();
        }

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

            Reset();

        }

        void Events_OpenDocument()
        {
            _activeViewEvents.ItemAdded -= new IActiveViewEvents_ItemAddedEventHandler(ArcMap_ItemAdded);
            _activeViewEvents.ItemDeleted -= new IActiveViewEvents_ItemDeletedEventHandler(ArcMap_ItemDeleted);

            IMap map = ArcMap.Document.ActiveView.FocusMap;

            _activeViewEvents = map as IActiveViewEvents_Event;

            _activeViewEvents.ItemAdded += new IActiveViewEvents_ItemAddedEventHandler(ArcMap_ItemAdded);
            _activeViewEvents.ItemDeleted += new IActiveViewEvents_ItemDeletedEventHandler(ArcMap_ItemDeleted);

            Reset();
        }

        void ArcMap_ItemDeleted(object Item)
        {
            AvailableLayer = GetListOfLayerFromActiveView(ArcMap.Document.ActiveView);

            ItemAdded.Notify(() => AvailableLayer);

        }

        private void ArcMap_ItemAdded(object item)
        {
            AvailableLayer = GetListOfLayerFromActiveView(ArcMap.Document.ActiveView);

            ItemAdded.Notify(() => AvailableLayer);
        }
        #endregion

        public WLCParameter GetWLCParameterBasedOnSelectedFields()
        {
            IList<WLCToolParameter> wlcParameters = new List<WLCToolParameter>();
            
            AvailableLayer.Where(l => l.IsSelected).ForEach(l => l.Fields.Where(f => f.IsSelected && f.IsNumber).ForEach(f => wlcParameters.Add(new WLCToolParameter(f.FieldName))));

            return new WLCParameter(wlcParameters);
        }

        /// <summary>
        /// Returns the field of the OID for the selected feature. However, it is possible that no feature is selected
        /// or that no OID exists or both. In this the methods retuns <code>null</code>
        /// </summary>
        /// <returns></returns>
        public MCDA.Model.Field GetOIDFieldFromSelectedFeature()
        {

            Model.Feature selectedFeature = _listOfAvailableLayer.FirstOrDefault(f => f.IsSelected);

            if (selectedFeature == null)
                return null;

            return selectedFeature.Fields.FirstOrDefault(f => f.IsOID);

        }

        public IList<MCDA.Model.Field> GetFieldsFromSelectedLayerWhichAreNumeric(IList<MCDA.Model.Feature> layer)
        {

            IList<MCDA.Model.Field> fieldList = new List<MCDA.Model.Field>();

            layer.Where(l => l.IsSelected).ForEach(l => l.Fields.Where(f => f.IsNumber).ForEach(f => fieldList.Add(f)));

            return fieldList;
        }

        public DataTable GetDataTableForParameterSet<T>(IList<T> toolParameter) where T : IToolParameter, new()
        {

           IList<MCDA.Model.Field> listOfFields = GetListOfFieldsFromToolParameter(toolParameter);

           DataTable dataTable = new DataTable();

            //add columns
           foreach (MCDA.Model.Field currentField in listOfFields)
           {
               
               dataTable.Columns.Add(currentField.FieldName, typeof(double));
           }

            //add the oid column
            MCDA.Model.Field oidField = GetOIDFieldFromSelectedFeature();

            //in fact no feature can be selected, thus we have to take care about potential null
            if (oidField != null)
            {

                dataTable.Columns.Add(oidField.FieldName, typeof(FieldTypeOID));

                //and to make it easier add the oidField to the rest of the fields
                listOfFields.Add(oidField);
            }

            //get data for rows
           IList<IList<double>> tableData = new List<IList<double>>();

           int expectedNumbersOfRow = 0;
           foreach (MCDA.Model.Field currentField in listOfFields)
           {
               IList<double> column = GetValuesOfField(currentField);
               tableData.Add(column);

               expectedNumbersOfRow = column.Count;
           }

           //add row
           for (int i = 0; i < expectedNumbersOfRow; i++)
           {
               DataRow row = dataTable.NewRow();

               for (int y = 0; y < tableData.Count; y++)
               {
                   //we have the oid column
                   if (dataTable.Columns[y].DataType == typeof(FieldTypeOID))
                   {
                       row[y] = new FieldTypeOID() { OID = (int) tableData[y][i] };
                       continue;
                   }

                   row[y] = tableData[y][i];
               }

               dataTable.Rows.Add(row);
           }

           return dataTable;

        }

        private IList<MCDA.Model.Field> GetListOfFieldsFromToolParameter<T>(IList<T> toolParameter) where T : IToolParameter
        {
            IList<MCDA.Model.Field> listOfFields = new List<MCDA.Model.Field>();

            foreach (IToolParameter currentToolParameter in toolParameter)
            {
                AvailableLayer.Where(l => l.IsSelected).ForEach(l => l.Fields.Where(f => f.FieldName.Equals(currentToolParameter.ColumnName)).ForEach(f => listOfFields.Add(f)));
            }

            return listOfFields;
        }

        public IList<double> GetValuesOfField(MCDA.Model.Field field)
        {
            IList<double> listOfValuesFromField = new List<double>();

            //this should be always the case, as we have only fields from feature layer
            IFeatureLayer2 _featureLayer = field.Layer.FeatureLayer;
          
            IFeatureCursor featureCursor = _featureLayer.FeatureClass.Search(null, true);

            IFeature feature = null;

            //while ((feature = featureCursor.NextFeature()) != null)
            //{

            feature = featureCursor.NextFeature();

                int fieldIndex = feature.Table.FindField(field.FieldName);

                ICursor cursor = feature.Table.Search(null, true);

                IRow row = null;

                while ((row = cursor.NextRow()) != null)
                {
                    //we have to cast explicitly ... https://connect.microsoft.com/VisualStudio/feedback/details/534288/ilist-dynamic-cannot-call-a-method-add-without-casting

                    listOfValuesFromField.Add(Convert.ToDouble(row.get_Value(fieldIndex)));
                }
            //}

            return listOfValuesFromField;
        }

        /*
        //http://forums.esri.com/Thread.asp?c=93&f=993&t=208152
        public IFeatureClass CreateFCFromExistingFC(IFeatureClass pTemplateFC, IWorkspace pWS, string sNewFCName)
        {

            IFeatureClass pNewFeatclass;

            IFields pOutFields;
            IFieldChecker pFieldChecker = new FieldCheckerClass();
            pFieldChecker.ValidateWorkspace = pWS;

            IEnumFieldError pErrorEnum;
            IFields pNewFields;
            IFieldsEdit pNewFieldsEdit;
            pNewFields = new FieldsClass();
            pNewFieldsEdit = (IFieldsEdit)pNewFields;
            pNewFieldsEdit.FieldCount_2 = pTemplateFC.Fields.FieldCount;

            IClone pClone;
            IField pCloneField;
            IField pObjectIDField;
            IFieldEdit pObjectIDFieldEdit;

            for (int j = 0; j < pTemplateFC.Fields.FieldCount; j++)
            {
                if (pTemplateFC.Fields.get_Field(j).Type == esriFieldType.esriFieldTypeOID)
                {
                    pObjectIDField = new FieldClass();
                    pObjectIDFieldEdit = (IFieldEdit)pObjectIDField;
                    pObjectIDFieldEdit.Name_2 = "ObjectID";
                    pObjectIDFieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
                    pNewFieldsEdit.set_Field(j, pObjectIDField);
                }
                else
                {
                    pClone = (IClone)pTemplateFC.Fields.get_Field(j);
                    pCloneField = (IField)pClone.Clone();
                    pNewFieldsEdit.set_Field(j, pCloneField);
                }
            }

           

            pFieldChecker.Validate(pNewFields, out pErrorEnum, out pOutFields);

            IUID pUID = pTemplateFC.CLSID;
            string sShapeFieldName = "Shape";

            IFeatureWorkspace pFeatureWS = (IFeatureWorkspace)pWS;
            pNewFeatclass = pFeatureWS.CreateFeatureClass(sNewFCName, pOutFields, (UID)pUID, null, esriFeatureType.esriFTSimple, sShapeFieldName, "");

            IFeatureLayer fl = new FeatureLayerClass();
            fl.FeatureClass = pNewFeatclass;

            fl.Name = "tha fl";
            ArcMap.Document.ActiveView.FocusMap.AddLayer(fl);

            return pNewFeatclass;
        }
        */

        #region private ArcObjects stuff

        private IList<MCDA.Model.Feature> GetListOfLayerFromActiveView(ESRI.ArcGIS.Carto.IActiveView activeView)
        {

            IList<MCDA.Model.Feature> layerList = new List<MCDA.Model.Feature>();

            ESRI.ArcGIS.Carto.IMap map = activeView.FocusMap;
           
            // Get the number of layers
            int numberOfLayers = map.LayerCount;

            // Loop through the layers and get the correct layer index
            for (System.Int32 i = 0; i < numberOfLayers; i++)
            {

                layerList.Add(new MCDA.Model.Feature(map.get_Layer(i)));
            }
            
            return layerList;
        }

        #endregion

        #region persistence
        protected override void OnSave(Stream outStrm)
        { 
            var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            //persist onlz the unique layer names of selected layer
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
