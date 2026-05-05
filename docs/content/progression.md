# Progression — Tiers, Rewards, and the Mood Matrix

> Authored content spec for issue #26.
> Source of truth for tier definitions, reward unlocks, and the stat→mood
> mapping that downstream systems (`OfflineBrain`, dialogue selection, visual
> state) should consume. This document is content; Sol owns the schemas and
> code that read it.

---

## 1. Design intent

DesktopPal's pal — by default, a round blue bear named **Buddy** — should
feel like it grows up *with* you. Progression here is not a grind. It is a
slow-burn relationship arc: an egg that hatches into a curious cub, becomes
a chatty companion, and eventually settles into a long-term roommate who
remembers the small shape of your day.

Three rules govern every tier:

1. **No tier punishes the user.** Skipping a day should not roll back a tier.
   Decay affects mood, not progression.
2. **Every tier unlocks something authored.** A new visual beat, a new
   interaction line, a new letter archetype, or a new world object. Never a
   stat bar widening for the sake of widening.
3. **Tiers are time-gated *and* care-gated.** The pal advances when both a
   minimum elapsed lifetime AND a minimum care threshold are met. Either one
   alone is not enough.

---

## 2. Tier ladder

Lifetime is measured from `BirthTime` and is real-world wall clock (the
existing offline-decay system already handles this). "Care score" is a
rolling average of the four stats over the lifetime of the pal — Sol can
define the exact formula; for content purposes, treat it as
`(Hunger + Hygiene + Happiness + Energy) / 4` averaged across recent days.

| Tier | Name              | Level | Min lifetime | Min care | Tone of this tier                                    |
| ---- | ----------------- | ----- | ------------ | -------- | ---------------------------------------------------- |
| 0    | Egg               | 0     | 0            | —        | Quiet, anticipatory, soft hums and shuffles.         |
| 1    | Hatchling         | 1     | 5 min        | 30       | Wide-eyed, easily startled, every word feels new.    |
| 2    | Cub               | 2     | 1 day        | 40       | Bouncy, asks lots of questions, easily delighted.    |
| 3    | Bright Cub        | 3     | 3 days       | 50       | More confident, starts naming things on the desktop. |
| 4    | Companion         | 4     | 7 days       | 55       | Settled, warm, begins remembering preferences.       |
| 5    | Steady Companion  | 5     | 14 days      | 60       | Calm, droll, gentle observations about your habits.  |
| 6    | Old Friend        | 6     | 30 days      | 65       | Reflective, occasionally nostalgic, very fond.       |
| 7    | Hearthbear        | 7     | 90 days      | 70       | Quiet authority, soft humor, deeply rooted.          |

> **Naming note for human review:** "Hearthbear" is a working title for the
> top tier and may want a less twee alternative. Flagged.

### Tier-up moment

When a pal advances a tier, three things happen on the same frame:

- a one-time **"tier-up" line** plays in the chat bubble (see `letter-beats.md`
  → *milestone* archetype),
- a one-time **letter** is written to the desktop announcing the change,
- the **visual unlock** for that tier becomes available.

The pal never mentions the tier number aloud. It says things like *"I feel
taller today"* or *"Something about the light is different"* — never *"I
reached level 4."*

---

## 3. Per-tier rewards

Each tier unlocks three lanes of content: **visual**, **interaction**, and
**dialogue / letter**. All copy below is authored content; mechanics are
Sol's call.

### Tier 0 — Egg
- **Visual:** speckled blue egg, faint shiver every ~30 s, occasional soft
  glow when hovered.
- **Interaction:** click to nudge; cannot drag yet. Hover tooltip: *"Almost…"*
- **Dialogue:** no spoken lines. Letter file is silent. The egg does not
  accept letters yet — any `.txt` dropped on the desktop while in this tier
  is queued for the Hatchling reply.

### Tier 1 — Hatchling
- **Visual:** small blue cub, oversized head, slightly unsteady wander
  pattern. Half-shell decoration optionally remains as a static world object.
- **Interaction:** drag enabled. Petting enabled. Calling enabled. Feeding
  enabled but the pal eats slowly.
