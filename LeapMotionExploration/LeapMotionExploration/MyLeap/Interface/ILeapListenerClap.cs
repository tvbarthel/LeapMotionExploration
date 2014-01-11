using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyLeap.Event;

namespace MyLeap.Interface
{
    /**
     * interface used to catch clap event
     */
    public class ILeapListenerClap
    {

        private Action<LeapEvent> _onClapeDetected;
        private Action<String> _onStateChange;

        public ILeapListenerClap(Action<LeapEvent> clapDetection)
            : this(clapDetection, null)
        {

        }

        public ILeapListenerClap(Action<LeapEvent> clapDetection, Action<String> stateChange)
        {
            _onClapeDetected = clapDetection;
            _onStateChange = stateChange;
        }

        /**
         * fired when clap happened
         */
        public void OnClapDetected(LeapEvent e)
        {
            if (_onClapeDetected != null) _onClapeDetected(e);
        }

        /**
         * fired each time state of the gesture change
         */
        public void OnClapStateChange(String state)
        {
            if (_onStateChange != null) _onStateChange(state);
        }
    }
}
