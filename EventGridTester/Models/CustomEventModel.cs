using System.Text.Json;

namespace EventGridTester.Models
{
    public class CustomEventModel
    {
        public string MyInteger { get; set; }
        public IEnumerable<string> Messages { get; set; }
        public string SomeFlag { get; set; }

        public string? CustomIdentifier { get; set; }

        public override string ToString()
        {
            var msg = Messages is null ? string.Empty : string.Join("; ", Messages);
            var content = $"MyInteger:{MyInteger}; SomeFlag: {SomeFlag}; Messages: {msg}; CustomerIdentifier: {CustomIdentifier}";
            return content;
        } 

        public static CustomEventModel FromEventData(BinaryData data)
        {
            if (data is null)
                return new CustomEventModel();

            var model = JsonSerializer.Deserialize<CustomEventModel>(data.ToString(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true});
            return model;
        }
    }
}
