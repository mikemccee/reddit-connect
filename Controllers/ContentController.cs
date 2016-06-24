using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Configuration;
using Newtonsoft.Json;

using reddit_connect.Models;
using reddit_connect.Database;

namespace reddit_connect.Controllers
{
    public class ContentController : ApiController
    {
        private IDataConnection dataConnection = null;

        public ContentController()
        {
            dataConnection = new SqlDataConnection();
        }

        // GET api/content/all
        [HttpGet, ActionName("all")]
        public IEnumerable<ContentModel> All(Guid? accessToken = null)
        {
            if (accessToken == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access token is required."));

            var user =
                dataConnection.FetchUser(accessToken: accessToken.Value);

            if (user == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "No user could be found with that access token.  Plesae try again."));

            List<ContentModel> contentList = new List<ContentModel>();

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = 
                httpClient.GetAsync(
                    new Uri(ConfigurationManager.AppSettings["ContentEndPoint"])
                ).Result;

            // check for success
            if (!response.IsSuccessStatusCode)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error connecting to the content service."));

            // generate strongly-typed object from JSON
            dynamic json = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
            if (json != null)
            {
                foreach (var item in json.data.children)
                {
                    if (item != null && item.data != null)
                    {
                        contentList.Add(
                            new ContentModel()
                            {
                                ID = item.data.id,
                                PermaLink = item.data.permalink,
                                URL = item.data.url,
                                Author = item.data.author
                            }
                        );
                    }
                }
            }

            return contentList;
        }

        // GET api/content/favorites
        [HttpGet, ActionName("favorites")]
        public IEnumerable<FavoriteModel> Favorites(Guid? accessToken = null)
        {
            if (accessToken == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access token is required."));

            var user =
                dataConnection.FetchUser(accessToken: accessToken);

            if (user == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "No user could be found with that access token.  Plesae try again."));

            List<FavoriteModel> favoritesList = new List<FavoriteModel>();

            var favorites =
                dataConnection.GetFavorites(user.ID).ToList();

            if (favorites != null && favorites.Count > 0)
            {
                foreach (var favorite in favorites)
                {
                    favoritesList.Add(
                        new FavoriteModel()
                        {
                            ContentID = favorite.ContentID,
                            TagName = favorite.TagName,
                        }
                    );
                }
            }
            return favoritesList;
        }
    }
}
