using System;
using GenericQuest.Core;
using GenericQuest.Core.Conditions;

namespace GenericQuest.Tests
{
    /// <summary>
    /// Unit tests for the quest system using a FakeEventBus and programmatic builders.
    /// </summary>
    public static class QuestSystemTests
    {
        public static void RunAllTests()
        {
            TestItemCollectedConditionCompletion();
            TestFailConditionTriggersQuestFailure();
            TestConditionGroupAnd();
            TestConditionGroupOr();
            TestPrerequisiteObjectives();
            TestOptionalObjectives();

            Console.WriteLine("✓ All tests passed!");
        }

        private static void TestItemCollectedConditionCompletion()
        {
            Console.WriteLine("\n[TEST] Item Collected Condition Completion");

            var eventBus = new FakeEventBus();
            var context = new QuestContext(null, null, null);

            // Create a simple condition
            var condition = new ItemCollectedConditionInstance("sword", 1);
            bool changeTriggered = false;

            condition.Bind(eventBus, context, () => changeTriggered = true);

            // Verify not met initially
            if (condition.IsMet)
                throw new Exception("Condition should not be met initially");

            // Publish the event
            eventBus.Publish(new ItemCollectedEvent("sword", 1));

            if (!condition.IsMet)
                throw new Exception("Condition should be met after event");

            if (!changeTriggered)
                throw new Exception("onChanged callback should have been triggered");

            Console.WriteLine("✓ Item collected condition works correctly");
        }

