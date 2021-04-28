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

            var currentUserId = _userManager.GetUserId(HttpContext.User);
            return View(_montageContext.AdobeClips.Where(y => _montageContext.ClipAssignments.Where(x => x.UserId == currentUserId).Any(z => z.ClipId == y.ClipId)).ToList());
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
