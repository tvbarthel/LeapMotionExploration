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
        //A list of the shapes present on the canvas.
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
                double posX = cursorContainer.ActualWidth * position.x;
                double posY = cursorContainer.ActualHeight * (1 - position.y);
                leapCursor.SetValue(Canvas.TopProperty, posY - leapCursor.Height / 2);
                leapCursor.SetValue(Canvas.LeftProperty, posX - leapCursor.Width / 2);

                updateHover(posX, posY);
            }));
        }


        private void updateHover(double posX, double posY)
        {
            foreach (Shape shape in Shapes)
            {
                if (isCursorOnShape(shape, posX, posY))
                {
                    //TODO hover only one shape
                    setShapeHover(shape);
                }
                else
                {
                    resetShapeHover(shape);
                }
            }
        }

        private void resetShapeHover(Shape shape)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                shape.Opacity = 1;
            }));
        }

        private void setShapeHover(Shape shape)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                shape.Opacity = 0.5;
            }));
        }

        private Boolean isCursorOnShape(Shape shape, double posX, double posY)
        {
            double shapeTop = (double)shape.GetValue(Canvas.TopProperty);
            double shapeLeft = (double)shape.GetValue(Canvas.LeftProperty);
            return (posX > shapeLeft && posX < (shapeLeft + shape.ActualWidth) && posY > shapeTop && posY < (shapeTop + shape.ActualHeight));            
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Controller.RemoveListener(Listener);
            Controller.Dispose();
            base.OnClosing(e);
        }

        private void OnHandClosed(HandCloseEvent e)
        {
            switch (e.Type)
            {
                case HandCloseEvent.OPEN:
                    //TODO if closed & dragging called DragFinished
                    break;
                case HandCloseEvent.CLOSE:
                    //TODO check the event position to know if an Ui element has been selected
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
