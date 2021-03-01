using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System
{
    static class ArrayUtil
    {
        private static class EmptyArray<T>
        {
            internal static readonly T[] Value = new T[0];
        }

        public static T[] Empty<T>()
        {
            return EmptyArray<T>.Value;
        }
    }
}
