using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Model;
using ESRI.ArcGIS.Carto;
using System.ComponentModel;
using MCDA.Extensions;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Threading;

namespace MCDA.ViewModel
{
    internal sealed class VisualizationViewModel : INotifyPropertyChanged
	{
        public event PropertyChangedEventHandler PropertyChanged;

        private Color _selectedStartColor, _selectedEndColor; 

        private BindingList<IClassify> _listOfClassificationMethod;
        private BindingList<int> _listOfNumberOfClasses;
        private int _selectedNumberOfClasses;
        private IClassify _selectedClassificationMethod;

        private ClassBreaksRendererContainer _classBreaksRendererContainer = new ClassBreaksRendererContainer();

        private double _biPolarColorSliderValue;
        private Color _selectedBiPolarNegativColor, _selectedBiPolarPositivColor, _selectedBiPolarNeutralColor;

        private BindingList<IRenderContainer> _resultList, _completeResultList;
        private IRenderContainer _selectedResult;

        private ICommand _applyClassBreaksRendererCommand;
        private ICommand _removeClassBreaksRendererCommand;

        private ICommand _applyBiPolarRendererCommand;
        private ICommand _removeBiPolarRendererCommand;

        //comment
        private bool _canClassBreaksRendererApplyCommandExecuted = false;
        private bool _canClassBreaksRendererRemoveCommandExecuted = false;
        private bool _isClassBreaksRendererApplyCommandExecuted = false;
        private bool _canBiPolarRendererApplyCommandExecuted = false;
        private bool _canBiPolarRendererRemoveCommandExecuted = false;
        private bool _isBiPolarRendererApplyCommandExecuted = false;
        //comment ende
        private IEnumerable<ICommand> applyCommands;
        private IEnumerable<ICommand> removeCommands;

        private MCDAExtension MCDAExtension = MCDAExtension.GetExtension();

        public VisualizationViewModel()
        {
            InitializeClassificationArguments();

            SelectedStartColor  = Color.FromRgb(255, 0, 0);
            SelectedEndColor  = Color.FromRgb(0, 255, 0);

            SelectedBiPolarNegativColor = Color.FromRgb(255, 0, 0);
            SelectedBiPolarPositivColor = Color.FromRgb(0, 255, 0);
            SelectedBiPolarNeutralColor = Color.FromRgb(255, 255, 255);

            BiPolarColorSliderValue = 50;

            MCDAExtension.RegisterPropertyHandler(x => x.LinkDictionary, LinkDictionaryPropertyChanged);
            MCDAExtension.RegisterPropertyHandler(x => x.AvailableLayer, AvailableLayerChanged);

            CreateResultList();
            CreateCompleteResultList();

            applyCommands = new List<ICommand> { ApplyBiPolarRendererCommand, ApplyClassBreaksRendererCommand };
            removeCommands = new List<ICommand> { RemoveBiPolarRendererCommand, RemoveClassBreaksRendererCommand };

            PropertyChanged.Notify(() => ResultList);
            PropertyChanged.Notify(() => CompleteResultList);
        }

        private void AvailableLayerChanged(object sender, PropertyChangedEventArgs e)
        {
            CreateCompleteResultList();

            //the user selected something, but it is no longer available
            if(_selectedResult != null && _completeResultList.Contains(_selectedResult))
                DisableAllRemoveButtons();

            _selectedResult = _completeResultList.FirstOrDefault(r => r.Equals(_selectedResult));

            PropertyChanged.Notify(() => ResultList);
            PropertyChanged.Notify(() => SelectedResult);
            PropertyChanged.Notify(() => CompleteResultList);
        }

        private void LinkDictionaryPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CreateResultList();


            if (_selectedResult == null)
                DisableAllRemoveButtons();

            _selectedResult = _resultList.FirstOrDefault(r => r.Equals(_selectedResult));

            PropertyChanged.Notify(() => ResultList);
            PropertyChanged.Notify(() => SelectedResult);
            PropertyChanged.Notify(() => CompleteResultList);
        }

        public BindingList<IRenderContainer> ResultList
        {
            get { return _resultList; }
        }

        public BindingList<IRenderContainer> CompleteResultList
        {
            get { return _completeResultList; }
        }

