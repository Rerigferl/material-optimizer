using System;
using System.Collections;
using System.Collections.Generic;
using Numeira.MaterialOptimizer.API;
using UnityEngine;

namespace Numeira.MaterialOptimizer.Modules
{
    public sealed class LilToonSettings : MaterialOptimizerSettingsBase
    {
        public string? Test;
    }

    [Flags]
    public enum OptimizeOptions
    {
        None = 0,

    }
}
