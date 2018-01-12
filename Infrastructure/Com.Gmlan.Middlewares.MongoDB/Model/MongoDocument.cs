using MongoDB.Bson;
using Newtonsoft.Json;
using System;

namespace Com.Gmlan.Middlewares.MongoDB.Model
{
    public class MongoDocument
    {
        public MongoDocument()
        {
            CreateDateTimeUtc = DateTime.UtcNow;
            Id = new ObjectId();
        }

        [JsonIgnore]
        public ObjectId Id { get; set; }

        public DateTime CreateDateTimeUtc { get; set; }
    }
}
