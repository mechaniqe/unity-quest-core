# Quest System Test Suite

This document provides a comprehensive guide to the quest system test suite, covering all critical functionality testing scenarios.

## Overview

The quest system includes multiple layers of testing:

1. **Unit Tests** (`QuestSystemTests.cs`) - Test individual components in isolation
2. **Integration Tests** (`QuestSystemIntegrationTests.cs`) - Test Unity component interactions
3. **Test Helpers** - Builder patterns and mock objects for test setup
4. **Test Validation** - Verification that test infrastructure is properly configured

## Running Tests

### Unit Tests (Standalone)

```csharp
// From code
DynamicBox.Quest.Tests.TestRunner.RunUnitTests();

// From Unity Console
DynamicBox.Quest.Tests.QuestSystemTests.RunAllTests();
```

### Integration Tests (Unity)

1. Add `QuestSystemIntegrationTests` component to a GameObject in your scene
2. Configure the component (optional):
   - Set `runTestsOnStart` to automatically run tests on Start()
   - Adjust `testDelay` for timing between test steps
3. Play the scene to run integration tests

### Test Validation

```csharp
// Validate test setup
DynamicBox.Quest.Tests.TestValidation.ValidateAllComponents();

// Run smoke test
DynamicBox.Quest.Tests.TestValidation.RunSmokeTest();
```

## Test Categories

### 1. Basic Condition Tests
- `TestItemCollectedConditionCompletion()` - Basic item collection
- `TestItemCollectedConditionMultipleEvents()` - Progressive item collection
- `TestItemCollectedConditionUnbinding()` - Event cleanup

### 2. Fail Condition Tests
- `TestFailConditionTriggersQuestFailure()` - Quest failure scenarios

### 3. Condition Group Tests
- `TestConditionGroupAnd()` - AND logic combinations
- `TestConditionGroupOr()` - OR logic combinations
- `TestConditionGroupNestedLogic()` - Complex nested conditions
- `TestConditionGroupPollingChildren()` - Polling condition groups

### 4. Quest Structure Tests
- `TestPrerequisiteObjectives()` - Objective dependencies
- `TestOptionalObjectives()` - Optional vs mandatory objectives
- `TestMultiplePrerequisites()` - Complex prerequisite chains

### 5. Event-Driven Condition Tests
- `TestAreaEnteredCondition()` - Location-based conditions
- `TestCustomFlagCondition()` - Flag-based conditions
- `TestCustomFlagConditionToggle()` - Flag state changes

### 6. Polling Condition Tests
- `TestTimeElapsedCondition()` - Time-based conditions
- `TestPollingConditionIntegration()` - Polling system integration

### 7. QuestManager Integration Tests
- `TestQuestManagerStartStopQuest()` - Quest lifecycle management
- `TestQuestManagerEventHandling()` - Event system integration
- `TestQuestManagerPollingIntegration()` - Polling system coordination
- `TestQuestManagerMultipleQuests()` - Multiple concurrent quests

### 8. State Management Tests
- `TestQuestStateTransitions()` - Quest status changes
- `TestObjectiveStateTransitions()` - Objective status changes
- `TestQuestLogManagement()` - Quest log operations

### 9. Edge Cases and Error Handling
- `TestNullConditionHandling()` - Null safety
- `TestEmptyQuestHandling()` - Empty quest scenarios
- `TestDuplicateObjectiveIds()` - ID collision handling
- `TestCircularPrerequisites()` - Circular dependency detection

### 10. Complete Quest Flows
- `TestCompleteQuestFlow()` - End-to-end quest completion
- `TestComplexQuestWithFailure()` - Complex failure scenarios

## Test Infrastructure

### Mock Classes

#### MockConditionAsset / MockConditionInstance
- Simulates any condition type for testing
- Allows manual triggering of condition states
- Used for testing condition logic without external dependencies

#### MockPollingConditionAsset / MockPollingConditionInstance  
- Extends mock condition with polling capabilities
- Tests polling system integration
- Tracks polling calls for verification

### Builder Classes

#### QuestBuilder
- Fluent API for creating test quests
- Simplifies test setup
- Ensures consistent quest configuration

```csharp
var quest = new QuestBuilder()
    .WithQuestId("test_quest")
    .WithDisplayName("Test Quest")
    .AddObjective(objective)
    .Build();
```

#### ObjectiveBuilder
- Fluent API for creating test objectives
- Supports prerequisites, conditions, and options
- Chainable configuration methods

```csharp
var objective = new ObjectiveBuilder()
    .WithObjectiveId("obj1")
    .WithCompletionCondition(condition)
    .WithFailCondition(failCondition)
    .AddPrerequisite(prereqObjective)
    .AsOptional(true)
    .Build();
```

## Test Patterns

### Event Testing Pattern
```csharp
// 1. Create condition
var condition = new ItemCollectedConditionInstance("sword", 1);

// 2. Bind to event system
condition.Bind(eventManager, context, () => changeTriggered = true);

// 3. Trigger event
eventManager.Raise(new ItemCollectedEvent("sword", 1));

// 4. Verify result
if (!condition.IsMet)
    throw new Exception("Condition should be met");
```

### State Transition Testing Pattern
```csharp
// 1. Create initial state
var questState = new QuestState(quest);

// 2. Verify initial state
if (questState.Status != QuestStatus.NotStarted)
    throw new Exception("Initial state incorrect");

// 3. Trigger transition
questState.SetStatus(QuestStatus.InProgress);

// 4. Verify new state
if (questState.Status != QuestStatus.InProgress)
    throw new Exception("State transition failed");
```

### Integration Testing Pattern
```csharp
// 1. Create quest manager
var questManager = CreateTestQuestManager();

// 2. Set up event handlers
bool questCompleted = false;
questManager.OnQuestCompleted += (q) => questCompleted = true;

// 3. Start quest
var questState = questManager.StartQuest(quest);

// 4. Trigger completion conditions
// ... trigger events or conditions

// 5. Verify integration
if (!questCompleted)
    throw new Exception("Quest should be completed");
```

## Best Practices

### Test Isolation
- Each test method should be independent
- Use fresh instances for each test
- Clean up resources after tests

### Error Messages
- Provide descriptive error messages
- Include expected vs actual values
- Use consistent error message format

### Test Data
- Use meaningful test data (realistic item names, etc.)
- Keep test data simple and focused
- Avoid complex test scenarios unless specifically testing complexity

### Mocking
- Use mock objects to isolate units under test
- Mock external dependencies (Unity components, file system, etc.)
- Verify mock interactions when relevant

## Troubleshooting

### Common Issues

#### "Condition should not be met initially"
- Check that mock conditions start in false state
- Verify event binding order
- Ensure clean test setup

#### "Change should be triggered"
- Verify event handler registration
- Check that events are being raised correctly
- Ensure condition logic is working

#### "Quest should be completed"
- Check prerequisite objective completion
- Verify all mandatory objectives are completed
- Ensure event processing is called

### Debugging Tests

1. Add debug output to test methods
2. Use Unity's Debug.Log for integration tests
3. Verify test setup before running assertions
4. Check event manager state between test steps

## Future Enhancements

### Planned Test Additions
- Performance tests for large quest systems
- Stress tests with many concurrent quests
- Serialization/deserialization tests
- Save/load system integration tests

### Test Infrastructure Improvements
- Automated test report generation
- Test coverage analysis
- Performance benchmarking
- Continuous integration setup
