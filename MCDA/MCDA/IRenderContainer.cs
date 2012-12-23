using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using System.ComponentModel;
using ESRI.ArcGIS.Geodatabase;

namespace MCDA.Model
{
    public interface IRenderContainer
    {
        Renderer Renderer { get; set; }

        IFeatureLayer FeatureLayer { get; set; }

        IFeatureClass FeatureClass { get; set; }

        ClassBreaksRendererContainer ClassBreaksRendererContainer { get; set; }

        BiPolarRendererContainer BiPolarRendererContainer { get; set; }
      
    }
}
