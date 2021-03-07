using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YouNameIt
{
    [Flags]
    enum TrackedStations
    {
        None = 0x0,
        Interplanetary = 0x1,
        Planetary = 0x2,
        All = Interplanetary | Planetary,
    }

    static class StationComponentExtensions
    {
        public static bool IsTracked(this StationComponent component, TrackedStations trackingOptions)
        {
            if (component.isCollector)
            {
                return false;
            }

            return component.isStellar
                ? (trackingOptions & TrackedStations.Interplanetary) != 0
                : (trackingOptions & TrackedStations.Planetary) != 0;
        }
    }
}
