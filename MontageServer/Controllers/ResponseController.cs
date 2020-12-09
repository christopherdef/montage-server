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
        public Response UploadFile()
        {
            HttpRequest currentRequest = HttpContext.Current.Request;

            // grab pending file
            HttpPostedFile file = null;
            if (currentRequest.Files.Count > 0)
                file = currentRequest.Files[0];


            // TODO/ANDY: add speech2text
            if (file.ContentType == "Content-Type: audio/mpeg")
            {
                // put in speech to text
                // Response r = new Response();
                // r.Transcript = 'whatever text';
                // return r;
            }

            

            // TODO: probably don't need to do this

            // if a file was pending, and its not empty:
            //if (file != null && file.ContentLength > 0)
            //{

            //    // find path to store file
            //    string fileName = Path.GetFileName(file.FileName);
            //    string path = Path.Combine(
            //        HttpContext.Current.Server.MapPath("~/uploads"),
            //        fileName
            //    );

            //    // store file
            //    if (!File.Exists(path))
            //        File.Create(path).Close();
            //    file.SaveAs(path);
            //}

            //var sentiments = new List<double>();
            //for (int i = 0; i < 10; i++)
            //    sentiments.Add(rng.NextDouble());

            var topics = new Dictionary<int, List<string>>();
            var individuals = new List<string>();
            var objects = new List<string>();
            var sentiments = new List<double>();
            Random rng;
            using (var sr = new StreamReader(file.InputStream))
            {
                string content = sr.ReadToEnd();

                // TODO: send this content to the analysis script
                // TODO: await response from analysis script

                /*
                 *  until ^^ that's complete, return some random garbage
                 */  

                content = content.Replace('\r', ' ');
                content = content.Replace('\n', ' ');

                // keep returns consistent for the same string
                int chash = content.GetHashCode();
                rng = new Random(chash);

                var words = content.Split(' ');

                int N = words.Length;
                
                // add space for the topics
                int topic_count = rng.Next(2, 10);
                for (int j = 0; j < topic_count; j++)
                    topics.Add(j, new List<string>());

                // randomly create a response
                for (int i = 0; i < N; i++)
                {
                    string w = words[i];
                    if (w.Length == 0)
                        continue;

                    // "calculate" sentiment
                    double sentiment = (double)(w.Length) / (double)N;
                    sentiments.Add(sentiment);

                    // "assign" to a topic
                    int topic_idx = rng.Next(0, topic_count);
                    topics[topic_idx].Add(w);

                    // randomly assign words as individuals, objects, or neither
                    switch (rng.Next(0, 10))
                    {
                        // individuals
                        case 0:
                            individuals.Add(w);
                            break;

                        // objects
                        case 1:
                            objects.Add(w);
                            break;
                        
                        // neither
                        default:
                            break;
                    }
                }
            }

            // return the file name
            // TODO: return more than just the file name
            //return file != null ? "/uploads/" + file.FileName : null;
            Response response = new Response()
            {
                ReqId = rng.Next(10000, 99999),
                Topics = topics,
                Individuals = individuals,
                Objects = objects,
                Sentiments = sentiments,
                Transcript = ""
            };
            return response;
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