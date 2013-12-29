using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;
using MyLeap.Utils;
using MyLeap.Processor;

namespace MyLeap.Listener
{
    class LeapListenerOneHandPosition : Leap.Listener
    {

        public event Action<Vector> OnPositionRecieve;
        private LeapProcessorHandPosition processorHandPosition;

        private int HandPreference;

        public LeapListenerOneHandPosition(int handPreference)
        {
            HandPreference = handPreference;
            processorHandPosition = new LeapProcessorHandPosition();
            processorHandPosition.OnPositionChange += OnPositionChange;
        }

        public LeapListenerOneHandPosition() : this(LeapUtils.RIGHT_MOST_HAND)
        {
          
        }

        public override void OnFrame(Controller controller)
        {
            var frame = controller.Frame();
            if (frame.IsValid)
            {
                Hand handToProcess = frame.Hands.Rightmost;
                if (HandPreference == LeapUtils.LEFT_MOST_HAND)
                {
                    handToProcess = frame.Hands.Leftmost;
                }
                processorHandPosition.process(handToProcess);
            }
        }

        public void OnPositionChange(Vector position)
        {
            Task.Factory.StartNew(() => OnPositionRecieve(position));
        }

        public void setHandPreference(int handPreference)
        {
            HandPreference = handPreference;
        }

    }
}
