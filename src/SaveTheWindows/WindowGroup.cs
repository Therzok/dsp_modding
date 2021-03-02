using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using UnityEngine;

namespace SaveTheWindows
{
    struct WindowGroup
    {
        internal readonly string Source;
        internal readonly RectTransform[] Transforms;
        readonly List<WindowGroup> _registrations;

        internal WindowGroup(string source, RectTransform[] transforms, List<WindowGroup> allRegistrations)
        {
            Source = source;
            Transforms = transforms;
            _registrations = allRegistrations;

            // sorted insert.
            int index = allRegistrations.BinarySearch(this, WindowGroupComparer.Instance);
            if (index < 0)
            {
                index = ~index;
            }

            allRegistrations.Insert(index, this);
        }

        sealed class WindowGroupComparer : IComparer<WindowGroup>
        {
            public static readonly WindowGroupComparer Instance = new WindowGroupComparer();

            public int Compare(WindowGroup x, WindowGroup y)
            {
                return StringComparer.Ordinal.Compare(x.Source, y.Source);
            }
        }

        public void Dispose()
        {
            _registrations.Remove(this);
        }
    }
}
