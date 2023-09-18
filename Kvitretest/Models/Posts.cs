using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kvitretest.Models
{
    public class OnePost
    {
        // Stupidity note to self:
        // Don't make "set" private, or deserialize will never fill the values.

        public int Id { get; set; }
        public string? User_Id { get; set; }
        public string? User_Name { get; set; }
        public string? Body { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize<OnePost>(this, new JsonSerializerOptions()
            {
                WriteIndented = true
            });
        }
    }

    public class AllPostsCollection
    {
        //public long totalElements { get; private set; }

        [JsonPropertyName("posts")]
        public IEnumerable<OnePost>? AllPostsNode { get; set; }

        public override string ToString() => JsonSerializer.Serialize<AllPostsCollection>(this);
    }
}
