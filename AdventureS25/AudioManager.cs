using System;
using System.IO;
using System.Media; // Requires System.Windows.Extensions package

namespace AdventureS25
{
    public static class AudioManager
    {
        private static SoundPlayer? currentPlayer; // Can be null if no sound is playing

        // Plays a sound once asynchronously.
        public static void PlayOnce(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            string fullPath = GetFullPath(fileName);
            if (File.Exists(fullPath))
            {
                try
                {
                    // Stop any currently playing sound before starting a new one
                    Stop(); 
                    currentPlayer = new SoundPlayer(fullPath);
                    currentPlayer.Play(); // Plays asynchronously
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing sound '{fileName}': {ex.Message}");
                    currentPlayer = null; // Ensure player is null on error
                }
            }
            else
            {
                Console.WriteLine($"Audio file not found: {fullPath}");
                 currentPlayer = null; // Ensure player is null if file not found
            }
        }

        // Plays a sound looping asynchronously.
        public static void PlayLooping(string? fileName)
        {
             if (string.IsNullOrEmpty(fileName)) return;

            string fullPath = GetFullPath(fileName);
            if (File.Exists(fullPath))
            {
                try
                {
                    // Stop any currently playing sound before starting a new loop
                    Stop();
                    currentPlayer = new SoundPlayer(fullPath);
                    currentPlayer.PlayLooping(); // Plays looping asynchronously
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing looping sound '{fileName}': {ex.Message}");
                     currentPlayer = null; // Ensure player is null on error
                }
            }
            else
            {
                Console.WriteLine($"Audio file not found: {fullPath}");
                 currentPlayer = null; // Ensure player is null if file not found
            }
        }

        // Stops the currently playing sound, if any.
        public static void Stop()
        {
            currentPlayer?.Stop();
            currentPlayer?.Dispose(); // Release resources
            currentPlayer = null;
        }

        // Helper to get the full path to the audio file in the 'Audio' directory.
        private static string GetFullPath(string fileName)
        {
            // Assuming 'Audio' folder is in the same directory as the executable
            string baseDirectory = AppContext.BaseDirectory; 
            return Path.Combine(baseDirectory, "Audio", fileName);
        }
    }
}
