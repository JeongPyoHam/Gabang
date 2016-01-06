using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gabang.Controls {
    /// <summary>
    /// Interaction logic for MatrixView2.xaml
    /// </summary>
    public partial class MatrixView2 : UserControl {
        private TranslateTransform _transform;
        public MatrixView2() {
            InitializeComponent();

            _transform = new TranslateTransform();
            RootCanvas.RenderTransform = _transform;
        }

        #region expreriment, X position

        private bool _toggle = false;

        public void Toggle() {
            if (_toggle) {
                foreach (FrameworkElement child in RootCanvas.Children) {
                    double left = (double) child.GetValue(Canvas.LeftProperty);
                    child.SetValue(Canvas.LeftProperty, left);
                }
            } else {
                foreach (FrameworkElement child in RootCanvas.Children) {
                    double left = (double)child.GetValue(Canvas.LeftProperty);
                    child.SetValue(Canvas.LeftProperty, left);
                }
            }

            _toggle ^= true;
        }


        public void WiggleTransform() {
            if (_toggle) {
                _transform.X += 5.0;
            } else {
                _transform.X -= 5.0;
            }
            _toggle ^= true;
        }

        public void ChangeText() {
            foreach (TextBlock textBlock in RootCanvas.Children) {
                textBlock.Text += "1";
            }
        }

        public void CustomMeasure() {
            if (RootCanvas.Children.Count == 0) {
                RootCanvas.Children.Clear();
                _textBlocks.Clear();

                double left = 0;
                double top = 0;
                for (int i = 0; i < 2000; i++) {
                    var textBlock = new TextBlock() { Text = i.ToString() };
                    textBlock.SetValue(Canvas.LeftProperty, left + 1000);
                    textBlock.SetValue(Canvas.TopProperty, top);


                    _textBlocks.Add(new TextBlock() { Text = i.ToString() });
                    RootCanvas.Children.Add(textBlock);

                    left += 10;
                    if (left >= ActualWidth) {
                        left = 0;
                        top += 10.0;
                    }
                }
            }
        }

        #endregion

        List<TextBlock> _textBlocks = new List<TextBlock>();

        protected override Size MeasureOverride(Size constraint) {
            using (var elapsed = new Elapsed("MatrixView2:Measure:")) {
                CustomMeasure();

                var measured = base.MeasureOverride(constraint);
                return measured;
            }
        }

        protected override Size ArrangeOverride(Size arrangeBounds) {
            using (var elapsed = new Elapsed("MatrixView2:Arrange:")) {
                //Point location = new Point();
                //foreach (var textBlock in _textBlocks) {
                //    Rect rect = new Rect(location, textBlock.DesiredSize);
                //    textBlock.Arrange(rect);
                //    location.X += 10;
                //}
                return base.ArrangeOverride(arrangeBounds);
            }
        }
    }
}
