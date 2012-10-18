using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MCDA.Model;
using MCDA.Extensions;
using MCDA.Entity;
using System.Data;
using System.Windows.Input;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using System.Windows.Interop;
using Microsoft.Win32;

namespace MCDA.ViewModel
{
    class WLCToolViewModel : AbstractToolViewModel,  INotifyPropertyChanged
    {
       public event PropertyChangedEventHandler PropertyChanged;

       private MCDAExtension _mcdaExtension;
       private WLCTool _wlcTool; 
       private DataTable _wlcResultDataTable;
       private BindingList<WLCToolParameter> _toolParameter;
       private IList<List<WLCToolParameter>> _toolParameterStorageForAnimationLikeUpdate = new List<List<WLCToolParameter>>();

       private ICommand _standardizationSelectionCommand;
       private ICommand _sendToInMemoryWorkspaceCommand;
       private ICommand _lockCommand;
       private ICommand _exportAsCSVCommand;

       private bool _isLocked = false;
       private bool _isSendToInMemoryWorkspaceCommand = false;
       private bool _isSliderDraged = false;
     
       public WLCToolViewModel()
       {
           _mcdaExtension = MCDAExtension.GetExtension();

           _wlcTool = ToolFactory.NewWLCTool();

           _wlcResultDataTable = _wlcTool.Data;

            _mcdaExtension.PropertyChanged += new PropertyChangedEventHandler(MCDAExtension_PropertyChanged);

            _toolParameter = new BindingList<WLCToolParameter>(_wlcTool.WLCParameter.ToolParameter);

            _toolParameter.ListChanged += new ListChangedEventHandler(ToolParameter_ListChanged);

            PropertyChanged.Notify(() => WLCParameter);
            PropertyChanged.Notify(() => WLCResult);

       }

       void ToolParameter_ListChanged(object sender, ListChangedEventArgs e)
       {
           base.Update();     
       }

       void MCDAExtension_PropertyChanged(object sender, PropertyChangedEventArgs e)
       {
           if (_isLocked)
               return;

           _wlcTool = ToolFactory.NewWLCTool();

           _wlcResultDataTable = _wlcTool.Data;

           _mcdaExtension.PropertyChanged += new PropertyChangedEventHandler(MCDAExtension_PropertyChanged);

           _toolParameter = new BindingList<WLCToolParameter>(_wlcTool.WLCParameter.ToolParameter);

           _toolParameter.ListChanged += new ListChangedEventHandler(ToolParameter_ListChanged);

           PropertyChanged.Notify(() => WLCParameter);
           PropertyChanged.Notify(() => WLCResult);
       }

        //called from the code behind page if slider is draged
       public void SliderChangedEvent()
       {
           _isSliderDraged = true;
           //base.Update();
       }

       protected override void UpdateDrag()
       {
           if (!_isSliderDraged)
               return;

          _wlcTool.Run();
          _wlcResultDataTable = _wlcTool.Data;

          if (_isSendToInMemoryWorkspaceCommand)
              ToolFeatureClassLinkTracker.Instance.JoinToolResultByOID(_wlcTool, _wlcTool.Data);

          _isSliderDraged = false;
           
       }

       protected override void UpdateRealtime()
       {
           _wlcTool.Run();
           _wlcResultDataTable = _wlcTool.Data;

           if (_isSendToInMemoryWorkspaceCommand)
               ToolFeatureClassLinkTracker.Instance.JoinToolResultByOID(_wlcTool, _wlcTool.Data);
       }

       protected override void UpdateAnimation()
       {
           if (!_isSliderDraged)
           {
               List<WLCToolParameter> tList = new List<WLCToolParameter>();

               for (int i = 0; i < _toolParameter.Count; i++)
               {
                   tList.Add(_toolParameter[i].DeepClone());
               }

               _toolParameterStorageForAnimationLikeUpdate.Add(tList);
           }

           else
           {
               BindingList<WLCToolParameter> latestToolParameter = _toolParameter;

               int steps = Convert.ToInt32(_toolParameterStorageForAnimationLikeUpdate.Count * 0.5f);
               //take several steps...
               for (int i = 0; i < _toolParameterStorageForAnimationLikeUpdate.Count; i = i+steps)
               {
                   _wlcTool.WLCParameter.ToolParameter = _toolParameterStorageForAnimationLikeUpdate[i];
                   _wlcTool.Run();
                   _wlcResultDataTable = _wlcTool.Data;

                   if (_isSendToInMemoryWorkspaceCommand)
                       ToolFeatureClassLinkTracker.Instance.JoinToolResultByOID(_wlcTool, _wlcTool.Data);
               }

               //make sure we add the latest one

               _wlcTool.WLCParameter.ToolParameter = latestToolParameter;
               _wlcTool.Run();
               _wlcResultDataTable = _wlcTool.Data;

               if (_isSendToInMemoryWorkspaceCommand)
                    ToolFeatureClassLinkTracker.Instance.JoinToolResultByOID(_wlcTool, _wlcTool.Data);
  
               _isSliderDraged = false;

               _toolParameterStorageForAnimationLikeUpdate.Clear();
           }
       }

