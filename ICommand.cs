using System;
using System.Collections.Generic;
using System.Text;
using Nethereum.Web3;

namespace nethereumTest
{
    interface ICommand
    {
        string Name { get; }

        int ArgumentCount { get; }

        string Run(Web3 web3, IDictionary<string, string> environment, params string[] paramters);

    }
}
