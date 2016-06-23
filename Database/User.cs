using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace reddit_connect.Database
{
    /// <summary>
    ///     POCO object representing User table
    /// </summary>
    public class User
    {
        public int ID;
        public string Email;
        public string Hash;
        public string Salt;
        public Guid? Token;
    }

}