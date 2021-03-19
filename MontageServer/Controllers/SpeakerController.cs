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
    public class SpeakerController : ControllerBase
    {
        private readonly MontageDbContext _context;

        public SpeakerController(MontageDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Train speaker segmentation model in this project by enrolling this speaker
        /// </summary>
        // POST: api/Speaker
        [HttpPost]
        public async Task<IActionResult> EnrollSpeaker([FromForm] string userId,
                                                       [FromForm] string projectId,
                                                       [FromForm] string speakerId,
                                                       [FromForm] IEnumerable<IFormFile> files)
        {
            // check if speaker has already been enrolled
            var query = await (from s in _context.SpeakerProfiles
                                 where s.ProjectId.Equals(projectId)
                                 select s).FirstOrDefaultAsync();

            string modelPath = query?.ModelPath ?? Path.GetTempFileName();

            // TODO: fill in speaker enrollment method
            try
            {
                foreach (var f in files)
                {
                    // train model
                    // await AnalysisController.EnrollSpeaker(speakerId, modelPath, f)
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                                  new { message = $"Error enrolling speaker {speakerId}!\nError:\n{e}" });
            }

            // if this speaker hasn't been enrolled, enroll them once training is complete
            if (query == null)
            {
                _context.Add(new SpeakerProfile
                {
                    SpeakerId = speakerId,
                    ProjectId = projectId,
                    UserId = userId,
                    ModelPath = modelPath
                });
                _context.SaveChanges();
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult GetSegmentations([FromForm] string userId,
                                                          [FromForm] string projectId,
                                                          [FromForm] ISet<string> speakerIds,
                                                          [FromForm] IFormFile file)
        {
            // ensure all speakers have been enrolled
            var enrolledSpeakers = speakerIds.Where(sId => (from s in _context.SpeakerProfiles
                                                            where s.SpeakerId.Equals(sId) &&
                                                                  s.ProjectId.Equals(projectId) &&
                                                                  s.UserId.Equals(userId)
                                                            select s).Any()
                                                    );

            // return error message if some speakers haven't been enrolled
            if (!enrolledSpeakers.Any())
            {
                var missingSpeakers = speakerIds.Except(enrolledSpeakers);
                return Ok(new
                {
                    message = "Must enroll all speakers before segmenting this clip!\n" +
                                           $"Missing speakers: {missingSpeakers.ToArray()}"
                });
            }

            // TODO: fill in speaker diarization method
            // await AnalysisController.DiarizeSpeakers(speakerIds, clipFile)

            return Ok();
        }
    }
}
