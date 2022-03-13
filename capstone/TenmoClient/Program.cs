namespace TenmoClient
{
    class Program
    {
        private const string apiUrl = "https://localhost:5001/";
        static void Main()
        {
            TenmoApp app = new TenmoApp(apiUrl);
            app.Run();
        }
    }
}
