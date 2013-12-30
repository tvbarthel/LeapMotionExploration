using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;
using MyLeap.Processor;
using MyLeap.Event;

namespace MyLeap.Listener
{
    class LeapListenerCloseLeftHand : Leap.Listener
    {
        public event Action<HandCloseEvent> OnHandStateChanged;
        private LeapProcessorHandClosed detector;

        public LeapListenerCloseLeftHand()
        {
            detector = new LeapProcessorHandClosed();
            detector.onHandStateChange += mostLeftHandStateChanged;
            detector.onStateChange += (x) => { };
        }

        public override void OnFrame(Controller controller)
        {
            var frame = controller.Frame();
            if (frame.IsValid)
            {
                detector.process(frame.Hands.Leftmost);
            }
        }

        public void mostLeftHandStateChanged(HandCloseEvent e)
        {
            Task.Factory.StartNew(() => OnHandStateChanged(e));
        }

    }
}
