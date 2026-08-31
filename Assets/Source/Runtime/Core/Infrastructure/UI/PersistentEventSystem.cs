using Core.Abstractions.Services.Input;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Infrastructure.UI
{
    /// <summary>
    /// Держит единственный <see cref="EventSystem"/> на всё время игры.
    /// </summary>
    /// <remarks>
    /// Стартовая сцена выгружается при переходе в матч, а корень UI помечен
    /// <c>DontDestroyOnLoad</c> и переход переживает. Без этого компонента канвас остаётся
    /// на экране, но нажатия обрабатывать некому. Копия, приехавшая с повторно загруженной
    /// сценой, убирает себя, а не первую: ссылки на первую уже раздали подписчикам.
    /// </remarks>
    [RequireComponent(typeof(EventSystem))]
    public sealed class PersistentEventSystem : MonoBehaviour
    {
        private static PersistentEventSystem _instance;

        [Inject] private IInputService _inputService;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Сервис ввода создаётся раньше этого объекта и в конструкторе до модуля ввода
        /// дотянуться не может. Сообщаем ему о готовности сами — с этого момента модуль
        /// работает на общем ассете действий, а не на пакетном.
        /// </summary>
        private void Start()
        {
            _inputService.EnableUIActions();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
