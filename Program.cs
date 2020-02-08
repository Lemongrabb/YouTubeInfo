using System;

namespace YouTubeInfo
{
    class Program //TODO how can i track variables that I need to recive from user?
    {
        static string videoName;
        static string queryLink;
        static void Main(string[] args)
        {
            //1. Show logo
            Console.WriteLine("=== YouTubeInfo! ==="); //TODO make a unicode logo

            //2.Ask for a video name
            Console.WriteLine("Enter title of the video you want to peek:");
            videoName = Console.ReadLine();

            //3. Prepare search link
            //3.1 Replace spaces with pluses //TODO do i need to change special chars?
            videoName = videoName.Replace(" ","+");
            //3.2 Put it do link
            queryLink = "https://www.youtube.com/results?search_query=" + videoName;

            //4. Parse HTML from the link
            // https://stackoverflow.com/questions/16642196/get-html-code-from-website-in-c-sharp

            //5. Get link of the 1st search result
            // https://stackoverflow.com/questions/8548579/library-to-extract-data-from-html-string

            //6. Do 4th step but with different link

            //7. Get specific data from video
        }
    }
}
