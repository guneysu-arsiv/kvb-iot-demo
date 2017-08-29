using System;
using System.Reactive.Linq;

namespace Sensors
{
    public abstract class Sensor
    {
        public Sensor ( )
        {
            Guid = Guid.NewGuid ( );
        }

        protected Sensor (double min, double max)
        {
            _min = min;
            _max = max;
            _random = new Random ( );

            Observable.Interval (TimeSpan.FromMilliseconds (100)).Subscribe (x => { _current = NextValue ( ); });
        }

        protected double _max { get; }
        protected double _current { get; set; }
        protected double _min { get; }
        protected Random _random { get; set; }

        protected Guid Guid { get; set; }

        public string Id => Guid.ToString ( );

        public double Read ( )
        {
            return _current;
        }

        public double NextValue ( )
        {
            var value = _min + ( _max - _min ) * _random.NextDouble ( );
            return value;
        }
    }
}