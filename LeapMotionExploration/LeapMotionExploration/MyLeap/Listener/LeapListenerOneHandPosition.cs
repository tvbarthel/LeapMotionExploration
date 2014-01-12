using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;
using MyLeap.Utils;
using MyLeap.Processor;
using MyLeap.Event;

namespace MyLeap.Listener
{
    class LeapListenerOneHandPosition : Leap.Listener
    {

        public event Action<LeapEvent> OnStateChange;

        private LeapProcessorHandPosition _processorHandPosition;
        private int _handPreference;

        public LeapListenerOneHandPosition(int handPreference)
        {
            _handPreference = handPreference;
            _processorHandPosition = new LeapProcessorHandPosition();
            _processorHandPosition.OnPositionChange += OnPositionChange;
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
                if (_handPreference == LeapUtils.LEFT_MOST_HAND)
                {
                    handToProcess = frame.Hands.Leftmost;
                }
                _processorHandPosition.Process(handToProcess);
            }
        }

        public void OnPositionChange(Vector position)
        {
            Task.Factory.StartNew(() => OnStateChange(new LeapEvent(position, LeapEvent.POSITION)));
        }

        public void SetHandPreference(int handPreference)
        {
            _handPreference = handPreference;
        }

    }
}
