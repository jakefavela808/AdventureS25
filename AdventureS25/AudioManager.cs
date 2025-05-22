using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Media;
using System.Threading;
using System.Threading.Tasks;

namespace AdventureS25
{
    public static class AudioManager
    {
        private static readonly List<Process> macSoundEffectProcesses = new List<Process>();
        private static readonly List<SoundPlayer> windowsSoundEffectPlayers = new List<SoundPlayer>();

        private static SoundPlayer? currentPlayer;
        private static Process? currentMacProcess;
        private static CancellationTokenSource? macLoopCts;
        private static readonly object macProcessLock = new object();
        private static string? currentLoopingFile;

        public static bool IsMuted { get; private set; } = false;

        public static void ToggleMute()
        {
            IsMuted = !IsMuted;
            if (IsMuted)
            {
                Stop();
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
                    PlayLooping(currentLoopingFile);
                }
            }
        }

        public static void PlayOnce(string? fileName)
        {
            if (IsMuted) return;
            if (string.IsNullOrEmpty(fileName)) return;

            string fullPath = GetFullPath(fileName);
            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"Audio file not found: {fullPath}");
                return;
            }

            Stop();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                try
                {
                    currentMacProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "afplay",
                            Arguments = $"\"{fullPath}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    currentMacProcess.Start();
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
                    currentPlayer.Play();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing sound '{fileName}': {ex.Message}");
                    currentPlayer = null;
                }
            }
            else
            {
                // Console.WriteLine($"Audio playback not supported on this OS: {RuntimeInformation.OSDescription}");
            }
        }

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

        public static void PlayLooping(string? fileName)
        {
            if (IsMuted)
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    currentLoopingFile = fileName;
                }
                return;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                Stop();
                currentLoopingFile = null;
                return;
            }

            string fullPath = GetFullPath(fileName);
            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"Audio file not found: {fullPath}");
                Stop();
                currentLoopingFile = null;
                return;
            }

            StopMusic();
            currentLoopingFile = fileName;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                macLoopCts = new CancellationTokenSource();
                CancellationToken token = macLoopCts.Token;

                Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (!File.Exists(fullPath))
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
                                    Arguments = $"\"{fullPath}\"",
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                    RedirectStandardError = true
                                }
                            };

                            lock (macProcessLock)
                            {
                                if (token.IsCancellationRequested) break;
                                currentMacProcess = loopProcess;
                            }
                            
                            loopProcess.Start();
                            await loopProcess.WaitForExitAsync(token);

                            if (loopProcess.ExitCode != 0 && !token.IsCancellationRequested)
                            {
                                string errorOutput = await loopProcess.StandardError.ReadToEndAsync();
                                Console.WriteLine($"afplay error for '{fileName}': {errorOutput.Trim()}");
                                break;
                            }
                        }
                        catch (OperationCanceledException) 
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error in afplay loop for '{fileName}': {ex.Message}");
                            break;
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
                        if (token.IsCancellationRequested) break;
                    }
                }, token);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    currentPlayer = new SoundPlayer(fullPath);
                    currentPlayer.PlayLooping();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing looping sound '{fileName}': {ex.Message}");
                     currentPlayer = null;
                }
            }
            else
            {
                // Console.WriteLine($"Audio playback not supported on this OS: {RuntimeInformation.OSDescription}");
            }
        }

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
        }

        private static string GetFullPath(string fileName)
        {
            string baseDirectory = AppContext.BaseDirectory; 
            return Path.Combine(baseDirectory, "Audio", fileName);
        }   

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

        }
        private static string GetSoundEffectFullPath(string soundFileName)
        {
            string baseDirectory = AppContext.BaseDirectory;
            return Path.Combine(baseDirectory, "Audio", soundFileName);
        }
    }
}
