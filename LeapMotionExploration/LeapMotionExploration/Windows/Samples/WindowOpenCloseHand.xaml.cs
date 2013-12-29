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

namespace LeapMotionExploration
{
    /// <summary>
    /// Logique d'interaction pour WindowOpenCloseHand.xaml
    /// </summary>
    public partial class WindowOpenCloseHand : Window
    {
        private Controller mController;
        private LeapListenerCloseLeftHand mListener;

        public WindowOpenCloseHand()
        {
            InitializeComponent();

            mController = new Controller();

            mListener = new LeapListenerCloseLeftHand();
            mController.AddListener(mListener);

            mListener.OnHandStateChanged += this.onHandClosed;


        }

        private void onHandClosed(string message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                textBlockMessage.Text = message;
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
