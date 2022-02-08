using System.Threading.Tasks;
using Newtonsoft.Json;
using Spectre.Console;

namespace HNReader
{
    class Program
    {
       private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task Main(string[] args)
        {
            bool runProgram = true;
            do
            {
                HomeHeader();
                string input = Console.ReadLine();
                switch(input)
                {
                    case "new":
                        await GetNewStories();
                        break;
                    case "top":
                        await GetTopStories();
                        break;
                    case "quit":
                        runProgram = false;
                        break;
                    default:
                        break;
                }
            } while (runProgram == true);

        }
        // HOME HEADER AND COMMAND UI
        private static void HomeHeader()
        {
            AnsiConsole.Write(
                new FigletText("HN Reader"));
            var table = new Table();
            table.AddColumn("new");
            table.AddColumn("top");
            table.AddColumn("quit");
            AnsiConsole.Write(table);
        }
        // NEW STORIES VIEW HEADER AND COMMAND UI
        private static void NewStoriesHeader()
        {
            AnsiConsole.Write(
                new FigletText("new")
                );
            var table = new Table();
            table.AddColumn("get one");
            table.AddColumn("back");
            AnsiConsole.Write(table);
        }
        // RETURN NEW STORIES FROM HN
        private static async Task GetNewStories()
        {
            NewStoriesHeader();
            try
            {
                //// clear HTTP headers
                //_httpClient.DefaultRequestHeaders.Accept.Clear();
                // API endpoint to return the _id of top stories on HN                
                var response = _httpClient.GetStringAsync("https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty");
                var storyIDs = await response;

                var jsonData = JsonConvert.DeserializeObject<List<dynamic>>(storyIDs);

                var table = new Table();
                table.Border(TableBorder.Ascii);

                table.AddColumn("i");
                table.AddColumn("date / time");
                table.AddColumn("title");
                table.AddColumn("score");

                for (int i = 0; i < 10; i++) {
                    Story story = await GetStory(jsonData[i]);
                    string index = i.ToString();
                    DateTime time = UnixTimeToDateTime(story.time);
                    string title = story.title.ToString();
                    string score = story.score.ToString();
                    table.AddRow(index, time.ToString(), title.EscapeMarkup(), score);
                }
                AnsiConsole.Write(table);

                bool runProgram = true;
                do
                {
                    string input = Console.ReadLine();
                    switch (input)
                    {
                        case "get one":
                            // TEST DATA, HARDCODED
                            GetStoryDetail(8863);
                            break;
                        case "back":
                            runProgram = false;
                            break;
                        default:
                            break;
                    }
                } while (runProgram == true);
            }
            catch (HttpRequestException e) {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }
        // RETURN TOP STORIES FROM HN
        private static async Task GetTopStories()
        {
            Console.WriteLine("Reading story detail\nEnter Story Index: ");
            string input = Console.ReadLine();
        }
        // STORY DETAIL VIEW
        private static async Task GetStoryDetail(Int64 storyID)
        {
            Console.WriteLine("Enter story index: ");
            string input = Console.ReadLine();

            var table = new Table();
            table.Border(TableBorder.Ascii);

            table.AddColumn("date / time");
            table.AddColumn("title");
            table.AddColumn("score");


            Story story = await GetStory(storyID);

            DateTime time = UnixTimeToDateTime(story.time);
            string title = story.title.ToString();
            string score = story.score.ToString();
            table.AddRow(time.ToString(), title.EscapeMarkup(), score);
            AnsiConsole.Write(table);

            bool runProgram = true;
            do
            {
                var control = new Table();
                control.AddColumn("show comments");
                control.AddColumn("back");
                AnsiConsole.Write(control);
                string command = Console.ReadLine();
                switch (command)
                {
                    case "show comments":
                        Console.WriteLine("Showing comments");
                        break;
                    case "back":
                        runProgram = false;
                        break;
                    default:
                        break;
                }
            } while (runProgram == true);

        }
        // RETURN A SINGLE STORY FROM HN
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
            } catch(HttpRequestException e)
            {
                Console.WriteLine("Exception {0}", e.Message);
            }

            return toReturn;
        }
        // CONVERT UNIX TIME TO HUMAN READABLE
        public static DateTime UnixTimeToDateTime(long unixtime)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixtime).ToLocalTime();
            return dtDateTime;
        }
        // STORY STRUCT
        public class Story {
            public int time { get; set; }
            public string? title { get; set; }
            public string? text { get; set; }

            public int score { get; set; }
            public int[] kids { get; set; }
        }
    }
    
}