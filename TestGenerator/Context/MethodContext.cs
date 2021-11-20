using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator.Context
{
    public class MethodContext
    {
        public List<(string type, string name)> Parameters { get; set;}
        public string ReturnVal { get; set; }
        public string Name { get; set; }

        public MethodContext() {
            Parameters = new List<(string type, string name)>();
        }

        public bool hasReturn() {
            return ReturnVal!=null && ReturnVal == "void";
        }

        public bool hasArgs() {
            return Parameters!=null && Parameters.Count != 0;
        }
    }
}
