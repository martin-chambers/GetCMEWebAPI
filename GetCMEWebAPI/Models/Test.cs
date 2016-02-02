using MongoDB.Bson.Serialization.Attributes;

namespace GetCMEWebAPI.Models
{
    public class Test
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public string InputDataId { get; set; }
        public string DateSetId { get; set; }
    }
}