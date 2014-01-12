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
        
        private LeapProcessorHandClosed _processorLeftHandClosed;
        private LeapProcessorHandClosed _processorRightHandClosed;
        private LeapProcessorTwoHandRoll _processorHandRoll;
        private LeapProcessorTwoHandDistance _processorHandDistance;
        private Vector _closePosition;
        private Boolean _isLeftHandClosed;
        private Boolean _isRightHandClosed;
        private float _lastRoll;
        private double _lastDistance;

        public LeapListenerTwoHandManipulation()
        {
            _isLeftHandClosed = false;
            _isRightHandClosed = false;

            _processorLeftHandClosed = new LeapProcessorHandClosed();
            _processorLeftHandClosed.OnHandStateChange += MostLeftHandStateChanged;
            _processorLeftHandClosed.OnStateChange += (x) => { };

            _processorRightHandClosed = new LeapProcessorHandClosed();
            _processorRightHandClosed.OnHandStateChange += MostRightHandStateChanged;
            _processorRightHandClosed.OnStateChange += (x) => { };

            _processorHandRoll = new LeapProcessorTwoHandRoll(0.01f);
            _processorHandRoll.OnAngleChanged += RollAngleChanged;

            _processorHandDistance = new LeapProcessorTwoHandDistance(1);
            _processorHandDistance.OnDistanceChanged += HandDistanceChanged;

            _lastRoll = 0;
            _lastDistance = 0;
        }

        public override void OnFrame(Controller controller)
        {
            var frame = controller.Frame();
            if (frame.IsValid)
            {
                _processorLeftHandClosed.Process(frame.Hands.Leftmost);

                if (frame.Hands.Count == 2)
                {
                    _processorRightHandClosed.Process(frame.Hands.Rightmost);
                }
                else
                {
                    _isRightHandClosed = false;
                }

                if (_isLeftHandClosed)
                {
                    if (_isRightHandClosed)
                    {
                        _processorHandDistance.Process(frame.Hands.Leftmost, frame.Hands.Rightmost);
                    }
                    else
                    {
                        _processorHandRoll.Process(frame.Hands.Leftmost, frame.Hands.Rightmost);   
                    }                    
                }
            }
        }

        public void HandDistanceChanged(double newDistance)
        {
            if (_lastDistance != 0)
            {
                if (newDistance > _lastDistance)
                {
                    System.Diagnostics.Debug.WriteLine("Size increases !");
                    Task.Factory.StartNew(() => OnStateChange(new LeapEvent(_closePosition, LeapEvent.TRANSFORMATION_SIZE_UP)));                    
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Size decrease !");
                    Task.Factory.StartNew(() => OnStateChange(new LeapEvent(_closePosition, LeapEvent.TRANSFORMATION_SIZE_DOWN)));
                }
            }
            _lastDistance = newDistance;
        }

        public void RollAngleChanged(float newRoll)
        {
            if(_lastRoll != 0)
            {
                if (newRoll > _lastRoll)
                {
                    System.Diagnostics.Debug.WriteLine("Rotate Unclockwize !");
                    Task.Factory.StartNew(() => OnStateChange(new LeapEvent(_closePosition, LeapEvent.TRANSFORMATION_ROTATE_UNCLOCKWIZE)));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Rotate Clocksize !");
                    Task.Factory.StartNew(() => OnStateChange(new LeapEvent(_closePosition, LeapEvent.TRANSFORMATION_ROTATE_CLOCKWIZE)));
                }
            }
            _lastRoll = newRoll;
        }

        public void MostLeftHandStateChanged(HandCloseEvent e)
        {
            _isLeftHandClosed = e.Type.Equals(HandCloseEvent.OPEN) ? false : true;
            if (_isLeftHandClosed)
            {
                _closePosition = e.Position;
            }
            else
            {
                _closePosition = null;
            }
        }

        public void MostRightHandStateChanged(HandCloseEvent e)
        {
            _isRightHandClosed = e.Type.Equals(HandCloseEvent.OPEN) ? false : true;
        }
    }
}
