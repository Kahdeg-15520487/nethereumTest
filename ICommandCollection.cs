using System;
using System.Collections.Generic;
using System.Text;

namespace nethereumTest
{
    interface ICommandCollection
    {
        string Name { get; }
        string[] Commands { get; }
    }
}
