using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;
using Kvitretest.Models;

namespace Kvitretest.Services
{
    public class SqliteDatabaseService : IDatabaseService
    {
        //private string _DatabaseUser;
        //private string _DatabasePassword;
        //private string _DatabasePath;

        //public SqliteDatabaseService()
        //{
        //    WebHostEnvironment = null;
        //    _DatabaseUser = "";
        //    _DatabasePassword = "";
        //    //_DatabasePath = "";
        //}

        public SqliteDatabaseService(IWebHostEnvironment webHostEnvironment)
        {
            WebHostEnvironment = webHostEnvironment;
            //_DatabaseUser = "";
            //_DatabasePassword = "";
            //_DatabasePath = "";
        }

        public IWebHostEnvironment WebHostEnvironment { get; }

        private string DatabasePath
        {
            //get { return System.IO.Path.Combine(WebHostEnvironment.WebRootPath, "data", "data_som_kan_sees_av_mange.json"); }
            get { return System.IO.Path.Combine(WebHostEnvironment.ContentRootPath, "data", "testdatabase.db"); }
        }

        /// <summary>
        /// Fetch zero, one or more users from the actual database and store the information into OneUser objects,
        /// then place any results into a List.
        /// Return a Task with a tuple containing number of results, and the list.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private async Task<(int Count, IEnumerable<OneUser> ResultList)> FetchUsersAsync(string sql)
        {
            using (var connection = new SqliteConnection(string.Format("Data Source={0}", DatabasePath)))
            {
                connection.Open();

                var queryRes = (await connection.QueryAsync(sql))
                    .Select(singleRow => new OneUser
                    {
                        Id = (int)singleRow.user_id,
                        Name = singleRow.user_name,
                        Token = singleRow.user_token,
                    })
                    .ToList();

                MyDebug.WriteLine(queryRes is null ? 0 : queryRes.Count);

                if (queryRes != null && queryRes.Count > 0)
                {
                    return (queryRes.Count, queryRes);
                }
            }
            return (0, Enumerable.Empty<OneUser>());
        }

        public IEnumerable<OneUser> GetAllUsers()
        {
            string sql = @"
                SELECT user_id, user_name, user_token
                FROM users;
            ;";

            // Since it's not awaited, the return value is a Task
            var taskTupleQueryResult = FetchUsersAsync(sql);

            //MyDebug.WriteLine(taskTupleQueryResult.Result.Count);

            return taskTupleQueryResult.Result.ResultList;
        }

        public async Task<IEnumerable<OneUser>> GetAllUsersTaskAsync()
        {
            string sql = @"
                SELECT user_id, user_name, user_token
                FROM users;
            ;";

            //var (count, queryResultList) = await FetchUsersAsync(sql);
            var (_, queryResultList) = await FetchUsersAsync(sql);

            //MyDebug.WriteLine(count);

            return queryResultList;
        }

        public OneUser? GetUser(int User_id)
        {
            OneUser? fetchedUserObj = null;

            using (var connection = new SqliteConnection(string.Format("Data Source={0}", DatabasePath)))
            {
                connection.Open();

                var command = connection.CreateCommand();
                //var id = -1;
                var name = string.Empty;
                var token = string.Empty;

                command.CommandText =
                @"
                    SELECT user_name, user_token
                    FROM users
                    WHERE user_id = $id
                ;";
                command.Parameters.AddWithValue("$id", User_id);

                SqliteDataReader reader;
                try
                {
                    reader = command.ExecuteReader();
                }
                catch (SqliteException ex)
                {
                    MyDebug.WriteLine(ex.Message);
                    throw new DataException(ex.Message, ex);
                }

                while (reader.Read())
                {
                    name = reader.GetString("user_name");
                    token = reader.GetString("user_token");

                    MyDebug.WriteLine($"Found: {name}");
                }
                fetchedUserObj = new OneUser()
                {
                    Id = User_id,
                    Name = name,
                    Token = token
                };
            }

            return fetchedUserObj;
        }

        /// <summary>
        /// Fetches the first user with a matching user_token.
        /// TODO: Changed to handle the case where several users have the same token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public OneUser? GetUserFromToken(string token)
        {
            OneUser? fetchedUserObj = null;

            using (var connection = new SqliteConnection(string.Format("Data Source={0}", DatabasePath)))
            {
                connection.Open();

                var command = connection.CreateCommand();
                var id = -1;
                var name = string.Empty;
                //var token = string.Empty;

                command.CommandText =
                @"
                    SELECT user_id, user_name
                    FROM users
                    WHERE user_token = $token
                    LIMIT 1
                ;";
                command.Parameters.AddWithValue("$token", token);

                int count = 0;
                SqliteDataReader reader;
                try
                {
                    reader = command.ExecuteReader();
                }
                catch (SqliteException ex)
                {
                    MyDebug.WriteLine(ex.Message);
                    throw new DataException(ex.Message, ex);
                }

                while (reader.Read())
                {
                    count++;
                    id = reader.GetInt32("user_id");
                    name = reader.GetString("user_name");

                    MyDebug.WriteLine(string.Format($"{count}:found: {id} | {name}"));
                }
                fetchedUserObj = new OneUser()
                {
                    Id = id,
                    Name = name,
                    Token = token
                };
            }

            return fetchedUserObj;
        }

