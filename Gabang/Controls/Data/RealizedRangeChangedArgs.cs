using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls.Data {
    public class RealizedRangeChangedArgs : EventArgs {
        public Range OldRowRange { get; set; }
        public Range OldColumnRange { get; set; }
        public Range NewRowRange { get; set; }
        public Range NewColumnRange { get; set; }
    }
}
