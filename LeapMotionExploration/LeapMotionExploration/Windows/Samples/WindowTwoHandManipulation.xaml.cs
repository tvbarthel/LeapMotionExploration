using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Leap;
using MyLeap.Listener;
using MyLeap.Event;

namespace LeapMotionExploration
{
    /// <summary>
    /// Logique d'interaction pour WindowTwoHandManipulation.xaml
    /// </summary>
    public partial class WindowTwoHandManipulation : Window
    {
        private Controller mController;
        private LeapListenerTwoHandManipulation mListener;
        private double currentRotation;
        private double currentSize;

        public WindowTwoHandManipulation()
        {
            InitializeComponent();

            mController = new Controller();
            mListener = new LeapListenerTwoHandManipulation();
            mController.AddListener(mListener);
            mListener.OnStateChange += OnStateChange;

            currentRotation = 0;
            setRotation();

            currentSize = 100;
            setSize();
        }

        public void OnStateChange(LeapEvent leapEvent)
        {
            switch (leapEvent.Type)
            {
                case LeapEvent.TRANSFORMATION_ROTATE_CLOCKWIZE:
                    currentRotation += 3;
                    setRotation();
                    break;

                case LeapEvent.TRANSFORMATION_ROTATE_UNCLOCKWIZE:
                    currentRotation -= 3;
                    setRotation();
                    break;
                case LeapEvent.TRANSFORMATION_SIZE_UP:
                    currentSize += 3;
                    setSize();
                    break;
                case LeapEvent.TRANSFORMATION_SIZE_DOWN:
                    currentSize -= 3;
                    setSize();
                    break;
            }            
        }

        private void setSize()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                rectangle.Width = (int)currentSize;
                rectangle.Height = (int)currentSize;
            }));
        }

        private void setRotation()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                rectangle.RenderTransform = new RotateTransform(currentRotation);
            }));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            mController.RemoveListener(mListener);
            mController.Dispose();
            base.OnClosing(e);
        }
    }
}
