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

namespace MontageServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(UserManager<IdentityUser> userManager)
        {
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
            return View();
        }

        public IActionResult AccountIndex()
        {            
            return View();
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
