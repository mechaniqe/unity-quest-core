Unity Quest Core – Technical Specification (v0.1)
0. Summary

We want a Unity-first, designer-friendly, event-driven quest system that:

Uses ScriptableObjects for quest/objective/condition definitions.

Uses our existing event-management system as the backbone (dependency).

Evaluates quest progress primarily via events, with optional light polling for “continuous” conditions.

Is easy for designers to author (no code) and easy for engineers to extend.

Persistence, multi-actor/party support, and fancy graph editors are explicitly out of scope for v0.1.

1. Goals & Non-Goals
1.1. Goals

Provide a generic mission/quest core for Unity projects.

Make it data-driven:

Quests & objectives = ScriptableObjects.

Conditions = ScriptableObjects + runtime instances.

Use event-driven architecture:

Game events (e.g. item collected, area entered) drive conditions.

System reacts only when something changes (no heavy per-frame polling).

Support composite objectives:

Prerequisites between objectives (simple graph).

Optional objectives.

Provide logical condition composition:

AND / OR via ConditionGroupAsset.

Be extensible:

Easy to add new condition types and integrate custom game events.

Be testable:

Core logic testable in pure C# with a fake event bus.

1.2. Non-Goals (for v0.1)

No save/load integration (persistence will be handled by a separate system).

No multi-actor/party semantics (we assume one “owner context” for quests).

No sophisticated quest graph editor (just basic inspectors; graph view is a future enhancement).

No direct coupling to a specific game (should be applicable across projects, though obviously Unity-specific).

2. Project Structure & Namespaces
2.1. Unity folder structure
Assets/
  GenericQuestCore/
    Runtime/
      Core/
        QuestAsset.cs
        ObjectiveAsset.cs
        ConditionAsset.cs
        ConditionGroupAsset.cs
        QuestState.cs
        ObjectiveState.cs
        QuestLog.cs
        QuestManager.cs
        QuestContext.cs
        IQuestEventBus.cs
      EventManagementAdapter/
        EventManagementQuestBus.cs
    Editor/
      Inspectors/
        QuestAssetEditor.cs
        ObjectiveListDrawer.cs
        ConditionGroupEditor.cs
      Windows/
        QuestDebuggerWindow.cs  (minimal v0, nice-to-have if time)

2.2. Namespaces

Suggested namespaces:

GenericQuest.Core – core runtime types (assets, state, manager, context, bus interface).

GenericQuest.EventManagement – adapter to mechaniqe/event-management.

GenericQuest.Editor – inspectors, editor windows.

3. Core Data Model (ScriptableObjects)
3.1. QuestAsset

File: QuestAsset.cs
Namespace: GenericQuest.Core

Represents a single quest definition.

using UnityEngine;
using System.Collections.Generic;

namespace GenericQuest.Core
{
    [CreateAssetMenu(menuName = "Quests/Quest", fileName = "NewQuest")]
    public class QuestAsset : ScriptableObject
    {
        [SerializeField] private string questId;
        [SerializeField] private string displayName;
        [TextArea] [SerializeField] private string description;

        // For now, objectives are referenced as sub-assets or direct references.
        [SerializeField] private List<ObjectiveAsset> objectives = new();

        public string QuestId => questId;
        public string DisplayName => displayName;
        public string Description => description;
        public IReadOnlyList<ObjectiveAsset> Objectives => objectives;
    }
}


Notes:

Objectives will usually be sub-assets of the QuestAsset (created via custom editor).

questId must be unique; no enforcement in code, but can be validated later.

3.2. ObjectiveAsset

File: ObjectiveAsset.cs
Namespace: GenericQuest.Core

Represents a single objective within a quest.

using UnityEngine;
using System.Collections.Generic;

namespace GenericQuest.Core
{
    [CreateAssetMenu(menuName = "Quests/Objective", fileName = "NewObjective")]
    public class ObjectiveAsset : ScriptableObject
    {
        [SerializeField] private string objectiveId;
        [SerializeField] private string title;
        [TextArea] [SerializeField] private string description;
        [SerializeField] private bool isOptional;

        // Other objectives that must be completed first
        [SerializeField] private List<ObjectiveAsset> prerequisites = new();

        // Completion and failure conditions (see ConditionAsset)
        [SerializeField] private ConditionAsset completionCondition;  // can be ConditionGroupAsset
        [SerializeField] private ConditionAsset failCondition;        // optional

        public string ObjectiveId => objectiveId;
        public string Title => title;
        public string Description => description;
        public bool IsOptional => isOptional;

