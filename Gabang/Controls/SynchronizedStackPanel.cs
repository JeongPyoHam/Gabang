using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace Gabang.Controls {
    [Flags]
    public enum ScrollOrientation {
        None = 0x00,
        Vertical = 0x01,
        Horizontal = 0x02,
    }

    public class DataGridItemsHost {
        internal static bool ProcessMoveFocus(Key key) {
            bool moveFocusSucceeded = false;

            //1. Determine the direction in which the focus would navigate
            FocusNavigationDirection navDirection;
            switch (key) {
                case Key.Down:
                    navDirection = FocusNavigationDirection.Down;
                    break;
                case Key.Left:
                    navDirection = FocusNavigationDirection.Left;
                    break;
                case Key.Right:
                    navDirection = FocusNavigationDirection.Right;
                    break;
                case Key.Up:
                    navDirection = FocusNavigationDirection.Up;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            //2. Call MoveFocus() on the currently focused keyboard element, 
            // since this is call within the OnKeyDown of the DataGridControl, it would normally mean
            // that the focused element is within the DataGridControl
            DependencyObject element = Keyboard.FocusedElement as DependencyObject;
            UIElement uiElement = element as UIElement;

            // If the focused element is not a UIElement (e.g. : Hyperlink), we go up until we find one.
            while (uiElement == null && element != null) {
                element = TreeHelper.GetParent(element);
                uiElement = element as UIElement;
            }

            if (uiElement != null) {
                uiElement.MoveFocus(new TraversalRequest(navDirection));

                moveFocusSucceeded = !(uiElement == Keyboard.FocusedElement);
            }

            return moveFocusSucceeded;
        }

        internal static void BringIntoViewKeyboardFocusedElement() {
            FrameworkElement frameworkElement = Keyboard.FocusedElement as FrameworkElement;

            if (frameworkElement != null)
                frameworkElement.BringIntoView();
        }
    }

    internal static class DoubleUtil {
        public static bool AreClose(double value1, double value2) {
            if (value1 == value2) {
                return true;
            }

            double num1 = value1 - value2;

            if (num1 < 1.53E-06) {
                return (num1 > -1.53E-06);
            }

            return false;
        }
    }

    public class SynchronizedScrollViewer : ScrollViewer {
        #region Constructors

        static SynchronizedScrollViewer() {
            ScrollViewer.HorizontalScrollBarVisibilityProperty.OverrideMetadata(
              typeof(SynchronizedScrollViewer),
              new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden));

            ScrollViewer.VerticalScrollBarVisibilityProperty.OverrideMetadata(
              typeof(SynchronizedScrollViewer),
              new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden));

            // By default, we never want item scrolling.
            ScrollViewer.CanContentScrollProperty.OverrideMetadata(
              typeof(SynchronizedScrollViewer),
              new FrameworkPropertyMetadata(false));
        }

        public SynchronizedScrollViewer() {
            // ScrollViewer binds these three properties to the TemplatedParent properties. 
            // Changing their default value using only OverrideMetadata would not work.
            this.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            this.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        #endregion

        #region ScrollOrientation property

        public static readonly DependencyProperty ScrollOrientationProperty =
          DependencyProperty.Register("ScrollOrientation",
          typeof(ScrollOrientation),
          typeof(SynchronizedScrollViewer),
          new PropertyMetadata(ScrollOrientation.Horizontal));

        public ScrollOrientation ScrollOrientation {
            get {
                return (ScrollOrientation)this.GetValue(SynchronizedScrollViewer.ScrollOrientationProperty);
            }

            set {
                this.SetValue(SynchronizedScrollViewer.ScrollOrientationProperty, value);
            }
        }

        #endregion ScrollOrientation property

        #region LimitScrolling Property

        internal static readonly DependencyProperty LimitScrollingProperty =
            DependencyProperty.Register("LimitScrolling",
            typeof(bool),
            typeof(SynchronizedScrollViewer),
            new UIPropertyMetadata(true));

        internal bool LimitScrolling {
            get {
                return (bool)GetValue(SynchronizedScrollViewer.LimitScrollingProperty);
            }
            set {
                SetValue(SynchronizedScrollViewer.LimitScrollingProperty, value);
            }
        }

        #endregion LimitScrolling Property

        #region Protected Overrides

        protected override Size MeasureOverride(Size constraint) {
            Size desired = base.MeasureOverride(constraint);

            return desired;
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            if (this.TemplatedParent != m_mainScrollViewer) {
                if (m_mainScrollViewer != null)
                    m_mainScrollViewer.ScrollChanged -= this.OnMainScrollViewer_ScrollChanged;

                m_mainScrollViewer = TreeHelper.FindParent<ScrollViewer>(this);

                if (m_mainScrollViewer != null)
                    m_mainScrollViewer.ScrollChanged += this.OnMainScrollViewer_ScrollChanged;
            }
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e) {
            //case 117456: Because the event is routed (bubble) we have to kill the event here so that other classes ( more particularly the
            // DataGridScrollViewer) does not get confused by the event.
            e.Handled = true;

            base.OnScrollChanged(e);

            // m_mainScrollViewer will remain null in design mode.
            // and if it's null, nothing to update
            if (m_mainScrollViewer == null)
                return;

            // Sometimes, a row in the SynchronizedScrollViewer will scroll by itself,
            // not triggered by the main ScrollViewer scrolling. In that case, we want
            // to update the main ScrollViewer. A typical example is when a Row's cell is 
            // brought into view by, let's say, activating the cell editor.
            ScrollOrientation orientation = this.ScrollOrientation;

            bool invalidateMainScrollViewerMeasure = false;

            if ((orientation & ScrollOrientation.Horizontal) == ScrollOrientation.Horizontal) {
                invalidateMainScrollViewerMeasure = !DoubleUtil.AreClose(0, e.ExtentWidthChange);

                // If the Extent is 0, there is no reason to update
                // the main ScrollViewer offset since this means there
                // are no children
                if (this.ExtentWidth == 0)
                    return;

                double offset = (m_originalScrollChangedEventArgs != null)
                  ? m_originalScrollChangedEventArgs.HorizontalOffset
                  : e.HorizontalOffset;

                if (offset != m_mainScrollViewer.HorizontalOffset) {
                    m_mainScrollViewer.ScrollToHorizontalOffset(offset);
                }
            }

            if ((orientation & ScrollOrientation.Vertical) == ScrollOrientation.Vertical) {
                invalidateMainScrollViewerMeasure = !DoubleUtil.AreClose(0, e.ExtentHeightChange);

                // If the Extent is 0, there is no reason to update
                // the main ScrollViewer offset since this means there
                // are no children
                if (this.ExtentHeight == 0)
                    return;

                double offset = (m_originalScrollChangedEventArgs != null)
                  ? m_originalScrollChangedEventArgs.VerticalOffset
                  : e.VerticalOffset;

                if ((offset != m_mainScrollViewer.VerticalOffset) && (this.ExtentHeight > 0)) {
                    m_mainScrollViewer.ScrollToVerticalOffset(offset);
                }
            }

            m_originalScrollChangedEventArgs = null;

            // In some situations, the Extent*Change event is received AFTER the 
            // layout pass of the mainScrollViewer is done. Since the measure of the
            // mainScrollViewer uses the SynchronizedScrollViewer Extent size, we must
            // call InvalidateMeasure on the mainScrollViewer to ensure it is correctly
            // layouted
            if (invalidateMainScrollViewerMeasure)
                m_mainScrollViewer.InvalidateMeasure();
        }

        #endregion

        #region  PreviewKeyDown and KeyDown handling overrides

        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            if (e.Handled)
                return;

            switch (e.Key) {
                // Handle the System key definition (basically with ALT key pressed)
                case Key.System:
                    this.HandlePreviewSystemKey(e);
                    break;

                case Key.Tab:
                    this.HandlePreviewTabKey(e);
                    break;

                case Key.PageUp:
                    this.HandlePreviewPageUpKey(e);
                    break;

                case Key.PageDown:
                    this.HandlePreviewPageDownKey(e);
                    break;

                case Key.Home:
                    this.HandlePreviewHomeKey(e);
                    break;

                case Key.End:
                    this.HandlePreviewEndKey(e);
                    break;

                case Key.Up:
                    this.HandlePreviewUpKey(e);
                    break;

                case Key.Down:
                    this.HandlePreviewDownKey(e);
                    break;

                case Key.Left:
                    this.HandlePreviewLeftKey(e);
                    break;

                case Key.Right:
                    this.HandlePreviewRightKey(e);
                    break;

                default:
                    base.OnPreviewKeyDown(e);
                    break;
            }
        }

        protected virtual void HandlePreviewSystemKey(KeyEventArgs e) {
        }

        protected virtual void HandlePreviewTabKey(KeyEventArgs e) {
        }

        protected virtual void HandlePreviewLeftKey(KeyEventArgs e) {
            DataGridItemsHost.BringIntoViewKeyboardFocusedElement();
            this.UpdateLayout();
        }

        protected virtual void HandlePreviewRightKey(KeyEventArgs e) {
            DataGridItemsHost.BringIntoViewKeyboardFocusedElement();
            this.UpdateLayout();
        }

        protected virtual void HandlePreviewUpKey(KeyEventArgs e) {
            DataGridItemsHost.BringIntoViewKeyboardFocusedElement();
            this.UpdateLayout();
        }

        protected virtual void HandlePreviewDownKey(KeyEventArgs e) {
            DataGridItemsHost.BringIntoViewKeyboardFocusedElement();
            this.UpdateLayout();
        }

        protected virtual void HandlePreviewPageUpKey(KeyEventArgs e) {
        }

        protected virtual void HandlePreviewPageDownKey(KeyEventArgs e) {
        }

        protected virtual void HandlePreviewHomeKey(KeyEventArgs e) {
        }

        protected virtual void HandlePreviewEndKey(KeyEventArgs e) {
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.Handled)
                return;

            switch (e.Key) {
                // Handle the System key definition (basically with ALT key pressed)
                case Key.System:
                    this.HandleSystemKey(e);
                    break;

                case Key.Tab:
                    this.HandleTabKey(e);
                    break;

                case Key.PageUp:
                    this.HandlePageUpKey(e);
                    break;

                case Key.PageDown:
                    this.HandlePageDownKey(e);
                    break;

                case Key.Home:
                    this.HandleHomeKey(e);
                    break;

                case Key.End:
                    this.HandleEndKey(e);
                    break;

                case Key.Up:
                    this.HandleUpKey(e);
                    break;

                case Key.Down:
                    this.HandleDownKey(e);
                    break;

                case Key.Left:
                    this.HandleLeftKey(e);
                    break;

                case Key.Right:
                    this.HandleRightKey(e);
                    break;

                default:
                    base.OnKeyDown(e);
                    break;
            }
        }

        protected virtual void HandleSystemKey(KeyEventArgs e) {
        }

        protected virtual void HandleTabKey(KeyEventArgs e) {
        }

        protected virtual void HandlePageUpKey(KeyEventArgs e) {
            // Ensure to at least process the PageUp as Key.Up
            // to avoid the TableViewScrollViewer to process the key
            // directly without moving the focus.
            e.Handled = DataGridItemsHost.ProcessMoveFocus(Key.Up);

            // We were not able to move focus out of the 
            // SynchronizedScrollViewer but the focus is still inside.
            // Mark the key as handled to avoid the DataGridScrollViewer
            // to process the PageUp.
            if (!e.Handled && this.IsKeyboardFocusWithin)
                e.Handled = true;
        }

        protected virtual void HandlePageDownKey(KeyEventArgs e) {
            // Ensure to at least process the PageDown as Key.Down
            // to avoid the TableViewScrollViewer to process the key
            // directly without moving the focus.
            e.Handled = DataGridItemsHost.ProcessMoveFocus(Key.Down);

            // We were not able to move focus out of the 
            // SynchronizedScrollViewer but the focus is still inside.
            // Mark the key as handled to avoid the DataGridScrollViewer
            // to process the PageDown.
            if (!e.Handled && this.IsKeyboardFocusWithin)
                e.Handled = true;
        }

        protected virtual void HandleHomeKey(KeyEventArgs e) {
        }

        protected virtual void HandleEndKey(KeyEventArgs e) {
        }

        protected virtual void HandleUpKey(KeyEventArgs e) {
            e.Handled = DataGridItemsHost.ProcessMoveFocus(e.Key);
        }

        protected virtual void HandleDownKey(KeyEventArgs e) {
            e.Handled = DataGridItemsHost.ProcessMoveFocus(e.Key);
        }

        protected virtual void HandleLeftKey(KeyEventArgs e) {
            e.Handled = DataGridItemsHost.ProcessMoveFocus(e.Key);
        }

        protected virtual void HandleRightKey(KeyEventArgs e) {
            e.Handled = DataGridItemsHost.ProcessMoveFocus(e.Key);
        }

        #endregion

        #region Private Methods

        private void OnMainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            if (e.OriginalSource != m_mainScrollViewer)
                return;

            ScrollOrientation orientation = this.ScrollOrientation;
            bool limitScrolling = this.LimitScrolling;

            if ((orientation & ScrollOrientation.Horizontal) == ScrollOrientation.Horizontal) {
                // If the Extent is 0, there is no reason to update
                // the main ScrollViewer offset since this means there
                // are no children
                if (this.ExtentWidth == 0)
                    return;

                double offset = (limitScrolling)
                  ? Math.Min(e.HorizontalOffset, this.ExtentWidth - this.ViewportWidth)
                  : e.HorizontalOffset;

                if (offset != this.HorizontalOffset) {
                    // We keep the original ScrollChangedEventArgs because when we'll update
                    // our offset, it might be less than what was asked in the first place. Even
                    // if this doesn't make sense to us, it might be ok for the m_mainScrollViewer
                    // to scroll to that value. Since changing our offset will trigger the OnScrollChanged
                    // that will update the m_mainScrollViewer, we want to changer it's offset to 
                    // the right value.
                    m_originalScrollChangedEventArgs = e;
                    this.ScrollToHorizontalOffset(offset);
                }
            }

            if ((orientation & ScrollOrientation.Vertical) == ScrollOrientation.Vertical) {
                // If the Extent is 0, there is no reason to update
                // the main ScrollViewer offset since this means there
                // are no children
                if (this.ExtentHeight == 0)
                    return;

                double offset = (limitScrolling)
                  ? Math.Min(e.VerticalOffset, this.ExtentHeight - this.ViewportHeight)
                  : e.VerticalOffset;

                if (offset != this.VerticalOffset) {
                    // We keep the original ScrollChangedEventArgs because when we'll update
                    // our offset, it might be less than what was asked in the first place. Even
                    // if this doesn't make sense to us, it might be ok for the m_mainScrollViewer
                    // to scroll to that value. Since changing our offset will trigger the OnScrollChanged
                    // that will update the m_mainScrollViewer, we want to changer it's offset to 
                    // the right value.
                    m_originalScrollChangedEventArgs = e;
                    this.ScrollToVerticalOffset(offset);
                }
            }
        }

        #endregion

        #region Private Fields

        private ScrollViewer m_mainScrollViewer; // = null
        private ScrollChangedEventArgs m_originalScrollChangedEventArgs = null;

        #endregion
    }


    internal static class TreeHelper {
        /// <summary>
        /// Tries its best to return the specified element's parent. It will 
        /// try to find, in this order, the VisualParent, LogicalParent, LogicalTemplatedParent.
        /// It only works for Visual, FrameworkElement or FrameworkContentElement.
        /// </summary>
        /// <param name="element">The element to which to return the parent. It will only 
        /// work if element is a Visual, a FrameworkElement or a FrameworkContentElement.</param>
        /// <remarks>If the logical parent is not found (Parent), we check the TemplatedParent
        /// (see FrameworkElement.Parent documentation). But, we never actually witnessed
        /// this situation.</remarks>
        public static DependencyObject GetParent(DependencyObject element) {
            return TreeHelper.GetParent(element, true);
        }

        private static DependencyObject GetParent(DependencyObject element, bool recurseIntoPopup) {
            if (recurseIntoPopup) {
                // Case 126732 : To correctly detect parent of a popup we must do that exception case
                Popup popup = element as Popup;

                if ((popup != null) && (popup.PlacementTarget != null))
                    return popup.PlacementTarget;
            }

            Visual visual = element as Visual;
            DependencyObject parent = (visual == null) ? null : VisualTreeHelper.GetParent(visual);

            if (parent == null) {
                // No Visual parent. Check in the logical tree.
                FrameworkElement fe = element as FrameworkElement;

                if (fe != null) {
                    parent = fe.Parent;

                    if (parent == null) {
                        parent = fe.TemplatedParent;
                    }
                } else {
                    FrameworkContentElement fce = element as FrameworkContentElement;

                    if (fce != null) {
                        parent = fce.Parent;

                        if (parent == null) {
                            parent = fce.TemplatedParent;
                        }
                    }
                }
            }

            return parent;
        }

        /// <summary>
        /// This will search for a parent of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the element to find</typeparam>
        /// <param name="startingObject">The node where the search begins. This element is not checked.</param>
        /// <returns>Returns the found element. Null if nothing is found.</returns>
        public static T FindParent<T>(DependencyObject startingObject) where T : DependencyObject {
            return TreeHelper.FindParent<T>(startingObject, false, null);
        }

        /// <summary>
        /// This will search for a parent of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the element to find</typeparam>
        /// <param name="startingObject">The node where the search begins.</param>
        /// <param name="checkStartingObject">Should the specified startingObject be checked first.</param>
        /// <returns>Returns the found element. Null if nothing is found.</returns>
        public static T FindParent<T>(DependencyObject startingObject, bool checkStartingObject) where T : DependencyObject {
            return TreeHelper.FindParent<T>(startingObject, checkStartingObject, null);
        }

        /// <summary>
        /// This will search for a parent of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the element to find</typeparam>
        /// <param name="startingObject">The node where the search begins.</param>
        /// <param name="checkStartingObject">Should the specified startingObject be checked first.</param>
        /// <param name="additionalCheck">Provide a callback to check additional properties 
        /// of the found elements. Can be left Null if no additional criteria are needed.</param>
        /// <returns>Returns the found element. Null if nothing is found.</returns>
        /// <example>Button button = TreeHelper.FindParent&lt;Button&gt;( this, foundChild => foundChild.Focusable );</example>
        public static T FindParent<T>(DependencyObject startingObject, bool checkStartingObject, Func<T, bool> additionalCheck) where T : DependencyObject {
            T foundElement;
            DependencyObject parent = (checkStartingObject ? startingObject : TreeHelper.GetParent(startingObject, true));

            while (parent != null) {
                foundElement = parent as T;

                if (foundElement != null) {
                    if (additionalCheck == null) {
                        return foundElement;
                    } else {
                        if (additionalCheck(foundElement))
                            return foundElement;
                    }
                }

                parent = TreeHelper.GetParent(parent, true);
            }

            return null;
        }

        /// <summary>
        /// This will search for a child of the specified type. The search is performed 
        /// hierarchically, breadth first (as opposed to depth first).
        /// </summary>
        /// <typeparam name="T">The type of the element to find</typeparam>
        /// <param name="parent">The root of the tree to search for. This element itself is not checked.</param>
        /// <returns>Returns the found element. Null if nothing is found.</returns>
        public static T FindChild<T>(DependencyObject parent) where T : DependencyObject {
            return TreeHelper.FindChild<T>(parent, null);
        }

        /// <summary>
        /// This will search for a child of the specified type. The search is performed 
        /// hierarchically, breadth first (as opposed to depth first).
        /// </summary>
        /// <typeparam name="T">The type of the element to find</typeparam>
        /// <param name="parent">The root of the tree to search for. This element itself is not checked.</param>
        /// <param name="additionalCheck">Provide a callback to check additional properties 
        /// of the found elements. Can be left Null if no additional criteria are needed.</param>
        /// <returns>Returns the found element. Null if nothing is found.</returns>
        /// <example>Button button = TreeHelper.FindChild&lt;Button&gt;( this, foundChild => foundChild.Focusable );</example>
        public static T FindChild<T>(DependencyObject parent, Func<T, bool> additionalCheck) where T : DependencyObject {
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            T child;

            for (int index = 0; index < childrenCount; index++) {
                child = VisualTreeHelper.GetChild(parent, index) as T;

                if (child != null) {
                    if (additionalCheck == null) {
                        return child;
                    } else {
                        if (additionalCheck(child))
                            return child;
                    }
                }
            }

            for (int index = 0; index < childrenCount; index++) {
                child = TreeHelper.FindChild<T>(VisualTreeHelper.GetChild(parent, index), additionalCheck);

                if (child != null)
                    return child;
            }

            return null;
        }

        /// <summary>
        /// Returns true if the specified element is a child of parent somewhere in the visual 
        /// tree. This method will work for Visual, FrameworkElement and FrameworkContentElement.
        /// </summary>
        /// <param name="element">The element that is potentially a child of the specified parent.</param>
        /// <param name="parent">The element that is potentially a parent of the specified element.</param>
        public static bool IsDescendantOf(DependencyObject element, DependencyObject parent) {
            return TreeHelper.IsDescendantOf(element, parent, true);
        }

        /// <summary>
        /// Returns true if the specified element is a child of parent somewhere in the visual 
        /// tree. This method will work for Visual, FrameworkElement and FrameworkContentElement.
        /// </summary>
        /// <param name="element">The element that is potentially a child of the specified parent.</param>
        /// <param name="parent">The element that is potentially a parent of the specified element.</param>
        public static bool IsDescendantOf(DependencyObject element, DependencyObject parent, bool recurseIntoPopup) {
            while (element != null) {
                if (element == parent)
                    return true;

                element = TreeHelper.GetParent(element, recurseIntoPopup);
            }

            return false;
        }
    }
}

