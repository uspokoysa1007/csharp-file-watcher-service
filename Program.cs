using System;
using System.IO;

namespace FileWatcherService
{
    class Program
    {
        static string logFilePath = "file_watcher.log";
        static bool autoSort = false;

        static void Main(string[] args)
        {
            Console.Title = "C# File Watcher Service";
            Console.WriteLine("================================================================");
            Console.WriteLine("             C# REAL-TIME FILE SYSTEM WATCHER                   ");
            Console.WriteLine("================================================================");

            string targetPath = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "watched");
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
                Console.WriteLine("[*] Создана директория для наблюдения: " + targetPath);
            }

            Console.WriteLine("Наблюдаемая директория: " + Path.GetFullPath(targetPath));
            Console.WriteLine("Файл журнала          : " + Path.GetFullPath(logFilePath));
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("Команды: [A] Переключить автосортировку | [Q] Выход");
            Console.WriteLine("----------------------------------------------------------------");

            using (FileSystemWatcher watcher = new FileSystemWatcher(targetPath))
            {
                watcher.NotifyFilter = NotifyFilters.FileName 
                                     | NotifyFilters.DirectoryName 
                                     | NotifyFilters.LastWrite 
                                     | NotifyFilters.Size;

                watcher.Filter = "*.*";
                watcher.IncludeSubdirectories = false;

                watcher.Created += OnCreated;
                watcher.Changed += OnChanged;
                watcher.Deleted += OnDeleted;
                watcher.Renamed += OnRenamed;

                watcher.EnableRaisingEvents = true;

                Console.WriteLine("[+] Монитор запущен. Ожидание файловых событий...\n");

                bool running = true;
                while (running)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true).Key;
                        if (key == ConsoleKey.Q)
                        {
                            running = false;
                        }
                        else if (key == ConsoleKey.A)
                        {
                            autoSort = !autoSort;
                            Console.WriteLine("\n[*] Автосортировка: " + (autoSort ? "ВКЛЮЧЕНА" : "ВЫКЛЮЧЕНА"));
                        }
                    }
                    System.Threading.Thread.Sleep(200);
                }
            }

            Console.WriteLine("\n[+] Монитор остановлен.");
        }

        static void LogEvent(string eventType, string name, string extra = "")
        {
            string timeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string line = $"[{timeStr}] [{eventType,-7}] {name} {extra}";
            
            Console.ForegroundColor = eventType switch {
                "CREATED" => ConsoleColor.Green,
                "CHANGED" => ConsoleColor.Cyan,
                "DELETED" => ConsoleColor.Red,
                "RENAMED" => ConsoleColor.Yellow,
                _ => ConsoleColor.White
            };
            Console.WriteLine(line);
            Console.ResetColor();

            try
            {
                File.AppendAllText(logFilePath, line + Environment.NewLine);
            }
            catch { }
        }

        static void OnCreated(object sender, FileSystemEventArgs e)
        {
            LogEvent("CREATED", e.Name ?? "");
            if (autoSort && File.Exists(e.FullPath))
            {
                TrySortFile(e.FullPath);
            }
        }

        static void OnChanged(object sender, FileSystemEventArgs e)
        {
            LogEvent("CHANGED", e.Name ?? "");
        }

        static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            LogEvent("DELETED", e.Name ?? "");
        }

        static void OnRenamed(object sender, RenamedEventArgs e)
        {
            LogEvent("RENAMED", e.OldName ?? "", "-> " + (e.Name ?? ""));
        }

        static void TrySortFile(string filePath)
        {
            try
            {
                string ext = Path.GetExtension(filePath).ToLower();
                string subDir = ext switch {
                    ".txt" or ".pdf" or ".docx" or ".xlsx" => "Documents",
                    ".jpg" or ".jpeg" or ".png" or ".gif" => "Images",
                    ".zip" or ".rar" or ".7z" or ".tar" => "Archives",
                    ".cs" or ".cpp" or ".py" or ".rs" => "Code",
                    _ => "Other"
                };

                string? parent = Path.GetDirectoryName(filePath);
                if (parent == null) return;
                string targetDir = Path.Combine(parent, subDir);
                Directory.CreateDirectory(targetDir);

                string targetPath = Path.Combine(targetDir, Path.GetFileName(filePath));
                File.Move(filePath, targetPath);
                LogEvent("SORTED ", Path.GetFileName(filePath), "-> " + subDir);
            }
            catch { }
        }
    }
}
