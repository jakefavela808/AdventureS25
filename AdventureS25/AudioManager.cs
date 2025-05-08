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
        // --- Sound Effect Management ---
        // Track all currently playing sound effect processes/players
        private static readonly List<Process> macSoundEffectProcesses = new List<Process>();
        private static readonly List<SoundPlayer> windowsSoundEffectPlayers = new List<SoundPlayer>();

        private static SoundPlayer? currentPlayer; // For Windows
        private static Process? currentMacProcess; // For macOS afplay process
        private static CancellationTokenSource? macLoopCts; // To cancel the looping task
        private static readonly object macProcessLock = new object(); // For thread-safe access to currentMacProcess
        private static string? currentLoopingFile; // Added to store the current looping file

        public static bool IsMuted { get; private set; } = false;

        public static void ToggleMute()
        {
            IsMuted = !IsMuted;
            if (IsMuted)
            {
                Stop(); // Stop any currently playing sound
                Typewriter.TypeLine("Audio muted.");
                Console.Clear();
                Player.Look();
            }
            else
            {
                Typewriter.TypeLine("Audio unmuted.");
                Console.Clear();
                Player.Look();
                if (!string.IsNullOrEmpty(currentLoopingFile))
                {
                    PlayLooping(currentLoopingFile); // Resume looping the stored file
                }
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

        // Stops all currently playing sound effects, but not background music.
        public static void StopAllSoundEffects()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                lock (macSoundEffectProcesses)
                {
                    foreach (var proc in macSoundEffectProcesses.ToList())
                    {
                        try { if (!proc.HasExited) proc.Kill(); } catch { }
                        try { proc.Dispose(); } catch { }
                    }
                    macSoundEffectProcesses.Clear();
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                lock (windowsSoundEffectPlayers)
                {
                    foreach (var player in windowsSoundEffectPlayers.ToList())
                    {
                        try { player.Stop(); } catch { }
                        try { player.Dispose(); } catch { }
                    }
                    windowsSoundEffectPlayers.Clear();
                }
            }
        }

        // Stops only the background music (looping audio), not sound effects.
        public static void StopMusic()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                lock (macProcessLock)
                {
                    if (currentMacProcess != null && !currentMacProcess.HasExited)
                    {
                        try { currentMacProcess.Kill(); } catch { }
                        try { currentMacProcess.Dispose(); } catch { }
                        currentMacProcess = null;
                    }
                }
                if (macLoopCts != null)
                {
                    try { macLoopCts.Cancel(); } catch { }
                    macLoopCts = null;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (currentPlayer != null)
                {
                    try { currentPlayer.Stop(); } catch { }
                    try { currentPlayer.Dispose(); } catch { }
                    currentPlayer = null;
                }
            }
        }

        // Plays a sound looping asynchronously.
        public static void PlayLooping(string? fileName)
        {
            // Scenario 1: Currently muted.
            // Update currentLoopingFile if a new fileName is provided (so unmuting plays the new desired track),
            // but do not play any sound now.
            if (IsMuted)
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    currentLoopingFile = fileName;
                }
                // If fileName is null/empty and muted, currentLoopingFile remains as it was (for unmuting to resume previous).
                return; // Do not proceed to play.
            }

            // Scenario 2: Not muted, but fileName is null or empty.
            // This implies a request to stop looping music and forget the track.
            if (string.IsNullOrEmpty(fileName))
            {
                Stop(); // Stop any active sound.
                currentLoopingFile = null; // Forget the looping file.
                return;
            }

            // Scenario 3: Not muted, fileName is provided, but file doesn't exist.
            string fullPath = GetFullPath(fileName);
            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"Audio file not found: {fullPath}");
                Stop(); // Stop any active sound.
                currentLoopingFile = null; // Forget the looping file as the new one is invalid.
                return;
            }

            // Scenario 4: Not muted, fileName is valid, file exists.
            // This is the main path to play a new looping sound.
            StopMusic(); // Only stop music, not sound effects.
            currentLoopingFile = fileName; // Set the new file as the current looping one.

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
                lock (macProcessLock)
                {
                    if (currentMacProcess != null && !currentMacProcess.HasExited)
                    {
                        try { currentMacProcess.Kill(); } catch { }
                        finally { currentMacProcess.Dispose(); }
                    }
                    currentMacProcess = null;
                }
                macLoopCts?.Cancel();
                macLoopCts = null;

                // Stop all sound effect processes
                lock (macSoundEffectProcesses)
                {
                    foreach (var proc in macSoundEffectProcesses)
                    {
                        try { if (!proc.HasExited) proc.Kill(); } catch { }
                        try { proc.Dispose(); } catch { }
                    }
                    macSoundEffectProcesses.Clear();
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                currentPlayer?.Stop();
                currentPlayer?.Dispose();
                currentPlayer = null;

                // Stop all sound effect players
                lock (windowsSoundEffectPlayers)
                {
                    foreach (var player in windowsSoundEffectPlayers)
                    {
                        try { player.Stop(); } catch { }
                        try { player.Dispose(); } catch { }
                    }
                    windowsSoundEffectPlayers.Clear();
                }
            }
            // currentLoopingFile = null; // Clear the looping file when explicitly stopped, unless mute is just toggling
        }

        // Helper to get the full path to the audio file in the 'Audio' directory.
        private static string GetFullPath(string fileName)
        {
            // Assuming 'Audio' folder is in the same directory as the executable
            string baseDirectory = AppContext.BaseDirectory; 
            return Path.Combine(baseDirectory, "Audio", fileName);
        }
        /// <summary>
        /// Plays a sound effect (e.g., Input.wav) without interrupting background music.
        /// Sound effects are loaded from the 'Audio/Sounds' subfolder.
        /// This supports concurrent playback: on Windows, each sound effect uses a new SoundPlayer instance;
        /// on macOS, each sound effect launches a separate afplay process.
        /// </summary>
        /// <param name="soundFileName">The file name of the sound effect (e.g., "Input.wav")</param>
        public static void PlaySoundEffect(string soundFileName)
        {
            if (IsMuted) return;
            if (string.IsNullOrEmpty(soundFileName)) return;

            string fullPath = GetSoundEffectFullPath(soundFileName);
            if (!File.Exists(fullPath))
            {
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Launch afplay in a separate process and track it
                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "afplay",
                            Arguments = $"\"{fullPath}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.EnableRaisingEvents = true;
                    process.Exited += (s, e) =>
                    {
                        lock (macSoundEffectProcesses)
                        {
                            macSoundEffectProcesses.Remove(process);
                        }
                        try { process.Dispose(); } catch { }
                    };
                    process.Start();
                    lock (macSoundEffectProcesses)
                    {
                        macSoundEffectProcesses.Add(process);
                    }
                }
                catch { }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Play sound effect on a new SoundPlayer instance/thread and track it
                Task.Run(() =>
                {
                    SoundPlayer? player = null;
                    try
                    {
                        player = new SoundPlayer(fullPath);
                        lock (windowsSoundEffectPlayers)
                        {
                            windowsSoundEffectPlayers.Add(player);
                        }
                        player.PlaySync();
                    }
                    catch { }
                    finally
                    {
                        if (player != null)
                        {
                            lock (windowsSoundEffectPlayers)
                            {
                                windowsSoundEffectPlayers.Remove(player);
                            }
                            try { player.Dispose(); } catch { }
                        }
                    }
                });
            }
            // else: unsupported OS, do nothing
        }

        /// <summary>
        /// Gets the full path to a sound effect file in the 'Audio' directory.
        /// </summary>
        private static string GetSoundEffectFullPath(string soundFileName)
        {
            string baseDirectory = AppContext.BaseDirectory;
            return Path.Combine(baseDirectory, "Audio", soundFileName);
        }
    }
}
