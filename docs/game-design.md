---
title: The Road That Was Taken — Fantasy Party Puzzle Game
aliases:
  - Fantasy Party Puzzle Game
  - Party Movement Puzzle
created: 2026-10-06
tags:
  - game-design
  - puzzle-game
  - fantasy
  - prototype
source-conversation: Game Design Exploration
---

# The Road That Was Taken — Fantasy Party Puzzle Game

> A working design note preserving the current conversation: the core idea, opinions, aesthetic direction, opportunities, risks, prototype scope, and unresolved decisions. Checkboxes are intentionally left open unless the conversation established a preference.

## One-sentence concept

A turn-based, grid-based fantasy puzzle adventure where each party member obeys a distinctive movement law, and the player solves rooms by choosing, sequencing, and combining those rules.

The board is patient: no reflex test, no rush to move. The tension comes from spatial reasoning and meaningful consequences. The central question grows from **“How do I reach the goal?”** into **“Whose rules make this board solvable, and which characters should I bring?”**

## The conversation’s strongest design opinion

The promising mutation is to move the complexity away from *Road Not Taken*’s object behaviors and make **the party itself the puzzle language**.

Characters are not simply keys for matching locks. Their movement restrictions and interactions should combine into surprising solutions. Over time, the player should stop seeing a set of isolated characters and start seeing a small, understandable machine made of rules.

The special character-recruitment reward could be:

> **“WAIT. THEY MOVE LIKE THAT?!”**

A new character is exciting because they add a new way to think, not just a new name on the roster.

## Inspiration, distinction, and creative boundaries

### What is being carried forward as inspiration

- Deliberate, turn-based spatial reasoning.
- A board the player can study without time pressure.
- Deterministic consequences that make each action matter.
- Simple player actions interacting with richer systemic rules.
- The pleasure of learning a puzzle grammar, then combining it.

### Where this game should establish its own identity

- Make party members’ movement laws the central toy.
- Make party sequencing, positioning, and composition central to solving rooms.
- Use a broadly readable fantasy-adventure frame, distinct from *Road Not Taken*’s sweet, lovely, sad woodland-fable feeling.
- Avoid making levitating, carrying, throwing, and combining environmental objects the central mechanic; that is a distinctive part of *Road Not Taken*’s design fingerprint.
- Do not copy its characters, writing, art, music, code, distinctive presentation, terminology, or exact puzzle content.

The earlier conversation’s ethical/IP takeaway was that a broad idea or method of play is different from copying a specific creative expression. It also flagged patents and trademarks as separate questions for a future commercial release. This is a summary of that conversation, not a legal clearance; seek qualified IP advice if the project approaches publication.

The discussion also placed this idea in a wider design family, mentioning *Dungeon Solver* and *Alice in Disguise* as examples of neighboring puzzle spaces. Their mention was meant to suggest a design lineage rather than a reason to abandon the idea. The source links for those examples were not retained in this note.

## Core design ideas proposed

### Game loop and structure

- Turn-based, grid-based puzzle rooms with no twitch pressure.
- Select one party member, see their legal moves, then act.
- Introduce characters gradually so each arrival expands the player’s movement vocabulary.
- Start with handcrafted rooms; consider procedural generation only after the puzzle system is understood.
- Make experimentation cheap with instant restart or rewind.
- Be cautious with stamina or move costs if they punish players for testing ideas.
- A possible compromise: ordinary movement is free while special abilities use a room-based resource, or the resource resets in each room.

### Character movement as personality

Movement laws can express class fantasy, temperament, history, or relationships. Initial examples:

- **Reckless fighter:** cannot move backward.
- **Protector:** must finish movement beside an ally.
- **Loner:** gains extra movement when separated.
- **Twins:** can swap positions.
- **Ghost:** passes through walls.
- **Scholar:** changes another character’s movement rule.
- **Exact mover:** moves exactly two spaces.
- **Diagonal mover:** moves only diagonally.
- **Unstoppable mover:** travels until hitting an obstacle.
- **Ally-seeker:** teleports to a party member.
- **Magnetic mover:** pulls another party member when moving.

Further sample personalities from the conversation:

- A painfully polite former archivist barbarian who moves in unstoppable straight lines because, once she commits, she *commits*.
- An irritable battlefield-surgeon cleric whose movement prevents allies from separating too far.
- A noble-born cowardly rogue whose mechanic revolves around moving behind other characters.

