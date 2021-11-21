using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGeneratormain.Context
{
    public record Code
    {
        public string CodeText { get; set; }
        public string Name { get ; set; }
        public string Path { get { return @"D:\Projects\C#\Spp\Tests\OutputOfGeneration" + Name + ".txt"; } }
    }
}
