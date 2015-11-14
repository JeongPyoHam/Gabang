﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls.DataVirtualization {
    public interface IGridProvider<T> {
        Task<IGrid<T>> GetRangeAsync(GridRange gridRange);
    }
}
