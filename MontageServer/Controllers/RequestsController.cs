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
    public class RequestsController : ControllerBase
    {
        private readonly UsersRolesDbContext _usersRolesContext;
        private readonly MontageDbContext _montageContext;

        public RequestsController(MontageDbContext montageContext, UsersRolesDbContext usersRolesContext)
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
        // POST: api/AnalyzeClip
        [HttpPost]
        public async Task<IActionResult> AnalyzeClip([FromForm] IFormFile file, [FromForm] string projectId, [FromForm] string clipId, [FromForm] string userId, [FromForm] string footagePath)
        {
            // TODO: userId never checked!

            // IF clip == null or clip+project seen, collect from db
            // read from DB, get response if it exists
            var query =
                from p in _montageContext.AdobeProjects
                where (p.ProjectId.Equals(projectId))
                join a in _montageContext.ClipAssignments
                on p.ProjectId equals a.ProjectId

                join c in _montageContext.AdobeClips
                                         .Where(c => c.ClipId.Equals(clipId))
                                         .DefaultIfEmpty()
                on a.ClipId equals c.ClipId

                select c;

            if (clipId == null || query.Any())
            {
                var clip = query.FirstOrDefault();
                var result = (AnalysisResult.DeserializeResponse(clip.AnalysisResultString));

                // footagePath and clipId with update if provided
                result.ClipId = clipId ?? result.ClipId;
                result.FootagePath = footagePath ?? result.FootagePath;

                clip.AnalysisResultString = result.Serialize();

                _montageContext.Update(clip);
                await _montageContext.SaveChangesAsync();

                return Ok(result);
            }

            // ELIF no file was sent, and a clip couldn't be found, return empty response
            else if (file is null)
                return Ok(new { message = $"unable to find preprocessed footage with id {projectId}" });

            // ELIF clip+project unseen, process file and add to db
            else
            {
                HttpRequest currentRequest = HttpContext.Request;

                using (var sr = new StreamReader(file.OpenReadStream()))
                {

                    // initialize empty response
                    AnalysisResult response = new AnalysisResult();

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

                    // write to DB 
                    var clip = new AdobeClip
                    {
                        ClipId = clipId,
                        FootagePath = footagePath,
                        AnalysisResultString = response.Serialize()
                    };

                    var project = (from p in _montageContext.AdobeProjects where p.ProjectId.Equals(projectId) select p).FirstOrDefault();
                    if(project == null)
                    {
                        project = new AdobeProject
                        {
                            ProjectId = projectId,

                            UserId = userId
                        };
                        _montageContext.AdobeProjects.Add(project);
                        _montageContext.SaveChanges();
                    }
                    var assignment = new ClipAssignment
                    {
                        ProjectId = projectId,
                        Id = project.UserId,
                        ClipId = clip.ClipId
                    };

                    _montageContext.AdobeClips.Add(clip);
                    _montageContext.ClipAssignments.Add(assignment);
                    await _montageContext.SaveChangesAsync();


                    response.ClipId = clipId;
                    response.FootagePath = footagePath;
                    return Ok(response);

                }
            
            }
        }

        /// <summary>
        /// Return array of clips which have been assigned to projects matching projectId 
        /// which have users matching userId
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetClips(string userId, string projectId)
        {
            // TODO: not actually using the userId rn!
            var query = await
                (from p in _montageContext.AdobeProjects
                 where p.ProjectId.Equals(projectId)
                 join a in _montageContext.ClipAssignments
                 on p.ProjectId equals a.ProjectId

                 select a.Clip).Select(c=>c.GetAnalysisResult()).ToListAsync();

            return Ok(query);


        }

        /// <summary>
        ///// For backwards compatibility
        ///// </summary>
        //// POST: api/AudioResponse
        //[Route("api/AudioResponse")]
        //[HttpPost]
        //public async Task<IActionResult> PostFormData([FromForm] IFormFile file, [FromForm] string projectId, [FromForm] string clipId, [FromForm] string footagePath)
        //{
        //    return await AnalyzeClip(file, projectId, clipId, _usersRolesContext.Users.FirstOrDefault().Id, footagePath);
        //}

        //// GET: api/AnalysisResults
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<AnalysisResult>>> GetAnalysisResult()
        //{
        //    return await _usersRolesContext.AnalysisResult.ToListAsync();
        //}

        //// GET: api/AnalysisResults/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<AnalysisResult>> GetAnalysisResult(string id)
        //{
        //    var audioResponse = await _usersRolesContext.AnalysisResult.FindAsync(id);

        //    if (audioResponse == null)
        //    {
        //        return NotFound();
        //    }

        //    return audioResponse;
        //}
    }
}
