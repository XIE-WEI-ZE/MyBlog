using System.Text.Json.Serialization;

namespace prjMyBlog.ViewModels
{
    public class TagItem
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = "";
    }
}
