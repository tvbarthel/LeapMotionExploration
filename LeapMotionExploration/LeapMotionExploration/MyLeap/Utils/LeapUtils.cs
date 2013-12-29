using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;

namespace MyLeap.Utils
{
    class LeapUtils
    {

        public const int RIGHT_MOST_HAND = 0;
        public const int LEFT_MOST_HAND = 1;
        
        public static double computeDistance(Leap.Vector vect1, Leap.Vector vect2)
        {
            return Math.Sqrt(Math.Pow(vect1.x - vect2.x, 2) + Math.Pow(vect1.y - vect2.y, 2) + Math.Pow(vect1.z - vect2.z, 2));
        }

        public static double computeDistanceBetweenPalmAndFingerTips(Hand hand)
        {
            double res = 0;
            foreach (Finger finger in hand.Fingers)
            {
                res += computeDistance(finger.TipPosition, hand.PalmPosition);
            }
            res /= hand.Fingers.Count;
            return res;
        }

    }
}
