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
using LeapMotionExploration.Windows.Samples.Converter;
using LeapMotionExploration.Windows.Samples.Ui;
using System.Windows.Media.Effects;

namespace LeapMotionExploration.Windows.Samples
{
    /// <summary>
    /// Logique d'interaction pour WindowDemo.xaml
    /// </summary>
    public partial class WindowFinalDemo : Window
    {

        private const int MIN_WIDTH = 75;
        private const int MIN_HEIGHT = 75;

        private const int DEFAULT_HEIGHT = 150;
        private const int DEFAULT_WIDTH = 150;        
        
        private Controller _controller;
        //A list of the graphic elements present on the canvas.
        private List<FrameworkElement> _graphicElements;
        //A list of the graphic elements that the user can not drag.
        private List<FrameworkElement> _staticGraphicElements;

        //Cursor position
        private LeapListenerOneHandPosition _cursorListener;
        private Boolean _isCursorPositionTracked;
        private LeapListenerOneHandClose _handCloseListener;

        //hovered
        private FrameworkElement _hoveredGraphicElement;        
        private DraggableHoveredAdorner _draggableHoveredAdorner;

        //drag motion
        private bool _isDragging;
        private Point _originalGraphicElementPoint;
        private Point _startCursorPoint;

        //Rotating selection
        private LeapListenerRotateSelection _rotatingSelectionListener;

        //Color rotating selection
        private TextBlock[] _mnColorPickerItems;
        private int _currentColorPickerItemIndex;
        
        //Shape rotation selection
        private TextBlock[] _mnShapePickerItems;
        private int _currentShapePickerItemIndex;

        private Shape _currentCandidateShape;

        private Point _currentCursorPoint;

