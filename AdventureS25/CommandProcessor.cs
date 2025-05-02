namespace AdventureS25;

public static class CommandProcessor
{
    public static Command Process()
    {
        string rawInput = GetInput();
        
        Command command = Parser.Parse(rawInput);

        Debugger.Write("Verb: [" + command.Verb + "]");
        Debugger.Write("Noun: [" + command.Noun + "]");
        
        bool isValid = CommandValidator.IsValid(command);
        command.IsValid = isValid;
        
        return command;
    }
    
    public static string GetInput()
    {
        Console.Write("> ");
        string input = "";
        int left = Console.CursorLeft;
        int top = Console.CursorTop;
        while (true)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                if (input.Length == 0)
                {
                    // If input is empty, just keep cursor at prompt
                    Console.SetCursorPosition(left, top);
                    continue;
                }
                else
                {
                    Console.WriteLine();
                    break;
                }
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (input.Length > 0)
                {
                    input = input.Substring(0, input.Length - 1);
                    int curLeft = Console.CursorLeft;
                    int curTop = Console.CursorTop;
                    if (curLeft > left)
                    {
                        Console.SetCursorPosition(curLeft - 1, curTop);
                        Console.Write(' ');
                        Console.SetCursorPosition(curLeft - 1, curTop);
                    }
                }
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                input += keyInfo.KeyChar;
                Console.Write(keyInfo.KeyChar);
            }
        }
        return input;
    }
}