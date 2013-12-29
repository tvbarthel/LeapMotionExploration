using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;
using MyLeap.Utils;
using MyLeap.Event;

namespace MyLeap.Processor
{
    class LeapProcessorHandClosed
    {
        public event Action<HandCloseEvent> onHandStateChange;
        public event Action<int> onStateChange;

        private const int STEP_0_UNKNOW = 0;
        private const int STEP_1_FULLY_OPENED = 1;
        private const int STEP_2_HALF_OPENED = 2;
        private const int STEP_3_NEARLY_CLOSED = 3;
        private const int STEP_4_CLOSED = 4;

        private const double HAND_CLOSING_START_Y = -15.0;
        private const double HAND_CLOSING_CONTINUE_Y = -20.0;
        private const double MIN_DISTANCE_FOR_ONE_FINGER = 90.0;

        private int currentStep;
        private Leap.Vector eventPosition;

        public LeapProcessorHandClosed()
        {
            currentStep = 0;
        }

        public void process(Hand hand)
        {
            if (hand.IsValid)
            {
                switch (currentStep)
                {
                    /**
                     * The hand is in a unknown state
                     * To go to the next step, we need a full openned hand, 
                     * that means at least 4 fingers detected.
                     */
                    case STEP_0_UNKNOW:
                        handleStepO(hand);
                        break;

                    /**
                     * The hand is fully openned (4 to 5 fingers detected).
                     * To go to the next step, we need to detect fingers going down.
                     */
                    case STEP_1_FULLY_OPENED:
                        eventPosition = hand.Fingers.Frontmost.StabilizedTipPosition.Normalized;
                        handleStep1(hand);
                        break;

                    /**
                     * The user has started to close their hand.
                     * To go to the next step, we need to detect fingers going even more down.
                     */
                    case STEP_2_HALF_OPENED:
                        handleStep2(hand);
                        break;

                    /**
                     * The user has continued to close their hand.
                     * To go to the next step, we need to detect no fingers.
                     */
                    case STEP_3_NEARLY_CLOSED:
                        handleStep3(hand);
                        break;

                    /**
                     * The user has closed their hand.
                     * If we detect at least on finger, we go back to the previous step.
                     */
                    case STEP_4_CLOSED:
                        handleStep4(hand);
                        break;
                }
            }
            else
            {
                if (currentStep != STEP_0_UNKNOW)
                {
                    currentStep = STEP_0_UNKNOW;
                    System.Diagnostics.Debug.WriteLine("No Hands. Get back to the step 0 !");
                    Task.Factory.StartNew(() => onHandStateChange(new HandCloseEvent(eventPosition,false)));
                }
            }
        }

        private void handleStepO(Hand hand)
        {
            if (hand.Fingers.Count >= 4)
            {
                //An oppenned hand has been detected.
                //Get to the next step
                currentStep = STEP_1_FULLY_OPENED;
                System.Diagnostics.Debug.WriteLine("Get to the first step !");
                Task.Factory.StartNew(() => onStateChange(STEP_1_FULLY_OPENED));
            }
            else if (hand.Fingers.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("The hand has been detected without any fingers!");
                currentStep = STEP_4_CLOSED;
                Task.Factory.StartNew(() => onHandStateChange(new HandCloseEvent(eventPosition,true)));
                Task.Factory.StartNew(() => onStateChange(STEP_4_CLOSED));
            }
        }

        private void handleStep1(Hand hand)
        {
            //Compute the current average Y distance
            if (hand.Fingers.Count >= 4)
            {
                double currentAverageYDistance = computeAverageFingerYDistance(hand);
                if (currentAverageYDistance <= HAND_CLOSING_START_Y)
                {
                    currentStep = STEP_2_HALF_OPENED;
                    System.Diagnostics.Debug.WriteLine("Hand closing starts! Go to step 2 !");
                    Task.Factory.StartNew(() => onStateChange(STEP_2_HALF_OPENED));
                }
            }
            else
            {
                currentStep = STEP_3_NEARLY_CLOSED;
                System.Diagnostics.Debug.WriteLine("Jump directly to step 3 !");
                Task.Factory.StartNew(() => onStateChange(STEP_3_NEARLY_CLOSED));
            }
        }

        private void handleStep2(Hand hand)
        {

            double currentAverageYDistance = computeAverageFingerYDistance(hand);
            if (currentAverageYDistance > HAND_CLOSING_START_Y)
            {
                //The fingers have gone up.
                //Go back to the previous step;
                currentStep = STEP_1_FULLY_OPENED;
                System.Diagnostics.Debug.WriteLine("Fingers went up! Go to step 1 !");
                Task.Factory.StartNew(() => onStateChange(STEP_1_FULLY_OPENED));
            }
            else if (currentAverageYDistance <= HAND_CLOSING_CONTINUE_Y)
            {
                //Fingers have gone even more down
                //The user is closing is hand.
                currentStep = STEP_3_NEARLY_CLOSED;
                System.Diagnostics.Debug.WriteLine("Hand closing continues! Go to step 3");
                Task.Factory.StartNew(() => onStateChange(STEP_3_NEARLY_CLOSED));
            }

        }

        private void handleStep3(Hand hand)
        {
            double currentAverageYDistance = computeAverageFingerYDistance(hand);
            if (hand.Fingers.Count >= 3 && currentAverageYDistance > HAND_CLOSING_CONTINUE_Y)
            {
                //The fingers have gone up
                //Go back to the previous step
                currentStep = STEP_2_HALF_OPENED;
                System.Diagnostics.Debug.WriteLine("Fingers went up! Go to step 2 !");
                Task.Factory.StartNew(() => onStateChange(STEP_2_HALF_OPENED));
            }
            else if (hand.Fingers.Count == 0)
            {
                //No fingers are detecteds
                //go to the next step
                currentStep = STEP_4_CLOSED;                
                System.Diagnostics.Debug.WriteLine("The hand is now closed! Go to step 4");
                Task.Factory.StartNew(() => onHandStateChange(new HandCloseEvent(eventPosition, true)));
                Task.Factory.StartNew(() => onStateChange(STEP_4_CLOSED));
            }

        }

        private void handleStep4(Hand hand)
        {
            if (hand.Fingers.Count >= 2 || 
                (hand.Fingers.Count == 1 && LeapUtils.computeDistanceBetweenPalmAndFingerTips(hand) >= MIN_DISTANCE_FOR_ONE_FINGER))
            {
                //At least two finger is detected
                //go back to the next step
                currentStep = STEP_3_NEARLY_CLOSED;
                System.Diagnostics.Debug.WriteLine("Go back to step 3");
                System.Diagnostics.Debug.WriteLine("Dist -> " + LeapUtils.computeDistanceBetweenPalmAndFingerTips(hand));
                Task.Factory.StartNew(() => onHandStateChange(new HandCloseEvent(eventPosition, false)));
            }
        }



        private double computeAverageFingerYDistance(Hand hand)
        {
            double res = 0;
            foreach (Finger finger in hand.Fingers)
            {
                res += finger.TipPosition.y - hand.PalmPosition.y; // LeapUtils.computeDistance(finger.TipPosition, hand.PalmPosition);
            }
            res /= hand.Fingers.Count;
            return res;
        }
    }
}
