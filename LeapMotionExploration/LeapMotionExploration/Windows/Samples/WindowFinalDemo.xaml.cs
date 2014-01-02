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

        private Controller _controller;
        //A list of the shapes present on the canvas.
        private List<FrameworkElement> _shapes;
        //A list of the shapes that the user can not drag.
        private List<FrameworkElement> _staticShapes;

        //Cursor position
        private LeapListenerOneHandPosition _cursorListener;
        private Boolean _isCursorPositionTracked;
        private LeapListenerOneHandClose _handCloseListener;

        private FrameworkElement _hoveredShape;

        //drag motion
        private bool _isDragging;
        private Point _originalShapePoint;
        private Point _startCursorPoint;

        //Color rotating selection
        private TextBlock[] _mnColorPickerItems;
        private int _currentColorPickerItemIndex;
        private LeapListenerRotateSelection _rotatingSelectionListener;

        private Point _currentCursorPoint;

        public WindowFinalDemo()
        {
            InitializeComponent();

            _isDragging = false;
            _currentCursorPoint = new Point(0,0);
            _originalShapePoint = new Point(0, 0);
            _startCursorPoint = new Point(0, 0);

            _controller = new Controller();

            _cursorListener = new LeapListenerOneHandPosition(LeapUtils.LEFT_MOST_HAND);
            _isCursorPositionTracked = true;
            _controller.AddListener(_cursorListener);
            _cursorListener.OnStateChange += this.OnPositionChange;

            _handCloseListener = new LeapListenerOneHandClose(LeapUtils.LEFT_MOST_HAND);
            _controller.AddListener(_handCloseListener);
            _handCloseListener.OnHandStateChange += this.OnHandClosed;

            _rotatingSelectionListener = new LeapListenerRotateSelection();
            _controller.AddListener(_rotatingSelectionListener);
            _rotatingSelectionListener.OnStateChange += rotationSelectionEvent;

            _mnColorPickerItems = new TextBlock[] { mnColorPickerBlue, mnColorPickerPurple, mnColorPickerGreen, mnColorPickerOrange, mnColorPickerRed };
            _currentColorPickerItemIndex = 0;
            selectColorItem(_currentColorPickerItemIndex);

            _shapes = new List<FrameworkElement>();
            _staticShapes = new List<FrameworkElement>();

            Rectangle rect1 = new Rectangle();
            rect1.Height = rect1.Width = 32;
            rect1.Fill = Brushes.Blue;
            Canvas.SetTop(rect1, 100);
            Canvas.SetLeft(rect1, 100);

            cursorContainer.Children.Add(rect1);


            _shapes.Add(colorPicker);
            _shapes.Add(shapePicker);
            _shapes.Add(preview);
            _shapes.Add(rect1);

            _staticShapes.Add(colorPicker);
            _staticShapes.Add(shapePicker);
        }

        private void OnPositionChange(LeapEvent leapEvent)
        {
            setCursorPosition(leapEvent.Position);
        }

        private void setCursorPosition(Leap.Vector position)
        {

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (_isCursorPositionTracked)
                {
                    _currentCursorPoint.X = cursorContainer.ActualWidth * position.x;
                    _currentCursorPoint.Y = cursorContainer.ActualHeight * (1 - position.y);
                    leapCursor.SetValue(Canvas.TopProperty, _currentCursorPoint.Y - leapCursor.Height / 2);
                    leapCursor.SetValue(Canvas.LeftProperty, _currentCursorPoint.X - leapCursor.Width / 2);
                }

                if (_isDragging)
                {
                    DragMoved();
                }
                else
                {
                    updateHover(_currentCursorPoint.X, _currentCursorPoint.Y);
                }

            }));

        }

        /**
         * Rotation Selection
         * 
         * rotationSelectionEvent(LeapEvent leapEvent)
         * colorSelectionEvent(LeapEvent leapEvent)
         */

        private void rotationSelectionEvent(LeapEvent leapEvent)
        {
            if (_hoveredShape.Equals(colorPicker))
            {
                colorSelectionEvent(leapEvent);
            }
        }

        private void colorSelectionEvent(LeapEvent leapEvent)
        {
            switch (leapEvent.Type)
            {
                case LeapEvent.ROTATION_SELECTION_START:
                    _isCursorPositionTracked = false;
                    setColorMenuVisibility(Visibility.Visible);
                    break;

                case LeapEvent.ROTATION_SELECTION_NEXT:
                    selectNextColor();
                    break;

                case LeapEvent.ROTATION_SELECTION_PREVIOUS:
                    selectPreviousColor();
                    break;

                case LeapEvent.ROTATION_SELECTION_END:
                    _isCursorPositionTracked = true;
                    setColorMenuVisibility(Visibility.Hidden);
                    break;
            }
        }   


        /**
         * Color Rotating Picker
         * 
         * selectColorItem(int i)
         * selectNextColor()
         * selectPreviousColor()
         * updateCurrentColorSelection()
         * setColorMenuVisibility(Visibility visibility)
         */

        private void selectColorItem(int i)
        {
            mnColorPicker.RenderTransform = new RotateTransform(90 - 45 * i);
            foreach (TextBlock textBlock in _mnColorPickerItems)
            {
                textBlock.Opacity = 0.4;
            }
            _mnColorPickerItems[i].Opacity = 1;
            colorPicker.Fill = _mnColorPickerItems[i].Background;
        }

        private void selectNextColor()
        {
            _currentColorPickerItemIndex = Math.Min(_mnColorPickerItems.Count(), _currentColorPickerItemIndex + 1);
            updateCurrentColorSelection();
        }

        private void selectPreviousColor()
        {
            _currentColorPickerItemIndex = Math.Max(0, _currentColorPickerItemIndex - 1);
            updateCurrentColorSelection();
        }

        private void updateCurrentColorSelection()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                selectColorItem(_currentColorPickerItemIndex);
            }));
        }

        private void setColorMenuVisibility(Visibility visibility)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                mnColorPicker.Visibility = visibility;
            }));
        }


        /**
         * Hover & Drag
         */

        private void updateHover(double posX, double posY)
        {
            //reset the current hovered shape if needed
            if (_hoveredShape!= null && !isCursorOnShape(_hoveredShape, posX, posY)) resetShapeHover();

            //look for a new hovered shape
            foreach (FrameworkElement shape in _shapes)
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
                    _hoveredShape = null;
                }));
            }
            
        }

        private void setShapeHover(FrameworkElement shape)
        {
            //if there is no _hoveredShape or if the shape has a higher index than _hoveredShape
            if (_hoveredShape == null || (_hoveredShape != shape && _shapes.IndexOf(shape) > _shapes.IndexOf(_hoveredShape)))
            {
                resetShapeHover();
                _hoveredShape = shape;
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    _hoveredShape.Opacity = 0.5;
                }));
            }           
        }

        private Boolean isCursorOnShape(FrameworkElement shape, double posX, double posY)
        {
            double shapeTop = (double)shape.GetValue(Canvas.TopProperty);
            double shapeLeft = (double)shape.GetValue(Canvas.LeftProperty);
            return (posX > shapeLeft && posX < (shapeLeft + shape.ActualWidth) && posY > shapeTop && posY < (shapeTop + shape.ActualHeight));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _controller.RemoveListener(_cursorListener);
            _controller.Dispose();
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
                    if (!_isDragging && _hoveredShape != null && !_staticShapes.Contains(_hoveredShape))
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
                _originalShapePoint.X = Canvas.GetLeft(_hoveredShape);
                _originalShapePoint.Y = Canvas.GetTop(_hoveredShape);

                //Store starting Point
                _startCursorPoint.X = _currentCursorPoint.X;
                _startCursorPoint.Y = _currentCursorPoint.Y;

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

                //TODO check drop area
                if (isCursorOnShape(basket, _currentCursorPoint.X, _currentCursorPoint.Y))
                {
                    _shapes.Remove(_hoveredShape);
                    cursorContainer.Children.Remove(_hoveredShape);
                }
            }));
        }

    }
}
