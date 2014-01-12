using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;
using System.Windows;
using MyLeap.Event;


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

        private int _currentStep;
        private Hand _rightHand;
        private Hand _leftHand;
        private double _rightDotProduct;
        private double _leftDotProduct;
        private double _handsDist;
        private bool _isClappedIn;

        public event Action<LeapEvent> OnClapDetected;
        public event Action<String> OnStateChange;

        public LeapListenerClap()
        {
            _currentStep = STEP_0;
        }

        public override void OnFrame(Controller controller)
        {
            var frame = controller.Frame();
            var handsNumber = frame.Hands.Count;
            _rightHand = frame.Hands.Rightmost;
            _leftHand = frame.Hands.Leftmost;


            if (_currentStep == STEP_4_HANDS_CLOSE)
            {
                //don't watch for 2 hands, because when hand are closer, sometimes, only one hand is detected
                double distLeftHandToRight = DistanceOfLeftPalmToRightHandPlan();
                double distRightHandToLeft = DistanceOfRightPalmToLeftHandPlan();
                if (distLeftHandToRight > BOUNDARIES_PALM_CLOSED ||
                    distRightHandToLeft > BOUNDARIES_PALM_CLOSED)
                {
                    _currentStep = STEP_3_PALMS_ALIGNED;
                }
                else if (DistanceOfLeftPalmToRightHandPlan() <= BOUNDARIES_CLAP)
                {
                    _currentStep = STEP_5_CLAP_IN;
                }
            }
            else if (_currentStep == STEP_5_CLAP_IN)
            {
                if (!_isClappedIn)
                {
                    //catch clap in
                    _isClappedIn = true;
                    System.Diagnostics.Debug.WriteLine("CLAP IN");
                    Leap.Vector eventPosition = (_rightHand.PalmPosition.Normalized + _leftHand.PalmPosition.Normalized) / 2;
                    FireClapDetected(eventPosition);
                }

                //Clap out condition
                if (DistanceOfLeftPalmToRightHandPlan() > BOUNDARIES_CLAP)
                {
                    System.Diagnostics.Debug.WriteLine("CLAP OUT");
                    _currentStep = STEP_3_PALMS_ALIGNED;
                    _isClappedIn = false;
                }
            }
            else if (handsNumber == 2)
            {
                //two hands
                switch (_currentStep)
                {
                    case STEP_0:
                        //initial state
                        _currentStep = STEP_1_TWO_HANDS;
                        FireStateChange("STEP_1_TWO_HANDS");
                        break;
                    case STEP_1_TWO_HANDS:
                        //two hands are in the frame
                        _rightDotProduct = _rightHand.PalmNormal.Dot(_leftHand.PalmNormal);
                        _leftDotProduct = _leftHand.PalmNormal.Dot(_rightHand.PalmNormal);
                        //dot product of two collinear vetors in opposite direction is -1
                        if (_rightDotProduct < BOUNDARIES_DOT_PRODUCT_IN ||
                            _leftDotProduct < BOUNDARIES_DOT_PRODUCT_IN)
                        {
                            _currentStep = STEP_2_PALMS_COLLINEAR;
                            FireStateChange("STEP_2_PALMS_COLLINEAR");
                        }
                        break;
                    case STEP_2_PALMS_COLLINEAR:
                        //two hands with palms face to face
                        _rightDotProduct = _rightHand.PalmNormal.Dot(_leftHand.PalmNormal);
                        _leftDotProduct = _leftHand.PalmNormal.Dot(_rightHand.PalmNormal);
                        if (_rightDotProduct > BOUNDARIES_DOT_PRODUCT_OUT ||
                            _leftDotProduct > BOUNDARIES_DOT_PRODUCT_OUT)
                        {
                            _currentStep = STEP_1_TWO_HANDS;
                            FireStateChange("STEP_1_TWO_HANDS");
                        }
                        else
                        {
                            if (LeftHandCenterInRightHandPlan() <= BOUNDARIES_DOT_PRODUCT_IN ||
                                RightHandCenterInLeftHandPlan() <= BOUNDARIES_DOT_PRODUCT_IN)
                            {
                                _currentStep = STEP_3_PALMS_ALIGNED;
                                FireStateChange("STEP_3_PALMS_ALIGNED");
                            }
                        }
                        break;
                    case STEP_3_PALMS_ALIGNED:
                        //two hands face to face and align
                        if (LeftHandCenterInRightHandPlan() > BOUNDARIES_PALM_ALIGNED ||
                            RightHandCenterInLeftHandPlan() > BOUNDARIES_PALM_ALIGNED)
                        {
                            _currentStep = STEP_2_PALMS_COLLINEAR;
                            FireStateChange("STEP_2_PALMS_COLLINEAR");
                        }
                        else
                        {
                            _handsDist = _rightHand.PalmPosition.DistanceTo(_leftHand.PalmPosition);
                            if (_handsDist <= BOUNDARIES_PALM_CLOSED)
                            {
                                _currentStep = STEP_4_HANDS_CLOSE;
                                FireStateChange("STEP_4_HANDS_CLOSE");
                            }
                        }

                        break;
                }

            }
            else
            {
                //return at the first step
                if (_currentStep != STEP_0)
                {
                    _currentStep = STEP_0;
                    FireStateChange("STEP_0");
                }
            }
        }

        private void FireClapDetected(Leap.Vector eventPosition)
        {
            if (OnClapDetected != null)
            {
                Task.Factory.StartNew(() => OnClapDetected(new LeapEvent(eventPosition)));
            }
        }

        private void FireStateChange(String state)
        {
            if (OnStateChange != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Task.Factory.StartNew(() => OnStateChange(state));
                }));
            }
        }

        private Double RightHandCenterInLeftHandPlan()
        {
            //retrieve the normal vector of the right hand plan
            var lNorm = _leftHand.PalmNormal.Cross(_leftHand.Direction).Normalized;
            //calculate the d factor for the plan equation
            var lNomD = _leftHand.PalmPosition.Dot(lNorm);

            var rPosNorm = _rightHand.PalmPosition;

            //check if the center of the left palm is in the right hand plan
            //plan equation ax + by + cz - d = 0
            return lNorm.x * rPosNorm.x + lNorm.y * rPosNorm.y + lNorm.z * rPosNorm.z - lNomD;
        }

        private Double LeftHandCenterInRightHandPlan()
        {
            //retrieve the normal vector of the right hand plan
            var rNorm = _rightHand.PalmNormal.Cross(_rightHand.Direction).Normalized;
            //calculate the d factor for the plan equation
            var rNomD = _rightHand.PalmPosition.Dot(rNorm);

            var lPosNorm = _leftHand.PalmPosition;

            //check if the center of the left palm is in the right hand plan
            //plan equation ax + by + cz - d = 0
            return rNorm.x * lPosNorm.x + rNorm.y * lPosNorm.y + rNorm.z * lPosNorm.z - rNomD;
        }

        private Double DistanceOfLeftPalmToRightHandPlan()
        {
            //return the distance of the left palm center to the right hand plan
            //using |ax + by + cz - d| / sqrt(a² + b² + c²)

            //normal vector to the right hand
            var rPlanNorm = _rightHand.PalmNormal;
            //calculate the d factor for the plan equation
            var rPlanD = _rightHand.PalmPosition.Dot(rPlanNorm);

            //left palm center
            var lPalmCenter = _leftHand.PalmPosition;

            var num = rPlanNorm.x * lPalmCenter.x + rPlanNorm.y * lPalmCenter.y + rPlanNorm.z * lPalmCenter.z - rPlanD;
            var denum = rPlanNorm.x * rPlanNorm.x + rPlanNorm.y * rPlanNorm.y + rPlanNorm.z * rPlanNorm.z;

            return Math.Abs(num) / Math.Sqrt(denum);

        }

        private Double DistanceOfRightPalmToLeftHandPlan()
        {
            //return the distance of the left palm center to the right hand plan
            //using |ax + by + cz - d| / sqrt(a² + b² + c²)

            //normal vector to the left hand
            var lPlanNorm = _leftHand.PalmNormal;
            //calculate the d factor for the plan equation
            var lPlanD = _leftHand.PalmPosition.Dot(lPlanNorm);

            //right palm center
            var rPalmCenter = _rightHand.PalmPosition;

            var num = lPlanNorm.x * rPalmCenter.x + lPlanNorm.y * rPalmCenter.y + lPlanNorm.z * rPalmCenter.z - lPlanD;
            var denum = lPlanNorm.x * lPlanNorm.x + lPlanNorm.y * lPlanNorm.y + lPlanNorm.z * lPlanNorm.z;

            return Math.Abs(num) / Math.Sqrt(denum);

        }
    }
}
