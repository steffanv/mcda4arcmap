using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using MCDA.Misc;
using MCDA.Model;
using MCDA.Extensions;
using System.Data;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Win32;

namespace MCDA.ViewModel
{
    internal sealed class WLCToolViewModel : AbstractToolViewModel,  INotifyPropertyChanged
    {
       public event PropertyChangedEventHandler PropertyChanged;

       private readonly MCDAExtension _mcdaExtension;
       private AbstractToolTemplate _wlcTool; 
       private DataTable _wlcResultDataTable;
       private BindingList<IToolParameter> _toolParameter;
       private readonly IList<List<IToolParameter>> _toolParameterStorageForAnimationLikeUpdate = new List<List<IToolParameter>>();
       private Feature _selectedFeature;

       private bool _isLocked = false;
       private bool _isSendToInMemoryWorkspaceCommand = false;
       private bool _isUpdateAllowed = false;

       private readonly PropertyChangedEventHandler _selectedFeaturePropertyChangedEventHandler;

       private readonly IList<PropertyChangedEventHandler> _listOfpropertyChangedEventHandlersForToolParameterIsBenefitCriterion = new List<PropertyChangedEventHandler>();
       private readonly IList<PropertyChangedEventHandler> _listOfpropertyChangedEventHandlersForToolParameterWeight = new List<PropertyChangedEventHandler>();
       private readonly IList<PropertyChangedEventHandler> _listOfpropertyChangedEventHandlersForFieldIsSelected = new List<PropertyChangedEventHandler>();
     
       public WLCToolViewModel()
       {
           _mcdaExtension = MCDAExtension.GetExtension();

           _wlcTool = ToolFactory.NewWLCTool();

           _wlcResultDataTable = _wlcTool.Data;

          _selectedFeaturePropertyChangedEventHandler =  _mcdaExtension.RegisterPropertyHandler(x => x.SelectedFeature, SelectedFeaturePropertyChanged);

           //we have to call our own update method to make sure we have a result column
            SelectedFeaturePropertyChanged(this, null);
       }

       private void WeightChanged(object sender, PropertyChangedEventArgs e)
       {
           base.Update();
       }

       private void BenefitCriterionChanged(object sender, PropertyChangedEventArgs e)
       {
           _isUpdateAllowed = true;

           base.Update();
       }

        protected override bool HasCriteriaSelected()
        {
            return _selectedFeature != null && _selectedFeature.Fields.Count(f => f.IsSelected) >= 1;
        }

        private void FieldPropertyChanged(object sender, PropertyChangedEventArgs e)
       {
            if (_isLocked)
            {
                return;
            }

            _wlcTool = ToolFactory.NewWLCTool();

           _toolParameter = new BindingList<IToolParameter>(_wlcTool.ToolParameterContainer.ToolParameter);


           if (_selectedFeature.Fields.Count(f => f.IsSelected) >= 1){

                 _wlcTool.Run();

                _wlcResultDataTable = _wlcTool.Data;
           }

           RegisterToolParameterEvents();

           PropertyChanged.Notify(() => WLCParameter);
           PropertyChanged.Notify(() => WLCResult);

           CommandManager.InvalidateRequerySuggested();
       }

       private void RegisterToolParameterEvents()
       {

           foreach (var currentToolParameter in _toolParameter)
           {
               currentToolParameter.UnRegisterPropertyHandler(_listOfpropertyChangedEventHandlersForToolParameterIsBenefitCriterion);
               currentToolParameter.UnRegisterPropertyHandler(_listOfpropertyChangedEventHandlersForToolParameterWeight);
           }

           foreach (var currentToolParameter in _toolParameter)
           {
               _listOfpropertyChangedEventHandlersForToolParameterIsBenefitCriterion.Add(currentToolParameter.RegisterPropertyHandler(b => b.IsBenefitCriterion, BenefitCriterionChanged));
               _listOfpropertyChangedEventHandlersForToolParameterWeight.Add(currentToolParameter.RegisterPropertyHandler(w => w.Weight, WeightChanged));
           }

       }

        private void SelectedFeaturePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isLocked)
            {
                return;
            }

            _wlcTool = ToolFactory.NewWLCTool();

            _toolParameter = new BindingList<IToolParameter>(_wlcTool.ToolParameterContainer.ToolParameter);

            if (_selectedFeature != null)
            {
                foreach (var currentField in _selectedFeature.Fields)
                    currentField.UnRegisterPropertyHandler(_listOfpropertyChangedEventHandlersForFieldIsSelected);
            }
            _selectedFeature = _mcdaExtension.SelectedFeature;

