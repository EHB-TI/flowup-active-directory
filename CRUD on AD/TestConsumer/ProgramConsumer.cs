﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib;
using Lib.XMLFlow;

namespace TestConsumer
{
    class ProgramConsumer
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Initializing Consumer...");

            ConsumerV2.getMessage();
        }
    }
}
