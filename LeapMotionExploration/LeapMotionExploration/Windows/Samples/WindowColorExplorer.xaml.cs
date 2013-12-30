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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Leap;
using MyLeap.Listener;
using MyLeap.Event;

namespace LeapMotionExploration
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class WindowColorExplorer : Window
    {

        private Controller mController;
        private LeapListenerOneHandPosition mListener;

        public WindowColorExplorer()
        {
            InitializeComponent();
            mController = new Controller();

            mListener = new LeapListenerOneHandPosition();
            mController.AddListener(mListener);

            mListener.OnStateChange += this.OnNormalizedPositionRecieve;

        }

        private void OnNormalizedPositionRecieve(LeapEvent leapEvent)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Leap.Vector normalizedPosition = leapEvent.Position;
                byte redValue = (byte)(255 * normalizedPosition.x);
                byte greenValue = (byte)(255 * normalizedPosition.y);
                byte blueValue = (byte)(255 * normalizedPosition.z);
                mainWindow.Background = new SolidColorBrush(Color.FromArgb(255, redValue, greenValue, blueValue));
                rectangleRedValue.Fill = new SolidColorBrush(Color.FromArgb(255, redValue, 0, 0));
                rectangleGreenValue.Fill = new SolidColorBrush(Color.FromArgb(255, 0, greenValue, 0));
                rectangleBlueValue.Fill = new SolidColorBrush(Color.FromArgb(255, 0, 0, blueValue));
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
