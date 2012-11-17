using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;

namespace MCDA.Model
{
    public class MCDAWorkspaceContainer
    {
        private IFeatureClass _fc;
        private IFeatureLayer _fl;
        private AbstractToolTemplate _tool;

        public MCDAWorkspaceContainer(AbstractToolTemplate tool, IFeatureClass fc)
        {
            _tool = tool;
            _fc = fc;
        }

        public AbstractToolTemplate Tool
        {
            get { return _tool; }
            set { _tool = value; }
        }

        public IFeatureClass FeatureClass
        {

            get { return _fc; }
            set { _fc = value; }
        }

        public IFeatureLayer FeatureLayer
        {
            get { return _fl; }
            set { _fl = value; }
        }

        public MCDA.Model.RendererFactory.ClassBreaksRendererContainer ClassBreaksRendererContainer { get; set; }
    }
}
