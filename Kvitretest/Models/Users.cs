using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kvitretest.Models
{
    public class OneUser
    {
        // Stupidity note to self:
        // Don't make "set" private, or deserialize will never fill the values.

        public int Id { get; set; }
        public string? Name { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        //public OneUser(int id = -1, string? name = null, string? token = null)
        //{
        //    Id = id;
        //    Name = name;
        //    Token = token;
        //}

        public override string ToString() => JsonSerializer.Serialize<OneUser>(this, new JsonSerializerOptions()
        {
            WriteIndented = true
        });
    }

    public class AllUsersCollection
    {
        //public long totalElements { get; private set; }

        [JsonPropertyName("users")]
        public IEnumerable<OneUser>? AllUsersNode { get; set; }

        public override string ToString() => JsonSerializer.Serialize<AllUsersCollection>(this);
    }
}
