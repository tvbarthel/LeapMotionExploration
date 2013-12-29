using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLeap.Listener
{
    class LeapEvent
    {
        public Leap.Vector Position;

        public LeapEvent(Leap.Vector occuredPosition)
        {
            Position = occuredPosition;
        }
    }
}