        public IRenderContainer SelectedResult
        {
            get { return _selectedResult; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _selectedResult, value, () => SelectedResult);

            SelectedResultChanged();

            }
        }

        #region class breaks renderer
        public BindingList<int> NumberOfClasses
        {
            get { return _listOfNumberOfClasses; }
        }

        public BindingList<IClassify> ListOfClassificationMethod
        {
            get { return _listOfClassificationMethod; }  
        }

        
        public int SelectedNumberOfClasses
        {
            get { return _selectedNumberOfClasses; }
            set { 
                
                _selectedNumberOfClasses = value;

                ClassBreaksRendererValuesChanged();
                //for the histogram
                PropertyChanged.Notify(() => SelectedNumberOfClasses);


            }
        }

        public IClassify SelectedClassificationMethod
        {
            get { return _selectedClassificationMethod; }
            set { 
                
                _selectedClassificationMethod = value;

                ClassBreaksRendererValuesChanged();

                //for the histogram
                PropertyChanged.Notify(() => SelectedClassificationMethod);

            }
        }

        public Color SelectedStartColor
        {
            get { return _selectedStartColor;  }
            set {

                _selectedStartColor = value;
                ClassBreaksRendererValuesChanged();
            }
        }

        public Color SelectedEndColor
        {
            get { return _selectedEndColor; }
            set
            {
                _selectedEndColor = value;
                ClassBreaksRendererValuesChanged();
            }
        }
        #endregion

        #region bi polar renderer

        public double BiPolarColorSliderValue
        {
            get { return _biPolarColorSliderValue; }
            set { 
                
                _biPolarColorSliderValue = value;
                //no more update after slider change, this takes to long on normal machines
                //from the code behind page if slider draged
                //BiPolarRendererValuesChanged();
            
            }
        }

        public Color SelectedBiPolarNegativColor
        {
            get { return _selectedBiPolarNegativColor; }
            set
            {
                _selectedBiPolarNegativColor = value;
                BiPolarRendererValuesChanged();
            }
        }

        public Color SelectedBiPolarPositivColor
        {
            get { return _selectedBiPolarPositivColor; }
            set
            {
                _selectedBiPolarPositivColor = value;
                BiPolarRendererValuesChanged();
            }
        }

        public Color SelectedBiPolarNeutralColor
        {
            get { return _selectedBiPolarNeutralColor; }
            set
            {
                _selectedBiPolarNeutralColor = value;
                BiPolarRendererValuesChanged();
            }
        }

        #endregion

        private void Render()
        {
            if(_selectedResult != null)
                ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<IRenderContainer>)MCDAExtension.Render, _selectedResult.RenderContainer);
        }

        private void CreateCompleteResultList()
        {
            _completeResultList = new BindingList<IRenderContainer>();

            foreach (Layer currentLayer in MCDAExtension.AvailableFeatureLayer)
            {
                foreach(MCDA.Model.Field currentField in currentLayer.Fields){

                    //RenderContainer tempRenderContainer = new RenderContainer();
                    //tempRenderContainer.FeatureClass = currentLayer.FeatureLayer.FeatureClass;
                    //tempRenderContainer.FeatureLayer = (IFeatureLayer) currentLayer.FeatureLayer;

                    //IRenderContainer temp = new RenderContainer();
                    //temp.Field = currentField.ESRIField;
                    //temp.RenderContainer = tempRenderContainer;

                    _completeResultList.Add(currentField.RenderContainer); 
                }

                //_completeResultList.OrderBy(rC => rC.FeatureLayerName).ThenBy(rC => rC.FieldName);
            }
        }

        private void CreateResultList()
        {
            _resultList = new BindingList<IRenderContainer>();

            foreach (MCDAWorkspaceContainer currentMCDAWorkspaceContainer in MCDAExtension.GetAllMCDAWorkspaceContainerFromShadowWorkspace())
            {
                //this list should include only one element, because one tool has only one result
                IList<IField> fields = MCDAExtension.GetExtension().GetListOfFieldsFromFeatureClass(currentMCDAWorkspaceContainer.FeatureClass).Where(f => f.Name.Equals(currentMCDAWorkspaceContainer.Tool.DefaultResultColumnName)).ToList();

                    //IRenderContainer temp = new RenderContainer();
                    MCDA.Model.Field field = fields.FirstOrDefault();
                    //temp.RenderContainer = currentMCDAWorkspaceContainer;

                    // in case the tool had not a run yet we will not add it, as it does not contain any result data
                    if(field == null)
                        continue;

                    _resultList.Add(field);    
            }
        }

        private void InitializeClassificationArguments()
        {
            QuantileClass quantileClass = new QuantileClass();

            _selectedClassificationMethod = quantileClass;

            _listOfClassificationMethod = new BindingList<IClassify>();
            _listOfClassificationMethod.Add(new NaturalBreaksClass());
            _listOfClassificationMethod.Add(quantileClass);
            _listOfClassificationMethod.Add(new EqualIntervalClass());
            _listOfClassificationMethod.Add(new GeometricalIntervalClass());

            _listOfNumberOfClasses =  new BindingList<int>(Enumerable.Range(2, 19).ToList());

            _selectedNumberOfClasses = 5;
        }

        private BiPolarRendererContainer GetBiPolarContainer()
        {
            return new BiPolarRendererContainer()
            {
                Field = _selectedResult.Field,
                NegativColor = _selectedBiPolarNegativColor,
                PositivColor = _selectedBiPolarPositivColor,
                NeutralColor = _selectedBiPolarNeutralColor,
                NeutralColorPosition = _biPolarColorSliderValue
            };
        }

        private ClassBreaksRendererContainer GetClassBreaksRendererContainer()
        {
            return new ClassBreaksRendererContainer()
            {
                Field = _selectedResult.Field,
                ClassificationMethod = _selectedClassificationMethod,
                EndColor = _selectedEndColor,
                StartColor = _selectedStartColor,
                NumberOfClasses = _selectedNumberOfClasses
            };
        }

        private void RendererContainerToView()
        {
            if (_selectedResult != null)
            {
                if (_selectedResult.Renderer == Renderer.None)
                    return;

                if (_selectedResult.Renderer == Renderer.ClassBreaksRenderer)
                {
                    _selectedClassificationMethod = _selectedResult.ClassBreaksRendererContainer.ClassificationMethod;
                    _selectedNumberOfClasses = _selectedResult.ClassBreaksRendererContainer.NumberOfClasses;
                    _selectedStartColor = _selectedResult.ClassBreaksRendererContainer.StartColor;
                    _selectedEndColor = _selectedResult.ClassBreaksRendererContainer.EndColor;

                    PropertyChanged.Notify(() => SelectedClassificationMethod);
                    PropertyChanged.Notify(() => SelectedNumberOfClasses);
                    PropertyChanged.Notify(() => SelectedStartColor);
                    PropertyChanged.Notify(() => SelectedEndColor);
                }

                if (_selectedResult.Renderer == Renderer.BiPolarRenderer)
                {
                    _selectedBiPolarNegativColor = _selectedResult.BiPolarRendererContainer.NegativColor;
                    _selectedBiPolarPositivColor = _selectedResult.BiPolarRendererContainer.PositivColor;
                    _selectedBiPolarNeutralColor = _selectedResult.BiPolarRendererContainer.NeutralColor;
                    _biPolarColorSliderValue = _selectedResult.BiPolarRendererContainer.NeutralColorPosition;

                    PropertyChanged.Notify(() => SelectedBiPolarNegativColor);
                    PropertyChanged.Notify(() => SelectedBiPolarNeutralColor);
                    PropertyChanged.Notify(() => SelectedBiPolarPositivColor);
                    PropertyChanged.Notify(() => BiPolarColorSliderValue);
                }
            }
        }

        public void BiPolarRendererValuesChanged()
        {
            if (_isBiPolarRendererApplyCommandExecuted)
            {
                //TODO hier hat es gekracht nachdem ich das wlc tool geschlossen habe und apply machen wollte (NPE)
                _selectedResult.BiPolarRendererContainer = GetBiPolarContainer();
                _selectedResult.Renderer = Renderer.BiPolarRenderer;

                Render();
            }
        }

        public void ClassBreaksRendererValuesChanged()
        {
            if (_isClassBreaksRendererApplyCommandExecuted)
            {
                _selectedResult.ClassBreaksRendererContainer = GetClassBreaksRendererContainer();
                _selectedResult.Renderer = Renderer.ClassBreaksRenderer;

                Render();
            }
        }

        private void SelectedResultChanged()
        {
            _isBiPolarRendererApplyCommandExecuted = false;
            _isClassBreaksRendererApplyCommandExecuted = false;

            if (_selectedResult != null)
            {
                switch (_selectedResult.Renderer)
                {
                    case Renderer.None: ApplyClassBreaksRendererCommand.CanExecute(true); ApplyClassBreaksRendererCommand.CanExecute(true);
                        break;
                    case Renderer.BiPolarRenderer: RemoveBiPolarRendererCommand.CanExecute(true); /*_isBiPolarRendererApplyCommandExecuted = true;*/ ApplyClassBreaksRendererCommand.CanExecute(true);                                                  
                        break;
                    case Renderer.ClassBreaksRenderer: /*_isClassBreaksRendererApplyCommandExecuted = true;*/; RemoveClassBreaksRendererCommand.CanExecute(true); ApplyBiPolarRendererCommand.CanExecute(true);
                        break;             
                }

                RendererContainerToView();
            }
        }

        #region commands

        public ICommand ApplyClassBreaksRendererCommand
        {
            get
            {
                if (_applyClassBreaksRendererCommand == null)
                    _applyClassBreaksRendererCommand = new RelayCommand(p => this.DoApplyClassBreaksRendererCommand(), p => false);

                return _applyClassBreaksRendererCommand;
            }
        }

        public ICommand RemoveClassBreaksRendererCommand
        {
            get
            {
                if (_removeClassBreaksRendererCommand == null)
                    _removeClassBreaksRendererCommand = new RelayCommand( p => this.DoRemoveClassBreaksRendererCommand(), p => false);

                return _removeClassBreaksRendererCommand;
            }
        }

        public ICommand ApplyBiPolarRendererCommand
        {
            get
            {
                if (_applyBiPolarRendererCommand == null)
                    _applyBiPolarRendererCommand = new RelayCommand( p => this.DoApplyBiPolarRendererCommand(), p => false);

                return _applyBiPolarRendererCommand;
            }
        }

        public ICommand RemoveBiPolarRendererCommand
        {
            get
            {
                if (_removeBiPolarRendererCommand == null)
                 _removeBiPolarRendererCommand = new RelayCommand( p => this.DoRemoveBiPolarRendererCommand(), p => false);
              
                return _removeBiPolarRendererCommand;
            }
        }

        #endregion

        #region implemented commands

        private void ApplyCommand(ICommand applyCommand)
        {

        }

        private void DoApplyClassBreaksRendererCommand(){

            _isClassBreaksRendererApplyCommandExecuted = true;
            ApplyClassBreaksRendererCommand.CanExecute(true);
            RemoveClassBreaksRendererCommand.CanExecute(true);

            _isBiPolarRendererApplyCommandExecuted = false;
            ApplyBiPolarRendererCommand.CanExecute(true);
            RemoveBiPolarRendererCommand.CanExecute(true);
            
            ClassBreaksRendererValuesChanged();
        }

        private void DoRemoveClassBreaksRendererCommand()
        {
            _isClassBreaksRendererApplyCommandExecuted = false;
            ApplyClassBreaksRendererCommand.CanExecute(true);
            RemoveClassBreaksRendererCommand.CanExecute(false);

            _isBiPolarRendererApplyCommandExecuted = false;
            _canBiPolarRendererApplyCommandExecuted = true;
            _canBiPolarRendererRemoveCommandExecuted = false;

            _selectedResult.RenderContainer.Renderer = Renderer.None;

            Render();
        }

        private void DoApplyBiPolarRendererCommand()
        {
            
            _canBiPolarRendererApplyCommandExecuted = false;
            _canBiPolarRendererRemoveCommandExecuted = true;
            _isBiPolarRendererApplyCommandExecuted = true;

            _isClassBreaksRendererApplyCommandExecuted = false;
            ApplyClassBreaksRendererCommand.CanExecute(false);
            RemoveClassBreaksRendererCommand.CanExecute(true);

            BiPolarRendererValuesChanged();
        }

        private void DoRemoveBiPolarRendererCommand()
        {
            _isClassBreaksRendererApplyCommandExecuted = false;
            _canClassBreaksRendererApplyCommandExecuted = true;
            _canClassBreaksRendererRemoveCommandExecuted = false;

            _isBiPolarRendererApplyCommandExecuted = false;
            _canBiPolarRendererApplyCommandExecuted = true;
            _canBiPolarRendererRemoveCommandExecuted = false;

            _selectedResult.RenderContainer.Renderer = Renderer.None;

            Render();
        }

        /// <summary>
        /// Diasbles all apply and remove buttons. Notifies the listeners.
        /// </summary>
        private void DisableAllRemoveButtons()
        {        
            //ApplyBiPolarRendererCommand.CanExecute(false);
            RemoveBiPolarRendererCommand.CanExecute(false);
            _isBiPolarRendererApplyCommandExecuted = false;

            //ApplyClassBreaksRendererCommand.CanExecute(false);
            RemoveClassBreaksRendererCommand.CanExecute(false);
            _isClassBreaksRendererApplyCommandExecuted = false;
        }

        #endregion

        //public class ResultContainer
        //{
        //    public IField Field { get; set; }
        //    public IRenderContainer RenderContainer { get; set; }

        //    // why additional properties? for some reason the binding does not work with ESRIs COM objects.
        //    public string FieldName { get { return Field.Name; } }

        //    public string FeatureLayerName { get { return RenderContainer.FeatureLayer.Name; } }

        //    public override bool Equals(object obj)
        //    { 
        //        ResultContainer arg = obj as ResultContainer;

        //        if(arg == null)
        //            return false;

        //        return arg.Field == Field && arg.RenderContainer == RenderContainer;
        //    }
        //}

	}
}