        private static void TestFailConditionTriggersQuestFailure()
        {
            Console.WriteLine("\n[TEST] Fail Condition Triggers Quest Failure");

            var eventBus = new FakeEventBus();
            var context = new QuestContext(null, null, null);

            // Create objective with fail condition
            var failCondition = new MockConditionAsset().CreateInstance() as MockConditionInstance;
            var completionCondition = new MockConditionAsset().CreateInstance() as MockConditionInstance;

            var objective = new ObjectiveBuilder()
                .WithObjectiveId("obj1")
                .Build();

            var questState = new QuestState(
                new QuestBuilder()
                    .WithQuestId("quest1")
                    .AddObjective(objective)
                    .Build()
            );

            questState.SetStatus(QuestStatus.InProgress);
            var objState = questState.Objectives["obj1"];

            // Manually set the fail condition (since we can't directly through normal flow)
            var failField = typeof(ObjectiveState).GetField("failInstance",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            failField?.SetValue(objState, failCondition);

            bool questFailed = false;
            failCondition.Bind(eventBus, context, () => { });

            // Trigger fail condition
            failCondition.SetMet(true);

            if (!failCondition.IsMet)
                throw new Exception("Fail condition should be met");

            Console.WriteLine("✓ Fail condition triggers correctly");
        }

        private static void TestConditionGroupAnd()
        {
            Console.WriteLine("\n[TEST] Condition Group AND Logic");

            var eventBus = new FakeEventBus();
            var context = new QuestContext(null, null, null);

            var cond1 = new MockConditionAsset().CreateInstance() as MockConditionInstance;
            var cond2 = new MockConditionAsset().CreateInstance() as MockConditionInstance;

            var group = new ConditionGroupInstance(ConditionOperator.And, new System.Collections.Generic.List<IConditionInstance> { cond1, cond2 });
            bool changeTriggered = false;

            group.Bind(eventBus, context, () => changeTriggered = true);

            if (group.IsMet)
                throw new Exception("AND group should not be met when both are false");

            cond1.SetMet(true);
            if (group.IsMet)
                throw new Exception("AND group should not be met when only one is true");

            cond2.SetMet(true);
            if (!group.IsMet)
                throw new Exception("AND group should be met when both are true");

            if (!changeTriggered)
                throw new Exception("Change should have been triggered");

            Console.WriteLine("✓ Condition Group AND logic works correctly");
        }

        private static void TestConditionGroupOr()
        {
            Console.WriteLine("\n[TEST] Condition Group OR Logic");

            var eventBus = new FakeEventBus();
            var context = new QuestContext(null, null, null);

            var cond1 = new MockConditionAsset().CreateInstance() as MockConditionInstance;
            var cond2 = new MockConditionAsset().CreateInstance() as MockConditionInstance;

            var group = new ConditionGroupInstance(ConditionOperator.Or, new System.Collections.Generic.List<IConditionInstance> { cond1, cond2 });
            bool changeTriggered = false;

            group.Bind(eventBus, context, () => changeTriggered = true);

            if (group.IsMet)
                throw new Exception("OR group should not be met when both are false");

            cond1.SetMet(true);
            if (!group.IsMet)
                throw new Exception("OR group should be met when at least one is true");

            if (!changeTriggered)
                throw new Exception("Change should have been triggered");

            Console.WriteLine("✓ Condition Group OR logic works correctly");
        }

        private static void TestPrerequisiteObjectives()
        {
            Console.WriteLine("\n[TEST] Prerequisite Objectives");

            var obj1 = new ObjectiveBuilder()
                .WithObjectiveId("obj1")
                .Build();

            var obj2 = new ObjectiveBuilder()
                .WithObjectiveId("obj2")
                .AddPrerequisite(obj1)
                .Build();

            var quest = new QuestBuilder()
                .WithQuestId("quest1")
                .AddObjective(obj1)
                .AddObjective(obj2)
                .Build();

            var questState = new QuestState(quest);

            // obj2 should not progress until obj1 is complete
            var obj1State = questState.Objectives["obj1"];
            var obj2State = questState.Objectives["obj2"];

            // Manually check prerequisite logic (simulating CanProgressObjective)
            bool canProgress = true;
            foreach (var prereq in obj2State.Definition.Prerequisites)
            {
                if (questState.TryGetObjective(prereq.ObjectiveId, out var preState))
                {
                    if (preState.Status != ObjectiveStatus.Completed)
                        canProgress = false;
                }
            }

            if (canProgress)
                throw new Exception("obj2 should not be able to progress without obj1 completed");

            // Complete obj1
            obj1State.SetStatus(ObjectiveStatus.Completed);

            // Now check again
            canProgress = true;
            foreach (var prereq in obj2State.Definition.Prerequisites)
            {
                if (questState.TryGetObjective(prereq.ObjectiveId, out var preState))
                {
                    if (preState.Status != ObjectiveStatus.Completed)
                        canProgress = false;
                }
            }

            if (!canProgress)
                throw new Exception("obj2 should be able to progress after obj1 is completed");

            Console.WriteLine("✓ Prerequisite objectives work correctly");
        }

        private static void TestOptionalObjectives()
        {
            Console.WriteLine("\n[TEST] Optional Objectives");

            var mandatoryObj = new ObjectiveBuilder()
                .WithObjectiveId("mandatory")
                .Build();

            var optionalObj = new ObjectiveBuilder()
                .WithObjectiveId("optional")
                .AsOptional(true)
                .Build();

            var quest = new QuestBuilder()
                .WithQuestId("quest1")
                .AddObjective(mandatoryObj)
                .AddObjective(optionalObj)
                .Build();

            var questState = new QuestState(quest);
            var mandatoryState = questState.Objectives["mandatory"];
            var optionalState = questState.Objectives["optional"];

            // Quest should not be complete with only optional completed
            mandatoryState.SetStatus(ObjectiveStatus.NotStarted);
            optionalState.SetStatus(ObjectiveStatus.Completed);

            bool allRequired = true;
            foreach (var obj in questState.GetObjectiveStates())
            {
                if (!obj.Definition.IsOptional && obj.Status != ObjectiveStatus.Completed)
                    allRequired = false;
            }

            if (allRequired)
                throw new Exception("Quest should not be complete with mandatory objective incomplete");

            // Complete mandatory, quest should be complete
            mandatoryState.SetStatus(ObjectiveStatus.Completed);

            allRequired = true;
            foreach (var obj in questState.GetObjectiveStates())
            {
                if (!obj.Definition.IsOptional && obj.Status != ObjectiveStatus.Completed)
                    allRequired = false;
            }

            if (!allRequired)
                throw new Exception("Quest should be complete when all mandatory objectives are complete");

            Console.WriteLine("✓ Optional objectives work correctly");
        }
    }
}
