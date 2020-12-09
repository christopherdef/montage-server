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
//using Microsoft.CognitiveServices.Speech;
//using Microsoft.CognitiveServices.Speech.Audio;

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


            // initialize empty response
            Response response;
            int reqId = file.GetHashCode();
            var topics = new Dictionary<int, List<string>>();
            var individuals = new List<string>();
            var objects = new List<string>();
            var sentiments = new List<double>();
            var transcript = "";

            response = new Response()
            {
                ReqId = reqId,
                Topics = topics,
                Individuals = individuals,
                Objects = objects,
                Sentiments = sentiments,
                Transcript = transcript
            };

            //// TODO: might need different condition to catch audio
            //// get audio transcripts if file was audio
            //if (file.ContentType == "Content-Type: audio/mpeg")
            //{
            //    // load our subscription
            //    // TODO: get this hard coded sensitive stuff outta here
            //    var speechConfig = SpeechConfig.FromSubscription("77f35c60bef24e58bce1a0c0b9f4be65", "eastus");

            //    // load bytestream -> audio stream
            //    // load audio config from audio stream
            //    // initialize speech recognizer
            //    using (var br = new BinaryReader(file.InputStream))
            //    using (var audioInputStream = AudioInputStream.CreatePushStream()) 
            //    using (var audioConfig = AudioConfig.FromStreamInput(audioInputStream))
            //    using (var recognizer = new SpeechRecognizer(speechConfig, audioConfig))
            //    {

            //        // read through bytes of audio
            //        byte[] readBytes;
            //        do
            //        {
            //            readBytes = br.ReadBytes(1024);
            //            audioInputStream.Write(readBytes, readBytes.Length);
            //        } while (readBytes.Length > 0);


            //        // call 
            //        var recognitionResult = recognizer.RecognizeOnceAsync();
            //        recognitionResult.Wait();

            //        transcript = recognitionResult.Result.Text;

            //        response.Transcript = transcript;

            //    }

            //    return response;
            //}


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
            response = new Response()
            {
                ReqId = reqId,
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