        public IReadOnlyList<ObjectiveAsset> Prerequisites => prerequisites;
        public ConditionAsset CompletionCondition => completionCondition;
        public ConditionAsset FailCondition => failCondition;
    }
}


Notes:

We use single completionCondition and failCondition, which can themselves be ConditionGroupAsset to support logical composition.

For v0.1, we don’t enforce cycle detection in code (documented in README as a caveat).

3.3. ConditionAsset (base) + ConditionGroupAsset

File: ConditionAsset.cs
Namespace: GenericQuest.Core

using UnityEngine;

namespace GenericQuest.Core
{
    /// <summary>
    /// Base class for designer-authored condition configuration.
    /// IMPORTANT: Do not store runtime state here. All mutable state must live in IConditionInstance.
    /// </summary>
    public abstract class ConditionAsset : ScriptableObject
    {
        public abstract IConditionInstance CreateInstance();
    }
}

ConditionGroupAsset

File: ConditionGroupAsset.cs
Namespace: GenericQuest.Core

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace GenericQuest.Core
{
    public enum ConditionOperator
    {
        And,
        Or
    }

    [CreateAssetMenu(menuName = "Quests/Condition Group", fileName = "NewConditionGroup")]
    public class ConditionGroupAsset : ConditionAsset
    {
        [SerializeField] private ConditionOperator @operator = ConditionOperator.And;
        [SerializeField] private List<ConditionAsset> children = new();

        public override IConditionInstance CreateInstance()
        {
            var instances = children
                .Where(c => c != null)
                .Select(c => c.CreateInstance())
                .ToList();

            return new ConditionGroupInstance(@operator, instances);
        }
    }
}

4. Runtime Model
4.1. Condition instance interfaces

File: IConditionInstance.cs
Namespace: GenericQuest.Core

using System;

namespace GenericQuest.Core
{
    public interface IConditionInstance
    {
        bool IsMet { get; }

        void Bind(IQuestEventBus eventBus, QuestContext context, Action onChanged);
        void Unbind(IQuestEventBus eventBus, QuestContext context);
    }

    // Optional interface for conditions that also want periodic polling.
    public interface IPollingConditionInstance
    {
        void Refresh(QuestContext context, Action onChanged);
    }
}

4.2. ConditionGroupInstance

File: ConditionGroupInstance.cs
Namespace: GenericQuest.Core

using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericQuest.Core
{
    public sealed class ConditionGroupInstance : IConditionInstance, IPollingConditionInstance
    {
        private readonly ConditionOperator _operator;
        private readonly List<IConditionInstance> _children;
        private readonly List<IPollingConditionInstance> _pollingChildren;

        private bool _isMet;
        private Action? _onChanged;

        public bool IsMet => _isMet;

        public ConditionGroupInstance(
            ConditionOperator @operator,
            List<IConditionInstance> children)
        {
            _operator = @operator;
            _children = children ?? new List<IConditionInstance>();
            _pollingChildren = _children.OfType<IPollingConditionInstance>().ToList();
        }

        public void Bind(IQuestEventBus eventBus, QuestContext context, Action onChanged)
        {
            _onChanged = onChanged;

            foreach (var child in _children)
                child.Bind(eventBus, context, ChildChanged);

            Recompute();
        }

        public void Unbind(IQuestEventBus eventBus, QuestContext context)
        {
            foreach (var child in _children)
                child.Unbind(eventBus, context);

            _onChanged = null;
        }

        public void Refresh(QuestContext context, Action onChanged)
        {
            foreach (var child in _pollingChildren)
                child.Refresh(context, ChildChanged);
        }

        private void ChildChanged()
        {
            if (Recompute())
                _onChanged?.Invoke();
        }

        private bool Recompute()
        {
            bool old = _isMet;

            _isMet = _operator switch
            {
                ConditionOperator.And => _children.All(c => c.IsMet),
                ConditionOperator.Or  => _children.Any(c => c.IsMet),
                _                     => _isMet
            };

            return _isMet != old;
        }
    }
}

4.3. Status enums

File: QuestStatus.cs
Namespace: GenericQuest.Core

namespace GenericQuest.Core
{
    public enum QuestStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }

    public enum ObjectiveStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }
}

4.4. ObjectiveState

File: ObjectiveState.cs
Namespace: GenericQuest.Core

using System.Collections.Generic;

namespace GenericQuest.Core
{
    public sealed class ObjectiveState
    {
        public ObjectiveAsset Definition { get; }
        public ObjectiveStatus Status { get; private set; } = ObjectiveStatus.NotStarted;