        public WindowFinalDemo()
        {
            InitializeComponent();

            _isDragging = false;
            _currentCursorPoint = new Point(0,0);
            _originalGraphicElementPoint = new Point(0, 0);
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

            _mnShapePickerItems = new TextBlock[] { mnShapePickerRectangle, mnShapePickerCircle, mnShapePickerEllipse };
            _currentShapePickerItemIndex = 0;
            selectShapeItem(_currentShapePickerItemIndex);

            _graphicElements = new List<FrameworkElement>();
            _staticGraphicElements = new List<FrameworkElement>();

            Rectangle rect1 = new Rectangle();
            rect1.Height = rect1.Width = 32;
            rect1.Fill = Brushes.Blue;
            Canvas.SetTop(rect1, 100);
            Canvas.SetLeft(rect1, 100);

            cursorContainer.Children.Add(rect1);


            _graphicElements.Add(colorPicker);
            _graphicElements.Add(shapePicker);
            _graphicElements.Add(rect1);

            _staticGraphicElements.Add(colorPicker);
            _staticGraphicElements.Add(shapePicker);

            createCandidateShape();
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

        private void createCandidateShape()
        {

            if (_currentCandidateShape != null)
            {
                cursorContainer.Children.Remove(_currentCandidateShape);
                _currentCandidateShape = null;
            }

            switch(_currentShapePickerItemIndex)
            {
                case 0:
                    //Rectangle
                    _currentCandidateShape = new Rectangle();
                    _currentCandidateShape.Width = DEFAULT_WIDTH;
                    _currentCandidateShape.Height = DEFAULT_HEIGHT;
                    break;

                case 1:
                    //Circle
                    _currentCandidateShape = new Ellipse();
                    _currentCandidateShape.Width = DEFAULT_WIDTH;
                    _currentCandidateShape.Height = DEFAULT_HEIGHT;
                    break;

                case 2:
                    //Ellipse
                    _currentCandidateShape = new Ellipse();
                    _currentCandidateShape.Width = DEFAULT_WIDTH;
                    _currentCandidateShape.Height = Math.Max(DEFAULT_HEIGHT / 2, MIN_HEIGHT);
                    break;
            }

            if (_currentCandidateShape != null)
            {
                _currentCandidateShape.Fill = _mnColorPickerItems[_currentColorPickerItemIndex].Background;

                MultiBinding mbCanvasTop = new MultiBinding();
                mbCanvasTop.Converter = new LeftMenuItemCanvasTopConverter();

                Binding containerHeight = new Binding("ActualHeight");
                containerHeight.ElementName = cursorContainer.Name;

                Binding selfHeight = new Binding("ActualHeight");
                selfHeight.RelativeSource = RelativeSource.Self;

                Binding marginTop = new Binding();
                marginTop.Source = 0d;

                mbCanvasTop.Bindings.Add(containerHeight);
                mbCanvasTop.Bindings.Add(selfHeight);
                mbCanvasTop.Bindings.Add(marginTop);

                _currentCandidateShape.SetBinding(Canvas.TopProperty, mbCanvasTop);
                _currentCandidateShape.SetValue(Canvas.LeftProperty, 100d);

                cursorContainer.Children.Add(_currentCandidateShape);
                _graphicElements.Add(_currentCandidateShape);
            }

        }

        /**
         * Rotation Selection
         * 
         * rotationSelectionEvent(LeapEvent leapEvent)
         * shapeSelectionEvent(LeapEvent leapEvent)
         * colorSelectionEvent(LeapEvent leapEvent)
         */

        private void rotationSelectionEvent(LeapEvent leapEvent)
        {

            if (_hoveredGraphicElement != null)
            {
                if (_hoveredGraphicElement.Equals(colorPicker))
                {
                    colorSelectionEvent(leapEvent);
                }
                else if (_hoveredGraphicElement.Equals(shapePicker))
                {
                    shapeSelectionEvent(leapEvent);
                }
                
            }
        }

        private void shapeSelectionEvent(LeapEvent leapEvent)
        {
            switch(leapEvent.Type)
            {
                case LeapEvent.ROTATION_SELECTION_START:
                    _isCursorPositionTracked = false;
                    setMenuVisibility(mnShapePicker, Visibility.Visible);
                    break;

                case LeapEvent.ROTATION_SELECTION_NEXT:
                    selectNextShape();
                    break;
                    
                case LeapEvent.ROTATION_SELECTION_PREVIOUS:
                    selectPreviousShape();
                    break;

                case LeapEvent.ROTATION_SELECTION_END:
                    _isCursorPositionTracked = true;
                    setMenuVisibility(mnShapePicker, Visibility.Hidden);
                    break;
            }
        }

        private void colorSelectionEvent(LeapEvent leapEvent)
        {
            switch (leapEvent.Type)
            {
                case LeapEvent.ROTATION_SELECTION_START:
                    _isCursorPositionTracked = false;
                    setMenuVisibility(mnColorPicker, Visibility.Visible);
                    break;

                case LeapEvent.ROTATION_SELECTION_NEXT:
                    selectNextColor();
                    break;

                case LeapEvent.ROTATION_SELECTION_PREVIOUS:
                    selectPreviousColor();
                    break;

                case LeapEvent.ROTATION_SELECTION_END:
                    _isCursorPositionTracked = true;
                    setMenuVisibility(mnColorPicker, Visibility.Hidden);
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
         */

        private void selectColorItem(int i)
        {
            selectItem(mnColorPicker, _mnColorPickerItems, i);
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

        /**
         * Shape Rotating Picker
         * 
         * selectShapeItem(int i)
         * selectNextShape()
         * selectPreviousShape()
         * updateCurrentShapeSelection() 
         */

        private void selectShapeItem(int i)
        {
            selectItem(mnShapePicker, _mnShapePickerItems, i);
            Shape newShape = null;
            switch(i)
            {
                case 0:
                    //Rectangle
                    newShape = new Rectangle();
                    newShape.Width = 100;
                    newShape.Height = 100;
                    break;

                case 1:
                    //Circle
                    newShape = new Ellipse();
                    newShape.Width = 100;
                    newShape.Height = 100;
                    break;

                case 2:
                    //Ellipse
                    newShape = new Ellipse();
                    newShape.Width = 100;
                    newShape.Height = 50;
                    break;

            }

            if (newShape != null)
            {
                newShape.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                newShape.StrokeThickness = 2;
                shapePicker.Children.Clear();
                shapePicker.Children.Add(newShape);
            }
            
        }

        private void selectNextShape() {
            _currentShapePickerItemIndex = Math.Min(_mnShapePickerItems.Count(), _currentShapePickerItemIndex + 1);
            updateCurrentShapeSelection();
        }

        private void selectPreviousShape()
        {
            _currentShapePickerItemIndex = Math.Max(0, _currentShapePickerItemIndex - 1);
            updateCurrentShapeSelection();
        }

        private void updateCurrentShapeSelection()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                selectShapeItem(_currentShapePickerItemIndex);
            }));
        }

        /**
         * Rotating Picker
         * 
         * selectItem(FrameworkElement menu, FrameworkElement[] items, int index)
         * setMenuVisibility(FrameworkElement menu, Visibility visibility)
         */

