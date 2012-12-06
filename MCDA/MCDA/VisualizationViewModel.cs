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
        //private BindingList<MCDAWorkspaceContainer> _listOfMCDAWorkspaceContainer;
        //private BindingList<IField> _listOfFields;
        //private MCDAWorkspaceContainer _selectedMCDAWorkspaceContainer;

        private BindingList<IClassify> _listOfClassificationMethod;
        private BindingList<int> _listOfNumberOfClasses;
        private int _selectedNumberOfClasses;
        private IClassify _selectedClassificationMethod;

        private PointCollection _histogramPointCollection;

        private ClassBreaksRendererContainer _classBreaksRendererContainer = new ClassBreaksRendererContainer();
        private PointCollection _breaksPointCollection;

        private double _biPolarColorSliderValue;
        private Color _selectedBiPolarNegativColor, _selectedBiPolarPositivColor, _selectedBiPolarNeutralColor;

        private BindingList<X> _resultList;
        private X _selectedResult;

        private ICommand _applyClassBreaksRendererCommand;
        private ICommand _removeClassBreaksRendererCommand;

        private ICommand _applyBiPolarRendererCommand;
        private ICommand _removeBiPolarRendererCommand;

        private bool _isClassBreaksRendererCommandExecuted = false;
        private bool _isBiPolarRendererCommandExecuted = false;

        private MCDAExtension MCDAExtension = MCDAExtension.GetExtension();

        public VisualizationViewModel()
        {
            InitializeClassificationArguments();

            SelectedStartColor  = Color.FromRgb(255, 0, 0);
            SelectedEndColor  = Color.FromRgb(0, 255, 0);

            SelectedBiPolarNegativColor = Color.FromRgb(255, 0, 0);
            SelectedBiPolarPositivColor = Color.FromRgb(0, 255, 0);
            SelectedBiPolarNeutralColor = Color.FromRgb(255, 255, 255);

            //_listOfMCDAWorkspaceContainer = new BindingList<MCDAWorkspaceContainer>(_MCDAExtension.GetAllMCDAWorkspaceContainerFromShadowWorkspace());
          
            MCDAExtension.RegisterPropertyHandler(x => x.LinkDictionary, LinkDictionaryPropertyChanged);

            Y();

            PropertyChanged.Notify(() => ResultList);
        }

        void LinkDictionaryPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            // dafue sorgen, dass das selected selected leibt wenn noch vorhanden!!!!

            /*
            IList<MCDAWorkspaceContainer> _mcdaWorkspaceContainer = _MCDAExtension.GetAllMCDAWorkspaceContainerFromShadowWorkspace();

            //the selected container is still in the in memory workspace
            if (_mcdaWorkspaceContainer.Any(c => c == _selectedMCDAWorkspaceContainer))
            {
                ClassBreaksRendererContainerToView();
                Render();
            }
            else
            {
                _selectedIField = null;
                SetFields();
            }

            _listOfMCDAWorkspaceContainer = new BindingList<MCDAWorkspaceContainer>(_mcdaWorkspaceContainer);
            */

            Y();

            //PropertyChanged.Notify(() => MCDAWorkspaceContainer);
            //PropertyChanged.Notify(() => SelectedMCDAWorkspaceContainer);
            //PropertyChanged.Notify(() => Fields);
            PropertyChanged.Notify(() => ResultList);
        }

        public BindingList<X> ResultList
        {
            get { return _resultList; }
        }

        public X SelectedResult
        {
            get { return _selectedResult; }
            set { _selectedResult = value;

            RendererContainerToView();

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
            //set { _listOfClassificationMethod = value; }
        }

        
        public int SelectedNumberOfClasses
        {
            get { return _selectedNumberOfClasses; }
            set { 
                
                _selectedNumberOfClasses = value;
                _classBreaksRendererContainer.NumberOfClasses = value;

                //for the histogram
                PropertyChanged.Notify(() => SelectedNumberOfClasses);

                Render();
            }
        }

        public IClassify SelectedClassificationMethod
        {
            get { return _selectedClassificationMethod; }
            set { 
                
                _selectedClassificationMethod = value;
                _classBreaksRendererContainer.ClassificationMethod = value;

                //for the histogram
                PropertyChanged.Notify(() => SelectedClassificationMethod);

                Render();
            }
        }

        public Color SelectedStartColor
        {
            get { return _selectedStartColor;  }
            set { 

                _selectedStartColor = value;
                _classBreaksRendererContainer.StartColor = value;

                Render();
            }
        }

        public Color SelectedEndColor
        {
            get { return _selectedEndColor; }
            set
            {
                _selectedEndColor = value;
                _classBreaksRendererContainer.EndColor = value;

                Render();
            }
        }
        #endregion

        #region bi polar renderer

        public double BiPolarColorSliderValue
        {
            get { return _biPolarColorSliderValue; }
            set { _biPolarColorSliderValue = value; }
        }

        public Color SelectedBiPolarNegativColor
        {
            get { return _selectedBiPolarNegativColor; }
            set
            {
                _selectedBiPolarNegativColor = value;
                Render();
            }
        }

        public Color SelectedBiPolarPositivColor
        {
            get { return _selectedBiPolarPositivColor; }
            set
            {
                _selectedBiPolarPositivColor = value;
                Render();
            }
        }

        public Color SelectedBiPolarNeutralColor
        {
            get { return _selectedBiPolarNeutralColor; }
            set
            {
                _selectedBiPolarNeutralColor = value;
               
                Render();
            }
        }

        #endregion

        private void Render()
        {
            
            if(_isBiPolarRendererCommandExecuted || _isClassBreaksRendererCommandExecuted)
                MCDAExtension.Render(_selectedResult.MCDAWorkspaceContainer);

        }

        private void Y()
        {
            _resultList = new BindingList<X>();

            foreach (MCDAWorkspaceContainer currentMCDAWorkspaceContainer in MCDAExtension.GetAllMCDAWorkspaceContainerFromShadowWorkspace())
            {
                //this list should include only one element, becauseone tool has only one result
                IList<IField> fields = MCDAExtension.GetExtension().GetListOfFieldsFromFeatureClass(currentMCDAWorkspaceContainer.FeatureClass).Where(f => f.Name.Equals(currentMCDAWorkspaceContainer.Tool.DefaultResultColumnName)).ToList();

                    X temp = new X();
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

        private void SelectedResultChanged()
        {
            if (_selectedResult == null)
            {
                //_applyClassBreaksRendererCommand.
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
                        p => !_isClassBreaksRendererCommandExecuted );
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
                        p => _isClassBreaksRendererCommandExecuted);
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
                        p => !_isBiPolarRendererCommandExecuted);
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
                    _removeClassBreaksRendererCommand = new RelayCommand(
                        p => this.DoRemoveBiPolarRendererCommand(),
                        p => _isBiPolarRendererCommandExecuted);
                }
                return _removeBiPolarRendererCommand;
            }
        }

        #endregion

        private void DoApplyClassBreaksRendererCommand(){

            _isClassBreaksRendererCommandExecuted = true;

            ClassBreaksRendererContainer container = new ClassBreaksRendererContainer();

            container.ClassificationMethod = _selectedClassificationMethod;
            container.EndColor = _selectedEndColor;
            container.StartColor = _selectedStartColor;
            container.NumberOfClasses = _selectedNumberOfClasses;
            container.Field = _selectedResult.Field;

            _selectedResult.MCDAWorkspaceContainer.ClassBreaksRendererContainer = container;
            _selectedResult.MCDAWorkspaceContainer.Renderer = Renderer.ClassBreaksRenderer;
        }

        private void DoRemoveClassBreaksRendererCommand()
        {
            _isClassBreaksRendererCommandExecuted = false;
            _selectedResult.MCDAWorkspaceContainer.Renderer = Renderer.None;
        }

        private void DoApplyBiPolarRendererCommand()
        {
            _isBiPolarRendererCommandExecuted = true;

            BiPolarRendererContainer container = new BiPolarRendererContainer();

            container.NeutralColor = _selectedBiPolarNeutralColor;
            container.NegativColor = _selectedBiPolarNegativColor;
            container.PositivColor = _selectedBiPolarPositivColor;
            container.NeutralColorPosition = _biPolarColorSliderValue;
            container.Field = _selectedResult.Field;
            
        }

        private void DoRemoveBiPolarRendererCommand()
        {
            _isBiPolarRendererCommandExecuted = false;
            _selectedResult.MCDAWorkspaceContainer.Renderer = Renderer.None;
        }

        public class X
        {
            public IField Field { get; set; }
            public MCDAWorkspaceContainer MCDAWorkspaceContainer { get; set; }
        }

	}
}
