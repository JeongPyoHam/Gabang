﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Gabang.Controls {
    internal class VisualGridScroller {
        private TaskScheduler ui;
        private BlockingCollection<ScrollCommand> _scrollCommands;

        public VisualGridScroller() {
            ui = TaskScheduler.FromCurrentSynchronizationContext();

            _scrollCommands = new BlockingCollection<ScrollCommand>();

            Task.Run(() => ScrollCommandsHandler());
        }

        public GridPoints Points { get; set; }
        public VisualGrid ColumnHeader { get; set; }
        public VisualGrid RowHeader { get; set; }
        //public VisualGrid DataGrid { get; set; }
        public GridPanel DataGrid { get; set; }
        public IGridProvider<string> DataProvider { get; set; }

        internal void EnqueueCommand(ScrollType code, double param) {
            _scrollCommands.Add(new ScrollCommand(code, param));
        }

        internal void EnqueueCommand(ScrollType code, Size size) {
            _scrollCommands.Add(new ScrollCommand(code, size));
        }

        private async void ScrollCommandsHandler() {
            List<ScrollCommand> batch = new List<ScrollCommand>();

            foreach (var command in _scrollCommands.GetConsumingEnumerable()) {
                try {
                    batch.Add(command);
                    if (_scrollCommands.Count > 0) {
                        // another command has been queued already. continue to next
                        continue;
                    } else {
                        for (int i = 0; i < batch.Count; i++) {
                            if (i < (batch.Count - 1)
                                && ((batch[i].Code == ScrollType.SizeChange && batch[i + 1].Code == ScrollType.SizeChange)
                                    || (batch[i].Code == ScrollType.SetHorizontalOffset && batch[i + 1].Code == ScrollType.SetHorizontalOffset)
                                    || (batch[i].Code == ScrollType.SetVerticalOffset && batch[i + 1].Code == ScrollType.SetVerticalOffset))) {
                                continue;
                            } else {
                                await ExecuteCommand(batch[i]);
                            }
                        }
                        batch.Clear();
                    }
                } catch (Exception ex) {
                    Trace.WriteLine(ex.Message);    // TODO: handle exception
                    batch.Clear();
                }
            }
        }

        private async Task ExecuteCommand(ScrollCommand cmd) {
            switch (cmd.Code) {
                case ScrollType.LineUp:
                    await LineUpAsync();
                    break;
                case ScrollType.LineDown:
                    await LineDownAsync();
                    break;
                case ScrollType.LineLeft: LineLeft(); break;
                case ScrollType.LineRight: LineRight(); break;

                case ScrollType.PageUp:
                    await PageUpAsync();
                    break;
                case ScrollType.PageDown:
                    await PageDownAsync();
                    break;
                case ScrollType.PageLeft: PageLeft(); break;
                case ScrollType.PageRight: PageRight(); break;

                case ScrollType.SetHorizontalOffset:
                    await SetHorizontalOffsetAsync(cmd.Param);
                    break;
                case ScrollType.SetVerticalOffset:
                    await SetVerticalOffsetAsync(cmd.Param);
                    break;
                case ScrollType.MouseWheel:
                    await SetMouseWheelAsync(cmd.Param); break;

                case ScrollType.SizeChange: {
                        await RefreshVisualsInternalAsync(
                            new Rect(
                                Points.HorizontalOffset,
                                Points.VerticalOffset,
                                DataGrid.RenderSize.Width,
                                DataGrid.RenderSize.Height));
                        break;
                    }
                case ScrollType.Invalid:
                    break;
            }
        }

        private async Task RefreshVisualsInternalAsync(Rect visualViewport) {
            using (var elapsed = new Elapsed("RefreshVisuals:")) {
                Debug.Assert(TaskUtilities.IsOnBackgroundThread());

                GridRange newViewport = Points.ComputeDataViewport(visualViewport);

                var data = await DataProvider.GetAsync(newViewport);

                await Task.Factory.StartNew(
                    () => DrawVisuals(newViewport, data),
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    ui);
            }
        }

        private void DrawVisuals(GridRange dataViewport, IGridData<string> data) {
            if (DataGrid != null) {
                DataGrid.DrawVisuals(dataViewport, data.Grid);
            }

            if (ColumnHeader != null) {
                GridRange columnViewport = new GridRange(
                    new Range(0, 1),
                    dataViewport.Columns);
                ColumnHeader.DrawVisuals(columnViewport, new Grid<string>(columnViewport, data.ColumnHeader)); // TODO: new data
            }

            if (RowHeader != null) {
                GridRange rowViewport = new GridRange(
                    dataViewport.Rows,
                    new Range(0, 1));
                RowHeader.DrawVisuals(rowViewport, new Grid<string>(rowViewport, data.RowHeader));    // TODO: new data
            }
        }

        private async Task SetVerticalOffsetAsync(double offset) {
            Points.VerticalOffset = offset;

            await RefreshVisualsInternalAsync(
                            new Rect(
                                Points.HorizontalOffset,
                                Points.VerticalOffset,
                                DataGrid.RenderSize.Width,
                                DataGrid.RenderSize.Height));
        }

        private Task LineUpAsync() {
            return SetVerticalOffsetAsync(Points.VerticalOffset - 10.0);    // TODO: do not hard-code the number here.
        }

        private Task LineDownAsync() {
            return SetVerticalOffsetAsync(Points.VerticalOffset + 10.0);    // TODO: do not hard-code the number here.
        }

        private Task PageUpAsync() {
            return SetVerticalOffsetAsync(Points.VerticalOffset - 100.0);    // TODO: do not hard-code the number here.
        }

        private Task PageDownAsync() {
            return SetVerticalOffsetAsync(Points.VerticalOffset + 100.0);    // TODO: do not hard-code the number here.
        }

        private async Task SetHorizontalOffsetAsync(double offset) {
            Points.HorizontalOffset = offset;

            await RefreshVisualsInternalAsync(
                            new Rect(
                                Points.HorizontalOffset,
                                Points.VerticalOffset,
                                DataGrid.RenderSize.Width,
                                DataGrid.RenderSize.Height));
        }

        private void LineRight() {
            throw new NotImplementedException();
        }

        private void LineLeft() {
            throw new NotImplementedException();
        }

        private void PageRight() {
            throw new NotImplementedException();
        }

        private void PageLeft() {
            throw new NotImplementedException();
        }

        private Task SetMouseWheelAsync(double delta) {
            return SetVerticalOffsetAsync(Points.VerticalOffset + delta);    // TODO: do not hard-code the number here.
        }
    }

    internal enum ScrollType {
        Invalid,
        LineUp,
        LineDown,
        LineLeft,
        LineRight,
        PageUp,
        PageDown,
        PageLeft,
        PageRight,
        SetHorizontalOffset,
        SetVerticalOffset,
        MouseWheel,
        SizeChange,
    }

    internal struct ScrollCommand {
        private static Lazy<ScrollCommand> _empty = new Lazy<ScrollCommand>(() => new ScrollCommand(ScrollType.Invalid, 0));
        public static ScrollCommand Empty { get { return _empty.Value; } }

        internal ScrollCommand(ScrollType code, double param) {
            Debug.Assert(code != ScrollType.SizeChange);

            Code = code;
            Param = param;
            Size = Size.Empty;
        }

        internal ScrollCommand(ScrollType code, Size size) {
            Code = code;
            Param = double.NaN;
            Size = size;
        }

        public ScrollType Code { get; set; }

        public double Param { get; set; }

        public Size Size { get; set; }
    }
}
