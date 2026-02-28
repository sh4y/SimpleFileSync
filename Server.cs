using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

public static class FileSyncServer
{
    public static async Task RunServer(string targetFolder)
    {
        // 2. Ensure target folder exists
        Directory.CreateDirectory(targetFolder);
        Console.WriteLine($"[Server] Listening on port {Program.PORT}. Saving files to: {targetFolder}");

        // 3. Start TCP Listener
        TcpListener listener = TcpListener.Create(Program.PORT);
        listener.Start();

        while (true)
        {
            // 4. Accept incoming client connection
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClientConnection(client, targetFolder));
        }
    }

    private static async Task HandleClientConnection(TcpClient client, string targetFolder)
    {
        try
        {
            using (client)
            using (var stream = client.GetStream())
            using (var reader = new BinaryReader(stream))
            {
                // 5. Read relative file path
                string relativePath = reader.ReadString();
                
                // 6. Read file length
                long fileLength = reader.ReadInt64();
                
                string destinationPath = Path.Combine(targetFolder, relativePath);
                
                // 7. Ensure destination directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

                // 8. Read file content and save to disk
                using (var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[8192];
                    long totalRead = 0;
                    while (totalRead < fileLength)
                    {
                        int read = await stream.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, fileLength - totalRead));
                        if (read == 0) break;
                        await fs.WriteAsync(buffer, 0, read);
                        totalRead += read;
                    }
                }
                
                Console.WriteLine($"[Server] Received updated file: {relativePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Error receiving file: {ex.Message}");
        }
    }
}
