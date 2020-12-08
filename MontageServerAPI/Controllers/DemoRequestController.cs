using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MontageServerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoRequestController : ControllerBase
    {
        // GET: api/<DemoRequestController>
        [HttpGet]
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

        // GET api/<DemoRequestController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<DemoRequestController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<DemoRequestController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<DemoRequestController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
