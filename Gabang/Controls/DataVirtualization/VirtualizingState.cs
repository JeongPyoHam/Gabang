using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls.DataVirtualization {
    public enum VirtualizingState {
        /// <summary>
        /// Virtualized
        /// </summary>
        Virtualized,

        /// <summary>
        /// Realized
        /// </summary>
        Realized,

        /// <summary>
        /// Realized, and waiting virtualization
        /// </summary>
        PendingVirtualization,

        /// <summary>
        /// virtualized, and waiting realization
        /// </summary>
        PendingRealization,
    }
}
