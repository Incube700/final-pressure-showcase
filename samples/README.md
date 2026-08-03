# Selected C# Samples

This folder contains five curated examples from the private FINAL PRESSURE production codebase. They are included to demonstrate engineering decisions without publishing the Unity project, licensed content, build assets, or full gameplay implementation.

| File | Focus |
|---|---|
| [`DIContainer.cs`](selected-csharp/DIContainer.cs) | Explicit registration, resolution, circular-dependency protection, and lifecycle cleanup |
| [`TouchMovementInputReader.cs`](selected-csharp/TouchMovementInputReader.cs) | Movement pointer ownership across modal UI and safe reset behavior |
| [`GameplayMusicService.cs`](selected-csharp/GameplayMusicService.cs) | Run-phase music transitions and deterministic pause/end/dispose flow |
| [`CapturePointUpdateExcerpt.cs`](selected-csharp/CapturePointUpdateExcerpt.cs) | One update slice of the capture-site state machine |
| [`RuntimeGameObjectPoolExcerpt.cs`](selected-csharp/RuntimeGameObjectPoolExcerpt.cs) | Defensive pooled spawn/release behavior for dense gameplay |

The `Excerpt` files deliberately omit surrounding fields, helper methods, instrumentation, and project-specific types. They are readable portfolio samples, not standalone compilable packages.

Copyright © 2026 Siarhei Ivanou / BurnHeartGames. All rights reserved. See the repository [license](../LICENSE.md).
