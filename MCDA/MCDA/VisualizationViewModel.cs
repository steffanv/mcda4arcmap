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

namespace MCDA.ViewModel
{
    class VisualizationViewModel : INotifyPropertyChanged
	{
        public event PropertyChangedEventHandler PropertyChanged;

        private Color _selectedStartColor, _selectedEndColor; 

        private BindingList<IClassify> _listOfClassificationMethod;
        private BindingList<int> _listOfNumberOfClasses;
        private int _selectedNumberOfClasses;
        private IClassify _selectedClassificationMethod;

        private PointCollection _histogramPointCollection;

        private ClassBreaksRendererContainer _classBreaksRendererContainer = new ClassBreaksRendererContainer();
        private PointCollection _breaksPointCollection;

        private double _biPolarColorSliderValue;
        private Color _selectedBiPolarNegativColor, _selectedBiPolarPositivColor, _selectedBiPolarNeutralColor;

        private BindingList<ResultContainer> _resultList;
        private ResultContainer _selectedResult;

        private ICommand _applyClassBreaksRendererCommand;
        private ICommand _removeClassBreaksRendererCommand;

        private ICommand _applyBiPolarRendererCommand;
        private ICommand _removeBiPolarRendererCommand;

        private bool _canClassBreaksRendererApplyCommandExecuted = false;
        private bool _canClassBreaksRendererRemoveCommandExecuted = false;
        private bool _isClassBreaksRendererApplyCommandExecuted = false;
        private bool _canBiPolarRendererApplyCommandExecuted = false;
        private bool _canBiPolarRendererRemoveCommandExecuted = false;
        private bool _isBiPolarRendererApplyCommandExecuted = false;

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

            CreateResultList();

            PropertyChanged.Notify(() => ResultList);
        }

        void LinkDictionaryPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            CreateResultList();

            _selectedResult = _resultList.FirstOrDefault(r => r.Equals(_selectedResult));
    
