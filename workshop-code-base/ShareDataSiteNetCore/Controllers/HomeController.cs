using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShareDataService;

namespace ShareDataSiteNetCore.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Filelist page.
        /// </summary>
        /// <returns>ViewResult object.</returns>
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Rawlist page.
        /// </summary>
        /// <returns>ViewResult object.</returns>
        [Route("rawlist")]
        public ActionResult RawList()
        {
            return View("rawlist");
        }

        /// <summary>
        /// Rawdata page.
        /// </summary>
        /// <returns>ViewResult object.</returns>
        [Route("rawdata")]
        public ActionResult RawData()
        {
            return View("rawdata");
        }
    }
}