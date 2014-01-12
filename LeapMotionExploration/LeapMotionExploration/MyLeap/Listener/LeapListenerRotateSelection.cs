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

        private LeapProcessorHandClosed _processorLeftHandClosed;
        private LeapProcessorTwoHandRoll _processorHandRoll;
        private Vector _closePosition;
        private Boolean _isMainHandClosed;
        private int _mainHand;
        private float _lastRoll;

        public LeapListenerRotateSelection(int mainHand)
        {
            _isMainHandClosed = false;

            _mainHand = mainHand;

            _processorLeftHandClosed = new LeapProcessorHandClosed();
            _processorLeftHandClosed.OnHandStateChange += MainHandStateChanged;
            _processorLeftHandClosed.OnStateChange += (x) => { };

            _processorHandRoll = new LeapProcessorTwoHandRoll();
            _processorHandRoll.OnAngleChanged += RollAngleChanged;

            _lastRoll = 0;
        }

        public LeapListenerRotateSelection() : this(LeapUtils.RIGHT_MOST_HAND)
        {            
        }

        public override void OnFrame(Controller controller)
        {
            var frame = controller.Frame();
            if (frame.IsValid)
            {
                _processorLeftHandClosed.Process(GetMainHand(frame));
                
                if (_isMainHandClosed && frame.Hands.Count == 2)
                {
                    _processorHandRoll.Process(frame.Hands.Leftmost, frame.Hands.Rightmost);
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

        public void RollAngleChanged(float newRoll)
        {
            if (_lastRoll != 0)
            {
                if (newRoll > _lastRoll)
                {
                    System.Diagnostics.Debug.WriteLine("Select Next !");
                    Task.Factory.StartNew(() => OnStateChange(new LeapEvent(_closePosition, LeapEvent.ROTATION_SELECTION_NEXT)));
                    
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Select Previous !");
                    Task.Factory.StartNew(() => OnStateChange(new LeapEvent(_closePosition, LeapEvent.ROTATION_SELECTION_PREVIOUS)));
                }
            }
            _lastRoll = newRoll;
        }

        public void MainHandStateChanged(HandCloseEvent handClose)
        {
            int type = handClose.Type;
            switch (type)
            {
                case HandCloseEvent.CLOSE:
                    System.Diagnostics.Debug.WriteLine("Selection Started !");
                    _isMainHandClosed = true;
                    _closePosition = handClose.Position;
                    _lastRoll = 0f;
                    Task.Factory.StartNew(() => OnStateChange(new LeapEvent(_closePosition, LeapEvent.ROTATION_SELECTION_START)));
                    break;
                case HandCloseEvent.OPEN:
                    System.Diagnostics.Debug.WriteLine("Selection Ended !");
                    _isMainHandClosed = false;                    
                    Task.Factory.StartNew(() => OnStateChange(new LeapEvent(_closePosition, LeapEvent.ROTATION_SELECTION_END)));
                    _closePosition = null;
                    break;
            }
        }
    }
}
