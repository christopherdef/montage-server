using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MontageServer.Models
{
    /// <summary>
    /// Assigns a particular clip to a particular project
    /// Multiple clips can be in a project
    /// Multiple projects can own a clip
    /// </summary>
    public class ClipAssignment
    {
        [Key]
        [ForeignKey("Clip")]
        public virtual string ClipId { get; set; }

        [Key]
        public virtual string ProjectId { get; set; }

        [Key]
        public virtual string Id { get; set; }

        [ForeignKey("ProjectId, Id")]
        public virtual AdobeProject Project { get; set; }
        public virtual AdobeClip Clip { get; set; }
    }
}
