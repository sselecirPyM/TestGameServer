namespace FarmingGameServer
{
    public class ComanndLineArgs
    {
        [CommandLine.Option(Default = "http://127.0.0.1:8080/")]
        public string url { get; set; } = "http://127.0.0.1:8080/";
    }
}
