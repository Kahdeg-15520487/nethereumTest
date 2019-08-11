using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts.Managed;
using Sharprompt;

namespace nethereumTest
{
    class Program
    {
        static string LOCAL_ROOT = AppDomain.CurrentDomain.BaseDirectory;
        static void Main(string[] args) {
            string senderAddress = string.Empty;
            string password = string.Empty;
            string contract = string.Empty;
            string contractAddress = string.Empty;
            if (args.Length != 4) {
                senderAddress = Prompt.Input<string>("adress:");
                password = Prompt.Input<string>("password:");
                contract = Prompt.Input<string>("contract path:");
                contractAddress = Prompt.Input<string>("contract address:");
            } else {
                senderAddress = args[0];
                password = args[1];
                contract = args[2];
                contractAddress = args[3];
            }

            string abi = File.ReadAllText(contract + ".abi");
            string bin = File.ReadAllText(contract + ".bin");

            GetAccountBalance(senderAddress).Wait();

            // string contractAddress = DeployContract(senderAddress, password, contract).GetAwaiter().GetResult();

            ManagedAccount account = new ManagedAccount(senderAddress, password);
            Web3 web3 = new Web3(account, "http://127.0.0.1:8545/rpc/");
            Dictionary<string, ICommand> commands = GetAllCommands();
            Dictionary<string, string> environment = new Dictionary<string, string>();
            environment.Add("senderAddress", senderAddress);
            environment.Add("abi", abi);
            environment.Add("bin", bin);
            environment.Add("contractAddress", contractAddress);

            bool isExit = false;
            do {
                ActionInput action = ActionInput.Parse(Prompt.Input<string>("> "));
                switch (action.Action) {
                    case "bal":
                        GetAccountBalance(web3, senderAddress).Wait();
                        break;
                    case "call":

                        break;

                    case "getKey":
                        string transactionHash = GetKey(web3, environment).GetAwaiter().GetResult();
                        break;
                    case "listfunc":
                        break;
                    case "listcmd":
                        break;
                    case "exit":
                        isExit = true;
                        break;
                    default:
                        if (commands.ContainsKey(action.Action)) {
                            commands[action.Action].Run(web3, environment, action.Parameters);
                        }
                        Console.WriteLine("Unknown action : {0}", action);
                        break;
                }
            } while (!isExit);
        }

        private static async Task<string> GetKey(Web3 web3, IDictionary<string, string> env) {
            string abi = env["abi"];
            string contractAddress = env["contractAddress"];
            string senderAddress = env["senderAddress"];
            Contract contract = web3.Eth.GetContract(abi, contractAddress);
            Function function = contract.GetFunction("getKey");
            HexBigInteger gasForDeployContract = new HexBigInteger(1000000);
            string transactionHash = await function.SendTransactionAsync(senderAddress, gasForDeployContract);
            return transactionHash;
        }

        private static Dictionary<string, ICommand> GetAllCommands() {
            Dictionary<string, ICommand> cmds = GetDirectoryPlugins<ICommand>(LOCAL_ROOT).ToDictionary(ksel => ksel.Name);
            return cmds;
        }

        public static List<T> GetFilePlugins<T>(string filename) {
            List<T> ret = new List<T>();
            if (File.Exists(filename)) {
                Type typeT = typeof(T);
                Assembly assembly;
                try {
                    assembly = Assembly.LoadFrom(filename);
                }
                catch {
                    return ret;
                }
                foreach (Type type in assembly.GetTypes()) {
                    if (!type.IsClass || type.IsNotPublic)
                        continue;
                    if (typeT.IsAssignableFrom(type)) {
                        T plugin = (T)Activator.CreateInstance(type);
                        ret.Add(plugin);
                    }
                }
            }
            return ret;
        }


        public static List<T> GetDirectoryPlugins<T>(string dirname) {
            List<T> ret = new List<T>();
            string[] dlls = Directory.EnumerateFiles(dirname).Where(x => x.EndsWith(".dll") || x.EndsWith(".exe")).ToArray();
            foreach (string dll in dlls) {
                List<T> dll_plugins = GetFilePlugins<T>(Path.GetFullPath(dll));
                ret.AddRange(dll_plugins);
            }
            return ret;
        }

        static async Task<string> DeployContract(string senderAddress, string password, string contract) {
            ManagedAccount account = new ManagedAccount(senderAddress, password);
            Web3 web3 = new Web3(account, "http://127.0.0.1:8545/rpc/");

            Console.WriteLine(senderAddress);
            if (!File.Exists(contract + ".abi")) {
                Console.WriteLine(contract + ".abi does not exist!");
                throw new FileNotFoundException(contract + ".abi");
            }

            string abi = await File.ReadAllTextAsync(contract + ".abi");
            Console.WriteLine(abi.Substring(0, 20));
            string bin = await File.ReadAllTextAsync(contract + ".bin");
            Console.WriteLine(bin.Substring(0, 20));
            HexBigInteger gasForDeployContract = new HexBigInteger(1000000);
            string transactionHash = await web3.Eth.DeployContract.SendRequestAsync(abi, bin, senderAddress, gasForDeployContract);

            return transactionHash;
        }

        static async Task GetAccountBalance(string senderAddress) {
            Web3 web3 = new Web3("http://127.0.0.1:8545/rpc/");
            HexBigInteger balance = await web3.Eth.GetBalance.SendRequestAsync(senderAddress);
            Console.WriteLine($"Balance in Wei: {balance.Value}");

            decimal etherAmount = Web3.Convert.FromWei(balance.Value);
            Console.WriteLine($"Balance in Ether: {etherAmount}");
        }

        static async Task<HexBigInteger> GetAccountBalance(Web3 web3, string senderAddress) {
            HexBigInteger balance = await web3.Eth.GetBalance.SendRequestAsync(senderAddress);
            Console.WriteLine($"Balance in Wei: {balance.Value}");

            decimal etherAmount = Web3.Convert.FromWei(balance.Value);
            Console.WriteLine($"Balance in Ether: {etherAmount}");
            return balance;

        }
    }
}
