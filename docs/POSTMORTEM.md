# Postmortem

FINAL PRESSURE was completed and publicly released as a focused Unity WebGL survival game. The most important result was not the number of systems built; it was turning a changing prototype into a coherent product that could be played, understood, and shipped through a real browser distribution flow.

## What Shipped

The released run combines auto-fire combat, resource collection, upgrade cards, optional specialist capture sites, support weapons, escalating horde pressure, and a final Commander encounter. It supports desktop and mobile browser play, including landscape fullscreen, touch movement, popup interaction, pause/resume, and focus-safe audio.

## Scope Decisions

The project accumulated more directions than one release needed. Finishing required treating scope reduction as product work rather than loss:

- the public objective was reduced to one complete survival run;
- unfinished narrative and meta-progression directions were removed from the release promise;
- weapon input was simplified around automatic behavior;
- five specialist sites were retained because they added tactical variety without changing the core loop;
- platform work concentrated on one WebGL artifact instead of parallel native release pipelines.

This made the final game easier to explain and left more time for clarity, mobile behavior, and release validation.

## What Worked

### Explicit runtime composition

A thin bootstrap, explicit dependency registration, plain C# services, and passive Unity views made integration problems visible. It also made lifecycle work—especially restart, focus, audio, and pooled cleanup—possible to reason about without relying on scene-wide global access.

### Performance as bounded work

Pooling alone was not enough. Dense combat became more predictable when navigation, targeting, effects, and frame-level queries were treated as budgets. Defensive reset paths on pooled objects reduced reuse bugs that otherwise appeared only late in a run.

### Readability passes close to release

External testing exposed issues that internal feature checks did not: players could miss where to stand at a capture site, confuse visually similar upgrade categories, or read small resource dots as background noise. Ground zones, state labels, distinct color/icon language, visible activation feedback, and a more object-like cash pickup improved comprehension without changing the game loop.

### One real publishing surface

Keeping the same itch.io page and replacing the active browser upload made the release process repeatable. Versioned artifacts, hashes, an inactive rollback, and a hosted smoke test were more valuable than creating additional distribution channels before the build was stable.

## Hard Problems

### WebGL sharpness versus mobile cost

Reducing device pixel ratio aggressively protected frame time but made HUD text and cards look soft. Using raw phone DPR without limits created the opposite risk. The final approach combines platform-aware DPR caps with a physical-pixel budget, preserving readability while bounding the framebuffer cost.

### Touch input through modal UI

A gameplay popup can appear while movement is held. Disabling the whole input layer interrupted the player; ignoring releases created stuck movement. Tracking the movement pointer independently, preserving it only for gameplay popups, and hard-resetting it for terminal/menu states resolved the conflict.

### Lifecycle bugs are multiplicative

Music that schedules twice after focus recovery, a pooled effect that keeps old state, or a stale touch pointer may look harmless in a single test. After several focus cycles or restarts, the symptoms compound. Explicit ownership and paired teardown became release requirements, not cleanup tasks.

## What I Would Do Earlier

- Establish the public one-sentence game loop before expanding content.
- Test capture-site comprehension with a new player as soon as the first site works.
- Measure effective WebGL framebuffer size on physical phones before UI polish.
- Treat pause, focus, restart, and terminal states as one lifecycle matrix from the start.
- Keep public controls generated from intended behavior, not legacy debug bindings.
- Maintain the release artifact record and rollback slot from the first external build.

## Outcome

The project reached a public `v0.1.0-rc.9` WebGL release with a complete playable flow and a documented publishing process. The strongest portfolio lesson is that completion depended on engineering and presentation together: architecture enabled safe iteration, but external readability, platform behavior, and disciplined release operations made the work usable as a product.
