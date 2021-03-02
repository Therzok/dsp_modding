using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using UnityEngine;

namespace SaveTheWindows
{
    sealed class WindowGroup : IDisposable
    {
        internal readonly string Source;
        internal readonly RectTransform[] Transforms;
        readonly List<WindowGroup> _registrations;

        internal WindowGroup(string source, RectTransform[] transforms, List<WindowGroup> allRegistrations)
        {
            Source = source;
            Transforms = transforms;
            _registrations = allRegistrations;

            allRegistrations.Add(this);
        }

        public void Dispose()
        {
            _registrations.Remove(this);
        }
    }
}
