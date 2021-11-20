using System;
using System.Collections.Generic;
using System.Text;

namespace TestGenerator
{
    public class Dummy { 
        public Dummy() { }
        public Dummy(IAsyncDisposable asyncDisposable) { 
        }

        public Dummy(ICloneable cloneable, IAsyncDisposable asyncDisposable) { 
        }

        public Dummy(ICloneable cloneable, IAsyncDisposable asyncDisposable,int fuck)
        {
        }

        public Dummy(ICloneable cloneable, int fuck)
        {
        }

        public int methodNoArgs() {
            return 0;
        }

        public void methodNoReturn(List<double[]> hard, Int16 fuck, HashSet<List<bool>> sjkd) { 
        
        }

        public (int, List<bool>) tupleMethod(int a,double b,DateTime aaa) {
            return (default, default);
        }

        public Dummy method(int i, Dummy n) {
            return null;
        } 
    }
}
