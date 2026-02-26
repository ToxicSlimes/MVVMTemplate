using System;
using Core.Abstractions.Config;
using TriInspector;
using UnityEngine;

namespace Core.Configs.Services
{
    [Flags]
    public enum WorkflowEnvironment : byte
    {
        Dev = 1,
        Prod = 2
    }

    [CreateAssetMenu(fileName = "NetworkConfig", menuName = "EndlessAdventure/Network Config")]
    public class NetworkConfig : ServiceConfigBase
    {
        #region URLs

        [Title("🌐 API URLs")]
        [SerializeField] private string _devBaseUrl = "https://backend-dev.caichat.ai";
        [SerializeField] private string _prodBaseUrl = "https://backend.caichat.ai";

        #endregion

        #region Authentication

        [Title("🔐 Authentication")]
        [SerializeField] private string _sessionCookie = "";
        [SerializeField] private string _authToken = "";
        [SerializeField] private WorkflowEnvironment _workflowEnvironment = WorkflowEnvironment.Dev;

        #endregion

        #region Network Settings

        [Title("⚙️ Network Settings")]
        [Range(100, 3000)]
        [Tooltip("Request timeout in seconds")]
        [SerializeField] private int _requestTimeout = 500;

        #endregion

        #region External APIs

        [Title("🤖 External APIs")]
        [Tooltip("Google AI / Gemini API Key for image generation")]
        [SerializeField] private string _geminiApiKey = "AIzaSyC-ehb9FQGbCkuz_78hm7aagCiugwCmsZY";

        #endregion

        #region Properties

        private string ActiveBaseUrl => (_workflowEnvironment & WorkflowEnvironment.Dev) != 0
            ? _devBaseUrl
            : _prodBaseUrl;

        public string AuthBaseUrl => ActiveBaseUrl;
        public string ApiBaseUrl => ActiveBaseUrl;
        public string ProdApiBaseUrl => _prodBaseUrl;
        public string SessionCookie => _sessionCookie;
        public string AuthToken => _authToken;
        public WorkflowEnvironment WorkflowEnvironment => _workflowEnvironment;
        public int RequestTimeout => _requestTimeout;
        public string GeminiApiKey => _geminiApiKey;

        #endregion

        #region Methods

        public void SetDevBaseUrl(string url)
        {
            _devBaseUrl = url;
        }

        public void SetSessionCookie(string cookie)
        {
            _sessionCookie = cookie;
        }

        public void SetAuthToken(string token)
        {
            _authToken = token;
        }

        public void SetWorkflowEnvironment(WorkflowEnvironment environment)
        {
            _workflowEnvironment = environment;
        }

        #endregion

        #region Validation

        protected override void ValidateConfig()
        {
            base.ValidateConfig();

            if (string.IsNullOrEmpty(_devBaseUrl))
            {
                Debug.LogWarning($"[{nameof(NetworkConfig)}] Dev Base URL is empty!");
            }

            if (string.IsNullOrEmpty(_prodBaseUrl))
            {
                Debug.LogWarning($"[{nameof(NetworkConfig)}] Prod Base URL is empty!");
            }

            if (_requestTimeout <= 0)
            {
                Debug.LogWarning($"[{nameof(NetworkConfig)}] Request timeout must be > 0!");
            }
        }

        #endregion
    }
}
