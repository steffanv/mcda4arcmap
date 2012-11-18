using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.ComponentModel;
using MCDA.Extensions;

namespace MCDA.Model
{
    /// <summary>
    /// A container for a tool and its 
    /// </summary>
    public class MCDAWorkspaceContainer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private IFeatureClass _fc;
        private IFeatureLayer _fl;
        private AbstractToolTemplate _tool;
        private ClassBreaksRendererContainer _classBreaksRendererContainer;

        public MCDAWorkspaceContainer(AbstractToolTemplate tool, IFeatureClass fc)
        {
            _tool = tool;
            _fc = fc;
        }

        public AbstractToolTemplate Tool
        {
            get { return _tool; }
        }

        public IFeatureClass FeatureClass
        {
            get { return _fc; }
        }

        public IFeatureLayer FeatureLayer
        {
            get { return _fl; }
            set { PropertyChanged.ChangeAndNotify(ref _fl, value, () => FeatureLayer); }
        }

        public ClassBreaksRendererContainer ClassBreaksRendererContainer { 
            
            get { return _classBreaksRendererContainer; }
            set { PropertyChanged.ChangeAndNotify(ref _classBreaksRendererContainer, value, () => ClassBreaksRendererContainer); } 
        
        }   
    }
}
