using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;
using MyLeap.Processor;
using MyLeap.Event;
using MyLeap.Utils;

namespace MyLeap.Listener
{
    class LeapListenerOneHandClose : Leap.Listener
    {
        public event Action<HandCloseEvent> OnHandStateChange;

        private LeapProcessorHandClosed _detector;
        private int _handPreference;

        public LeapListenerOneHandClose(int handPreference)
        {
            _handPreference = handPreference;
            _detector = new LeapProcessorHandClosed();
            _detector.OnHandStateChange += HandStateChanged;
            _detector.OnStateChange += (x) => { };
        }

        public LeapListenerOneHandClose() : this(LeapUtils.RIGHT_MOST_HAND) {

        }

        public override void OnFrame(Controller controller)
        {
            var frame = controller.Frame();
            if (frame.IsValid)
            {
                if (_handPreference == LeapUtils.RIGHT_MOST_HAND)
                {
                    _detector.Process(frame.Hands.Rightmost);
                }
                else if (_handPreference == LeapUtils.LEFT_MOST_HAND)
                {
                    _detector.Process(frame.Hands.Leftmost);
                }                
            }
        }

        public void HandStateChanged(HandCloseEvent e)
        {
            Task.Factory.StartNew(() => OnHandStateChange(e));
        }

    }
}
