﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Azure.Security
{
    /// <summary>
    /// Class Filter.
    /// </summary>
    internal static class Filter
    {
        /// <summary>
        /// The dictionary
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, string>> Dictionary =
            new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Gets the default.
        /// </summary>
        /// <value>The default.</value>
        public static string Default { get; private set; }


        /// <summary>
        /// Loads this instance.
        /// </summary>
        public static void Load()
        {
            foreach (
                var line in File.ReadAllLines("Settings\\filter.ini", Encoding.Default)
                    .Where(line => !line.StartsWith("#") || !line.StartsWith("//") || line.Contains("=")))
            {
                var array = line.Split('=');
                var mode = array[0];
                var jsonStr = string.Join("=", array.Skip(1));

                var serializer = new JavaScriptSerializer();
                dynamic items = serializer.Deserialize<object[]>(jsonStr);

                var dic = new Dictionary<string, string>();
                foreach (object[] item in items)
                {
                    var key = item[0].ToString();
                    var value = string.Empty;
                    if (item.Count() > 1) value = item[1].ToString();

                    dic.Add(key, value);
                }

                if (dic.ContainsKey(mode)) continue;
                if (Default == null) Default = mode;
                Dictionary.Add(mode, dic);
            }

            Out.WriteLine("Loaded " + Dictionary.Count + " filter modes.", "Azure.Security.Filter");
        }

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        public static void Reload()
        {
            Dictionary.Clear();
            Load();
        }

        /// <summary>
        /// Replaces the specified mode.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="str">The string.</param>
        /// <returns>System.String.</returns>
        public static string Replace(string mode, string str)
        {
            str = str.RemoveDiacritics().ToLower();

            if (!Dictionary.ContainsKey(mode) || string.IsNullOrEmpty(str)) return str;
            return Dictionary[mode].Aggregate(str, (current, array) => current.Replace(array.Key, array.Value));
        }

        private static String RemoveDiacritics(this String s)
        {
            var normalizedString = s.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (
                var c in
                    normalizedString.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                ) stringBuilder.Append(c);

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}