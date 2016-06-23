using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reddit_connect.Database
{
    public interface IDataConnection
    {
        #region Users

        Guid? CreateUser(string email, string hash, string salt);

        bool UserExists(string email);

        User FetchUser(string email = null, Guid? accessToken = null);

        bool UpdateUser(User user);

        #endregion

        #region Content

        bool CreateFavorite(int userID, string contentID, string tagName = null);

        bool FavoriteExists(int userID, string contentID);

        IEnumerable<UserFavorite> GetFavorites(int userID);

        #endregion
    }
}
