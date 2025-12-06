# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.7.0] - 2025-12-05

### Changed
- **Unified Condition Identification**: Refactored all condition assets to use inherited `conditionId` from base `ConditionAsset` class
  - Removed `itemId` field from `ItemCollectedConditionAsset` - now uses `ConditionId`
  - Removed `_areaId` field from `AreaEnteredConditionAsset` - now uses `ConditionId`
  - Removed `_flagId` field from `CustomFlagConditionAsset` - now uses `ConditionId`
  - Removed redundant `OnValidate()` methods from subclasses
  - Auto-generation of IDs handled by base class for consistency

### Added
- **ConditionAsset.ConditionId**: Public property providing unified access to condition identifiers
  - Auto-generates IDs based on type and asset name
  - Format: `{TypeName}_{AssetName}` (lowercase with underscores)
  - Manually editable in Inspector when needed

### Improved
- Reduced code duplication across condition asset subclasses
- Simplified API - all conditions now share the same ID property
- Consistent behavior across all condition types
- Better maintainability with less code to manage per condition type

## [0.6.0] - 2025-11-29

### Added - Phase 3: Architecture Refinement
- **DirtyQueueProcessor**: Extracted dirty queue management from QuestManager
  - Centralizes objective evaluation queuing and processing
  - Manages event firing based on evaluation results
  - Provides cleaner separation of concerns
- **AssemblyInfo.cs**: Added InternalsVisibleTo attribute for test assembly
  - Allows tests to access internal members without public test accessors
  - Cleaner API surface for production code
- **QuestManager.ProcessPendingEvaluations()**: Public method for manual evaluation
  - Useful for testing, cutscenes, or forcing immediate evaluation
  - Replaces reflection-based test patterns with clean public API
  - Can be used before saving games to ensure all state is resolved

### Changed
- **ObjectiveState.CanProgress**: Moved from extension method to instance method
  - Better encapsulation - prerequisite logic now lives with the state
  - More intuitive API - `objective.CanProgress(quest)` instead of extension method
  - Simplified StatusExtensions class
- **QuestManager**: Significantly simplified through extraction of responsibilities
  - Removed 45+ lines of dirty queue processing logic
  - Removed MarkDirty and ProcessDirtySet methods (moved to DirtyQueueProcessor)
  - Events now properly delegated through processor
  - Update() method simplified to polling + processor.ProcessAll()
- **ObjectiveState**: Removed public test accessor methods
  - Removed GetCompletionInstance() and GetFailInstance()
  - Tests now use direct property access via InternalsVisibleTo
  - Cleaner public API for production use

### Improved
- Better Single Responsibility Principle adherence throughout management layer
- Reduced cyclomatic complexity in QuestManager
- More testable code through proper separation of concerns
- Cleaner internal vs public API boundaries

## [0.5.0] - 2025-11-29

### Added - Phase 2: Service Layer
- **Complete Service Interface Contracts**: All service interfaces now have proper method signatures
  - `IQuestAreaService` - Area tracking with CurrentAreaId, HasEnteredArea(), IsInArea()
  - `IQuestInventoryService` - Inventory queries with GetItemCount(), HasItem(), HasEverCollected()
  - `IQuestTimeService` - Time tracking with TotalGameTime, DeltaTime, TimeOfDay, CurrentDay
  - `IQuestFlagService` - Flag/counter system with Get/Set for bools and ints, IncrementCounter()
- **Example Service Implementations**: Production-ready default implementations
  - `DefaultTimeService` - Time tracking using Unity's Time API with configurable time scale
  - `DefaultFlagService` - In-memory flag storage with debug logging
  - `SimpleAreaService` - Trigger-based area tracking system
  - `SimpleInventoryService` - Simple item collection tracking
- **Enhanced QuestContext**: Service discovery methods
  - `GetRequiredService<T>()` - Gets service or throws helpful error
  - `GetService<T>()` - Safe service retrieval
  - `HasService<T>()` - Service availability check
- **Improved Service Integration**:
  - QuestPlayerRef now supports FlagService configuration
  - Validation warnings for misconfigured service providers
  - Better error messages when required services are missing

### Changed
- **BREAKING**: `QuestContext` constructor now accepts 4 services (added FlagService)
- `TimeElapsedConditionInstance` now properly uses `IQuestTimeService.DeltaTime` instead of Unity's Time.deltaTime
- `CustomFlagConditionInstance` now queries `IQuestFlagService` for initial flag state
- Service interfaces moved from inline to dedicated files in `Runtime/Core/Services/`
- QuestPlayerRef now includes tooltips and validation for service configuration

### Improved
- Polling conditions now use service abstractions instead of Unity static APIs
- Conditions provide helpful warnings when required services are missing
- Service layer is now fully documented and ready for production use
- Better separation between quest system and game-specific implementations

## [0.4.0] - 2025-11-29

