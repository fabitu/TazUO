using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ClassicUO.Game.Data
{
    public class StaticCustomItens
    {
        [JsonPropertyOrder(0)]
        public string Description { get; set; }
        [JsonPropertyOrder(1)]        
        public ushort ReplaceToGraphic { get; set; }
        [JsonPropertyOrder(2)]
        public List<ushort> ToReplaceGraphicArray = new List<ushort>();
    }
    
}