- **Dialogue:** unlocks *Idle*, *Hungry*, *Petting*, *Calling*, plus the
  *Curious* category in a reduced subset (first 5 phrases). Vocabulary is
  short — average 6 words per line, max 12.
- **Letter beats:** *greeting*, *thank-you* archetypes only.
- **Milestone letter:** *"hello-from-the-shell"* — pal writes its first
  short letter the day after hatching.

### Tier 2 — Cub
- **Visual:** slightly more solid build, brighter blue, blinkier eyes,
  occasional small hop in the wander cycle.
- **Interaction:** can plant the first decoration (a single small flower).
  Can be called between monitors.
- **Dialogue:** full *Idle*, *Hungry*, *Petting*, *Calling*; full *Curious*
  unlocked; *Excited* category unlocked.
- **Letter beats:** *encouragement* archetype unlocked.
- **Milestone letter:** *"the-day-i-named-the-icons"* — pal writes a letter
  describing the desktop in cub-language ("the big blue square", "the
  rectangle with the words").

### Tier 3 — Bright Cub
- **Visual:** small accent — a soft tuft of darker blue fur on the head; eyes
  gain a faint shine.
- **Interaction:** unlocks the *little gift* gesture — pal occasionally
  drops a tiny world object (acorn, button, paperclip drawing) near the
  cursor when Happiness is high.
- **Dialogue:** *Sleepy* category unlocked; *Encouraging* category unlocked
  in reduced form.
- **Letter beats:** *playful*, *observation* archetypes unlocked.
- **Milestone letter:** *"things-i-have-noticed"* — three short observations
  about the desktop's recent contents.

### Tier 4 — Companion
- **Visual:** a small soft scarf accessory becomes available (toggleable in
  settings). Wander cycle gains an idle sit-and-look-up animation.
- **Interaction:** can plant trees as well as flowers. Cleanup interactions
  feel more deliberate (slower, with a small satisfied animation).
- **Dialogue:** full *Encouraging* category unlocked. Pal begins using the
  user's name (from settings) in roughly 1 in 8 lines.
- **Letter beats:** *advice-deflect*, *check-in* archetypes unlocked.
- **Milestone letter:** *"a-week-with-you"* — gentle, slightly longer
  letter, first time the pal explicitly says it is glad to be here.

### Tier 5 — Steady Companion
- **Visual:** subtle ambient particle when sitting still for >2 minutes
  (small drifting dust motes, very faint).
- **Interaction:** pal can rearrange its own decorations once per day.
  Cleanup cooperation: when the user clears desktop clutter, the pal
  performs a small celebratory hop.
- **Dialogue:** the pal starts referencing *yesterday* and *the other day* —
  light continuity language even without true memory. Sleepy category
  expands.
- **Letter beats:** *quiet-evening* archetype unlocked.
- **Milestone letter:** *"two-weeks-of-this"* — the pal lists three small
  things it has come to like.

### Tier 6 — Old Friend
- **Visual:** soft blue glow at dusk hours (system clock 18:00–22:00).
  Optional reading-glasses accessory.
- **Interaction:** the pal can leave a *bookmark* — a tiny note pinned to
  the desktop world referencing a recent letter exchange.
- **Dialogue:** vocabulary expands to include reflective phrases. Hungry
  category becomes more polite, less urgent.
- **Letter beats:** *reflection*, *fondness* archetypes unlocked.
- **Milestone letter:** *"a-month-in"* — longest letter to date,
  ~150 words, written in a calm voice.

### Tier 7 — Hearthbear
- **Visual:** unlocks a small "home spot" — a permanent cozy corner on the
  desktop where the pal returns to nap. Faintly different palette at night.
- **Interaction:** the pal sometimes initiates letters unprompted (one per
  week max). Pal has a permanent sleeping animation when Energy is low.
- **Dialogue:** all categories full. The pal's lines run slightly longer
  on average and use softer humor.
- **Letter beats:** all archetypes unlocked, including *milestone-anniversary*.
- **Milestone letter:** *"three-months"* — the pal writes about time itself,
  briefly and warmly. Never maudlin.

---

## 4. Stat-based mood matrix

Mood is a derived runtime state, not a stored stat. The matrix below maps
the four core stats (each 0–100) to mood archetypes that drive **dialogue
category selection**, **wander animation**, and **letter tone**.

### 4.1 Per-stat bands

| Stat      | Critical (0–20) | Low (21–45) | Okay (46–70) | Good (71–90) | Great (91–100) |
| --------- | --------------- | ----------- | ------------ | ------------ | -------------- |
| Hunger    | Famished        | Peckish     | Settled      | Full         | Stuffed        |
| Hygiene   | Grubby          | Dusty       | Tidy         | Fresh        | Sparkling      |
| Happiness | Glum            | Quiet       | Content      | Cheery       | Beaming        |
| Energy    | Drowsy          | Sleepy      | Awake        | Lively       | Bouncy         |

Each band biases which phrase categories are eligible. Critical bands
**force** their related category to dominate (e.g., Famished forces a
*Hungry* line within the next 60 s of idle dialogue).

### 4.2 Mood archetypes (composite)

The composite mood is the dominant flavor of the four bands together. Twelve
archetypes cover the practical space:

| Mood archetype | Dominant signal                               | Voice cue                                  | Eligible categories                          |
| -------------- | --------------------------------------------- | ------------------------------------------ | -------------------------------------------- |
| Sunny          | All stats Good or Great                       | Bright, light, slightly bouncy             | Idle, Excited, Encouraging, Curious          |
| Cozy           | Energy Okay/Good, others Okay+                | Warm, soft, settled                        | Idle, Curious, Encouraging                   |
| Curious        | Happiness Good+, Energy Lively+               | Asks questions, names things               | Curious, Idle, Excited                       |
| Sleepy         | Energy Low or Drowsy, others Okay+            | Slow, drifty, half-words                   | Sleepy, Idle                                 |
| Hungry         | Hunger Low or Critical                        | Plaintive but not grumpy                   | Hungry (forced), short Idle                  |
| Grubby         | Hygiene Low or Critical                       | Mildly embarrassed, apologetic             | A subset of Idle flagged as `hygiene-aware`  |
| Glum           | Happiness Low, others Okay                    | Quiet, fewer words, no exclamation         | A muted subset of Idle, Encouraging (rare)   |
| Restless       | Energy Lively+, Happiness Okay-               | Antsy, asks to play                        | Excited, Curious                             |
| Tender         | Recently petted, Happiness Good+              | Affectionate, low-key                      | Petting, Idle (warm subset)                  |
| Helpful        | Stats balanced, recent letter sent            | Encouraging, reassuring                    | Encouraging, Idle                            |
| Wistful        | Tier 5+, late-hour clock, Energy Low          | Reflective, slightly quieter               | Sleepy, a `reflective` subset of Idle        |
| Worn           | Two or more stats Critical                    | Subdued, asks for help directly            | Hungry / hygiene-aware Idle, no Excited      |

Dialogue selection should pick the archetype each tick (or each idle event)
and draw a line from the eligible categories. If no archetype matches
cleanly, default to **Cozy**.

### 4.3 Forcing rules

- Any stat at Critical → the related category is forced within 60 seconds.
- Two stats Critical → mood becomes **Worn** until at least one is back to
  Okay; *Excited* and *Bouncy* lines are suppressed.
- All stats Great for >10 minutes → mood becomes **Sunny** and unlocks
  the most expressive subset of *Excited*.
- Sleep window (system clock 23:00–06:00) and Energy ≤ Sleepy → **Sleepy**
  takes precedence over everything except Critical Hunger.

---

## 5. What this document does NOT define

- Exact decay rates, formulas, or save-file fields. (Sol.)
- The schema that maps tiers to assets and code paths. (Sol.)
- The wiki-facing player explanation of progression. (Jesse, after Sol
  confirms shipped behavior.)
- Specific letter copy beyond archetype names — see `letter-beats.md`.

— Vex
