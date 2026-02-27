using UnityEngine;

namespace Core.Data.Domain.Models.Elements.Samples
{
    public struct TextCardData : IElementData
    {
        public string Id { get; set; }
        public string Title;
        public string Description;
        public Sprite Icon;
    }
}
