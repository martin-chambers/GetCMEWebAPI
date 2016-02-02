using System.Web.Mvc;
using System.Configuration;

namespace GetCMEWebAPI.Controllers
{
    public class DefaultController : Controller
    {
        // GET: Default
        /// <summary>
        /// The default controller displays in the web root
        /// </summary>
        public ActionResult Index()
        {
            // The steps for customising the Swagger UI are set out in this blog:
            // http://brazilianldsjag.com/2015/07/24/how-to-add-swagger-ui-to-web-api-2/
            ViewBag.Title = "GetCMEWebAPI";
            string urlString = ConfigurationManager.AppSettings["SwaggerUrl"];
            ViewBag.Url = urlString;
            return View();
        }
    }
}