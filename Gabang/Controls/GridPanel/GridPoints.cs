using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls {
    public class GridPoints {
        private double[] _xPositions;
        private double[] _yPositions;
        private double[] _width;
        private double[] _height;

        private bool _xPositionValid;
        private bool _yPositionValid;

        public GridPoints(int rowCount, int columnCount) {
            RowCount = rowCount;
            ColumnCount = columnCount;

            _xPositions = new double[ColumnCount + 1];
            _xPositionValid = false;
            _yPositions = new double[RowCount + 1];
            _yPositionValid = false;
            _width = new double[ColumnCount];
            _height = new double[RowCount];

            InitializeWidthAndHeight();
        }

        public int RowCount { get; }

        public int ColumnCount { get; }

        public double MinItemWidth { get { return 20.0; } }

        public double MinItemHeight { get { return 10.0; } }

        public double VerticalOffset { get; set; }

        public double HorizontalOffset { get; set; }

        public double xPosition(int xIndex) {
            EnsureXPositions();
            return _xPositions[xIndex] - HorizontalOffset;
        }


        public double yPosition(int yIndex) {
            EnsureYPositions();
            return _yPositions[yIndex] - VerticalOffset;
        }

        public double GetWidth(int xIndex) {
            return _width[xIndex];
        }

        public void SetWidth(int xIndex, double value) {
            _width[xIndex] = value;
            _xPositionValid = false;
        }

        public double GetWidth(Range range) {
            return Size(range, _xPositions);
        }

        public double GetHeight(int yIndex) {
            return _height[yIndex];
        }

        public void SetHeight(int yIndex, double value) {
            _height[yIndex] = value;
            _yPositionValid = false;
        }

        public double GetHeight(Range range) {
            return Size(range, _yPositions);
        }

        private double Size(Range range, double[] positions) {
            return positions[range.Start + range.Count] - positions[range.Start];
        }

        public int xIndex(double position) {
            EnsureXPositions();
            return Index(position, _xPositions);
        }

        public int yIndex(double position) {
            EnsureYPositions();
            return Index(position, _yPositions);
        }

        private int Index(double position, double[] positions) {
            int index = Array.BinarySearch(positions, position);
            return (index < 0) ? ~index : index;
        }

        private void InitializeWidthAndHeight() {
            for (int i = 0; i < ColumnCount; i++) {
                _width[i] = MinItemWidth;
            }

            for (int i = 0; i < RowCount; i++) {
                _height[i] = MinItemHeight;
            }

            ComputePositions();
        }

        public void ComputePositions() {
            ComputeYPositions();
            ComputeXPositions();
        }

        private void EnsureYPositions() {
            if (!_yPositionValid) {
                ComputeYPositions();
            }
        }

        private void ComputeYPositions() {
            double height = 0.0;
            for (int i = 0; i < RowCount; i++) {
                height += _height[i];
                _yPositions[i + 1] = height;
            }
            _yPositionValid = true;
        }

        private void EnsureXPositions() {
            if (!_xPositionValid) {
                ComputeXPositions();
            }
        }

        public void ComputeXPositions() {
            double width = 0.0;
            for (int i = 0; i < ColumnCount; i++) {
                width += _width[i];
                _xPositions[i + 1] = width;
            }
            _xPositionValid = true;
        }
    }
}
