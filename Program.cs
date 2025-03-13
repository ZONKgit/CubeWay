using CubeWay;

namespace CubeWay
{
    class Program
    {
        public static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            using (Game game = new Game())
            {
                game.Run();
            }
        }
    }
}