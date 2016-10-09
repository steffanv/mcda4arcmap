namespace MCDA.Model
{
    internal enum RenderOption { AfterSliderDrag, AnimationLike, Realtime}

    internal sealed class ConfigSingleton
    {
        private static ConfigSingleton _instance;

        private RenderOption _selectedRenderOption = RenderOption.AfterSliderDrag;

        private ConfigSingleton() { }

        public static ConfigSingleton Instance
        {
            get { return _instance ?? (_instance = new ConfigSingleton()); }
        }

        public RenderOption SelectedRenderoption
        {
            get { return _selectedRenderOption; }
            set { _selectedRenderOption = value; }
        }
    }
}
