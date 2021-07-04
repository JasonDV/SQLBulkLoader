using System.IO;
using Newtonsoft.Json;

namespace ivaldez.Sql.SharedTestFramework
{
    public class LocalConfig
    {
        private static object lockObject = new object();
        private static LocalConfig _instance;

        public static LocalConfig Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (_instance == null)
                    {
                        var path = Path.Combine(TestUtilities.AssemblyDirectory, @".localconfig");

                        _instance = TestUtilities.DeserializeFile<LocalConfig>(path);
                    }

                    return _instance;
                }
            }
        }

        public string DatabaseName { get; set; }
        public string SqlExpressConnectionString { get; set; }
        public string SqlExpressMasterConnectionString { get; set; }
        public string PostgreSqlConnectionString { get; set; }
        public string PostgreSqlMasterConnectionString { get; set; }
    }
}
