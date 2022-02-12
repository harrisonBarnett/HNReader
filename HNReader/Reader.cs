using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Newtonsoft.Json;

namespace HNReader
{
    internal class Reader
    {
        // initialize http client
        private static HttpClient _httpClient = new HttpClient();

        // list of stories stored in memory when reader is instantiated
        private static List<Story> _storyList;

        // paginator index
        private static int _paginator = 0;


        // main view upon opening app and returning to home view
        public async Task HomeViewAsync()
        {
            AnsiConsole.Write(new FigletText("HN Reader"));

            await GetStories();

            var table = new Table();
            table.AddColumn("prev");
            table.AddColumn("next");
            table.AddColumn("get one");
            table.AddColumn("refresh");
            table.AddColumn("quit");
            AnsiConsole.Write(table);

            
        }

        // Get abbreviated stories 10 at a time
        private static async Task GetStories()
        {
            // TODO: implement pagination?
            try
            {
                // API endpoint to return list of IDs of top stories on HN                
                var response = _httpClient.GetStringAsync("https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty");
                var storyIDs = await response;
                var deserializedIDs = JsonConvert.DeserializeObject<List<dynamic>>(storyIDs);

                var table = new Table();
                table.Border(TableBorder.Ascii);

                table.AddColumn("i");
                table.AddColumn("date / time");
                table.AddColumn("title");
                table.AddColumn("score");

                for (int i = _paginator; i < _paginator + 10; i++)
                {
                    Story story = await GetStory(deserializedIDs[i]);
                    string index = i.ToString();
                    DateTime time = UnixTimeToDateTime(story.time);
                    string title = story.title.ToString();
                    string score = story.score.ToString();
                    table.AddRow(index, time.ToString(), title.EscapeMarkup(), score);
                }
                AnsiConsole.Write(table);

            }
            catch (HttpRequestException e)
            {
                AnsiConsole.Write(new FigletText("ERROR:"));
                AnsiConsole.Write(new FigletText(e.StatusCode.ToString())); 
                Console.WriteLine(e.Message);
            }
        }

        // Get story detail
        public async Task GetStoryDetail(string storyIndex)
        {
            Console.WriteLine("hello from the get story detail method. fetching story {0}", storyIndex);
        }

        // Get and deserialize a story to json data
        private static async Task<Story> GetStory(Int64 storyID)
        {
            Story toReturn = null;
            try
            {
                _httpClient.DefaultRequestHeaders.Accept.Clear();

                var response = _httpClient.GetStringAsync("https://hacker-news.firebaseio.com/v0/item/" + storyID + ".json?print=pretty");
                var story = await response;
                var jsonData = JsonConvert.DeserializeObject<Story>(story);

                toReturn = jsonData;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Exception {0}", e.Message);
            }

            return toReturn;
        }

        public void PaginatePrevious()
        {
            if(_paginator >= 10)
            {
                _paginator -= 10;
            }
        }

        public void PaginateNext()
        {
            if(_paginator <= 30)
            {
                _paginator += 10;
            }
        }

        // Convert unix time to human readable
        private static DateTime UnixTimeToDateTime(long unixtime)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixtime).ToLocalTime();
            return dtDateTime;
        }

        // // Story model
        private class Story
        {
            public int time { get; set; }
            public string? title { get; set; }
            public string? text { get; set; }

            public int score { get; set; }
            public int[] kids { get; set; }
        }
    }
}
