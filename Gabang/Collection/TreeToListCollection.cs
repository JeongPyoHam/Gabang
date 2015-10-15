using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabang.Collection;

namespace GabangCollection
{
    public class TreeToListCollection<T>
    {
        public ObservableTreeNode<T> TreeRoot { get; set; }

        public IList<T> List { get; set; }
    }
}
