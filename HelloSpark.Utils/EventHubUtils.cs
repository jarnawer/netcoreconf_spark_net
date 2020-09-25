using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HelloSpark.Utils
{
    public class EventHubPosition
    {
        public string Offset { get; set; }
        public long SeqNo { get; set; }
        public string EnqueuedTime { get; set; }
        public bool IsInclusive { get; set; }
    }

    public class PositionKey
    {
        [JsonPropertyName("ehName")]
        public string EventHubName { get; set; }
        [JsonPropertyName("partitionId")]
        public int PartitionId { get; set; }
    }

    public static class EventHubConnection
    {
        // When you call this method you will pass in here the number of partitions that your EventHub has.
        public static Dictionary<string, string> GetEventHubConnectionSettings(int eventHubPartitionCount)
        {
            // Change this for the name of your EventHub
            string eventHubName = "your-eventhub-name";

            var eventHubStartingPosition = new EventHubPosition
            {
                // Here I am always defaulting to start from 0 so I always re-process everything on the consumer group, but you might want to start at a different offset.
                Offset = "0",
                SeqNo = -1
            };

            var startingPositions = new Dictionary<string, EventHubPosition>();

            for (int i = 0; i < eventHubPartitionCount; i++)
            {
                startingPositions.Add(JsonSerializer.Serialize<PositionKey>(new PositionKey { EventHubName = eventHubName, PartitionId = i }),
                                      eventHubStartingPosition);
            }

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return new Dictionary<string, string>
            {
                // Here should pass in your eventhub connection string. I'm reading it from a env variable, but you can do it differently if you want to.
                {"eventhubs.connectionString", "your-event-hub-connection-string"},
                // Change this for the name of your consumer Group.
                {"eventhubs.consumerGroup", "$Default" },
                {"eventhubs.startingPositions", JsonSerializer.Serialize<Dictionary<string, EventHubPosition>>(startingPositions, options) }
            };
        }
    }
}
