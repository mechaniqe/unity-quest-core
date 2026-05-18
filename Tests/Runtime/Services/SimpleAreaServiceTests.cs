using NUnit.Framework;
using DynamicBox.Quest.Core.Services;
using UnityEngine;

namespace DynamicBox.Quest.Tests.Services
{
    [TestFixture]
    public class SimpleAreaServiceTests
    {
        private GameObject _go;
        private SimpleAreaService _service;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject();
            _service = _go.AddComponent<SimpleAreaService>();
            // After Awake(), CurrentAreaId == "starting_zone" and it is in _visitedAreas
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void Awake_SetsCurrentAreaToStartingZone()
        {
            Assert.That(_service.CurrentAreaId, Is.EqualTo("starting_zone"));
        }

        [Test]
        public void EnterArea_SetsCurrent()
        {
            _service.EnterArea("forest");

            Assert.That(_service.CurrentAreaId, Is.EqualTo("forest"));
        }

        [Test]
        public void ExitArea_ClearsCurrentAreaId()
        {
            _service.EnterArea("dungeon");

            _service.ExitArea();

            Assert.That(_service.CurrentAreaId, Is.Null);
        }

        [Test]
        public void HasEnteredArea_AfterEntry_ReturnsTrue()
        {
            _service.EnterArea("market");

            Assert.That(_service.HasEnteredArea("market"), Is.True);
        }

        [Test]
        public void HasEnteredArea_NeverEntered_ReturnsFalse()
        {
            Assert.That(_service.HasEnteredArea("secret-cave"), Is.False);
        }

        [Test]
        public void HasEnteredArea_StartingZone_ReturnsTrueFromAwake()
        {
            Assert.That(_service.HasEnteredArea("starting_zone"), Is.True);
        }

        [Test]
        public void IsInArea_WhileInArea_ReturnsTrue()
        {
            _service.EnterArea("tavern");

            Assert.That(_service.IsInArea("tavern"), Is.True);
        }

        [Test]
        public void IsInArea_AfterExit_ReturnsFalse()
        {
            _service.EnterArea("tavern");
            _service.ExitArea();

            Assert.That(_service.IsInArea("tavern"), Is.False);
        }

        [Test]
        public void GetVisitedAreas_IncludesPreviouslyVisitedAreas()
        {
            _service.EnterArea("village");
            _service.EnterArea("castle");

            var visited = _service.GetVisitedAreas();

            Assert.That(visited, Does.Contain("village"));
            Assert.That(visited, Does.Contain("castle"));
        }

        [Test]
        public void ClearHistory_EmptiesVisitedAndResetsToStartingZone()
        {
            _service.EnterArea("random-place");

            _service.ClearHistory();

            Assert.That(_service.GetVisitedAreas().Count, Is.EqualTo(1)); // only starting_zone
            Assert.That(_service.CurrentAreaId, Is.EqualTo("starting_zone"));
        }
    }
}
