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
        //A list of the shapes present on the canvas.
        private List<Shape> Shapes;
        private LeapListenerOneHandPosition CursorListener;
        private LeapListenerOneHandClose HandCloseListener;

        private Shape _hoveredShape;

        //drag motion
        private bool _isDragging;
        private Point _originalShapePoint;
        private Point _startCursorPoint;

        public WindowFinalDemo()
        {
            InitializeComponent();

            _isDragging = false;
            Controller = new Controller();

            CursorListener = new LeapListenerOneHandPosition(LeapUtils.LEFT_MOST_HAND);
            Controller.AddListener(CursorListener);
            CursorListener.OnStateChange += this.OnPositionChange;

            HandCloseListener = new LeapListenerOneHandClose(LeapUtils.LEFT_MOST_HAND);
            Controller.AddListener(HandCloseListener);
            HandCloseListener.OnHandStateChange += this.OnHandClosed;

            Shapes = new List<Shape>();

            Rectangle rect1 = new Rectangle();
            rect1.Height = rect1.Width = 32;
            rect1.Fill = Brushes.Blue;
            Canvas.SetTop(rect1, 100);
            Canvas.SetLeft(rect1, 100);

            cursorContainer.Children.Add(rect1);


            Shapes.Add(colorPicker);
            Shapes.Add(shapePicker);
            Shapes.Add(preview);
            Shapes.Add(rect1);
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

                if (_isDragging)
                {
                    DragMoved();
                }
                else
                {
                    updateHover(posX, posY);
                }

            }));

        }


        private void updateHover(double posX, double posY)
        {
            //reset the current hovered shape if needed
            if (_hoveredShape!= null && !isCursorOnShape(_hoveredShape, posX, posY)) resetShapeHover();

            //look for a new hovered shape
            foreach (Shape shape in Shapes)
            {
                if (isCursorOnShape(shape, posX, posY))
                {
                    setShapeHover(shape);
                }
            }            
        }

        private void resetShapeHover()
        {
            if (_hoveredShape != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    _hoveredShape.Opacity = 1;
                    _hoveredShape.StrokeThickness = 0;
                    _hoveredShape = null;
                }));
            }
            
        }

        private void setShapeHover(Shape shape)
        {
            //if there is no _hoveredShape or if the shape has a higher index than _hoveredShape
            if (_hoveredShape == null || (_hoveredShape != shape && Shapes.IndexOf(shape) > Shapes.IndexOf(_hoveredShape)))
            {
                resetShapeHover();
                _hoveredShape = shape;
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    _hoveredShape.Opacity = 0.5;
                }));
            }           
        }

        private Boolean isCursorOnShape(Shape shape, double posX, double posY)
        {
            double shapeTop = (double)shape.GetValue(Canvas.TopProperty);
            double shapeLeft = (double)shape.GetValue(Canvas.LeftProperty);
            return (posX > shapeLeft && posX < (shapeLeft + shape.ActualWidth) && posY > shapeTop && posY < (shapeTop + shape.ActualHeight));
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
                    if (_isDragging)
                    {
                        DragFinished(false);
                    }

                    break;
                case HandCloseEvent.CLOSE:
                    //TODO check the event position to know if an Ui element has been selected
                    if (!_isDragging && _hoveredShape != null)
                    {
                        DragStarted();
                    }
                    break;
            }
        }

        private void DragStarted()
        {

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {

                //Inform drag started
                _isDragging = true;

                //Store original position of the element, in case of cancel
                _originalShapePoint = new Point(Canvas.GetLeft(_hoveredShape), Canvas.GetTop(_hoveredShape));

                //Store starting Point
                _startCursorPoint = new Point(Canvas.GetLeft(leapCursor), Canvas.GetTop(leapCursor));

                System.Diagnostics.Debug.WriteLine("DragStarted");

                //ui
                leapCursor.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));

            }));

        }

        private void DragMoved()
        {

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {

                //Get the current cursor position
                Point current = new Point(Canvas.GetLeft(leapCursor), Canvas.GetTop(leapCursor));


                //Calculate the offset
                Point offset = new Point(_startCursorPoint.X - current.X, _startCursorPoint.Y - current.Y);


                //Update the element position
                Canvas.SetTop(_hoveredShape, _originalShapePoint.Y - offset.Y);
                Canvas.SetLeft(_hoveredShape, _originalShapePoint.X - offset.X);
            }));

        }

        private void DragFinished(bool cancelled)
        {

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                //Inform drag stop
                _isDragging = false;

                //if cancelled reset position with original position
                System.Diagnostics.Debug.WriteLine("DragFinished");

                //ui
                leapCursor.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }));
        }
    }
}
