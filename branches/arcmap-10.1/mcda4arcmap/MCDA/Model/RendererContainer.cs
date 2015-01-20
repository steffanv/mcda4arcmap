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
    internal sealed class RendererContainer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Renderer renderer = Renderer.None;
        private ClassBreaksRendererContainer classBreaksRendererContainer;
        private BiPolarRendererContainer biPolarRendererContainer;

        public RendererContainer(Field field)
        {

            Field = field;
        }

        public Field Field { get; private set;}

        public Renderer Renderer
        {
            get { return renderer; }
            set { PropertyChanged.ChangeAndNotify(ref renderer, value, () => Renderer); }
        }

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
    }
}
