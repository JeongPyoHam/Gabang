﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls.Data {
    public interface IVirtualizable<T> : IEquatable<IVirtualizable<T>> {
        Task RealizeAsync();

        Task VirtualizeAsync();
    }
}
