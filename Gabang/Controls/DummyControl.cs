﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gabang.Controls {
    public class DummyControl : Control {
        protected override Size MeasureOverride(Size constraint) {
            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size arrangeBounds) {
            return base.ArrangeOverride(arrangeBounds);
        }
    }
}
