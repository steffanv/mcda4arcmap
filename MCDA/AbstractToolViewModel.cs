using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Model;

namespace MCDA.ViewModel
{
    abstract class AbstractToolViewModel
    {

        public abstract void UpdateDrag();
        public abstract void UpdateRealtime();
        public abstract void UpdateAnimation();

        public void Update()
        {
            switch (ConfigSingleton.Instance.SelectedRenderoption)
            {

                case RenderOption.AfterSliderDrag:
                    UpdateDrag();
                    break;
                case RenderOption.AnimationLike:
                    UpdateAnimation();
                    break;
                case RenderOption.RealTime:
                    UpdateRealtime();
                    break;
                default:
                    UpdateDrag();
                    break;

            }
   
        }

    }
}
