using System;
using System.Collections.Generic;
using System.Text;
using TestGenerator.Context;

namespace TestGenerator
{
    public class TypeContext
    {
        public string Namespace { get; set; }
        public string Type { get; set; }

        public string Name { get { return "test" + Type; } }

        public MethodContext[] Methods { get; set; }
        
        public ConstructorContext Constructor { get; set; }

        public TypeContext(string _namespace, string _type, MethodContext[] _methods,ConstructorContext _constructor) {
            Namespace = _namespace;
            Type = _type;
            Methods = _methods;
            Constructor = _constructor;
        }

        public TypeContext(string _namespace, string _type):this(_namespace,_type,null,null) { 
        }
        public TypeContext(string _namespace) : this(_namespace, null, null,null)
        {
        }
    }
}
