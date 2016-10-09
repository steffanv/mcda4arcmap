using System.ComponentModel;
using MCDA.Extensions;


namespace MCDA.Model
{
    internal sealed class RendererContainer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Renderer _renderer = Renderer.None;
        private ClassBreaksRendererContainer _classBreaksRendererContainer;
        private BiPolarRendererContainer _biPolarRendererContainer;

        public RendererContainer(Field field)
        {
            Field = field;
        }

        public Field Field { get; private set;}

        public Renderer Renderer
        {
            get { return _renderer; }
            set { PropertyChanged.ChangeAndNotify(ref _renderer, value, () => Renderer); }
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
