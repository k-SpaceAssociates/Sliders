using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sliders
{
    public class RegValRow
    {
        public List<string> Values { get; set; } = new();

        public string this[int index]
        {
            get => Values[index];
            set => Values[index] = value;
        }
    }

}
