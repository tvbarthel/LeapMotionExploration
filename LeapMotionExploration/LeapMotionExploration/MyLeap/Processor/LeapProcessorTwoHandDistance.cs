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

        private double distanceDelta;
        private double lastDistance;

        public LeapProcessorTwoHandDistance(double delta)
        {
            distanceDelta = delta;
        }

        public LeapProcessorTwoHandDistance()
        {
            distanceDelta = DISTANCE_DELTA_NORMAL;
        }

        public void process(Hand mostLeftHand, Hand mostRightHand)
        {
            double currentDistance = Math.Sqrt(Math.Pow(mostLeftHand.PalmPosition.x - mostRightHand.PalmPosition.x, 2) + Math.Pow(mostLeftHand.PalmPosition.y - mostRightHand.PalmPosition.y, 2));
            if (Math.Abs(currentDistance - lastDistance) > distanceDelta)
            {
                System.Diagnostics.Debug.WriteLine("Distance Changed -> " + currentDistance);
                Task.Factory.StartNew(() => OnDistanceChanged(currentDistance));
                lastDistance = currentDistance;
            }
        }
    }
}
