using System;
using System.ComponentModel;

namespace Gabang.Controls {
    /// <summary>
    /// double that keeps the maximum value when assigned
    /// </summary>
    public class MaxDouble {
        public MaxDouble() { }
        public MaxDouble(double initialValue) {
            _max = initialValue;
        }

        /// <summary>
        /// bool 
        /// </summary>
        [DefaultValue(false)]
        public bool Frozen { get; set; }


        double? _max;
        /// <summary>
        /// Maximum value
        /// </summary>
        public double? Max {
            get {
                return _max;
            }
            set {
                if (!Frozen) {
                    if (_max.HasValue && value.HasValue) {
                        _max = Math.Max(_max.Value, value.Value);
                    } else {
                        _max = value;
                    }
                }
            }
        }
    }
}
