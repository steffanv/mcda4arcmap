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

        private Color selectedStartColor, selectedEndColor; 

        private BindingList<IClassify> listOfClassificationMethod;
        private BindingList<int> listOfNumberOfClasses;
        private int selectedNumberOfClasses;
        private IClassify selectedClassificationMethod;

        private ClassBreaksRendererContainer classBreaksRendererContainer = new ClassBreaksRendererContainer();

        private double biPolarColorSliderValue;
        private Color selectedBiPolarNegativColor, selectedBiPolarPositivColor, selectedBiPolarNeutralColor;

        private BindingList<RendererContainer> _resultList, _completeResultList;
        private RendererContainer selectedRendererContainer;

        private ICommand _applyClassBreaksRendererCommand;
        private ICommand _removeClassBreaksRendererCommand;

        private ICommand _applyBiPolarRendererCommand;
        private ICommand _removeBiPolarRendererCommand;

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
            MCDAExtension.RegisterPropertyHandler(x => x.AvailableFeatures, AvailableLayerChanged);

            CreateResultList();
            CreateCompleteResultList();

            PropertyChanged.Notify(() => ResultList);
            PropertyChanged.Notify(() => CompleteResultList);
        }

        private void AvailableLayerChanged(object sender, PropertyChangedEventArgs e)
        {
            CreateCompleteResultList();

            ////the user selected something, but it is no longer available
            //if(selectedRendererContainer != null && _completeResultList.Contains(selectedRendererContainer))
            //    DisableAllRemoveButtons();

            selectedRendererContainer = _completeResultList.FirstOrDefault(r => r.Equals(selectedRendererContainer));

            PropertyChanged.Notify(() => ResultList);
            PropertyChanged.Notify(() => SelectedResult);
            PropertyChanged.Notify(() => CompleteResultList);
        }

        private void LinkDictionaryPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CreateResultList();


            //if (selectedRendererContainer == null)
            //    DisableAllRemoveButtons();

            selectedRendererContainer = _resultList.FirstOrDefault(r => r.Equals(selectedRendererContainer));

            PropertyChanged.Notify(() => ResultList);
            PropertyChanged.Notify(() => SelectedResult);
            PropertyChanged.Notify(() => CompleteResultList);
        }

        public BindingList<RendererContainer> ResultList
        {
            get { return _resultList; }
        }

        public BindingList<RendererContainer> CompleteResultList
        {
            get { return _completeResultList; }
        }

        public RendererContainer SelectedResult
        {
            get { return selectedRendererContainer; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref selectedRendererContainer, value, () => SelectedResult);

            SelectedResultChanged();

            }
        }

        #region class breaks renderer
        public BindingList<int> NumberOfClasses
        {
            get { return listOfNumberOfClasses; }
        }

        public BindingList<IClassify> ListOfClassificationMethod
        {
            get { return listOfClassificationMethod; }  
        }

        
        public int SelectedNumberOfClasses
        {
            get { return selectedNumberOfClasses; }
            set { 
                
                selectedNumberOfClasses = value;

                ClassBreaksRendererValuesChanged();
                //for the histogram
                PropertyChanged.Notify(() => SelectedNumberOfClasses);


            }
        }

        public IClassify SelectedClassificationMethod
        {
            get { return selectedClassificationMethod; }
            set { 
                
                selectedClassificationMethod = value;

                ClassBreaksRendererValuesChanged();

                //for the histogram
                PropertyChanged.Notify(() => SelectedClassificationMethod);

            }
        }

        public Color SelectedStartColor
        {
            get { return selectedStartColor;  }
            set {

                selectedStartColor = value;
                ClassBreaksRendererValuesChanged();
            }
        }

        public Color SelectedEndColor
        {
            get { return selectedEndColor; }
            set
            {
                selectedEndColor = value;
                ClassBreaksRendererValuesChanged();
            }
        }
        #endregion

        #region bi polar renderer

        public double BiPolarColorSliderValue
        {
            get { return biPolarColorSliderValue; }
            set { 
                
                biPolarColorSliderValue = value;
                //no more update after slider change, this takes to long on normal machines
                //from the code behind page if slider dragged
                //BiPolarRendererValuesChanged();
            
            }
        }

        public Color SelectedBiPolarNegativColor
        {
            get { return selectedBiPolarNegativColor; }
            set
            {
                selectedBiPolarNegativColor = value;
                BiPolarRendererValuesChanged();
            }
        }

        public Color SelectedBiPolarPositivColor
        {
            get { return selectedBiPolarPositivColor; }
            set
            {
                selectedBiPolarPositivColor = value;
                BiPolarRendererValuesChanged();
            }
        }

        public Color SelectedBiPolarNeutralColor
        {
            get { return selectedBiPolarNeutralColor; }
            set
            {
                selectedBiPolarNeutralColor = value;
                BiPolarRendererValuesChanged();
            }
        }

        #endregion

        private void Render()
        {
            if(selectedRendererContainer != null)
                ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<RendererContainer, IFeatureLayer2>)MCDAExtension.Render, selectedRendererContainer, selectedRendererContainer.Field.Feature.FeatureLayer);
        }

        private void CreateCompleteResultList()
        {
            _completeResultList = new BindingList<RendererContainer>(MCDAExtension.EligibleFeaturesForMCDA.SelectMany(f => f.Fields).Select(field => field.RenderContainer).ToList()); 
        }

        private void CreateResultList()
        {
            _resultList = new BindingList<RendererContainer>(MCDAExtension.FeaturesFromInMemoryWorkspace.SelectMany(f => f.Fields).Select(field => field.RenderContainer).ToList());
  
        }

        private void InitializeClassificationArguments()
        {
            QuantileClass quantileClass = new QuantileClass();

            selectedClassificationMethod = quantileClass;

            listOfClassificationMethod = new BindingList<IClassify>();
            listOfClassificationMethod.Add(new NaturalBreaksClass());
            listOfClassificationMethod.Add(quantileClass);
            listOfClassificationMethod.Add(new EqualIntervalClass());
            listOfClassificationMethod.Add(new GeometricalIntervalClass());

            listOfNumberOfClasses =  new BindingList<int>(Enumerable.Range(2, 19).ToList());

            selectedNumberOfClasses = 5;
        }

        private BiPolarRendererContainer GetBiPolarContainer()
        {
            return new BiPolarRendererContainer()
            {
                Field = selectedRendererContainer.Field.ESRIField,
                NegativColor = selectedBiPolarNegativColor,
                PositivColor = selectedBiPolarPositivColor,
                NeutralColor = selectedBiPolarNeutralColor,
                NeutralColorPosition = biPolarColorSliderValue
            };
        }

        private ClassBreaksRendererContainer GetClassBreaksRendererContainer()
        {
            return new ClassBreaksRendererContainer()
            {
                Field = selectedRendererContainer.Field.ESRIField,
                ClassificationMethod = selectedClassificationMethod,
                EndColor = selectedEndColor,
                StartColor = selectedStartColor,
                NumberOfClasses = selectedNumberOfClasses
            };
        }

        private void RendererContainerToView()
        {
            if (selectedRendererContainer != null)
            {
                if (selectedRendererContainer.Renderer == Renderer.None)
                    return;

                if (selectedRendererContainer.Renderer == Renderer.ClassBreaksRenderer)
                {
                    selectedClassificationMethod = selectedRendererContainer.ClassBreaksRendererContainer.ClassificationMethod;
                    selectedNumberOfClasses = selectedRendererContainer.ClassBreaksRendererContainer.NumberOfClasses;
                    selectedStartColor = selectedRendererContainer.ClassBreaksRendererContainer.StartColor;
                    selectedEndColor = selectedRendererContainer.ClassBreaksRendererContainer.EndColor;

                    PropertyChanged.Notify(() => SelectedClassificationMethod);
                    PropertyChanged.Notify(() => SelectedNumberOfClasses);
                    PropertyChanged.Notify(() => SelectedStartColor);
                    PropertyChanged.Notify(() => SelectedEndColor);
                }

                if (selectedRendererContainer.Renderer == Renderer.BiPolarRenderer)
                {
                    selectedBiPolarNegativColor = selectedRendererContainer.BiPolarRendererContainer.NegativColor;
                    selectedBiPolarPositivColor = selectedRendererContainer.BiPolarRendererContainer.PositivColor;
                    selectedBiPolarNeutralColor = selectedRendererContainer.BiPolarRendererContainer.NeutralColor;
                    biPolarColorSliderValue = selectedRendererContainer.BiPolarRendererContainer.NeutralColorPosition;

                    PropertyChanged.Notify(() => SelectedBiPolarNegativColor);
                    PropertyChanged.Notify(() => SelectedBiPolarNeutralColor);
                    PropertyChanged.Notify(() => SelectedBiPolarPositivColor);
                    PropertyChanged.Notify(() => BiPolarColorSliderValue);
                }
            }
        }

        public void BiPolarRendererValuesChanged()
        {
            if (SelectedResult != null)
            {
                SelectedResult.BiPolarRendererContainer = GetBiPolarContainer();
                Render();
            }
        }

        public void ClassBreaksRendererValuesChanged()
        {
            if (SelectedResult != null)
            {
                SelectedResult.ClassBreaksRendererContainer = GetClassBreaksRendererContainer();
                Render();
            }
        }

        private void SelectedResultChanged()
        {
            //_isBiPolarRendererApplyCommandExecuted = false;
            //_isClassBreaksRendererApplyCommandExecuted = false;

            //if (selectedRendererContainer != null)
            //{
            //    switch (selectedRendererContainer.Renderer)
            //    {
            //        case Renderer.None: ApplyClassBreaksRendererCommand.CanExecute(true); ApplyClassBreaksRendererCommand.CanExecute(true);
            //            break;
            //        case Renderer.BiPolarRenderer: RemoveBiPolarRendererCommand.CanExecute(true); /*_isBiPolarRendererApplyCommandExecuted = true;*/ ApplyClassBreaksRendererCommand.CanExecute(true);                                                  
            //            break;
            //        case Renderer.ClassBreaksRenderer: /*_isClassBreaksRendererApplyCommandExecuted = true;*/; RemoveClassBreaksRendererCommand.CanExecute(true); ApplyBiPolarRendererCommand.CanExecute(true);
            //            break;             
            //    }

               
            //}

            RendererContainerToView();
        }

        #region commands

        public ICommand ApplyClassBreaksRendererCommand
        {
            get
            {
                if (_applyClassBreaksRendererCommand == null)
                    _applyClassBreaksRendererCommand = new RelayCommand(p => this.DoApplyClassBreaksRendererCommand(), p => CanApplyClassBreaksRendererCommand());

                return _applyClassBreaksRendererCommand;
            }
        }

        public ICommand RemoveClassBreaksRendererCommand
        {
            get
            {
                if (_removeClassBreaksRendererCommand == null)
                    _removeClassBreaksRendererCommand = new RelayCommand( p => this.DoRemoveClassBreaksRendererCommand(), p => CanRemoveClassBreaksRendererCommand());

                return _removeClassBreaksRendererCommand;
            }
        }

        public ICommand ApplyBiPolarRendererCommand
        {
            get
            {
                if (_applyBiPolarRendererCommand == null)
                    _applyBiPolarRendererCommand = new RelayCommand( p => this.DoApplyBiPolarRendererCommand(), p => CanApplyBiPolarRendererCommand());

                return _applyBiPolarRendererCommand;
            }
        }

        public ICommand RemoveBiPolarRendererCommand
        {
            get
            {
                if (_removeBiPolarRendererCommand == null)
                 _removeBiPolarRendererCommand = new RelayCommand( p => this.DoRemoveBiPolarRendererCommand(), p => CanRemoveBiPolarRendererCommand());
              
                return _removeBiPolarRendererCommand;
            }
        }

        #endregion

        #region implemented commands

        public bool CanApplyClassBreaksRendererCommand()
        {
            if (SelectedResult == null)
                return false;

            return !SelectedResult.Field.IsSelectedFieldForRendering || SelectedResult.Renderer != Renderer.ClassBreaksRenderer;
        }

        public bool CanRemoveClassBreaksRendererCommand()
        {
            if (SelectedResult == null)
                return false;

            return SelectedResult.Field.IsSelectedFieldForRendering && SelectedResult.Renderer == Renderer.ClassBreaksRenderer;
        }

        public bool CanApplyBiPolarRendererCommand()
        {
            if (SelectedResult == null)
                return false;

            return !SelectedResult.Field.IsSelectedFieldForRendering || SelectedResult.Renderer != Renderer.BiPolarRenderer;
        }

        public bool CanRemoveBiPolarRendererCommand()
        {
            if (SelectedResult == null)
                return false;

            return SelectedResult.Field.IsSelectedFieldForRendering && SelectedResult.Renderer != Renderer.BiPolarRenderer;
        }

        private void DoApplyClassBreaksRendererCommand(){

            SelectedResult.Renderer = Renderer.ClassBreaksRenderer;
            SelectedResult.ClassBreaksRendererContainer = GetClassBreaksRendererContainer();

            SelectedResult.Field.PromoteToSelectedFieldForRendering();

            Render();
        }

        private void DoRemoveClassBreaksRendererCommand()
        {
            SelectedResult.Renderer = Renderer.None;

            SelectedResult.Field.DemoteAsSelectedFieldForRendering();

            Render();
        }

        private void DoApplyBiPolarRendererCommand()
        {
            SelectedResult.Renderer = Renderer.BiPolarRenderer;
            SelectedResult.BiPolarRendererContainer = GetBiPolarContainer();

            SelectedResult.Field.PromoteToSelectedFieldForRendering();

            Render();
        }

        private void DoRemoveBiPolarRendererCommand()
        {
            SelectedResult.Renderer = Renderer.None;

            SelectedResult.Field.DemoteAsSelectedFieldForRendering();

            Render();
        }
        #endregion
	}
}
