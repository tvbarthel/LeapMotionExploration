using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLeap.Event
{
    class HandCloseEvent : LeapEvent
    {
        public const int CLOSE = 0;
        public const int OPEN = 1;

        public HandCloseEvent(Leap.Vector eventPosition, Boolean isClose) : base(eventPosition)
        {
            Type = isClose ? CLOSE : OPEN;
        }
    }
}
