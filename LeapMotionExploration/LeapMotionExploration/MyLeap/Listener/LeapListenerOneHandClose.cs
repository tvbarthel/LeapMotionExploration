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
        public event Action<HandCloseEvent> OnHandStateChanged;
        private LeapProcessorHandClosed detector;
        private int HandPreference;

        public LeapListenerOneHandClose(int handPreference)
        {
            HandPreference = handPreference;
            detector = new LeapProcessorHandClosed();
            detector.onHandStateChange += mostLeftHandStateChanged;
            detector.onStateChange += (x) => { };
        }

        public LeapListenerOneHandClose() : this(LeapUtils.RIGHT_MOST_HAND) {

        }

        public override void OnFrame(Controller controller)
        {
            var frame = controller.Frame();
            if (frame.IsValid)
            {
                if (HandPreference == LeapUtils.RIGHT_MOST_HAND)
                {
                    detector.process(frame.Hands.Rightmost);
                }
                else if (HandPreference == LeapUtils.LEFT_MOST_HAND)
                {
                    detector.process(frame.Hands.Leftmost);
                }                
            }
        }

        public void mostLeftHandStateChanged(HandCloseEvent e)
        {
            Task.Factory.StartNew(() => OnHandStateChanged(e));
        }

    }
}
