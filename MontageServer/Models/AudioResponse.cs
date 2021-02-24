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
    /*
     Class for holding all of the data driven responses to the client
     TODO: replace placeholders types with corresponding models for each
        i.e. "topics" should be its own model and have its own serialization if needed
    */
    [JsonObject(MemberSerialization.OptIn)]
    public class AudioResponse
    {
        [Key]
        [JsonProperty]
        public string ReqId { get; set; }

        [JsonProperty]
        [NotMapped]
        public IDictionary<string, IEnumerable<string>> Topics { get; set; }

        [JsonProperty]
        [NotMapped]
        public IEnumerable<string> Individuals { get; set; }

        [JsonProperty]
        [NotMapped]
        public IEnumerable<string> Objects { get; set; }

        [JsonProperty]
        [NotMapped]
        public IEnumerable<double> Sentiments { get; set; }

        [JsonProperty]
        public string Transcript { get; set; }
    }

    public class Topic : IEnumerable<string>
    {

        public IEnumerable<string> Members { get; set; }

        public IEnumerator<string> GetEnumerator()
        {
            return Members.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Members).GetEnumerator();
        }

    }
}
