using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls {
    public class JointCollectionGridColumnCollection : ConstantCountList<JointCollectionGridColumn> {
        public JointCollectionGridColumnCollection(int columnCount) : base(columnCount) { }


        public override JointCollectionGridColumn this[int index] {
            get {
                return new JointCollectionGridColumn();
            }

            set {
                throw new NotSupportedException("JointCollectionGridColumnCollection is read only");
            }
        }

        public override bool Contains(JointCollectionGridColumn item) {
            return IndexOf(item) != -1;
        }

        public override int IndexOf(JointCollectionGridColumn item) {
            throw new NotImplementedException();
        }
    }
}
