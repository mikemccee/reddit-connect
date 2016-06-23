using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace reddit_connect.Models
{
    public class ContentModel
    {
        public string ID { get; set; }
        public string PermaLink { get; set; }
        public string URL { get; set; }
        public string Author { get; set; }
    }
}