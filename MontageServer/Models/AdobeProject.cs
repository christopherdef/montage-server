using Newtonsoft.Json;
using System;
using System.Collections;
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

        public string FootagePath { get; set; }

        public string AudioResponseString { get; set; }
    }
}
