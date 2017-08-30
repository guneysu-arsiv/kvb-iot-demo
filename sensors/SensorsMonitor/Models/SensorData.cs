using System;

namespace SensorsMonitor.Models
{
    public class SensorData
    {
        public DateTime DateTime { get; set; }
        public float Value { get; set; }
        public string Type { get; set; }
    }
}