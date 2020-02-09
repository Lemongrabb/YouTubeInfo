using System.Linq;
using System;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading.Tasks;

namespace YouTubeInfo
{
    class Program //TODO how can i track variables that I need to recive from user?
    {
        static string videoName;
        static string url;
        static string html;
        static dynamic video;
        static HttpClient httpClient;
        static HtmlDocument htmlDocument;
        static async Task Main(string[] args)
        {
            //1. Show logo
            Console.WriteLine("=== YouTubeInfo! ==="); //TODO make a unicode logo

            //2.Ask for a video name
            Console.WriteLine("Enter title of the video you want to peek:");
            videoName = Console.ReadLine();

            //3. Prepare search link
            videoName = videoName.Replace(" ", "+");//TODO do i need to change special chars?
            url = "https://www.youtube.com/results?search_query=" + videoName;

            //4. Parse HTML from the link
            await GetHtmlAsync();

            //5. Get link of the 1st search result
            //TODO learn about async and wait xD

            //6. Do 4th step but with different link

            //7. Get specific data from video
            
        }

        private static async Task GetHtmlAsync()
        {
            httpClient = new HttpClient();
            html = await httpClient.GetStringAsync(url);
            htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            video = htmlDocument.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("id","")
                .Equals("dismissable"));
        }
    }
}