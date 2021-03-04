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
        public string ProjectId { get; set; }

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

        [JsonProperty]
        public string FootagePath { get; set; }


        /// <summary>
        /// Deserialize the json AudioResponse returned by the Python script
        /// </summary>
        public static AudioResponse DeserializeResponse(string scriptOutput)
        {
            var options = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            // deserialize json AudioResponse from script
            Console.WriteLine(scriptOutput);

            try
            {
                return JsonConvert.DeserializeObject<AudioResponse>(scriptOutput, options);
            }
            catch (Exception ex)
            {
                return new AudioResponse() { Transcript = $"SCRIPT OUTPUT\n{scriptOutput}\n\n\nEXCEPTION\n{ex}" };
            }

        }

        public string Serialize()
        {
            var options = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.SerializeObject(this, options);
        }
    }
}
