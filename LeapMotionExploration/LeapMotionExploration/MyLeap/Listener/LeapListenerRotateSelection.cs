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
    class LeapListenerRotateSelection : Leap.Listener
    {

        public event Action<LeapEvent> OnStateChange;

        private LeapProcessorHandClosed processorLeftHandClosed;
        private LeapProcessorTwoHandRoll processorHandRoll;
        private Vector ClosePosition;
        private Boolean isMainHandClosed;
        private int _mainHand;
        private float lastRoll;

        public LeapListenerRotateSelection(int mainHand)
        {
            isMainHandClosed = false;

            _mainHand = mainHand;

            processorLeftHandClosed = new LeapProcessorHandClosed();
            processorLeftHandClosed.onHandStateChange += mostLeftHandStateChanged;
            processorLeftHandClosed.onStateChange += (x) => { };

            processorHandRoll = new LeapProcessorTwoHandRoll();
            processorHandRoll.onAngleChanged += rollAngleChanged;

            lastRoll = 0;
        }

        public LeapListenerRotateSelection() : this(LeapUtils.RIGHT_MOST_HAND)
        {            
        }

        public override void OnFrame(Controller controller)
        {
            var frame = controller.Frame();
            if (frame.IsValid)
            {
                processorLeftHandClosed.process(GetMainHand(frame));
                
                if (isMainHandClosed && frame.Hands.Count == 2)
                {
                    processorHandRoll.process(frame.Hands.Leftmost, frame.Hands.Rightmost);
                }

            }
        }

        private Hand GetMainHand(Frame frame)
        {
            if (_mainHand.Equals(LeapUtils.RIGHT_MOST_HAND))
            {
                return frame.Hands.Rightmost;
            }

            return frame.Hands.Leftmost;
        }

        public void rollAngleChanged(float newRoll)
        {
            if (lastRoll != 0)
            {
                if (newRoll > lastRoll)
                {
                    System.Diagnostics.Debug.WriteLine("Select Next !");
                    Task.Factory.StartNew(() => OnStateChange(new LeapEvent(ClosePosition, LeapEvent.ROTATION_SELECTION_NEXT)));
                    
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Select Previous !");
                    Task.Factory.StartNew(() => OnStateChange(new LeapEvent(ClosePosition, LeapEvent.ROTATION_SELECTION_PREVIOUS)));
                }
            }
            lastRoll = newRoll;
        }

        public void mostLeftHandStateChanged(HandCloseEvent handClose)
        {
            int type = handClose.Type;
            switch (type)
            {
                case HandCloseEvent.CLOSE:
                    System.Diagnostics.Debug.WriteLine("Selection Started !");
                    isMainHandClosed = true;
                    ClosePosition = handClose.Position;
                    lastRoll = 0f;
                    Task.Factory.StartNew(() => OnStateChange(new LeapEvent(ClosePosition, LeapEvent.ROTATION_SELECTION_START)));
                    break;
                case HandCloseEvent.OPEN:
                    System.Diagnostics.Debug.WriteLine("Selection Ended !");
                    isMainHandClosed = false;                    
                    Task.Factory.StartNew(() => OnStateChange(new LeapEvent(ClosePosition, LeapEvent.ROTATION_SELECTION_END)));
                    ClosePosition = null;
                    break;
            }
        }
    }
}
