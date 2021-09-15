using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using rectangle_manipulation.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace rectangle_manipulation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private InkCanvas InkingCanvas = new InkCanvas();
        private CanvasControl InkingCanvasControl = new CanvasControl();

        private List<InkStroke> SoWetInkStrokes = new List<InkStroke>();

        public MainPage()
        {
            this.InitializeComponent();

            Init();
        }

        private void Init()
        {
            InkingCanvas.Width = RootCanvas.Width;
            InkingCanvas.Height = RootCanvas.Height;

            InkingCanvasControl.Width = RootCanvas.Width;
            InkingCanvasControl.Height = RootCanvas.Height;
            InkingCanvasControl.DpiScale = ScrollViewer.MaxZoomFactor;

            InkingCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Touch;
            InkingCanvas.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Inking;
            //InkingCanvas.InkPresenter.ActivateCustomDrying();
            InkingCanvas.InkPresenter.UpdateDefaultDrawingAttributes(
                new InkDrawingAttributes
                {
                    IgnorePressure = false,
                    FitToCurve = true,
                    Color = Colors.Brown,
                    PenTip = PenTipShape.Circle
                });

            InkingCanvas.InkPresenter.StrokesCollected += OnInkStrokesCollected;
            //InkingCanvas.InkPresenter.StrokesErased += OnInkStrokesErased;

            InkingCanvasControl.ClearColor = Colors.Transparent;
            InkingCanvasControl.Draw += OnInkingCanvasControlDraw;
            InkingCanvasControl.CompositeMode = ElementCompositeMode.MinBlend;

            Viewport.Children.Add(InkingCanvasControl);
            Viewport.Children.Add(InkingCanvas);

            Canvas.SetZIndex(RootCanvas, 0);
            Canvas.SetZIndex(InkingCanvasControl, 5);
            Canvas.SetZIndex(InkingCanvas, 10);
        }

        private void OnInkStrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            var strokes = args.Strokes.ToList();
            strokes.ForEach(x => { SoWetInkStrokes.Add(x); });
        }

        private CanvasRenderTarget RenderTarget;
        private List<InkStroke> NotSoWetInkStrokes = new List<InkStroke>();

        private void OnInkingCanvasControlDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {

            RenderTarget = new CanvasRenderTarget(InkingCanvasControl, (float)InkingCanvasControl.Size.Width, (float)InkingCanvasControl.Size.Height, 256);

            using (var drawingSession = RenderTarget.CreateDrawingSession())
                drawingSession.Clear(Colors.Transparent);

            DrawNotSoWetInkStrokes();

            args.DrawingSession.DrawImage(RenderTarget);=

            RenderBoundary();
        }

        private void DrawNotSoWetInkStrokes()
        {
            using (var drawingSession = RenderTarget.CreateDrawingSession())
            {
                drawingSession.DrawInk(NotSoWetInkStrokes);
            }
        }

        private void CanAddSoWetStrokesToNotSoWetStrokes(object sender, RoutedEventArgs e)
        {
            ClearInkCanvasControl();

            if (SoWetInkStrokes.Count == 0)
                return;

            InkStroke[] soWetInkStrokes = new InkStroke[SoWetInkStrokes.Count];
            SoWetInkStrokes.CopyTo(soWetInkStrokes);
            NotSoWetInkStrokes.AddRange(soWetInkStrokes.ToList());

            InkingCanvas.InkPresenter.StrokeContainer.Clear();
            SoWetInkStrokes.Clear();

            InkingCanvasControl.Invalidate();
        }

        private void RenderBoundary()
        {
            if (NotSoWetInkStrokes.Count == 0)
                return;

            Canvas.SetZIndex(RootCanvas, 10);
            Canvas.SetZIndex(InkingCanvasControl, 5);
            Canvas.SetZIndex(InkingCanvas, 0);

            InkStroke[] notSoWetInkStrokesCopy = new InkStroke[NotSoWetInkStrokes.Count];
            int i = 0;
            NotSoWetInkStrokes.ForEach(s => notSoWetInkStrokesCopy[i++] = s.Clone());

            var inkContainer = new InkStrokeContainer();
            inkContainer.AddStrokes(notSoWetInkStrokesCopy.ToList());

            var inkPointTransform = notSoWetInkStrokesCopy[0].PointTransform;
            var topLeft = new Point(inkContainer.BoundingRect.Left, inkContainer.BoundingRect.Top);

            var Boundary = new ResizableRectangle { Name = "Boundary", MaxWidth = RootCanvas.Width, MaxHeight = RootCanvas.Height };
            var transformGroup = new TransformGroup();

            transformGroup.Children.Add(new TranslateTransform { X = topLeft.X, Y = topLeft.Y });
            transformGroup.Children.Add(new ScaleTransform());

            Boundary.BoundaryTransform = transformGroup;
            Boundary.ParentZoomFactor = ScrollViewer.ZoomFactor;
            Boundary.Width = inkContainer.BoundingRect.Width;
            Boundary.Height = inkContainer.BoundingRect.Height;
            Boundary.Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
            Boundary.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 99, 226, 255));
            Boundary.BorderThickness = new Thickness(2d);
            Boundary.CustomTransformMatrix = inkPointTransform;
            Boundary.CustomTransformMatrixChanged += BoundaryTransformMatrixChanged;
            Boundary.UpdateLayout();

            RootCanvas.Children.Add(Boundary);
        }

        private void BoundaryTransformMatrixChanged(object sender, PropertyChangedEventArgs e)
        {
            var matrix = (sender as ResizableRectangle).CustomTransformMatrix;

            if (NotSoWetInkStrokes.Count == 0)
                return;

            var transformedInkStrokes = new List<InkStroke>();

            foreach (var stroke in NotSoWetInkStrokes)
            {
                var inkPoints = stroke.GetInkPoints();
                var strokeBuilder = new InkStrokeBuilder();

                strokeBuilder.SetDefaultDrawingAttributes(stroke.DrawingAttributes);
                var points = new List<Point>();
                foreach (var inkPoint in inkPoints)
                {
                    var _matrix = new Matrix(matrix.M11, matrix.M12, matrix.M21, matrix.M22, matrix.M31, matrix.M32);
                    points.Add(_matrix.Transform(inkPoint.Position));
                }

                var transformedStroke = strokeBuilder.CreateStroke(points);

                transformedInkStrokes.Add(transformedStroke);
            }

            NotSoWetInkStrokes.Clear();
            NotSoWetInkStrokes.AddRange(transformedInkStrokes);

            ClearCanvas();
            InkingCanvasControl.Invalidate();
        }

        private void OnScrollViewerViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var boundary = RootCanvas.Children.Where(x => x.GetType() == typeof(ResizableRectangle)).FirstOrDefault() as ResizableRectangle;

            if (boundary is null)
                return;

            boundary.ParentZoomFactor = (sender as ScrollViewer).ZoomFactor;
        }

        private void ClearInkCanvasControl()
        {
            NotSoWetInkStrokes.Clear();
            InkingCanvasControl.Invalidate();

            ClearCanvas();

            Canvas.SetZIndex(RootCanvas, 0);
            Canvas.SetZIndex(InkingCanvasControl, 5);
            Canvas.SetZIndex(InkingCanvas, 10);
        }

        private void OnClearInkClicked(object sender, RoutedEventArgs e) => ClearInkCanvasControl();

        private void ClearCanvas() => RootCanvas.Children.Clear();
    }
}
