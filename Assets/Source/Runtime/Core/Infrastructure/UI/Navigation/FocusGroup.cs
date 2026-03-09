using System.Collections.Generic;
using Core.Abstractions.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Infrastructure.UI.Navigation
{
    public sealed class FocusGroup : MonoBehaviour, IFocusGroup
    {
        [SerializeField] private bool _autoCollectChildren = true;
        [SerializeField] private bool _wrapAround = true;
        [SerializeField] private Selectable _initialFocus;
        [SerializeField] private Selectable[] _customOrder;

        private readonly List<Selectable> _selectables = new();
        private int _currentIndex;

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            _selectables.Clear();

            if (_autoCollectChildren)
            {
                var children = GetComponentsInChildren<Selectable>(false);
                for (var i = 0; i < children.Length; i++)
                {
                    if (children[i].interactable)
                        _selectables.Add(children[i]);
                }
            }
            else if (_customOrder != null)
            {
                for (var i = 0; i < _customOrder.Length; i++)
                {
                    if (_customOrder[i] != null && _customOrder[i].interactable)
                        _selectables.Add(_customOrder[i]);
                }
            }

            _currentIndex = 0;
        }

        public void SelectInitial()
        {
            if (_selectables.Count == 0) return;

            var target = _initialFocus != null && _initialFocus.interactable
                ? _initialFocus
                : _selectables[0];

            _currentIndex = _selectables.IndexOf(target);
            if (_currentIndex < 0) _currentIndex = 0;

            EventSystem.current?.SetSelectedGameObject(target.gameObject);
        }

        public void SelectNext()
        {
            if (_selectables.Count == 0) return;

            if (_wrapAround)
                _currentIndex = (_currentIndex + 1) % _selectables.Count;
            else
                _currentIndex = Mathf.Min(_currentIndex + 1, _selectables.Count - 1);

            EventSystem.current?.SetSelectedGameObject(_selectables[_currentIndex].gameObject);
        }

        public void SelectPrevious()
        {
            if (_selectables.Count == 0) return;

            if (_wrapAround)
                _currentIndex = (_currentIndex - 1 + _selectables.Count) % _selectables.Count;
            else
                _currentIndex = Mathf.Max(_currentIndex - 1, 0);

            EventSystem.current?.SetSelectedGameObject(_selectables[_currentIndex].gameObject);
        }

        public IReadOnlyList<Selectable> GetSelectables() => _selectables;
    }
}
