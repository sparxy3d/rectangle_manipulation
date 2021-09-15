using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

namespace rectangle_manipulation.Controls
{
    public sealed partial class ResizableRectangle : UserControl, INotifyPropertyChanged
    {
        private Matrix3x2 customTransformMatrix = Matrix3x2.Identity;
        //private Matrix3x2 initialTransformMatrix = Matrix3x2.Identity;

        public TransformGroup BoundaryTransform
        {
            get { return (TransformGroup)GetValue(BoundaryTransformProperty); }
            set { SetValue(BoundaryTransformProperty, value); }
        }

        public static readonly DependencyProperty BoundaryTransformProperty =
            DependencyProperty.Register(nameof(BoundaryTransform), typeof(TransformGroup), typeof(ResizableRectangle), new PropertyMetadata(null));

        public float ParentZoomFactor
        {
            get { return (float)GetValue(ParentZoomFactorProperty); }
            set { SetValue(ParentZoomFactorProperty, value); }
        }

        public static readonly DependencyProperty ParentZoomFactorProperty =
            DependencyProperty.Register(nameof(ParentZoomFactor), typeof(float), typeof(ResizableRectangle), new PropertyMetadata(1));

        public Matrix3x2 CustomTransformMatrix
        {
            get { return customTransformMatrix; }
            set { if (value != customTransformMatrix) { customTransformMatrix = value; OnCustomTransformMatrixChanged(); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler CustomTransformMatrixChanged;

        private void OnCustomTransformMatrixChanged()
        {
            PropertyChangedEventHandler handler = CustomTransformMatrixChanged;

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(nameof(CustomTransformMatrix)));
        }

        public ResizableRectangle()
        {
            this.InitializeComponent();

            SetBaseOrigin();
        }

        private void SetBaseOrigin()
        {
            BaseContainer.RenderTransform = new TranslateTransform { X = 0, Y = 0 };
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            SetBaseOrigin();

            if (BoundaryTransform != null)
            {
                var baseTranslateTransform = BoundaryTransform.Children.Where(x => x.GetType() == typeof(TranslateTransform)).FirstOrDefault() as TranslateTransform;
                var baseScaleTransform = BoundaryTransform.Children.Where(x => x.GetType() == typeof(ScaleTransform)).FirstOrDefault() as ScaleTransform;

                if (baseTranslateTransform != null && baseScaleTransform != null)
                {
                    //Rectangle.RenderTransformOrigin = new Point(baseTranslateTransform.X, baseTranslateTransform.Y);
                    //initialTransformMatrix.M31 = (float)baseTranslateTransform.X;
                    //initialTransformMatrix.M32 = (float)baseTranslateTransform.Y;

                    TopLeft.RenderTransform = new TranslateTransform { X = baseTranslateTransform.X - 4, Y = baseTranslateTransform.Y - 4 };
                    TopRight.RenderTransform = new TranslateTransform { X = baseTranslateTransform.X + this.Width - 6, Y = baseTranslateTransform.Y - 4 };
                    BottomRight.RenderTransform = new TranslateTransform { X = baseTranslateTransform.X + this.Width - 6, Y = baseTranslateTransform.Y + this.Height - 6 };
                    BottomLeft.RenderTransform = new TranslateTransform { X = baseTranslateTransform.X - 4, Y = baseTranslateTransform.Y + this.Height - 6 };
                }
            }

            return base.ArrangeOverride(finalSize);
        }

        private void OnRectanglePointerEntered(object sender, PointerRoutedEventArgs e)
        {
            (sender as FrameworkElement).CapturePointer(e.Pointer);
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeAll, 1);
        }

        private void OnTopLeft_BottomRightPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeNorthwestSoutheast, 5);
        }

