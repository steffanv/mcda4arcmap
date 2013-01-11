using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Model;

namespace MCDA.ViewModel
{
    public class ConfigViewModel
    {
        public RenderOption RenderOptions
        {
            get { return ConfigSingleton.Instance.SelectedRenderoption; }
            set {ConfigSingleton.Instance.SelectedRenderoption = value;}
        }
    }
}
