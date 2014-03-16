using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ESRI.ArcGIS.Carto;
using MCDA.Extensions;
using ESRI.ArcGIS.Geodatabase;

namespace MCDA.Model
{
    internal sealed class RenderContainer : IRenderContainer, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //private IFeatureLayer featureLayer;
        //private IFeatureClass featureClass;
        private Renderer renderer = Renderer.None;
        private ClassBreaksRendererContainer classBreaksRendererContainer;
        private BiPolarRendererContainer biPolarRendererContainer;

        public Renderer Renderer
        {
            get { return renderer; }
            set { PropertyChanged.ChangeAndNotify(ref renderer, value, () => Renderer); }
        }

        //public IFeatureLayer FeatureLayer
        //{
        //    get { return featureLayer; }
        //    set { PropertyChanged.ChangeAndNotify(ref featureLayer, value, () => FeatureLayer); }
        //}

        //public IFeatureClass FeatureClass
        //{
        //    get { return featureClass; }
        //    set { PropertyChanged.ChangeAndNotify(ref featureClass, value, () => FeatureClass); }
        //}

        public ClassBreaksRendererContainer ClassBreaksRendererContainer
        {

            get { return classBreaksRendererContainer; }
            set { PropertyChanged.ChangeAndNotify(ref classBreaksRendererContainer, value, () => ClassBreaksRendererContainer); }
        }

        public BiPolarRendererContainer BiPolarRendererContainer
        {
            get { return biPolarRendererContainer; }
            set { PropertyChanged.ChangeAndNotify(ref biPolarRendererContainer, value, () => BiPolarRendererContainer); }
        }

        //public string FieldName { get { return Field.Name; } }

        //public string FeatureLayerName { get { return featureLayer.Name; } }
    }
}
