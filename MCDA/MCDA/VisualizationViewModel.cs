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

namespace MCDA.ViewModel
{
    class VisualizationViewModel : INotifyPropertyChanged
	{
        public event PropertyChangedEventHandler PropertyChanged;

        private System.Windows.Media.Color _selectedStartColor, _selectedEndColor;

        private BindingList<MCDAWorkspaceContainer> _listOfMCDAWorkspaceContainer;
        private BindingList<IField> _listOfFields;
        private MCDAWorkspaceContainer _selectedMCDAWorkspaceContainer;

        private MCDAExtension _MCDAExtension = MCDAExtension.GetExtension();
        private IField _selectedIField;

        private BindingList<IClassify> _listOfClassificationMethod;
        private int _selectedNumberOfClasses;
        private IClassify _selectedClassificationMethod;

        private PointCollection _histogramPointCollection;

        private ClassBreaksRendererContainer _classBreaksRendererContainer = new ClassBreaksRendererContainer();
        private PointCollection _breaksPointCollection;

        public VisualizationViewModel()
        {
            InitializeClassificationMethods();

            SelectedStartColor  = Color.FromRgb(255, 0, 0);
            SelectedEndColor  = Color.FromRgb(0, 255, 0);

            _listOfMCDAWorkspaceContainer = new BindingList<MCDAWorkspaceContainer>(_MCDAExtension.GetAllMCDAWorkspaceContainerFromShadowWorkspace());
          
            _MCDAExtension.RegisterPropertyHandler(x => x.LinkDictionary, _MCDAExtensionPropertyChanged);

        }

        void _MCDAExtensionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IList<MCDAWorkspaceContainer> _mcdaWorkspaceContainer = _MCDAExtension.GetAllMCDAWorkspaceContainerFromShadowWorkspace();

            //the selected container is still in the in memory workspace
            if (_mcdaWorkspaceContainer.Any(c => c == _selectedMCDAWorkspaceContainer))
            {
                 //...
            }
            else
            {
                _selectedIField = null;
                SetFields();
            }

            _listOfMCDAWorkspaceContainer = new BindingList<MCDAWorkspaceContainer>(_mcdaWorkspaceContainer);

            PropertyChanged.Notify(() => MCDAWorkspaceContainer);
            PropertyChanged.Notify(() => SelectedMCDAWorkspaceContainer);
            PropertyChanged.Notify(() => Fields);
            PropertyChanged.Notify(() => SelectedIField);
        }

        public BindingList<MCDAWorkspaceContainer> MCDAWorkspaceContainer
        {
            get { return _listOfMCDAWorkspaceContainer; }
            set {  _listOfMCDAWorkspaceContainer = value; }
        }

        public BindingList<IField> Fields
        {
            get { return _listOfFields; }
            set { _listOfFields = value; }
        }

        public BindingList<int> NumberOfClasses
        {
            get { return new BindingList<int>(Enumerable.Range(2, 29).ToList()); }
        }

        public BindingList<IClassify> ListOfClassificationMethod
        {
            get { return _listOfClassificationMethod; }
            set { _listOfClassificationMethod = value; }
        }

        public MCDAWorkspaceContainer SelectedMCDAWorkspaceContainer
        {
            get { return _selectedMCDAWorkspaceContainer; }
            set {

                _selectedMCDAWorkspaceContainer = value;

                if (value != null)
                {
                    //the user defined values in a previous session
                    if (_selectedMCDAWorkspaceContainer.ClassBreaksRendererContainer != null)
                    {
                        ClassBreaksRendererContainerToView();
                    }
                    else
                    {
                        SetFields();
                        _selectedMCDAWorkspaceContainer.ClassBreaksRendererContainer = _classBreaksRendererContainer;
                    }
                }
            }
        }

        public IField SelectedIField
        {
            get { return _selectedIField; }
            set { 
                
            _selectedIField = value;
            _classBreaksRendererContainer.Field = value;

            PropertyChanged.Notify(() => SelectedIField);
            }
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

        public System.Windows.Media.Color SelectedStartColor
        {
            get { return _selectedStartColor;  }
            set { 

                _selectedStartColor = value;
                _classBreaksRendererContainer.StartColor = value;

                Render();
            
            }
        }

        public System.Windows.Media.Color SelectedEndColor
        {
            get { return _selectedEndColor; }
            set
            {
                _selectedEndColor = value;
                _classBreaksRendererContainer.EndColor = value;

                Render();

            }
        }

        private void Render()
        {
            if (_selectedNumberOfClasses > 1 && _selectedClassificationMethod != null)
            {
                _MCDAExtension.Render(_selectedMCDAWorkspaceContainer);
            }
        }

        private void SetFields()
        {
            if (_selectedMCDAWorkspaceContainer == null || _selectedMCDAWorkspaceContainer.FeatureClass == null)
            {
                _listOfFields = null;
            }
            else{

            _listOfFields = new BindingList<IField>( MCDAExtension.GetListOfFieldsFromFeatureClass(_selectedMCDAWorkspaceContainer.FeatureClass));
            }

            PropertyChanged.Notify(() => Fields);
        }

        private void InitializeClassificationMethods()
        {
            _listOfClassificationMethod = new BindingList<IClassify>();
            _listOfClassificationMethod.Add(new NaturalBreaksClass());
            _listOfClassificationMethod.Add(new QuantileClass());
            _listOfClassificationMethod.Add(new EqualIntervalClass());
            _listOfClassificationMethod.Add(new GeometricalIntervalClass());
        }

        private void ClassBreaksRendererContainerToView()
        {

          _selectedClassificationMethod = _selectedMCDAWorkspaceContainer.ClassBreaksRendererContainer.ClassificationMethod;
          _selectedNumberOfClasses = _selectedMCDAWorkspaceContainer.ClassBreaksRendererContainer.NumberOfClasses;
          _selectedIField = _selectedMCDAWorkspaceContainer.ClassBreaksRendererContainer.Field;
          _selectedStartColor = _selectedMCDAWorkspaceContainer.ClassBreaksRendererContainer.StartColor;
          _selectedEndColor = _selectedMCDAWorkspaceContainer.ClassBreaksRendererContainer.EndColor;
           
        }

	}
}
