using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CustomControls
{
    public sealed class ResizableRectangle : Control
    {
        public ResizableRectangle()
        {
            this.DefaultStyleKey = typeof(ResizableRectangle);
        }

        private void AddHandles()
        {
            var topLeft = new Ellipse { Height = 8, Width = 8, Fill = BorderBrush };
            var topRight = new Ellipse { Height = 8, Width = 8, Fill = BorderBrush };
            var bottomRight = new Ellipse { Height = 8, Width = 8, Fill = BorderBrush };
            var bottomLeft = new Ellipse { Height = 8, Width = 8, Fill = BorderBrush };

            Canvas.SetLeft(topLeft, ActualOffset.X);
            Canvas.SetTop(topLeft, ActualOffset.Y);
        }

        public double Top
        {
            get { return (double)GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Top.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TopProperty =
            DependencyProperty.Register(nameof(Top), typeof(double), typeof(ResizableRectangle), new PropertyMetadata(0d));

        public double Left
        {
            get { return (double)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Left.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftProperty =
            DependencyProperty.Register("Left", typeof(double), typeof(ResizableRectangle), new PropertyMetadata(0d));

        public double Bottom
        {
            get { return (double)GetValue(BottomProperty); }
            set { SetValue(BottomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bottom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BottomProperty =
            DependencyProperty.Register("Bottom", typeof(double), typeof(ResizableRectangle), new PropertyMetadata(0d));

        public double Right
        {
            get { return (double)GetValue(RightProperty); }
            set { SetValue(RightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Right.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightProperty =
            DependencyProperty.Register("Right", typeof(double), typeof(ResizableRectangle), new PropertyMetadata(0d));
    }
}