        internal IConditionInstance? CompletionInstance { get; }
        internal IConditionInstance? FailInstance { get; }

        public ObjectiveState(ObjectiveAsset definition)
        {
            Definition = definition;

            if (definition.CompletionCondition != null)
                CompletionInstance = definition.CompletionCondition.CreateInstance();

            if (definition.FailCondition != null)
                FailInstance = definition.FailCondition.CreateInstance();
        }

        internal void SetStatus(ObjectiveStatus status)
        {
            Status = status;
        }
    }
}

4.5. QuestState

File: QuestState.cs
Namespace: GenericQuest.Core

using System.Collections.Generic;
using System.Linq;

namespace GenericQuest.Core
{
    public sealed class QuestState
    {
        public QuestAsset Definition { get; }
        public QuestStatus Status { get; private set; } = QuestStatus.NotStarted;

        // Keyed by ObjectiveId
        public IReadOnlyDictionary<string, ObjectiveState> Objectives => _objectives;
        private readonly Dictionary<string, ObjectiveState> _objectives;

        public QuestState(QuestAsset definition)
        {
            Definition = definition;
            _objectives = definition.Objectives
                .Where(o => o != null)
                .ToDictionary(
                    o => o.ObjectiveId,
                    o => new ObjectiveState(o)
                );
        }

        internal void SetStatus(QuestStatus status)
        {
            Status = status;
        }

        internal IEnumerable<ObjectiveState> GetObjectiveStates() => _objectives.Values;

        internal bool TryGetObjective(string objectiveId, out ObjectiveState state)
            => _objectives.TryGetValue(objectiveId, out state);
    }
}

4.6. QuestLog

File: QuestLog.cs
Namespace: GenericQuest.Core

using System.Collections.Generic;

namespace GenericQuest.Core
{
    public sealed class QuestLog
    {
        private readonly List<QuestState> _active = new();

        public IReadOnlyList<QuestState> Active => _active;

        public QuestState StartQuest(QuestAsset quest)
        {
            var state = new QuestState(quest);
            state.SetStatus(QuestStatus.InProgress);
            _active.Add(state);
            return state;
        }

        public void RemoveQuest(QuestState state)
        {
            _active.Remove(state);
        }
    }
}

5. Event Bus Integration
5.1. IQuestEventBus

File: IQuestEventBus.cs
Namespace: GenericQuest.Core

using System;

namespace GenericQuest.Core
{
    public interface IQuestEventBus
    {
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
        void Publish<TEvent>(TEvent evt) where TEvent : class; // optional use
    }
}

5.2. EventManagementQuestBus (adapter)

File: EventManagementQuestBus.cs
Namespace: GenericQuest.EventManagement

Implementation must wrap the actual API from
https://github.com/mechaniqe/event-management
This is a placeholder; dev should map the calls to the real EventManager.

using System;
using GenericQuest.Core;
using Mechaniqe.EventManagement; // adjust to actual namespace

namespace GenericQuest.EventManagement
{
    public sealed class EventManagementQuestBus : IQuestEventBus
    {
        private readonly EventManager _eventManager;

        public EventManagementQuestBus(EventManager eventManager)
        {
            _eventManager = eventManager;
        }

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            // TODO: Map to actual event-management API
            _eventManager.RegisterListener(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            _eventManager.UnregisterListener(handler);
        }

        public void Publish<TEvent>(TEvent evt) where TEvent : class
        {
            _eventManager.DispatchEvent(evt);
        }
    }
}

6. QuestContext & Services

File: QuestContext.cs
Namespace: GenericQuest.Core

namespace GenericQuest.Core
{
    // Marker holder for whatever services the game offers to conditions.
    // For v0.1, this is minimal and can be expanded per project.
    public sealed class QuestContext
    {
        // Example: inventory, areas, time, etc.
        // These are interfaces implemented by game-side systems.

        public IQuestAreaService? AreaService { get; }
        public IQuestInventoryService? InventoryService { get; }
        public IQuestTimeService? TimeService { get; }

        public QuestContext(
            IQuestAreaService? areaService,
            IQuestInventoryService? inventoryService,
            IQuestTimeService? timeService)
        {
            AreaService = areaService;
            InventoryService = inventoryService;
            TimeService = timeService;
        }
    }

    // For v0.1, we only define minimal interfaces; actual implementations live in game code.
    public interface IQuestAreaService { /* optional for now */ }
    public interface IQuestInventoryService { /* optional for now */ }
    public interface IQuestTimeService { /* optional for now */ }
}


