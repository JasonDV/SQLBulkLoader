using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace ivaldez.Sql.SharedTestFramework
{
    public class TestUtilities
    {
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static T DeserializeFile<T>(string file)
        {
            var json = File.ReadAllText(file);

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
