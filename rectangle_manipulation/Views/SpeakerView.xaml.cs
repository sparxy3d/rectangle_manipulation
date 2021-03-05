using CustomControls;
using CustomControls.ResizableRectangle;
using rectangle_manipulation.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace rectangle_manipulation.Views
{
    public sealed partial class SpeakerView : Page
    {
        ObservableCollection<Speaker> Speakers { get; set; }

        public SpeakerView()
        {
            this.InitializeComponent();

            var random = new Random();
            var x = random.Next(5, 650);
            var y = random.Next(5, 550);

            Rectangle rectangle = new Rectangle
            {
                Name = "annotation"
            };

            rectangle.Width = 100;
            rectangle.Height = 123;

            //Canvas.SetTop(rectangle, x);
            //Canvas.SetLeft(rectangle, y);

            rectangle.RenderTransform = new CompositeTransform
            {
                TranslateX = x,
                TranslateY = y
            };

            var brush = new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 255));
            rectangle.Stroke = brush;
            rectangle.StrokeThickness = 2;
            rectangle.Fill = new SolidColorBrush(Colors.Aquamarine);

            RootCanvas.Children.Add(rectangle);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(RootCanvas).Position;
            Rect rectangle;
            var transform = new CompositeTransform();

            foreach (var child in RootCanvas.Children)
            {
                if (child.GetType() != typeof(Rectangle))
                    continue;

                transform = child.RenderTransform as CompositeTransform;
                var rect = new Rect(transform.TranslateX, transform.TranslateY, (child as Rectangle).Width * transform.ScaleX, (child as Rectangle).Height * transform.ScaleY);
                if (rect.Contains(point))
                {
                    rectangle = rect;
                    break;
                }

                rectangle = new Rect();
            }

            RootCanvas.Children.Remove(RootCanvas.Children.Where(x => x.GetType() == typeof(ResizableRectangle)).FirstOrDefault());
            RootCanvas.Children.Remove(RootCanvas.Children.Where(x => x.GetType() == typeof(ResizableRectangle)).FirstOrDefault());

            if (rectangle == new Rect())
                return;

            //var resizableRect = new ResizableRectangle
            //{
            //    Height = rectangle.Height + 2,
            //    Width = rectangle.Width + 2,
            //    //Top = rectangle.Top,
            //    //Right = rectangle.Right,
            //    //Bottom = rectangle.Bottom,
            //    //Left = rectangle.Left
            //};

            //resizableRect.BorderBrush = new SolidColorBrush(Colors.Black);
            //resizableRect.BorderThickness = new Thickness(4);
            //resizableRect.BorderDashArray = new DoubleCollection() { 3, 2 };

            //resizableRect.PointerReleased += ResizableRect_PointerReleased;

            //Canvas.SetLeft(resizableRect, rectangle.X - 1);
            //Canvas.SetTop(resizableRect, rectangle.Y - 1);

            var resizableRectV2 = new ResizableRectangle
            {
                Top = rectangle.Top,
                Right = rectangle.Right,
                Bottom = rectangle.Bottom,
                Left = rectangle.Left,
                Width = rectangle.Width,
                Height = rectangle.Height,
                Stroke = new SolidColorBrush(Colors.DarkBlue),
                StrokeThickness = 2,
                BorderBrush = new SolidColorBrush(Colors.White),
                ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY,
                CompositeMode = ElementCompositeMode.SourceOver
            };

            resizableRectV2.ManipulationCompleted += delegate (object sender, ManipulationCompletedRoutedEventArgs args)
            {
                var matrix = resizableRectV2.CustomTransformMatrix;
            };

            //resizableRectV2.PointerReleased += delegate (object sender, PointerRoutedEventArgs args)
            //{
            //    var handler = sender as ResizableRectangle;
            //    var matrix = handler.CustomTransformMatrix;

            //    var annotation = RootCanvas.Children.Where(x => (x as Rectangle).Name == "annotation").FirstOrDefault();
            //    annotation.RenderTransform = new CompositeTransform
            //    {
            //        ScaleX = matrix.M11,
            //        ScaleY = matrix.M22,
            //        TranslateX = matrix.OffsetX,
            //        TranslateY = matrix.OffsetY
            //    };
            //};

            //Canvas.SetLeft(resizableRectV2, rectangle.X - 1);
            //Canvas.SetTop(resizableRectV2, rectangle.Y - 1); ;

            // RootCanvas.Children.Add(resizableRect);
            RootCanvas.Children.Add(resizableRectV2);

            base.OnPointerPressed(e);
        }

        private void ResizableRect_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var handler = sender as ResizableRectangle;
            //throw new NotImplementedException();
            var matrix = handler.CustomTransformMatrix;

            var annotation = RootCanvas.Children.Where(x => (x as Rectangle).Name == "annotation").FirstOrDefault();

            Canvas.SetLeft(annotation, annotation.ActualOffset.X + matrix.OffsetX);
            Canvas.SetTop(annotation, annotation.ActualOffset.Y + matrix.OffsetY);

            annotation.RenderTransform = new CompositeTransform
            {
                ScaleX = matrix.M11,
                ScaleY = matrix.M22,
                //TranslateX = matrix.OffsetX,
                //TranslateY = matrix.OffsetY
            };
        }
    }
}
