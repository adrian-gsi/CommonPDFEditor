using PDF_Service.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace PDF_Service.Controllers
{
    public class PDFController : ApiController
    {
        private static readonly MediaTypeHeaderValue _mediaType = MediaTypeHeaderValue.Parse("application/pdf");
        [HttpGet]
        [HttpHead]
        [Route("somefile")]
        public HttpResponseMessage RangePDF()
        {
            string file = @"C:\GSI\sample.pdf";
            
            // The control sends a HEAD Request first to check the length of the file.
            if (Request.Method.Method == "HEAD")
            {
                FileInfo fi = new FileInfo(file);
                HttpResponseMessage headresponse = new HttpResponseMessage(HttpStatusCode.OK);
                headresponse.Headers.Add("Accept-Ranges", "bytes");
                headresponse.Content = new StringContent("File.pdf");
                headresponse.Content.Headers.Add("Content-Length", ""+fi.Length);

                return headresponse;
            }
            
            FileStream sourceStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Check to see if this is a range request (i.e. contains a Range header field)
            // Range requests can also be made conditional using the If-Range header field. This can be 
            // used to generate a request which says: send me the range if the content hasn't changed; 
            // otherwise send me the whole thing.
            if (Request.Headers.Range != null)
             {
                try
                 {
                     HttpResponseMessage partialResponse = Request.CreateResponse(HttpStatusCode.PartialContent);
                     partialResponse.Content = new ByteRangeStreamContent(sourceStream, Request.Headers.Range, _mediaType);
                     return partialResponse;
                 }
                 catch (InvalidByteRangeException invalidByteRangeException)
                 {
                     return Request.CreateErrorResponse(invalidByteRangeException);
                 }
             }
             else
             {
                 // If it is not a range request we just send the whole thing as normal
                 HttpResponseMessage fullResponse = Request.CreateResponse(HttpStatusCode.OK);
                 fullResponse.Content = new StreamContent(sourceStream);
                 fullResponse.Content.Headers.ContentType = _mediaType;
                 return fullResponse;
             }
        }
    }
}
