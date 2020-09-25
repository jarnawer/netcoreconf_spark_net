using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HelloSpark.Utils;
using Microsoft.Spark;
using Microsoft.Spark.Sql;
using Microsoft.Spark.Sql.Types;
using static Microsoft.Spark.Sql.Functions;

namespace HelloSpark
{
    class HelloSparkJob
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

            var spark = SparkSession
                .Builder()
                .AppName("Batch Job example using Apache Spark .Net")
                .GetOrCreate();

            if (sparkConf != null)
            {
                sparkConf.ToList().ForEach(kv => { spark.Conf().Set(kv.Key, kv.Value); });
            }

            var df = spark
                .Read()
                .Schema(GetSchema())
                .Option("header", true)
                .Csv(args[0]);

            var processedDF = ProcessDataset(df);

            ShowDatasetInfo(processedDF);

            WriteData(processedDF, args[1]);

            Console.WriteLine("Finished .Net Spark Job!!");
        }

        private static DataFrame ProcessDataset(DataFrame df)
        {
           return df
                .WithColumn("movie_title", Trim(Col("movie_title")))
                .WithColumn("processed_date", CurrentTimestamp());

        }

        private static void WriteData(DataFrame df, string rootPath)
        {
            var theDate = DateTime.Now.Date;
            var dateToHiveFormat = $"year={theDate:yyyy}/month={theDate:MM}/day={theDate:dd}";

            //Hive format
            var path = $"{rootPath}/movies/{dateToHiveFormat}";

            df.Write().Mode(SaveMode.Overwrite).Parquet(path);

            Console.WriteLine($"Data saved in {path}!!");
        }

        private static void ShowDatasetInfo(DataFrame df)
        {
            //Print first rows and schema
            df.Show();
            df.PrintSchema();

            var numRows = df.Count();
            Console.WriteLine($"Found {numRows} movies in movie dataset");

            var directorsCount = df.Select("director_name").Distinct().Count();
            Console.WriteLine($"Found {directorsCount} diferent Directors in movie dataset");

            var maxActorDF = df.GroupBy("actor_1_name").Count().OrderBy(Desc("count"));

            maxActorDF.Explain();
            
            maxActorDF.Show();

            var actorWithMostAppearances = maxActorDF.First().GetAs<String>(0);
            Console.WriteLine($"Actor with most appearances is {actorWithMostAppearances}");

            var mostVotedMovieDF = df.Select("movie_title", "movie_facebook_likes").OrderBy(Desc("movie_facebook_likes"));
            mostVotedMovieDF.Show();

            var mostVotedMovieRow = mostVotedMovieDF.First();

            Console.WriteLine($"Most rated movie is {mostVotedMovieRow.GetAs<String>("movie_title")} " +
                $"with {mostVotedMovieRow.GetAs<Int32>("movie_facebook_likes")} votes");
        }

        private static StructType GetSchema()
        {
            return new StructType(new[] {
                new StructField("color", new StringType()),
                new StructField("director_name", new StringType()),
                new StructField("num_critic_for_reviews", new IntegerType()),
                new StructField("duration", new IntegerType()),
                new StructField("director_facebook_likes", new IntegerType()),
                new StructField("actor_3_facebook_likes", new IntegerType()),
                new StructField("actor_2_name", new StringType()),
                new StructField("actor_1_facebook_likes", new IntegerType()),
                new StructField("gross", new LongType()),
                new StructField("genres", new StringType()),
                new StructField("actor_1_name", new StringType()),
                new StructField("movie_title", new StringType()),
                new StructField("num_voted_users", new IntegerType()),
                new StructField("cast_total_facebook_likes", new IntegerType()),
                new StructField("actor_3_name", new StringType()),
                new StructField("facenumber_in_poster", new IntegerType()),
                new StructField("plot_keywords", new StringType()),
                new StructField("movie_imdb_link", new StringType()),
                new StructField("num_user_for_reviews", new IntegerType()),
                new StructField("language", new StringType()),
                new StructField("country", new StringType()),
                new StructField("content_rating", new StringType()),
                new StructField("budget", new LongType()),
                new StructField("title_year", new IntegerType()),
                new StructField("actor_2_facebook_likes", new IntegerType()),
                new StructField("imdb_score", new DecimalType()),
                new StructField("aspect_ratio", new DecimalType()),
                new StructField("movie_facebook_likes", new IntegerType())
            });

        }
    }
}

