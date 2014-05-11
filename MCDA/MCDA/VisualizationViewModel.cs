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
using System.Collections.ObjectModel;

namespace MCDA.ViewModel
{
    internal sealed class VisualizationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Color _classBreaksRendererStartColor, _classBreaksRendererEndColor;

        private int _selectedNumberOfClasses;
        private IClassify _selectedClassificationMethod;

        private ClassBreaksRendererContainer _classBreaksRendererContainer = new ClassBreaksRendererContainer();

        private double _biPolarColorSliderValue;
        private Color _biPolarRendererStartColor, _biPolarRenderEndColor, _biPolarRendererNeutralColor;

        private RendererContainer _selectedFieldToRender;

        private ICommand _applyClassBreaksRendererCommand;
        private ICommand _removeClassBreaksRendererCommand;

        private ICommand _applyBiPolarRendererCommand;
        private ICommand _removeBiPolarRendererCommand;

        private readonly MCDAExtension _mcdaExtension = MCDAExtension.GetExtension();

        public VisualizationViewModel()
        {
            InitializeClassificationArguments();

            ClassBreaksRendererStartColor = Color.FromRgb(255, 0, 0);
            ClassBreaksRendererEndColor = Color.FromRgb(0, 255, 0);

            BiPolarRendererStartColor = Color.FromRgb(255, 0, 0);
            BiPolarRendererEndColor = Color.FromRgb(0, 255, 0);
            BiPolarRendererNeutralColor = Color.FromRgb(255, 255, 255);

            BiPolarColorSliderValue = 0.5;

            GetToolFieldList();
            GetAllFieldsList();

            PropertyChanged.Notify(() => ToolFieldsList);
            PropertyChanged.Notify(() => AllFieldsList);
        }

        public ObservableCollection<MCDA.Model.Feature> ToolFieldsList { get; private set; }

        public ObservableCollection<MCDA.Model.Feature> AllFieldsList { get; private set; }

