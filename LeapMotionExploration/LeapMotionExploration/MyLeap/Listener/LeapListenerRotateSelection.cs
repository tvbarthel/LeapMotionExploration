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
    class LeapListenerRotateSelection : Leap.Listener
    {

        public event Action<int> OnSelectionChanged;
        public const int SELECTION_START = 0;
        public const int SELECTION_NEXT = 1;
        public const int SELECTION_PREVIOUS = 2;
        public const int SELECTION_END = 3;

        private LeapProcessorHandClosed processorLeftHandClosed;
        private LeapProcessorTwoHandRoll processorHandRoll;
        private Boolean isLeftHandClosed;
        private float lastRoll;

        public LeapListenerRotateSelection()
        {
            isLeftHandClosed = false;

            processorLeftHandClosed = new LeapProcessorHandClosed();
            processorLeftHandClosed.onHandStateChange += mostLeftHandStateChanged;
            processorLeftHandClosed.onStateChange += (x) => { };

            processorHandRoll = new LeapProcessorTwoHandRoll();
            processorHandRoll.onAngleChanged += rollAngleChanged;

            lastRoll = 0;
        }

        public override void OnFrame(Controller controller)
        {
            var frame = controller.Frame();
            if (frame.IsValid)
            {
                processorLeftHandClosed.process(frame.Hands.Leftmost);

                if (isLeftHandClosed && frame.Hands.Count == 2)
                {
                    processorHandRoll.process(frame.Hands.Leftmost, frame.Hands.Rightmost);
                }

            }
        }

        public void rollAngleChanged(float newRoll)
        {
            if (lastRoll != 0)
            {
                if (newRoll > lastRoll)
                {
                    System.Diagnostics.Debug.WriteLine("Select Next !");
                    Task.Factory.StartNew(() => OnSelectionChanged(SELECTION_NEXT));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Select Previous !");
                    Task.Factory.StartNew(() => OnSelectionChanged(SELECTION_PREVIOUS));
                }
            }
            lastRoll = newRoll;
        }

        public void mostLeftHandStateChanged(HandCloseEvent handClose)
        {
            int type = handClose.Type;
            switch (type)
            {
                case HandCloseEvent.OPEN:
                    System.Diagnostics.Debug.WriteLine("Selection Started !");
                    lastRoll = 0f;
                    Task.Factory.StartNew(() => OnSelectionChanged(SELECTION_START));
                    break;
                case HandCloseEvent.CLOSE:
                    System.Diagnostics.Debug.WriteLine("Selection Ended !");
                    Task.Factory.StartNew(() => OnSelectionChanged(SELECTION_END));
                    break;
            }
        }
    }
}