        private void selectItem(FrameworkElement menu, FrameworkElement[] items, int index)
        {
            menu.RenderTransform = new RotateTransform(90 - 45 * index);
            foreach(FrameworkElement item in items)
            {
                item.Opacity = 0.4;
            }
            items[index].Opacity = 1;
        }

        private void setMenuVisibility(FrameworkElement menu, Visibility visibility)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                menu.Visibility = visibility;
            }));
        }


        /**
         * Hover & Drag
         */

        private void updateHover(double posX, double posY)
        {
            //reset the current hovered graphic element if needed
            if (_hoveredGraphicElement!= null && !isCursorOnGraphicElement(_hoveredGraphicElement, posX, posY)) resetHoveredGraphicElement();

            //look for a new hovered graphic element
            foreach (FrameworkElement graphicElement in _graphicElements)
            {
                if (isCursorOnGraphicElement(graphicElement, posX, posY))
                {
                    setHoveredGraphicElement(graphicElement);
                }
            }            
        }

        private void resetHoveredGraphicElement()
        {
            if (_hoveredGraphicElement != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    _hoveredGraphicElement.Opacity = 1;
                    _hoveredGraphicElement = null;
                    resetDraggableOverlay();
                }));
            }
            
        }

        private void setDraggableOverlay(FrameworkElement graphicElement)
        {
            _draggableHoveredAdorner = new DraggableHoveredAdorner(graphicElement);
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(graphicElement);
            layer.Add(_draggableHoveredAdorner);
        }

        private void resetDraggableOverlay(){
            if (_draggableHoveredAdorner != null)
            {
                AdornerLayer.GetAdornerLayer(_draggableHoveredAdorner.AdornedElement).Remove(_draggableHoveredAdorner);
                _draggableHoveredAdorner = null;

            }
        }

        private void setHoveredGraphicElement(FrameworkElement graphicElement)
        {
            //if there is no _hoveredGraphicElement or if the graphic element has a higher index than _hoveredGraphicElement
            if (_hoveredGraphicElement == null || (_hoveredGraphicElement != graphicElement && _graphicElements.IndexOf(graphicElement) > _graphicElements.IndexOf(_hoveredGraphicElement)))
            {
                resetHoveredGraphicElement();
                _hoveredGraphicElement = graphicElement;
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (!_staticGraphicElements.Contains(graphicElement))
                    {
                        setDraggableOverlay(graphicElement);
                       
                    }
                    else
                    {
                        _hoveredGraphicElement.Opacity = 0.5;
                    }
                }));
            }           
        }

        private Boolean isCursorOnGraphicElement(FrameworkElement graphicElement, double posX, double posY)
        {
            double graphicElementTop = (double)graphicElement.GetValue(Canvas.TopProperty);
            double graphicElementLeft = (double)graphicElement.GetValue(Canvas.LeftProperty);
            return (posX > graphicElementLeft && posX < (graphicElementLeft + graphicElement.ActualWidth) && posY > graphicElementTop && posY < (graphicElementTop + graphicElement.ActualHeight));
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
                    if (!_isDragging && _hoveredGraphicElement != null && !_staticGraphicElements.Contains(_hoveredGraphicElement))
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
                _originalGraphicElementPoint.X = Canvas.GetLeft(_hoveredGraphicElement);
                _originalGraphicElementPoint.Y = Canvas.GetTop(_hoveredGraphicElement);

                //Store starting Point
                _startCursorPoint.X = _currentCursorPoint.X;
                _startCursorPoint.Y = _currentCursorPoint.Y;

                System.Diagnostics.Debug.WriteLine("DragStarted");

                //ui
                leapCursor.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
                resetDraggableOverlay();
                _hoveredGraphicElement.Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 320,
                    ShadowDepth = 7,
                    Opacity = 0.8,
                    BlurRadius = 4
                };

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
                Canvas.SetTop(_hoveredGraphicElement, _originalGraphicElementPoint.Y - offset.Y);
                Canvas.SetLeft(_hoveredGraphicElement, _originalGraphicElementPoint.X - offset.X);
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
                _hoveredGraphicElement.Effect = null;
                setDraggableOverlay(_hoveredGraphicElement);

                //TODO check drop area
                if (isCursorOnGraphicElement(basket, _currentCursorPoint.X, _currentCursorPoint.Y))
                {
                    _graphicElements.Remove(_hoveredGraphicElement);
                    cursorContainer.Children.Remove(_hoveredGraphicElement);
                    _hoveredGraphicElement = null;
                }
            }));
        }

    }
}
