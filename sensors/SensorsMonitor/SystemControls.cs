using System;
using System.Threading.Tasks;
using SensorsMonitor.Models;
using Flurl;
using Flurl.Http;

namespace SensorsMonitor
{
    public class SystemControls
    {
        public SystemControls()
        {
            this.BaseApiUrl =  "http://localhost:5000".AppendPathSegment("api");
        }

        public Url BaseApiUrl { get; set; }

        public void Alarm(SensorData data)
        {
            var gas = this.CutGasFlow();
            var sprinkler = this.ActivateSprinkler();
            var timeout = TimeSpan.FromSeconds(1);

            gas.Wait(timeout);
            sprinkler.Wait(timeout);
            
//            Task.WaitAll(gas, sprinkler);
        }

        public Task<bool> CutGasFlow()
        {
            return BaseApiUrl
                .AppendPathSegment("valf")
                .DeleteAsync()
                .ReceiveJson<bool>();
        }

        public Task<bool> ActivateSprinkler()
        {
            return BaseApiUrl
                .AppendPathSegment("sprinkler")
                .PostAsync()
                .ReceiveJson<bool>();
        }
    }
}