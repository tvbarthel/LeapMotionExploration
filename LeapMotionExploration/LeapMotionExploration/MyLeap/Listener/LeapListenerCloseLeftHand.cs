using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;
using MyLeap.Processor;

namespace MyLeap.Listener
{
    class LeapListenerCloseLeftHand : Leap.Listener
    {
        public event Action<String> OnHandStateChanged;
        private LeapProcessorHandClosed detector;

        public LeapListenerCloseLeftHand() {
            detector = new LeapProcessorHandClosed();
            detector.onHandStateChange += mostLeftHandStateChanged;
        }

        public override void OnFrame(Controller controller)
        {
            var frame = controller.Frame();
            if (frame.IsValid)
            {
                detector.process(frame.Hands.Leftmost);
            }
        }

        public void mostLeftHandStateChanged(Boolean isClosed)
        {
            System.Diagnostics.Debug.WriteLine("Is closed ? " + isClosed);
            if (isClosed)
            {
                Task.Factory.StartNew(() => OnHandStateChanged("You have just closed your hand!"));
            }
            else
            {
                Task.Factory.StartNew(() => OnHandStateChanged("You have just openned your hand!"));
            }
        }

    }
}