        /// <summary>
        /// Fetches the user_id of the first user with a matching user_token.
        /// TODO: Changed to handle the case where several users have the same token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public int GetUserIdFromToken(string token)
        {
            int user_id = -1;

            using (var connection = new SqliteConnection(string.Format("Data Source={0}", DatabasePath)))
            {
                connection.Open();

                var command = connection.CreateCommand();
                //var token = string.Empty;

                command.CommandText =
                @"
                    SELECT user_id
                    FROM users
                    WHERE user_token = $token
                    LIMIT 1
                ;";
                command.Parameters.AddWithValue("$token", token);

                int count = 0;
                SqliteDataReader reader;
                try
                {
                    reader = command.ExecuteReader();
                }
                catch (SqliteException ex)
                {
                    MyDebug.WriteLine(ex.Message);
                    throw new DataException(ex.Message, ex);
                }

                while (reader.Read())
                {
                    count++;
                    user_id = reader.GetInt32("user_id");

                    MyDebug.WriteLine($"{count} :: {user_id}");
                }
                if (count > 1)
                {
                    string errorStr = $"Too many users with the same Token: {count} : {token}";
                    MyDebug.WriteLine("ERROR! " + errorStr);
                    //throw new ConstraintException(errorStr);
                }
            }

            return user_id;
        }

        public async Task<(int Count, IEnumerable<OnePost> ResultList)> FetchPostsAsync(string sql)
        {
            using (var connection = new SqliteConnection(String.Format("Data Source={0}", DatabasePath)))
            {
                connection.Open();

                var queryRes = (await connection.QueryAsync(sql))
                    .Select(singleRow => new OnePost
                    {
                        Id = (int)singleRow.post_id,
                        User_Id = singleRow.post_user_id,
                        Body = singleRow.post_body
                    })
                    .ToList();

                MyDebug.WriteLine(queryRes.Count);
                //Console.WriteLine(queryRes.Count);
                //Console.WriteLine(queryRes.ToString());

                if (queryRes != null && queryRes.Count > 0)
                {
                    return (queryRes.Count, queryRes);
                }
            }
            return (0, Enumerable.Empty<OnePost>());
        }

        public async Task<(int Count, IEnumerable<OnePost> ResultList)> FetchPostWithUsernameAsync(string sql, object? sqlParams = null)
        {
            using (var connection = new SqliteConnection(String.Format("Data Source={0}", DatabasePath)))
            {
                connection.Open();
                List<OnePost> queryRes;

                //string sql = @"
                //    SELECT p.post_id, p.post_user_id, p.post_body, u.user_name
                //    FROM posts p, users u
                //    WHERE p.post_user_id = u.user_id
                //";
                //string sql = @"
                //    SELECT p.post_id, p.post_user_id, p.post_body, u.user_name
                //    FROM posts p, users u
                //    WHERE p.post_user_id = u.user_id
                //    ORDER BY p.post_id DESC
                //";
                if (sqlParams != null)
                {
                    queryRes = (await connection.QueryAsync(sql, sqlParams))
                        .Select(singleRow => new OnePost
                        {
                            Id = (int)singleRow.post_id,
                            User_Id = singleRow.post_user_id,
                            User_Name = singleRow.user_name,
                            Body = singleRow.post_body
                        })
                        //.OrderByDescending(singleRow => singleRow.Id)
                        .ToList();
                }
                else
                {
                    queryRes = (await connection.QueryAsync(sql))
                        .Select(singleRow => new OnePost
                        {
                            Id = (int)singleRow.post_id,
                            User_Id = singleRow.post_user_id,
                            User_Name = singleRow.user_name,
                            Body = singleRow.post_body
                        })
                        //.OrderByDescending(singleRow => singleRow.Id)
                        .ToList();
                }

                MyDebug.WriteLine(queryRes.Count);
                //Console.WriteLine(queryRes.Count);
                //Console.WriteLine(queryRes.ToString());

                if (queryRes != null && queryRes.Count > 0)
                {
                    return (queryRes.Count, queryRes);
                }
            }
            return (0, Enumerable.Empty<OnePost>());
        }

        public IEnumerable<OnePost> GetAllPosts()
        {
            //var sqlResultTask = GetAllPostsTaskAsync();
            //return sqlResultTask.Result;

            var sql = @"
                SELECT post_id, post_user_id, post_body
                FROM posts
            ;";
            var taskTupleQueryResult = FetchPostsAsync(sql);

            MyDebug.WriteLine(taskTupleQueryResult.Result.Count);
            //MyDebug.WriteLine(taskTupleQueryResult.Result.PostsList.ToString());

            return taskTupleQueryResult.Result.ResultList;
        }

