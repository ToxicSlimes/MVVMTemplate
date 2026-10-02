using Core.Abstractions.Config;
using UnityEngine;

namespace Core.Infrastructure.Configs.Services
{
    [CreateAssetMenu(fileName = "LocalizationConfig", menuName = "Configs/Services/Localization Config")]
    public class LocalizationConfig : ConfigBase
    {
        [SerializeField] private string defaultTable = "UI";

        public string DefaultTable => string.IsNullOrWhiteSpace(defaultTable) ? "UI" : defaultTable;
    }
}