            if (_selectedFeature != null)
            {
                foreach (var currentField in _selectedFeature.Fields)
                {
                    _listOfpropertyChangedEventHandlersForFieldIsSelected.Add(
                        currentField.RegisterPropertyHandler(f => f.IsSelected, FieldPropertyChanged));
                }

                if (_selectedFeature.Fields.Count(f => f.IsSelected) >= 1)
                {
                    _wlcTool.Run();

                    _wlcResultDataTable = _wlcTool.Data;
                }
            }

           RegisterToolParameterEvents();

           PropertyChanged.Notify(() => WLCParameter);
           PropertyChanged.Notify(() => WLCResult);

            CommandManager.InvalidateRequerySuggested();
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
           {
               return;
           }

           _wlcTool.Run();
          _wlcResultDataTable = _wlcTool.Data;

           if (_isSendToInMemoryWorkspaceCommand)
           {
               _mcdaExtension.JoinToolResultByOID(_wlcTool, _wlcTool.Data);
           }

           _isUpdateAllowed = false;
           
       }

       protected override void UpdateRealtime()
       {
           _wlcTool.Run();
           _wlcResultDataTable = _wlcTool.Data;

           if (_isSendToInMemoryWorkspaceCommand)
           {
               _mcdaExtension.JoinToolResultByOID(_wlcTool, _wlcTool.Data);
           }
       }

       protected override void UpdateAnimation()
       {
           if (!_isUpdateAllowed)
           {
               List<IToolParameter> tList = _toolParameter.Select(t => t.DeepClone()).ToList();

               _toolParameterStorageForAnimationLikeUpdate.Add(tList);
           }

           else
           {
               var latestToolParameter = _toolParameter;

               if (_toolParameterStorageForAnimationLikeUpdate.Count > 0)
               {
                   int steps = (int) Math.Sqrt(_toolParameterStorageForAnimationLikeUpdate.Count);
                   //take several steps...
                   for (int i = 0; i < _toolParameterStorageForAnimationLikeUpdate.Count; i = i + steps)
                   {
                       _wlcTool.ToolParameterContainer.ToolParameter = _toolParameterStorageForAnimationLikeUpdate[i];
                       _wlcTool.Run();
                       _wlcResultDataTable = _wlcTool.Data;

                       if (_isSendToInMemoryWorkspaceCommand)
                       {
                           _mcdaExtension.JoinToolResultByOID(_wlcTool, _wlcTool.Data);
                       }
                   }
               }

               //make sure we add the latest one
               _wlcTool.ToolParameterContainer.ToolParameter = latestToolParameter;
               _wlcTool.Run();
               _wlcResultDataTable = _wlcTool.Data;

               if (_isSendToInMemoryWorkspaceCommand)
               {
                   _mcdaExtension.JoinToolResultByOID(_wlcTool, _wlcTool.Data);
               }

               _isUpdateAllowed = false;

               _toolParameterStorageForAnimationLikeUpdate.Clear();
           }
       }

       protected override void AfterUpdate()
       {
           PropertyChanged.Notify(() => WLCResult);
       }

       public BindingList<IToolParameter> WLCParameter
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
           set { _isLocked = value;}
       }

       public bool IsSendToInMemoryWorkspaceCommand
       {
           get { return _isSendToInMemoryWorkspaceCommand;}
           set { _isSendToInMemoryWorkspaceCommand = value; }
       }

       protected override void DoExportAsCSVCommand()
       {
           var saveFileDialog = new SaveFileDialog();
           saveFileDialog.FileName = _wlcTool.ToString();
           saveFileDialog.DefaultExt = ".csv";
           saveFileDialog.Filter = "Comma Separated Values (.csv)|*.csv";

           var result = saveFileDialog.ShowDialog();

           if (result == true)
           {
               Export.ToCSV<IToolParameter>(_wlcTool.Data, _wlcTool.ToolParameterContainer.ToolParameter,
                   saveFileDialog.FileName);
           }
       }

       protected override void DoLockCommand()
       {
           _isLocked = !_isLocked;

           if (!_isLocked && _isSendToInMemoryWorkspaceCommand)
           {
               ESRI.ArcGIS.Framework.IMessageDialog msgBox = new ESRI.ArcGIS.Framework.MessageDialogClass();
               bool userResult = msgBox.DoModal("Unlocking", "Unlocking also disconnects from the managed layer.", "Unlock", "Abort", ArcMap.Application.hWnd);

               //if the user hit no we have to set the lock state back to locked
               if (!userResult)
               {
                   _isLocked = !_isLocked;
                   return;
               }
               if (userResult)
               {
                   _isSendToInMemoryWorkspaceCommand = !_isSendToInMemoryWorkspaceCommand;
                   _mcdaExtension.RemoveLink(_wlcTool);
                   this.SelectedFeaturePropertyChanged(this, null);

                   PropertyChanged.Notify(() => IsSendToInMemoryWorkspaceCommand);
               }
           }

           if (_isLocked)
           {
                //ProgressDialog.ShowProgressDialog("Creating In Memory Representation",
                //    (Action<AbstractToolTemplate>)_mcdaExtension.EstablishLink, _wlcTool);

                ProgressDialogBuilder progressDialogBuilder = new ProgressDialogBuilder((Action<AbstractToolTemplate>)_mcdaExtension.EstablishLink, _wlcTool);
                progressDialogBuilder.SetTitle("Creating In Memory Representation").SetHideCancelButton(true).SetIsIndeterminate(true).Build().Show();
            }

            if (!_isLocked && !_isSendToInMemoryWorkspaceCommand)
           {
               _mcdaExtension.RemoveLink(_wlcTool);
               this.SelectedFeaturePropertyChanged(this, null);
           }

           PropertyChanged.Notify(() => IsLocked);
       }

       protected override void DoSendToInMemoryWorkspaceCommand()
       {
           _isSendToInMemoryWorkspaceCommand = !_isSendToInMemoryWorkspaceCommand;

           if (_isSendToInMemoryWorkspaceCommand && !_isLocked)
           {
               DoLockCommand();
           }

           if (_isSendToInMemoryWorkspaceCommand)
           {
               _mcdaExtension.DisplayLink(_wlcTool);
               //ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<AbstractToolTemplate, DataTable>)_mcdaExtension.JoinToolResultByOID, _wlcTool, _wlcTool.Data);

                ProgressDialogBuilder progressDialogBuilder = new ProgressDialogBuilder((Action<AbstractToolTemplate, DataTable>)_mcdaExtension.JoinToolResultByOID, _wlcTool, _wlcTool.Data);
                progressDialogBuilder.SetTitle("Creating Symbology").SetHideCancelButton(true).SetIsIndeterminate(true).Build().Show();
            }

           if (!_isSendToInMemoryWorkspaceCommand)
           {
               DoLockCommand();
           }

           PropertyChanged.Notify(() => IsSendToInMemoryWorkspaceCommand);
       }

       protected override void DoNormalizationSelectionCommand()
       {
           var parentHandle = new IntPtr(ArcMap.Application.hWnd);

           NormalizationView = new NormalizationSelectionView { DataContext = NormalizationViewModel };

           NormalizationViewModel.SelectedTransformationStrategy = _wlcTool.TransformationStrategy;

           var helper = new WindowInteropHelper(NormalizationView) { Owner = parentHandle };

           NormalizationView.Closing += NormalizationViewClosing;
          
           NormalizationView.ShowDialog();        
       }

       void NormalizationViewClosing(object sender, CancelEventArgs e)
       {
           NormalizationViewModel.SelectedTransformationStrategy = _wlcTool.TransformationStrategy;
       }

       protected override void DoApplyNormalizationCommand()
       {
           _wlcTool.TransformationStrategy = NormalizationViewModel.SelectedTransformationStrategy;

           _isUpdateAllowed = true;
           base.Update();
       }

       protected override void DoCancelNormalizationCommand()
       {
           NormalizationViewModel.SelectedTransformationStrategy = _wlcTool.TransformationStrategy;

           NormalizationView.Closing -= NormalizationViewClosing;
           NormalizationView.Close();
       }

       protected override void DoOkayNormalizationCommand()
       {
           if (_wlcTool.TransformationStrategy != NormalizationViewModel.SelectedTransformationStrategy)
           {
               DoApplyNormalizationCommand();
           }

           NormalizationView.Closing -= NormalizationViewClosing;
           NormalizationView.Close();
       }

       protected override void DoDistributionCommand()
        {
            _wlcTool.ToolParameterContainer.DistributeEquallyToolParameterWeights();

            _isUpdateAllowed = true;
            base.Update();

            PropertyChanged.Notify(() => WLCParameter);
            PropertyChanged.Notify(() => WLCResult);
        }


       protected override void DoClosingCommand()
       {
           if (_isLocked || _isSendToInMemoryWorkspaceCommand)
           {
               _mcdaExtension.RemoveLink(_wlcTool);
           }

           _mcdaExtension.UnRegisterPropertyHandler(_selectedFeaturePropertyChangedEventHandler);

           if (_selectedFeature != null)
           {
               foreach (var currentField in _selectedFeature.Fields)
               {
                   currentField.UnRegisterPropertyHandler(_listOfpropertyChangedEventHandlersForFieldIsSelected);
               }
           }

           foreach (var currentToolParameter in _toolParameter)
           {
               currentToolParameter.UnRegisterPropertyHandler(_listOfpropertyChangedEventHandlersForToolParameterIsBenefitCriterion);
               currentToolParameter.UnRegisterPropertyHandler(_listOfpropertyChangedEventHandlersForToolParameterWeight);
           }
       }
    }
}
