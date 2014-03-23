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

        private Color classBreaksRendererStartColor, classBreaksRendererEndColor;

        private BindingList<IClassify> listOfClassificationMethod;
        private BindingList<int> listOfNumberOfClasses;
        private int selectedNumberOfClasses;
        private IClassify selectedClassificationMethod;

        private ClassBreaksRendererContainer classBreaksRendererContainer = new ClassBreaksRendererContainer();

        private double biPolarColorSliderValue;
        private Color biPolarRendererStartColor, biPolarRenderEndColor, biPolarRendererNeutralColor;

        private RendererContainer selectedFieldToRender;

        private ICommand applyClassBreaksRendererCommand;
        private ICommand removeClassBreaksRendererCommand;

        private ICommand applyBiPolarRendererCommand;
        private ICommand removeBiPolarRendererCommand;

        private MCDAExtension MCDAExtension = MCDAExtension.GetExtension();

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

        public BindingList<RendererContainer> ToolFieldsList { get; private set; }

        public BindingList<RendererContainer> AllFieldsList { get; private set; }

        public RendererContainer SelectedFieldToRender
        {
            get { return selectedFieldToRender; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref selectedFieldToRender, value, () => SelectedFieldToRender);

                RendererContainerToView();
                UpdateHistogramControl();

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
            set
            {

                PropertyChanged.ChangeAndNotify(ref selectedNumberOfClasses, value, () => SelectedNumberOfClasses);

                ClassBreaksRendererValuesChanged();
            }
        }

        public IClassify SelectedClassificationMethod
        {
            get { return selectedClassificationMethod; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref selectedClassificationMethod, value, () => SelectedClassificationMethod);

                ClassBreaksRendererValuesChanged();
            }
        }

        public Color ClassBreaksRendererStartColor
        {
            get { return classBreaksRendererStartColor; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref classBreaksRendererStartColor, value, () => ClassBreaksRendererStartColor);

                ClassBreaksRendererValuesChanged();
            }
        }

        public Color ClassBreaksRendererEndColor
        {
            get { return classBreaksRendererEndColor; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref classBreaksRendererEndColor, value, () => ClassBreaksRendererEndColor);

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

            Classification.Histogram(selectedFieldToRender.Field.Feature.FeatureClass, SelectedFieldToRender.Field.ESRIField, out data, out freq);

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
            get { return biPolarColorSliderValue; }
            set
            {

                PropertyChanged.ChangeAndNotify(ref biPolarColorSliderValue, value, () => BiPolarColorSliderValue);
                BiPolarRendererValuesChanged();
            }
        }

        public Color BiPolarRendererStartColor
        {
            get { return biPolarRendererStartColor; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref biPolarRendererStartColor, value, () => BiPolarRendererStartColor);
                BiPolarRendererValuesChanged();
            }
        }

        public Color BiPolarRendererEndColor
        {
            get { return biPolarRenderEndColor; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref biPolarRenderEndColor, value, () => BiPolarRendererEndColor);
                BiPolarRendererValuesChanged();
            }
        }

        public Color BiPolarRendererNeutralColor
        {
            get { return biPolarRendererNeutralColor; }
            set
            {
                PropertyChanged.ChangeAndNotify(ref biPolarRendererNeutralColor, value, () => BiPolarRendererNeutralColor);
                BiPolarRendererValuesChanged();
            }
        }

        #endregion

        private void Render()
        {
            if (IsFieldToRenderSelected)
                ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<RendererContainer, IFeatureLayer2>)MCDAExtension.Render, selectedFieldToRender, selectedFieldToRender.Field.Feature.FeatureLayer);
        }

        private void GetAllFieldsList()
        {
            AllFieldsList = new BindingList<RendererContainer>(MCDAExtension.EligibleFeaturesForMCDA.SelectMany(f => f.Fields).Where(f => f.IsNumeric).Select(f => f.RenderContainer).ToList());
        }

        private void GetToolFieldList()
        {
            ToolFieldsList = new BindingList<RendererContainer>(MCDAExtension.FeaturesFromInMemoryWorkspace.SelectMany(f => f.Fields).Where(f => f.IsNumeric).Select(f => f.RenderContainer).ToList());

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

            listOfNumberOfClasses = new BindingList<int>(Enumerable.Range(2, 19).ToList());

            selectedNumberOfClasses = 5;
        }

        private BiPolarRendererContainer GetBiPolarContainer()
        {
            return new BiPolarRendererContainer()
            {
                Field = selectedFieldToRender.Field.ESRIField,
                NegativColor = biPolarRendererStartColor,
                PositivColor = biPolarRenderEndColor,
                NeutralColor = biPolarRendererNeutralColor,
                NeutralColorPosition = biPolarColorSliderValue
            };
        }

        private ClassBreaksRendererContainer GetClassBreaksRendererContainer()
        {
            return new ClassBreaksRendererContainer()
            {
                Field = selectedFieldToRender.Field.ESRIField,
                ClassificationMethod = selectedClassificationMethod,
                EndColor = classBreaksRendererEndColor,
                StartColor = classBreaksRendererStartColor,
                NumberOfClasses = selectedNumberOfClasses
            };
        }

        private void RendererContainerToView()
        {
            if (IsFieldToRenderSelected)
            {
                if (selectedFieldToRender.Renderer == Renderer.None)
                    return;

                if (selectedFieldToRender.Renderer == Renderer.ClassBreaksRenderer)
                {
                    selectedClassificationMethod = selectedFieldToRender.ClassBreaksRendererContainer.ClassificationMethod;
                    selectedNumberOfClasses = selectedFieldToRender.ClassBreaksRendererContainer.NumberOfClasses;
                    classBreaksRendererStartColor = selectedFieldToRender.ClassBreaksRendererContainer.StartColor;
                    classBreaksRendererEndColor = selectedFieldToRender.ClassBreaksRendererContainer.EndColor;

                    PropertyChanged.Notify(() => SelectedClassificationMethod);
                    PropertyChanged.Notify(() => SelectedNumberOfClasses);
                    PropertyChanged.Notify(() => ClassBreaksRendererStartColor);
                    PropertyChanged.Notify(() => ClassBreaksRendererEndColor);
                }

                if (selectedFieldToRender.Renderer == Renderer.BiPolarRenderer)
                {
                    biPolarRendererStartColor = selectedFieldToRender.BiPolarRendererContainer.NegativColor;
                    biPolarRenderEndColor = selectedFieldToRender.BiPolarRendererContainer.PositivColor;
                    biPolarRendererNeutralColor = selectedFieldToRender.BiPolarRendererContainer.NeutralColor;
                    biPolarColorSliderValue = selectedFieldToRender.BiPolarRendererContainer.NeutralColorPosition;

                    PropertyChanged.Notify(() => BiPolarRendererStartColor);
                    PropertyChanged.Notify(() => BiPolarRendererNeutralColor);
                    PropertyChanged.Notify(() => BiPolarRendererEndColor);
                    PropertyChanged.Notify(() => BiPolarColorSliderValue);
                }
            }
        }

        public bool IsFieldToRenderSelected
        {
            get { return SelectedFieldToRender != null; }
        }

        public void BiPolarRendererValuesChanged()
        {
            if (IsFieldToRenderSelected)
            {
                SelectedFieldToRender.BiPolarRendererContainer = GetBiPolarContainer();
                Render();
            }
        }

        public void ClassBreaksRendererValuesChanged()
        {
            if (IsFieldToRenderSelected)
            {
                UpdateHistogramControl();

                SelectedFieldToRender.ClassBreaksRendererContainer = GetClassBreaksRendererContainer();
                Render();
            }
        }

        #region commands

        public ICommand ApplyClassBreaksRendererCommand
        {
            get
            {
                if (applyClassBreaksRendererCommand == null)
                    applyClassBreaksRendererCommand = new RelayCommand(p => this.DoApplyClassBreaksRendererCommand(), p => CanApplyClassBreaksRendererCommand());

                return applyClassBreaksRendererCommand;
            }
        }

        public ICommand RemoveClassBreaksRendererCommand
        {
            get
            {
                if (removeClassBreaksRendererCommand == null)
                    removeClassBreaksRendererCommand = new RelayCommand(p => this.DoRemoveClassBreaksRendererCommand(), p => CanRemoveClassBreaksRendererCommand());

                return removeClassBreaksRendererCommand;
            }
        }

        public ICommand ApplyBiPolarRendererCommand
        {
            get
            {
                if (applyBiPolarRendererCommand == null)
                    applyBiPolarRendererCommand = new RelayCommand(p => this.DoApplyBiPolarRendererCommand(), p => CanApplyBiPolarRendererCommand());

                return applyBiPolarRendererCommand;
            }
        }

        public ICommand RemoveBiPolarRendererCommand
        {
            get
            {
                if (removeBiPolarRendererCommand == null)
                    removeBiPolarRendererCommand = new RelayCommand(p => this.DoRemoveBiPolarRendererCommand(), p => CanRemoveBiPolarRendererCommand());

                return removeBiPolarRendererCommand;
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
