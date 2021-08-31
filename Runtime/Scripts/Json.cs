using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Alteracia.Web
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class JsonPropertiesAttribute : Attribute
    {
        private string _jsonName;

        /// <summary>
        ///   <para></para>
        /// </summary>
        /// <param name="oldName">The name of the field before renaming.</param>
        public JsonPropertiesAttribute(string jsonName) => this._jsonName = jsonName;

        /// <summary>
        ///   <para>The name of the field before the rename.</para>
        /// </summary>
        public string jsonName => this._jsonName;
    }
    
    public class JsonStringArray { public string[] list; }
    public class JsonArray<T> { public T[] list; }
    
    public static class AltJson
    {
        public static string FormatJsonText(this string json, System.Type systemType)
        {
            json = json.FixOneFieldJson(systemType);
            // Two levels TODO more
            foreach (var fieldInfo in systemType.GetFields())
            {
                json = json.FormatJsonText(fieldInfo);
                Type fType = fieldInfo.FieldType.IsArray ? fieldInfo.FieldType.GetElementType() : fieldInfo.FieldType;
                
                if (fType == null) continue;
                foreach (var fn in fType.GetFields())
                {
                    json = json.FormatJsonText(fn);
                    Type ft = fn.FieldType.IsArray ? fn.FieldType.GetElementType() : fn.FieldType;
                    json = ft?.GetFields().Aggregate(json, (current, field) => current.FormatJsonText(field));
                }
            }
            // If json is array wrap to JsonArray
            return json.FormatIfJsonArray().FormatIfArrayInArray();
        }

        public static string FixOneFieldJson(this string json, System.Type systemType)
        {
            if (systemType.GetFields().Length != 1)
                return json;
            if (!json.StartsWith("\"") && !json.EndsWith("\""))
                return json;

            // Get Field
            var fieldName = (systemType.GetFields())[0].Name;
            // Return valid json
            return "{\"" + fieldName + "\":" + json + "}";
        }
        
        public static string FormatJsonText(this string json, System.Reflection.MemberInfo memberInfo)
        {
            Alteracia.Web.JsonPropertiesAttribute[] attrs =
                (Alteracia.Web.JsonPropertiesAttribute[]) memberInfo.GetCustomAttributes
                    (typeof(Alteracia.Web.JsonPropertiesAttribute), false);
            foreach (var attr in attrs)
            {
                json = Regex.Replace(json, $"\"({attr.jsonName}.*?)\"", "\"" + memberInfo.Name + "\"");
                //json = json.Replace("\"" + attr.jsonName, "\"" + memberInfo.Name + "\""); // TODO FIX to Regex!
            }

            return json;
        }

        public static string FormatIfJsonArray(this string json)
        {
            return json.StartsWith("[") ? "{ \"list\": " + json + "}" : json;
        }

        public static string FormatIfArrayInArray(this string json) // TODO regex
        {
            // Line
            json = Regex.Replace(json, @"[\r*\n*]", "");
            // Rid of spaces
            json = Regex.Replace(json, @"\[(\s*?)\[", "[[");
            json = Regex.Replace(json, @"\](\s*?)\]", "]]");
            //Debug.Log("result:  " + json);
            //Regex regex = new Regex(@"\[(\s*\r*\n*)\[(.*?)\](\s*\r*\n*)\]");
            //string newJson = "";
            /*
            foreach (string match in regex.)//regex.Matches(json))
            {
                newJson += match;
                //Debug.Log(match);
            }*/
            //Debug.Log("Trim return " + json);
            json = json.Replace("[[", "{-->");
            json = json.Replace("]]", "<--}");
            string[] parts = json.Split(new string[] {"-->"}, StringSplitOptions.None);
            
            if (parts.Length == 1) return json;
            
            json = "";
            foreach (var part in parts)
            {
                string[] sub = part.Split(new string[] {"<--"}, StringSplitOptions.None);
                if (sub.Length < 2)
                {
                    json += part;
                    continue;
                }
                string newPart = "";
                string[] arrays = sub[0].Split(']');
                foreach (var array in arrays)
                {
                    string newArray = "";
                    string[] clean = array.Split('[');
                    string clear = clean[clean.Length > 1 ? 1 : 0];
                    clear = clear.Replace(',', ':');
                    newArray = clean.Length > 1 ? clean[0] + clear : clear;
                    newPart += newArray;
                }
                json += newPart + sub[1];
            }
            return json;
        }
    }
}
