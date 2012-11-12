using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using MCDA.Extensions;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Editor;
using System.Data;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.ADF;
using System.ComponentModel;

namespace MCDA.Model
{
    class ToolFeatureClassLinkTracker : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static ToolFeatureClassLinkTracker _instance;

        private IDictionary<AbstractToolTemplate,MCDAWorkspaceContainer> _dictionaryOfLinks= new Dictionary<AbstractToolTemplate,MCDAWorkspaceContainer>();

        private IWorkspace _shadowWorkspace;

        private MCDA.MCDAExtension _mcdaExtension = MCDA.MCDAExtension.GetExtension();
        private IEditor _editor;

        private ToolFeatureClassLinkTracker() {

           _shadowWorkspace = CreateInMemoryWorkspace();
        }

        public static ToolFeatureClassLinkTracker Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ToolFeatureClassLinkTracker();
                }

                return _instance;
            }
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

        static void DefineUniqueValueRenderer(IGeoFeatureLayer pGeoFeatureLayer, string fieldName)
        {
            if (pGeoFeatureLayer == null || fieldName == null)
                return;

            IRandomColorRamp pRandomColorRamp = new RandomColorRampClass();
            //Create the color ramp for the symbols in the renderer.
            pRandomColorRamp.MinSaturation = 20;
            pRandomColorRamp.MaxSaturation = 90;
            pRandomColorRamp.MinValue = 10;
            pRandomColorRamp.MaxValue = 100;
            pRandomColorRamp.StartHue = 16;
            pRandomColorRamp.EndHue = 288;
            pRandomColorRamp.UseSeed = true;
            pRandomColorRamp.Seed = 43;

            //Create the renderer.
            IUniqueValueRenderer pUniqueValueRenderer = new UniqueValueRendererClass();

            ISimpleFillSymbol pSimpleFillSymbol = new SimpleFillSymbolClass();
            pSimpleFillSymbol.Style = esriSimpleFillStyle.esriSFSSolid;
            pSimpleFillSymbol.Outline.Width = 0.4;

            //These properties should be set prior to adding values.
            pUniqueValueRenderer.FieldCount = 1;
            pUniqueValueRenderer.set_Field(0, fieldName);
            pUniqueValueRenderer.DefaultSymbol = pSimpleFillSymbol as ISymbol;
            pUniqueValueRenderer.UseDefaultSymbol = true;

            IDisplayTable pDisplayTable = pGeoFeatureLayer as IDisplayTable;
            IFeatureCursor pFeatureCursor = pDisplayTable.SearchDisplayTable(null, false) as
                IFeatureCursor;
            IFeature pFeature = pFeatureCursor.NextFeature();


            bool ValFound;
            int fieldIndex;

            IFields pFields = pFeatureCursor.Fields;
            fieldIndex = pFields.FindField(fieldName);
            while (pFeature != null)
            {
                ISimpleFillSymbol pClassSymbol = new SimpleFillSymbolClass();
                pClassSymbol.Style = esriSimpleFillStyle.esriSFSSolid;
                pClassSymbol.Outline.Width = 0.4;

                string classValue;
                classValue = Convert.ToString( pFeature.get_Value(fieldIndex) );

                //Test to see if this value was added to the renderer. If not, add it.
                ValFound = false;
                for (int i = 0; i <= pUniqueValueRenderer.ValueCount - 1; i++)
                {
                    if (pUniqueValueRenderer.get_Value(i) == classValue)
                    {
                        ValFound = true;
                        break; //Exit the loop if the value was found.
                    }
                }
                //If the value was not found, it's new and will be added.
                if (ValFound == false)
                {
                    pUniqueValueRenderer.AddValue(classValue, fieldName, pClassSymbol as
                        ISymbol);
                    pUniqueValueRenderer.set_Label(classValue, classValue);
                    pUniqueValueRenderer.set_Symbol(classValue, pClassSymbol as ISymbol);
                }
                pFeature = pFeatureCursor.NextFeature();
            }
            //Since the number of unique values is known, the color ramp can be sized and the colors assigned.
            pRandomColorRamp.Size = pUniqueValueRenderer.ValueCount;
            bool bOK;
            pRandomColorRamp.CreateRamp(out bOK);

            IEnumColors pEnumColors = pRandomColorRamp.Colors;
            pEnumColors.Reset();
            for (int j = 0; j <= pUniqueValueRenderer.ValueCount - 1; j++)
            {
                string xv;
                xv = pUniqueValueRenderer.get_Value(j);
                if (xv != "")
                {
                    ISimpleFillSymbol pSimpleFillColor = pUniqueValueRenderer.get_Symbol(xv)
                        as ISimpleFillSymbol;
                    pSimpleFillColor.Color = pEnumColors.Next();
                    pUniqueValueRenderer.set_Symbol(xv, pSimpleFillColor as ISymbol);

                }
            }

            //'** If you didn't use a predefined color ramp in a style, use "Custom" here. 
            //'** Otherwise, use the name of the color ramp you selected.
            pUniqueValueRenderer.ColorScheme = "Custom";
            ITable pTable = pDisplayTable as ITable;
            bool isString = pTable.Fields.get_Field(fieldIndex).Type ==
                esriFieldType.esriFieldTypeString;
            pUniqueValueRenderer.set_FieldType(0, isString);
            pGeoFeatureLayer.Renderer = pUniqueValueRenderer as IFeatureRenderer;


            //This makes the layer properties symbology tab show the correct interface.
            IUID pUID = new UIDClass();
            pUID.Value = "{683C994E-A17B-11D1-8816-080009EC732A}";
            pGeoFeatureLayer.RendererPropertyPageClassID = pUID as UIDClass;

        }

        public void DisplayLink(AbstractToolTemplate tool)
        {
            MCDAWorkspaceContainer mcdaWorkspaceContainer;
           
            if (!_dictionaryOfLinks.TryGetValue(tool, out mcdaWorkspaceContainer))
                return;

            IFeatureClass fc = mcdaWorkspaceContainer.FeatureClass;

           IFeatureLayer fl = new FeatureLayerClass();
           fl.FeatureClass = fc;

           fl.Name = CreateLayerName(tool);

           mcdaWorkspaceContainer.FeatureLayer = fl;
          
           IGeoFeatureLayer gFl = fl as IGeoFeatureLayer;

           DefineUniqueValueRenderer(gFl, tool.DefaultResultColumnName);

           ArcMap.Document.ActiveView.FocusMap.AddLayer(fl);
        }

        public void RemoveLink(AbstractToolTemplate tool)
        {
            MCDAWorkspaceContainer mcdaWorkspaceContainer;
            if (!_dictionaryOfLinks.TryGetValue(tool, out mcdaWorkspaceContainer))
                return;

            _dictionaryOfLinks.Remove(tool);
        }

        private String CreateLayerName(AbstractToolTemplate tool)
        {
            return tool.ToString() + DateTime.Now.ToString("MMddHHmmssffff");
        }

        private String CreateTimeStamp()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssffff");
        }

        public IFeatureClass EstablishLink(AbstractToolTemplate tool)
        {
            IFeatureLayer2 fl = _mcdaExtension.AvailableFeatureLayer.Where(l => l.IsSelected).ToList()[0].FeatureLayer;
            IFeatureClass fc = fl.FeatureClass;

           IFeatureClass fcCopy = CopyFeatureClassIntoNewWorkspace(fc, _shadowWorkspace, tool.ToString()+CreateTimeStamp());
            _dictionaryOfLinks.Add(tool, new MCDAWorkspaceContainer(tool,fcCopy));

            return fcCopy;
        }

        /*
        private bool IsFeatureLayerNameExistingInInMemoryWorkspace(string name)
        {
            IEnumDatasetName enumDataSetName = _shadowWorkspace.get_DatasetNames(esriDatasetType.esriDTLayer);
            IDatasetName dataSetName;

            while ((dataSetName = enumDataSetName.Next()) != null)
            {
                if (name.Equals(dataSetName.))
                    return true;
            }

            return false;
        }

        private bool IsFeatureClassNameExistingInInMemoryWorkspace(string name)
        {
            IWorkspace2 ws = _shadowWorkspace as IWorkspace2;
            return ws.get_NameExists(esriDatasetType.esriDTFeatureClass, name);
            /*
            IEnumDatasetName enumDataSetName = _shadowWorkspace.get_DatasetNames(esriDatasetType.esriDTFeatureClass);
            IDatasetName dataSetName;

            while ((dataSetName = enumDataSetName.Next()) != null)
            {
                if(name.Equals(dataSetName.Name))
                    return true;
            }

            return false;
             * *
        }*/

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

                    //DataRow[] dRow = dataTable.Select(fc.OIDFieldName.+ "=" + oid);
                    //feature.set_Value(fieldIndex, dRow[0][tool.DefaultResultColumnName]);
                    //feature.set_Value(fieldIndex, dRow[tool.DefaultResultColumnName]);

                    feature.Store();
                    //featureCursor.UpdateFeature(feature);
                    feature = featureCursor.NextFeature();
                }

            }

           // System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);

            DefineUniqueValueRenderer(mcdaWorkspaceContainer.FeatureLayer as IGeoFeatureLayer, tool.DefaultResultColumnName);

            //ArcMap.Document.ActiveView.Refresh();
            PartialRefresh(tool);
            ArcMap.Document.UpdateContents();
        }

        private void PartialRefresh(AbstractToolTemplate tool)
        {
            MCDAWorkspaceContainer mcdaWorkspaceContainer;
            if (!_dictionaryOfLinks.TryGetValue(tool, out mcdaWorkspaceContainer))
                return;

            ArcMap.Document.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, mcdaWorkspaceContainer.FeatureLayer, null);
        }

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

        public IList<MCDAWorkspaceContainer> GetAllMCDAWorkspaceContainerFromShadowWorkspace()
        {
            return _dictionaryOfLinks.Values.ToList();
        }
    }

    internal class MCDAWorkspaceContainer
    {
        private IFeatureClass _fc;
        private IFeatureLayer _fl;
        private AbstractToolTemplate _tool;
        
        public MCDAWorkspaceContainer(AbstractToolTemplate tool, IFeatureClass fc)
        {
            _tool = tool;
            _fc = fc;
        }

        public AbstractToolTemplate Tool
        {
            get { return _tool; }
            set { _tool = value; }
        }

            public IFeatureClass FeatureClass{

                get { return _fc;}
                set { _fc = value; }
            }

            public IFeatureLayer FeatureLayer
            {
                get { return _fl; }
                set { _fl = value; }
            }
    }
}
