using System;
using System.Collections.Generic;
using System.Linq;

namespace HelloSpark.Utils
{
    public class SparkConfUtils
    {
        public static IDictionary<string, string> GetSparkConfigurationForFilePath(string[] args)
        {
            return args switch
            {
                 string[] arr when arr.Any(s=>s.StartsWith("wasbs"))=>
                     new Dictionary<string, string>() { 
                         { "fs.azure", "org.apache.hadoop.fs.azure.NativeAzureFileSystem" },
                         { "fs.azure.account.key.tempdatasetsa.blob.core.windows.net","your-storage-account-key" } 
                     },
                _ => null
            };
        }
    }
}
