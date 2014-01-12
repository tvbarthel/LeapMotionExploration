using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;

namespace MyLeap.Processor
{
    class LeapProcessorTwoHandRoll
    {
        public event Action<float> OnAngleChanged;

        public const float ROLL_DELTA_SMALL = 0.10f;
        public const float ROLL_DELTA_NORMAL = 0.20f;
        public const float ROLL_DELTA_HIGH = 0.30f;

        private float _rollDelta;
        private float _lastRoll;

        public LeapProcessorTwoHandRoll(float delta)
        {
            _rollDelta = delta;
        }

        public LeapProcessorTwoHandRoll()
        {
            _rollDelta = ROLL_DELTA_NORMAL;
        }


        public void Process(Hand leftMostHand, Hand rightMostHand) {
            Vector twoPalmVector = new Vector(rightMostHand.PalmPosition.x - leftMostHand.PalmPosition.x,
                rightMostHand.PalmPosition.y - leftMostHand.PalmPosition.y,
                rightMostHand.PalmPosition.z - leftMostHand.PalmPosition.z);
            float candidateRoll = twoPalmVector.Roll;
            if (Math.Abs(candidateRoll - _lastRoll) > _rollDelta)
            {
                _lastRoll = candidateRoll;
                System.Diagnostics.Debug.WriteLine("Angle -> " + _lastRoll);
                Task.Factory.StartNew(() => OnAngleChanged(_lastRoll));
            }
            
        }
        
    }
}
