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

       private bool _isLocked = false;
       private bool _isSendToInMemoryWorkspaceCommand = false;
       private bool _isUpdateAllowed = false;
     
       public WLCToolViewModel()
       {
           _mcdaExtension = MCDAExtension.GetExtension();

           _wlcTool = ToolFactory.NewWLCTool();

           _wlcResultDataTable = _wlcTool.Data;

           _mcdaExtension.RegisterPropertyHandler(x => x.AvailableLayer, MCDAExtensionPropertyChanged);

           if(_mcdaExtension.SelectedLayer != null)
               _mcdaExtension.SelectedLayer.Fields.ForEach(x => x.RegisterPropertyHandler(f => f.IsSelected, FieldPropertyChanged));

           _toolParameter = new BindingList<WLCToolParameter>(_wlcTool.WLCParameter.ToolParameter);

           _toolParameter.ForEach(t => t.RegisterPropertyHandler(b => b.IsBenefitCriterion,BenefitCriterionChanged));
           _toolParameter.ForEach(t => t.RegisterPropertyHandler(w => w.Weight, WeightChanged));
           
            PropertyChanged.Notify(() => WLCParameter);
            PropertyChanged.Notify(() => WLCResult);

           //we have to call our own update method to make sure we have a result column
            MCDAExtensionPropertyChanged(this, null);

       }

       private void WeightChanged(object sender, PropertyChangedEventArgs e)
       {
           base.Update();
       }

       private void BenefitCriterionChanged(object sender, PropertyChangedEventArgs e)
       {
           UpdateRealtime();
       }

       private void FieldPropertyChanged(object sender, PropertyChangedEventArgs e)
       {
           _toolParameter = new BindingList<WLCToolParameter>(_wlcTool.WLCParameter.ToolParameter);

           _toolParameter.ForEach(t => t.UnRegisterPropertyHandler(b => b.IsBenefitCriterion, BenefitCriterionChanged));
           _toolParameter.ForEach(t => t.UnRegisterPropertyHandler(w => w.Weight, WeightChanged));

           _toolParameter.ForEach(t => t.RegisterPropertyHandler(b => b.IsBenefitCriterion, BenefitCriterionChanged));
           _toolParameter.ForEach(t => t.RegisterPropertyHandler(w => w.Weight, WeightChanged));

           PropertyChanged.Notify(() => WLCParameter);
           PropertyChanged.Notify(() => WLCResult);
       }

       private void MCDAExtensionPropertyChanged(object sender, PropertyChangedEventArgs e)
       {
           if (_isLocked)
               return;

           _wlcTool = ToolFactory.NewWLCTool();
           _wlcTool.Run();

           _wlcResultDataTable = _wlcTool.Data;

           if (_mcdaExtension.SelectedLayer != null)
           {

               _mcdaExtension.SelectedLayer.Fields.ForEach(x => x.UnRegisterPropertyHandler(f => f.IsSelected, FieldPropertyChanged));
               _mcdaExtension.SelectedLayer.Fields.ForEach(x => x.RegisterPropertyHandler(f => f.IsSelected, FieldPropertyChanged));
           }
          
       }

        //called from the code behind page if something changed
       public void UpdateAllowedEvent()
       {
           _isUpdateAllowed = true;
           base.Update();

       }

       protected override void UpdateDrag()
       {
           if (!_isUpdateAllowed)
               return;

          _wlcTool.Run();
          _wlcResultDataTable = _wlcTool.Data;

          if (_isSendToInMemoryWorkspaceCommand)
              _mcdaExtension.JoinToolResultByOID(_wlcTool, _wlcTool.Data);

          _isUpdateAllowed = false;
           
       }

       protected override void UpdateRealtime()
       {
           _wlcTool.Run();
           _wlcResultDataTable = _wlcTool.Data;

           if (_isSendToInMemoryWorkspaceCommand)
               _mcdaExtension.JoinToolResultByOID(_wlcTool, _wlcTool.Data);
       }

       protected override void UpdateAnimation()
       {
           if (!_isUpdateAllowed)
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

               if (_toolParameterStorageForAnimationLikeUpdate.Count > 0)
               {
                   int steps = (int) Math.Sqrt(_toolParameterStorageForAnimationLikeUpdate.Count);
                   //take several steps...
                   for (int i = 0; i < _toolParameterStorageForAnimationLikeUpdate.Count; i = i + steps)
                   {
                       _wlcTool.WLCParameter.ToolParameter = _toolParameterStorageForAnimationLikeUpdate[i];
                       _wlcTool.Run();
                       _wlcResultDataTable = _wlcTool.Data;

                       if (_isSendToInMemoryWorkspaceCommand)
                           _mcdaExtension.JoinToolResultByOID(_wlcTool, _wlcTool.Data);
                   }
               }

               //make sure we add the latest one
               _wlcTool.WLCParameter.ToolParameter = latestToolParameter;
               _wlcTool.Run();
               _wlcResultDataTable = _wlcTool.Data;

               if (_isSendToInMemoryWorkspaceCommand)
                   _mcdaExtension.JoinToolResultByOID(_wlcTool, _wlcTool.Data);
  
               _isUpdateAllowed = false;

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
           set { _isLocked = value;

           _wlcTool.IsLocked = value;
           }
       }

       public bool IsSendToInMemoryWorkspaceCommand
       {
           get { return _isSendToInMemoryWorkspaceCommand;}
           set { _isSendToInMemoryWorkspaceCommand = value; }
       }

       protected override void DoExportAsCSVCommand()
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

       protected override void DoLockCommand()
       {
           _isLocked = !_isLocked;

           if (!_isLocked && _isSendToInMemoryWorkspaceCommand)
           {
               ESRI.ArcGIS.Framework.IMessageDialog msgBox = new ESRI.ArcGIS.Framework.MessageDialogClass();
               bool userResult = msgBox.DoModal("Unlocking", "Unlocking also removes the existing in memory connection.", "Yes", "No", ArcMap.Application.hWnd);

               //if the user hit no we have to set the lock state back to locked
               if (!userResult)
               {
                   _isLocked = !_isLocked;
                   return;
               }
               if (userResult)
                   //simulate another send to in memory workspace command
                   //this actually unlinks everything
                   DoSendToInMemoryWorkspaceCommand();
           }

           if(_isLocked)
               _mcdaExtension.EstablishLink(_wlcTool);

           if (!_isLocked)
           {
               _mcdaExtension.RemoveLink(_wlcTool);
               this.MCDAExtensionPropertyChanged(this, null);
           }

           PropertyChanged.Notify(() => IsLocked);
       }

       protected override void DoSendToInMemoryWorkspaceCommand()
       {

           _isSendToInMemoryWorkspaceCommand = !_isSendToInMemoryWorkspaceCommand;

           if (_isSendToInMemoryWorkspaceCommand && !_isLocked)
               DoLockCommand();

           if (_isSendToInMemoryWorkspaceCommand)
           {
               _mcdaExtension.JoinToolResultByOID(_wlcTool, _wlcTool.Data);
               _mcdaExtension.DisplayLink(_wlcTool);
           }

           if (!_isSendToInMemoryWorkspaceCommand)
               _mcdaExtension.RemoveLink(_wlcTool);

           PropertyChanged.Notify(() => IsSendToInMemoryWorkspaceCommand);
       }

       protected override void DoStandardizationSelectionCommand()
       {

           var parentHandle = new IntPtr(ArcMap.Application.hWnd);

           var wpfWindow = new StandardizationSelectionView();

           StandardizationSelectionViewModel standardizationSelectionViewModel = wpfWindow.DataContext as StandardizationSelectionViewModel;

           standardizationSelectionViewModel.SelectedTransformationStrategy = _wlcTool.TransformationStrategy;

           var helper = new WindowInteropHelper(wpfWindow);

           helper.Owner = parentHandle;

           wpfWindow.Closed += delegate(object sender, EventArgs e)
           {

               _wlcTool.TransformationStrategy = standardizationSelectionViewModel.SelectedTransformationStrategy;

               _wlcTool.Run();
               _wlcResultDataTable = _wlcTool.Data;

               _isUpdateAllowed = true;

               base.Update();

           };

           wpfWindow.ShowDialog();
 
       }
    }
}
