# Quest System Test Troubleshooting Guide

## Common Issues & Solutions

### ❌ "Type not found" errors
**Problem:** Missing assembly references
**Solution:** 
1. Check that `DynamicBox.Quest.Tests.asmdef` exists in Tests folder
2. Verify assembly references include:
   - DynamicBox.Quest.Core
   - DynamicBox.Quest.GameEvents
   - Unity Test Framework (if using Unity Test Runner)

### ❌ "EventManager.Instance is null"
**Problem:** EventManager not initialized
**Solution:** Tests initialize their own EventManager instance - this shouldn't happen

### ❌ Tests run but fail immediately
**Problem:** Missing dependencies or setup
**Solution:**
1. Run validation first: `TestValidation.ValidateAllComponents()`
2. Check Console for detailed error messages
3. Ensure Unity is in Play Mode for integration tests

### ❌ "TestExecutor component not found"
**Problem:** Script compilation issues
**Solution:**
1. Check Console for compilation errors
2. Reimport Tests folder: Right-click → Reimport
3. Restart Unity if needed

## Verification Steps

### Step 1: Basic Compilation Check
```csharp
// In Unity Console - should not show errors:
var testType = typeof(DynamicBox.Quest.Tests.QuestSystemTests);
Debug.Log($"Test type found: {testType != null}");
```

### Step 2: Quick Smoke Test
```csharp
// Should complete without errors:
DynamicBox.Quest.Tests.TestValidation.RunSmokeTest();
```

### Step 3: Infrastructure Validation  
```csharp
// Should return true:
bool valid = DynamicBox.Quest.Tests.TestValidation.ValidateAllComponents();
Debug.Log($"Infrastructure valid: {valid}");
```

## Performance Notes

- **Unit Tests**: Run in ~1-2 seconds (25+ tests)
- **Integration Tests**: Run in ~5-10 seconds (9 tests with coroutines)
- **Memory Usage**: Minimal impact, tests clean up after themselves

## When to Run Tests

### Development Workflow
- **Before commits**: Run unit tests to catch regressions
- **After changes**: Run relevant test categories
- **Before releases**: Run full test suite including integration tests

### Automated Testing
- **CI/CD Pipeline**: Use `TestRunner.RunUnitTests()` method
- **Build Validation**: Include in pre-build scripts
- **Performance Monitoring**: Regular test execution for performance baselines

## Getting Help

If tests still don't work:
1. Check Unity Console for detailed error messages
2. Verify all test files are present (see TEST_COVERAGE.md)
3. Ensure Unity version compatibility (2022.3+ recommended)
4. Check that quest system core components are properly set up
