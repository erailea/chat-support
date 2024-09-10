using MongoDB.Bson.Serialization.Attributes;

namespace ChatSupport.Models
{
    //create custom attribute to specify collection name
    public class CollectionNameAttribute : BsonDiscriminatorAttribute
    {
        public string CollectionName { get; }
        public CollectionNameAttribute(string collectionName) : base()
        {
            CollectionName = collectionName;
        }
    }
}
