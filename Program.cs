using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using YoutubeSearch;
using System.IO;
using VideoLibrary;
using MediaToolkit.Model;
using MediaToolkit;
using System.Diagnostics;

/*
    YOUTUBEINFO
    This is a console app, with can be used to peek the metadata of a YouTube video.
    But most importantly, it allows to quick download audio and/or video of a clip.
    The app will be probably upgraded in a future.

    THE TO DO LIST
    TODO let user choose a name and directory of saved file
    TODO make a fancy progress bar
    TODO add more languages
    TODO let user search on other pages
    TODO make options function more flexlible
*/

namespace YouTubeInfo
{
    partial class Program
    {
        static string link, filenameBeforeConvertion, fileName, directory = @"C:\YTinfoDownloads\";
        static int pagesToCheck = 1;
        static YouTube youTube;
        static Video video;
        static Task task;
        
        /*
            The main function. Basicly it shows the unicode logo I made, and ask user for a video title, that will be searched on YouTube.
            The function show all videos detected on 1st page and ask user if 1st result is correct.
            If not, the app will show user whole list of found videos, and asks for selecting the correct one 
            (or entering new phrase if searched video is not on the list).
            When user confirm any of results, the program will show all available info, and let user download it.
        */

        ///<summary>The main function asks user for a phrase and let him or her, select the correct result and then gives some options to do with it.</summary>
        static void Main()
        {    
            Console.Clear();
            Console.WriteLine(logo);
            Console.WriteLine("Enter title of the video you want to peek:");
            string videoName = Console.ReadLine();
            
            try
            {
                VideoSearch items = new VideoSearch();
                List<VideoInformation> list = items.SearchQuery(videoName, pagesToCheck);
                Console.WriteLine("Did you mean: " + list[0].Title + "?");
                if (Confirm()) ShowAllInfo(list[0], true);
                else SelectVideo(list, true);
            }
            catch (System.Net.WebException)
            {
                Console.WriteLine("Connection error.\nCheck your internet connection and press any key to try again.");
                Console.ReadKey();
                Main();
            }
        }

        ///<summary>It returns true or false depending on whether the user pressed Y or N (and don't allow to press anything else)</summary>
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

        ///<summary>It prints all available metadata and options about target clip.</summary>
        ///<param name="video">Video to peek.</param>
        ///<param name="clearBefore">If true, the function will clear console before displaying list of data.</param>
        static void ShowAllInfo(VideoInformation video, bool clearBefore)
        {
            if(clearBefore) Console.Clear();

            Console.WriteLine("=== VIDEO DETAILS ===");
            Console.WriteLine("Title: " + video.Title);
            if (video.Author == "") Console.WriteLine("Author: UNKNOWN");
            else Console.WriteLine("Author: " + video.Author);
            if (video.Description == "") Console.WriteLine("Description: NONE");
            else Console.WriteLine("Description: " + video.Description);
            Console.WriteLine("Duration: " + video.Duration);
            Console.WriteLine("Thumbnail: " + video.Thumbnail);
            Console.WriteLine("Url: " + video.Url);
            link = video.Url;
            Options();
        }

        ///<summary>Function that allows user to choose a video from the list of videos founded for 1st search page.</summary>
        ///<param name="list">List of videos to show.</param>
        ///<param name="clearBefore">If true, the function will clear console before displaying list of data.</param>
        static void SelectVideo(List<VideoInformation> list, bool clearBefore)
        {
            int i = 1;

            if(clearBefore) Console.Clear();
            Console.WriteLine("=== SELECT VIDEO ===");
            foreach (var item in list) Console.WriteLine(i++ + ". " + item.Title);
            Console.WriteLine(i++ + ". ===BACK===");

            string option = Console.ReadLine();
            bool parsable = Int32.TryParse(option, out int number);
            if (parsable && number <= list.Count) ShowAllInfo(list[number-1],true);
            else if (number == list.Count + 1) Main();
            else SelectVideo(list,clearBefore);
        }

        ///<summary>Allows user to decide whatever to do with the target video. It can start the download task.</summary>
        static void Options()
        {
            Console.WriteLine("\nWhat would you like to do?");
            Console.WriteLine("1. Download MP3");
            Console.WriteLine("2. Download MP4");
            Console.WriteLine("3. Download MP3 and MP4");
            Console.WriteLine("4. Search for another video");
            Console.WriteLine("5. Exit");

            bool goodOption = false;
            do
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        goodOption = true;
                        task = new Task(new Action(DownloadMP3));
                        task.Start();
                        loadingAnimation();
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        goodOption = true;
                        task = new Task(new Action(DownloadMP4));
                        task.Start();
                        loadingAnimation();
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        goodOption = true;
                        task = new Task(new Action(DownloadMP3andMP4));
                        task.Start();
                        loadingAnimation();
                        break;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        goodOption = true;
                        Console.Clear();
                        Main();
                        break;
                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        goodOption = true;
                        Environment.Exit(1);
                        break;
                }
            } while (!goodOption);
            Console.WriteLine("\rWould you like to search for another video?");
            if (Confirm()) Main();
            else Environment.Exit(1);
        }

        #region Downloads
        ///<summary>This function download target video and puts it to special folder.</summary>
        static void DownloadMP4()
        {
            try
            {
                youTube = YouTube.Default;
                video = youTube.GetVideo(link);
                if(!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                File.WriteAllBytes(directory + video.FullName, video.GetBytes());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                Thread.CurrentThread.Abort();
                Main();
            }
            Console.WriteLine("\nDone!");
            Process.Start("explorer.exe", directory);
        }

        ///<summary>This function download target video and converts it to audio (video file will remain)</summary>
        // It uses above download function, but also converts file to the MP3 format.
        static void DownloadMP3andMP4()
        {
            DownloadMP4();

            var inputFile = new MediaFile(filenameBeforeConvertion = fileName = directory + video.FullName); 
            var outputFile = new MediaFile { Filename = directory + video.FullName.Remove(video.FullName.Length-4) + ".mp3"};

            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);
                engine.Convert(inputFile, outputFile);
            }
        }

        ///<summary>This function download target video and converts it to audio without leaving mp4 file.</summary>
        // It is basicly the above function, but it also deletes MP4 file after convertion.
        static void DownloadMP3()
        {
            DownloadMP3andMP4();
            if(File.Exists(filenameBeforeConvertion)) File.Delete(filenameBeforeConvertion);
        }
        #endregion

        ///<summary>This function show a loading animation, as long the download task is on</summary>
        static void loadingAnimation()
        {
            int i = 0;
            string downloadText = "Downloading";
            Console.Write(downloadText);
            while (task.Status.Equals(TaskStatus.Running))
            {
                Console.Write(".");
                Thread.Sleep(500);
                if (i++ >= 2)
                {
                    Console.SetCursorPosition(downloadText.Length, Console.CursorTop);
                    Console.Write("   ");
                    Console.SetCursorPosition(downloadText.Length, Console.CursorTop);
                    i = 0;
                }                    
            }
        }
    }
}