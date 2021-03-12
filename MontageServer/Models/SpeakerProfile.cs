using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MontageServer.Models
{
    public class SpeakerProfile
    {

        [Key]
        public string SpeakerId { get; set; }

        [Key]
        public virtual string ProjectId { get; set; }

        [Key]
        public virtual string UserId { get; set; }

        public string ModelPath { get; set; } = null;


        [ForeignKey("ProjectId, UserId")]
        public virtual AdobeProject Project { get; set; }
    }
}
