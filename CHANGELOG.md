# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
