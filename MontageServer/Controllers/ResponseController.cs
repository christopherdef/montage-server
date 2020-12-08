using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using MontageServer.Data;
using MontageServerAPI;

namespace MontageServer.Controllers
{
    public class ResponseController : ApiController
    {
        private MontageServerContext db = new MontageServerContext();

        // GET: api/Response
        public IQueryable<Response> GetResponses()
        {
            return db.Responses;
        }

        // GET: api/Response/5
        [ResponseType(typeof(Response))]
        public async Task<IHttpActionResult> GetResponse(int id)
        {
            Response response = await db.Responses.FindAsync(id);
            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        // PUT: api/Response/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutResponse(int id, Response response)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != response.ReqId)
            {
                return BadRequest();
            }

            db.Entry(response).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResponseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// On receipt of POST request, upload the provided file to disk
        /// then, trigger file analysis script and await response.
        /// 
        /// Return results of analysis in json form
        /// 
        /// TODO: streamline in some way to avoid saving to disk
        /// TODO: trigger actual analysis script
        /// TODO: properly format output from script
        /// TODO: configure async response
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public string UploadFile()
        {
            HttpRequest currentRequest = HttpContext.Current.Request;

            // grab pending file
            HttpPostedFile file = null;
            if (currentRequest.Files.Count > 0)
                file = currentRequest.Files[0];


            // if a file was pending, and its not empty:
            if (file != null && file.ContentLength > 0)
            {

                // find path to store file
                string fileName = Path.GetFileName(file.FileName);
                string path = Path.Combine(
                    HttpContext.Current.Server.MapPath("~/uploads"),
                    fileName
                );

                // store file
                if (!File.Exists(path))
                    File.Create(path).Close();
                file.SaveAs(path);
            }

            // return the file name
            // TODO: return more than just the file name
            return file != null ? "/uploads/" + file.FileName : null;
        }


        /*
         * Prewritten POST method
         * Collides with current upload method atm
         */
        //// POST: api/Response
        //[ResponseType(typeof(Response))]
        //public async Task<IHttpActionResult> PostResponse(Response response)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.Responses.Add(response);
        //    await db.SaveChangesAsync();

        //    return CreatedAtRoute("DefaultApi", new { id = response.ReqId }, response);
        //}

        // DELETE: api/Response/5
        [ResponseType(typeof(Response))]
        public async Task<IHttpActionResult> DeleteResponse(int id)
        {
            Response response = await db.Responses.FindAsync(id);
            if (response == null)
            {
                return NotFound();
            }

            db.Responses.Remove(response);
            await db.SaveChangesAsync();

            return Ok(response);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ResponseExists(int id)
        {
            return db.Responses.Count(e => e.ReqId == id) > 0;
        }
    }
}