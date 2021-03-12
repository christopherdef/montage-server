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
        // POST: api/Requests
        [HttpPost]
        public async Task<IActionResult> AnalyzeClip([FromForm] IFormFile file,
                                                     [FromForm] string projectId,
                                                     [FromForm] string clipId,
                                                     [FromForm] string userId,
                                                     [FromForm] string footagePath)
        {
            // TODO: potentially remove clipId from form parameters, make clipId=file.GetHashCode() and return it in the AnalysisResult

            // TODO: check userId!

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

                // only return currently stored result if the result wasn't errored out
                if (result.Error)
                    result = RepairResult(result);

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
                return Ok(new { message = $"unable to find preprocessed footage with id {projectId}/{clipId} for user {userId}" });

            // ELIF clip+project unseen, process file and add to db
            else
            {
                HttpRequest currentRequest = HttpContext.Request;

                using (var sr = new StreamReader(file.OpenReadStream()))
                {
                    // initialize empty response
                    AnalysisResult response = new AnalysisResult();
                    response.ClipId = clipId;
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

                    // load or make corresponding clip
                    var clip = FindClip(clipId);
                    if (clip == null)
                    {
                        clip = new AdobeClip
                        {
                            ClipId = clipId,
                            FootagePath = footagePath,
                            AnalysisResultString = response.Serialize()
                        };
                        _montageContext.AdobeClips.Add(clip);
                        _montageContext.SaveChanges();
                    }

                    // load or make corresponding project
                    var project = FindProject(projectId);
                    if (project == null)
                    {
                        project = new AdobeProject
                        {
                            ProjectId = projectId,
                            UserId = userId
                        };
                        _montageContext.AdobeProjects.Add(project);
                        _montageContext.SaveChanges();
                    }

                    // load or make corresponding clip assignment
                    var assignment = FindAssignment(projectId, clipId);
                    if (assignment == null)
                    {
                        assignment = new ClipAssignment
                        {
                            ProjectId = projectId,
                            UserId = project.UserId,
                            ClipId = clip.ClipId
                        };
                        _montageContext.ClipAssignments.Add(assignment);
                        _montageContext.SaveChanges();
                    }

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
            // TODO: check userId!
            var query = await
                (from p in _montageContext.AdobeProjects
                 where p.ProjectId.Equals(projectId)
                 join a in _montageContext.ClipAssignments
                 on p.ProjectId equals a.ProjectId

                 select a.Clip).Select(c => c.GetAnalysisResult()).ToListAsync();

            return Ok(query);


        }

        /// <summary>
        /// Attempt to find a project with the given projectId
        /// </summary>
        private AdobeProject FindProject(string projectId)
        {
            // TODO: include userId in parameters!
            return (from p in _montageContext.AdobeProjects
                    where p.ProjectId.Equals(projectId)
                    select p).FirstOrDefault();
        }

        /// <summary>
        /// Attempt to find a clip with the given clipId
        /// </summary>
        private AdobeClip FindClip(string clipId)
        {
            // Note: we *shouldn't* include project or user context here
            return (from c in _montageContext.AdobeClips
                    where c.ClipId.Equals(clipId)
                    select c).FirstOrDefault();
        }

        /// <summary>
        /// Attempt to find an assignment between projectId and clipId
        /// </summary>
        private ClipAssignment FindAssignment(string projectId, string clipId)
        {
            return (from a in _montageContext.ClipAssignments
                    where a.ClipId.Equals(clipId) && a.ProjectId.Equals(projectId)
                    select a).FirstOrDefault();
        }

        /// <summary>
        /// Repairs a result which was previously unable to be analyzed
        /// If repair fails, return original result
        /// </summary>
        private AnalysisResult RepairResult(AnalysisResult result)
        {
            // TODO: complete method
            return result;
        }
    }
}
   
