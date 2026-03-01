using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    public const int PORT = 13890;

    static async Task Main(string[] args)
    {
        // 1. Check command line arguments to determine if we are running as Server or Client
        if (args.Length < 1)
        {
            PrintUsage();
            return;
        }

        string mode = args[0].ToLower();

        if (mode == "server")
        {
            string folder = args.Length > 1 ? args[1] : "./SyncTarget";
            await FileSyncServer.RunServer(folder);
        }
        else if (mode == "client")
        {
            string ip = "127.0.0.1";
            string folder = "./SyncSource";
            int delaySeconds = 15;

            var clientArgs = new List<string>(args.Skip(1));
            
            // Allow configurable delay e.g., --delay 30
            int delayIndex = clientArgs.IndexOf("--delay");
            if (delayIndex >= 0 && delayIndex < clientArgs.Count - 1)
            {
                if (int.TryParse(clientArgs[delayIndex + 1], out int d)) delaySeconds = d;
                clientArgs.RemoveAt(delayIndex);
                clientArgs.RemoveAt(delayIndex);
            }

            bool init = false;
            int initIndex = clientArgs.IndexOf("--init");
            if (initIndex >= 0)
            {
                init = true;
                clientArgs.RemoveAt(initIndex);
            }

            if (clientArgs.Count == 1)
            {
                // If only one extra argument is provided, check if it looks like an IP or localhost
                if (System.Net.IPAddress.TryParse(clientArgs[0], out _) || clientArgs[0].ToLower() == "localhost")
                {
                    ip = clientArgs[0];
                }
                else
                {
                    folder = clientArgs[0]; // It's a folder path (like '.')
                }
            }
            else if (clientArgs.Count >= 2)
            {
                ip = clientArgs[0];
                folder = clientArgs[1];
            }

            FileSyncClient.RunClient(ip, folder, delaySeconds, init);
        }
        else
        {
            PrintUsage();
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  Server: sfs server <optional_folder>");
        Console.WriteLine("  Client: sfs client <optional_server_ip> <optional_folder> [--delay <seconds>] [--init]");
    }
}
