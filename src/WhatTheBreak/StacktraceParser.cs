using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace WhatTheBreak
{
    sealed class StacktraceParser
    {
        const string DMDHeader = "(wrapper dynamic-method) ";

        readonly char[] _whitespace = { '\t', ' ' };

        public bool ParseStackTraceLines(string source, Action<string, string> validate)
        {
            bool update = false;

            foreach (string original in source.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int headerOffset = original.IndexOf(DMDHeader, StringComparison.Ordinal);
                if (headerOffset == -1)
                {
                    // Not interesting.
                    continue;
                }

                RealFakeSpan span = new RealFakeSpan(original).Slice(headerOffset + DMDHeader.Length);

                // type and method
                int typeAndMethodEnd = span.IndexOfAny(_whitespace);
                if (typeAndMethodEnd == -1)
                {
                    continue;
                }

                span = span.Slice(0, typeAndMethodEnd);

                //string typeAndMethod = line.Substring(start, typeAndMethodEnd - start);

                const string marker = ".DMD<";
                int typeEnd = span.IndexOf(marker);
                if (typeEnd == -1)
                {
                    continue;
                }

                // appears in DMD section
                //RealFakeSpan stackType = span.Slice(0, typeEnd);

                // Cut off .DMD<...>
                span = span.Slice(typeEnd + marker.Length);
                span = span.Slice(0, span.Length - 1);

                const string separator = "..";
                typeEnd = span.IndexOf(separator);

                RealFakeSpan dmdType = span.Slice(0, typeEnd);
                RealFakeSpan dmdMethod = span.Slice(typeEnd + separator.Length);

                validate(dmdType.AsString(), dmdMethod.AsString());
                update = true;
            }

            return update;
        }

    }
}
