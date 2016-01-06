using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Gabang.Controls {
    internal class GridPanelCell : Border {
        public const double MinimumWidth = 20.0;
        public const double MinimumHeight = 10.0;

        public GridPanelCell() {
            Child = new TextBlock();
            BorderThickness = new Thickness(0, 0, 1, 1);
            BorderBrush = Brushes.Black;
        }

        public int Row { get; set; }

        public int Column { get; set; }

        public string Text {
            get {
                return ((TextBlock)Child).Text;
            }

            set {
                ((TextBlock)Child).Text = value;
            }
        }

        public void Inflate(double width, double height) {
            Width = Math.Max(MinimumWidth, width);
            Height = Math.Max(MinimumHeight, height);
        }
    }

    internal class GridPanel : Canvas {
        public GridPanel() {
            RenderTransform = new TranslateTransform();
        }

        public int RowCount { get; set; }

        public int ColumnCount { get; set; }

        public IGridProvider<string> DataProvider { get; set; }

        public GridPoints Points { get; set; }

        internal void SetOffset(double x, double y) {
            var transform = (TranslateTransform)RenderTransform;
            transform.X = x;
            transform.Y = y;
        }

        private GridRange _dataViewport;
        private Grid<GridPanelCell> _cells;

        internal void DrawVisuals(GridRange newViewport, IGrid<string> data) {
            Children.Clear();

            var orgViewport = _dataViewport;
            var orgGrid = _cells;
            _cells = new Grid<GridPanelCell>(
                newViewport,
                (r, c) => {
                    if (orgViewport.Contains(r, c)) {
                        return orgGrid[r, c];
                    }
                    var visual = new GridPanelCell();
                    visual.Row = r;
                    visual.Column = c;
                    visual.Text = data[r, c];

                    return visual;
                });
            _dataViewport = newViewport;

            Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (int r in newViewport.Rows.GetEnumerable()) {
                foreach (int c in newViewport.Columns.GetEnumerable()) {
                    GridPanelCell textBlock = _cells[r, c];
                    textBlock.Measure(infiniteSize);

                    Points.SetWidth(c, textBlock.DesiredSize.Width);
                    Points.SetHeight(r, textBlock.DesiredSize.Height);

                    Children.Add(textBlock);
                }
            }

            foreach (GridPanelCell child in Children) {
                child.SetValue(Canvas.LeftProperty, Points.xPosition(child.Column));
                child.SetValue(Canvas.TopProperty, Points.yPosition(child.Row));

                child.Inflate(Points.GetWidth(child.Column), Points.GetHeight(child.Row));
            }
        }
    }
}
