using System.Web.Mvc;
using ShareDataSite.Filters;

namespace ShareDataSite.Controllers
{
    /// <summary>
    /// Home Controller.
    /// </summary>
    [AuthorizedViewData]
    public class HomeController : Controller
    {
        /// <summary>
        /// Filelist page.
        /// </summary>
        /// <returns>ViewResult object.</returns>
        [Route("")]
        public ActionResult Index()
        {
            return View("filelist");
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

        /// <summary>
        /// Pagenotfound page.
        /// </summary>
        /// <returns>ViewResult object.</returns>
        public ActionResult PageNotFound()
        {
            return View("pagenotfound");
        }
    }
}