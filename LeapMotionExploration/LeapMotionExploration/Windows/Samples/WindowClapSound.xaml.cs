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

namespace LeapMotionExploration.Windows.Samples
{
    /// <summary>
    /// Logique d'interaction pour WindowClap.xaml
    /// </summary>
    public partial class WindowClapSound : Window
    {
        private Controller myController;
        private LeapListenerClap myListener;

        public WindowClapSound()
        {
            InitializeComponent();
            myController = new Controller();
            myListener = new LeapListenerClap();

            myController.AddListener(myListener);

            myListener.OnClapDetected += this.OnClapReceive;
            myListener.OnStateChange += this.OnClapStateChange;
        }

        private void OnClapStateChange(String state)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                right.Visibility = System.Windows.Visibility.Hidden;
                left.Visibility = System.Windows.Visibility.Hidden;
            }));
        }


        private void OnClapReceive(LeapEvent clapEvent)
        {
            Leap.Vector eventPosition = clapEvent.Position;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (eventPosition.x > 0)
                {
                    new System.Media.SoundPlayer(@"./drum3.wav").Play();
                    right.Visibility = System.Windows.Visibility.Visible;
                    left.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    new System.Media.SoundPlayer(@"./drum2.wav").Play();
                    right.Visibility = System.Windows.Visibility.Hidden;
                    left.Visibility = System.Windows.Visibility.Visible;
                }
            }));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            myController.RemoveListener(myListener);
            myController.Dispose();
            base.OnClosing(e);
        }
    }
}