        public IEnumerable<OnePost> GetAllPostsWithUsername()
        {
            var sql = @"
                SELECT p.post_id, p.post_user_id, p.post_body, u.user_name
                FROM posts p, users u
                WHERE p.post_user_id = u.user_id
                ORDER BY p.post_id DESC
            ;";
            var taskTupleQueryResult = FetchPostWithUsernameAsync(sql);

            MyDebug.WriteLine(taskTupleQueryResult.Result.Count);
            //MyDebug.WriteLine(taskTupleQueryResult.Result.PostsList.ToString());

            return taskTupleQueryResult.Result.ResultList;
        }

        public OnePost? GetPost(int PostId)
        {
            var sql = @"
                SELECT p.post_id, p.post_user_id, p.post_body, u.user_name
                FROM posts p, users u
                WHERE p.post_id = @paramPostId
                  AND p.post_user_id = u.user_id
            ;";
            var taskTupleQueryResult = FetchPostWithUsernameAsync(sql, new { paramPostId = PostId });

            MyDebug.WriteLine(PostId);
            MyDebug.WriteLine(taskTupleQueryResult.Result.Count);
            //MyDebug.WriteLine(taskTupleQueryResult.Result.PostsList.ToString());

            if (taskTupleQueryResult != null && taskTupleQueryResult.Result.Count > 0)
            {
                return taskTupleQueryResult.Result.ResultList.First();
            }
            return null;
        }

        public OnePost? CreatePost(int user_id, string messageStr)
        {
            OnePost? newPost = null;

            using (var connection = new SqliteConnection(String.Format("Data Source={0}", DatabasePath)))
            {
                connection.Open();

                var command = connection.CreateCommand();
                var new_post_id = -1;
                //var new_user_id = string.Empty;
                //var new_messageStr = string.Empty;

                command.CommandText =
                @"
                    INSERT INTO posts (post_user_id, post_body)
                    VALUES ($user_id, $message_str)
                    ;
                    -- //SELECT post_id, post_user_id, post_body
                    SELECT post_id
                    FROM posts
                    WHERE post_id = (SELECT last_insert_rowid())
                ;";
                command.Parameters.AddWithValue("$user_id", user_id);
                command.Parameters.AddWithValue("$message_str", messageStr);

                SqliteDataReader reader;
                try
                {
                    reader = command.ExecuteReader();
                }
                catch (SqliteException ex)
                {
                    MyDebug.WriteLine(ex.Message);
                    throw new DataException(ex.Message, ex);
                }

                while (reader.Read())
                {
                    new_post_id = reader.GetInt32("post_id");
                    //new_user_id = reader.GetString("post_user_id");
                    //new_messageStr = reader.GetString("post_body");

                    MyDebug.WriteLine(string.Format($"New! {new_post_id}"));
                }
                newPost = new OnePost()
                {
                    Id = new_post_id,
                    User_Id = user_id.ToString(),
                    //User_Name = "",
                    Body = messageStr
                };
            }

            return newPost;
        }

        public OnePost? UpdatePost(int post_id, int user_id, string messageStr)
        {
            OnePost? editedPost = null;

            using (var connection = new SqliteConnection(String.Format("Data Source={0}", DatabasePath)))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    UPDATE posts
                    SET (post_body) = ($message_str)
                    WHERE post_id = $post_id AND post_user_id = $user_id
                    RETURNING post_id
                ;";
                //;
                //-- //SELECT post_id, post_user_id, post_body
                //SELECT post_id
                //FROM posts
                //WHERE post_id = $post_id
                command.Parameters.AddWithValue("$message_str", messageStr);
                command.Parameters.AddWithValue("$post_id", post_id);
                command.Parameters.AddWithValue("$user_id", user_id);

                int rowsUpdated = -1;
                try
                {
                    rowsUpdated = command.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    MyDebug.WriteLine(ex.Message);
                    throw new DataException(ex.Message, ex);
                }

                if (rowsUpdated > 0)
                {
                    editedPost = new OnePost()
                    {
                        Id = post_id,
                        User_Id = user_id.ToString(),
                        //User_Name = "",
                        Body = messageStr
                    };
                    MyDebug.WriteLine(string.Format($"Updated! {editedPost.Id}"));
                }
            }

            return editedPost;
        }

        public bool DeletePost(int post_id, int post_user_id = -1)
        {
            int possibly_deleted_post_id = -1;

            using (var connection = new SqliteConnection(String.Format("Data Source={0}", DatabasePath)))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    DELETE FROM posts
                    WHERE post_id = $post_id
                " + (post_user_id > 0 ?
                @"
                    AND post_user_id = $post_user_id
                " : "") +
                @"
                    RETURNING *
                    ;
                ;";
                command.Parameters.AddWithValue("$post_id", post_id);
                command.Parameters.AddWithValue("$post_user_id", post_user_id);

                SqliteDataReader reader;
                try
                {
                    reader = command.ExecuteReader();
                }
                catch (SqliteException ex)
                {
                    MyDebug.WriteLine(ex.Message);
                    throw new DataException(ex.Message, ex);
                }

                while (reader.Read())
                {
                    possibly_deleted_post_id = reader.GetInt32("post_id");

                    MyDebug.WriteLine(string.Format($"Deleted? {possibly_deleted_post_id}"));
                }
            }

            return (possibly_deleted_post_id > 1);
        }
    }
}
