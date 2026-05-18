using System.Collections.Generic;
using NUnit.Framework;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Tests.Helpers;
using UnityEngine;

namespace DynamicBox.Quest.Tests.Editor.Assets
{
    [TestFixture]
    public class QuestAssetCreateForTestTests
    {
        [Test]
        public void CreateForTest_SetsQuestId()
        {
            var asset = QuestAsset.CreateForTest("my-quest-id", "Display Name", "", new List<ObjectiveAsset>());

            Assert.That(asset.QuestId, Is.EqualTo("my-quest-id"));
        }

        [Test]
        public void CreateForTest_SetsDisplayName()
        {
            var asset = QuestAsset.CreateForTest("q1", "My Quest Name", "", new List<ObjectiveAsset>());

            Assert.That(asset.DisplayName, Is.EqualTo("My Quest Name"));
        }

        [Test]
        public void CreateForTest_SetsDescription()
        {
            var asset = QuestAsset.CreateForTest("q1", "", "Quest description text.", new List<ObjectiveAsset>());

            Assert.That(asset.Description, Is.EqualTo("Quest description text."));
        }

        [Test]
        public void CreateForTest_WithObjectives_SetsObjectivesList()
        {
            var obj1 = new ObjectiveBuilder().WithObjectiveId("obj1").Build();
            var obj2 = new ObjectiveBuilder().WithObjectiveId("obj2").Build();

            var asset = QuestAsset.CreateForTest("q1", "", "", new List<ObjectiveAsset> { obj1, obj2 });

            Assert.That(asset.Objectives.Count, Is.EqualTo(2));
        }

        [Test]
        public void CreateForTest_EmptyObjectives_ObjectivesListEmpty()
        {
            var asset = QuestAsset.CreateForTest("q1", "", "", new List<ObjectiveAsset>());

            Assert.That(asset.Objectives.Count, Is.EqualTo(0));
        }

        [Test]
        public void CreateForTest_ReturnsNonNullAsset()
        {
            var asset = QuestAsset.CreateForTest("q1", "Name", "Desc", new List<ObjectiveAsset>());

            Assert.That(asset, Is.Not.Null);
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup is handled by Unity's GC for ScriptableObjects created with CreateInstance
        }
    }
}