       protected override void AfterUpdate()
       {
           PropertyChanged.Notify(() => WLCResult);
       }

       public BindingList<WLCToolParameter> WLCParameter
       {
           get { return _toolParameter; }
           set {  _toolParameter = value; }
       }
       public DataView WLCResult
       {
           get { return _wlcResultDataTable.DefaultView; }
       }

       public bool IsLocked
       {
           get{ return _isLocked;}
           set { _isLocked = value; }
       }
       public bool IsSendToInMemoryWorkspaceCommand
       {
           get { return _isSendToInMemoryWorkspaceCommand;}
           set { _isSendToInMemoryWorkspaceCommand = value; }
       }

       public ICommand ExportAsCSVCommand
       {
           get {

               if (_exportAsCSVCommand == null)
               {
                   _exportAsCSVCommand = new RelayCommand(
                       p => this.DoExportAsCSVCommand(),
                       p => true
                       );
               }

               return _exportAsCSVCommand;
           }
       }

       private void DoExportAsCSVCommand()
       {
           SaveFileDialog saveFileDialog = new SaveFileDialog();
           saveFileDialog.FileName = _wlcTool.ToString();
           saveFileDialog.DefaultExt = ".csv";
           saveFileDialog.Filter = "Comma Separated Values (.csv)|*.csv";

           Nullable<bool> result = saveFileDialog.ShowDialog();

           if (result == true)
           {
               Export.ToCSV(_wlcTool.Data,_wlcTool.WLCParameter.ToolParameter, saveFileDialog.FileName);
           }
       }

       public ICommand LockCommand
       {
           get {

               if (_lockCommand == null)
               {
                   _lockCommand = new RelayCommand(
                       p => this.DoLockCommand(),
                       p => true
                       );
               }

               return _lockCommand;
           }
       }

       private void DoLockCommand()
       {
           _isLocked = !_isLocked;

           if(_isLocked)
           ToolFeatureClassLinkTracker.Instance.EstablishLink(_wlcTool);

           if (!_isLocked && _isSendToInMemoryWorkspaceCommand)
           {
               //TODO dlg
               DoSendToInMemoryWorkspaceCommand();
           }

           if (!_isLocked)
           {
               ToolFeatureClassLinkTracker.Instance.RemoveLink(_wlcTool);
               this.MCDAExtension_PropertyChanged(this, null);
           }

           PropertyChanged.Notify(() => IsLocked);
       }

       public ICommand SendToInMemoryWorkspaceCommand
       {
           get
           {
               if (_sendToInMemoryWorkspaceCommand == null)
               {

                   _sendToInMemoryWorkspaceCommand = new RelayCommand(
                       param => this.DoSendToInMemoryWorkspaceCommand(),
                       param => this.CanCreateInMemoryFCCommand()
                   );
               }
               return _sendToInMemoryWorkspaceCommand;
           }
       }

       private void DoSendToInMemoryWorkspaceCommand(){

           _isSendToInMemoryWorkspaceCommand = !_isSendToInMemoryWorkspaceCommand;

           if (_isSendToInMemoryWorkspaceCommand && !_isLocked)
               DoLockCommand();

           if (_isSendToInMemoryWorkspaceCommand)
           {
               ToolFeatureClassLinkTracker.Instance.JoinToolResultByOID(_wlcTool, _wlcTool.Data);
               ToolFeatureClassLinkTracker.Instance.DisplayLink(_wlcTool);
           }

           if (!_isSendToInMemoryWorkspaceCommand)
               ToolFeatureClassLinkTracker.Instance.RemoveLink(_wlcTool);


           PropertyChanged.Notify(() => IsSendToInMemoryWorkspaceCommand);
       }

       private bool CanCreateInMemoryFCCommand()
       {
           return true;
       }

       public ICommand StandardizationSelectionCommand
       {
           get { if (_standardizationSelectionCommand == null) {

               _standardizationSelectionCommand = new RelayCommand(
                   param => this.DoStandardizationSelectionCommand(),
                   param => true
               );
           }
           return _standardizationSelectionCommand;
           }
       }

       private void DoStandardizationSelectionCommand()
       {

           var parentHandle = new IntPtr(ArcMap.Application.hWnd);

           var wpfWindow = new StandardizationSelectionView();

           var helper = new WindowInteropHelper(wpfWindow);

           helper.Owner = parentHandle;

           wpfWindow.Closed += delegate(object sender, EventArgs e)
           {

               StandardizationSelectionViewModel standardizationSelectionViewModel = wpfWindow.DataContext as StandardizationSelectionViewModel;

               _wlcTool.TransformationStrategy = standardizationSelectionViewModel.SelectedTransformationStrategy;

               base.Update();

           };

           wpfWindow.ShowDialog();
 
       }
    }
}
