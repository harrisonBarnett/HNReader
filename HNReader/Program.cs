using System.Threading.Tasks;
using Newtonsoft.Json; 

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
                Console.WriteLine("HNReader\nCommands: new | quit");
                string input = Console.ReadLine();
                switch(input)
                {
                    case "new":
                        New();
                        break;
                    case "quit":
                        runProgram = false;
                        break;
                    default:
                        break;
                }

            } while (runProgram == true);

        }
        private static async Task New()
        {
            try{
                // clear HTTP headers
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                // API endpoint to return the _id of top stories on HN                
                var response = _httpClient.GetStringAsync("https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty");
                var storyIDs = await response;

                ItemList _itemList = JsonConvert.DeserializeObject<ItemList>(storyIDs);

                Console.WriteLine(_itemList);

            } catch(HttpRequestException e) {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }
        public class ItemList {
            public List<Item> items {get; set;}
        }
        public class Item {
            public string id {get; set;}
        }
 
    }
    
}