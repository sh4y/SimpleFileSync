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
            string folder = "./SyncTarget";
            int port = PORT;

            var serverArgs = new List<string>(args.Skip(1));
            
            int portIndex = serverArgs.IndexOf("--port");
            if (portIndex >= 0 && portIndex < serverArgs.Count - 1)
            {
                if (int.TryParse(serverArgs[portIndex + 1], out int p)) port = p;
                serverArgs.RemoveAt(portIndex);
                serverArgs.RemoveAt(portIndex);
            }

            if (serverArgs.Count >= 1)
            {
                folder = serverArgs[0];
            }

            await FileSyncServer.RunServer(folder, port);
        }
        else if (mode == "client")
        {
            string ip = "127.0.0.1";
            string folder = "./SyncSource";
            int delaySeconds = 15;
            int port = PORT;

            var clientArgs = new List<string>(args.Skip(1));
            
            // Allow configurable port e.g., --port 8080
            int clientPortIndex = clientArgs.IndexOf("--port");
            if (clientPortIndex >= 0 && clientPortIndex < clientArgs.Count - 1)
            {
                if (int.TryParse(clientArgs[clientPortIndex + 1], out int p)) port = p;
                clientArgs.RemoveAt(clientPortIndex);
                clientArgs.RemoveAt(clientPortIndex);
            }
            
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

            FileSyncClient.RunClient(ip, folder, delaySeconds, init, port);
        }
        else
        {
            PrintUsage();
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  Server: sfs server <optional_folder> [--port <port>]");
        Console.WriteLine("  Client: sfs client <optional_server_ip> <optional_folder> [--delay <seconds>] [--init] [--port <port>]");
    }
}
