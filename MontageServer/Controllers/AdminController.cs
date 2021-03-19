using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MontageServer.Data;
using MontageServer.Models;

namespace MontageServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly MontageDbContext _context;
        private ILogger<AdminController> _logger;
        public AdminController(MontageDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("DropAll")]
        public async Task<IActionResult> DropAll()
        {

#if (DEBUG)
            _context.Database.ExecuteSqlRaw("DELETE FROM [AdobeClip];");
            _context.Database.ExecuteSqlRaw("DELETE FROM [ClipAssignment];");
            _context.Database.ExecuteSqlRaw("DELETE FROM [AdobeProject];");

            await _context.SaveChangesAsync();
            return Ok(new { message = "records deleted" });
#else
            return Ok(new {message = "not enabled"});
#endif
        }
    }
}
