using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MontageServerAPI;

namespace MontageServer.Controllers
{
    public class DemoController : ApiController
    {
        // GET: api/Demo
        public IEnumerable<Response> Get()
        {
            var rng = new Random();

            var sentiments = new List<double>();
            for (int i = 0; i < 10; i++)
                sentiments.Add(rng.NextDouble());

            var topics = new List<string>();
            for (int i = 0; i < 10; i++)
                topics.Add(string.Format(""));

            return Enumerable.Range(1, 5).Select(index => new Response
            {
                ReqId = rng.Next(10000, 99999),
                Topics = new string[] { "topic 1", "topic 2", "topic 3" },
                Individuals = new string[] { "individual A", "individual B" },
                Objects = new string[] { "sample object response" },
                Sentiments = sentiments
            })
            .ToArray();
        }

        // GET: api/Demo/5
        public string Get(int id)
        {
            return $"hello id:{id}!";
        }

        //api/demo/byName?firstName=a&lastName=b
        [HttpGet]
        public string Get(string firstName, string lastName, string address)
        {
            return $"hello {firstName} {lastName}, who lives at {address}!";
        }


        // POST: api/Demo
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Demo/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Demo/5
        public void Delete(int id)
        {
        }
    }
}
