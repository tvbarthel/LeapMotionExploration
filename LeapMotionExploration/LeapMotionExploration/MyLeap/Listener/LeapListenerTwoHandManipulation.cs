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
    class LeapListenerTwoHandManipulation : Leap.Listener
    {
        public event Action<LeapEvent> OnStateChange;
        
        private LeapProcessorHandClosed processorLeftHandClosed;
        private LeapProcessorHandClosed processorRightHandClosed;
        private LeapProcessorTwoHandRoll processorHandRoll;
        private LeapProcessorTwoHandDistance processorHandDistance;
        private Vector ClosePosition;
        private Boolean isLeftHandClosed;
        private Boolean isRightHandClosed;
        private float lastRoll;
        private double lastDistance;

        public LeapListenerTwoHandManipulation()
        {
            isLeftHandClosed = false;
            isRightHandClosed = false;

            processorLeftHandClosed = new LeapProcessorHandClosed();
            processorLeftHandClosed.onHandStateChange += mostLeftHandStateChanged;
            processorLeftHandClosed.onStateChange += (x) => { };

            processorRightHandClosed = new LeapProcessorHandClosed();
            processorRightHandClosed.onHandStateChange += mostRightHandStateChanged;
            processorRightHandClosed.onStateChange += (x) => { };

            processorHandRoll = new LeapProcessorTwoHandRoll(0.01f);
            processorHandRoll.onAngleChanged += rollAngleChanged;

            processorHandDistance = new LeapProcessorTwoHandDistance(1);
            processorHandDistance.OnDistanceChanged += HandDistanceChanged;

            lastRoll = 0;
            lastDistance = 0;
        }

        public override void OnFrame(Controller controller)
        {
            var frame = controller.Frame();
            if (frame.IsValid)
            {
                processorLeftHandClosed.process(frame.Hands.Leftmost);

                if (frame.Hands.Count == 2)
                {
                    processorRightHandClosed.process(frame.Hands.Rightmost);
                }
                else
                {
                    isRightHandClosed = false;
                }

                if (isLeftHandClosed)
                {
                    if (isRightHandClosed)
                    {
                        processorHandDistance.process(frame.Hands.Leftmost, frame.Hands.Rightmost);
                    }
                    else
                    {
                        processorHandRoll.process(frame.Hands.Leftmost, frame.Hands.Rightmost);   
                    }                    
                }
            }
        }

        public void HandDistanceChanged(double newDistance)
        {
            if (lastDistance != 0)
            {
                if (newDistance > lastDistance)
                {
                    System.Diagnostics.Debug.WriteLine("Size increases !");
                    Task.Factory.StartNew(() => OnStateChange(new LeapEvent(ClosePosition, LeapEvent.TRANSFORMATION_SIZE_UP)));                    
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Size decrease !");
                    Task.Factory.StartNew(() => OnStateChange(new LeapEvent(ClosePosition, LeapEvent.TRANSFORMATION_SIZE_DOWN)));
                }
            }
            lastDistance = newDistance;
        }

        public void rollAngleChanged(float newRoll)
        {
            if(lastRoll != 0)
            {
                if (newRoll > lastRoll)
                {
                    System.Diagnostics.Debug.WriteLine("Rotate Unclockwize !");
                    Task.Factory.StartNew(() => OnStateChange(new LeapEvent(ClosePosition, LeapEvent.TRANSFORMATION_ROTATE_UNCLOCKWIZE)));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Rotate Clocksize !");
                    Task.Factory.StartNew(() => OnStateChange(new LeapEvent(ClosePosition, LeapEvent.TRANSFORMATION_ROTATE_CLOCKWIZE)));
                }
            }
            lastRoll = newRoll;
        }

        public void mostLeftHandStateChanged(HandCloseEvent e)
        {
            isLeftHandClosed = e.Type.Equals(HandCloseEvent.OPEN) ? false : true;
            if (isLeftHandClosed)
            {
                ClosePosition = e.Position;
            }
            else
            {
                ClosePosition = null;
            }
        }

        public void mostRightHandStateChanged(HandCloseEvent e)
        {
            isRightHandClosed = e.Type.Equals(HandCloseEvent.OPEN) ? false : true;
        }
    }
}
