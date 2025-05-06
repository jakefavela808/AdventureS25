using AdventureS25;
using System;

namespace AdventureS25;

class Program
{
    public static void Main(string[] args)
    {
        // Ensure Unicode block characters display correctly
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        // Register the ProcessExit event handler
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        Game.PlayGame();
    }

    // Event handler for ProcessExit
    private static void OnProcessExit(object? sender, EventArgs e)
    {
        AudioManager.Stop();
    }
}