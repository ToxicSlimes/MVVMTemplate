using System;
using System.Reflection;
using Core.UI.Toolkit;
using NUnit.Framework;
using R3;
using UnityEngine.UIElements;

namespace Core.Tests.Toolkit
{
    public sealed class CounterConfirmOverlayTests
    {
        [Test]
        public void Rebind_DisposesOpenOverlayAndCancelsOldConfirmation()
        {
            var root = new VisualElement();
            var panel = new VisualElement { name = "confirm-overlay" };
            root.Add(panel);
            root.Add(new Button { name = "confirm-button" });
            root.Add(new Button { name = "cancel-button" });

            var bag = new DisposableBag();
            var oldOverlay = new ToolkitConfirmOverlay(root);
            bag.Add(oldOverlay);
            var oldCalls = 0;
            oldOverlay.Show("", "", "", "", () => oldCalls++);
            bag.Dispose();

            Assert.That(panel.style.display.value, Is.EqualTo(DisplayStyle.None));
            var newOverlay = new ToolkitConfirmOverlay(root);
            var newCalls = 0;
            newOverlay.Show("", "", "", "", () => newCalls++);
            var confirm = typeof(ToolkitConfirmOverlay).GetMethod("OnConfirmClicked", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(confirm, Is.Not.Null);
            confirm.Invoke(oldOverlay, null);
            confirm.Invoke(newOverlay, null);
            Assert.That(oldCalls, Is.Zero);
            Assert.That(newCalls, Is.EqualTo(1));
            newOverlay.Dispose();
        }
    }
}
