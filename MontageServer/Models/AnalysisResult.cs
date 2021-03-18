using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
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
    public class AnalysisResult : object
    {
        [Key]
        [JsonProperty]
        public string ClipId { get; set; }

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

        [JsonProperty]
        public bool Error { get; set; } = false;

        [JsonProperty]
        [NotMapped]
        public List<string> ErrorMessages { get; set; } = new List<string>();


        /// <summary>
        /// Deserialize the json AnalysisResult in a string
        /// </summary>
        public static AnalysisResult DeserializeResponse(string str)
        {
            var options = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
                
            };

            // deserialize json AnalysisResult from script
            Console.WriteLine(str);

            try
            {
                return JsonConvert.DeserializeObject<AnalysisResult>(str, options);
            }
            catch (Exception ex)
            {
                return new AnalysisResult() { Transcript = $"PARSED STRING\n{str}\n\n\nEXCEPTION\n{ex}", Error = true };
            }
        }

        private class PythonResponse
        {
            [JsonProperty]
            public string ClipId { get; set; }

            [JsonProperty]
            public IDictionary<string, IEnumerable<string>> Topics { get; set; }

            [JsonProperty]
            public IEnumerable<string> Individuals { get; set; }

            [JsonProperty]
            public IEnumerable<string> Objects { get; set; }

            [JsonProperty]
            public IEnumerable<double> Sentiments { get; set; }

            [JsonProperty]
            public string Transcript { get; set; }
        }
        /// <summary>
        /// Deserialize the json AnalysisResult returned by the Python script
        /// </summary>
        public static AnalysisResult DeserializePythonResponse(string scriptOutput)
        {
            var options = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All

            };

            try
            {
                //// dynamically resolve members of script json output (god, real languages have so many rules)
                //dynamic responseObj = JsonConvert.DeserializeObject<ExpandoObject>(scriptOutput, options);

                var pr = JsonConvert.DeserializeObject<PythonResponse>(scriptOutput, options);
                var ar = new AnalysisResult()
                {
                    Topics = pr.Topics,
                    Sentiments = pr.Sentiments,
                    Objects = pr.Objects,
                    Individuals = pr.Individuals,
                    Transcript = pr.Transcript
                };
                //var getPropType = new Func<string, Type>(s => ar.GetType().GetProperty(s).GetType());
                //ar.Transcript = responseObj.transcript;
                //Type topic_type = getPropType("Topics");
                //ar.Topics = 
                //ar.Sentiments = Slapper.AutoMapper.MapDynamic<IEnumerable<double>> (responseObj.sentiments) as IEnumerable<double>;
                //ar.Objects = Slapper.AutoMapper.MapDynamic<IEnumerable<string>> (responseObj.objects) as IEnumerable<string>;
                //ar.Individuals = Slapper.AutoMapper.MapDynamic<IEnumerable<string>> (responseObj.individuals) as IEnumerable<string>;
                return ar;
            }
            catch (Exception ex)
            {
                return new AnalysisResult() { Transcript = $"SCRIPT OUTPUT\n{scriptOutput}\n\n\nEXCEPTION\n{ex}", Error = true };
            }
        }

        /// <summary>
        /// Right join of attrs in rhsResult with this
        /// </summary>
        public void JoinRight(AnalysisResult rhsResult)
        {
            ClipId = rhsResult.ClipId ?? ClipId;
            Transcript = rhsResult.Transcript ?? Transcript;
            FootagePath = rhsResult.FootagePath ?? FootagePath;
            Error = rhsResult.Error;

            Topics = rhsResult.Topics ?? Topics;
            Sentiments = rhsResult.Sentiments ?? Sentiments;
            Individuals = rhsResult.Individuals ?? Individuals;
            Objects = rhsResult.Objects ?? Objects;
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
