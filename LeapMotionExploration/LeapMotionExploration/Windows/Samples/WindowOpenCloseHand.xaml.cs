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
    /// Logique d'interaction pour WindowOpenCloseHand.xaml
    /// </summary>
    public partial class WindowOpenCloseHand : Window
    {
        private Controller mController;
        private LeapListenerOneHandClose mListener;

        public WindowOpenCloseHand()
        {
            InitializeComponent();

            mController = new Controller();

            mListener = new LeapListenerOneHandClose();
            mController.AddListener(mListener);

            mListener.OnHandStateChange += this.onHandClosed;


        }

        private void onHandClosed(HandCloseEvent e)
        {
            String message = null;

            switch (e.Type)
            {
                case HandCloseEvent.OPEN:
                    message = "You have just openned your hand!";
                    System.Diagnostics.Debug.WriteLine("hand open");
                    break;
                case HandCloseEvent.CLOSE:
                    message = "You have just closed your hand!";
                    System.Diagnostics.Debug.WriteLine("hand close");
                    break;
            }

            if (message != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    textBlockMessage.Text = message;
                }));
            }            
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            mController.RemoveListener(mListener);
            mController.Dispose();
            base.OnClosing(e);
        }
    }
}
