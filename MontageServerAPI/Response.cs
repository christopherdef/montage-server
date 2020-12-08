using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MontageServerAPI
{
    public class Response
    {
        public int ReqId { get; set; }

        public IEnumerable<string> Topics { get; set; }

        public IEnumerable<string> Individuals { get; set; }

        public IEnumerable<string> Objects { get; set; }

        public IEnumerable<double> Sentiments { get; set; }

    }

    
}