        private void OnTopRight_BottomLeftPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeNortheastSouthwest, 5);
        }

        private void OnResizeHandlerPointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 5);
            (sender as FrameworkElement).ReleasePointerCaptures();
        }

        private void OnHandlerPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            (sender as FrameworkElement).CapturePointer(e.Pointer);
        }

        private void OnHandlerPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            (sender as FrameworkElement).ReleasePointerCaptures();
        }

        private Matrix3x2 CalculateRectangleTransform(Point manipulatedTranslation)
        {
            var originalWidth = this.Width;
            var originalHeight = this.Height;

            var currentMatrix = new Matrix3x2
            {
                M11 = 1f,
                M12 = 0f,
                M21 = 0f,
                M22 = 1f,
                M31 = (float)manipulatedTranslation.X / ParentZoomFactor,
                M32 = (float)manipulatedTranslation.Y / ParentZoomFactor
            };

            return currentMatrix;
        }

        #region scale handlers
        private void OnTopLeftManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var deltaTransform = new CompositeTransform();
            var matrixTransform = new MatrixTransform();
            matrixTransform.Matrix = Matrix.Identity;

            var topLeftHandler = sender as Ellipse;
            var rectangleTranslateTransform = (Rectangle.RenderTransform as TransformGroup).Children.Where(x => x.GetType() == typeof(TranslateTransform)).FirstOrDefault() as TranslateTransform;
            var rectangleScaleTransform = (Rectangle.RenderTransform as TransformGroup).Children.Where(x => x.GetType() == typeof(ScaleTransform)).FirstOrDefault() as ScaleTransform;

            if (rectangleTranslateTransform is null)
                return;

            var topLeftHandlerTransform = topLeftHandler.RenderTransform as TranslateTransform;
            var topRightHandlerTransform = TopRight.RenderTransform as TranslateTransform;
            var bottomLeftHandlerTransform = BottomLeft.RenderTransform as TranslateTransform;

            topLeftHandlerTransform.X += e.Delta.Translation.X / ParentZoomFactor;
            topLeftHandlerTransform.Y += e.Delta.Translation.Y / ParentZoomFactor;

            topRightHandlerTransform.Y += e.Delta.Translation.Y / ParentZoomFactor;

            bottomLeftHandlerTransform.X += e.Delta.Translation.X / ParentZoomFactor;

            //rectangleScaleTransform.ScaleX *= ((Rectangle.Width + (-e.Delta.Translation.X / ParentZoomFactor)) / Rectangle.Width);
            //rectangleScaleTransform.ScaleY *= ((Rectangle.Height + (-e.Delta.Translation.Y / ParentZoomFactor)) / Rectangle.Height);

            rectangleTranslateTransform.X += e.Delta.Translation.X / ParentZoomFactor;
            rectangleTranslateTransform.Y += e.Delta.Translation.Y / ParentZoomFactor;

            //rectangleTranslateTransform.X /= rectangleScaleTransform.ScaleX;
            //rectangleTranslateTransform.Y /= rectangleScaleTransform.ScaleY;

            Rectangle.Width += -e.Delta.Translation.X / ParentZoomFactor;
            Rectangle.Height += -e.Delta.Translation.Y / ParentZoomFactor;
        }

        private void OnTopRightManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var topRightHandler = sender as Ellipse;
            var rectangleTranslateTransform = (Rectangle.RenderTransform as TransformGroup).Children.Where(x => x.GetType() == typeof(TranslateTransform)).FirstOrDefault() as TranslateTransform;

            if (rectangleTranslateTransform is null)
                return;

            var topRightHandlerTransform = topRightHandler.RenderTransform as TranslateTransform;
            var topLeftHandlerTransform = TopLeft.RenderTransform as TranslateTransform;
            var bottomRightHandlerTransform = BottomRight.RenderTransform as TranslateTransform;

            topRightHandlerTransform.X += e.Delta.Translation.X / ParentZoomFactor;
            topRightHandlerTransform.Y += e.Delta.Translation.Y / ParentZoomFactor;

            topLeftHandlerTransform.Y += e.Delta.Translation.Y / ParentZoomFactor;

            bottomRightHandlerTransform.X += e.Delta.Translation.X / ParentZoomFactor;

            rectangleTranslateTransform.Y += e.Delta.Translation.Y / ParentZoomFactor;

            Rectangle.Width += e.Delta.Translation.X / ParentZoomFactor;
            Rectangle.Height += -e.Delta.Translation.Y / ParentZoomFactor;
        }

        private void OnBottomLeftManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var bottomLeftHandler = sender as Ellipse;
            var rectangleTranslateTransform = (Rectangle.RenderTransform as TransformGroup).Children.Where(x => x.GetType() == typeof(TranslateTransform)).FirstOrDefault() as TranslateTransform;

            if (rectangleTranslateTransform is null)
                return;

            var bottomLeftHandlerTransform = bottomLeftHandler.RenderTransform as TranslateTransform;
            var topLeftHandlerTransform = TopLeft.RenderTransform as TranslateTransform;
            var bottomRightHandlerTransform = BottomRight.RenderTransform as TranslateTransform;

            bottomLeftHandlerTransform.X += e.Delta.Translation.X / ParentZoomFactor;
            bottomLeftHandlerTransform.Y += e.Delta.Translation.Y / ParentZoomFactor;

            topLeftHandlerTransform.X += e.Delta.Translation.X / ParentZoomFactor;

            bottomRightHandlerTransform.Y += e.Delta.Translation.Y / ParentZoomFactor;

            rectangleTranslateTransform.X += e.Delta.Translation.X / ParentZoomFactor;

            Rectangle.Width += -e.Delta.Translation.X / ParentZoomFactor;
            Rectangle.Height += e.Delta.Translation.Y / ParentZoomFactor;
        }

        private void OnBottomRightManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var bottomRightHandler = sender as Ellipse;

            var bottomRightHandlerTransform = bottomRightHandler.RenderTransform as TranslateTransform;
            var topRightHandlerTransform = TopRight.RenderTransform as TranslateTransform;
            var bottomLeftHandlerTransform = BottomLeft.RenderTransform as TranslateTransform;

            bottomRightHandlerTransform.X += e.Delta.Translation.X / ParentZoomFactor;
            bottomRightHandlerTransform.Y += e.Delta.Translation.Y / ParentZoomFactor;

            topRightHandlerTransform.X += e.Delta.Translation.X / ParentZoomFactor;

            bottomLeftHandlerTransform.Y += e.Delta.Translation.Y / ParentZoomFactor;

            Rectangle.Width += e.Delta.Translation.X / ParentZoomFactor;
            Rectangle.Height += e.Delta.Translation.Y / ParentZoomFactor;
        }
        #endregion

        private void OnRectangleManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var rectangleHandler = sender as Rectangle;
            var rectangleTranslateTransform = (rectangleHandler.RenderTransform as TransformGroup).Children.Where(x => x.GetType() == typeof(TranslateTransform)).FirstOrDefault() as TranslateTransform;

            if (rectangleTranslateTransform is null)
                return;

            var topLeftHandlerTransform = TopLeft.RenderTransform as TranslateTransform;
            var bottomRightHandlerTransform = BottomRight.RenderTransform as TranslateTransform;
            var topRightHandlerTransform = TopRight.RenderTransform as TranslateTransform;
            var bottomLeftHandlerTransform = BottomLeft.RenderTransform as TranslateTransform;

            rectangleTranslateTransform.X += e.Delta.Translation.X / ParentZoomFactor;
            rectangleTranslateTransform.Y += e.Delta.Translation.Y / ParentZoomFactor;

            topLeftHandlerTransform.X += e.Delta.Translation.X / ParentZoomFactor;
            topLeftHandlerTransform.Y += e.Delta.Translation.Y / ParentZoomFactor;

            bottomRightHandlerTransform.X += e.Delta.Translation.X / ParentZoomFactor;
            bottomRightHandlerTransform.Y += e.Delta.Translation.Y / ParentZoomFactor;

            topRightHandlerTransform.X += e.Delta.Translation.X / ParentZoomFactor;
            topRightHandlerTransform.Y += e.Delta.Translation.Y / ParentZoomFactor;

            bottomLeftHandlerTransform.X += e.Delta.Translation.X / ParentZoomFactor;
            bottomLeftHandlerTransform.Y += e.Delta.Translation.Y / ParentZoomFactor;
        }

        private void OnRectangleManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e) => CustomTransformMatrix = CalculateRectangleTransform(e.Cumulative.Translation);

        private void OnTopLeftManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e) => CustomTransformMatrix = CalculateBoundaryScale(e.Cumulative.Translation, ResizingHandles.TopLeft);

        private void OnTopRightManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e) => CustomTransformMatrix = CalculateBoundaryScale(e.Cumulative.Translation, ResizingHandles.TopRight);

        private void OnBottomRightManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e) => CustomTransformMatrix = CalculateBoundaryScale(e.Cumulative.Translation, ResizingHandles.BottomRight);

        private void OnBottomLeftManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e) => CustomTransformMatrix = CalculateBoundaryScale(e.Cumulative.Translation, ResizingHandles.BottomLeft);

        private Matrix3x2 CalculateBoundaryScale(Point translation, ResizingHandles handle)
        {
            var xMovement = (float)translation.X / ParentZoomFactor;
            var yMovement = (float)translation.Y / ParentZoomFactor;
            var rectangleTranslateTransform = (Rectangle.RenderTransform as TransformGroup).Children.Where(x => x.GetType() == typeof(TranslateTransform)).FirstOrDefault() as TranslateTransform;

            int xSignFactor = 1;
            int ySignFactor = 1;

            if (handle == ResizingHandles.TopLeft || handle == ResizingHandles.BottomLeft)
                xSignFactor *= -1;

            if (handle == ResizingHandles.TopLeft || handle == ResizingHandles.TopRight)
                ySignFactor *= -1;

            var scale = Matrix3x2.CreateScale(1f + ((xSignFactor * xMovement) / (float)this.Width), 1f + ((ySignFactor * yMovement) / (float)this.Height));
            float xOffset = 0f;
            float yOffset = 0f;

            switch (handle)
            {
                case ResizingHandles.TopLeft:
                    xOffset = (float)rectangleTranslateTransform.X - (((float)rectangleTranslateTransform.X - xMovement) * scale.M11);
                    yOffset = (float)rectangleTranslateTransform.Y - (((float)rectangleTranslateTransform.Y - yMovement) * scale.M22);
                    break;
                case ResizingHandles.TopRight:
                    xOffset = (float)rectangleTranslateTransform.X - (float)rectangleTranslateTransform.X * scale.M11;
                    yOffset = (float)rectangleTranslateTransform.Y - (((float)rectangleTranslateTransform.Y - yMovement) * scale.M22);
                    break;
                case ResizingHandles.BottomLeft:
                    xOffset = (float)rectangleTranslateTransform.X - (((float)rectangleTranslateTransform.X - xMovement) * scale.M11);
                    yOffset = (float)rectangleTranslateTransform.Y - (float)rectangleTranslateTransform.Y * scale.M22; ;
                    break;
                case ResizingHandles.BottomRight:
                    xOffset = (float)rectangleTranslateTransform.X - (float)rectangleTranslateTransform.X * scale.M11;
                    yOffset = (float)rectangleTranslateTransform.Y - (float)rectangleTranslateTransform.Y * scale.M22;
                    break;
            }

            var newMtrx = new Matrix3x2
            {
                M11 = scale.M11,
                M12 = 0,
                M21 = 0,
                M22 = scale.M22,
                M31 = xOffset,
                M32 = yOffset
            };

            return newMtrx;
        }
    }
}
