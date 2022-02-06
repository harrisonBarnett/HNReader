using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HNReader
{
    class Program
    {
       private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to HNReader");
            await GetNewStories();

            bool runProgram = true;
            do
            {
                Console.WriteLine("Commands: fetch all | fetch one | quit");
                string input = Console.ReadLine();
                switch(input)
                {
                    case "fetch all":
                        GetNewStories();
                        break;
                    case "fetch one":
                        GetStoryDetail();
                        break;
                    case "quit":
                        runProgram = false;
                        break;
                    default:
                        break;
                }
            } while (runProgram == true);

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

                Console.WriteLine("New Stories:");
                for (int i = 0; i < 10; i++) {
                    Console.Write("{0}: ", i);
                    //Console.Write(jsonData[i].GetType());
                    await GetStoryAbbreviated(jsonData[i]);
                }

            } catch(HttpRequestException e) {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }
        private static async Task GetStoryAbbreviated(Int64 storyID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Accept.Clear();

                var response = _httpClient.GetStringAsync("https://hacker-news.firebaseio.com/v0/item/" + storyID + ".json?print=pretty");
                var story = await response;

                var jsonData = JsonConvert.DeserializeObject<StoryAbbreviated>(story);
                DateTime time = UnixTimeToDateTime(jsonData.time);
                Console.Write("{0} | {1} | score: {2} \n", time, jsonData.title, jsonData.score);
            } catch(HttpRequestException e)
            {
                Console.WriteLine("Exception {0}", e.Message);
            }
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
            public string title { get; set; }
            public int score { get; set; }

        }
    }
    
}