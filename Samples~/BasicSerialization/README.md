# Basic Serialization Sample

Minimal example showing quest state snapshot capture and restoration.

## What It Shows

- Capturing a snapshot with `QuestStateManager.CaptureSnapshot()`
- Serializing to JSON with `JsonUtility.ToJson()`
- Deserializing from JSON with `JsonUtility.FromJson()`
- Restoring quest state with `QuestStateManager.RestoreFromSnapshot()`

## Usage

1. Assign a QuestAsset in the Inspector
2. Right-click component → "Capture Snapshot"
3. Right-click component → "Restore Snapshot"

**~30 lines of code** demonstrating the core API.
