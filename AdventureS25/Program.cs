using AdventureS25;
using System;

namespace AdventureS25;

class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        Game.PlayGame();
    }

    private static void OnProcessExit(object? sender, EventArgs e)
    {
        AudioManager.Stop();
    }
}