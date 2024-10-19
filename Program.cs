using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RockPaperScissorsLizardSpockClient
{
    class Program
    {
        private static TcpClient client;
        private static NetworkStream stream;

        static async Task Main()
        {
            client = new TcpClient();
            await ConnectToServerAsync();

            Console.WriteLine("Выберите режим игры: (Человек-человек, Человек-компьютер, Компьютер-компьютер)");
            string? gameMode = Console.ReadLine();
            Console.WriteLine($"Игра в режиме: {gameMode}");

            for (int i = 0; i < 5; i++)
            {
                string playerMove = string.Empty;
                string computerMove1 = string.Empty;
                string computerMove2 = string.Empty;

                if (gameMode == "Человек-компьютер")
                {
                    Console.WriteLine("Ваш ход (Камень, Ножницы, Бумага, Ящерица, Спок): ");
                    playerMove = Console.ReadLine();
                    computerMove1 = GetComputerMove();
                    Console.WriteLine($"Компьютер выбрал: {computerMove1}");
                    await SendMoves(playerMove, computerMove1);
                }
                else if (gameMode == "Компьютер-компьютер")
                {
                    computerMove1 = GetComputerMove();
                    computerMove2 = GetComputerMove();
                    Console.WriteLine($"Компьютер 1 выбрал: {computerMove1}");
                    Console.WriteLine($"Компьютер 2 выбрал: {computerMove2}");
                    await SendMoves(computerMove1, computerMove2);
                }
                else // Человек-человек
                {
                    Console.WriteLine("Ваш ход (Камень, Ножницы, Бумага, Ящерица, Спок): ");
                    playerMove = Console.ReadLine();
                    await SendMoves(playerMove, ""); 
                }

                string result = await ReceiveResult();
                Console.WriteLine($"Результат раунда: {result}");

                if (i == 4) 
                {
                    Console.WriteLine("Игра завершена. Соединение закрыто.");
                    break;
                }
            }

            client.Close();
            Console.WriteLine("Соединение закрыто.");
        }

        private static async Task ConnectToServerAsync()
        {
            try
            {
                await client.ConnectAsync("127.0.0.1", 5500);
                stream = client.GetStream();
                Console.WriteLine("Успешно подключено к серверу!");
            }
            catch (SocketException)
            {
                Console.WriteLine("Не удалось подключиться к серверу.");
                Environment.Exit(0);
            }
        }

        private static async Task SendMoves(string playerMove, string computerMove)
        {
            string message = string.IsNullOrEmpty(computerMove) ? playerMove : $"{playerMove},{computerMove}";
            byte[] data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(data, 0, data.Length);
        }

        private static async Task<string> ReceiveResult()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
        }

        private static string GetComputerMove()
        {
            Random random = new Random();
            int choice = random.Next(0, 5);
            string[] moves = { "Камень", "Ножницы", "Бумага", "Ящерица", "Спок" };
            return moves[choice];
        }
    }
}
