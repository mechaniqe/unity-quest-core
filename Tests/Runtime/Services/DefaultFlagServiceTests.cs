using NUnit.Framework;
using DynamicBox.Quest.Core.Services;
using UnityEngine;

namespace DynamicBox.Quest.Tests.Services
{
    [TestFixture]
    public class DefaultFlagServiceTests
    {
        private GameObject _go;
        private DefaultFlagService _service;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject();
            _service = _go.AddComponent<DefaultFlagService>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void GetFlag_Default_ReturnsFalse()
        {
            Assert.That(_service.GetFlag("any-flag"), Is.False);
        }

        [Test]
        public void SetFlag_True_GetFlagReturnsTrue()
        {
            _service.SetFlag("door-open", true);

            Assert.That(_service.GetFlag("door-open"), Is.True);
        }

        [Test]
        public void SetFlag_False_GetFlagReturnsFalse()
        {
            _service.SetFlag("door-open", true);
            _service.SetFlag("door-open", false);

            Assert.That(_service.GetFlag("door-open"), Is.False);
        }

        [Test]
        public void GetCounter_Default_ReturnsZero()
        {
            Assert.That(_service.GetCounter("kills"), Is.EqualTo(0));
        }

        [Test]
        public void SetCounter_SetsValue()
        {
            _service.SetCounter("score", 42);

            Assert.That(_service.GetCounter("score"), Is.EqualTo(42));
        }

        [Test]
        public void IncrementCounter_IncreasesByAmount()
        {
            _service.SetCounter("kills", 5);

            int result = _service.IncrementCounter("kills", 3);

            Assert.That(result, Is.EqualTo(8));
            Assert.That(_service.GetCounter("kills"), Is.EqualTo(8));
        }

        [Test]
        public void IncrementCounter_DefaultAmount_IncreasesByOne()
        {
            int result = _service.IncrementCounter("steps");

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void HasFlagBeenSet_AfterSetTrue_ReturnsTrue()
        {
            _service.SetFlag("boss-defeated", true);

            Assert.That(_service.HasFlagBeenSet("boss-defeated"), Is.True);
        }

        [Test]
        public void HasFlagBeenSet_AfterSetTrueAndFalse_StillReturnsTrue()
        {
            _service.SetFlag("found-secret", true);
            _service.SetFlag("found-secret", false);

            Assert.That(_service.HasFlagBeenSet("found-secret"), Is.True);
        }

        [Test]
        public void HasFlagBeenSet_NeverSet_ReturnsFalse()
        {
            Assert.That(_service.HasFlagBeenSet("unset-flag"), Is.False);
        }

        [Test]
        public void GetAllFlags_ReflectsSetFlags()
        {
            _service.SetFlag("flag-a", true);
            _service.SetFlag("flag-b", false);

            var flags = _service.GetAllFlags();

            Assert.That(flags.ContainsKey("flag-a"), Is.True);
            Assert.That(flags.ContainsKey("flag-b"), Is.True);
        }

        [Test]
        public void ClearAll_ResetsAllFlagsAndCounters()
        {
            _service.SetFlag("some-flag", true);
            _service.SetCounter("some-counter", 99);

            _service.ClearAll();

            Assert.That(_service.GetFlag("some-flag"), Is.False);
            Assert.That(_service.GetCounter("some-counter"), Is.EqualTo(0));
            Assert.That(_service.HasFlagBeenSet("some-flag"), Is.False);
        }
    }
}
