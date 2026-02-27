using System;
using System.Collections.Generic;
using Core.Abstractions.Popup;
using Core.Infrastructure.Extensions;
using UnityEngine;

namespace Core.Infrastructure.Popup
{
    public class PopupPool : IPopupPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _poolContainer;
        private readonly Stack<PopupBase> _inactive = new();
        private readonly HashSet<PopupBase> _active = new();
        private readonly int _maxSize;

        public int CountActive => _active.Count;
        public int CountInactive => _inactive.Count;

        public PopupPool(GameObject prefab, Transform poolContainer, int initialSize = 2, int maxSize = 20)
        {
            _prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
            _poolContainer = poolContainer;
            _maxSize = maxSize;

            for (var i = 0; i < initialSize; i++)
                CreateInstance();
        }

        public IPopup Get()
        {
            PopupBase popup;

            if (_inactive.Count > 0)
            {
                popup = _inactive.Pop();
            }
            else if (_active.Count < _maxSize)
            {
                popup = CreateInstance();
            }
            else
            {
                Debug.LogWarning($"PopupPool for {_prefab.name} reached max size ({_maxSize}). Consider increasing max size.");
                return null;
            }

            if (popup == null) return null;

            _active.Add(popup);
            popup.OnGetFromPool();
            return popup;
        }

        public void Return(IPopup popup)
        {
            if (popup is not PopupBase popupBase) return;
            if (!_active.Contains(popupBase)) return;

            _active.Remove(popupBase);
            popupBase.OnReturnToPool();
            popupBase.gameObject.SetActive(false);
            popupBase.transform.SetParent(_poolContainer);
            _inactive.Push(popupBase);
        }

        public void Clear()
        {
            foreach (var popup in _active)
            {
                if (popup != null)
                    popup.gameObject.SafeDestroy();
            }
            _active.Clear();

            foreach (var popup in _inactive)
            {
                if (popup != null)
                    popup.gameObject.SafeDestroy();
            }
            _inactive.Clear();
        }

        private PopupBase CreateInstance()
        {
            var instance = UnityEngine.Object.Instantiate(_prefab, _poolContainer);
            instance.SetActive(false);

            var popup = instance.GetComponent<PopupBase>();
            if (popup == null)
            {
                Debug.LogError($"Prefab {_prefab.name} does not have PopupBase component");
                instance.SafeDestroy();
                return null;
            }

            _inactive.Push(popup);
            return popup;
        }
    }
}