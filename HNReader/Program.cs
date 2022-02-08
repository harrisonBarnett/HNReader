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
                        await GetStoryDetail();
                        break;
                    case "quit":
                        runProgram = false;
                        break;
                    default:
                        break;
                }
            } while (runProgram == true);

        }
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
        private static void NewStoriesHeader()
        {
            AnsiConsole.Write(
                new FigletText("new")
                );
            var table = new Table();
            table.AddColumn("fetch one");
            table.AddColumn("back");
            AnsiConsole.Write(table);
        }
        private static async Task GetNewStories()
        {
            try{
                // clear HTTP headers
                _httpClient.DefaultRequestHeaders.Accept.Clear();
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
                    StoryAbbreviated story = await GetStoryAbbreviated(jsonData[i]);
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
                    var control = new Table();
                    control.AddColumn("fetch one");
                    control.AddColumn("back");
                    AnsiConsole.Write(control);
                    string input = Console.ReadLine();
                    switch (input)
                    {
                        case "fetch one":
                            Console.WriteLine("fetching one story");
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
        private static async Task<StoryAbbreviated> GetStoryAbbreviated(Int64 storyID)
        {
            StoryAbbreviated toReturn = null;

            try
            {
                _httpClient.DefaultRequestHeaders.Accept.Clear();

                var response = _httpClient.GetStringAsync("https://hacker-news.firebaseio.com/v0/item/" + storyID + ".json?print=pretty");
                var story = await response;

                var jsonData = JsonConvert.DeserializeObject<StoryAbbreviated>(story);

                toReturn = jsonData;
            } catch(HttpRequestException e)
            {
                Console.WriteLine("Exception {0}", e.Message);
            }

            return toReturn;
        }
        private static async Task GetStoryDetail()
        {
            Console.WriteLine("Reading story detail\nEnter Story Index: ");
            string input = Console.ReadLine();
        }
        public static DateTime UnixTimeToDateTime(long unixtime)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixtime).ToLocalTime();
            return dtDateTime;
        }
        public class StoryAbbreviated {
            public int time { get; set; }
            public string? title { get; set; }
            public int score { get; set; }

        }
    }
    
}