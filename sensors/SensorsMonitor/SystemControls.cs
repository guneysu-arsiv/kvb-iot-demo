using System;
using System.Threading.Tasks;
using SensorsMonitor.Models;

namespace SensorsMonitor
{
    public class SystemControls
    {
        public void Alarm(SensorData data)
        {
            var gas = this.CutGasFlow();
            var sprinkler = this.ActivateSprinkler();
            Task.WaitAll(gas, sprinkler);
        }

        public Task CutGasFlow()
        {
            throw new NotImplementedException();
        }

        public Task ActivateSprinkler()
        {
            throw new NotImplementedException();
        }
    }
}