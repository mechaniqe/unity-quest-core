using System.Collections.Generic;
using NUnit.Framework;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Tests.Helpers;

namespace DynamicBox.Quest.Tests.Editor.Assets
{
    [TestFixture]
    public class ObjectiveAssetCreateForTestTests
    {
        [Test]
        public void CreateForTest_SetsObjectiveId()
        {
            var asset = ObjectiveAsset.CreateForTest("my-obj-id", "Title", "Desc",
                false, null, null, null);

            Assert.That(asset.ObjectiveId, Is.EqualTo("my-obj-id"));
        }

        [Test]
        public void CreateForTest_IsOptional_True()
        {
            var asset = ObjectiveAsset.CreateForTest("obj", "", "", true, null, null, null);

            Assert.That(asset.IsOptional, Is.True);
        }

        [Test]
        public void CreateForTest_IsOptional_False()
        {
            var asset = ObjectiveAsset.CreateForTest("obj", "", "", false, null, null, null);

            Assert.That(asset.IsOptional, Is.False);
        }

        [Test]
        public void CreateForTest_IsRetryable_True()
        {
            var asset = ObjectiveAsset.CreateForTest("obj", "", "", false, null, null, null, isRetryable: true);

            Assert.That(asset.IsRetryable, Is.True);
        }

        [Test]
        public void CreateForTest_IsRetryable_DefaultFalse()
        {
            var asset = ObjectiveAsset.CreateForTest("obj", "", "", false, null, null, null);

            Assert.That(asset.IsRetryable, Is.False);
        }

        [Test]
        public void CreateForTest_WithPrerequisites_SetsPrerequisitesList()
        {
            var prereq = new ObjectiveBuilder().WithObjectiveId("prereq").Build();

            var asset = ObjectiveAsset.CreateForTest("obj", "", "", false,
                new List<ObjectiveAsset> { prereq }, null, null);

            Assert.That(asset.Prerequisites.Count, Is.EqualTo(1));
            Assert.That(asset.Prerequisites[0].ObjectiveId, Is.EqualTo("prereq"));
        }

        [Test]
        public void CreateForTest_WithCompletionCondition_SetsCompletionCondition()
        {
            var conditionAsset = UnityEngine.ScriptableObject.CreateInstance<MockConditionAsset>();

            var asset = ObjectiveAsset.CreateForTest("obj", "", "", false, null, conditionAsset, null);

            Assert.That(asset.CompletionCondition, Is.SameAs(conditionAsset));
        }

        [Test]
        public void CreateForTest_WithFailCondition_SetsFailCondition()
        {
            var conditionAsset = UnityEngine.ScriptableObject.CreateInstance<MockConditionAsset>();

            var asset = ObjectiveAsset.CreateForTest("obj", "", "", false, null, null, conditionAsset);

            Assert.That(asset.FailCondition, Is.SameAs(conditionAsset));
        }

        [Test]
        public void CreateForTest_NullPrerequisites_PrerequisitesListEmpty()
        {
            var asset = ObjectiveAsset.CreateForTest("obj", "", "", false, null, null, null);

            // Prerequisites with null passed should be null or empty
            // either is acceptable from the contract
            bool isEmptyOrNull = asset.Prerequisites == null || asset.Prerequisites.Count == 0;
            Assert.That(isEmptyOrNull, Is.True);
        }
    }
}
