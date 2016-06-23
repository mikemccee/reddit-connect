using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace reddit_connect.Database
{
    /// <summary>
    ///     POCO object representing UserFavorites table
    /// </summary>
    public class UserFavorite
    {
        public int ID;
        public int UserID;
        public string ContentID;
        public string TagName;
    }
}