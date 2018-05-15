using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ShareDataSite.Filters;

namespace ShareDataSite.Controllers
{

    /// <summary>
    /// Authorization Controller.
    /// </summary>
    [AuthorizedViewData]
    public class AuthorizationController : Controller
    {
        /// <summary>
        /// Login page.
        /// </summary>
        /// <returns>ViewResult object.</returns>
        [Route("Authorization/Login")]
        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Authorize page.
        /// </summary>
        /// <returns>ViewResult object.</returns>
        [Route("Authorization/Authorize")]
        public ActionResult Authorize()
        {
            return View();
        }

        /// <summary>
        /// Request token.
        /// </summary>
        /// <param name="code">Authorize code.</param>
        /// <returns>Results with Token.</returns>
        [Route("Authorization/Code")]
        public ActionResult Code(string code)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(AuthorizedViewDataAttribute.TokenUrl));
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            NameValueCollection outgoingQueryString = HttpUtility.ParseQueryString(string.Empty);
            outgoingQueryString.Add("code", code);
            outgoingQueryString.Add("client_id", AuthorizedViewDataAttribute.ClientId);
            outgoingQueryString.Add("client_secret", AuthorizedViewDataAttribute.ClientSecret);
            outgoingQueryString.Add("redirect_uri", AuthorizedViewDataAttribute.RedirectUri);
            outgoingQueryString.Add("grant_type", "authorization_code");
            outgoingQueryString.Add("scope", AuthorizedViewDataAttribute.Scope);
            string postdata = outgoingQueryString.ToString();
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(postdata);
            request.ContentLength = buffer.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(buffer, 0, buffer.Length);
            writer.Close();
            try
            {
                var response = request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                string result = sr.ReadToEnd();
                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                if (ex is WebException webex)
                {
                    StreamReader sr = new StreamReader(webex.Response.GetResponseStream());
                    var a = sr.ReadToEnd();
                }

                throw;
            }
        }

        /// <summary>
        /// Refresh Token.
        /// </summary>
        /// <param name="refresh_token">The refresh_token that you acquired during the token request.</param>
        /// <returns>Results with Token.</returns>
        [Route("Authorization/RefreshToken")]
        public ActionResult RefreshToken(string refresh_token)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(AuthorizedViewDataAttribute.TokenUrl));
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            NameValueCollection outgoingQueryString = HttpUtility.ParseQueryString(string.Empty);
            outgoingQueryString.Add("client_id", AuthorizedViewDataAttribute.ClientId);
            outgoingQueryString.Add("refresh_token", refresh_token);
            outgoingQueryString.Add("scope", AuthorizedViewDataAttribute.Scope);
            outgoingQueryString.Add("redirect_uri", AuthorizedViewDataAttribute.RedirectUri);
            outgoingQueryString.Add("grant_type", "refresh_token");
            outgoingQueryString.Add("client_secret", AuthorizedViewDataAttribute.ClientSecret);
            string postdata = outgoingQueryString.ToString();
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(postdata);
            request.ContentLength = buffer.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(buffer, 0, buffer.Length);
            writer.Close();
            try
            {
                var response = request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                string result = sr.ReadToEnd();
                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                if (ex is WebException webex)
                {
                    StreamReader sr = new StreamReader(webex.Response.GetResponseStream());
                    var a = sr.ReadToEnd();
                }

                throw;
            }
        }
    }
}