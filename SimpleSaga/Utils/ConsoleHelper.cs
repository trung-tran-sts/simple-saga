namespace SimpleSaga.Utils
{
    public static class ConsoleHelper
    {
        const ConsoleColor DefaultColor = ConsoleColor.White;

        public static void WriteLineYellow(string text)
        {
            Write(text, Console.WriteLine, ConsoleColor.Yellow);
        }

        public static void WriteYellow(string text)
        {
            Write(text, Console.Write, ConsoleColor.Yellow);
        }

        private static void Write(string text, Action<string> action, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            action(text);
            Console.ForegroundColor = DefaultColor;
        }
    }
}
