namespace EventGridTester.ConfigurationModels
{
    public class EventGridConfig
    {
        public string Uri { get; set; }

        public Uri ToURI() => new Uri(this.Uri);
    }
}
