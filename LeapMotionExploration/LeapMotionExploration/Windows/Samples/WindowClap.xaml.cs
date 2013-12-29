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
    public partial class WindowClap : Window
    {
        private Controller myController;
        private LeapListenerClap myListener;
        private int myCounter;

        public WindowClap()
        {
            InitializeComponent();
            myController = new Controller();
            myListener = new LeapListenerClap();
            myCounter = 0;

            myController.AddListener(myListener);

            myListener.OnClapDetected += this.OnClapReceive;
            myListener.OnStateChange += this.OnClapStateChange;
        }

        private void OnClapStateChange(String state)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                listenerState.Text = state;
            }));
        }

        private void OnClapReceive(Leap.Vector eventPosition)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                counter.Text = Convert.ToString(++myCounter);
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
