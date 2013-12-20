using Ionic.Zip;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UpdateGoagent
{
    class Program
    {
        static void Main(string[] args)
        {
            //var patchUrl = @"https://nodeload.github.com/goagent/goagent/legacy.zip/3.0";

            //var client = new RestClient(patchUrl);
            //var request = new RestRequest(Method.GET);

            //var data = client.DownloadData(request);
            byte[] bytes = null;
            DateTime start = DateTime.Now;
            using (var webClient = new WebClient())
            {
                var webproxy = new WebProxy();

                webClient.DownloadFileCompleted += (sender, e) =>
                {
                    Console.WriteLine("Over");
                };
                webClient.DownloadProgressChanged += (sender, e) =>
                {
                    var seconds = (DateTime.Now - start).TotalSeconds;
                    var percent = e.BytesReceived / 1000 / seconds;

                    Console.Write("\r{0}%-----{1}/{2} ==({3}kb/s)  ", e.ProgressPercentage, e.BytesReceived, e.TotalBytesToReceive, percent.ToString("0.00"));
                };
                var uri = new Uri("https://nodeload.github.com/goagent/goagent/legacy.zip/3.0");
                start = DateTime.Now;
                var task = webClient.DownloadDataTaskAsync(uri);
                task.Wait();
                bytes = task.Result;
            }
            Console.WriteLine();
            var targetPath = @"F:\goagent\";
            using (var memory = new MemoryStream(bytes))
            {
                DeleteCache(Path.Combine(targetPath, "goagent"));
                Console.WriteLine("开始解压到{0}", targetPath);

                using (var zip = ZipFile.Read(memory))
                {
                    var zips = zip.Entries.ToList();
                    for (int i = 0; i < zips.Count; i++)
                    {
                        var zEntriy = zips[i];
                        var names = zEntriy.FileName.Split('/');
                        names[0] = "goagent";
                        zEntriy.FileName = Path.Combine(names);
                        zEntriy.Extract(targetPath, ExtractExistingFileAction.OverwriteSilently);
                        Console.WriteLine(zEntriy.FileName);
                    }
                }
            }

            Console.WriteLine("更新完成");
            Console.ReadKey();
        }

        private static void DeleteTmpCache(string currentPath)
        {
            var deleteCache = false;
            int maxCounter = 3;
            do
            {
                deleteCache = DeleteCache(currentPath);
                maxCounter--;
            } while (!deleteCache && maxCounter > 0);
        }

        private static bool DeleteCache(string currentPath)
        {
            try
            {
                if (Directory.Exists(currentPath))
                {
                    Directory.GetFiles(currentPath, "*.tmp", SearchOption.AllDirectories).ToList().ForEach(f => System.IO.File.Delete(f));
                    Directory.GetFiles(currentPath, "*.PendingOverwrite", SearchOption.AllDirectories).ToList().ForEach(f => System.IO.File.Delete(f));
                }
            }
            catch (Exception ex)
            {
                System.Threading.Thread.Sleep(1000);
                return false;
            }
            return true;
        }
    }
    static class Code
    {
        public static void OverwriteConsoleMessage(string message)
        {
            Console.CursorLeft = 0;
            int maxCharacterWidth = Console.WindowWidth - 1;
            if (message.Length > maxCharacterWidth)
            {
                message = message.Substring(0, maxCharacterWidth - 3) + "...";
            }
            message = message + new string(' ', maxCharacterWidth - message.Length);
            Console.Write(message);
        }

        public static void RenderConsoleProgress(int percentage)
        {
            RenderConsoleProgress(percentage, '\u2590', Console.ForegroundColor, "");
        }

        public static void RenderConsoleProgress(int percentage, char progressBarCharacter,
                  ConsoleColor color, string message)
        {
            Console.CursorVisible = false;
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.CursorLeft = 0;
            int width = Console.WindowWidth - 1;
            int newWidth = (int)((width * percentage) / 100d);
            string progBar = new string(progressBarCharacter, newWidth) +
                  new string(' ', width - newWidth);
            Console.Write(progBar);
            if (string.IsNullOrEmpty(message)) message = "";
            Console.CursorTop++;
            OverwriteConsoleMessage(message);
            Console.CursorTop--;
            Console.ForegroundColor = originalColor;
            Console.CursorVisible = true;
        }


    }
}
