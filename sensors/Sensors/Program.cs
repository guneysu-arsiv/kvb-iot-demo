using System;
using Sensors.Simulation;

namespace Sensors
{
    public class Program
    {
        /*
         * http://www.hivemq.com/blog/mqtt-essentials-part-5-mqtt-topics-best-practices
         
         */
        public static void Main(string[] args)
        {
            var manager = new SensorSimulation();

            manager.Watch(new Gas(min: 0.05, max: 0.95), TimeSpan.FromMilliseconds(1000));
            manager.Watch(new Smoke(min: 0.05, max: 0.95), TimeSpan.FromMilliseconds(1000));

            Console.ReadLine();

            //            Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe(x =>
            //            {
            //                manager.StopAll();
            //            });
            //            Thread.Sleep(60);
        }
    }
}