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
using System.Windows.Media.Animation;
using System.Timers;
using System.IO;

namespace LeapMotionExploration.Windows.Samples
{
    /// <summary>
    /// Logique d'interaction pour WindowDemo.xaml
    /// </summary>
    public partial class WindowFinalDemo : Window
    {

        private const int MIN_WIDTH = 100;
        private const int MIN_HEIGHT = 100;

        private const int DEFAULT_HEIGHT = 150;
        private const int DEFAULT_WIDTH = 150;

        private const double SHAPE_CANDIDATE_LEFT_MARGIN = 180d;
        private const double SHAPE_CANDIDATE_MAX_SPAWN_DISTANCE = 200d;

        private const double DEFAULT_SIZE_DELTA = 2.5d;

        private const String SCREENSHOT_FILE_NAME_FORMAT = "leap_motion_exploration_{0:dd_MM_yyyy_HH_mm_ss}.png";

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

        //basket
        private DeleteAdorner _deleteAdorner;

        //Rotating selection
        private LeapListenerRotateSelection _rotatingSelectionListener;

        //Color rotating selection
        private TextBlock[] _mnColorPickerItems;
        private int _currentColorPickerItemIndex;

        //Shape rotation selection
        private TextBlock[] _mnShapePickerItems;
        private int _currentShapePickerItemIndex;

        //Shape manipulation
        private LeapListenerTwoHandManipulation _shapeManipulationListener;

        //Snapshot saver
        private LeapListenerClap _snapshotSaverListener;

        //info place holder
        private TextBlock _infoPlaceHolder;

        private MultiBinding _mbCanvasTop;
        private Shape _currentShapeCandidate;
        private Point _currentShapeCandidateSpawn;

        private Point _currentCursorPoint;

