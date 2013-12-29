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
        public event Action<String> OnHandStateChanged;
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
            switch (e.Type)
            {
                case HandCloseEvent.OPEN:
                    Task.Factory.StartNew(() => OnHandStateChanged("You have just openned your hand!"));
                    System.Diagnostics.Debug.WriteLine("hand open");
                    break;
                case HandCloseEvent.CLOSE:
                    Task.Factory.StartNew(() => OnHandStateChanged("You have just closed your hand!"));
                    System.Diagnostics.Debug.WriteLine("hand close");
                    break;
            }
        }

    }
}