            PropertyChanged.Notify(() => ResultList);
            PropertyChanged.Notify(() => SelectedResult);
        }


        public BindingList<ResultContainer> ResultList
        {
            get { return _resultList; }
        }

        public ResultContainer SelectedResult
        {
            get { return _selectedResult; }
            set { _selectedResult = value;

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

                ClassBreaksRendererValuesChanged();
                _selectedStartColor = value;

            }
        }

        public Color SelectedEndColor
        {
            get { return _selectedEndColor; }
            set
            {
                ClassBreaksRendererValuesChanged();
                _selectedEndColor = value;

            }
        }
        #endregion

        #region bi polar renderer

        public double BiPolarColorSliderValue
        {
            get { return _biPolarColorSliderValue; }
            set { 
                
                _biPolarColorSliderValue = value;
                BiPolarRendererValuesChanged();
            
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
                MCDAExtension.Render(_selectedResult.MCDAWorkspaceContainer);
        }

        private void CreateResultList()
        {
            _resultList = new BindingList<ResultContainer>();

            foreach (MCDAWorkspaceContainer currentMCDAWorkspaceContainer in MCDAExtension.GetAllMCDAWorkspaceContainerFromShadowWorkspace())
            {
                //this list should include only one element, becauseone tool has only one result
                IList<IField> fields = MCDAExtension.GetExtension().GetListOfFieldsFromFeatureClass(currentMCDAWorkspaceContainer.FeatureClass).Where(f => f.Name.Equals(currentMCDAWorkspaceContainer.Tool.DefaultResultColumnName)).ToList();

                    ResultContainer temp = new ResultContainer();
                    temp.Field = fields.FirstOrDefault();
                    temp.MCDAWorkspaceContainer = currentMCDAWorkspaceContainer;

                    _resultList.Add(temp);    
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
            BiPolarRendererContainer container = new BiPolarRendererContainer();

            container.Field = _selectedResult.Field;
            container.NegativColor = _selectedBiPolarNegativColor;
            container.PositivColor = _selectedBiPolarPositivColor;
            container.NeutralColor = _selectedBiPolarNeutralColor;
            container.NeutralColorPosition = _biPolarColorSliderValue;

            return container;
        }

        private ClassBreaksRendererContainer GetClassBreaksRendererContainer()
        {
            ClassBreaksRendererContainer container = new ClassBreaksRendererContainer();

            container.Field = _selectedResult.Field;
            container.ClassificationMethod = _selectedClassificationMethod;
            container.EndColor = _selectedEndColor;
            container.StartColor = _selectedStartColor;
            container.NumberOfClasses = _selectedNumberOfClasses;

            return container;
        }

        private void RendererContainerToView()
        {
            if (_selectedResult != null)
            {
                if (_selectedResult.MCDAWorkspaceContainer.Renderer == Renderer.None)
                    return;

                if (_selectedResult.MCDAWorkspaceContainer.Renderer == Renderer.ClassBreaksRenderer)
                {
                    _selectedClassificationMethod = _selectedResult.MCDAWorkspaceContainer.ClassBreaksRendererContainer.ClassificationMethod;
                    _selectedNumberOfClasses = _selectedResult.MCDAWorkspaceContainer.ClassBreaksRendererContainer.NumberOfClasses;
                    _selectedStartColor = _selectedResult.MCDAWorkspaceContainer.ClassBreaksRendererContainer.StartColor;
                    _selectedEndColor = _selectedResult.MCDAWorkspaceContainer.ClassBreaksRendererContainer.EndColor;

                    PropertyChanged.Notify(() => SelectedClassificationMethod);
                    PropertyChanged.Notify(() => SelectedNumberOfClasses);
                    PropertyChanged.Notify(() => SelectedStartColor);
                    PropertyChanged.Notify(() => SelectedEndColor);
                }

                if (_selectedResult.MCDAWorkspaceContainer.Renderer == Renderer.BiPolarRenderer)
                {
                    _selectedBiPolarNegativColor = _selectedResult.MCDAWorkspaceContainer.BiPolarRendererContainer.NegativColor;
                    _selectedBiPolarPositivColor = _selectedResult.MCDAWorkspaceContainer.BiPolarRendererContainer.PositivColor;
                    _selectedBiPolarNeutralColor = _selectedResult.MCDAWorkspaceContainer.BiPolarRendererContainer.NeutralColor;
                    _biPolarColorSliderValue = _selectedResult.MCDAWorkspaceContainer.BiPolarRendererContainer.NeutralColorPosition;

                    PropertyChanged.Notify(() => SelectedBiPolarNegativColor);
                    PropertyChanged.Notify(() => SelectedBiPolarNeutralColor);
                    PropertyChanged.Notify(() => SelectedBiPolarPositivColor);
                    PropertyChanged.Notify(() => BiPolarColorSliderValue);
                }
            }
        }


        private void BiPolarRendererValuesChanged()
        {
            if (_isBiPolarRendererApplyCommandExecuted)
            {
                _selectedResult.MCDAWorkspaceContainer.BiPolarRendererContainer = GetBiPolarContainer();
                _selectedResult.MCDAWorkspaceContainer.Renderer = Renderer.BiPolarRenderer;

                Render();
            }
        }

        private void ClassBreaksRendererValuesChanged()
        {
            if (_isClassBreaksRendererApplyCommandExecuted)
            {
                _selectedResult.MCDAWorkspaceContainer.ClassBreaksRendererContainer = GetClassBreaksRendererContainer();
                _selectedResult.MCDAWorkspaceContainer.Renderer = Renderer.ClassBreaksRenderer;

                Render();
            }
        }

        private void SelectedResultChanged()
        {
            _canBiPolarRendererApplyCommandExecuted = false;
            _canBiPolarRendererRemoveCommandExecuted = false;
            _isBiPolarRendererApplyCommandExecuted = false;

            _canClassBreaksRendererApplyCommandExecuted = false;
            _canClassBreaksRendererRemoveCommandExecuted = false;
            _isClassBreaksRendererApplyCommandExecuted = false;

            if (_selectedResult == null)
            {
            //...    
            }
            else
            {
                switch (_selectedResult.MCDAWorkspaceContainer.Renderer)
                {
                    case Renderer.None: _canBiPolarRendererApplyCommandExecuted = true; _canClassBreaksRendererApplyCommandExecuted = true;
                        break;
                    case Renderer.BiPolarRenderer: _canBiPolarRendererRemoveCommandExecuted = true; _isBiPolarRendererApplyCommandExecuted = true; _canClassBreaksRendererApplyCommandExecuted = true;                                                  
                        break;
                    case Renderer.ClassBreaksRenderer: _isClassBreaksRendererApplyCommandExecuted = true; _canClassBreaksRendererRemoveCommandExecuted = true; _canBiPolarRendererApplyCommandExecuted = true;
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
                {
                    _applyClassBreaksRendererCommand = new RelayCommand(
                        p => this.DoApplyClassBreaksRendererCommand(),
                        p => _canClassBreaksRendererApplyCommandExecuted);
                }
                return _applyClassBreaksRendererCommand;
            }
        }

        public ICommand RemoveClassBreaksRendererCommand
        {
            get
            {
                if (_removeClassBreaksRendererCommand == null)
                {
                    _removeClassBreaksRendererCommand = new RelayCommand(
                        p => this.DoRemoveClassBreaksRendererCommand(),
                        p => _canClassBreaksRendererRemoveCommandExecuted);
                }
                return _removeClassBreaksRendererCommand;
            }
        }

        public ICommand ApplyBiPolarRendererCommand
        {
            get
            {
                if (_applyBiPolarRendererCommand == null)
                {
                    _applyBiPolarRendererCommand = new RelayCommand(
                        p => this.DoApplyBiPolarRendererCommand(),
                        p => _canBiPolarRendererApplyCommandExecuted );
                }
                return _applyBiPolarRendererCommand;
            }
        }

        public ICommand RemoveBiPolarRendererCommand
        {
            get
            {
                if (_removeBiPolarRendererCommand == null)
                {
                    _removeBiPolarRendererCommand = new RelayCommand(
                        p => this.DoRemoveBiPolarRendererCommand(),
                        p => _canBiPolarRendererRemoveCommandExecuted);
                }
                return _removeBiPolarRendererCommand;
            }
        }

        #endregion

        #region implemented commands

        private void DoApplyClassBreaksRendererCommand(){

            _isClassBreaksRendererApplyCommandExecuted = true;
            _canClassBreaksRendererApplyCommandExecuted = false;
            _canClassBreaksRendererRemoveCommandExecuted = true;

            _isBiPolarRendererApplyCommandExecuted = false;
            _canBiPolarRendererApplyCommandExecuted = true;
            _canBiPolarRendererRemoveCommandExecuted = false;

            ClassBreaksRendererValuesChanged();
        }

        private void DoRemoveClassBreaksRendererCommand()
        {
            _isClassBreaksRendererApplyCommandExecuted = false;
            _canClassBreaksRendererApplyCommandExecuted = true;
            _canClassBreaksRendererRemoveCommandExecuted = false;

            _isBiPolarRendererApplyCommandExecuted = false;
            _canBiPolarRendererApplyCommandExecuted = true;
            _canBiPolarRendererRemoveCommandExecuted = false;

            _selectedResult.MCDAWorkspaceContainer.Renderer = Renderer.None;

            Render();
        }

        private void DoApplyBiPolarRendererCommand()
        {
            
            _canBiPolarRendererApplyCommandExecuted = false;
            _canBiPolarRendererRemoveCommandExecuted = true;
            _isBiPolarRendererApplyCommandExecuted = true;

            _isClassBreaksRendererApplyCommandExecuted = false;
            _canClassBreaksRendererApplyCommandExecuted = true;
            _canClassBreaksRendererRemoveCommandExecuted = false;

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

            _selectedResult.MCDAWorkspaceContainer.Renderer = Renderer.None;

            Render();
        }

        #endregion

        public class ResultContainer
        {
            public IField Field { get; set; }
            public MCDAWorkspaceContainer MCDAWorkspaceContainer { get; set; }

            public override bool Equals(object obj)
            { 
                ResultContainer arg = obj as ResultContainer;

                if(arg == null)
                    return false;

                return arg.Field == Field && arg.MCDAWorkspaceContainer == MCDAWorkspaceContainer;
            }
        }

	}
}
