using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2.Resources
{
    internal class DictionaryTemplates
    {
        internal static string NameElemetnByDefault { get => "<not-found>"; }

        private static Dictionary<string, string>? _dictNameMap;
        private static Dictionary<string, string>? _dictNameOperators;

        static DictionaryTemplates()
        {
            FillDictNameMap();
            FillDictNameOperators();
        }

        internal static string GetMap(string key)
            => GetDictValue(_dictNameMap!, key);
        internal static string GetOperators(string key)
               => GetDictValue(_dictNameOperators!, key);

        internal static string GetKeyMap(string value)
            => GetDictKey(_dictNameMap!, value);

        private static string GetDictValue(Dictionary<string, string> dict, string key)
        {
            string name = NameElemetnByDefault;

            if (string.IsNullOrEmpty(key))
                key = "<не найдено>";

            if (dict.ContainsKey(key))
                name = dict[key];

#if DEBUG
            //name += " (" + key + ")";
#endif

            return name;
        }

        private static string GetDictKey(Dictionary<string, string> dict, string value)
        {
            string name = NameElemetnByDefault;

            if (string.IsNullOrEmpty(value))
                name = "<не найдено>";

            if (dict.ContainsValue(value))
                name = dict.First(el => el.Value == value).Key;

#if DEBUG
            //name += " (" + value + ")";
#endif

            return name;
        }

        internal static List<string> GetAllValuesMap()
            => GetAllValuesDict(_dictNameMap!);
        internal static List<string> GetAllValuesOperators()
              => GetAllValuesDict(_dictNameOperators!);

        private static List<string> GetAllValuesDict(Dictionary<string, string> dict)
        {
            List<string> values = new();

            foreach (KeyValuePair<string, string> item in dict)
                values.Add(item.Value);

            return values;
        }

        private static void FillDictNameMap()
            => _dictNameMap = DeserializeJsonString(GetJsonTextResource("NameMaps.json"))
                ?? throw new NullReferenceException("Не удалось прочитать NameMaps.json");
        private static void FillDictNameOperators()
            => _dictNameOperators = DeserializeJsonString(GetJsonTextResource("NameOperators.json"))
                ?? throw new NullReferenceException("Не удалось прочитать NameOperators.json");

        private static Dictionary<string, string>? DeserializeJsonString(string value)
            => JsonConvert.DeserializeObject<Dictionary<string, string>>(value);

        private static string GetJsonTextResource(string nameResource)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            string? jsonTextNameMIssions = null;
            using (Stream? stream = assembly.GetManifestResourceStream($"CaliberTournamentsV2.Resources.{nameResource}"))
            {
                if (stream == null)
                    throw new NullReferenceException($"Не удалось найти ресурс {nameResource}");

                byte[] temp = new byte[stream.Length];
                stream.Read(temp, 0, temp.Length);

                jsonTextNameMIssions = Encoding.UTF8.GetString(temp);
            }

            return jsonTextNameMIssions;
        }
    }
}
