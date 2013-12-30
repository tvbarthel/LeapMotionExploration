using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLeap.Event
{
    public class LeapEvent
    {
        public const int NONE = 0;

        //Rotation selection
        public const int ROTATION_SELECTION_START = 1;
        public const int ROTATION_SELECTION_NEXT = 2;
        public const int ROTATION_SELECTION_PREVIOUS = 3;
        public const int ROTATION_SELECTION_END = 4;

        //Transformation
        public const int TRANSFORMATION_SIZE_UP = 5;
        public const int TRANSFORMATION_SIZE_DOWN = 6;
        public const int TRANSFORMATION_ROTATE_CLOCKWIZE = 7;
        public const int TRANSFORMATION_ROTATE_UNCLOCKWIZE = 8;

        public Leap.Vector Position;
        public int Type;

        public LeapEvent(Leap.Vector occuredPosition)
        {
            Position = occuredPosition;
            Type = NONE;
        }

        public LeapEvent(Leap.Vector position, int type)
        {
            Position = position;
            Type = type;
        }
    }
}
