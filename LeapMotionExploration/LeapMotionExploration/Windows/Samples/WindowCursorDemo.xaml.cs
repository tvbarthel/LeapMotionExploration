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
using MyLeap.Utils;

namespace LeapMotionExploration.Windows.Samples
{
    /// <summary>
    /// Logique d'interaction pour WindowLeftMostCursor.xaml
    /// </summary>
    public partial class WindowCursorDemo : Window
    {

        private Controller mController;
        private LeapListenerOneHandPosition mListener;

        private const String COLOR_RED = "#FF4444";
        private const String COLOR_BLUE = "#33B5E5";

        public WindowCursorDemo() : this(LeapUtils.RIGHT_MOST_HAND)
        {
        }

        public WindowCursorDemo(int handPreference)
        {
            InitializeComponent();
            mController = new Controller();

            mListener = new LeapListenerOneHandPosition(handPreference);
            mController.AddListener(mListener);

            mListener.OnPositionRecieve += this.OnNormalizedPositionRecieve;

            if (handPreference == LeapUtils.RIGHT_MOST_HAND)
            {
                applyColor(COLOR_BLUE);
            }
            else if (handPreference == LeapUtils.LEFT_MOST_HAND)
            {
                applyColor(COLOR_RED);
            }
            
        }

        private void OnNormalizedPositionRecieve(Leap.Vector normalizedPosition)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var cursorSize = 10 + 60 * normalizedPosition.z;
                leapCursor.Width = cursorSize;
                leapCursor.Height = cursorSize;

                var leftMargin = cursorContainer.ActualWidth * normalizedPosition.x - cursorSize / 2;
                var topMargin = cursorContainer.ActualHeight * normalizedPosition.y - cursorSize / 2;
                leapCursor.Margin = new Thickness(leftMargin, 0, 0, topMargin);
            }));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            mController.RemoveListener(mListener);
            mController.Dispose();
            base.OnClosing(e);
        }

        private void applyColor(String colorStr)
        {
            Color color = (Color)ColorConverter.ConvertFromString(colorStr);
            textBlockApplicationTitle.Foreground = new SolidColorBrush(color);
            color.A = 60;
            leapCursor.Fill = new SolidColorBrush(color);
        }
    }
}
