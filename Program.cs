using System.Threading;
using System;
using System.Collections.Generic;
using YoutubeSearch;
using System.IO;
using VideoLibrary;
using MediaToolkit.Model;
using MediaToolkit;
using System.Diagnostics;

namespace YouTubeInfo
{
    class Program
    {
        static int i = 1;
        static int number;
        static string link;
        static string directory = @"C:\Users\ktomc\Downloads\";
        static void Main()
        {
            VideoSearch items = new VideoSearch(); 
            Console.WriteLine
            (
            @"    
____  ____             __________                        __           _
\   \/   /            |___    ___|       _              |  |         / /
 \      /                 |  |          / |             |  |       _| |_
  \    /   ___    _    _  |  |  _    _  | |__     __    |  |  ___ |_   _| ___
   |  |   /   \  | |  | | |  | | |  | | |    \   / __\  |  | |   \  | |  /   \ 
   |  |  |  |  | | |__| | |  | | |__| | |  |  | | \___  |  | | |  | | | |  |  |
   |__|   \___/   \____/  |__|  \____/   \___/   \___/  |__| |_|__| |_|  \___/
            ");
            Console.WriteLine("Enter title of the video you want to peek:");
            string videoName = Console.ReadLine();
            int pagesToCheck = 1;

            List<VideoInformation> list = items.SearchQuery(videoName, pagesToCheck);
            Console.WriteLine("Did you mean: " + list[0].Title + "?");
            if (Confirm()) ShowAllInfo(list[0], true);
            else SelectVideo(list, true);
        }

        private static bool Confirm()
        {
            Console.WriteLine("[Y/N]");
            while(true)
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.Y: return true;
                    case ConsoleKey.N: return false;
                }
            }
        }

        static void ShowAllInfo(VideoInformation video, bool clearAfter)
        {
            if(clearAfter) Console.Clear();

            Console.WriteLine("=== VIDEO DETAILS ===");
            Console.WriteLine("Title: " + video.Title);
            if (video.Author == "") Console.WriteLine("Author: UNKNOWN"); //TODO find out why author is always unknown
            else Console.WriteLine("Author: " + video.Author);
            if (video.Description == "") Console.WriteLine("Description: NONE");
            else Console.WriteLine("Description: " + video.Description);
            Console.WriteLine("Duration: " + video.Duration);
            Console.WriteLine("Thumbnail: " + video.Thumbnail);
            Console.WriteLine("Url: " + video.Url);
            link = video.Url;
            Options();
        }

        static void SelectVideo(List<VideoInformation> list, bool clearAfter) //TODO arrows support
        {
            i = 1;

            if(clearAfter) Console.Clear();
            Console.WriteLine("=== SELECT VIDEO ===");
            foreach (var item in list) Console.WriteLine(i++ + ". " + item.Title);

            bool parsable = Int32.TryParse(Console.ReadLine(), out number);
            if (parsable) ShowAllInfo(list[number-1],true);
            else SelectVideo(list,clearAfter); //TODO what if video is not on the list?
        }

        static void Options() //TODO make this function more flexible
        {
            Console.WriteLine("\nWhat would you like to do?");//TODO add arrows support to the options
            Console.WriteLine("1. Download MP3");
            Console.WriteLine("2. Download MP4");
            Console.WriteLine("3. Search for another video");
            Console.WriteLine("4. Exit");

            bool goodOption = false;
            do
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        goodOption = true;
                        Thread downloadMP3Thread = new Thread(()=>DownloadMP3(link));
                        downloadMP3Thread.Start();
                        loadingAnimation(downloadMP3Thread,2);
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        goodOption = true;
                        Thread downloadMP4Thread = new Thread(()=>DownloadMP4(link));
                        downloadMP4Thread.Start();
                        loadingAnimation(downloadMP4Thread,1);
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        goodOption = true;
                        Console.Clear();
                        Main();
                        break;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        goodOption = true;
                        Environment.Exit(1);
                        break;
                }
            } while (!goodOption);
            //TODO show next options after finishig the job
        }

        static void DownloadMP3(string link)
        {
            YouTube youTube = YouTube.Default; //TODO handle connection error exception
            YouTubeVideo video = youTube.GetVideo(link);
            File.WriteAllBytes(directory + video.FullName, video.GetBytes()); //TODO let user change directory

            var inputFile = new MediaFile { Filename = directory + video.FullName}; //TODO let user choose a name
            var outputFile = new MediaFile { Filename = directory + video.FullName.Remove(video.FullName.Length-4) + ".mp3"};

            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);
                engine.Convert(inputFile, outputFile);
            }
            Console.WriteLine("\nMP3 downloaded successfully");
            Process.Start("explorer.exe", directory);
        }

        static void DownloadMP4(string link)
        {
            YouTube youTube = YouTube.Default;
            YouTubeVideo video = youTube.GetVideo(link);
            File.WriteAllBytes(directory + video.FullName, video.GetBytes()); //TODO let user change directory
            Console.WriteLine("\nMP4 downloaded successfully"); //TODO make a progress bar
            Process.Start("explorer.exe", directory);
        }

        #region loadingCommets
        /// <summary>
        /// This function show a loading animation, as long selected thread is on
        /// </summary>
        /// <param name="thread">Thread to watch</param>
        /// <param name="type">
        /// Type of animation
        ///     <para/>1: Spinner type
        ///     <para/>2: Dots type
        /// </param>
        #endregion
        static void loadingAnimation(Thread thread, int type)
        {
            int i = 0;
            switch (type)
            {
                case 1: 
                    while (thread.IsAlive)
                    {
                        Console.Write("/");
                        System.Threading.Thread.Sleep(500);
                        Console.Write("\b");
                        Console.Write("-");
                        System.Threading.Thread.Sleep(500);
                        Console.Write("\b");
                        Console.Write(@"\");
                        System.Threading.Thread.Sleep(500);
                        Console.Write("\b");
                        Console.Write("|");
                        System.Threading.Thread.Sleep(500);
                        Console.Write("\b");
                    }
                    break;
                case 2:
                    while (thread.IsAlive)
                    {
                        Console.Write(".");
                        System.Threading.Thread.Sleep(200);
                        if (i++ > 4)
                        {
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write("     ");
                            Console.SetCursorPosition(0, Console.CursorTop);
                            i = 0;
                        }
                    }
                    break;
            }
        }
    }
}