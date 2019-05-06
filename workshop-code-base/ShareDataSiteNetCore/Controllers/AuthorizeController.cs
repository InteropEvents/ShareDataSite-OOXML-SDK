using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace ShareDataSiteNetCore.Controllers
{
    public class AuthorizeController : Controller
    {
        AzureADConfig azureAD;
        public AuthorizeController(IOptions<AzureADConfig> azureAD)
        {
            this.azureAD = azureAD.Value;
        }

        [HttpGet]
        [Route("Login")]
        public ActionResult Login()
        {
            object auth_url = azureAD.Auth_Url + "?" +
              $"client_id={azureAD.Client_Id}&" +
              $"redirect_uri={HttpUtility.UrlEncode(azureAD.Redirect_Uri)}&" +
              $"response_type={azureAD.Response_Type}&" +
              $"scope={azureAD.Scope}";
              //$"prompt=login";
            return PartialView(auth_url);
        }

        [HttpGet]
        [Route("Authorize")]
        public ActionResult Authorize()
        {
            return PartialView();
        }

        [HttpPost]
        [Route("Authorize")]
        public async Task<ActionResult> Code(string code)
        {
            WebClient client = new WebClient();
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString.Add("code", code);
            queryString.Add("client_id", azureAD.Client_Id);
            queryString.Add("client_secret", azureAD.Client_Secret);
            queryString.Add("redirect_uri", azureAD.Redirect_Uri);
            queryString.Add("grant_type", "authorization_code");
            queryString.Add("scope", azureAD.Scope);
            try
            {
                var result = await client.UploadStringTaskAsync(azureAD.Token_Url, queryString.ToString());
                return Content(result, "application/json");
            }
            catch (Exception)
            {
                throw;
            }
        }

        ///// <summary>
        ///// Refresh Token.
        ///// </summary>
        ///// <param name="refresh_token">The refresh_token that you acquired during the token request.</param>
        ///// <returns>Results with Token.</returns>
        //[Route("Authorization/RefreshToken")]
        //public ActionResult RefreshToken(string refresh_token)
        //{
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(AuthorizedViewDataAttribute.TokenUrl));
        //    request.Method = "POST";
        //    request.ContentType = "application/x-www-form-urlencoded";
        //    NameValueCollection outgoingQueryString = HttpUtility.ParseQueryString(string.Empty);
        //    outgoingQueryString.Add("client_id", AuthorizedViewDataAttribute.ClientId);
        //    outgoingQueryString.Add("refresh_token", refresh_token);
        //    outgoingQueryString.Add("scope", AuthorizedViewDataAttribute.Scope);
        //    outgoingQueryString.Add("redirect_uri", AuthorizedViewDataAttribute.RedirectUri);
        //    outgoingQueryString.Add("grant_type", "refresh_token");
        //    outgoingQueryString.Add("client_secret", AuthorizedViewDataAttribute.ClientSecret);
        //    string postdata = outgoingQueryString.ToString();
        //    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(postdata);
        //    request.ContentLength = buffer.Length;
        //    Stream writer = request.GetRequestStream();
        //    writer.Write(buffer, 0, buffer.Length);
        //    writer.Close();
        //    try
        //    {
        //        var response = request.GetResponse();
        //        StreamReader sr = new StreamReader(response.GetResponseStream());
        //        string result = sr.ReadToEnd();
        //        return Content(result, "application/json");
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is WebException webex)
        //        {
        //            StreamReader sr = new StreamReader(webex.Response.GetResponseStream());
        //            var a = sr.ReadToEnd();
        //        }

        //        throw;
        //    }
        //}
    }
}

