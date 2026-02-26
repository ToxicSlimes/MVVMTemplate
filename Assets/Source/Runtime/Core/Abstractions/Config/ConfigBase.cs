using UnityEngine;
using TriInspector;

namespace Core.Abstractions.Config
{
    /// <summary>
    /// Базовый класс для всех ScriptableObject конфигов.
    /// Предоставляет красивый Inspector с помощью Tri-Inspector.
    /// 
    /// Используй атрибуты:
    /// - [Title("Section")] — заголовок секции
    /// - [Group("GroupName")] — группировка полей
    /// - [Tab("TabName")] — табы для разделения
    /// - [Required] — обязательное поле
    /// - [ShowIf("condition")] — условное отображение
    /// - [InfoBox("message")] — информационный блок
    /// - [Button] — кнопка в инспекторе
    /// - [InlineEditor] — inline редактор для вложенных SO
    /// - [TableList] — таблица для списков
    /// </summary>
    [DeclareBoxGroup("header", Title = "📋 Config Info")]
    [DeclareTabGroup("tabs")]
    public abstract class ConfigBase : ScriptableObject
    {
        [Group("header")]
        [ShowInInspector, ReadOnly]
        [LabelText("Config Type")]
        protected virtual string ConfigTypeName => GetType().Name;

        [Group("header")]
        [ShowInInspector][ReadOnly]
        [LabelText("Asset Path")]
        protected string AssetPath
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.AssetDatabase.GetAssetPath(this);
#else
                return "Runtime";
#endif
            }
        }

        /// <summary>
        /// Вызывается при изменении значений в Inspector.
        /// Переопредели для валидации.
        /// </summary>
        protected virtual void OnValidate()
        {
            ValidateConfig();
        }

        /// <summary>
        /// Переопредели для кастомной валидации.
        /// </summary>
        protected virtual void ValidateConfig()
        {
        }

#if UNITY_EDITOR
        [Button(ButtonSizes.Medium), PropertyOrder(100)]
        private void PingInProject()
        {
            UnityEditor.EditorGUIUtility.PingObject(this);
            UnityEditor.Selection.activeObject = this;
        }

        [Button(ButtonSizes.Medium), PropertyOrder(101)]
        private void OpenScript()
        {
            var script = UnityEditor.MonoScript.FromScriptableObject(this);
            if (script != null)
            {
                UnityEditor.AssetDatabase.OpenAsset(script);
            }
        }
#endif
    }
}
