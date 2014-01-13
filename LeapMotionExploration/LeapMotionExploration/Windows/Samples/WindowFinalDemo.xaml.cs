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

        private int _mainHand;

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
        private Boolean _isTakingSnapshot;

        //info place holder
        private TextBlock _infoPlaceHolder;

        private MultiBinding _mbCanvasTop;
        private Shape _currentShapeCandidate;
        private Point _currentShapeCandidateSpawn;

        private Point _currentCursorPoint;

        public WindowFinalDemo(int mainHand)
        {
            InitializeComponent();

            initMainHand(mainHand);

            _isDragging = false;
            _currentCursorPoint = new Point(0, 0);
            _originalGraphicElementPoint = new Point(0, 0);
            _startCursorPoint = new Point(0, 0);

            _controller = new Controller();

            _cursorListener = new LeapListenerOneHandPosition(_mainHand);
            _isCursorPositionTracked = true;
            _controller.AddListener(_cursorListener);
            _cursorListener.OnStateChange += this.OnPositionChange;

            _handCloseListener = new LeapListenerOneHandClose(_mainHand);
            _controller.AddListener(_handCloseListener);
            _handCloseListener.OnHandStateChange += this.OnHandClosed;

            _rotatingSelectionListener = new LeapListenerRotateSelection(_mainHand);
            _controller.AddListener(_rotatingSelectionListener);
            _rotatingSelectionListener.OnStateChange += rotationSelectionEvent;

            _shapeManipulationListener = new LeapListenerTwoHandManipulation(_mainHand);
            _controller.AddListener(_shapeManipulationListener);
            _shapeManipulationListener.OnStateChange += this.OnShapeManipulationEvent;

            _snapshotSaverListener = new LeapListenerClap();
            _controller.AddListener(_snapshotSaverListener);
            _snapshotSaverListener.OnClapDetected += this.OnSnapshotSaveEvent;
            _isTakingSnapshot = false;

            _mnShapePickerItems = new TextBlock[] { mnShapePickerRectangle, mnShapePickerCircle, mnShapePickerEllipse, mnShapePickerTriangle };
            _currentShapePickerItemIndex = 0;

            _mnColorPickerItems = new TextBlock[] { mnColorPickerBlue, mnColorPickerPurple, mnColorPickerGreen, mnColorPickerOrange, mnColorPickerRed };
            _currentColorPickerItemIndex = 0;

            _graphicElements = new List<FrameworkElement>();
            _staticGraphicElements = new List<FrameworkElement>();

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

            _staticGraphicElements.Add(colorPicker);
            _staticGraphicElements.Add(shapePicker);

            selectShapeItem(_currentShapePickerItemIndex);
            selectColorItem(_currentColorPickerItemIndex);
        }

        private void initMainHand(int mainHand)
        {
            _mainHand = mainHand;
            adaptUIToMainHand();
        }

        private void setMainHand(int mainHand)
        {
            if (!mainHand.Equals(_mainHand))
            {
                _mainHand = mainHand;
                adaptUIToMainHand();
            }
            
        }

        private void adaptUIToMainHand()
        {
            DependencyProperty canvasPropertyToClean = Canvas.RightProperty;
            DependencyProperty canvasPropertyToSet = Canvas.LeftProperty;
            Point renderTransformOriginForChildren = new Point(-0.8, 0.5);
            Point renderTransformOriginForParent = new Point(-1, 0.5);

            if (_mainHand.Equals(LeapUtils.RIGHT_MOST_HAND))
            {
                canvasPropertyToClean = Canvas.LeftProperty;
                canvasPropertyToSet = Canvas.RightProperty;

                renderTransformOriginForChildren.X += 3;
                renderTransformOriginForParent.X += 3;
            }

            colorPickerBackground.ClearValue(canvasPropertyToClean);
            colorPickerBackground.SetValue(canvasPropertyToSet, 40d);

            colorPicker.ClearValue(canvasPropertyToClean);
            colorPicker.SetValue(canvasPropertyToSet, 50d);

            shapePickerBackground.ClearValue(canvasPropertyToClean);
            shapePickerBackground.SetValue(canvasPropertyToSet, 40d);

            shapePicker.ClearValue(canvasPropertyToClean);
            shapePicker.SetValue(canvasPropertyToSet, 50d);

            menuBackground.ClearValue(canvasPropertyToClean);
            menuBackground.SetValue(canvasPropertyToSet, -245d);

            mnColorPicker.ClearValue(canvasPropertyToClean);
            mnColorPicker.SetValue(canvasPropertyToSet, 200d);

            mnShapePicker.ClearValue(canvasPropertyToClean);
            mnShapePicker.SetValue(canvasPropertyToSet, 200d);

            foreach(UIElement colorElement in mnColorPicker.Children)
            {
                colorElement.RenderTransformOrigin = renderTransformOriginForChildren;
            }

            mnColorPicker.RenderTransformOrigin = renderTransformOriginForParent; 
           
            foreach(UIElement shapeElement in mnShapePicker.Children)
            {
                shapeElement.RenderTransformOrigin = renderTransformOriginForChildren;
            }

            mnShapePicker.RenderTransformOrigin = renderTransformOriginForParent;

            basket.ClearValue(canvasPropertyToSet);
            basket.SetValue(canvasPropertyToClean, -100d);

            basketIcon.ClearValue(canvasPropertyToSet);
            basketIcon.SetValue(canvasPropertyToClean, 0d);
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

                case 3:
                    //Triangle
                    _currentShapeCandidate = CreateTriangle();
                    _currentShapeCandidate.Width = DEFAULT_WIDTH;
                    _currentShapeCandidate.Height = DEFAULT_HEIGHT;
                    break;
            }

            if (_currentShapeCandidate != null)
            {
                _currentShapeCandidate.Fill = _mnColorPickerItems[_currentColorPickerItemIndex].Background;

                _currentShapeCandidate.SetBinding(Canvas.TopProperty, _mbCanvasTop);

                if (_mainHand.Equals(LeapUtils.RIGHT_MOST_HAND))
                {
                    _currentShapeCandidate.SetValue(Canvas.RightProperty, SHAPE_CANDIDATE_LEFT_MARGIN);
                } else
                {
                    _currentShapeCandidate.SetValue(Canvas.LeftProperty, SHAPE_CANDIDATE_LEFT_MARGIN);
                }
                

                cursorContainer.Children.Add(_currentShapeCandidate);
                _graphicElements.Add(_currentShapeCandidate);

            }

        }

        /**
         * Snapshot Saver
         * 
         * OnSnapshotSaveEvent(LeapEvent leapEvent)
         * TakeSnapshot(Canvas canvas)
         * MakeSnapshotEffect(Canvas canvas, BitmapSource snapshotSource)
         */
        private void OnSnapshotSaveEvent(LeapEvent leapEvent)
        {
            if (!_isTakingSnapshot)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    BitmapSource snapshotSource = TakeSnapshot(cursorContainer);
                    MakeSnapshotEffect(cursorContainer, snapshotSource);

                }));
            }
            
        }

        private BitmapSource TakeSnapshot(Canvas canvas)
        {
            _isTakingSnapshot = true;
            RenderTargetBitmap targetBitmap = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96d, 96d, PixelFormats.Default);
            targetBitmap.Render(canvas);            

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(targetBitmap));

            FileStream file = File.Open(String.Format(SCREENSHOT_FILE_NAME_FORMAT, DateTime.Now), FileMode.OpenOrCreate);
            using (file)
            {
                encoder.Save(file);
            }
            PopInfo("Screenshot enregistré sous : " + file.Name);

            return targetBitmap;
        }

        private void MakeSnapshotEffect(Canvas canvas, BitmapSource snapshotSource)
        {
            Border snapshotPreview = new Border();
            snapshotPreview.RenderTransform = new ScaleTransform(1, 1, 0.5, 0.5);
            snapshotPreview.RenderTransformOrigin = new Point(0.5, 0.5);
            snapshotPreview.Child = new Image() { Source = snapshotSource };
            snapshotPreview.Background = new SolidColorBrush(Colors.DarkGray);
            snapshotPreview.SetBinding(Border.WidthProperty, new Binding("ActualWidth") { Source = canvas });
            snapshotPreview.SetBinding(Border.HeightProperty, new Binding("ActualHeight") { Source = canvas });            
            snapshotPreview.Padding = new Thickness(5,10,5,10);
            Canvas.SetZIndex(snapshotPreview, int.MaxValue);
            canvas.Children.Add(snapshotPreview);            

            Storyboard snapshotStoryboard = new Storyboard();
            snapshotStoryboard.Completed += delegate { canvas.Children.Remove(snapshotPreview); _isTakingSnapshot = false;};            

            //Fade Out
            DoubleAnimation fadeOutAnimation = new DoubleAnimation() {
                From = 1,
                To = 0,
                BeginTime = TimeSpan.FromSeconds(1.1),
                Duration = new Duration(TimeSpan.FromSeconds(0.8))
            };
            snapshotStoryboard.Children.Add(fadeOutAnimation);
            Storyboard.SetTarget(fadeOutAnimation, snapshotPreview);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(Border.OpacityProperty));

            //Scale X Down
            DoubleAnimation scaleXDownAnimation = new DoubleAnimation()
            {
                From = 1,
                To = 0.75,
                Duration = new Duration(TimeSpan.FromSeconds(0.2))
            };
            snapshotStoryboard.Children.Add(scaleXDownAnimation);
            Storyboard.SetTarget(scaleXDownAnimation, snapshotPreview);
            Storyboard.SetTargetProperty(scaleXDownAnimation, new PropertyPath("RenderTransform.ScaleX"));
            
            //Scale Y Down
            DoubleAnimation scaleYDownAnimation = new DoubleAnimation()
            {
                From = 1,
                To = 0.75,
                Duration = new Duration(TimeSpan.FromSeconds(0.2))
            };
            snapshotStoryboard.Children.Add(scaleYDownAnimation);
            Storyboard.SetTarget(scaleYDownAnimation, snapshotPreview);
            Storyboard.SetTargetProperty(scaleYDownAnimation, new PropertyPath("RenderTransform.ScaleY"));

            snapshotStoryboard.Begin(this);
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

                case 3:
                    //Triangle
                    newShape = CreateTriangle();
                    newShape.Width = 100;
                    newShape.Height = 100;
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

        private Shape CreateTriangle()
        {
            Polygon triangle = new Polygon();
            PointCollection trianglePoints = new PointCollection();
            trianglePoints.Add(new Point(1, 1));
            trianglePoints.Add(new Point(0, 2));
            trianglePoints.Add(new Point(2, 2));
            triangle.Points = trianglePoints;
            triangle.Stretch = Stretch.Fill;
            return triangle;
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
                if (double.IsNaN(_originalGraphicElementPoint.X))
                {
                    _originalGraphicElementPoint.X = cursorContainer.ActualWidth - Canvas.GetRight(_hoveredGraphicElement) - _hoveredGraphicElement.ActualWidth;
                }
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
                    _currentShapeCandidateSpawn = new Point(_originalGraphicElementPoint.X + _currentShapeCandidate.ActualWidth / 2, _originalGraphicElementPoint.Y + _currentShapeCandidate.ActualHeight / 2);
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
                    if (_mainHand.Equals(LeapUtils.RIGHT_MOST_HAND))
                    {
                        _currentShapeCandidate.ClearValue(Canvas.LeftProperty);
                        _currentShapeCandidate.SetValue(Canvas.RightProperty, SHAPE_CANDIDATE_LEFT_MARGIN);
                    }
                    else
                    {
                        _currentShapeCandidate.SetValue(Canvas.LeftProperty, SHAPE_CANDIDATE_LEFT_MARGIN);
                    }
                    
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

        /**
         * Create a TextBlock for the Info placeholder
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
            _infoPlaceHolder.TextWrapping = TextWrapping.Wrap;

            Binding widthBinding = new Binding("ActualWidth");
            widthBinding.Source = cursorContainer;
            _infoPlaceHolder.SetBinding(TextBlock.WidthProperty, widthBinding);

            Canvas.SetTop(_infoPlaceHolder, 0);
            Canvas.SetRight(_infoPlaceHolder, 0);
            cursorContainer.Children.Add(_infoPlaceHolder);
        }

        /**
         * Pop some text and hide it after a while
         */  
        private void PopInfo(String info)
        {

            DisplayInfo(info);
            DoubleAnimation fadeOut = new DoubleAnimation()
            {
                From = 1,
                To = 0.75,
                BeginTime = TimeSpan.FromSeconds(2),
                Duration = new Duration(TimeSpan.FromMilliseconds(500))
            };
            fadeOut.Completed += delegate { HideInfo(); };
            _infoPlaceHolder.BeginAnimation(TextBlock.OpacityProperty, fadeOut);           
        }

        /**
         * Display some text, don't forget to call HideInfo when needed
         */ 
        private void DisplayInfo(String info)
        {
            if (_infoPlaceHolder == null)
            {
                CreateInfoPlaceholder();
            }
            _infoPlaceHolder.Text = " " + info + " ";
            _infoPlaceHolder.BeginAnimation(Canvas.TopProperty, CreateVerticalSlideAnimation(-30,0));
        }

        /**
         * Hide info placeholder
         */ 
        private void HideInfo()
        {
            if (_infoPlaceHolder != null)
            {
                DoubleAnimation hide = CreateVerticalSlideAnimation(0, -60);
                hide.Completed += (s, args) =>
                {
                    cursorContainer.Children.Remove(_infoPlaceHolder);
                    _infoPlaceHolder = null;
                };
                _infoPlaceHolder.BeginAnimation(Canvas.TopProperty, hide);

            }
        }

        private DoubleAnimation CreateVerticalSlideAnimation(double from, double to)
        {
            return new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromMilliseconds(500))
            };
        }

    }
}
