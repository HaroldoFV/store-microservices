using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CartMicroservice.Model;

public class Cart
{
    public static readonly string DocumentName = nameof(Cart);

    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; init; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; }

    public List<CartItem> CartItems { get; init; } = new();
}