        public RendererContainer SelectedFieldToRender
        {
            get { return _selectedFieldToRender; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _selectedFieldToRender, value, () => SelectedFieldToRender);

                RendererContainerToView();
                UpdateHistogramControl();

            }
        }

        #region class breaks renderer

        public BindingList<int> NumberOfClasses { get; private set; }

        public BindingList<IClassify> ListOfClassificationMethod { get; private set; }


        public int SelectedNumberOfClasses
        {
            get { return _selectedNumberOfClasses; }
            set
            {

                PropertyChanged.ChangeAndNotify(ref _selectedNumberOfClasses, value, () => SelectedNumberOfClasses);

                ClassBreaksRendererValuesChanged();
            }
        }

        public IClassify SelectedClassificationMethod
        {
            get { return _selectedClassificationMethod; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _selectedClassificationMethod, value, () => SelectedClassificationMethod);

                ClassBreaksRendererValuesChanged();
            }
        }

        public Color ClassBreaksRendererStartColor
        {
            get { return _classBreaksRendererStartColor; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _classBreaksRendererStartColor, value, () => ClassBreaksRendererStartColor);

                ClassBreaksRendererValuesChanged();
            }
        }

        public Color ClassBreaksRendererEndColor
        {
            get { return _classBreaksRendererEndColor; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _classBreaksRendererEndColor, value, () => ClassBreaksRendererEndColor);

                ClassBreaksRendererValuesChanged();
            }
        }

        public long[] HistogramData { get; private set; }

        public long[] HistogramBreaks { get; private set; }

        private void UpdateHistogramControl()
        {
            if (!IsFieldToRenderSelected)
                return;

            double[] data;
            int[] freq;

            Classification.Histogram(_selectedFieldToRender.Field.Feature.FeatureClass, SelectedFieldToRender.Field.ESRIField, out data, out freq);

            HistogramData = System.Array.ConvertAll<int, long>(Classification.NormalizeHistogramData(data, freq), Convert.ToInt64);

            double[] classes = Classification.Classify(SelectedClassificationMethod, SelectedFieldToRender.Field.Feature.FeatureClass, SelectedFieldToRender.Field.ESRIField, SelectedNumberOfClasses);

            HistogramBreaks = System.Array.ConvertAll<int, long>(Classification.NormalizeBreaks(classes), Convert.ToInt64);

            PropertyChanged.Notify(() => HistogramData);
            PropertyChanged.Notify(() => HistogramBreaks);
        }

        #endregion

        #region bi polar renderer

        public double BiPolarColorSliderValue
        {
            get { return _biPolarColorSliderValue; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _biPolarColorSliderValue, value, () => BiPolarColorSliderValue);
                BiPolarRendererValuesChanged();
            }
        }

        public Color BiPolarRendererStartColor
        {
            get { return _biPolarRendererStartColor; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _biPolarRendererStartColor, value, () => BiPolarRendererStartColor);
                BiPolarRendererValuesChanged();
            }
        }

        public Color BiPolarRendererEndColor
        {
            get { return _biPolarRenderEndColor; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _biPolarRenderEndColor, value, () => BiPolarRendererEndColor);
                BiPolarRendererValuesChanged();
            }
        }

        public Color BiPolarRendererNeutralColor
        {
            get { return _biPolarRendererNeutralColor; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _biPolarRendererNeutralColor, value, () => BiPolarRendererNeutralColor);
                BiPolarRendererValuesChanged();
            }
        }

        #endregion

        private void Render()
        {
            if (IsFieldToRenderSelected)
                ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<RendererContainer, IFeatureLayer2>)_mcdaExtension.Render, _selectedFieldToRender, _selectedFieldToRender.Field.Feature.FeatureLayer);
        }

        private void GetAllFieldsList()
        {
            AllFieldsList = new ObservableCollection<MCDA.Model.Feature>(_mcdaExtension.AvailableFeatures.Where(f => f.IsSuitableForMCDA).Where(f => f.Fields.Any(x => x.IsSuitableForMCDA)).ToList());
        }

        private void GetToolFieldList()
        {
            ToolFieldsList = new ObservableCollection<MCDA.Model.Feature>(_mcdaExtension.FeaturesFromInMemoryWorkspace.Where(f => f.IsSuitableForMCDA).Where(x => x.Fields.Any(f => f.IsSuitableForMCDA)).ToList());
        }

        private void InitializeClassificationArguments()
        {
            var quantileClass = new QuantileClass();

            _selectedClassificationMethod = quantileClass;

            ListOfClassificationMethod = new BindingList<IClassify>
            {
                new NaturalBreaksClass(),
                quantileClass,
                new EqualIntervalClass(),
                new GeometricalIntervalClass()
            };

            NumberOfClasses = new BindingList<int>(Enumerable.Range(2, 19).ToList());

            _selectedNumberOfClasses = 5;
        }

        private BiPolarRendererContainer GetBiPolarContainer()
        {
            return new BiPolarRendererContainer()
            {
                Field = _selectedFieldToRender.Field.ESRIField,
                NegativColor = _biPolarRendererStartColor,
                PositivColor = _biPolarRenderEndColor,
                NeutralColor = _biPolarRendererNeutralColor,
                NeutralColorPosition = _biPolarColorSliderValue
            };
        }

        private ClassBreaksRendererContainer GetClassBreaksRendererContainer()
        {
            return new ClassBreaksRendererContainer()
            {
                Field = _selectedFieldToRender.Field.ESRIField,
                ClassificationMethod = _selectedClassificationMethod,
                EndColor = _classBreaksRendererEndColor,
                StartColor = _classBreaksRendererStartColor,
                NumberOfClasses = _selectedNumberOfClasses
            };
        }

        private void RendererContainerToView()
        {
            if (!IsFieldToRenderSelected) return;

            if (_selectedFieldToRender.Renderer == Renderer.None)
                return;

            if (_selectedFieldToRender.Renderer == Renderer.ClassBreaksRenderer)
            {
                _selectedClassificationMethod = _selectedFieldToRender.ClassBreaksRendererContainer.ClassificationMethod;
                _selectedNumberOfClasses = _selectedFieldToRender.ClassBreaksRendererContainer.NumberOfClasses;
                _classBreaksRendererStartColor = _selectedFieldToRender.ClassBreaksRendererContainer.StartColor;
                _classBreaksRendererEndColor = _selectedFieldToRender.ClassBreaksRendererContainer.EndColor;

                PropertyChanged.Notify(() => SelectedClassificationMethod);
                PropertyChanged.Notify(() => SelectedNumberOfClasses);
                PropertyChanged.Notify(() => ClassBreaksRendererStartColor);
                PropertyChanged.Notify(() => ClassBreaksRendererEndColor);
            }

            if (_selectedFieldToRender.Renderer == Renderer.BiPolarRenderer)
            {
                _biPolarRendererStartColor = _selectedFieldToRender.BiPolarRendererContainer.NegativColor;
                _biPolarRenderEndColor = _selectedFieldToRender.BiPolarRendererContainer.PositivColor;
                _biPolarRendererNeutralColor = _selectedFieldToRender.BiPolarRendererContainer.NeutralColor;
                _biPolarColorSliderValue = _selectedFieldToRender.BiPolarRendererContainer.NeutralColorPosition;

                PropertyChanged.Notify(() => BiPolarRendererStartColor);
                PropertyChanged.Notify(() => BiPolarRendererNeutralColor);
                PropertyChanged.Notify(() => BiPolarRendererEndColor);
                PropertyChanged.Notify(() => BiPolarColorSliderValue);
            }
        }

        public bool IsFieldToRenderSelected
        {
            get { return SelectedFieldToRender != null; }
        }

        public void BiPolarRendererValuesChanged()
        {
            if (!IsFieldToRenderSelected) return;

            SelectedFieldToRender.BiPolarRendererContainer = GetBiPolarContainer();
            Render();
        }

        public void ClassBreaksRendererValuesChanged()
        {
            if (!IsFieldToRenderSelected) return;

            UpdateHistogramControl();

            SelectedFieldToRender.ClassBreaksRendererContainer = GetClassBreaksRendererContainer();
            Render();
        }

        #region commands

        public ICommand ApplyClassBreaksRendererCommand
        {
            get {
                return _applyClassBreaksRendererCommand ?? (_applyClassBreaksRendererCommand = new RelayCommand(p => this.DoApplyClassBreaksRendererCommand(),
                               p => CanApplyClassBreaksRendererCommand()));
            }
        }

        public ICommand RemoveClassBreaksRendererCommand
        {
            get {
                return _removeClassBreaksRendererCommand ?? (_removeClassBreaksRendererCommand = new RelayCommand(p => this.DoRemoveClassBreaksRendererCommand(),
                               p => CanRemoveClassBreaksRendererCommand()));
            }
        }

        public ICommand ApplyBiPolarRendererCommand
        {
            get {
                return _applyBiPolarRendererCommand ?? (_applyBiPolarRendererCommand =  new RelayCommand(p => this.DoApplyBiPolarRendererCommand(),
                               p => CanApplyBiPolarRendererCommand()));
            }
        }

        public ICommand RemoveBiPolarRendererCommand
        {
            get {
                return _removeBiPolarRendererCommand ?? (_removeBiPolarRendererCommand = new RelayCommand(p => this.DoRemoveBiPolarRendererCommand(),
                               p => CanRemoveBiPolarRendererCommand()));
            }
        }

        #endregion

        #region implemented commands

        public bool CanApplyClassBreaksRendererCommand()
        {
            return IsFieldToRenderSelected && (!SelectedFieldToRender.Field.IsSelectedFieldForRendering || SelectedFieldToRender.Renderer != Renderer.ClassBreaksRenderer);
        }

        public bool CanRemoveClassBreaksRendererCommand()
        {
            return IsFieldToRenderSelected && SelectedFieldToRender.Field.IsSelectedFieldForRendering && SelectedFieldToRender.Renderer == Renderer.ClassBreaksRenderer;
        }

        public bool CanApplyBiPolarRendererCommand()
        {
            return IsFieldToRenderSelected && (!SelectedFieldToRender.Field.IsSelectedFieldForRendering || SelectedFieldToRender.Renderer != Renderer.BiPolarRenderer);
        }

        public bool CanRemoveBiPolarRendererCommand()
        {
            return IsFieldToRenderSelected && SelectedFieldToRender.Field.IsSelectedFieldForRendering && SelectedFieldToRender.Renderer == Renderer.BiPolarRenderer;
        }

        private void DoApplyClassBreaksRendererCommand()
        {

            SelectedFieldToRender.Renderer = Renderer.ClassBreaksRenderer;
            SelectedFieldToRender.ClassBreaksRendererContainer = GetClassBreaksRendererContainer();

            SelectedFieldToRender.Field.PromoteToSelectedFieldForRendering();

            Render();
        }

        private void DoRemoveClassBreaksRendererCommand()
        {
            SelectedFieldToRender.Renderer = Renderer.None;

            SelectedFieldToRender.Field.DemoteAsSelectedFieldForRendering();

            Render();
        }

        private void DoApplyBiPolarRendererCommand()
        {
            SelectedFieldToRender.Renderer = Renderer.BiPolarRenderer;
            SelectedFieldToRender.BiPolarRendererContainer = GetBiPolarContainer();

            SelectedFieldToRender.Field.PromoteToSelectedFieldForRendering();

            Render();
        }

        private void DoRemoveBiPolarRendererCommand()
        {
            SelectedFieldToRender.Renderer = Renderer.None;

            SelectedFieldToRender.Field.DemoteAsSelectedFieldForRendering();

            Render();
        }
        #endregion
    }
}