The actual game will implement these interfaces and pass them in via a QuestPlayerRef MonoBehaviour or similar.

7. QuestManager (MonoBehaviour, hybrid event/polling)

File: QuestManager.cs
Namespace: GenericQuest.Core

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericQuest.Core
{
    public class QuestManager : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private MonoBehaviour eventManagerSource; // reference to concrete EventManager
        [SerializeField] private QuestPlayerRef playerRef;          // builds QuestContext

        [Header("Polling (optional)")]
        [SerializeField] private bool enablePolling = true;
        [SerializeField] private float pollingInterval = 0.25f;

        private IQuestEventBus _eventBus;
        private QuestLog _log;
        private QuestContext _context;

        private readonly Queue<(QuestState quest, ObjectiveState obj)> _dirtyQueue = new();
        private float _pollTimer;

        public event Action<QuestState> OnQuestCompleted;
        public event Action<QuestState> OnQuestFailed;
        public event Action<ObjectiveState> OnObjectiveStatusChanged;

        private void Awake()
        {
            // eventManagerSource must be cast/wrapped into concrete adapter in game code
            _eventBus = CreateEventBus(eventManagerSource);
            _log = new QuestLog();
            _context = playerRef.BuildContext();
        }

        private IQuestEventBus CreateEventBus(MonoBehaviour source)
        {
            // This method can be overridden or changed to use DI.
            // For now, assume source has an EventManager component.
            var eventManager = source.GetComponent<Mechaniqe.EventManagement.EventManager>();
            return new GenericQuest.EventManagement.EventManagementQuestBus(eventManager);
        }

        private void Update()
        {
            if (enablePolling)
            {
                _pollTimer += Time.deltaTime;
                if (_pollTimer >= pollingInterval)
                {
                    _pollTimer = 0f;
                    PollConditions();
                }
            }

            ProcessDirtyQueue();
        }

        public QuestState StartQuest(QuestAsset questAsset)
        {
            var state = _log.StartQuest(questAsset);
            BindQuestConditions(state);
            return state;
        }

        public void StopQuest(QuestState questState)
        {
            UnbindQuestConditions(questState);
            _log.RemoveQuest(questState);
        }

        private void BindQuestConditions(QuestState quest)
        {
            foreach (var obj in quest.GetObjectiveStates())
            {
                if (obj.CompletionInstance != null)
                    obj.CompletionInstance.Bind(_eventBus, _context, () => MarkDirty(quest, obj));

                if (obj.FailInstance != null)
                    obj.FailInstance.Bind(_eventBus, _context, () => MarkDirty(quest, obj));
            }
        }

        private void UnbindQuestConditions(QuestState quest)
        {
            foreach (var obj in quest.GetObjectiveStates())
            {
                if (obj.CompletionInstance != null)
                    obj.CompletionInstance.Unbind(_eventBus, _context);

                if (obj.FailInstance != null)
                    obj.FailInstance.Unbind(_eventBus, _context);
            }
        }

        private void PollConditions()
        {
            foreach (var quest in _log.Active)
            {
                if (quest.Status is QuestStatus.Completed or QuestStatus.Failed)
                    continue;

                foreach (var obj in quest.GetObjectiveStates())
                {
                    if (!CanProgressObjective(obj, quest))
                        continue;

                    if (obj.CompletionInstance is IPollingConditionInstance pComp)
                        pComp.Refresh(_context, () => MarkDirty(quest, obj));

                    if (obj.FailInstance is IPollingConditionInstance pFail)
                        pFail.Refresh(_context, () => MarkDirty(quest, obj));
                }
            }
        }

        private void MarkDirty(QuestState quest, ObjectiveState obj)
        {
            _dirtyQueue.Enqueue((quest, obj));
        }

        private void ProcessDirtyQueue()
        {
            while (_dirtyQueue.Count > 0)
            {
                var (quest, obj) = _dirtyQueue.Dequeue();
                EvaluateObjectiveAndQuest(quest, obj);
            }
        }

        private void EvaluateObjectiveAndQuest(QuestState quest, ObjectiveState obj)
        {
            if (quest.Status is QuestStatus.Completed or QuestStatus.Failed)
                return;

            // Ensure objective can be active
            if (!CanProgressObjective(obj, quest))
                return;

            // Fail first
            if (obj.FailInstance != null && obj.FailInstance.IsMet)
            {
                obj.SetStatus(ObjectiveStatus.Failed);
                OnObjectiveStatusChanged?.Invoke(obj);

                quest.SetStatus(QuestStatus.Failed);
                OnQuestFailed?.Invoke(quest);

                UnbindQuestConditions(quest);
                _log.RemoveQuest(quest);
                return;
            }

            // Complete
            if (obj.CompletionInstance != null && obj.CompletionInstance.IsMet)
            {
                if (obj.Status == ObjectiveStatus.NotStarted)
                {
                    obj.SetStatus(ObjectiveStatus.InProgress);
                    OnObjectiveStatusChanged?.Invoke(obj);
                }

                obj.SetStatus(ObjectiveStatus.Completed);
                OnObjectiveStatusChanged?.Invoke(obj);
            }

            // Quest completion check
            if (quest.GetObjectiveStates().All(o =>
                    o.Definition.IsOptional || o.Status == ObjectiveStatus.Completed))
            {
                quest.SetStatus(QuestStatus.Completed);
                OnQuestCompleted?.Invoke(quest);

                UnbindQuestConditions(quest);
                _log.RemoveQuest(quest);
            }
        }

        private static bool CanProgressObjective(ObjectiveState obj, QuestState quest)
        {
            if (obj.Status is ObjectiveStatus.Completed or ObjectiveStatus.Failed)
                return false;

            var prereq = obj.Definition.Prerequisites;
            if (prereq == null || prereq.Count == 0)
                return true;

            foreach (var pre in prereq)
            {
                if (pre == null) continue;

                if (!quest.TryGetObjective(pre.ObjectiveId, out var preState))
                    continue; // or treat missing as incomplete?

                if (preState.Status != ObjectiveStatus.Completed)
                    return false;
            }

            return true;
        }
    }
}

