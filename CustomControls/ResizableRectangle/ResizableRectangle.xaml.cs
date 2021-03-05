using CustomControls.Core.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CustomControls.ResizableRectangle
{
    public sealed partial class ResizableRectangle : UserControl
    {
        public ResizableRectangle()
        {
            this.InitializeComponent();
        }

        private Point PointerPressedPoint;
        private Point PreviousRectanglePosition;
        private Point PreviousHandlePosition;

        Ellipse topLeft = new Ellipse { Height = 8, Width = 8, Fill = new SolidColorBrush(Colors.White), StrokeThickness = 2 };
        Ellipse topRight = new Ellipse { Height = 8, Width = 8, Fill = new SolidColorBrush(Colors.White), StrokeThickness = 2 };
        Ellipse bottomRight = new Ellipse { Height = 8, Width = 8, Fill = new SolidColorBrush(Colors.White), StrokeThickness = 2 };
        Ellipse bottomLeft = new Ellipse { Height = 8, Width = 8, Fill = new SolidColorBrush(Colors.White), StrokeThickness = 2 };

        public DoubleCollection BorderDashArray
        {
            get { return (DoubleCollection)GetValue(BorderDashArrayProperty); }
            set { SetValue(BorderDashArrayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderDashArray.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderDashArrayProperty =
            DependencyProperty.Register(nameof(BorderDashArray), typeof(DoubleCollection), typeof(ResizableRectangle), new PropertyMetadata((2, 1)));

        public Matrix CustomTransformMatrix
        {
            get { return (Matrix)GetValue(CustomTransformMatrixProperty); }
            private set { SetValue(CustomTransformMatrixProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TransformMatrix.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomTransformMatrixProperty =
            DependencyProperty.Register(nameof(CustomTransformMatrix), typeof(Matrix), typeof(ResizableRectangle), new PropertyMetadata(Matrix.Identity));

        private void Container_Loaded(object sender, RoutedEventArgs e)
        {
            PreviousRectanglePosition = new Point(0, 0);
            PreviousHandlePosition = new Point(0, 0);

            boundingRectangleTransform.TranslateX = 0;
            boundingRectangleTransform.TranslateY = 0;

            AddHandles();
        }

        private void AddHandles()
        {
            topLeft.Stroke = BorderBrush;
            topRight.Stroke = BorderBrush;
            bottomRight.Stroke = BorderBrush;
            bottomLeft.Stroke = BorderBrush;

            Canvas.SetLeft(topLeft, -2);
            Canvas.SetTop(topLeft, -2);

            Canvas.SetLeft(topRight, this.Width - 6);
            Canvas.SetTop(topRight, -2);

            Canvas.SetLeft(bottomRight, this.Width - 6);
            Canvas.SetTop(bottomRight, this.Height - 6);

            Canvas.SetLeft(bottomLeft, -2);
            Canvas.SetTop(bottomLeft, this.Height - 6);

            Canvas.SetZIndex(boundingRectangle, 1);
            Canvas.SetZIndex(topLeft, 2);
            Canvas.SetZIndex(topRight, 2);
            Canvas.SetZIndex(bottomRight, 2);
            Canvas.SetZIndex(bottomLeft, 2);

            boundingRectangle.PointerPressed += BoundingRectangle_PointerPressed;
            boundingRectangle.PointerReleased += BoundingRectangle_PointerReleased;
            boundingRectangle.PointerMoved += BoundingRectangle_PointerMoved;
            boundingRectangle.PointerExited += Handle_PointerExited;

            topLeft.PointerMoved += delegate (object sender, PointerRoutedEventArgs e) { Handle_PointerMoved(sender, e, ScaleHandlePosition.TopLeft); };
            topLeft.PointerExited += Handle_PointerExited;

            topRight.PointerMoved += delegate (object sender, PointerRoutedEventArgs e) { Handle_PointerMoved(sender, e, ScaleHandlePosition.TopRight); };
            topRight.PointerExited += Handle_PointerExited;

            bottomRight.PointerMoved += delegate (object sender, PointerRoutedEventArgs e) { Handle_PointerMoved(sender, e, ScaleHandlePosition.BottomRight); };
            bottomRight.PointerExited += Handle_PointerExited;
            bottomRight.PointerPressed += delegate (object sender, PointerRoutedEventArgs e) { ResizeHandle_PointerPressed(sender, e, ScaleHandlePosition.BottomRight); };
            bottomRight.PointerReleased += delegate (object sender, PointerRoutedEventArgs e) { ResizeHandle_PointerReleased(sender, e, ScaleHandlePosition.BottomRight); };

            bottomLeft.PointerMoved += delegate (object sender, PointerRoutedEventArgs e) { Handle_PointerMoved(sender, e, ScaleHandlePosition.BottomLeft); };
            bottomLeft.PointerExited += Handle_PointerExited;

            container.Children.Add(topLeft);
            container.Children.Add(topRight);
            container.Children.Add(bottomRight);
            container.Children.Add(bottomLeft);

            PreviousRectanglePosition = new Point(boundingRectangle.ActualOffset.X, boundingRectangle.ActualOffset.Y);
        }

        private void BoundingRectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var rectangle = sender as Rectangle;
            rectangle.CapturePointer(e.Pointer);

            PreviousRectanglePosition = new Point(rectangle.ActualOffset.X, rectangle.ActualOffset.Y);
            PointerPressedPoint = e.GetCurrentPoint(this).Position;
        }

        private void BoundingRectangle_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var rectangle = sender as Rectangle;

            PointerPressedPoint = new Point();
            rectangle.ReleasePointerCaptures();

            var newRectanglePosition = new Point(rectangle.ActualOffset.X, rectangle.ActualOffset.Y);
            var deltaX = newRectanglePosition.X - PreviousRectanglePosition.X;
            var deltaY = newRectanglePosition.Y - PreviousRectanglePosition.Y;

            var matrix = CustomTransformMatrix;
            matrix.OffsetX = deltaX;
            matrix.OffsetY = deltaY;

            CustomTransformMatrix = matrix;
        }

        private void BoundingRectangle_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var dragging = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
            
            if(PointerPressedPoint == new Point())
                PointerPressedPoint = e.GetCurrentPoint(this).Position;

            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeAll, 0);

            if (dragging)
            {
                var newPosition = e.GetCurrentPoint(this).Position;
                var deltaX = newPosition.X - PointerPressedPoint.X;
                var deltaY = newPosition.Y - PointerPressedPoint.Y;

                Canvas.SetLeft(topLeft, topLeft.ActualOffset.X + deltaX);
                Canvas.SetTop(topLeft, topLeft.ActualOffset.Y + deltaY);

                Canvas.SetLeft(topRight, topRight.ActualOffset.X + deltaX);
                Canvas.SetTop(topRight, topRight.ActualOffset.Y + deltaY);

                Canvas.SetLeft(bottomRight, bottomRight.ActualOffset.X + deltaX);
                Canvas.SetTop(bottomRight, bottomRight.ActualOffset.Y + deltaY);

                Canvas.SetLeft(bottomLeft, bottomLeft.ActualOffset.X + deltaX);
                Canvas.SetTop(bottomLeft, bottomLeft.ActualOffset.Y + deltaY);

                Canvas.SetLeft(boundingRectangle, boundingRectangle.ActualOffset.X + deltaX);
                Canvas.SetTop(boundingRectangle, boundingRectangle.ActualOffset.Y + deltaY);
                
                PointerPressedPoint = e.GetCurrentPoint(this).Position;
            }
        }

        private void ResizeHandle_PointerPressed(object sender, PointerRoutedEventArgs e, ScaleHandlePosition handlePosition)
        {
            var handle = sender as Ellipse;
            handle.CapturePointer(e.Pointer);

            switch(handlePosition)
            {
                case ScaleHandlePosition.BottomRight:
                    PreviousHandlePosition = new Point(boundingRectangle.ActualOffset.X + boundingRectangle.Width, boundingRectangle.ActualOffset.Y + boundingRectangle.Height);
                    break;
            }
            //PreviousHandlePosition = new Point(handle.ActualOffset.X, handle.ActualOffset.Y);
            PointerPressedPoint = e.GetCurrentPoint(this).Position;
        }

        private void ResizeHandle_PointerReleased(object sender, PointerRoutedEventArgs e, ScaleHandlePosition handlePosition)
        {
            var handle = sender as Ellipse;
            PointerPressedPoint = new Point();
            handle.ReleasePointerCaptures();

            var newHandlePosition = new Point(handle.ActualOffset.X, handle.ActualOffset.Y);
            var deltaX = newHandlePosition.X - PreviousHandlePosition.X;
            var deltaY = newHandlePosition.Y - PreviousHandlePosition.Y;

            var matrix = CustomTransformMatrix;
            matrix.M11 *= 1 + (deltaX / (boundingRectangle.Width));
            matrix.M22 *= 1 + (deltaY / (boundingRectangle.Height));

            CustomTransformMatrix = matrix;
        }

        private void Handle_PointerMoved(object sender, PointerRoutedEventArgs e, ScaleHandlePosition handlePosition)
        {
            var cursor = new CoreCursor(CoreCursorType.Arrow, 0);
            var dragging = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;

            if (PointerPressedPoint == new Point())
                PointerPressedPoint = e.GetCurrentPoint(this).Position;

            switch (handlePosition)
            {
                case ScaleHandlePosition.TopLeft:
                case ScaleHandlePosition.BottomRight:
                    cursor = new CoreCursor(CoreCursorType.SizeNorthwestSoutheast, 0);
                    break;
                case ScaleHandlePosition.TopRight:
                case ScaleHandlePosition.BottomLeft:
                    cursor = new CoreCursor(CoreCursorType.SizeNortheastSouthwest, 0);
                    break;
            }

            Window.Current.CoreWindow.PointerCursor = cursor;

            if (dragging)
            {
                var newPosition = e.GetCurrentPoint(this).Position;
                var deltaX = newPosition.X - PointerPressedPoint.X;
                var deltaY = newPosition.Y - PointerPressedPoint.Y;

                switch (handlePosition)
                {
                    case ScaleHandlePosition.BottomRight:
                        Canvas.SetLeft(topRight, topRight.ActualOffset.X + deltaX);

                        Canvas.SetLeft(bottomRight, bottomRight.ActualOffset.X + deltaX);
                        Canvas.SetTop(bottomRight, bottomRight.ActualOffset.Y + deltaY);

                        Canvas.SetTop(bottomLeft, bottomLeft.ActualOffset.Y + deltaY);

                        boundingRectangle.Width += deltaX;
                        boundingRectangle.Height += deltaY;
                        break;
                }

                PointerPressedPoint = e.GetCurrentPoint(this).Position;
            }
        }

        private void Handle_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                return;

            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
            //PointerPressedPoint = new Point();
        }
    }
}
