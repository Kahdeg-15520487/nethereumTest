using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nethereumTest
{
    class ActionInput
    {
        public ActionInput(string action, string[] parameters) {
            this.Action = action;
            this.Parameters = parameters;
        }

        public string Action { get; private set; }
        public string[] Parameters { get; private set; }

        public static ActionInput Parse(string input) {
            string[] sep = input.Split(" ");
            return new ActionInput(sep[0], sep.Skip(1).ToArray());
        }
    }
}
