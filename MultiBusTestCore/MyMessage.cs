using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiBusTest
{
    public class MyMessage
    {
        public int Server { get; set; }
        public required string MsgData { get; set; }

    }
}
