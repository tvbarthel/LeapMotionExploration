using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLeap.Event
{
    class LeapEvent
    {
        public const int NONE = 0;

        public Leap.Vector Position;
        public int Type;

        public LeapEvent(Leap.Vector occuredPosition)
        {
            Position = occuredPosition;
            Type = NONE;
        }
    }
}
