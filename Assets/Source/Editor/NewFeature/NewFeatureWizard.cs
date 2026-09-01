#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using CodeGenRunner = Core.Editor.CodeGenPipeline.CodeGenPipeline;

namespace Core.Editor.NewFeature
{
    public sealed class NewFeatureWizard : EditorWindow
    {
        private const string RuntimeRoot = "Assets/Source/Runtime/Core";
        private const string CodeGenRoot = "Assets/Source/CodeGen/Views";

        private string _featureName = "MyFeature";
        private string _title = "My Feature";
        private bool _openGeneratedFolder = true;
        private Vector2 _scroll;

        [MenuItem("Tools/MVVM/New Feature...", false, 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<NewFeatureWizard>("New Feature");
            window.minSize = new Vector2(440, 320);
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("New Feature Wizard", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Creates Model interface + implementation, ViewModel, View, and ViewMapping. " +
                "Then runs Scan & Generate All for DI registration.",
                MessageType.Info);

            EditorGUILayout.Space(8);
            _featureName = EditorGUILayout.TextField("Feature Name (PascalCase)", _featureName);
            _title = EditorGUILayout.TextField("Default Title", _title);
            _openGeneratedFolder = EditorGUILayout.Toggle("Open folder after create", _openGeneratedFolder);

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Files to create", EditorStyles.boldLabel);
            DrawPathPreview();

            EditorGUILayout.Space(12);
            GUI.enabled = IsValidFeatureName(_featureName);
            if (GUILayout.Button("Create Feature", GUILayout.Height(32)))
            {
                CreateFeature();
            }

            GUI.enabled = true;
            EditorGUILayout.EndScrollView();
        }

        private void DrawPathPreview()
        {
            var feature = SanitizeFeatureName(_featureName);
            EditorGUILayout.LabelField($"Abstractions/MVVM/Models/{feature}/I{feature}Model.cs", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"MVVM/Models/{feature}/{feature}Model.cs", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"MVVM/ViewModels/{feature}/{feature}ViewModel.cs", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"MVVM/Views/{feature}/{feature}View.cs", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"CodeGen/Views/{feature}/{feature}ViewMapping.cs", EditorStyles.miniLabel);
        }

        private void CreateFeature()
        {
            var feature = SanitizeFeatureName(_featureName);
            if (!IsValidFeatureName(feature))
            {
                EditorUtility.DisplayDialog("Invalid name", "Use PascalCase letters and digits only (e.g. Shop, QuestLog).", "OK");
                return;
            }

            var created = new StringBuilder();
            var skipped = new StringBuilder();

            WriteFile(
                $"{RuntimeRoot}/Abstractions/MVVM/Models/{feature}/I{feature}Model.cs",
                GenerateModelInterface(feature),
                created,
                skipped);
            WriteFile(
                $"{RuntimeRoot}/MVVM/Models/{feature}/{feature}Model.cs",
                GenerateModel(feature, EscapeCString(_title)),
                created,
                skipped);
            WriteFile(
                $"{RuntimeRoot}/MVVM/ViewModels/{feature}/{feature}ViewModel.cs",
                GenerateViewModel(feature),
                created,
                skipped);
            WriteFile(
                $"{RuntimeRoot}/MVVM/Views/{feature}/{feature}View.cs",
                GenerateView(feature),
                created,
                skipped);
            WriteFile(
                $"{CodeGenRoot}/{feature}/{feature}ViewMapping.cs",
                GenerateViewMapping(feature),
                created,
                skipped);

            AssetDatabase.Refresh();
            CodeGenRunner.ScanAndGenerateAll();

            var message = created.Length > 0
                ? $"Created:\n{created}"
                : "No new files created.";

            if (skipped.Length > 0)
            {
                message += $"\n\nSkipped (already exist):\n{skipped}";
            }

            message += "\n\nNext: build a prefab with UI Prefab Builder or assign an Addressable, " +
                       $"then open from your menu via ShowViewRequest<{feature}View, {feature}ViewModel>().";

            EditorUtility.DisplayDialog("New Feature", message, "OK");

            if (_openGeneratedFolder && created.Length > 0)
            {
                var folder = $"{RuntimeRoot}/MVVM/Views/{feature}";
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folder);
                if (obj != null)
                {
                    EditorGUIUtility.PingObject(obj);
                    Selection.activeObject = obj;
                }
            }
        }

