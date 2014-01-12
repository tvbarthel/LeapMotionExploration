using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;

namespace MyLeap.Processor
{
    class LeapProcessorHandPosition
    {
        public event Action<Vector> OnPositionChange;

        public void Process(Hand hand)
        {
            var palmPosition = hand.StabilizedPalmPosition;
            if (palmPosition.IsValid())
            {
                var normalizedPosition = hand.Frame.InteractionBox.NormalizePoint(palmPosition, true);
                if (normalizedPosition.IsValid())
                {
                    Task.Factory.StartNew(() => OnPositionChange(normalizedPosition));
                }
            }
        }
    }
}
