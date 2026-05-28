using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CodeVisApi.Models
{
    public class AstNode
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("attributes")]
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        [JsonPropertyName("children")]
        public List<AstNode> Children { get; set; } = new List<AstNode>();

        public AstNode(string name)
        {
            Name = name;
        }

        public AstNode(string name, string value)
        {
            Name = name;
            Attributes.Add("value", value);
        }

        public void AddChild(AstNode child)
        {
            if (child != null)
            {
                Children.Add(child);
            }
        }
    }
}