Use familiar archetypes to make mechanics legible, then make each individual character specific and memorable. “Paladin lady, rogue guy, wizard elf” is not enough characterization.

### Interaction and puzzle philosophy

- Make party members interact rather than assigning each a matching obstacle.
- Avoid “red wizard opens red magic door” key-lock puzzles as the default pattern.
- Build toward puzzles where one character’s position or action changes another character’s legal moves.
- Let later rooms require action chains across several characters.
- Explore synergies, friction, pushes, pulls, swaps, carrying, throwing, teleportation, and characters acting as obstacles—but keep each rule understandable.
- Relationships may eventually alter mechanics. A pair could begin unable to end a turn adjacent, then reconcile and gain a joint ability when adjacent. Character development would then change the geometry of the board.
- Ask whether interactions produce emergent combinations, rather than merely assigning each character a separate job.

### Interface and readability

The board should “speak” the movement rules:

- On selection, show legal destinations directly on the board.
- Use glowing tiles, ghost footprints, projected paths, or arrows.
- Show full trajectories for rules such as “continue until blocked.”
- Preview secondary consequences, including pulls, swaps, pushes, and teleports.
- Make projected positions visible when one move affects another character.
- Avoid requiring players to memorize a pile of dense tooltips. The player should be able to see the movement grammar.

### Room goals and world elements

Possible room objectives include reaching an exit, retrieving an object, defeating or avoiding a creature, escorting someone, activating locations, forming a pattern, or solving an environmental interaction. “Everyone must escape” was recommended for an early prototype because it makes party positioning inherently important.

Possible terrain and objects include walls, pits, pressure plates, doors, moving platforms, ice, water, lava, portals, traps, crates, statues, monsters, magical fields, rotating rooms, and one-way passages. Introduce these gradually; do not debut several new character, enemy, terrain, and status rules at once.

## Aesthetic direction

Established preferences:

- [x] Fantasy.
- [x] A party-adventure frame.
- [x] D&D-adjacent in feel.
- [x] Broad appeal.
- [x] Distinct from *Road Not Taken*’s aesthetic.
- [x] Not aggressively artsy.

Current opinion: **stylized fantasy rather than gritty fantasy** is likely a good starting direction—immediately readable, warm enough for broad appeal, with clear swords, ruins, spell effects, monsters, and treasure. The goal is “ADVENTURE,” not bleakness or visual obscurity.

Still open: top-down vs. isometric, 2D vs. 2.5D, pixel art vs. illustration vs. stylized 3D, cute vs. heroic proportions, and bright heroic fantasy vs. darker dungeon fantasy.

## First prototype: prove the nucleus

Make an aggressively plain, functional prototype. Gray squares and lettered circles are fine; the board is the experiment.

### Proposed scope

- An approximately 8×8 board.
- Four characters.
- One simple objective: get **all characters** to an exit.
- Around fifteen handcrafted puzzles.
- No lore, inventory, town, crafting, romance system, skill trees, equipment, or procedural generation.
- No conventional combat system yet.
- Instant reset/rewind; do not punish experimentation.

| Prototype character | One movement rule |
|---|---|
| Fighter | Moves one orthogonal square |
| Ranger | Moves exactly two squares |
| Rogue | Moves diagonally |
| Mage | Swaps positions with an ally within three tiles |

Test this question first:

> Can I create situations where the solution requires characters to use one another’s movement restrictions, instead of each character doing an independent job?

If that becomes fun, the core is worth developing. Then test the more distinctive extension:

> Can one character’s movement change another character’s legal moves?

### Prototype checklist

- [ ] Build an approximately 8×8 grid.
- [ ] Implement the four sample movement rules.
- [ ] Make legal movement visible on selection.
- [ ] Add one shared exit condition that requires the whole party to escape.
- [ ] Make a set of roughly fifteen handcrafted rooms.
- [ ] Add instant reset or rewind.
- [ ] Check whether characters need one another’s movement restrictions.
- [ ] Identify where movement rules combine in surprising ways.
- [ ] Try one interaction that changes another character’s legal moves.
- [ ] Keep notes on confusion, dead ends, and puzzles that feel like simple keys.
- [ ] Decide whether the prototype has a satisfying nucleus before adding narrative or progression.

