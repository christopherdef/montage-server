using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MontageServer.Data;
using MontageServer.Models;

namespace MontageServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AudioResponseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AudioResponseController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// On receipt of POST request, upload the provided file to disk
        /// then, trigger file analysis script and await response.
        /// 
        /// Return results of analysis in json form
        /// 
        /// TODO: streamline in some way to avoid saving to disk
        /// TODO: properly format output from script
        /// </summary>
        /// <returns></returns>
        // POST: api/AudioResponses
        [HttpPost]
        public async Task<IActionResult> PostFormData([FromForm] IFormFile file)
        {
            // if no file was sent, return empty response
            if (file is null)
                return Ok();
            HttpRequest currentRequest = HttpContext.Request;

            using (var sr = new StreamReader(file.OpenReadStream()))
            {

                // initialize empty response
                AudioResponse response = new AudioResponse();

                // TODO: make real id counter
                string reqId = (new Random(currentRequest.Path.GetHashCode())).Next(10000, 99999).ToString();
                response.ReqId = reqId;

                // process file in bg
               await Task.Run(() =>
               {
                   // if an audio file was sent, return transcript
                   if (file.ContentType.StartsWith("audio/"))
                       AnalysisController.TranscribeAudio(ref response, file);
                   else
                       AnalysisController.AnalyzeTranscript(ref response, file);
               });

                // TODO: output validation, error codes, etc.
               return Ok(response);
            }
        }
       
        // GET: api/AudioResponses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AudioResponse>>> GetAudioResponse()
        {
            return await _context.AudioResponse.ToListAsync();
        }

        // GET: api/AudioResponses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AudioResponse>> GetAudioResponse(string id)
        {
            var audioResponse = await _context.AudioResponse.FindAsync(id);

            if (audioResponse == null)
            {
                return NotFound();
            }

            return audioResponse;
        }
    }
}