### Added - Phase 1: Core Architecture
- **Architecture Refactoring**: Core architecture improvements
- `ObjectiveEvaluator` - Extracted evaluation logic from QuestManager for better separation of concerns
- `ConditionBindingService` - Centralized service for managing condition event bindings
- `StatusExtensions` - Extension methods for cleaner status checking (`IsTerminal()`, `IsActive()`, `CanProgress()`)
- `EventDrivenConditionBase<TEvent>` - Base class for event-driven conditions that eliminates boilerplate
- Project-wide nullable reference type annotations for improved type safety
- Comprehensive XML documentation comments on all public APIs
- HashSet-based dirty queue deduplication to prevent redundant evaluations

### Changed
- **BREAKING**: QuestManager extensively refactored - now delegates to specialized services
- **BREAKING**: Dirty queue changed from Queue to HashSet to eliminate duplicate evaluations
- `ItemCollectedConditionInstance` now inherits from `EventDrivenConditionBase`
- `CustomFlagConditionInstance` now inherits from `EventDrivenConditionBase`
- All core classes now have `#nullable enable` and proper null annotations
- Improved error handling with null checks and helpful error messages
- QuestManager.StartQuest() now immediately evaluates objectives after activation

### Improved
- Reduced code duplication across condition implementations
- Better separation of concerns following Single Responsibility Principle
- More maintainable codebase with smaller, focused classes
- Clearer status checking logic using extension methods
- Enhanced type safety with nullable reference types throughout

## [0.3.0] - 2024-12-XX

### Added
- Direct integration with DynamicBox EventManagement package
- Automatic EventManager.Instance usage (no manual wiring needed)
- GameEvents namespace with organized event classes

### Changed
- **BREAKING**: Removed IQuestEventBus abstraction layer
- **BREAKING**: All condition instances now use EventManager directly
- **BREAKING**: QuestManager now uses EventManager.Instance singleton (no manual wiring needed)
- All GameEvent classes moved to DynamicBox.Quest.GameEvents namespace
- GameEvent classes now extend DynamicBox.EventManagement.GameEvent
- Simplified setup - no EventManager reference required in QuestManager
- Tests now use EventManager.Instance directly (no fake/mock needed)

### Removed
- IQuestEventBus interface and implementations
- EventManagementQuestBus adapter class
- Custom GameEvent base class (now uses EventManagement's GameEvent)
- FakeEventManager test utility (tests use real EventManager.Instance)

### Removed
- Standalone event bus fallback (EventManager is now required)

## [0.1.0] - 2025-11-25

### Added

#### Core Systems
- `QuestAsset` - Designer-authored quest definitions (ScriptableObject)
- `ObjectiveAsset` - Objective definitions with prerequisites and conditions
- `ConditionAsset` - Base class for extensible condition system
- `ConditionGroupAsset` - Composite conditions with AND/OR operators

#### Runtime State Management
- `QuestState` - Runtime quest state and progress tracking
- `ObjectiveState` - Runtime objective state and progress tracking
- `QuestLog` - Registry for active quests
- `QuestStatus` & `ObjectiveStatus` - Proper state machine enums

#### Condition System
- `IConditionInstance` - Event-driven condition interface
- `IPollingConditionInstance` - Optional polling interface for continuous conditions
- `ConditionGroupInstance` - Composite condition evaluation with AND/OR logic
- `ItemCollectedCondition` - Example event-driven condition
- `TimeElapsedCondition` - Example polling-based condition

#### Infrastructure
- `QuestManager` - MonoBehaviour orchestrator for quest evaluation
- `QuestContext` - Service container for game services
- `QuestPlayerRef` - Helper for building QuestContext from game components
- `IQuestEventBus` - Event bus interface for condition subscriptions

#### Testing
- `FakeEventBus` - In-memory event bus for unit testing
- `QuestBuilder` - Fluent builder for programmatic quest creation
- `ObjectiveBuilder` - Fluent builder for programmatic objective creation
- `MockCondition` - Controllable condition for testing
- Comprehensive unit tests covering all major features

#### Documentation
- README.md - Quick start guide with examples
- API_REFERENCE.md - Complete API documentation
- IMPLEMENTATION.md - Technical architecture and design decisions
- PROGRESS.md - Development status and roadmap

### Architecture Features
- Event-driven evaluation with optional polling
- Dirty queue pattern for efficient batch condition evaluation
- Separation of concerns: assets (data) vs instances (runtime state)
- Extensible condition system via inheritance
- Service injection framework for game-specific logic
- Pure C# core with zero engine dependencies for testability

## [Unreleased]

### Planned for 0.2.0
- Event integration with mechaniqe/event-management library
- Custom editor inspectors for quest/objective/condition editing
- Built-in quest debugger window
- Additional example conditions (AreaEntered, CustomFlag)
- Sample project demonstrating real-world usage
