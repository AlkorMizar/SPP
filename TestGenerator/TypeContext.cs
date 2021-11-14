using System;
using System.Collections.Generic;
using System.Text;

namespace TestGenerator
{
    class TypeContext
    {
        public string Namespace { get; set; }
        public string Type { get; set; }

        public string[] Methods { get; set; }

        public TypeContext(string _namespace, string _type, string[] _methods) {
            Namespace = _namespace;
            Type = _type;
            Methods = _methods;
        }

        public TypeContext(string _namespace, string _type):this(_namespace,_type,null) { 
        }
        public TypeContext(string _namespace) : this(_namespace, null, null)
        {
        }
    }
}
