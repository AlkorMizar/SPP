using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator.Context
{
    public class ConstructorContext
    {
        public List<(string type, string name,bool isDependency)> Parameters { get; set; }
        public ConstructorContext() {
            Parameters = new List<(string type, string name, bool isDependency)>();
        }

        public (int depend,int other) getCount() {
            int dCount = 0, oCount=0;

            foreach (var param in Parameters)
            {
                if (param.isDependency)
                {
                    dCount++;
                }
                else 
                {
                    oCount++;
                }
            }
            return (dCount, oCount);
        }
    }
}
