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
    public class AdobeClip
    {
        [Key]
        public string ClipId { get; set; }

        public string FootagePath { get; set; }

        public string AnalysisResultString { get; set; }


        public AnalysisResult GetAnalysisResult()
        {
            return AnalysisResult.DeserializeResponse(AnalysisResultString);
        }
    }
}
