using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls
{
    public class SubVariable
    {
        public string X { get; set; }
    }
    public class Variable
    {
        public string VariableName { get; set; }
        public string VariableValue { get; set; }
        public string TypeName { get; set; }

        public object Sub { get; set; }
    }
}
