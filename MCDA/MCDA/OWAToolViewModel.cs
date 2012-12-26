﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MCDA.Model;
using MCDA.Extensions;
using System.Data;
using Microsoft.Win32;
using System.Windows.Interop;
using System.Windows.Input;

namespace MCDA.ViewModel
{
    class OWAToolViewModel : AbstractToolViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MCDAExtension _mcdaExtension;
        private OWATool _owaTool;
        private DataTable _owaResultDataTable;
        private BindingList<IToolParameter> _toolParameter;
        private IList<List<IToolParameter>> _toolParameterStorageForAnimationLikeUpdate = new List<List<IToolParameter>>();

        private bool _isLocked = false;
        private bool _isSendToInMemoryWorkspaceCommand = false;
        private bool _isUpdateAllowed = false;

        private ICommand _alphaSelectionCommand;

        public OWAToolViewModel()
        {
            _mcdaExtension = MCDAExtension.GetExtension();

            _owaTool = ToolFactory.NewOWATool();

            _owaResultDataTable = _owaTool.Data;

            _mcdaExtension.RegisterPropertyHandler(x => x.AvailableLayer, MCDAExtensionPropertyChanged);

            if (_mcdaExtension.SelectedLayer != null)
                _mcdaExtension.SelectedLayer.Fields.ForEach(x => x.RegisterPropertyHandler(f => f.IsSelected, FieldPropertyChanged));

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
            if (_isLocked)
                return;

            _owaTool = ToolFactory.NewOWATool();

            _toolParameter = new BindingList<IToolParameter>(_owaTool.ToolParameterContainer.ToolParameter);

            _owaTool.Run();

            _owaResultDataTable = _owaTool.Data;

            RegisterToolParameterEvents();

            PropertyChanged.Notify(() => OWAParameter);
            PropertyChanged.Notify(() => OWAResult);
        }

        private void RegisterToolParameterEvents()
        {
            _toolParameter.ForEach(t => t.UnRegisterPropertyHandler(b => b.IsBenefitCriterion, BenefitCriterionChanged));
            _toolParameter.ForEach(t => t.UnRegisterPropertyHandler(w => w.Weight, WeightChanged));

            _toolParameter.ForEach(t => t.RegisterPropertyHandler(b => b.IsBenefitCriterion, BenefitCriterionChanged));
            _toolParameter.ForEach(t => t.RegisterPropertyHandler(w => w.Weight, WeightChanged));
        }

