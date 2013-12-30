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
        private LeapListenerOneHandPosition Listener;
        private List<Shape> Shapes;

        public WindowFinalDemo()
        {
            InitializeComponent();
            Controller = new Controller();

            Listener = new LeapListenerOneHandPosition(LeapUtils.LEFT_MOST_HAND);
            Controller.AddListener(Listener);

            Listener.OnStateChange += this.OnPositionChange;

            Shapes = new List<Shape>();
        }

        private void OnPositionChange(LeapEvent leapEvent)
        {
            setCursorPosition(leapEvent.Position);
        }

        private void setCursorPosition(Leap.Vector position)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                double posX = cursorContainer.ActualWidth * position.x - leapCursor.Width / 2;
                double posY = cursorContainer.ActualHeight * (1 - position.y) - leapCursor.Height / 2;
                leapCursor.SetValue(Canvas.TopProperty, posY);
                leapCursor.SetValue(Canvas.LeftProperty, posX);

                checkHover(posX, posY);
            }));
        }

        private void checkHover(double posX, double posY)
        {
            foreach (Shape shape in Shapes)
            {
                if (isShapeHover(shape, posX, posY))
                {
                    setShapeHover(shape);
                    break;
                }
            }
        }

        private void setShapeHover(Shape shape)
        {
            //TODO
        }

        private Boolean isShapeHover(Shape shape, double posX, double posY)
        {
            //TODO
            return false;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Controller.RemoveListener(Listener);
            Controller.Dispose();
            base.OnClosing(e);
        }
    }
}
