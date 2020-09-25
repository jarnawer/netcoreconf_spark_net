using Microsoft.Spark.Sql;
using Microsoft.Spark.Sql.Streaming;
using static Microsoft.Spark.Sql.Functions;
using Microsoft.Spark.Sql.Types;
using System.Diagnostics;
using System.IO;
using System.Text;
using HelloSpark.Utils;
using System.Linq;
using System;
using System.Runtime.CompilerServices;

namespace HelloSparkStreaming
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine(
                    "Usage: Remember to include input and output path as arguments");
                Environment.Exit(1);
            }

            var sparkConf = SparkConfUtils.GetSparkConfigurationForFilePath(args);

            SparkSession spark = SparkSession
                .Builder()
                .AppName("Streaming example using Spark.NET")
                .GetOrCreate();

            if (sparkConf != null)
            {
                sparkConf.ToList().ForEach(kv => { spark.Conf().Set(kv.Key, kv.Value); });
            }


            var events = spark
                .ReadStream()
                .Format("eventhubs")
                .Options(EventHubConnection.GetEventHubConnectionSettings(eventHubPartitionCount: 2))
                .Load();

            var processedEvents = events
                .Select(
                    FromJson(Col("body").Cast("string"), "temperature String, humidity String").As("Raw"),
                    Col("properties"),
                    Col("enqueuedTime")
                )
                .WithColumn("Raw.temperature", Col("Raw.temperature").Cast("double"))
                .WithColumn("Raw.humidity", Col("Raw.humidity").Cast("double"))
                .WithColumnRenamed("Raw.temperature","temperature")
                .WithColumnRenamed("Raw.humidity","humidity")
                .WithColumn("temperatureAlert", Col("temperature") >= 40)
                .SelectExpr("temperature", "humidity", "properties", "enqueuedTime", "temperatureAlert");

                processedEvents.PrintSchema();
            

            var streamingQuery = processedEvents
                .WriteStream()
                .OutputMode(OutputMode.Append)
                .Format("console")
                .Option("path", args[0])
                .Option("checkpointLocation", args[1])
                .Start();

            streamingQuery.AwaitTermination();
        }

    }
}