using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AdventureS25
{
    public static class Typewriter
    {
        // Easily adjustable timings for each character type (in milliseconds)
        public static int DefaultDelay = 30;
        public static int CommaDelay = 120;
        public static int PeriodDelay = 250;
        public static int ExclamationDelay = 250;
        public static int QuestionDelay = 250;
        public static int EllipsisDelay = 400;
        public static int NewlineDelay = 100;

        // Tracks if user wants to skip
        private static bool skipRequested = false;
        
        public static void TypeLine(string text)
        {
            skipRequested = false;
            var printTask = Task.Run(() => PrintWithTypewriter(text));
            while (!printTask.IsCompleted)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                    {
                        skipRequested = true;
                    }
                }
                Thread.Sleep(10);
            }
            Console.WriteLine();
        }

        private static void PrintWithTypewriter(string text)
        {
            int i = 0;
            while (i < text.Length)
            {
                if (skipRequested)
                {
                    Console.Write(text.Substring(i));
                    return;
                }
                char c = text[i];
                Console.Write(c);
                int delay = GetDelay(text, i);
                Task.Delay(delay).Wait();
                i++;
            }
        }

        private static int GetDelay(string text, int idx)
        {
            // Handle ellipsis
            if (text.Substring(idx).StartsWith("..."))
                return EllipsisDelay;
            char c = text[idx];
            switch (c)
            {
                case ',': return CommaDelay;
                case '.': return PeriodDelay;
                case '!': return ExclamationDelay;
                case '?': return QuestionDelay;
                case '\n': return NewlineDelay;
                default: return DefaultDelay;
            }
        }
    }
}
