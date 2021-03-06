﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using reddit_connect.Models;
using reddit_connect.Database;

namespace reddit_connect.Controllers
{
    public class UserController : ApiController
    {
        private EncryptionProvider encryptionProvider = null;
        private IDataConnection dataConnection = null;

        public UserController()
        {
            encryptionProvider = new EncryptionProvider();
            dataConnection = new SqlDataConnection();
        }

        // POST api/user/register
        [HttpPost, ActionName("register")]
        public HttpResponseMessage Register([FromBody]UserModel userModel)
        {
            if (userModel == null || String.IsNullOrEmpty(userModel.Email) || String.IsNullOrEmpty(userModel.Password))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Email and password are required fields.");

            if (dataConnection.UserExists(userModel.Email))
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "A user already exists with that email address.  Please log in.");

            string saltString, hashString;
            encryptionProvider.GetHashAndSaltString(userModel.Password, out hashString, out saltString);

            try
            {
                Guid? accessToken = 
                    dataConnection.CreateUser(userModel.Email, hashString, saltString);

                if (accessToken == null)
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An error occurred while attempting to create the user.");

                return Request.CreateResponse(HttpStatusCode.Created, String.Format("User access token: {0}", accessToken));
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // POST api/user/login
        [HttpPost, ActionName("login")]
        public HttpResponseMessage Login([FromBody]UserModel userModel)
        {
            if (userModel == null || String.IsNullOrEmpty(userModel.Email) || String.IsNullOrEmpty(userModel.Password))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Email and password are required fields.");

            if (!dataConnection.UserExists(userModel.Email))
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No user exists with that email address.  Please register.");

            var user = dataConnection.FetchUser(userModel.Email);

            if (user == null)
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An error occurred while attempting to locate the user.");

            bool loginSuccess = 
                encryptionProvider.VerifyHashString(userModel.Password, user.Hash, user.Salt);

            if (!loginSuccess)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid username/password combination.  Please try again.");

            user.Token = Guid.NewGuid();

            bool updateSuccess = 
                dataConnection.UpdateUser(user);

            if (!updateSuccess)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error generating new access token.  Please try again.");

            return Request.CreateResponse(HttpStatusCode.Found, String.Format("User access token: {0}", user.Token));
        }

        // GET api/user/favorites
        [HttpGet, ActionName("favorites")]
        public IEnumerable<ContentModel> Favorites(Guid? accessToken = null)
        {
            if (accessToken == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access token is required."));

            var user =
                dataConnection.FetchUser(accessToken: accessToken);

            if (user == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "No user could be found with that access token.  Plesae try again."));

            List<ContentModel> favoritesList = new List<ContentModel>();

            var favorites =
                dataConnection.GetFavorites(user.ID).ToList();

            if (favorites != null && favorites.Count > 0)
            {
                foreach (var favorite in favorites)
                {
                    favoritesList.Add(
                        new ContentModel()
                        {
                            ID = favorite.ContentID,
                            PermaLink = favorite.PermaLink,
                            URL = favorite.URL,
                            Author = favorite.Author,
                            TagName = favorite.TagName,
                        }
                    );
                }
            }
            return favoritesList;
        }

    }
}
