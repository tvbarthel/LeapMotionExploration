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
using MyLeap.Event;

namespace LeapMotionExploration.Windows.Samples
{
    /// <summary>
    /// Logique d'interaction pour WindowDemo.xaml
    /// </summary>
    public partial class WindowFinalDemo : Window
    {

        private Controller Controller;
        private LeapListenerOneHandPosition CursorListener;
        private LeapListenerOneHandClose HandCloseListener;

        public WindowFinalDemo()
        {
            InitializeComponent();
            Controller = new Controller();

            CursorListener = new LeapListenerOneHandPosition(LeapUtils.LEFT_MOST_HAND);
            Controller.AddListener(CursorListener);
            CursorListener.OnStateChange += this.OnPositionChange;

            HandCloseListener = new LeapListenerOneHandClose(LeapUtils.LEFT_MOST_HAND);
            Controller.AddListener(HandCloseListener);
            HandCloseListener.OnHandStateChanged += this.OnHandClosed;

        }

        private void OnPositionChange(LeapEvent leapEvent)
        {
            setCursorPosition(leapEvent.Position);
        }

        private void setCursorPosition(Leap.Vector position)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                double xPos = cursorContainer.ActualWidth * position.x - leapCursor.Width / 2;
                double yPos = cursorContainer.ActualHeight * (1 - position.y) - leapCursor.Height / 2;
                leapCursor.SetValue(Canvas.TopProperty, yPos);
                leapCursor.SetValue(Canvas.LeftProperty, xPos);
            }));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Controller.RemoveListener(CursorListener);
            Controller.Dispose();
            base.OnClosing(e);
        }

        private void OnHandClosed(HandCloseEvent e)
        {
            switch (e.Type)
            {
                case HandCloseEvent.OPEN:
                    //TODO if closed & dragging called DragFinished
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        leapCursor.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                    }));
                    break;
                case HandCloseEvent.CLOSE:
                    //TODO check the event position to know if an Ui element has been selected
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        leapCursor.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));

                    }));
                    break;
            }
        }

        private void DragStarted()
        {
            //TODO inform drag started
            //TODO store original position of the element

        }

        private void DragMoved()
        {
            //TODO get the cursor position
            //TODO calculate the offset
            //TODO update the element position

        }

        private void DragFinished(bool cancelled)
        {
            //TODO inform drag stop
            //if cancelled reset position with original position
        }
    }
}
