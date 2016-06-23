﻿using System;
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

        // GET api/content
        public IEnumerable<ContentModel> Get(Guid? accessToken)
        {
            if (accessToken == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access token is required."));

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

        // POST api/content/favorite
        public HttpResponseMessage Favorite([FromBody]FavoriteModel favoriteModel)
        {
            if (favoriteModel == null || !favoriteModel.AccessToken.HasValue)
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access Token is required.  Please log in if you need one.");

            var user =
                dataConnection.FetchUser(accessToken: favoriteModel.AccessToken.Value);

            if (user == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No user could be found with that access token.  Plesae try again.");

            if (dataConnection.FavoriteExists(user.ID, favoriteModel.ContentID))
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "You've already favorited that content.");

            bool success = 
                dataConnection.CreateFavorite(user.ID, favoriteModel.ContentID, favoriteModel.TagName);

            return Request.CreateResponse(HttpStatusCode.Created, "Content successfully favorited.");
        }
    }
}