using System;
using UnityEngine;

namespace Core.Data.Domain.Models.Elements.Samples
{
    public struct ActionListItemData : IElementData
    {
        public string Id { get; set; }
        public string Title;
        public string Subtitle;
        public Sprite Icon;
        public string ActionLabel;
        public bool IsActionEnabled;
    }
}
