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
using MyLeap.Dispatcher;
using MyLeap.Interface;
using MyLeap.Event;
using MyLeap.Utils;

namespace LeapMotionExploration.Windows.Samples
{
    /// <summary>
    /// Logique d'interaction pour WindowDispatcher.xaml
    /// </summary>
    public partial class WindowDispatcher : Window
    {
        private CanvasDispatcher _dispatcher;
        public WindowDispatcher()
        {
            InitializeComponent();

            _dispatcher = new CanvasDispatcher(canvas);

            _dispatcher.SetCursor(cursor,LeapUtils.LEFT_MOST_HAND);

            ILeapListenerClap clapListener = new ILeapListenerClap(leapEvent => {
                if (leapEvent.Source == right)
                {
                    info.Text = "Right";
                }
                else if (leapEvent.Source == left)
                {
                    info.Text = "Left";
                }
            });

            _dispatcher.AddClapListener(left, clapListener);

            _dispatcher.AddClapListener(right, clapListener);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _dispatcher.OnClose();
            base.OnClosing(e);
        }
    }
}
