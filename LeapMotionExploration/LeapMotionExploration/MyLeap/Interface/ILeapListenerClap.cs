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
    interface ILeapListenerClap
    {
        /**
         * fired when clap happened
         */ 
        void OnClapDetected(LeapEvent e);

        /**
         * fired each time state of the gesture change
         */ 
        void OnClapStateChange(String state);
    }
}
