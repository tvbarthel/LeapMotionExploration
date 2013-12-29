using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;
using MyLeap.Processor;

namespace MyLeap.Listener
{
    class LeapListenerTwoHandManipulation : Leap.Listener
    {
        public event Action<int> OnAction;
        public const int ACTION_SIZE_UP = 0;
        public const int ACTION_SIZE_DOWN = 1;
        public const int ACTION_ROTATE_CLOCKWIZE = 2;
        public const int ACTION_ROTATE_UNCLOCKWIZE = 3;
        
        private LeapProcessorHandClosed processorLeftHandClosed;
        private LeapProcessorHandClosed processorRightHandClosed;
        private LeapProcessorTwoHandRoll processorHandRoll;
        private LeapProcessorTwoHandDistance processorHandDistance;
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

            processorRightHandClosed = new LeapProcessorHandClosed();
            processorRightHandClosed.onHandStateChange += mostRightHandStateChanged;

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
                    Task.Factory.StartNew(() => OnAction(ACTION_SIZE_UP));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Size decrease !");
                    Task.Factory.StartNew(() => OnAction(ACTION_SIZE_DOWN));
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
                    Task.Factory.StartNew(() => OnAction(ACTION_ROTATE_UNCLOCKWIZE));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Rotate Clocksize !");
                    Task.Factory.StartNew(() => OnAction(ACTION_ROTATE_CLOCKWIZE));
                }
            }
            lastRoll = newRoll;
        }

        public void mostLeftHandStateChanged(Boolean isClosed)
        {            
            isLeftHandClosed = isClosed;
        }

        public void mostRightHandStateChanged(Boolean isClosed)
        {
            isRightHandClosed = isClosed;
        }
    }
}
