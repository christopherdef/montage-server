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
        /// Given a new clip which either hasn't been seen or was processed incorrectly
        /// Direct its file to the AnalysisController for analysis
        /// Save the results in the database
        /// Return the results to the caller
        /// </summary>
        private async Task<IActionResult> ProcessNewClip(IFormFile file, string projectId, string clipId, string userId, string footagePath)
        {
            if (file is null)
                return StatusCode(StatusCodes.Status400BadRequest,
                    new
                    {
                        message = $"Unable to find processed footage with id {projectId}/{clipId} for user {userId}\n" +
                                     "(did you mean to send a file?)"
                    });

            HttpRequest currentRequest = HttpContext.Request;

            // initialize empty response
            AnalysisResult response = new AnalysisResult();
            response.ClipId = clipId;
            response.FootagePath = footagePath;

            // process file in bg
            await Task.Run(() =>
            {
                var fileStream = file.OpenReadStream();

                // if an audio file was sent, return transcript
                if (file.ContentType.StartsWith("audio/"))
                {
                    try
                    {
                        if (!(file.ContentType.EndsWith("/wav")|| file.ContentType.EndsWith("/wave")))
                            fileStream = ConversionController.ConvertToWav(fileStream, file.ContentType);
                        //var buf = ((MemoryStream)fileStream).GetBuffer();
                        //var f = System.IO.File.Create(@"C:\data\audio\converted.wav", buf.Length);
                        //f.Write(buf);
                    }
                    finally
                    {
                        // analyze converted audio
                        // AnalysisController.TranscribeAudio(ref response, file);
                        AnalysisController.AnalyzeAudio(ref response, fileStream);
                        fileStream.Close();
                    }
                }
                else if (file.ContentType.StartsWith("video/"))
                {
                    try
                    {
                        var vidPt = Path.GetTempFileName();
                        var preAudPt = Path.GetTempFileName(); //@"C:\data\pre_tmp.wav";
                        var postAudPt = Path.GetTempFileName(); //@"C:\data\tmp.wav";
                        ConversionController.WriteStreamToDisk(fileStream, vidPt);
                        fileStream = System.IO.File.OpenRead(vidPt);

                        var splitStreams = ConversionController.SeparateAudioVideo(fileStream, file.ContentType);
                        fileStream = splitStreams.audioStream;

                        ConversionController.WriteStreamToDisk(fileStream, preAudPt);
                        
                        fileStream = ConversionController.ConvertToWavFiles(preAudPt, "");

                        ConversionController.WriteStreamToDisk(fileStream, postAudPt);

                        fileStream = System.IO.File.OpenRead(postAudPt);
                    }
                    finally
                    {
                        AnalysisController.AnalyzeAudio(ref response, fileStream);
                        fileStream.Close();
                    }
                }
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

            // clipId, projectId, and userId are required parameters
            if (clipId is null || projectId is null || userId is null)
            {
                StatusCode(StatusCodes.Status400BadRequest,
                                  new
                                  {
                                      message = $"clipId, projectId, and userId must all not be null!\n" +
                                                   $"(userId:{userId}//project:{projectId}//clip:{clipId})"
                                  });
            }

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

            // IF clip+project seen, collect from db
            if (query.Any())
            {
                AdobeClip clip = query.FirstOrDefault();
                AnalysisResult result = (AnalysisResult.DeserializeResponse(clip.AnalysisResultString));

                // if the result in the db has an error, delete it and try again
                if (result is null || result.Error)
                {
                    // delete result from db
                    try
                    {
                        _montageContext.AdobeClips.Remove(clip);
                        _montageContext.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError,
                                          new
                                          {
                                              message = $"(this shouldn't happen!) Unable to repair malformed clip (userId:{userId}//project:{projectId}//clip:{clipId})\n" +
                                                           "Please try adding under another clipId\n" +
                                                           $"Exception:\n{e}"
                                          });
                    }

                    // re-add it
                    return await ProcessNewClip(file, projectId, clipId, userId, footagePath);
                }
                else
                {
                    // return read from db
                    // footagePath with update if provided
                    result.FootagePath = footagePath ?? result.FootagePath;

                    clip.AnalysisResultString = result.Serialize();

                    try
                    {
                        _montageContext.Update(clip);
                        await _montageContext.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        // this REALLY should never happen
                        return StatusCode(StatusCodes.Status500InternalServerError,
                                          new
                                          {
                                              message = $"Unable to update clip with new footage path" +
                                                           $"Exception:\n{e}"
                                          });
                    }

                    return Ok(result);
                }
            }
            // IF clip records were found and the result wasn't an error, return result
            else
            {
                // add new clip to db
                return await ProcessNewClip(file, projectId, clipId, userId, footagePath);
            }
        }

        /// <summary>
        /// Return array of clips which have been assigned to projects matching projectId 
        /// which have users matching userId
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetClips(string userId, string projectId)
        {
            try
            {
                var query = await
                    (from p in _montageContext.AdobeProjects
                     where p.ProjectId.Equals(projectId)
                     join a in _montageContext.ClipAssignments
                     on p.ProjectId equals a.ProjectId
                     select a.Clip).Select(c => c.GetAnalysisResult()).ToListAsync();

                if (query.Any())
                    return Ok(query);
                else
                    return StatusCode(StatusCodes.Status204NoContent,
                                      new { message = $"No clips under user:{userId}/project:{projectId}" });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  new
                                  {
                                      message = $"(idk what happened but it wasn't good!) Error collecting clips under user:{userId}/project:{projectId}\n" +
                                                    $"Error:\n{e}"
                                  });
            }
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

