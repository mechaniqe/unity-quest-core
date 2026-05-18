using System.Collections.Generic;
using NUnit.Framework;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.State;
using DynamicBox.Quest.Tests.Helpers;

namespace DynamicBox.Quest.Tests.State
{
    [TestFixture]
    public class QuestStateSnapshotTests
    {
        [Test]
        public void IsValid_NonEmptyQuestId_ReturnsTrue()
        {
            var snapshot = new QuestStateSnapshot { QuestId = "quest-1" };

            Assert.That(snapshot.IsValid(), Is.True);
        }

        [Test]
        public void IsValid_EmptyQuestId_ReturnsFalse()
        {
            var snapshot = new QuestStateSnapshot { QuestId = "" };

            Assert.That(snapshot.IsValid(), Is.False);
        }

        [Test]
        public void IsValid_NullQuestId_ReturnsFalse()
        {
            var snapshot = new QuestStateSnapshot { QuestId = null };

            Assert.That(snapshot.IsValid(), Is.False);
        }

        [Test]
        public void GetObjectiveStatusesDict_ReturnsCorrectMapping()
        {
            var snapshot = new QuestStateSnapshot
            {
                QuestId = "q1",
                ObjectiveStatuses = new List<ObjectiveStatusEntry>
                {
                    new ObjectiveStatusEntry { ObjectiveId = "obj1", Status = ObjectiveStatus.Completed },
                    new ObjectiveStatusEntry { ObjectiveId = "obj2", Status = ObjectiveStatus.InProgress },
                }
            };

            var dict = snapshot.GetObjectiveStatusesDict();

            Assert.That(dict["obj1"], Is.EqualTo(ObjectiveStatus.Completed));
            Assert.That(dict["obj2"], Is.EqualTo(ObjectiveStatus.InProgress));
        }

        [Test]
        public void GetObjectiveStatusesDict_EmptyList_ReturnsEmptyDict()
        {
            var snapshot = new QuestStateSnapshot { QuestId = "q1" };

            var dict = snapshot.GetObjectiveStatusesDict();

            Assert.That(dict.Count, Is.EqualTo(0));
        }

        [Test]
        public void QuestSaveData_IsValid_WhenAllSnapshotsValid()
        {
            var saveData = QuestSaveData.Create();
            saveData.Quests.Add(new QuestStateSnapshot { QuestId = "q1" });
            saveData.Quests.Add(new QuestStateSnapshot { QuestId = "q2" });

            Assert.That(saveData.IsValid(), Is.True);
        }

        [Test]
        public void QuestSaveData_IsValid_WhenContainsInvalidSnapshot_ReturnsFalse()
        {
            var saveData = QuestSaveData.Create();
            saveData.Quests.Add(new QuestStateSnapshot { QuestId = "q1" });
            saveData.Quests.Add(new QuestStateSnapshot { QuestId = "" });

            Assert.That(saveData.IsValid(), Is.False);
        }

        [Test]
        public void QuestSaveData_FindQuest_ExistingId_ReturnsSnapshot()
        {
            var saveData = QuestSaveData.Create();
            var snap = new QuestStateSnapshot { QuestId = "target" };
            saveData.Quests.Add(snap);

            var found = saveData.FindQuest("target");

            Assert.That(found, Is.SameAs(snap));
        }

        [Test]
        public void QuestSaveData_FindQuest_MissingId_ReturnsNull()
        {
            var saveData = QuestSaveData.Create();

            var found = saveData.FindQuest("nonexistent");

            Assert.That(found, Is.Null);
        }

        [Test]
        public void QuestSaveData_Create_HasNonNullQuestsList()
        {
            var saveData = QuestSaveData.Create();

            Assert.That(saveData.Quests, Is.Not.Null);
        }

        [Test]
        public void QuestSaveData_Create_HasSaveTimestamp()
        {
            var saveData = QuestSaveData.Create();

            Assert.That(saveData.SaveTimestamp, Is.Not.Null.And.Not.Empty);
        }
    }
}
