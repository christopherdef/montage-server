using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MontageServer.Models
{
    public class DisplayAccountInfo
    {
        public string ClipId { get; set; }
        public string FootagePath { get; set; }
        public DisplayClipInformation displayClipInformation { get; set; }
    }

    public class DisplayClipInformation
    {
        public string Transcript { get; set; }

        public string Topics { get; set; }

    }
}
