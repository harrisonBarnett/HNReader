namespace HNReader
{
    class Program
    {
        static void Main(string[] args)
        {
            Reader reader = new Reader();
            Console.WriteLine("Welcome to HNReader. Enter the command 'get' to retrieve new stories.");
            string input = Console.ReadLine();
            string result = "";
            if(input == "get")
            {
                result = reader.Get();
            }
            Console.WriteLine(result);
        }

    }
    
    class Reader
    {
        public string Get()
        {
            return "hello harrison";
        }
    }
}