using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace MontageServerAPI
{
    /*
     Class for holding all of the data driven responses to the client
     TODO: replace placeholders types with corresponding models for each
        i.e. "topics" should be its own model and have its own serialization if needed
    */
    [JsonObject(MemberSerialization.OptIn)]
    public class Response
    {
        [Key]
        [JsonProperty]
        public string ReqId { get; set; }

        [JsonProperty]
        public IEnumerable<IEnumerable<int>> Topics { get; set; }

        [JsonProperty]
        public IEnumerable<string> Individuals { get; set; }

        [JsonProperty]
        public IEnumerable<string> Objects { get; set; }

        [JsonProperty]
        public IEnumerable<double> Sentiments { get; set; }

        [JsonProperty]
        public string Transcript { get; set; }

    }


}