        private static void WriteFile(string assetPath, string content, StringBuilder created, StringBuilder skipped)
        {
            var fullPath = Path.GetFullPath(assetPath);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(fullPath))
            {
                skipped.AppendLine(assetPath);
                return;
            }

            File.WriteAllText(fullPath, content, Encoding.UTF8);
            created.AppendLine(assetPath);
        }

        private static bool IsValidFeatureName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
            {
                return false;
            }

            if (!char.IsUpper(name[0]) || !char.IsLetter(name[0]))
            {
                return false;
            }

            for (var i = 1; i < name.Length; i++)
            {
                var c = name[i];
                if (!char.IsLetterOrDigit(c))
                {
                    return false;
                }
            }

            return true;
        }

        private static string SanitizeFeatureName(string raw) =>
            string.IsNullOrWhiteSpace(raw) ? string.Empty : raw.Trim();

        private static string GenerateModelInterface(string feature) =>
$@"using System;

namespace Core.Abstractions.MVVM.Models.{feature}
{{
    public interface I{feature}Model : IDisposable
    {{
        string Title {{ get; set; }}
        event Action<string> TitleChanged;
    }}
}}
";

        private static string EscapeCString(string value) =>
            string.IsNullOrEmpty(value) ? string.Empty : value.Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static string GenerateModel(string feature, string defaultTitle) =>
$@"using System;
using Core.Abstractions.MVVM.Models.{feature};

namespace Core.MVVM.Models.{feature}
{{
    public class {feature}Model : I{feature}Model
    {{
        private string _title = ""{defaultTitle}"";

        public event Action<string> TitleChanged;

        public string Title
        {{
            get => _title;
            set
            {{
                if (_title == value) return;
                _title = value;
                TitleChanged?.Invoke(_title);
            }}
        }}

        public void Dispose()
        {{
            TitleChanged = null;
        }}
    }}
}}
";

        private static string GenerateViewModel(string feature) =>
$@"using System;
using Core.Abstractions.MVVM.Models.{feature};
using Core.Abstractions.MVVM.ViewModels;
using Core.Application.Commands.Common;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Attributes;
using Cysharp.Threading.Tasks;
using R3;

namespace Core.MVVM.ViewModels.{feature}
{{
    [GenerateViewCommands]
    public class {feature}ViewModel : DisposableObject
    {{
        private readonly I{feature}Model _model;
        private readonly IMediator _mediator;

        public ReactiveProperty<string> Title {{ get; }}
        public ReactiveCommand<Unit> BackCommand {{ get; set; }}

        public {feature}ViewModel(I{feature}Model model, IMediator mediator)
        {{
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

            Title = new ReactiveProperty<string>(_model.Title);

            SetupModelSubscriptions();
            SetupCommands();
        }}

        private void SetupModelSubscriptions()
        {{
            _model.TitleChanged += OnModelTitleChanged;
            Title
                .Subscribe(this, static (v, s) => s._model.Title = v)
                .AddTo(ref Disposables);
        }}

        private void SetupCommands()
        {{
            BackCommand = new ReactiveCommand<Unit>(_ => OnBack().Forget());
            BackCommand.AddTo(ref Disposables);
        }}

        private void OnModelTitleChanged(string value) => Title.Value = value;

        private async UniTaskVoid OnBack()
        {{
            await _mediator.Send(new NavigateBackCommand());
        }}

        public override void Dispose()
        {{
            _model.TitleChanged -= OnModelTitleChanged;
            _model.Dispose();
            base.Dispose();
        }}
    }}
}}
";

        private static string GenerateView(string feature) =>
$@"using Core.MVVM.ViewModels.{feature};
using Core.UI.Controls;
using R3;
using UnityEngine;

namespace Core.MVVM.Views.{feature}
{{
    public class {feature}View : PanelView<{feature}ViewModel>
    {{
        [SerializeField] private BaseText _titleText;
        [SerializeField] private BaseButton _backButton;

        protected override void BindPanel(ref DisposableBag disposables)
        {{
            Bind(_titleText, ViewModel.Title, ref disposables);
            Bind(_backButton, ViewModel.BackCommand, ref disposables);
        }}
    }}
}}
";

        private static string GenerateViewMapping(string feature) =>
$@"using Core.Infrastructure.MVVM.Config;
using Core.MVVM.ViewModels.{feature};
using Core.MVVM.Views.{feature};

namespace Core.Views.{feature}
{{
    public class {feature}ViewMapping : ViewMappingBase<{feature}View, {feature}ViewModel> {{ }}
}}
";
    }
}
#endif
