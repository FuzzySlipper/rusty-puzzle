# Rusty Puzzle agent guidance

Rusty Puzzle is a turn-based, grid-based fantasy party puzzle game: each party
member obeys one distinctive movement law, and rooms are solved by choosing,
sequencing and combining those rules on a patient board with no twitch
pressure. Read [docs/game-design.md](docs/game-design.md) for product direction
and the first-prototype scope, and [docs/reuse.md](docs/reuse.md) before
borrowing sibling code. Den project `rusty-puzzle` owns tasks and progress.
Keep the product small and explicit; proposed game systems are not implemented
owners until the code and the architecture owner map say so.

> The product decides. The Engine guarantees.

## Start here

Read [README.md](README.md) for setup and commands and
[docs/architecture.md](docs/architecture.md) for the current owners. Before
changing the Engine boundary, read the Engine's
[C# SDK guide](https://github.com/FuzzySlipper/rusty-engine/blob/main/docs/csharp-sdk.md)
and architecture. `rusty --help` is the workflow reference. Ordinary builds
consume the pinned package; verify capabilities against that pin (its release
notes and API surface) rather than against Engine source at another revision.

The user request and owning task define scope and acceptance. If work is tied
to Den, resolve that project's live guidance, task, and dependencies. Report
failed reads; do not invent task state. Continue independently authorized work
and pause only decisions that need unavailable authority.

## Ownership and source

- `src/RustyPuzzle.Game/` owns board and party state, puzzle rules, room
  authoring interpretation, semantic input interpretation, and UI facts.
  Organize additions by product domain (board, party and movement laws, rooms,
  interface); keep the product entry focused on explicit composition and
  lifecycle. The board is deterministic: legal moves, consequences, win checks
  and undo are product decisions over admitted Engine input, never timers.
- `Rusty.Engine` owns named Engine mechanisms: lifecycle/update admission,
  input delivery, rendering/resources, spatial queries, content delivery,
  persistence primitives, and host integration. Search the safe SDK and
  existing product owners before adding a mechanism.
- `src/ui/` is a DOM companion. It observes Engine projections and submits
  semantic intents. Gameplay state, game rendering, canvas, transport, and
  scheduling stay with their C#/Engine owners.
- `content/` holds product-authored data: handcrafted rooms, party member
  definitions and their movement laws, tuning and interface text. Interpret it
  in typed C# through Engine content services. Keep authored definitions, live
  state, and transient presentation distinct. Gameplay prose and tuning values
  belong in content, not in C# or JS literals.
- The SDK generates the bind entry point and interop under ignored `obj/` output.
  Product code stays safe C#: no handwritten ABI/PInvoke, exports, raw native
  access, downstream Rust, or checked-in composition projects.

There is one Engine-admitted update path. Use its time/input facts; do not add
another loop, clock, scheduler, renderer, or state authority downstream.

## Product style

Prefer ordinary readable C#, explicit composition, direct methods, and one
clear mutable owner per domain. Keep operations thin: read, decide, apply,
publish. Use typed boundaries where they help; do not introduce a framework,
reflection discovery, generic bus, scripting language, or service locator for
hypothetical needs.

Use nullable types, file-scoped namespaces, and `internal`/`sealed` defaults
where the public product contract does not require otherwise. Keep structural
constants beside their algorithm; give meaningful identities names.

Trust first-party runtime state and Engine-admitted data. Preserve concrete
eligibility rules, current-data errors, and resource lifetime/disposal. Do not
add repeated hashing, compatibility layers, whole-state rollback, or validation
ceremony without a task-owned failure it prevents. Save meaningful values at
explicit save boundaries; native handles and presentation resources are not
product save state.

## Content and code organization

This game is content heavy and will be tuned constantly: characters, movement
laws, interactions, terrain, enemies, rooms and their introduction order all
change as the puzzle grammar is discovered. Organize ahead of need: agents and
people extend the shape they find, so an ad hoc catch-all becomes the pattern
every later change follows. Put an addition where its future siblings will
live. If the file, class or module you are extending is already a catch-all,
split it along domain lines first, as its own change. Organization means
folders, files, typed records and named owners, not a framework, generic
registry, rule engine or plugin system.

- **Authored data is a domain-organized tree.** `content/` has directories and
  files named for what they hold (party members, movement laws and
  interactions, terrain kinds, enemy behaviors, rooms, campaigns or room
  order, tuning, interface text). A product-named or catch-all file
  (`puzzle.json`, `data.json`, `config.json`) is a smell. Keep reusable
  definitions (a character, a terrain kind, an enemy kind) apart from where one
  room places them. Each domain loads and validates its own typed records; do
  not funnel all content through one record that every owner reads.
- **Gameplay rules are composed from typed parts, not written into classes.**
  A character is an authored composition of parts: a movement law, zero or more
  interaction rules, and presentation. A movement law is a typed record from a
  small closed vocabulary (step pattern, distance rule, path rule, blocking
  rule, target rule) that one owner evaluates; the ranger's "exactly two" and
  the fighter's "one orthogonal" are different values of the same records, not
  two classes with hand-written move methods. Interactions (swap, pull, push,
  teleport, change an ally's law) are typed effect records dispatched by the
  owning domain. Terrain kinds, enemy behaviors and room objectives follow the
  same shape: a definition composed of typed parts, with the behavior for each
  part written once. Adding a new character from existing parts is a content
  edit; adding a genuinely new part is one typed record plus one evaluator,
  never a `switch` on a name or a subclass per character.
- **Consequences are explicit typed effects.** A move resolves to a list of
  typed board effects (occupy, vacate, swap, displace, exit, modify a law) that
  one applier executes and the projection can preview; legal-move display,
  undo and tests all read the same effects. Do not mutate board state inside
  rule evaluation.
- **No gameplay prose in C# or JS.** Names, descriptions, rule sentences,
  prompts, refusal reasons and control labels belong in their domain's content
  as templates filled from definition values. Never repeat a definition's name
  or number inside a literal. Code composes text; it does not author it. A
  screen module's fixed chrome may live in its markup, and so may
  developer-console output.
- **Gameplay values are tuning.** Board sizes, distances, ranges, counts,
  highlight timings and limits go in content. A C# `const` is not tuning
  support. Missing or invalid authored values fail validation naming the file
  and field; no silent defaults.
- **Variants are typed.** A kind, mode or behavior is an enum or typed record in
  its definition, dispatched by the owning domain. Do not compare strings in
  rules, views or projections.
- **One table per vocabulary.** Key bindings and labels, intent actions, cell
  and piece kinds, projected facts and saved fields each have one declaration
  that every consumer (C# input, rules, projection, DOM, tests, playtest
  module) reads. A feature should not be added in several places that must be
  kept in step by hand.
- **Size signals prompt a split.** A C# owner past roughly 300 lines, a method
  interleaving several domains, a JS module holding more than one screen, or a
  JSON file spanning several domains is the cue to divide before extending.
  These are prompts to look, not gates.

Resist the opposite failure too. The closed vocabularies stay small and
concrete, grown one typed part at a time by a real room that needs it; this is
not a general rules engine, expression language or data-driven everything.

## Engine dependencies and gaps

`Directory.Build.props` owns the exact SDK/runtime pin. Install it with
`rusty install`; deliberately advance it with `rusty update`, read the release
notes it lists, then run the focused checks. `rusty status` reports the pin,
installation and missing prerequisites. Keep exact
versions in executable configuration and evidence, not duplicated in prose.
Normal development uses the matched runtime pack through `rusty dev`.
NativeAOT is an explicit fidelity/release check. Do not make an adjacent
Engine checkout a build dependency or modify it as part of downstream work.

The product builds and runs on Windows and Linux. Anything `rusty dev` or
`rusty build` runs (a UI build command, an `Exec`, a step the README puts
before `rusty dev`) must work under both `cmd.exe` and `/bin/sh`: one
`npm`/`pnpm`/`node` invocation with quoted paths, MSBuild `Copy`/`MakeDir`
for files, no `bash`, shell utilities or absolute machine paths. Generated
output stays under `obj/` or an ignored directory. Linux-only tooling is named
as such and kept off that path.

If a required mechanism is missing, verify the safe API, name the blocked
behavior and upstream owner, and file/link one narrow Engine request when
that is authorized. Distinguish a missing mechanism or binding from a helper
or documentation gap. Stop that dependent slice; continue independent work.
Do not conceal the gap with a local substitute, fake success, or proof-only path.

## Durable documentation

Repository Markdown is for settled, permanently useful information: product
intent, implemented ownership, contracts, authoring recipes, repeatable commands
and incorporated code provenance. Den owns plans, task status, handoffs,
reviews, investigations, playtest evidence and dated measurements. Do not add
a session diary, roadmap, "current state" or "remaining work" section, Den task
IDs, or TODO comments that carry a plan to repository docs; point from Den to
the repo, not back. Design may state intended direction; whether a part exists
is visible from the code and the architecture owner map. When a change settles
a durable rule, update its owning document in the same work.

## Review and evidence

Use [docs/agent-review/README.md](docs/agent-review/README.md). Every change gets
an Engine-reuse and existing-product-reuse check; trivial changes may record
that no mechanism is affected. Assign bounded independent lanes when review
agents are requested or the task's review workflow calls for them. Keep the
same reviewer for fix rounds and reconcile source-backed findings against the
original task. Review is not an extra user-approval gate.

`rusty build` builds the default project `Directory.Build.props` names
and stages the ordinary CoreCLR product; `--aot` additionally publishes NativeAOT. Use focused
semantic or interaction evidence only when it answers the changed behavior;
do not add broad test gates to this small product. Distinguish build/staging,
host launch, and visible interaction claims. Repeat passed checks only after
material changes or an unresolved failure.

Preserve unrelated edits. Keep generated output and installed artifacts
ignored. Do not reset, force-push, or change adjacent repositories. Report what
changed, relevant checks, and concrete limitations. Commit/push when requested
or authorized by the active task; a review packet does not authorize publishing.
