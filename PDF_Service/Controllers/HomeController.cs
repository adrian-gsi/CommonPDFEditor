using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PDF_Service.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        // Serving a file through MVC Controller....
        /*
        [HttpGet]
        [ActionName("big.pdf")]
        public ActionResult ActionPDF()
        {
            Stream stream = new System.IO.FileStream(@"C:\GSI\BIG2.pdf", System.IO.FileMode.Open);
            return new FileStreamResult(stream, "application/pdf");
        }
        */
    }
}
