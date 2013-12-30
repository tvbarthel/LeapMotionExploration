﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLeap.Event
{
    class LeapEvent
    {
        public const int NONE = 0;

        //Rotation selection
        public const int ROTATION_SELECTION_START = 1;
        public const int ROTATION_SELECTION_NEXT = 2;
        public const int ROTATION_SELECTION_PREVIOUS = 3;
        public const int ROTATION_SELECTION_END = 4;

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
