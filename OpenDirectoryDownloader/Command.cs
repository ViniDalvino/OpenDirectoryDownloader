﻿using NLog;
using OpenDirectoryDownloader.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TextCopy;

namespace OpenDirectoryDownloader
{
    public class Command
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Set console properties (Window size)
        /// </summary>
        internal static void SetConsoleProperties()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WindowWidth = Math.Min(170, Console.LargestWindowWidth);
                Console.WindowHeight = Math.Min(34, Console.LargestWindowHeight);
            }
        }

        internal static void ShowInfoAndCommands()
        {
            Console.WriteLine(
                "┌─────────────────────────────────────────────────────────────────────────┐\n" +
                "│  Press I for info (this)                                                │\n" +
                "│  Press S for statistics                                                 │\n" +
                "│  Press T for thread info                                                │\n" +
                "│  Press J for Save JSON                                                  │\n" +
                "├─────────────────────────────────────────────────────────────────────────┤\n" +
                "│  Press ESC or X to EXIT                                                 │\n" +
                "└─────────────────────────────────────────────────────────────────────────┘\n");
                "****************************************************************************"
            );
                "****************************************************************************"
            );
                "****************************************************************************"
            );
        }

        internal static void ProcessConsoleInput(OpenDirectoryIndexer openDirectoryIndexer)
        {
            if (Console.IsInputRedirected)
            {
                string message = "Console input is redirect, maybe it is run inside another host. This could mean that no input will be send/processed.";
                Console.WriteLine(message);
                Logger.Warn(message);
            }

            while (true)
            {
                try
                {
                    if (Console.IsInputRedirected)
                    {
                        int keyPressed = Console.Read();

                        if (keyPressed == -1)
                        {
                            // Needed, when input is redirected it will immediately return with -1
                            Task.Delay(10).Wait();
                        }

                        //if (char.IsControl((char)keyPressed))
                        //{
                        //    Console.WriteLine($"Pressed Console.Read(): {keyPressed}");
                        //}
                        //else
                        //{
                        //    Console.WriteLine($"Pressed Console.Read(): {(char)keyPressed}");
                        //}

                        switch (keyPressed)
                        {
                            case 'x':
                            case 'X':
                                KillApplication();
                                break;
                            case 'i':
                                ShowInfoAndCommands();
                                break;
                            case 'c':
                                if (OpenDirectoryIndexer.Session.Finished != DateTimeOffset.MinValue)
                                {
                                    new Clipboard().SetText(Statistics.GetSessionStats(OpenDirectoryIndexer.Session, includeExtensions: true, onlyRedditStats: true));
                                    KillApplication();
                                }
                                break;
                            case 's':
                            case 'S':
                                ShowStatistics(openDirectoryIndexer);
                                break;
                            case 't':
                            case 'T':
                                ShowThreads(openDirectoryIndexer);
                                break;
                            case 'j':
                            case 'J':
                                SaveSession(openDirectoryIndexer);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        if (!Console.KeyAvailable)
                        {
                            Task.Delay(10).Wait();
                            continue;
                        }
                        ConsoleKey keyPressed = Console.ReadKey(intercept: true).Key;
                        //Console.WriteLine($"Pressed (Console.ReadKey(): {keyPressed}");

                        switch (keyPressed)
                        {
                            case ConsoleKey.X:
                            case ConsoleKey.Escape:
                                KillApplication();
                                break;
                            case ConsoleKey.I:
                                ShowInfoAndCommands();
                                break;
                            case ConsoleKey.C:
                                if (OpenDirectoryIndexer.Session.Finished != DateTimeOffset.MinValue)
                                {
                                    try
                                    {
                                        new Clipboard().SetText(Statistics.GetSessionStats(OpenDirectoryIndexer.Session, includeExtensions: true, onlyRedditStats: true));
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error($"Error copying stats to clipboard: {ex.Message}");
                                    }

                                    KillApplication();
                                }
                                break;
                            case ConsoleKey.S:
                                ShowStatistics(openDirectoryIndexer);
                                break;
                            case ConsoleKey.T:
                                ShowThreads(openDirectoryIndexer);
                                break;
                            case ConsoleKey.J:
                                SaveSession(openDirectoryIndexer);
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error processing action");
                    throw;
                }
            }
        }

        private static void SaveSession(OpenDirectoryIndexer openDirectoryIndexer)
        {
            try
            {
                Console.WriteLine("Saving session to JSON");
                Library.SaveSessionJson(OpenDirectoryIndexer.Session);
                Console.WriteLine("Saved session to JSON");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public static void KillApplication()
        {
            Console.WriteLine("Exiting...");
            Logger.Info("Exiting...");
            Environment.Exit(1);
        }

        private static void ShowThreads(OpenDirectoryIndexer openDirectoryIndexer)
        {
            Console.WriteLine($"Running threads:");

            lock (openDirectoryIndexer.WebDirectoryProcessorInfoLock)
            {
                foreach (KeyValuePair<string, WebDirectory> webDirectory in openDirectoryIndexer.WebDirectoryProcessorInfo.OrderBy(i => i.Key))
                {
                    Console.WriteLine($"[{webDirectory.Key}] {Library.FormatWithThousands((DateTimeOffset.UtcNow - webDirectory.Value.StartTime).TotalMilliseconds)}ms | {webDirectory.Value.Url}");
                }
            }
        }

        private static void ShowStatistics(OpenDirectoryIndexer openDirectoryIndexer)
        {
            Console.WriteLine(Statistics.GetSessionStats(OpenDirectoryIndexer.Session, includeExtensions: true));
            Console.WriteLine($"Queue: {Library.FormatWithThousands(openDirectoryIndexer.WebDirectoriesQueue.Count)}, Threads: {openDirectoryIndexer.RunningWebDirectoryThreads}");
            Console.WriteLine($"Queue (filesize): {Library.FormatWithThousands(openDirectoryIndexer.WebFilesFileSizeQueue.Count)}, Threads (filesize): {openDirectoryIndexer.RunningWebFileFileSizeThreads}");
        }
    }
}