        public WindowFinalDemo()
        {
            InitializeComponent();

            _isDragging = false;
            _currentCursorPoint = new Point(0, 0);
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

            _shapeManipulationListener = new LeapListenerTwoHandManipulation();
            _controller.AddListener(_shapeManipulationListener);
            _shapeManipulationListener.OnStateChange += this.OnShapeManipulationEvent;

            _snapshotSaverListener = new LeapListenerClap();
            _controller.AddListener(_snapshotSaverListener);
            _snapshotSaverListener.OnClapDetected += this.OnSnapshotSaveEvent;
            //TODO check if OnStateChange != null before invoking an action.
            _snapshotSaverListener.OnStateChange += new Action<string>((string s) => { });

            _mnShapePickerItems = new TextBlock[] { mnShapePickerRectangle, mnShapePickerCircle, mnShapePickerEllipse };
            _currentShapePickerItemIndex = 0;

            _mnColorPickerItems = new TextBlock[] { mnColorPickerBlue, mnColorPickerPurple, mnColorPickerGreen, mnColorPickerOrange, mnColorPickerRed };
            _currentColorPickerItemIndex = 0;

            _graphicElements = new List<FrameworkElement>();
            _staticGraphicElements = new List<FrameworkElement>();

            Rectangle rect1 = new Rectangle();
            rect1.Height = rect1.Width = 32;
            rect1.Fill = Brushes.Blue;
            Canvas.SetTop(rect1, 100);
            Canvas.SetLeft(rect1, 100);

            cursorContainer.Children.Add(rect1);

            _mbCanvasTop = new MultiBinding();
            _mbCanvasTop.Converter = new LeftMenuItemCanvasTopConverter();

            Binding containerHeight = new Binding("ActualHeight");
            containerHeight.ElementName = cursorContainer.Name;

            Binding selfHeight = new Binding("ActualHeight");
            selfHeight.RelativeSource = RelativeSource.Self;

            Binding marginTop = new Binding();
            marginTop.Source = 0d;

            _mbCanvasTop.Bindings.Add(containerHeight);
            _mbCanvasTop.Bindings.Add(selfHeight);
            _mbCanvasTop.Bindings.Add(marginTop);


            _graphicElements.Add(colorPicker);
            _graphicElements.Add(shapePicker);
            _graphicElements.Add(rect1);

            _staticGraphicElements.Add(colorPicker);
            _staticGraphicElements.Add(shapePicker);

            selectShapeItem(_currentShapePickerItemIndex);
            selectColorItem(_currentColorPickerItemIndex);
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

        private void updateCandidateShapeColor()
        {
            if (_currentShapeCandidate == null)
            {
                createCandidateShape();
            }
            else
            {
                _currentShapeCandidate.Fill = _mnColorPickerItems[_currentColorPickerItemIndex].Background;
            }
        }

        private void createCandidateShape()
        {

            if (_currentShapeCandidate != null)
            {
                cursorContainer.Children.Remove(_currentShapeCandidate);
                _graphicElements.Remove(_currentShapeCandidate);
                _currentShapeCandidate = null;
            }

            switch (_currentShapePickerItemIndex)
            {
                case 0:
                    //Rectangle
                    _currentShapeCandidate = new Rectangle();
                    _currentShapeCandidate.Width = DEFAULT_WIDTH;
                    _currentShapeCandidate.Height = DEFAULT_HEIGHT;
                    break;

                case 1:
                    //Circle
                    _currentShapeCandidate = new Ellipse();
                    _currentShapeCandidate.Width = DEFAULT_WIDTH;
                    _currentShapeCandidate.Height = DEFAULT_HEIGHT;
                    break;

                case 2:
                    //Ellipse
                    _currentShapeCandidate = new Ellipse();
                    _currentShapeCandidate.Width = DEFAULT_WIDTH;
                    _currentShapeCandidate.Height = Math.Max(DEFAULT_HEIGHT / 2, MIN_HEIGHT);
                    break;
            }

            if (_currentShapeCandidate != null)
            {
                _currentShapeCandidate.Fill = _mnColorPickerItems[_currentColorPickerItemIndex].Background;

                _currentShapeCandidate.SetBinding(Canvas.TopProperty, _mbCanvasTop);
                _currentShapeCandidate.SetValue(Canvas.LeftProperty, SHAPE_CANDIDATE_LEFT_MARGIN);

                cursorContainer.Children.Add(_currentShapeCandidate);
                _graphicElements.Add(_currentShapeCandidate);

            }

        }

        /**
         * Snapshot Saver
         * 
         * OnSnapshotSaveEvent(LeapEvent leapEvent)
         * CanvasSnapshopt(Canvas canvas)
         */

        private void OnSnapshotSaveEvent(LeapEvent leapEvent)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                CanvasSnapshot(cursorContainer);
            }));
        }

        private void CanvasSnapshot(Canvas canvas)
        {
            RenderTargetBitmap targetBitmap = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96d, 96d, PixelFormats.Default);
            targetBitmap.Render(canvas);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(targetBitmap));

            using (FileStream file = File.Open(String.Format(SCREENSHOT_FILE_NAME_FORMAT, DateTime.Now), FileMode.OpenOrCreate))
            {
                encoder.Save(file);
            }
        }

        /**
         * Shape Manipulation
         * 
         * OnShapeManipulationEvent(LeapEvent leapEvent)
         * invokeChangeUIElementSize(FrameworkElement uiElement, double widthDelta, double heightDelta)
         * chageUIElementSize(FrameworkElement uiElement, double widthDelta, double heightDelta)
         */
        private void OnShapeManipulationEvent(LeapEvent leapEvent)
        {
            if (_hoveredGraphicElement != null && !_staticGraphicElements.Contains(_hoveredGraphicElement))
            {
                switch (leapEvent.Type)
                {
                    case LeapEvent.TRANSFORMATION_SIZE_UP:                        
                        invokeChangeUIElementSize(_hoveredGraphicElement, DEFAULT_SIZE_DELTA, DEFAULT_SIZE_DELTA);
                        break;

                    case LeapEvent.TRANSFORMATION_SIZE_DOWN:
                        invokeChangeUIElementSize(_hoveredGraphicElement, -DEFAULT_SIZE_DELTA, -DEFAULT_SIZE_DELTA);
                        break;
                }
            }            
        }

        private void invokeChangeUIElementSize(FrameworkElement uiElement, double widthDelta, double heightDelta)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                changeUIElementSize(uiElement, widthDelta, heightDelta);
            }));
        }

        private void changeUIElementSize(FrameworkElement uiElement, double widthDelta, double heightDelta)
        {            
            uiElement.Width = Math.Max(MIN_WIDTH, uiElement.Width + widthDelta);
            uiElement.Height = Math.Max(MIN_HEIGHT, uiElement.Height + heightDelta); 
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
            switch (leapEvent.Type)
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
            updateCandidateShapeColor();
        }

        private void selectNextColor()
        {
            _currentColorPickerItemIndex = Math.Min(_mnColorPickerItems.Count() - 1, _currentColorPickerItemIndex + 1);
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
            switch (i)
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
                createCandidateShape();
            }

        }

        private void selectNextShape()
        {
            _currentShapePickerItemIndex = Math.Min(_mnShapePickerItems.Count() - 1, _currentShapePickerItemIndex + 1);
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
            foreach (FrameworkElement item in items)
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
            if (_hoveredGraphicElement != null && !isCursorOnGraphicElement(_hoveredGraphicElement, posX, posY)) resetHoveredGraphicElement();

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
                    UnsetDraggableOverlay();
                }));
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
                        SetDraggableOverlay(graphicElement);

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
            //Look for a top reference
            double graphicElementTop = Canvas.GetTop(graphicElement);
            if (double.IsNaN(graphicElementTop))
            {
                graphicElementTop = cursorContainer.ActualHeight - Canvas.GetBottom(graphicElement) - graphicElement.ActualHeight;
            }

            //Look for a left reference
            double graphicElementLeft = Canvas.GetLeft(graphicElement);
            if (double.IsNaN(graphicElementLeft))
            {
                graphicElementLeft = cursorContainer.ActualWidth - Canvas.GetRight(graphicElement) - graphicElement.ActualWidth;
            }

            return !double.IsNaN(graphicElementLeft) && !double.IsNaN(graphicElementLeft) && posX > graphicElementLeft && posX < (graphicElementLeft + graphicElement.ActualWidth) && posY > graphicElementTop && posY < (graphicElementTop + graphicElement.ActualHeight);
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


        /**
         * Drag gestures
         */

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
                SetDraggingOverlay();

                //Save the current spawning point
                if (_hoveredGraphicElement.Equals(_currentShapeCandidate))
                {
                    _currentShapeCandidateSpawn = new Point(SHAPE_CANDIDATE_LEFT_MARGIN + _currentShapeCandidate.ActualWidth / 2, cursorContainer.ActualHeight / 2);
                }
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

                //check basket area                
                if (isCursorOnGraphicElement(basket, _currentCursorPoint.X, _currentCursorPoint.Y))
                {
                    SetDeleteOverlay();
                }
                else
                {
                    UnsetDeleteOverlay();
                }

                //Update the element position
                Canvas.SetTop(_hoveredGraphicElement, _originalGraphicElementPoint.Y - offset.Y);
                Canvas.SetLeft(_hoveredGraphicElement, _originalGraphicElementPoint.X - offset.X);

                //If the current candidate shape is being dragged,
                //Check if it went to far from the spawning point.
                if (_hoveredGraphicElement.Equals(_currentShapeCandidate))
                {
                    double currentDistance = Math.Sqrt(Math.Pow(_currentShapeCandidateSpawn.X - (Canvas.GetLeft(_currentShapeCandidate) + _currentShapeCandidate.ActualWidth / 2), 2)
                        + Math.Pow(_currentShapeCandidateSpawn.Y - (Canvas.GetTop(_currentShapeCandidate) + _currentShapeCandidate.ActualHeight / 2), 2));
                    if (currentDistance > SHAPE_CANDIDATE_MAX_SPAWN_DISTANCE)
                    {
                        //The current candidate shape went to far and can no longer be considered as a candidate.
                        _currentShapeCandidate = null;
                        createCandidateShape();
                    }
                }
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
                UnsetDragginOverlay();

                //TODO check drop area
                if (isCursorOnGraphicElement(basket, _currentCursorPoint.X, _currentCursorPoint.Y))
                {
                    UnsetDraggableOverlay();
                    UnsetDeleteOverlay();
                    _graphicElements.Remove(_hoveredGraphicElement);
                    cursorContainer.Children.Remove(_hoveredGraphicElement);
                    _hoveredGraphicElement = null;

                }

                //Reset the position of the current candidate shape.
                if (_hoveredGraphicElement != null && _hoveredGraphicElement.Equals(_currentShapeCandidate))
                {
                    _currentShapeCandidate.SetBinding(Canvas.TopProperty, _mbCanvasTop);
                    _currentShapeCandidate.SetValue(Canvas.LeftProperty, SHAPE_CANDIDATE_LEFT_MARGIN);
                }
            }));
        }

        /**
         * 
         * Overlay 
         * 
         */


        //Drag
        private void SetDraggingOverlay()
        {
            UnsetDraggableOverlay();
            Zoom(_hoveredGraphicElement, new DoubleAnimation(1, 1.2, TimeSpan.FromMilliseconds(100)));
            _hoveredGraphicElement.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 320,
                ShadowDepth = 7,
                Opacity = 0.8,
                BlurRadius = 4
            };
        }

        private void UnsetDragginOverlay()
        {
            _hoveredGraphicElement.Effect = null;
            DoubleAnimation anim = new DoubleAnimation(1.2, 1, TimeSpan.FromMilliseconds(100));
            Zoom(_hoveredGraphicElement, anim);
            SetDraggableOverlay(_hoveredGraphicElement);
        }




        //Draggable
        private void SetDraggableOverlay(FrameworkElement graphicElement)
        {
            _draggableHoveredAdorner = new DraggableHoveredAdorner(graphicElement);
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(graphicElement);
            layer.Add(_draggableHoveredAdorner);
        }

        private void UnsetDraggableOverlay()
        {
            if (_draggableHoveredAdorner != null)
            {
                AdornerLayer.GetAdornerLayer(_draggableHoveredAdorner.AdornedElement).Remove(_draggableHoveredAdorner);
                _draggableHoveredAdorner = null;

            }
        }




        //Deletable
        private void SetDeleteOverlay()
        {
            if (_deleteAdorner == null)
            {
                _deleteAdorner = new DeleteAdorner(_hoveredGraphicElement);
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(_hoveredGraphicElement);
                layer.Add(_deleteAdorner);
                _hoveredGraphicElement.Opacity = 0.8;
                DisplayInfo("Déposez la forme pour la supprimer définitivement.");
            }
        }

        private void UnsetDeleteOverlay()
        {
            if (_deleteAdorner != null)
            {
                AdornerLayer.GetAdornerLayer(_deleteAdorner.AdornedElement).Remove(_deleteAdorner);
                _deleteAdorner = null;
                _hoveredGraphicElement.Opacity = 1.0;
                HideInfo();

            }
        }

        private void Zoom(FrameworkElement target, DoubleAnimation animation)
        {
            ScaleTransform trans = new ScaleTransform();
            target.RenderTransformOrigin = new Point(0.5, 0.5);
            target.RenderTransform = trans;
            DoubleAnimation anim = animation;
            trans.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            trans.BeginAnimation(ScaleTransform.ScaleYProperty, anim);

        }

        /**
         * Info place holder
         */

        private void CreateInfoPlaceholder()
        {
            _infoPlaceHolder = new TextBlock();
            _infoPlaceHolder.Foreground = new SolidColorBrush(Colors.Black);
            _infoPlaceHolder.Background = new SolidColorBrush(Color.FromRgb(211, 211, 211));
            _infoPlaceHolder.FontSize = 20;
            _infoPlaceHolder.FontFamily = new FontFamily("Roboto");
            _infoPlaceHolder.Padding = new Thickness(10);
            _infoPlaceHolder.TextAlignment = TextAlignment.Center;

            Binding widthBinding = new Binding("ActualWidth");
            widthBinding.Source = cursorContainer;
            _infoPlaceHolder.SetBinding(TextBlock.WidthProperty, widthBinding);

            Canvas.SetTop(_infoPlaceHolder, 0);
            Canvas.SetRight(_infoPlaceHolder, 0);
            cursorContainer.Children.Add(_infoPlaceHolder);
        }

        private void DisplayInfo(String info)
        {
            if (_infoPlaceHolder == null)
            {
                CreateInfoPlaceholder();
            }
            _infoPlaceHolder.Text = " " + info + " ";
            DoubleAnimation show = new DoubleAnimation(-30, 0, TimeSpan.FromMilliseconds(500));
            _infoPlaceHolder.BeginAnimation(Canvas.TopProperty, show);
        }

        private void HideInfo()
        {
            if (_infoPlaceHolder != null)
            {
                DoubleAnimation hide = new DoubleAnimation(0, -30, TimeSpan.FromMilliseconds(500));
                hide.Completed += (s, args) =>
                {
                    cursorContainer.Children.Remove(_infoPlaceHolder);
                    _infoPlaceHolder = null;
                };

                _infoPlaceHolder.BeginAnimation(Canvas.TopProperty, hide);

            }
        }

    }
}
