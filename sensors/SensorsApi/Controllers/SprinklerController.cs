using System;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace SensorsApi.Controllers
{
    [Route("api/sprinkler")]
    public class SprinklerController : Controller
    {
        [HttpDelete(Name = "StopSprinkler")]
        public bool Delete()
        {
            Console.WriteLine("Sprinklerrr");
            return false; // Sprinkler state false = off
        }
        
        [HttpPost(Name = "StartSprinkler")]
        public bool Post()
        {
            Console.WriteLine("Sprinkler System Activated");
            return true; // Sprinkler state true = on
        }

        [HttpHead]
        public void Head()
        {
            
        }
    }
}