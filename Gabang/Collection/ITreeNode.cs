using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Collection
{
    public interface ITreeNode<T>
    {
        T Value { get; }

        IEnumerable<ITreeNode<T>> Children { get; }
    }
}
