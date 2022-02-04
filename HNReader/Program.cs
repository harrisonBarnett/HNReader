using System.Threading.Tasks;
namespace HNReader
{
    class Program
    {
       private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to HNReader. Enter 'get' to get the latest top stories.");
            string input = Console.ReadLine();
            if(input == "get")
            {
                await Get();
            } else
            {
                Console.WriteLine("Response not recognized.");
            }

        }
        private static async Task Get()
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();

            // API endpoint to return the _id of top stories on HN
            var taskString = _httpClient.GetStringAsync("https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty");

            var msg = await taskString;
            Console.WriteLine(msg);
        }
 
    }
    
}