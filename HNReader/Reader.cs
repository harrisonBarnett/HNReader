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
        private static List<dynamic> _storyIDs;

        // paginator index
        private static int _paginator = 0;


        // main view upon opening app and returning to home view
        public async Task RenderHomeView()
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
            try
            {
                // API endpoint to return list of IDs of top stories on HN                
                var response = _httpClient.GetStringAsync("https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty");
                var storyIDs = await response;
                var deserializedIDs = JsonConvert.DeserializeObject<List<dynamic>>(storyIDs);
                _storyIDs = deserializedIDs;

                var table = new Table();

                table.AddColumn("i");
                table.AddColumn("date / time");
                table.AddColumn("title");
                table.AddColumn("score");

                // render abbreviated stories 10 at a time depending on pagination
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

        public async Task RenderStoryDetail(string storyIndex)
        {
            // TODO:
            // figure out why i'm not getting text data from deserialized json

            Story story = await GetStory(_storyIDs[int.Parse(storyIndex)]);

            var storyTable = new Table();
            storyTable.AddColumn(story.title.EscapeMarkup());
            storyTable.AddRow(story.url.EscapeMarkup());
            AnsiConsole.Write(storyTable);

            await RenderComments(story.kids);

            while(true)
            {
                var commands = new Table();
                commands.AddColumn("save");
                commands.AddColumn("back");
                AnsiConsole.Write(commands);

                string input = Console.ReadLine();
                switch(input)
                {
                    case "save":
                        break;
                    case "back":
                        return;
                }

            }

        }

        // render comments in a story detail view
        private async Task RenderComments(int[] kids)
        {
            if(kids.Length > 10)
            {
                for(int i = 0; i < 10; i++)
                {
                    Console.WriteLine(kids[i]);
                }
            } else
            {
                foreach(int i in kids)
                {
                    Console.WriteLine(i);
                }
            }
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

        // pagination left and right by 10 up to 50 stories total
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

        // // Convert unix time to human readable
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
            public string? url { get; set; }

            public int score { get; set; }
            public int[] kids { get; set; }
        }
    }
}
