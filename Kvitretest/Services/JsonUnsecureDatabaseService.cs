using System.Text.Json;
using System.Text.Json.Nodes;
using Kvitretest.Models;

namespace Kvitretest.Services
{
    public class JsonUnsecureDatabaseService : IDatabaseService
    {
        private readonly JsonDocumentOptions jsonReadOptions = new JsonDocumentOptions()
        {
            AllowTrailingCommas = true,                  // true if an extra comma at the end of a list of JSON values in an object or array is allowed; otherwise, false. Default is false
            CommentHandling = JsonCommentHandling.Skip,  // Allows comments within the JSON input and ignores them. The Utf8JsonReader behaves as if no comments are present.
            MaxDepth = 8                                 // The maximum depth allowed when parsing JSON data, with the default (that is, 0) indicating a maximum depth of 64.
        };
        private readonly JsonSerializerOptions jsonReadSerializerOptions = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,  // Gets or sets a value that indicates whether a property's name uses a case-insensitive comparison during deserialization. The default value is false.
        };

        public JsonUnsecureDatabaseService(IWebHostEnvironment webHostEnvironment)
        {
            WebHostEnvironment = webHostEnvironment;
        }

        public IWebHostEnvironment WebHostEnvironment { get; }

        private string DatabasePath
        {
            //get { return System.IO.Path.Combine(WebHostEnvironment.WebRootPath, "data", "data_som_kan_sees_av_mange.json"); }
            get { return System.IO.Path.Combine(WebHostEnvironment.ContentRootPath, "data", "data_som_kan_sees_av_mange.json"); }
        }

        public IEnumerable<Models.OneUser> GetAllUsers()
        {
            using (var fileJsonFile = File.OpenText(DatabasePath))
            {
                JsonDocument jsonWholeDocument = JsonDocument.Parse(fileJsonFile.ReadToEnd(), jsonReadOptions);

                //JsonElement jsonUsers = jsonWholeDocument.RootElement.GetProperty("users");
                //bool foundJsonNode = jsonWholeDocument.RootElement.TryGetProperty("users", out jsonUsers);
                ////Console.WriteLine(foundJsonNode);
                ////Console.WriteLine(jsonUsers);
                //var jsonAllUsersAsObjects = JsonSerializer.Deserialize<OneUser[]>(jsonUsers, jsonReadSerializerOptions);
                ////JsonElement jsonUsers = new JsonElement();

                var jsonAllUsersNodeObject = JsonSerializer.Deserialize<AllUsersCollection>(jsonWholeDocument, jsonReadSerializerOptions);

                if (jsonAllUsersNodeObject != null && jsonAllUsersNodeObject.AllUsersNode != null)
                {
                    Console.WriteLine(jsonAllUsersNodeObject.ToString());
                    return jsonAllUsersNodeObject.AllUsersNode;
                }
            }
            return Enumerable.Empty<OneUser>();
        }

        public OneUser? GetUser(int User_id)
        {
            throw new NotImplementedException();
        }

        public OneUser? GetUserFromToken(string token)
        {
            throw new NotImplementedException();
        }

        public int GetUserIdFromToken(string token)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.OnePost> GetAllPosts()
        {
            using (var fileJsonFile = File.OpenText(DatabasePath))
            {
                JsonDocument jsonWholeDocument = JsonDocument.Parse(fileJsonFile.ReadToEnd(), jsonReadOptions);

                var jsonAllPostsNodeObject = JsonSerializer.Deserialize<AllPostsCollection>(jsonWholeDocument, jsonReadSerializerOptions);

                if (jsonAllPostsNodeObject != null && jsonAllPostsNodeObject.AllPostsNode != null)
                {
                    Console.WriteLine(jsonAllPostsNodeObject.ToString());
                    return jsonAllPostsNodeObject.AllPostsNode;
                }
            }
            return Enumerable.Empty<OnePost>();
        }

        public IEnumerable<OnePost> GetAllPostsWithUsername()
        {
            throw new NotImplementedException();
        }

        public OnePost? GetPost(int Post_id)
        {
            throw new NotImplementedException();
        }

        public OnePost? CreatePost(int user_id, string messageStr)
        {
            throw new NotImplementedException();
        }

        public OnePost? UpdatePost(int post_id, int user_id, string messageStr)
        {
            throw new NotImplementedException();
        }

        public bool DeletePost(int post_id, int post_user_id = -1)
        {
            throw new NotImplementedException();
        }
    }
}
