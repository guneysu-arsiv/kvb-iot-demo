using Microsoft.AspNetCore.Mvc;

namespace SensorsApi.Controllers
{
    [Route("api/sprinkler")]
    public class SprinklerController : Controller
    {
        [HttpDelete(Name = "StopSprinkler")]
        public string Delete()
        {
            return "Delete";
        }
        
        [HttpPost(Name = "StartSprinkler")]
        public string Post()
        {
            return "Post";
        }
    }
}