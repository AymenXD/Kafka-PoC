using Microsoft.AspNetCore.Mvc;
namespace DataGeneration.Controllers
{
    [ApiController]
    [Route("data")]
    public class DataController : ControllerBase
    {
        private static readonly Random random = new Random();

        [HttpGet("ats1")]
        public IActionResult GetAts1Data()
        {
            var data = new
            {
                device_id = random.Next(0, 5),
                temperature = random.NextDouble() * 10 + 20,
                humidity = random.NextDouble() * 30 + 30,
                timestamp = DateTimeOffset.UtcNow
            };
            return Ok(data);
        }

        [HttpGet("ats2")]
        public IActionResult GetAts2Data()
        {
            var data = new
            {
                device_id = random.Next(0, 5),
                signal = random.Next(0,601),
                timestamp = DateTimeOffset.UtcNow
            };
            return Ok(data);
        }

        [HttpGet("ats3")]
        public IActionResult GetAts3Data()
        {
            var data = new
            {
                device_id = random.Next(0, 5),
                latitude = random.NextDouble() * 10 + 20,
                longitude = random.NextDouble() * 10 + 50,
                timestamp = DateTimeOffset.UtcNow
            };
            return Ok(data);
        }
    }
}
