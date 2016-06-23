using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.SqlClient;

namespace reddit_connect.Database
{
    public class SqlDataConnection : IDataConnection
    {
        private string connectionString;

        #region IDataConnection

        #region Users

        public SqlDataConnection()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["SQLSERVER_CONNECTION_STRING"];
        }

        public Guid? CreateUser(string email, string hash, string salt)
        {
            if (String.IsNullOrEmpty(email) && String.IsNullOrEmpty(hash) && String.IsNullOrEmpty(salt))
                throw new ArgumentNullException("email or hash or salt");

            Guid token = Guid.NewGuid();

            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = new SqlCommand("insert into Users (email, pass, pass_salt, token) values (@email, @hash, @salt, @token)", sqlConnection);

            sqlCommand.Parameters.AddWithValue("@email", email);
            sqlCommand.Parameters.AddWithValue("@hash", hash);
            sqlCommand.Parameters.AddWithValue("@salt", salt);
            sqlCommand.Parameters.AddWithValue("@token", token);

            using (sqlConnection)
            {
                sqlConnection.Open();

                int success = sqlCommand.ExecuteNonQuery();

                if (success > 0)
                    return token;
                else
                    return null;
            }
        }

        public bool UserExists(string email)
        {
            if (String.IsNullOrEmpty(email))
                throw new ArgumentNullException("email");

            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = new SqlCommand("select email from Users where email = @email", sqlConnection);

            sqlCommand.Parameters.AddWithValue("@email", email);

            string found = null;

            using (sqlConnection)
            {
                sqlConnection.Open();

                using (SqlDataReader dataReader = sqlCommand.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                {
                    while (dataReader.Read())
                    {
                        found = dataReader["email"].ToString();
                    }
                }
            }

            return !String.IsNullOrEmpty(found);
        }

        public User FetchUser(string email = null, Guid? accessToken = null)
        {
            if (String.IsNullOrEmpty(email) && !accessToken.HasValue)
                throw new ArgumentNullException("email or accessToken");

            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand;

            if (accessToken.HasValue)
            {
                sqlCommand = new SqlCommand("select id, email, pass, pass_salt, token from Users where token = @token", sqlConnection);
                sqlCommand.Parameters.AddWithValue("@token", accessToken);
            }
            else
            {
                sqlCommand = new SqlCommand("select id, email, pass, pass_salt, token from Users where email = @email", sqlConnection);
                sqlCommand.Parameters.AddWithValue("@email", email);
            }

            using (sqlConnection)
            {
                sqlConnection.Open();

                using (SqlDataReader dataReader = sqlCommand.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                {
                    while (dataReader.Read())
                    {
                        User user = 
                            new User()
                            {
                                ID = Convert.ToInt32(dataReader["id"]),
                                Email = dataReader["email"].ToString(),
                                Hash = dataReader["pass"].ToString(),
                                Salt = dataReader["pass_salt"].ToString(),                                
                            };

                        if (!String.IsNullOrEmpty(dataReader["token"].ToString()))
                            user.Token = new Guid(dataReader["token"].ToString());

                        return user;
                    }
                }
            }

            return null;
        }

        public bool UpdateUser(User user)
        {
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = new SqlCommand("update Users set token = @token)", sqlConnection);

            sqlCommand.Parameters.AddWithValue("@token", user.Token);

            using (sqlConnection)
            {
                sqlConnection.Open();

                int success = sqlCommand.ExecuteNonQuery();

                return success > 0;
            }

        }

        #endregion

        #region Content

        public bool CreateFavorite(int userID, string contentID, string tagName = null)
        {
            if (userID <= 0 && String.IsNullOrEmpty(contentID))
                throw new ArgumentNullException("userID or contentID");

            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = new SqlCommand("insert into UserFavorites (userid, contentid, tagname) values (@userid, @contentid, @tagname)", sqlConnection);

            sqlCommand.Parameters.AddWithValue("@userid", userID);
            sqlCommand.Parameters.AddWithValue("@contentid", contentID);
            sqlCommand.Parameters.AddWithValue("@tagname", tagName);

            using (sqlConnection)
            {
                sqlConnection.Open();

                int success = sqlCommand.ExecuteNonQuery();

                return success > 0;
            }
        }

        public bool FavoriteExists(int userID, string contentID)
        {
            if (userID <= 0 && String.IsNullOrEmpty(contentID))
                throw new ArgumentNullException("userID or contentID");

            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = new SqlCommand("select id from UserFavorites where userid = @userid and contentid = @contentid", sqlConnection);

            sqlCommand.Parameters.AddWithValue("@userid", userID);
            sqlCommand.Parameters.AddWithValue("@contentid", contentID);

            string found = null;

            using (sqlConnection)
            {
                sqlConnection.Open();

                using (SqlDataReader dataReader = sqlCommand.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                {
                    while (dataReader.Read())
                    {
                        found = dataReader["id"].ToString();
                    }
                }
            }

            return !String.IsNullOrEmpty(found);
        }

        public IEnumerable<UserFavorite> GetFavorites(int userID)
        {
            if (userID <= 0)
                throw new ArgumentNullException("userID");

            List<UserFavorite> favorites = new List<UserFavorite>();

            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = new SqlCommand("select id, contentid, tagname from UserFavorites where userid = @userid", sqlConnection);

            sqlCommand.Parameters.AddWithValue("@userID", userID);

            using (sqlConnection)
            {
                sqlConnection.Open();

                using (SqlDataReader dataReader = sqlCommand.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        favorites.Add(
                            new UserFavorite()
                            {
                                ID = Convert.ToInt32(dataReader["id"]),
                                ContentID = dataReader["contentid"].ToString(),
                                TagName = dataReader["tagname"].ToString(),
                            }
                        );
                    }
                }
            }

            return favorites;
        }

        #endregion

        #endregion
    }
}