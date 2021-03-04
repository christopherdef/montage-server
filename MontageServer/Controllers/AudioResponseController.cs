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
        private readonly UsersRolesDbContext _usersRolesContext;
        private readonly MontageDbContext _montageContext;

        public AudioResponseController(MontageDbContext montageContext, UsersRolesDbContext usersRolesContext)
        {
            _montageContext = montageContext;
            _usersRolesContext = usersRolesContext;
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
        public async Task<IActionResult> PostFormData([FromForm] IFormFile file, [FromForm] string projectId, [FromForm] string footagePath)
        {
            // read from DB, get response if it exists
            var cachedProject = _montageContext.AdobeProject.Find(projectId);
            if (cachedProject != null)
            {
                var audioResponse = AudioResponse.DeserializeResponse(cachedProject.AudioResponseString);
                audioResponse.FootagePath = cachedProject.FootagePath;
                return Ok(audioResponse);
            }

            // if no file was sent, return empty response
            if (file is null)
                return Ok(new { message = $"unable to find preprocessed footage with id {projectId}"});

            HttpRequest currentRequest = HttpContext.Request;

            using (var sr = new StreamReader(file.OpenReadStream()))
            {

                // initialize empty response
                AudioResponse response = new AudioResponse();
                response.ProjectId = projectId;
                response.FootagePath = footagePath;

                // process file in bg
                await Task.Run(() =>
               {
                   // if an audio file was sent, return transcript
                   if (file.ContentType.StartsWith("audio/"))
                   {
                       // TODO: file conversion - something like this vv
                       // using (var ffmpegConverter = new FfmpegConverter())
                       //    file = ffmpegConverter.convert(file, "wav");

                       // analyze converted audio
                       // AnalysisController.TranscribeAudio(ref response, file);
                       AnalysisController.AnalyzeAudio(ref response, file);
                   }
                   else
                       AnalysisController.AnalyzeTranscript(ref response, file);
               });

                // write to DB (projId, projPath, response)
                cachedProject = new AdobeProject
                {
                    ProjectId = projectId,
                    FootagePath = footagePath,
                    AudioResponseString = response.Serialize()
                };

                _montageContext.AdobeProject.Add(cachedProject);
                _montageContext.SaveChanges();
                response.FootagePath = footagePath;
                // TODO: output validation, error codes, etc.
                return Ok(response);
            }
        }
       
        // GET: api/AudioResponses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AudioResponse>>> GetAudioResponse()
        {
            return await _usersRolesContext.AudioResponse.ToListAsync();
        }

        // GET: api/AudioResponses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AudioResponse>> GetAudioResponse(string id)
        {
            var audioResponse = await _usersRolesContext.AudioResponse.FindAsync(id);

            if (audioResponse == null)
            {
                return NotFound();
            }

            return audioResponse;
        }
    }
}
