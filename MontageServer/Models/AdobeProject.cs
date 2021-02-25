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
        public String ProjectID { get; set; }

        public String Path { get; set; }

        public String AudioResponseString { get; set; }
    }
}
