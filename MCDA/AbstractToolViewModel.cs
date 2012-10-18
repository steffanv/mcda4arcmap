using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Model;

namespace MCDA.ViewModel
{
    abstract class AbstractToolViewModel
    {

        protected virtual void BeforeUpdate(){}
        protected abstract void UpdateDrag();
        protected abstract void UpdateRealtime();
        protected abstract void UpdateAnimation();
        protected virtual void AfterUpdate(){}

        public void Update()
        {
            BeforeUpdate();

            switch (ConfigSingleton.Instance.SelectedRenderoption)
            {

                case RenderOption.AfterSliderDrag:
                    UpdateDrag();
                    break;
                case RenderOption.AnimationLike:
                    UpdateAnimation();
                    break;
                case RenderOption.Realtime:
                    UpdateRealtime();
                    break;
                default:
                    UpdateDrag();
                    break;

            }

            AfterUpdate();
   
        }

    }
}
