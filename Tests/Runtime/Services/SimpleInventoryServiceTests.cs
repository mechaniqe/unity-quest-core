using NUnit.Framework;
using DynamicBox.Quest.Core.Services;
using UnityEngine;

namespace DynamicBox.Quest.Tests.Services
{
    [TestFixture]
    public class SimpleInventoryServiceTests
    {
        private GameObject _go;
        private SimpleInventoryService _service;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject();
            _service = _go.AddComponent<SimpleInventoryService>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void AddItem_IncreasesItemCount()
        {
            _service.AddItem("sword", 2);

            Assert.That(_service.GetItemCount("sword"), Is.EqualTo(2));
        }

        [Test]
        public void AddItem_AccumulatesQuantity()
        {
            _service.AddItem("gold", 10);
            _service.AddItem("gold", 5);

            Assert.That(_service.GetItemCount("gold"), Is.EqualTo(15));
        }

        [Test]
        public void RemoveItem_DecreasesItemCount()
        {
            _service.AddItem("key", 3);

            bool result = _service.RemoveItem("key", 2);

            Assert.That(result, Is.True);
            Assert.That(_service.GetItemCount("key"), Is.EqualTo(1));
        }

        [Test]
        public void RemoveItem_InsufficientQuantity_ReturnsFalse()
        {
            _service.AddItem("gem", 1);

            bool result = _service.RemoveItem("gem", 5);

            Assert.That(result, Is.False);
            Assert.That(_service.GetItemCount("gem"), Is.EqualTo(1));
        }

        [Test]
        public void HasItem_ExactQuantity_ReturnsTrue()
        {
            _service.AddItem("potion", 3);

            Assert.That(_service.HasItem("potion", 3), Is.True);
        }

        [Test]
        public void HasItem_InsufficientQuantity_ReturnsFalse()
        {
            _service.AddItem("potion", 1);

            Assert.That(_service.HasItem("potion", 5), Is.False);
        }

        [Test]
        public void HasItem_NotInInventory_ReturnsFalse()
        {
            Assert.That(_service.HasItem("arrow"), Is.False);
        }

        [Test]
        public void HasEverCollected_AfterAdd_ReturnsTrue()
        {
            _service.AddItem("key");

            Assert.That(_service.HasEverCollected("key"), Is.True);
        }

        [Test]
        public void HasEverCollected_AfterAddAndRemove_StillReturnsTrue()
        {
            _service.AddItem("key");
            _service.RemoveItem("key");

            Assert.That(_service.HasEverCollected("key"), Is.True);
        }

        [Test]
        public void HasEverCollected_NeverAdded_ReturnsFalse()
        {
            Assert.That(_service.HasEverCollected("legendary-sword"), Is.False);
        }

        [Test]
        public void GetAllItems_ReflectsInventory()
        {
            _service.AddItem("itemA", 2);
            _service.AddItem("itemB", 4);

            var items = _service.GetAllItems();

            Assert.That(items.ContainsKey("itemA"), Is.True);
            Assert.That(items.ContainsKey("itemB"), Is.True);
            Assert.That(items.Count, Is.EqualTo(2));
        }

        [Test]
        public void ClearInventory_RemovesAllItems()
        {
            _service.AddItem("stuff", 5);

            _service.ClearInventory();

            Assert.That(_service.GetItemCount("stuff"), Is.EqualTo(0));
            Assert.That(_service.HasEverCollected("stuff"), Is.False);
        }
    }
}
