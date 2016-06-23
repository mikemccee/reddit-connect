using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace reddit_connect.Models
{
    public class FavoriteModel
    {
        public Guid? AccessToken { get; set; }
        public string ContentID { get; set; }
        public string TagName { get; set; }
    }
}