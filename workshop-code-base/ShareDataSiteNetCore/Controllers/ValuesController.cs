using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace ShareDataSiteNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        AzureADConfig azureAD;
        public ValuesController(IOptions<AzureADConfig> azureAD)
        {
            this.azureAD = azureAD?.Value;
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            string auth_url = azureAD.Auth_Url +
              $"client_id={azureAD.Client_id}&" +
              $"redirect_uri={HttpUtility.UrlEncode(azureAD.Redirect_uri)}&" +
              $"response_type={azureAD.Response_type}&" +
              $"prompt=login";
            return auth_url;
        }
    }
}

