using System.Web.Mvc;

namespace ShareDataSite.Filters
{
    /// <summary>
    /// Authorized View Data Attribute.
    /// </summary>
    public class AuthorizedViewDataAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Authorized url.
        /// </summary>
        public const string AuthUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";

        /// <summary>
        /// Request token url.
        /// </summary>
        public const string TokenUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/token";

        /// <summary>
        /// Logout url,recommended if available.
        /// </summary>
        public const string LogoutUrl = "";

        /// <summary>
        /// Client id,required.
        /// </summary>
        public const string ClientId = "e0375f87-c47c-4180-9e20-ed3cebd53353";

        /// <summary>
        /// A space-separated list of permissions.
        /// </summary>
        public const string Scope = "offline_access openid User.Read Files.Read.All Files.ReadWrite.All Sites.Read.All Sites.ReadWrite.All";

        /// <summary>
        /// Response type.
        /// </summary>
        public const string ResponseType = "token";

        /// <summary>
        /// Redirect uri.
        /// </summary>
        public const string RedirectUri = "https://localhost:44313/Authorization/Authorize";

        /// <summary>
        /// He application secret that you created in the app registration portal for your app. 
        /// </summary>
        public const string ClientSecret = "xvqmxVWR403=(crCZGQ93=!";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.Controller.ViewBag.AuthUrl = AuthUrl;
            filterContext.Controller.ViewBag.TokenUrl = TokenUrl;
            filterContext.Controller.ViewBag.LogoutUrl = LogoutUrl;
            filterContext.Controller.ViewBag.ClientId = ClientId;
            filterContext.Controller.ViewBag.Scope = Scope;
            filterContext.Controller.ViewBag.ResponseType = ResponseType;
            filterContext.Controller.ViewBag.RedirectUri = RedirectUri;
            base.OnActionExecuting(filterContext);
        }
    }
}