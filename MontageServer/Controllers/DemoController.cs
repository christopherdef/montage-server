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

            return null;
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
