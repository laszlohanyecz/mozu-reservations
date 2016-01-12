using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MozuProductReservationsReport
{
    public class CSVUtil
    {
        public static string ToCSV<T>(T[] records)
        {
            StringBuilder sb = new StringBuilder();
            string delim = ",";

            Type t = typeof(T);
            PropertyInfo[] props = t.GetProperties();

            // header
            foreach (PropertyInfo prop in props)
            {
                string name = prop.Name;
                name = name.Replace("\"", "\"\"");
                sb.AppendFormat("\"{1}\"{0}", delim, name);
            }
            sb.AppendLine();

            foreach (var record in records)
            {
                foreach (PropertyInfo prop in props)
                {
                    object o = prop.GetValue(record);
                    if (o != null)
                    {
                        string val = o.ToString();
                        // if it's a collection, make it into a json string
                        if (prop.PropertyType.IsArray || prop.PropertyType.FullName.StartsWith("System.Collections."))
                        {
                            val = Newtonsoft.Json.JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.None);
                        }
                        if (val != null)
                        {
                            val = val.Replace("\"", "\"\"");
                        }
                        sb.AppendFormat("\"{1}\"{0}", delim, val);
                    }
                    else
                    {
                        sb.AppendFormat("{0}", delim);
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static void WriteCSV<T>(IEnumerable<T> items, StreamWriter writer)
        {
            Type itemType = typeof(T);
            var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            writer.WriteLine(string.Join(",", props.Select(p => "\"" + p.Name.Replace("\"", "\"\"") + "\"")));

            foreach (var item in items)
            {
                List<string> values = new List<string>();
                foreach (PropertyInfo pf in props)
                {
                    object value = pf.GetValue(item);
                    values.Add(value == null ? string.Empty : value.ToString());
                }

                //replace any single quote with a double one.  csv gets tricky when you might have commas/quotes/etc in text
                writer.WriteLine(string.Join(",", values.Select(p => "\"" + p.Replace("\"", "\"\"") + "\"")));
            }
        }

    }
}
