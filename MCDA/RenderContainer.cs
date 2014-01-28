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

        private IFeatureLayer _fl;
        private IFeatureClass _fc;
        private Renderer _renderer = Renderer.None;
        private ClassBreaksRendererContainer _classBreaksRendererContainer;
        private BiPolarRendererContainer _biPolarRendererContainer;

        public Renderer Renderer
        {
            get { return _renderer; }
            set { PropertyChanged.ChangeAndNotify(ref _renderer, value, () => Renderer); }
        }

        public IFeatureLayer FeatureLayer
        {
            get { return _fl; }
            set { PropertyChanged.ChangeAndNotify(ref _fl, value, () => FeatureLayer); }
        }

        public IFeatureClass FeatureClass
        {
            get { return _fc; }
            set { PropertyChanged.ChangeAndNotify(ref _fc, value, () => FeatureClass); }
        }

        public ClassBreaksRendererContainer ClassBreaksRendererContainer
        {

            get { return _classBreaksRendererContainer; }
            set { PropertyChanged.ChangeAndNotify(ref _classBreaksRendererContainer, value, () => ClassBreaksRendererContainer); }
        }

        public BiPolarRendererContainer BiPolarRendererContainer
        {
            get { return _biPolarRendererContainer; }
            set { PropertyChanged.ChangeAndNotify(ref _biPolarRendererContainer, value, () => BiPolarRendererContainer); }
        }
    }
}