        private void MCDAExtensionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isLocked)
                return;

            _owaTool = ToolFactory.NewOWATool();

            _toolParameter = new BindingList<IToolParameter>(_owaTool.ToolParameterContainer.ToolParameter);

            _owaTool.Run();

            _owaResultDataTable = _owaTool.Data;

            if (_mcdaExtension.SelectedLayer != null)
            {
                _mcdaExtension.SelectedLayer.Fields.ForEach(x => x.UnRegisterPropertyHandler(f => f.IsSelected, FieldPropertyChanged));
                _mcdaExtension.SelectedLayer.Fields.ForEach(x => x.RegisterPropertyHandler(f => f.IsSelected, FieldPropertyChanged));
            }

            RegisterToolParameterEvents();

            PropertyChanged.Notify(() => OWAParameter);
            PropertyChanged.Notify(() => OWAResult);
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

            _owaTool.Run();
            _owaResultDataTable = _owaTool.Data;

            if (_isSendToInMemoryWorkspaceCommand)
                ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<AbstractToolTemplate, DataTable>)_mcdaExtension.JoinToolResultByOID, _owaTool, _owaTool.Data);

            _isUpdateAllowed = false;

        }

        protected override void UpdateRealtime()
        {
            _owaTool.Run();
            _owaResultDataTable = _owaTool.Data;

            if (_isSendToInMemoryWorkspaceCommand)
                ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<AbstractToolTemplate, DataTable>)_mcdaExtension.JoinToolResultByOID, _owaTool, _owaTool.Data);
        }

        protected override void UpdateAnimation()
        {
            if (!_isUpdateAllowed)
            {
                List<IToolParameter> tList = new List<IToolParameter>();

                for (int i = 0; i < _toolParameter.Count; i++)
                {
                    tList.Add(_toolParameter[i].DeepClone());
                }

                _toolParameterStorageForAnimationLikeUpdate.Add(tList);
            }

            else
            {
                BindingList<IToolParameter> latestToolParameter = _toolParameter;

                if (_toolParameterStorageForAnimationLikeUpdate.Count > 0)
                {
                    int steps = (int)Math.Sqrt(_toolParameterStorageForAnimationLikeUpdate.Count);
                    //take several steps...
                    for (int i = 0; i < _toolParameterStorageForAnimationLikeUpdate.Count; i = i + steps)
                    {
                        _owaTool.ToolParameterContainer.ToolParameter = _toolParameterStorageForAnimationLikeUpdate[i];
                        _owaTool.Run();
                        _owaResultDataTable = _owaTool.Data;

                        if (_isSendToInMemoryWorkspaceCommand)
                            ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<AbstractToolTemplate, DataTable>)_mcdaExtension.JoinToolResultByOID, _owaTool, _owaTool.Data);
                    }
                }

                //make sure we add the latest one
                _owaTool.ToolParameterContainer.ToolParameter = latestToolParameter;
                _owaTool.Run();
                _owaResultDataTable = _owaTool.Data;

                if (_isSendToInMemoryWorkspaceCommand)
                    ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<AbstractToolTemplate, DataTable>)_mcdaExtension.JoinToolResultByOID, _owaTool, _owaTool.Data);

                _isUpdateAllowed = false;

                _toolParameterStorageForAnimationLikeUpdate.Clear();
            }
        }

        protected override void AfterUpdate()
        {
            PropertyChanged.Notify(() => OWAResult);
        }

        public BindingList<IToolParameter> OWAParameter
        {
            get { return _toolParameter; }
            set { _toolParameter = value; }
        }
        public DataView OWAResult
        {
            get { return _owaResultDataTable.DefaultView; }
        }

        public bool IsLocked
        {
            get { return _isLocked; }
            set { _isLocked = value; }
        }

        public bool IsSendToInMemoryWorkspaceCommand
        {
            get { return _isSendToInMemoryWorkspaceCommand; }
            set { _isSendToInMemoryWorkspaceCommand = value; }
        }

        protected override void DoExportAsCSVCommand()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = _owaTool.ToString();
            saveFileDialog.DefaultExt = ".csv";
            saveFileDialog.Filter = "Comma Separated Values (.csv)|*.csv";

            Nullable<bool> result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                Export.ToCSV(_owaTool.Data, _owaTool.ToolParameterContainer.ToolParameter, saveFileDialog.FileName);
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
                {
                    _isSendToInMemoryWorkspaceCommand = !_isSendToInMemoryWorkspaceCommand;
                    _mcdaExtension.RemoveLink(_owaTool);
                    this.MCDAExtensionPropertyChanged(this, null);
                }

                PropertyChanged.Notify(() => IsSendToInMemoryWorkspaceCommand);
            }

            if (_isLocked)
                ProgressDialog.ShowProgressDialog("Creating In Memory Representation", (Action<AbstractToolTemplate>)_mcdaExtension.EstablishLink, _owaTool);

            if (!_isLocked)
            {
                _mcdaExtension.RemoveLink(_owaTool);
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
                _mcdaExtension.DisplayLink(_owaTool);
                ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<AbstractToolTemplate, DataTable>)_mcdaExtension.JoinToolResultByOID, _owaTool, _owaTool.Data);
            }

            if (!_isSendToInMemoryWorkspaceCommand)
            {
                //_mcdaExtension.RemoveLink(_owaTool);
                DoLockCommand();
            }

            PropertyChanged.Notify(() => IsSendToInMemoryWorkspaceCommand);
        }

        protected override void DoStandardizationSelectionCommand()
        {

            var parentHandle = new IntPtr(ArcMap.Application.hWnd);

            var wpfWindow = new StandardizationSelectionView();

            StandardizationSelectionViewModel standardizationSelectionViewModel = wpfWindow.DataContext as StandardizationSelectionViewModel;

            standardizationSelectionViewModel.SelectedTransformationStrategy = _owaTool.TransformationStrategy;

            var helper = new WindowInteropHelper(wpfWindow);

            helper.Owner = parentHandle;

            wpfWindow.Closed += delegate(object sender, EventArgs e)
            {

                _owaTool.TransformationStrategy = standardizationSelectionViewModel.SelectedTransformationStrategy;

                _owaTool.Run();
                _owaResultDataTable = _owaTool.Data;

                _isUpdateAllowed = true;

                base.Update();

                PropertyChanged.Notify(() => OWAResult);

            };

            wpfWindow.ShowDialog();
        }

        public ICommand AlphaSelectionCommand
        {
            get
            {

                if (_alphaSelectionCommand == null)
                {
                    _alphaSelectionCommand = new RelayCommand(
                        p => this.DoAlphaSelectionCommand(),
                        p => true
                        );
                }

                return _alphaSelectionCommand;
            }
        }

        public void DoAlphaSelectionCommand()
        {
            var parentHandle = new IntPtr(ArcMap.Application.hWnd);

            var wpfWindow = new AlphaSelectionView();

            AlphaSelectionViewModel alphaSelectionViewModel = wpfWindow.DataContext as AlphaSelectionViewModel;

            alphaSelectionViewModel.Alpha = _owaTool.Alpha;

            var helper = new WindowInteropHelper(wpfWindow);

            helper.Owner = parentHandle;

            wpfWindow.Closed += delegate(object sender, EventArgs e)
            {

                _owaTool.Alpha = alphaSelectionViewModel.Alpha;

                _owaTool.Run();
                _owaResultDataTable = _owaTool.Data;

                _isUpdateAllowed = true;

                base.Update();

                PropertyChanged.Notify(() => OWAResult);

            };

            wpfWindow.ShowDialog();
        }

        protected override void DoDistributionCommand()
        {
            _owaTool.ToolParameterContainer.DistributeEquallyToolParameterWeights();

            _owaTool.Run();

            _owaResultDataTable = _owaTool.Data;

            PropertyChanged.Notify(() => OWAParameter);
            PropertyChanged.Notify(() => OWAResult);

            UpdateRealtime();
        }

        protected override void DoClosingCommand()
        {
            if (_isLocked || _isSendToInMemoryWorkspaceCommand)
            {
                _mcdaExtension.RemoveLink(_owaTool); 
            }
        }
    }
}
