using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MontageServer.Models
{
    public class AdobeProject
    {
        [Key]
        public string ProjectId { get; set; }

        [Key]
        public virtual string UserId { get; set; }

    }
}
