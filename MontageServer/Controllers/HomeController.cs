using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MontageServer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MontageServer.Areas.Identity.Pages.Account;
using MontageServer.Data;
using System.Security.Authentication;

namespace MontageServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly MontageDbContext _montageContext;

        public HomeController(MontageDbContext montageContex, UserManager<IdentityUser> userManager)
        {
            _montageContext = montageContex;
            _userManager = userManager;
            
        }

        public IActionResult Index()
        {
            if (_userManager != null)
            {
                ViewBag.user = _userManager.GetUserName(HttpContext.User);

            }


            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult Tutorial()
        {
            return View();
        }

        public IActionResult Account()
        {
            if (_userManager != null)
            {
                ViewBag.user = _userManager.GetUserName(HttpContext.User);

            }

            var returnList = new List<DisplayAccountInfo>();
            var currentUserId = _userManager.GetUserId(HttpContext.User);
            foreach (var item in _montageContext.AdobeClips.Where(y => _montageContext.ClipAssignments.Where(x => x.UserId == currentUserId).Any(z => z.ClipId == y.ClipId)).ToList())
            {
                var analysisResult = AnalysisResult.DeserializeResponse(item.AnalysisResultString);

                var topicsString = "";
                foreach (var topic in analysisResult.Topics)
                {
                    string addString = topic.Value.ToString();

                    topicsString += "Popularity " + topic.Key + ": " + string.Join(", ", topic.Value.ToArray()) + "\n"; //addTopic.Add(new DisplayTopics() { match = topic.Key, topics = string.Join(",", topic.Value.ToArray()) });
                }

                returnList.Add(new DisplayAccountInfo() { ClipId = item.ClipId, FootagePath = item.FootagePath, displayClipInformation = new DisplayClipInformation() { Transcript = analysisResult.Transcript, Topics = topicsString } });
            }
            return View(returnList);

        }


        public IActionResult Delete(string clipID)
        {
            var currentUserId = _userManager.GetUserId(HttpContext.User);
            var removeClipAssignment = _montageContext.ClipAssignments.FirstOrDefault(x => x.ClipId == clipID && x.UserId == currentUserId);

            if (removeClipAssignment != null)
                _montageContext.ClipAssignments.Remove(removeClipAssignment);

            var removeClip = _montageContext.AdobeClips.FirstOrDefault(x => x.ClipId == clipID);

            if (removeClip != null)
                _montageContext.AdobeClips.Remove(removeClip);

            _montageContext.SaveChanges();


            return View("~/Views/Home/Account.cshtml", _montageContext.AdobeClips.Where(y => _montageContext.ClipAssignments.Where(x => x.UserId == currentUserId).Any(z => z.ClipId == y.ClipId)).ToList());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
