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
            Reader reader = new Reader();

            while (true)
            {
                reader.HomeViewAsync();

                string input = Console.ReadLine();
                switch(input)
                {
                    case "prev":
                        Console.WriteLine("getting previous page");
                        break;
                    case "next":
                        Console.WriteLine("getting next page"); 
                        break;
                    case "get one":
                        Console.WriteLine("enter story index: ");
                        string storyIndex = Console.ReadLine();
                        reader.GetStoryDetail(storyIndex);
                        break;
                    case "refresh":
                        reader.HomeViewAsync();
                        break;
                    case "quit":
                        return;
                        break;
                    default:
                        break;
                }
            }

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