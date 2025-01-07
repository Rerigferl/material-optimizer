using System;
using System.Collections;
using System.Collections.Generic;
using Numeira.MaterialOptimizer.API;
using UnityEngine;

namespace Numeira.MaterialOptimizer.Modules
{
    [DisallowMultipleComponent]
    public sealed class LilToonSettings : MaterialOptimizerSettingsBase
    {

    }

    [Flags]
    public enum OptimizeOptions
    {
        None = 0,

    }
}
