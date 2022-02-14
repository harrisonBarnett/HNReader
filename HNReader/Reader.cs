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
            var figFont = FigletFont.Load("./colossal.flf");
            AnsiConsole.Write(new FigletText(figFont, "HN Reader")
                .Color(Color.Orange3));

            await GetStories();

            var UIControl = new Table()
                .BorderColor(Color.Orange3);
            UIControl.Border = TableBorder.Double;

            UIControl.AddColumn("prev");
            UIControl.AddColumn("next");
            UIControl.AddColumn("get one");
            UIControl.AddColumn("refresh");
            UIControl.AddColumn("quit");
            AnsiConsole.Write(UIControl);

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

                var table = new Table()
                    .BorderColor(Color.Orange3);
                table.Width(100);
                table.Border = TableBorder.Double;
                

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
                    string title = story.title;
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

        public async Task RenderStoryDetail(int storyIndex)
        {
            // initial story, top of the thread
            Story story = await GetStory(_storyIDs[storyIndex]);

            var storyTable = new Table();
            storyTable.Width(100);

            storyTable.AddColumn("title: " + story.title.EscapeMarkup());
            storyTable.AddRow("url: " + story.url.EscapeMarkup());

            // comments table
            var commentsTable = new Table();
            commentsTable.Width(100);

            commentsTable.AddColumn("user");
            commentsTable.AddColumn("comment");
            commentsTable.AddColumn("score");

            if(story.kids.Length > 10)
            {
                for(int i = 0; i < 10; i++)
                {
                    Story comment = await GetStory(story.kids[i]);
                    string user = comment.by;
                    string text = comment.text;
                    int score = comment.score;
                    commentsTable.AddRow(user.EscapeMarkup(), text.EscapeMarkup(), score.ToString());
                    commentsTable.AddEmptyRow();
                }
            } else
            {
                foreach(int i in story.kids)
                {
                    Story comment = await GetStory(i);
                    string user = comment.by;
                    string text = comment.text;
                    int score = comment.score;
                    commentsTable.AddRow(user.EscapeMarkup(), text.EscapeMarkup(), score.ToString());
                    commentsTable.AddEmptyRow();
                }
            }

            // UI commands
            var UIControl = new Table();

            UIControl.AddColumn("save");
            UIControl.AddColumn("back");

            AnsiConsole.Write(storyTable);
            AnsiConsole.Write(commentsTable);
            AnsiConsole.Write(UIControl);

            while (true)
            {
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
            public string by { get; set; }
            public string text { get; set; }
        }
    }
}
