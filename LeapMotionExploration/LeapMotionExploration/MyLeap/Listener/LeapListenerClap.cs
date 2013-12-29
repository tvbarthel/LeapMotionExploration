using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;
using System.Windows;


namespace MyLeap.Listener
{
    class LeapListenerClap : Leap.Listener
    {
        //step
        private const int STEP_0 = 0x00000000;
        private const int STEP_1_TWO_HANDS = 0x00000001;
        private const int STEP_2_PALMS_COLLINEAR = 0x00000002;
        private const int STEP_3_PALMS_ALIGNED = 0x000000003;
        private const int STEP_4_HANDS_CLOSE = 0x000000004;
        private const int STEP_5_CLAP_IN = 0x000000005;

        //boundaries
        private const double BOUNDARIES_DOT_PRODUCT_IN = -0.7;
        private const double BOUNDARIES_DOT_PRODUCT_OUT = -0.8;
        private const double BOUNDARIES_PALM_ALIGNED = 100;
        private const double BOUNDARIES_PALM_CLOSED = 100;
        private const double BOUNDARIES_CLAP = 45;

        private int mCurrentStep;
        private Hand mRightHand;
        private Hand mLeftHand;
        private double mRightDotProduct;
        private double mLeftDotProduct;
        private double mHandsDist;
        private bool mIsClappedIn;

        public event Action<LeapEvent> OnClapDetected;
        public event Action<String> OnStateChange;

        public LeapListenerClap()
        {
            mCurrentStep = STEP_0;
        }

        public override void OnFrame(Controller controller)
        {
            var frame = controller.Frame();
            var handsNumber = frame.Hands.Count;
            mRightHand = frame.Hands.Rightmost;
            mLeftHand = frame.Hands.Leftmost;


            if (mCurrentStep == STEP_4_HANDS_CLOSE)
            {
                //don't watch for 2 hands, because when hand are closer, sometimes, only one hand is detected
                double distLeftHandToRight = DistanceOfLeftPalmToRightHandPlan();
                double distRightHandToLeft = DistanceOfRightPalmToLeftHandPlan();
                if (distLeftHandToRight > BOUNDARIES_PALM_CLOSED ||
                    distRightHandToLeft > BOUNDARIES_PALM_CLOSED)
                {
                    mCurrentStep = STEP_3_PALMS_ALIGNED;
                }
                else if (DistanceOfLeftPalmToRightHandPlan() <= BOUNDARIES_CLAP)
                {
                    mCurrentStep = STEP_5_CLAP_IN;
                }
            }
            else if (mCurrentStep == STEP_5_CLAP_IN)
            {
                if (!mIsClappedIn)
                {
                    //catch clap in
                    mIsClappedIn = true;
                    System.Diagnostics.Debug.WriteLine("CLAP IN");
                    Leap.Vector eventPosition = (mRightHand.PalmPosition.Normalized + mLeftHand.PalmPosition.Normalized) / 2;
                    Task.Factory.StartNew(() => OnClapDetected(new LeapEvent(eventPosition)));
                }

                //Clap out condition
                if (DistanceOfLeftPalmToRightHandPlan() > BOUNDARIES_CLAP)
                {
                    System.Diagnostics.Debug.WriteLine("CLAP OUT");
                    mCurrentStep = STEP_3_PALMS_ALIGNED;
                    mIsClappedIn = false;
                }
            }
            else if (handsNumber == 2)
            {
                //two hands
                switch (mCurrentStep)
                {
                    case STEP_0:
                        //initial state
                        mCurrentStep = STEP_1_TWO_HANDS;
                        fireStateChange("STEP_1_TWO_HANDS");
                        break;
                    case STEP_1_TWO_HANDS:
                        //two hands are in the frame
                        mRightDotProduct = mRightHand.PalmNormal.Dot(mLeftHand.PalmNormal);
                        mLeftDotProduct = mLeftHand.PalmNormal.Dot(mRightHand.PalmNormal);
                        //dot product of two collinear vetors in opposite direction is -1
                        if (mRightDotProduct < BOUNDARIES_DOT_PRODUCT_IN ||
                            mLeftDotProduct < BOUNDARIES_DOT_PRODUCT_IN)
                        {
                            mCurrentStep = STEP_2_PALMS_COLLINEAR;
                            fireStateChange("STEP_2_PALMS_COLLINEAR");
                        }
                        break;
                    case STEP_2_PALMS_COLLINEAR:
                        //two hands with palms face to face
                        mRightDotProduct = mRightHand.PalmNormal.Dot(mLeftHand.PalmNormal);
                        mLeftDotProduct = mLeftHand.PalmNormal.Dot(mRightHand.PalmNormal);
                        if (mRightDotProduct > BOUNDARIES_DOT_PRODUCT_OUT ||
                            mLeftDotProduct > BOUNDARIES_DOT_PRODUCT_OUT)
                        {
                            mCurrentStep = STEP_1_TWO_HANDS;
                            fireStateChange("STEP_1_TWO_HANDS");
                        }
                        else
                        {
                            if (LeftHandCenterInRightHandPlan() <= BOUNDARIES_DOT_PRODUCT_IN ||
                                RightHandCenterInLeftHandPlan() <= BOUNDARIES_DOT_PRODUCT_IN)
                            {
                                mCurrentStep = STEP_3_PALMS_ALIGNED;
                                fireStateChange("STEP_3_PALMS_ALIGNED");
                            }
                        }
                        break;
                    case STEP_3_PALMS_ALIGNED:
                        //two hands face to face and align
                        if (LeftHandCenterInRightHandPlan() > BOUNDARIES_PALM_ALIGNED ||
                            RightHandCenterInLeftHandPlan() > BOUNDARIES_PALM_ALIGNED)
                        {
                            mCurrentStep = STEP_2_PALMS_COLLINEAR;
                            fireStateChange("STEP_2_PALMS_COLLINEAR");
                        }
                        else
                        {
                            mHandsDist = mRightHand.PalmPosition.DistanceTo(mLeftHand.PalmPosition);
                            if (mHandsDist <= BOUNDARIES_PALM_CLOSED)
                            {
                                mCurrentStep = STEP_4_HANDS_CLOSE;
                                fireStateChange("STEP_4_HANDS_CLOSE");
                            }
                        }

                        break;
                }

            }
            else
            {
                //return at the first step
                if (mCurrentStep != STEP_0)
                {
                    mCurrentStep = STEP_0;
                    fireStateChange("STEP_0");
                }
            }
        }