## The Big Design Checklist

### 1. Define the core fantasy

- [ ] What is the player doing fictionally?
- [ ] Are they exploring dungeons?
- [ ] Escaping ruins?
- [ ] Delving for treasure?
- [ ] Rescuing people?
- [ ] Hunting monsters?
- [ ] Completing adventurer-guild contracts?
- [ ] Is this a continuing party or a roster assembled mission by mission?
- [ ] What makes the player want to enter the next puzzle besides “because puzzle”?

Useful sentence to complete:

> “You are a band of adventurers who ______.”

### 2. Define what winning a room means

- [ ] Reach an exit?
- [ ] Get the whole party to the exit?
- [ ] Retrieve an object?
- [ ] Defeat a creature?
- [ ] Escort someone?
- [ ] Activate several locations?
- [ ] Arrange characters into a formation?
- [ ] Solve environmental interactions?
- [ ] Can objectives vary?

**Current prototype preference:** everyone must escape.

### 3. Decide what a “turn” means

- [ ] Does one character move per turn?
- [ ] Does switching characters cost anything?
- [ ] Do enemies or the environment move after every action?
- [ ] Can the player undo one move?
- [ ] Can they rewind indefinitely?
- [ ] Does a room have a move limit?
- [ ] Does anything punish experimentation?

**Current preference:** no punishment for experimentation. This is a thinking game; avoid making players pay a scarce resource just to test a hypothesis.

### 4. Define each character’s movement grammar

For every playable character:

- [ ] How do they move?
- [ ] What can they not do?
- [ ] What happens when they move?
- [ ] What happens when they touch another character?
- [ ] What happens near terrain?
- [ ] What makes their movement feel like *them*?
- [ ] Can another character modify their rules?
- [ ] Can the rule be explained in one sentence?

If the rule needs a paragraph, it may be mechanically overcooked.

### 5. Decide how party composition works

- [ ] How many characters exist?
- [ ] How many can enter a dungeon?
- [ ] Can the player change the party between rooms?
- [ ] Only between dungeons?
- [ ] At camps?
- [ ] Can every puzzle be solved by multiple party combinations?
- [ ] Are some puzzles deliberately built around specific characters?
- [ ] Can the player get trapped because they chose the “wrong” party?

Watch for impossible-to-test party combinations and unsolvable rooms. A player should not reach a puzzle only to discover that their selected party cannot solve it.

### 6. Decide how class relates to movement

Familiar archetypes can make rules quickly legible: fighter, rogue, wizard, cleric, ranger, barbarian, bard, paladin, warlock, monk, druid.

- [ ] Are these actual classes or loose inspirations?
- [ ] Does each class imply a movement family?
- [ ] Are individual characters variations within that family?
- [ ] Does equipment change movement?
- [ ] Do characters level up?

**Current preference:** avoid traditional stat-heavy RPG leveling at first. The board is the real character sheet.

### 7. Determine how combat works, if at all

- [ ] Are enemies essentially moving puzzle pieces?
- [ ] Can characters attack?
- [ ] Does entering an enemy tile defeat it?
- [ ] Does facing matter?
- [ ] Does damage persist?
- [ ] Is combat another spatial rule?
- [ ] Could monsters be obstacles rather than conventional enemies?

**Current preference:** combat as puzzle behavior, not RPG damage arithmetic. Example: an ogre advances one tile toward the nearest hero after every player action. That is readable board pressure without a separate combat spreadsheet.

### 8. Design character interactions

- [ ] Which characters improve one another?
- [ ] Which interfere with one another?
- [ ] Can someone carry or throw someone else?
- [ ] Can someone teleport another character?
- [ ] Can one character become an obstacle for another?
- [ ] Can two movement rules combine into a third effect?
- [ ] Can relationships unlock mechanical synergies?

Relationship-driven mechanics are a particularly promising later experiment: estrangement could constrain adjacency, while reconciliation could enable a shared move.

### 9. Establish environmental vocabulary

Candidate vocabulary:

- [ ] Walls
- [ ] Pits
- [ ] Pressure plates
- [ ] Doors
- [ ] Moving platforms
- [ ] Ice or sliding tiles
- [ ] Water
- [ ] Lava
- [ ] Portals
- [ ] Traps
- [ ] Crates
- [ ] Statues
- [ ] Monsters
- [ ] Magical fields
- [ ] Rotating rooms
- [ ] One-way passages

