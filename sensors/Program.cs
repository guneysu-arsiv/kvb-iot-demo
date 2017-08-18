using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Reactive.Linq;
using System.Threading;

namespace Sensors
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var manager = new SensorManager();

            manager.Watch(new GasSensor(min: 0.05, max: 0.951), TimeSpan.FromMilliseconds(1000));
            manager.Watch(new SmokeSensor(min: 0.05, max: 0.951), TimeSpan.FromMilliseconds(1000));

            Console.ReadKey();

//            Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe(x =>
//            {
//                manager.StopAll();
//            });
//            Thread.Sleep(60);
        }
    }

    public class SensorManager
    {
        List<IDisposable> _subscriptions = new List<IDisposable>();
        List<Sensor> _sensors = new List<Sensor>();

        public void Watch(Sensor sensor, TimeSpan interval)
        {
            _sensors.Add(sensor);

            _subscriptions.Add(Observable.Interval(interval)
                .Subscribe(x => { Console.WriteLine(sensor.Read()); }));
        }

        public void StopAll()
        {
            _subscriptions.ForEach(x => x.Dispose());
            _subscriptions.Clear();
        }
    }

    public abstract class Sensor
    {
        protected Sensor(double min, double max)
        {
            _min = min;
            _max = max;
            _random = new Random();

            Observable.Interval(TimeSpan.FromMilliseconds(100)).Subscribe(x => { _current = NextValue(); });
        }

        protected double _max { get; }
        protected double _current { get; set; }
        protected double _min { get; }
        protected Random _random { get; set; }


        public double Read()
        {
            return _current;
        }

        public double NextValue()
        {
            var value = _min + (_max - _min) * _random.NextDouble();
            return value;
        }
    }

    public class GasSensor : Sensor
    {
        public GasSensor(double min, double max) : base(min, max)
        {
        }
    }

    public class SmokeSensor : Sensor
    {
        public SmokeSensor(double min, double max) : base(min, max)
        {
        }
    }
}