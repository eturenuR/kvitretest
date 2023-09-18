using Kvitretest.Models;

namespace Kvitretest.Services
{
    public interface IDatabaseService
    {
        IEnumerable<OneUser> GetAllUsers();
        OneUser? GetUser(int user_id);
        OneUser? GetUserFromToken(string token);
        int GetUserIdFromToken(string token);

        IEnumerable<OnePost> GetAllPosts();
        IEnumerable<OnePost> GetAllPostsWithUsername();
        OnePost? GetPost(int post_id);

        /// <summary>
        /// Attempt to create a new post.
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="messageStr"></param>
        /// <exception cref="DataException"></exception>
        /// <returns></returns>
        OnePost? CreatePost(int user_id, string messageStr);
        OnePost? UpdatePost(int post_id, int user_id, string messageStr);
        bool DeletePost(int post_id, int user_id);
    }
}
