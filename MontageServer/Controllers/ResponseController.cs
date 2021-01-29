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
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
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

            if (id.ToString().Equals(response.ReqId))
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
        [ResponseType(typeof(Response))]
        public async Task<Response> UploadFile()
        {
            HttpRequest currentRequest = HttpContext.Current.Request;

            // initialize empty response
            Response response = new Response();

            // TODO: make real id counter
            string reqId = (new Random(currentRequest.Url.GetHashCode())).Next(10000, 99999).ToString();
            response.ReqId = reqId;

            HttpPostedFile file = null;
            // TODO: foreach file in request (get HandleAudio and HandleText fxns)
            if (currentRequest.Files.Count > 0)
                file = currentRequest.Files[0];

            // if no file was sent, return empty response
            if (file is null)
                return response;

            // if an audio file was sent, return transcript
            if (file.ContentType.StartsWith("audio/"))
            {
                // TODO: local workaround pending azure resolution
                //return await Task.Run(() => AnalysisController.TranscribeAudio(ref response, file));
                return await Task.Run(() => AnalysisController.TranscribeAudioLocal(ref response, file));
            }
            else
            {
                return await Task.Run(() => AnalysisController.AnalyzeTranscript(ref response, file));
            }
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
            return db.Responses.Count(e => e.ReqId.Equals(id.ToString())) > 0;
        }
    }
}