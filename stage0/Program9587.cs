namespace stage0
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Welcome9587();
            Welcome3771();
            Console.ReadKey();
        }

        static partial void Welcome3771();
        private static void Welcome9587()
        {
            Console.Write("Enter your name: ");
            string userName = Console.ReadLine();
            Console.WriteLine("{0}, welcome to my first console application", userName);
        }
    }
}
