using Microsoft.AspNetCore.Mvc;

namespace SensorsApi.Controllers
{
    [Route("api/valf")]
    public class GasValfController : Controller
    {
        [HttpDelete(Name = "BreakGas")]
        public string Delete()
        {
            return "Delete";
        }
        
        [HttpPost(Name = "SupplyGas")]
        public string Post()
        {
            return "Post";
        }
    }
}