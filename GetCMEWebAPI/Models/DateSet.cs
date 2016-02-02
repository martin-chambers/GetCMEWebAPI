using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace GetCMEWebAPI.Models
{
    // regarding the MongoDB-related attributes ...
    // in theory domain objects such as this should not know about the data persistence
    // model elsewhere in the solution, but this is a POC and my priority is
    // to get it working. Worth considering a level of indirection in a production
    // solution: e.g. an injectable decorator class or some such ...
    public class DateSet
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }        
        public List<string> DateList { get; set; }
    }
    
}