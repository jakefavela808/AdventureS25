using System;
using System.IO;
using System.Runtime.InteropServices; // Added for OSPlatform
using System.Diagnostics;         // Added for Process
using System.Media; // Requires System.Windows.Extensions package
using System.Threading; // Added for CancellationTokenSource and CancellationToken
using System.Threading.Tasks; // Added for Task.Run and async/await

namespace AdventureS25
{
    public static class AudioManager
    {
        private static SoundPlayer? currentPlayer; // For Windows
        private static Process? currentMacProcess; // For macOS afplay process
        private static CancellationTokenSource? macLoopCts; // To cancel the looping task
        private static readonly object macProcessLock = new object(); // For thread-safe access to currentMacProcess

        public static bool IsMuted { get; private set; } = false;

        public static void ToggleMute()
        {
            IsMuted = !IsMuted;
            if (IsMuted)
            {
                Stop(); // Stop any currently playing sound
                Typewriter.TypeLine("Audio muted.");
            }
            else
            {
                Typewriter.TypeLine("Audio unmuted.");
                // Optional: Consider replaying current location's audio if applicable
                // For example, if Player.CurrentLocation.AudioFile is accessible and should resume:
                // if (Player.CurrentLocation != null && !string.IsNullOrEmpty(Player.CurrentLocation.AudioFile))
                // {
                //     PlayLooping(Player.CurrentLocation.AudioFile);
                // }
            }
        }

        // Plays a sound once asynchronously.
        public static void PlayOnce(string? fileName)
        {
            if (IsMuted) return;
            if (string.IsNullOrEmpty(fileName)) return;

            string fullPath = GetFullPath(fileName);
            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"Audio file not found: {fullPath}");
                return; // Return early if file not found
            }

            Stop(); // Stop any currently playing sound before starting a new one

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                try
                {
                    currentMacProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "afplay",
                            Arguments = $"\"{fullPath}\"", // Quote path for spaces
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    currentMacProcess.Start();
                    // afplay plays asynchronously and exits when done
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing sound '{fileName}' with afplay: {ex.Message}");
                    currentMacProcess = null;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
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
                // Console.WriteLine($"Audio playback not supported on this OS: {RuntimeInformation.OSDescription}");
            }
        }

        // Plays a sound looping asynchronously.
        public static void PlayLooping(string? fileName)
        {
             if (IsMuted) return;
             if (string.IsNullOrEmpty(fileName)) return;

            string fullPath = GetFullPath(fileName);
            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"Audio file not found: {fullPath}");
                return; // Return early if file not found
            }

            Stop(); // Stop any currently playing sound before starting a new one

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                macLoopCts = new CancellationTokenSource();
                CancellationToken token = macLoopCts.Token;

                Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (!File.Exists(fullPath)) // Check if file still exists before playing
                        {
                            Console.WriteLine($"Audio file for loop not found: {fullPath}");
                            break; 
                        }

                        Process? loopProcess = null;
                        try
                        {
                            loopProcess = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "afplay",
                                    Arguments = $"\"{fullPath}\"", // Quote path for spaces
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                    RedirectStandardError = true // Capture errors
                                }
                            };

                            lock (macProcessLock)
                            {
                                if (token.IsCancellationRequested) break;
                                currentMacProcess = loopProcess;
                            }
                            
                            loopProcess.Start();
                            await loopProcess.WaitForExitAsync(token); // Asynchronously wait for exit or cancellation

                            if (loopProcess.ExitCode != 0 && !token.IsCancellationRequested)
                            {
                                string errorOutput = await loopProcess.StandardError.ReadToEndAsync();
                                Console.WriteLine($"afplay error for '{fileName}': {errorOutput.Trim()}");
                                break; // Stop looping on error
                            }
                        }
                        catch (OperationCanceledException) 
                        {
                            // Loop was cancelled, expected during Stop()
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error in afplay loop for '{fileName}': {ex.Message}");
                            break; // Stop looping on other errors
                        }
                        finally
                        {
                            lock (macProcessLock)
                            {
                                if (currentMacProcess == loopProcess)
                                {
                                    currentMacProcess = null;
                                }
                            }
                            loopProcess?.Dispose();
                        }
                        if (token.IsCancellationRequested) break; // Check again before next iteration
                    }
                }, token);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
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
                // Console.WriteLine($"Audio playback not supported on this OS: {RuntimeInformation.OSDescription}");
            }
        }

        // Stops the currently playing sound, if any.
        public static void Stop()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                macLoopCts?.Cancel(); // Request cancellation of the loop task
                macLoopCts?.Dispose();
                macLoopCts = null;

                Process? processToKill = null;
                lock (macProcessLock)
                {
                    if (currentMacProcess != null)
                    {
                        processToKill = currentMacProcess;
                        currentMacProcess = null; // Prevent new loops from using this instance
                    }
                }

                if (processToKill != null)
                {
                    try
                    {
                        if (!processToKill.HasExited)
                        {
                            processToKill.Kill();
                        }
                    }
                    catch (InvalidOperationException) 
                    {
                        // Process may have already exited
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error stopping afplay process: {ex.Message}");
                    }
                    finally
                    {
                        processToKill.Dispose();
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                currentPlayer?.Stop();
                currentPlayer?.Dispose(); // Release resources
                currentPlayer = null;
            }
            // No action needed for unsupported OS if nothing was started
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
