﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            get
            {
                if (_instance == null)
                {
                    _instance = new ConfigSingleton();
                }

                return _instance;
            }
        }

        public RenderOption SelectedRenderoption
        {
            get { return _selectedRenderOption; }
            set { _selectedRenderOption = value; }
        }
    }
}