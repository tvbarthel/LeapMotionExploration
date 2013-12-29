using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;
using MyLeap.Utils;

namespace MyLeap.Processor
{
    class LeapProcessorHandClosed
    {
        public event Action<Boolean> onHandStateChange;

        private const int STEP_0 = 0;
        private const int STEP_1 = 1;
        private const int STEP_2 = 2;
        private const int STEP_3 = 3;
        private const int STEP_4 = 4;

        private const double HAND_CLOSING_START_Y = -15.0;
        private const double HAND_CLOSING_CONTINUE_Y = -20.0;
        private const double MIN_DISTANCE_FOR_ONE_FINGER = 90.0;

        private int currentStep;

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
                    case STEP_0:
                        handleStepO(hand);
                        break;

                    /**
                     * The hand is fully openned (4 to 5 fingers detected).
                     * To go to the next step, we need to detect fingers going down.
                     */
                    case STEP_1:
                        handleStep1(hand);
                        break;

                    /**
                     * The user has started to close their hand.
                     * To go to the next step, we need to detect fingers going even more down.
                     */
                    case STEP_2:
                        handleStep2(hand);
                        break;

                    /**
                     * The user has continued to close their hand.
                     * To go to the next step, we need to detect no fingers.
                     */
                    case STEP_3:
                        handleStep3(hand);
                        break;

                    /**
                     * The user has closed their hand.
                     * If we detect at least on finger, we go back to the previous step.
                     */
                    case STEP_4:
                        handleStep4(hand);
                        break;
                }
            }
            else
            {
                if (currentStep != STEP_0)
                {
                    currentStep = STEP_0;
                    System.Diagnostics.Debug.WriteLine("No Hands. Get back to the step 0 !");
                    Task.Factory.StartNew(() => onHandStateChange(false));
                }
            }
        }

        private void handleStepO(Hand hand)
        {
            if (hand.Fingers.Count >= 4)
            {
                //An oppenned hand has been detected.
                //Get to the next step
                currentStep = STEP_1;
                System.Diagnostics.Debug.WriteLine("Get to the first step !");
            }
            else if (hand.Fingers.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("The hand has been detected without any fingers!");
                currentStep = STEP_4;
                Task.Factory.StartNew(() => onHandStateChange(true));
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
                    currentStep = STEP_2;
                    System.Diagnostics.Debug.WriteLine("Hand closing starts! Go to step 2 !");
                }
            }
            else
            {
                currentStep = STEP_3;
                System.Diagnostics.Debug.WriteLine("Jump directly to step 3 !");
            }
        }

        private void handleStep2(Hand hand)
        {

            double currentAverageYDistance = computeAverageFingerYDistance(hand);
            if (currentAverageYDistance > HAND_CLOSING_START_Y)
            {
                //The fingers have gone up.
                //Go back to the previous step;
                currentStep = STEP_1;
                System.Diagnostics.Debug.WriteLine("Fingers went up! Go to step 1 !");
            }
            else if (currentAverageYDistance <= HAND_CLOSING_CONTINUE_Y)
            {
                //Fingers have gone even more down
                //The user is closing is hand.
                currentStep = STEP_3;
                System.Diagnostics.Debug.WriteLine("Hand closing continues! Go to step 3");
            }

        }

        private void handleStep3(Hand hand)
        {
            double currentAverageYDistance = computeAverageFingerYDistance(hand);
            if (hand.Fingers.Count >= 3 && currentAverageYDistance > HAND_CLOSING_CONTINUE_Y)
            {
                //The fingers have gone up
                //Go back to the previous step
                currentStep = STEP_2;
                System.Diagnostics.Debug.WriteLine("Fingers went up! Go to step 2 !");
            }
            else if (hand.Fingers.Count == 0)
            {
                //No fingers are detecteds
                //go to the next step
                currentStep = STEP_4;                
                System.Diagnostics.Debug.WriteLine("The hand is now closed! Go to step 4");
                Task.Factory.StartNew(() => onHandStateChange(true));
            }

        }

        private void handleStep4(Hand hand)
        {
            if (hand.Fingers.Count >= 2 || 
                (hand.Fingers.Count == 1 && LeapUtils.computeDistanceBetweenPalmAndFingerTips(hand) >= MIN_DISTANCE_FOR_ONE_FINGER))
            {
                //At least two finger is detected
                //go back to the next step
                currentStep = STEP_3;
                System.Diagnostics.Debug.WriteLine("Go back to step 3");
                System.Diagnostics.Debug.WriteLine("Dist -> " + LeapUtils.computeDistanceBetweenPalmAndFingerTips(hand));
                Task.Factory.StartNew(() => onHandStateChange(false));
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
