using System;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace SensorsApi.Controllers
{
    [Route("api/valf")]
    public class GasValfController : Controller
    {
        [HttpDelete(Name = "BreakGas")]
        public bool Delete()
        {
            Console.WriteLine("Gas flow blocked");
            return true; // Valf State true = on 
        }
        
        [HttpPost(Name = "SupplyGas")]
        public bool Post()
        {
            return false; // Valf State false = off
        }

        [HttpHead]
        public void Head()
        {
            
        }
    }
}