        private void fireStateChange(String state)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Task.Factory.StartNew(() => OnStateChange(state));
            }));
        }

        private Double RightHandCenterInLeftHandPlan()
        {
            //retrieve the normal vector of the right hand plan
            var lNorm = mLeftHand.PalmNormal.Cross(mLeftHand.Direction).Normalized;
            //calculate the d factor for the plan equation
            var lNomD = mLeftHand.PalmPosition.Dot(lNorm);

            var rPosNorm = mRightHand.PalmPosition;

            //check if the center of the left palm is in the right hand plan
            //plan equation ax + by + cz - d = 0
            return lNorm.x * rPosNorm.x + lNorm.y * rPosNorm.y + lNorm.z * rPosNorm.z - lNomD;
        }

        private Double LeftHandCenterInRightHandPlan()
        {
            //retrieve the normal vector of the right hand plan
            var rNorm = mRightHand.PalmNormal.Cross(mRightHand.Direction).Normalized;
            //calculate the d factor for the plan equation
            var rNomD = mRightHand.PalmPosition.Dot(rNorm);

            var lPosNorm = mLeftHand.PalmPosition;

            //check if the center of the left palm is in the right hand plan
            //plan equation ax + by + cz - d = 0
            return rNorm.x * lPosNorm.x + rNorm.y * lPosNorm.y + rNorm.z * lPosNorm.z - rNomD;
        }

        private Double DistanceOfLeftPalmToRightHandPlan()
        {
            //return the distance of the left palm center to the right hand plan
            //using |ax + by + cz - d| / sqrt(a² + b² + c²)

            //normal vector to the right hand
            var rPlanNorm = mRightHand.PalmNormal;
            //calculate the d factor for the plan equation
            var rPlanD = mRightHand.PalmPosition.Dot(rPlanNorm);

            //left palm center
            var lPalmCenter = mLeftHand.PalmPosition;

            var num = rPlanNorm.x * lPalmCenter.x + rPlanNorm.y * lPalmCenter.y + rPlanNorm.z * lPalmCenter.z - rPlanD;
            var denum = rPlanNorm.x * rPlanNorm.x + rPlanNorm.y * rPlanNorm.y + rPlanNorm.z * rPlanNorm.z;

            return Math.Abs(num) / Math.Sqrt(denum);

        }

        private Double DistanceOfRightPalmToLeftHandPlan()
        {
            //return the distance of the left palm center to the right hand plan
            //using |ax + by + cz - d| / sqrt(a² + b² + c²)

            //normal vector to the left hand
            var lPlanNorm = mLeftHand.PalmNormal;
            //calculate the d factor for the plan equation
            var lPlanD = mLeftHand.PalmPosition.Dot(lPlanNorm);

            //right palm center
            var rPalmCenter = mRightHand.PalmPosition;

            var num = lPlanNorm.x * rPalmCenter.x + lPlanNorm.y * rPalmCenter.y + lPlanNorm.z * rPalmCenter.z - lPlanD;
            var denum = lPlanNorm.x * lPlanNorm.x + lPlanNorm.y * lPlanNorm.y + lPlanNorm.z * lPlanNorm.z;

            return Math.Abs(num) / Math.Sqrt(denum);

        }
    }
}
