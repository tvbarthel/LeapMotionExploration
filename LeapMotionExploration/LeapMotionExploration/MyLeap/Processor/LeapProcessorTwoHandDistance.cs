using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;

namespace MyLeap.Processor
{
    class LeapProcessorTwoHandDistance
    {
        public event Action<double> OnDistanceChanged;

        public const double DISTANCE_DELTA_SMALL = 0.5;
        public const double DISTANCE_DELTA_NORMAL = 1.0;
        public const double DISTANCE_DELTA_HIGH = 2.0;

        private double _distanceDelta;
        private double _lastDistance;

        public LeapProcessorTwoHandDistance(double delta)
        {
            _distanceDelta = delta;
        }

        public LeapProcessorTwoHandDistance()
        {
            _distanceDelta = DISTANCE_DELTA_NORMAL;
        }

        public void Process(Hand mostLeftHand, Hand mostRightHand)
        {
            double currentDistance = Math.Sqrt(Math.Pow(mostLeftHand.PalmPosition.x - mostRightHand.PalmPosition.x, 2) + Math.Pow(mostLeftHand.PalmPosition.y - mostRightHand.PalmPosition.y, 2));
            if (Math.Abs(currentDistance - _lastDistance) > _distanceDelta)
            {
                System.Diagnostics.Debug.WriteLine("Distance Changed -> " + currentDistance);
                Task.Factory.StartNew(() => OnDistanceChanged(currentDistance));
                _lastDistance = currentDistance;
            }
        }
    }
}