Introduce these slowly. Avoid combining several unfamiliar systems in the same first encounter.

### 10. Decide how much randomness exists

- [ ] Are puzzle layouts handcrafted?
- [ ] Procedural?
- [ ] Hybrid?
- [ ] Are enemy behaviors deterministic?
- [ ] Is loot random?
- [ ] Can randomness make a puzzle unwinnable?

**Current recommendation:** deterministic puzzles and handcrafted rooms, with randomized rewards only if useful. Players should lose because their solution was wrong, not because a random roll invalidated the plan.

### 11. Decide how narrative enters the puzzle loop

- [ ] Dialogue before rooms?
- [ ] Party banter inside rooms?
- [ ] Camp scenes afterward?
- [ ] Character recruitment quests?
- [ ] Environmental storytelling?
- [ ] Short visual-novel-style conversations?
- [ ] Relationships changing mechanics?
- [ ] Party members commenting on how the player solves things?

Character voice and party banter could make the fantasy emotionally sticky, while mechanical relationship changes can tie story directly to the puzzle system.

### 12. Establish visual identity

Already established:

- [x] Fantasy.
- [x] Party adventure.
- [x] D&D-adjacent feel.
- [x] Broadly appealing.
- [x] Visually distinct from *Road Not Taken*.
- [x] Not aggressively artsy.

Still to decide:

- [ ] Top-down?
- [ ] Isometric?
- [ ] 2D?
- [ ] 2.5D?
- [ ] Pixel art?
- [ ] Illustrated?
- [ ] Stylized 3D?
- [ ] Cute proportions or more heroic proportions?
- [ ] Bright heroic fantasy or darker dungeon fantasy?
- [ ] How readable must characters remain when viewed on a grid?

**Current opinion:** stylized fantasy is a promising middle ground between warmth/readability and a clear sense of adventure.

## Threats to keep watching

- [ ] **Key-lock syndrome:** characters become keys for matching obstacles rather than interacting systems.
- [ ] **Rules overload:** too many mechanics arrive simultaneously.
- [ ] **Combinatorial explosion:** party combinations outgrow the ability to design and test them.
- [ ] **Softlocks:** players can irreversibly make a room impossible; provide generous reset/rewind.
- [ ] **Wrong-party traps:** players reach an unsolvable room because they selected the wrong roster.
- [ ] **RPG sprawl:** stats, equipment, leveling, inventory, and side systems bury the puzzle game.
- [ ] **Genre-bolted combat:** combat becomes a separate game layered on top of spatial puzzles.
- [ ] **Punished experimentation:** resource costs make trying ideas stressful.
- [ ] **Unmemorable movement rules:** characters need paragraphs of explanation.
- [ ] **Environmental gimmicks take over:** terrain systems overshadow the party.
- [ ] **Too-close imitation:** presentation or object manipulation follows *Road Not Taken* when this concept has a stronger differentiator.
- [ ] **Generic archetypes:** familiar fantasy classes make characters feel like stock roles. Use archetypes for readability, then give each person a specific personality and mechanic.

## Next design exercise

Do this before designing the larger story:

- [ ] Invent six party members.
- [ ] Give each one a fantasy archetype.
- [ ] Give each one a one-sentence personality.
- [ ] Give each one exactly one core movement rule.
- [ ] Give each one exactly one interaction rule.
- [ ] Identify two characters who naturally synergize.
- [ ] Identify two characters whose mechanics create friction.
- [ ] Design three two-character puzzles.
- [ ] Design three three-character puzzles.
- [ ] Look for emergent mechanics that were not intentionally designed.
- [ ] Only then decide what larger game structure is needed.

The test is simple: **if these six odd little adventurers are fun to move around an empty grid, there is a game here.** Towns, dragons, dialogue, loot, and orchestral horns can wait their turn.

## Reference and provenance

- Source conversation: **Game Design Exploration**, continued on 2026-10-06.
- The user’s referenced game: [*Road Not Taken* on Steam](https://store.steampowered.com/app/293740/Road_Not_Taken/).
- This note consolidates the conversation’s ideas and opinions; it is a living design document, not a transcript or a legal opinion.
