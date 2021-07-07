﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace D9Framework
{
    /// <summary>
    /// Utility Log. Convenient logging methods which automatically prefix themselves for identifiability, plus debug-only messages.
    /// </summary>
    class ULog
    {
        public static bool DEBUG => D9FModSettings.DEBUG;
        public static string modid = "D9 Framework";
        public static string prefix => "[" + modid + "] ";

        public static void Message(String s)
        {
            Log.Message(prefix + s);
        }

        public static void Warning(String s)
        {
            Log.Warning(prefix + s);
        }

        public static void Error(String s)
        {
            Log.Error(prefix + s);
        }

        public static void DebugMessage(String s, bool addPrefix = true)
        {
            if (DEBUG) Log.Message((addPrefix ? prefix : "") + s);
        }
    }
}
