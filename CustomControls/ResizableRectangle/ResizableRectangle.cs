using CustomControls.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CustomControls.ResizableRectangle
{
    public sealed class ResizableRectangle : Control
    {
        public ScaleHandlePosition ActiveScaleHandle { get; private set; }
        public Point ActiveScaleHandlePosition { get; private set; }
        private Point OriginalPointerPressedPosition;
        private Rectangle OriginalBoundary;
        private Dictionary<ScaleHandlePosition, UIElement> ScaleHandles { get; } = new Dictionary<ScaleHandlePosition, UIElement>();
        public Matrix CustomTransformMatrix { get; private set; }
        //public Rect Rect { get; private set; }

        public ResizableRectangle()
        {
            this.DefaultStyleKey = typeof(ResizableRectangle);
        }

        public double Top
        {
            get { return (double)GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Top.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TopProperty =
            DependencyProperty.Register(nameof(Top), typeof(double), typeof(ResizableRectangle), new PropertyMetadata(0d));

        public double Right
        {
            get { return (double)GetValue(RightProperty); }
            set { SetValue(RightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Right.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightProperty =
            DependencyProperty.Register(nameof(Right), typeof(double), typeof(ResizableRectangle), new PropertyMetadata(0d));

        public double Bottom
        {
            get { return (double)GetValue(BottomProperty); }
            set { SetValue(BottomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bottom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BottomProperty =
            DependencyProperty.Register(nameof(Bottom), typeof(double), typeof(ResizableRectangle), new PropertyMetadata(0d));

        public double Left
        {
            get { return (double)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Left.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftProperty =
            DependencyProperty.Register(nameof(Left), typeof(double), typeof(ResizableRectangle), new PropertyMetadata(0d));

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Stroke.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(ResizableRectangle), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StrokeThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(ResizableRectangle), new PropertyMetadata(4d));

        public (double Width, double Height) Overlay
        {
            get { return ((double Width, double Height))GetValue(OverlayProperty); }
            set { SetValue(OverlayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Overlay.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OverlayProperty =
            DependencyProperty.Register("Overlay", typeof((double Width, double Height)), typeof(ResizableRectangle), new PropertyMetadata((0d, 0d)));

        protected override void OnApplyTemplate()
        {
            Width = Math.Abs(Left - Right) + 2;
            Height = Math.Abs(Bottom - Top) + 2;

            var overlay = GetTemplateChild("Overlay") as Canvas;
            overlay.Width = 617d;
            overlay.Height = 792d;

            InitScaleHandles();
            BindBoundaryEvents();

            base.OnApplyTemplate();
        }

        private void BindBoundaryEvents()
        {
            var boundary = GetTemplateChild("Boundary") as Rectangle;
            if (boundary == null)
                return;

            var boundaryTransform = GetTemplateChild("BoundaryTransform") as CompositeTransform;
            boundaryTransform.ScaleX = 1;
            boundaryTransform.ScaleY = 1;
            boundaryTransform.TranslateX = Left - 1d;
            boundaryTransform.TranslateY = Top - 1d;

            OriginalBoundary = new Rectangle
            {
                Width = boundary.Width,
                Height = boundary.Height
            };

            boundary.PointerEntered += delegate (object sender, PointerRoutedEventArgs e)
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeAll, 0);
            };

            boundary.PointerExited += delegate (object sender, PointerRoutedEventArgs e)
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
            };

            //boundary.ManipulationDelta += delegate (object sender, ManipulationDeltaRoutedEventArgs e)
            //{
            //    boundaryTransform.TranslateX += e.Cumulative.Translation.X;
            //    boundaryTransform.TranslateY += e.Cumulative.Translation.Y;
            //};

            //boundary.ManipulationCompleted += delegate (object sender, ManipulationCompletedRoutedEventArgs e)
            //{
            //    (sender as Rectangle).ReleasePointerCaptures();
            //    OriginalPointerPressedPosition = new Point();

            //    // var transform = GetTemplateChild("BoundaryTransform") as CompositeTransform;

            //    if (boundaryTransform == null)
            //        return;

            //    CustomTransformMatrix = new Matrix
            //    {
            //        M11 = boundaryTransform.ScaleX,
            //        M12 = 0,
            //        M21 = 0,
            //        M22 = boundaryTransform.ScaleY,
            //        OffsetX = boundaryTransform.TranslateX + 1,
            //        OffsetY = boundaryTransform.TranslateY + 1
            //    };
            //};

            boundary.PointerPressed += delegate (object sender, PointerRoutedEventArgs e)
            {
                OriginalPointerPressedPosition = e.GetCurrentPoint(this).Position;
                (sender as Rectangle).CapturePointer(e.Pointer);
                ActiveScaleHandle = ScaleHandlePosition.None;
            };
            boundary.PointerMoved += delegate (object sender, PointerRoutedEventArgs e) { ProcessBoundaryMovement((sender as Rectangle), e); };
            boundary.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
            {
                var handle = sender as Rectangle;
                handle.ReleasePointerCaptures();
                OriginalPointerPressedPosition = new Point();

                // var transform = GetTemplateChild("BoundaryTransform") as CompositeTransform;

                if (boundaryTransform == null)
                    return;

                CustomTransformMatrix = new Matrix
                {
                    M11 = boundaryTransform.ScaleX,
                    M12 = 0,
                    M21 = 0,
                    M22 = boundaryTransform.ScaleY,
                    OffsetX = boundaryTransform.TranslateX + 1 - handle.RenderTransformOrigin.X,
                    OffsetY = boundaryTransform.TranslateY + 1 - handle.RenderTransformOrigin.Y
                };
            };
        }

        private void MoveScaleHandlesWithBoundary(double deltaX, double deltaY)
        {
            foreach (var element in ScaleHandles)
            {
                var handle = element.Value as Ellipse;

                (handle.RenderTransform as TranslateTransform).X += deltaX;
                (handle.RenderTransform as TranslateTransform).Y += deltaY;
            }
        }

        private void ProcessBoundaryMovement(Rectangle boundary, PointerRoutedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || ActiveScaleHandle != ScaleHandlePosition.None)
                return;

            if (OriginalPointerPressedPosition == new Point())
                OriginalPointerPressedPosition = e.GetCurrentPoint(this).Position;

            var currentPointerPosition = e.GetCurrentPoint(this).Position;
            var deltaX = currentPointerPosition.X - OriginalPointerPressedPosition.X;
            var deltaY = currentPointerPosition.Y - OriginalPointerPressedPosition.Y;

            var boundaryTransform = GetTemplateChild("BoundaryTransform") as CompositeTransform;
            boundaryTransform.TranslateX += deltaX;
            boundaryTransform.TranslateY += deltaY;

            MoveScaleHandlesWithBoundary(deltaX, deltaY);

            OriginalPointerPressedPosition = currentPointerPosition;
        }

        private void InitScaleHandles()
        {
            //var topLeftHandle = GetTemplateChild("TopLeftHandle") as Ellipse;
            //topLeftHandle.RenderTransform = new TranslateTransform { X = Left - 4d, Y = Top - 4d };

            //var topRightHandle = GetTemplateChild("TopRightHandle") as Ellipse;
            //topRightHandle.RenderTransform = new TranslateTransform { X = Right - 5d, Y = Top - 4d };

            var bottomRightHandle = GetTemplateChild("BottomRightHandle") as Ellipse;
            bottomRightHandle.RenderTransformOrigin = new Point(Right, Bottom);
            bottomRightHandle.RenderTransform = new TranslateTransform { X = Right - 5d, Y = Bottom - 5d };

            //var bottomLeftHandle = GetTemplateChild("BottomLeftHandle") as Ellipse;
            //bottomLeftHandle.RenderTransform = new TranslateTransform { X = Left - 5d, Y = Bottom - 5d };

            //ScaleHandles.Add(ScaleHandlePosition.TopLeft, topLeftHandle);
            //ScaleHandles.Add(ScaleHandlePosition.TopRight, topRightHandle);
            ScaleHandles.Add(ScaleHandlePosition.BottomRight, bottomRightHandle);
            //ScaleHandles.Add(ScaleHandlePosition.BottomLeft, bottomLeftHandle);

            foreach (var element in ScaleHandles)
            {
                var handle = element.Value as Ellipse;

                handle.PointerEntered += delegate (object sender, PointerRoutedEventArgs e)
                {
                    (sender as Ellipse).CapturePointer(e.Pointer);
                    ChangeScaleHandleCursor(element.Key);
                };
                handle.PointerPressed += delegate (object sender, PointerRoutedEventArgs e) { ActiveScaleHandle = element.Key; };
                handle.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
                {
                    ProcessBoundaryScaling();
                    (sender as Ellipse).ReleasePointerCaptures();
                };
                handle.PointerExited += delegate (object sender, PointerRoutedEventArgs e)
                {
                    ChangeScaleHandleCursor();
                    (sender as Ellipse).ReleasePointerCaptures();
                };
            }
        }

        private void ProcessBoundaryScaling()
        {
            OriginalPointerPressedPosition = new Point();

            var boundary = GetTemplateChild("Boundary") as Rectangle;
            var transform = GetTemplateChild("BoundaryTransform") as CompositeTransform;

            if (transform == null)
                return;

            var scaleX = 1 - (-(boundary.Width - OriginalBoundary.Width) / OriginalBoundary.Width);
            var scaleY = 1 - (-(boundary.Height - OriginalBoundary.Height) / OriginalBoundary.Height);

            CustomTransformMatrix = new Matrix
            {
                M11 = scaleX,
                M12 = 0,
                M21 = 0,
                M22 = scaleY,
                OffsetX = transform.TranslateX + 1 - boundary.RenderTransformOrigin.X,
                OffsetY = transform.TranslateY + 1 - boundary.RenderTransformOrigin.Y
            };

            OriginalBoundary = new Rectangle
            {
                Width = boundary.Width,
                Height = boundary.Height
            };
        }

        private void ChangeScaleHandleCursor(ScaleHandlePosition handlePosition = ScaleHandlePosition.None)
        {
            switch (handlePosition)
            {
                case ScaleHandlePosition.TopLeft:
                case ScaleHandlePosition.BottomRight:
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeNorthwestSoutheast, 0);
                    break;
                case ScaleHandlePosition.TopRight:
                case ScaleHandlePosition.BottomLeft:
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeNortheastSouthwest, 0);
                    break;
                default:
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
                    break;
            }

            ActiveScaleHandle = handlePosition;
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            this.CapturePointer(e.Pointer);
            base.OnPointerPressed(e);
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            var pointerPressed = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
            var boundary = GetTemplateChild("Boundary") as Rectangle;
            var boundaryTransform = GetTemplateChild("BoundaryTransform") as CompositeTransform;

            if (pointerPressed && ActiveScaleHandle != ScaleHandlePosition.None)
            {
                if (OriginalPointerPressedPosition == new Point())
                    OriginalPointerPressedPosition = e.GetCurrentPoint(this).Position;

                var currentPointerPosition = e.GetCurrentPoint(this).Position;
                var deltaX = currentPointerPosition.X - OriginalPointerPressedPosition.X;
                var deltaY = currentPointerPosition.Y - OriginalPointerPressedPosition.Y;

                var handle = ScaleHandles[ActiveScaleHandle] as Ellipse;
                (handle.RenderTransform as TranslateTransform).X += deltaX;
                (handle.RenderTransform as TranslateTransform).Y += deltaY;

                switch (ActiveScaleHandle)
                {
                    case ScaleHandlePosition.TopLeft:
                        boundaryTransform.TranslateX += deltaX;
                        boundaryTransform.TranslateY += deltaY;

                        (ScaleHandles[ScaleHandlePosition.TopRight].RenderTransform as TranslateTransform).Y += deltaY;
                        (ScaleHandles[ScaleHandlePosition.BottomLeft].RenderTransform as TranslateTransform).X += deltaX;

                        boundary.Width += deltaX > 0 ? -deltaX : Math.Abs(deltaX);
                        boundary.Height += deltaY > 0 ? -deltaY : Math.Abs(deltaY);
                        break;
                    case ScaleHandlePosition.TopRight:
                        boundaryTransform.TranslateY += deltaY;

                        (ScaleHandles[ScaleHandlePosition.TopLeft].RenderTransform as TranslateTransform).Y += deltaY;
                        (ScaleHandles[ScaleHandlePosition.BottomRight].RenderTransform as TranslateTransform).X += deltaX;

                        boundary.Width += deltaX;
                        boundary.Height += deltaY > 0 ? -deltaY : Math.Abs(deltaY);
                        break;
                    case ScaleHandlePosition.BottomLeft:
                        boundaryTransform.TranslateX += deltaX;

                        (ScaleHandles[ScaleHandlePosition.TopLeft].RenderTransform as TranslateTransform).X += deltaX;
                        (ScaleHandles[ScaleHandlePosition.BottomRight].RenderTransform as TranslateTransform).Y += deltaY;

                        boundary.Width += deltaX > 0 ? -deltaX : Math.Abs(deltaX);
                        boundary.Height += deltaY;
                        break;
                    case ScaleHandlePosition.BottomRight:
                        //(ScaleHandles[ScaleHandlePosition.TopRight].RenderTransform as TranslateTransform).X += deltaX;
                        //(ScaleHandles[ScaleHandlePosition.BottomLeft].RenderTransform as TranslateTransform).Y += deltaY;

                        boundary.Width += deltaX;
                        boundary.Height += deltaY;
                        break;
                }

                OriginalPointerPressedPosition = currentPointerPosition;
            }

            base.OnPointerMoved(e);
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            this.ReleasePointerCaptures();
            base.OnPointerReleased(e);
        }
    }
}
