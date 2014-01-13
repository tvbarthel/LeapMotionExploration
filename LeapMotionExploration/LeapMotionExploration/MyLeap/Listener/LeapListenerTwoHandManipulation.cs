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
    class LeapListenerTwoHandManipulation : Leap.Listener
    {
        public event Action<LeapEvent> OnStateChange;
        
        private LeapProcessorHandClosed _processorPrimaryHandClosed;
        private LeapProcessorHandClosed _processorSecondaryHandClosed;
        private LeapProcessorTwoHandRoll _processorHandRoll;
        private LeapProcessorTwoHandDistance _processorHandDistance;
        private Vector _closePosition;
        private Boolean _isPrimaryHandClosed;
        private Boolean _isSecondaryHandClosed;
        private float _lastRoll;
        private double _lastDistance;
        private int _primaryHand;

        public LeapListenerTwoHandManipulation(int primaryHand)
        {
            _primaryHand = primaryHand;

            _isPrimaryHandClosed = false;
            _isSecondaryHandClosed = false;

            _processorPrimaryHandClosed = new LeapProcessorHandClosed();
            _processorPrimaryHandClosed.OnHandStateChange += PrimaryHandStateChanged;
            _processorPrimaryHandClosed.OnStateChange += (x) => { };

            _processorSecondaryHandClosed = new LeapProcessorHandClosed();
            _processorSecondaryHandClosed.OnHandStateChange += SecondaryHandStateChanged;
            _processorSecondaryHandClosed.OnStateChange += (x) => { };

            _processorHandRoll = new LeapProcessorTwoHandRoll(0.01f);
            _processorHandRoll.OnAngleChanged += RollAngleChanged;

            _processorHandDistance = new LeapProcessorTwoHandDistance(1);
            _processorHandDistance.OnDistanceChanged += HandDistanceChanged;

            _lastRoll = 0;
            _lastDistance = 0;
        }

        public LeapListenerTwoHandManipulation() : this(LeapUtils.RIGHT_MOST_HAND) { }

        public override void OnFrame(Controller controller)
        {
            var frame = controller.Frame();
            if (frame.IsValid)
            {
                Hand primaryHand = GetPrimaryHand(frame);
                Hand secondaryHand = GetSecondaryHand(frame);
                
                _processorPrimaryHandClosed.Process(primaryHand);

                if (frame.Hands.Count == 2)
                {
                    _processorSecondaryHandClosed.Process(secondaryHand);
                }
                else
                {
                    _isSecondaryHandClosed = false;
                }

                if (_isPrimaryHandClosed)
                {
                    if (_isSecondaryHandClosed)
                    {
                        _processorHandDistance.Process(primaryHand, secondaryHand);
                    }
                    else
                    {
                        _processorHandRoll.Process(primaryHand, secondaryHand);   
                    }                    
                }
            }
        }

        private Hand GetPrimaryHand(Frame frame)
        {
            Hand primaryHand = frame.Hands.Rightmost;

            if (_primaryHand.Equals(LeapUtils.LEFT_MOST_HAND))
            {
                primaryHand = frame.Hands.Leftmost;
            }

            return primaryHand;
        }

        private Hand GetSecondaryHand(Frame frame)
        {
            Hand secondaryHand = frame.Hands.Leftmost;

            if (_primaryHand.Equals(LeapUtils.LEFT_MOST_HAND))
            {
                secondaryHand = frame.Hands.Rightmost;
            }

            return secondaryHand;
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

        public void PrimaryHandStateChanged(HandCloseEvent e)
        {
            _isPrimaryHandClosed = e.Type.Equals(HandCloseEvent.OPEN) ? false : true;
            if (_isPrimaryHandClosed)
            {
                _closePosition = e.Position;
            }
            else
            {
                _closePosition = null;
            }
        }

        public void SecondaryHandStateChanged(HandCloseEvent e)
        {
            _isSecondaryHandClosed = e.Type.Equals(HandCloseEvent.OPEN) ? false : true;
        }
    }
}
