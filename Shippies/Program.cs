using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Shippies
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Shippies Client";
            Console.WriteLine("\n=================================================");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("                 Welcome to Shippies!             ");
            Console.ResetColor();
            Console.WriteLine("=================================================\n");

            // Start the client
            string serverIp = "";
            TcpClient client = null;

            while (client == null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("To begin, please connect to a Shippies server.");
                Console.ResetColor();
                Console.WriteLine("-------------------------------------------------");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Enter the server IP address or 'exit' to quit: ");
                serverIp = Console.ReadLine()?.Trim();
                Console.ResetColor();

                if (string.Equals(serverIp, "exit", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                try
                {
                    client = new TcpClient(serverIp, 3000);
                }
                catch (Exception e)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"An error occurred trying to connect: {e.Message}");
                    Console.WriteLine("Please try again.\n");
                    Console.ResetColor();
                    client = null;
                }
            }

            using (client)
            using (StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.ASCII) { AutoFlush = true })
            using (StreamReader reader = new StreamReader(client.GetStream(), Encoding.ASCII))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nConnected to the server. Type \"ready\" to ready up or \"dc\" to disconnect.");
                Console.ResetColor();

                Thread listenThread = new Thread(() => ListenForServerMessages(reader));
                listenThread.Start();

                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    string messageToSend = Console.ReadLine();
                    Console.ResetColor();

                    if (string.IsNullOrEmpty(messageToSend))
                    {
                        continue;
                    }

                    writer.WriteLine(messageToSend);

                    if (messageToSend.ToLower() == "dc")
                    {
                        break;
                    }
                }
            }
        }


        static void ListenForServerMessages(StreamReader reader)
        {
            try
            {
                while (true)
                {
                    string serverMessage = reader.ReadLine();
                    if (serverMessage != null)
                    {
                        if (serverMessage.StartsWith("SERVER_NAME:"))
                        {
                            var serverName = serverMessage.Substring("SERVER_NAME:".Length);
                            Console.Title = $"Shippies Client - {serverName}";
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"Welcome to {serverName}!");
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Instructions:");
                            Console.WriteLine("1. Type \"ready\" when you are ready to play.");
                            Console.WriteLine("2. Once both players are ready, you will enter the ship placement phase.");
                            Console.WriteLine("   - You will place your ships on your grid by specifying their start and end positions.");
                            Console.WriteLine("3. After placing all ships, type 'done' and you'll enter the attack phase where you can target your opponent's grid.");
                            Console.WriteLine("4. Type coordinates (e.g., A5) to attack. The goal is to sink all of your opponent's ships.");
                            Console.WriteLine("5. Type \"dc\" at any time to disconnect from the server.");
                            Console.WriteLine("\nEnjoy the game and may the best strategist win!");
                            Console.ResetColor();
                        }
                        else if (serverMessage.Equals("CLEAR_CONSOLE", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.Clear();
                        }
                        else if (serverMessage.Equals("CLEAR_CONSOLE_WHOLE", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.Clear();
                            Console.WriteLine("\x1b[3J");
                            Console.Clear();
                        }
                        else
                        {
                            Console.WriteLine(serverMessage);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in receiving server message: {e.Message}");
                Console.ResetColor();
            }
        }

    }
}