8. QuestPlayerRef (building QuestContext)

File: QuestPlayerRef.cs
Namespace: GenericQuest.Core

using UnityEngine;

namespace GenericQuest.Core
{
    public class QuestPlayerRef : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour areaServiceProvider;      // implements IQuestAreaService
        [SerializeField] private MonoBehaviour inventoryServiceProvider; // implements IQuestInventoryService
        [SerializeField] private MonoBehaviour timeServiceProvider;      // implements IQuestTimeService

        public QuestContext BuildContext()
        {
            var area = areaServiceProvider as IQuestAreaService;
            var inv  = inventoryServiceProvider as IQuestInventoryService;
            var time = timeServiceProvider as IQuestTimeService;

            return new QuestContext(area, inv, time);
        }
    }
}


Game-side code will implement the services and wire them here.

9. Minimal Editor Support
9.1. QuestAssetEditor

Custom inspector that:

Shows quest ID, name, description.

Shows a reorderable list of objectives.

Allows "Add Objective" → creates ObjectiveAsset as a sub-asset of QuestAsset.

For each objective:

Display fields inline: objectiveId, title, isOptional, prerequisites, completionCondition, failCondition.

9.2. ConditionGroupAsset Editor

Custom inspector that:

Shows operator (AND/OR).

Shows list of child ConditionAsset references.

9.3. QuestDebuggerWindow (optional v0.1)

Basic window:

Lists active quests from the QuestManager (you can find it via FindObjectOfType<QuestManager>() in editor).

For each quest: show objective statuses and IsMet status of conditions if we can expose them.

Implementation details can be left to the dev, as long as it follows this intent.

10. Minimal Built-in Condition Examples (for dev to implement)

To prove the architecture, v0.1 should ship with 2–3 example conditions:

ItemCollectedCondition

Designer fields: itemId, requiredCount

Listens to an ItemCollectedEvent from the event system.

Increments internal counter; IsMet = currentCount >= requiredCount.

TimeElapsedCondition

Designer field: requiredSeconds

Subscribes to a GameTickEvent or uses IPollingConditionInstance.Refresh with QuestContext.TimeService.

IsMet becomes true once total elapsed time >= requiredSeconds.

CustomFlagCondition (optional)

Designer field: flagName

Listens to generic flag-changed events or queries a flag service via polling.

Each of these will be:

ItemCollectedConditionAsset : ConditionAsset with corresponding ItemCollectedConditionInstance.

Etc.

11. Testing

We want at least a couple of pure C# unit tests:

Use a FakeEventBus : IQuestEventBus.

Programmatically construct a QuestAsset with one objective + a simple condition, or mock the asset layer if needed.

Verify that:

Publishing the correct events marks the quest as completed.

Fail conditions trigger quest failure.

ConditionGroupAsset (AND/OR) behaves as expected.