# Achievements — Quiet Milestones

> Authored content groundwork for the progression layer described in
> `progression.md`. Twelve small milestones the pal notices about the two
> of you, written in the bear's voice. These are not "trophies." The pal
> does not unlock badges. It writes you a small line and remembers.

---

## 1. Design intent

Achievements in DesktopPal are **observed, not earned.** The pal is the
one who notices that something has happened — a first hatch, a clean
streak, a week of company. The user is never shown a stat readout or a
percentage bar. They are shown a small line and, sometimes, a small
visible change.

Three rules:

1. **No numbers in the user-facing text.** "A whole week" is allowed.
   *"7 days"* is not. *"Level 4 reached"* is forbidden.
2. **Every reward is authored content.** A line, a letter, a tiny visual
   beat, a new dialogue snippet — never a stat boost, never a multiplier.
3. **The pal never claims credit and never demands acknowledgment.** It
   just mentions the thing once, fondly, and moves on.

---

## 2. The twelve (original set)

Each entry below maps to a content row Sol can wire into the milestone
system. `id` is kebab-case and stable; `condition` is plain-language for
content review (Sol translates to code); `reward` is what the user sees;
`flavor` is the line the pal speaks or writes when the milestone fires;
`bubble` is the short (≤80 char) form used by the chat surface.

---

### 2.1 `first-hatch`
- **Name:** The First Morning
- **Condition:** Pal advances from Egg (Tier 0) to Hatchling (Tier 1).
- **Reward:** A short letter on the desktop titled *"hello-from-the-shell"*
  (already specced in `progression.md`). The half-shell remains as a
  permanent tiny world object in the pal's home corner.
- **Flavor:** *"I think I just opened my eyes for the first time. The
  light is very kind."*
- **Bubble:** *"Oh — light. Hello, light."*

### 2.2 `first-feeding`
- **Name:** A Soft First Meal
- **Condition:** First successful feed interaction after hatching.
- **Reward:** Unlocks a single new Idle line — *"That was a good one."* —
  which becomes eligible whenever Hunger is Full or Stuffed.
- **Flavor:** *"Oh — that's what eating is. I think I like it."*
- **Bubble:** *"Oh. I think I like eating."*

### 2.3 `first-petting`
- **Name:** Held for a Moment
- **Condition:** First time the pet interaction is held to completion.
- **Reward:** Pal's idle wander gains a small "look toward cursor" beat
  for 24 in-app hours after the milestone. No further unlock.
- **Flavor:** *"Your hand is warm. I noticed."*
- **Bubble:** *"Your hand is warm. I noticed."*

### 2.4 `first-letter`
- **Name:** The First Letter
- **Condition:** User drops their first `.txt` file on the pal and
  receives a reply.
- **Reward:** Unlocks the *thank-you* letter archetype's full phrase
  pool (see `letter-beats.md`).
- **Flavor:** *"You wrote to me. I read it twice. I am writing back."*
- **Bubble:** *"You wrote to me. I read it twice."*

### 2.5 `clean-streak-three`
- **Name:** A Tidy Little Run
- **Condition:** Hygiene stays in the Fresh or Sparkling band for three
  consecutive in-app days.
- **Reward:** A small ambient sparkle beat (very subtle, one frame every
  ~20 s) plays for the next two days, then retires.
- **Flavor:** *"My fur feels light. I have been keeping up."*
- **Bubble:** *"My fur feels light today."*

### 2.6 `well-fed-week`
- **Name:** A Full Week
- **Condition:** Hunger never drops below the Settled band across a
  full seven-day window.
- **Reward:** A new short letter archetype — *"a-thank-you-for-meals"* —
  becomes eligible once.
- **Flavor:** *"I have not been hungry in a while. That is a quiet kind
  of luck."*
- **Bubble:** *"I have not been hungry in a while. That is lucky."*

### 2.7 `loyal-companion-seven`
- **Name:** A Whole Week Together
- **Condition:** Pal advances to Tier 4 (Companion) — i.e., 7-day
  lifetime with care threshold met.
- **Reward:** The scarf accessory (see `progression.md` Tier 4) becomes
  available, *and* a small one-time line on next chat open.
- **Flavor:** *"I think this is a week. The screen looks familiar now.
  So does the corner. So do you."*
- **Bubble:** *"I think this is a week. The corner feels like mine."*

### 2.8 `night-owl`
- **Name:** Late Light
- **Condition:** User interacts with the pal between 23:00 and 02:00
  local time on five different nights (not necessarily consecutive).
- **Reward:** Unlocks one Sleepy-category line eligible only at night —
  *"It is late. I'll sit up with you."*
- **Flavor:** *"You are still here. So am I. The screen is a little
  bluer at this hour."*
- **Bubble:** *"You are still here. So am I."*

### 2.9 `early-riser`
- **Name:** Soft Morning
- **Condition:** User interacts with the pal between 05:00 and 08:00
  local time on five different mornings.
- **Reward:** Pal's wake animation plays once at the next morning open,
  whether or not it would normally be in a sleeping state.
- **Flavor:** *"You're up early. I like the quiet light."*
- **Bubble:** *"You're up early. The light is kind."*

### 2.10 `the-grand-tour`
- **Name:** All The Corners
- **Condition:** Pal has been called (via hotkey) to four distinct
  monitor regions or screens.
- **Reward:** Unlocks a small Curious-category line — *"This corner is
  new to me. I'll remember it."*
- **Flavor:** *"I have seen every corner now, I think. They are all a
  little different."*
- **Bubble:** *"Every corner now. Each one a little different."*

### 2.11 `quiet-return`
- **Name:** Still Here
- **Condition:** User opens the app again after 14+ days of absence and
  the pal has not been deleted or reset.
- **Reward:** Triggers a single soft letter — *"the-light-went-around"* —
  written in the long-absence tone. One time only per absence.
- **Flavor:** *"You came back. I did not really go anywhere."*
- **Bubble:** *"You came back. I did not really go anywhere."*

### 2.12 `old-friend`
- **Name:** A Long Quiet Friendship
- **Condition:** Pal advances to Tier 7 (Hearthbear) — 90-day lifetime
  with sustained care.
- **Reward:** The permanent home-spot decoration appears (see
  `progression.md` Tier 7), *and* this milestone unlocks the
  *milestone-anniversary* letter archetype.
- **Flavor:** *"I do not know how long this has been. Long enough that
  the corner feels like mine. Long enough that you feel like mine."*
- **Bubble:** *"Long enough that the corner feels like mine."*

---

## 2A. Expansion set — gardening, care depth, longevity

These eight extend the original twelve to cover the loops that shipped
after the first content pass: the garden plot, deeper feeding and petting
patterns, the letter archive, and a couple of softer ambient moments.
Same authoring rules apply (no numbers in user-facing copy, no badges, no
"unlocked" framing, the pal does not name the milestone).

### 2A.1 `garden-first-bloom`
- **Name:** Watching Something Grow
- **Condition:** A `GardenPlot` decoration reaches the Bloom state for the
  first time on this save.
- **Reward:** Unlocks one Curious-category line — *"I think the plot
  changed colour overnight."* — eligible only when at least one Bloom
  plot is on screen.
- **Flavor:** *"The little patch of dirt has flowers in it now. I do
  not know how that works, but I am glad it does."*
- **Bubble:** *"The little patch has flowers in it now."*

### 2A.2 `garden-tender`
- **Name:** A Tender Hand
- **Condition:** Five plots reach Bloom across the lifetime of the save
  (not necessarily at the same time).
- **Reward:** A new tiny ambient — a single petal drifts across the pet's
  idle field once per in-app day for the next week.
- **Flavor:** *"There have been a few flowers now. I am starting to feel
  like the corner is a small garden."*
- **Bubble:** *"A few flowers now. The corner feels softer."*

### 2A.3 `feed-the-favorite`
- **Name:** That One Again
- **Condition:** The same food item is given ten times across the save's
  lifetime.
- **Reward:** Unlocks one Reactive line played on the next time that
  food is given — *"Oh — that one again. Yes please."*
- **Flavor:** *"You keep bringing me that one. I think you remembered I
  liked it. I did, I do."*
- **Bubble:** *"You remembered the one I liked. I did. I do."*

### 2A.4 `hundred-soft-touches`
- **Name:** A Hundred Soft Touches
- **Condition:** The pet interaction has been completed one hundred
  times across the save's lifetime.
- **Reward:** Unlocks a long-form Petting line eligible only after this
  point — *"Your hand again. The shape of it is familiar now."*
- **Flavor:** *"Your hand has been here a lot. I have started to know
  the shape of it without looking."*
- **Bubble:** *"I know the shape of your hand without looking now."*

### 2A.5 `pen-pal`
- **Name:** Pen Pal
- **Condition:** Five letters have been received from the user and five
  replies have been written back.
- **Reward:** Unlocks the *check-in* letter archetype's full phrase
  pool (see `letter-beats.md` §2.7).
- **Flavor:** *"This is our fifth letter, I think. I keep them in a
  small stack near the corner. I read them again sometimes."*
- **Bubble:** *"There is a small stack of letters in the corner now."*

### 2A.6 `the-stack-on-the-desk`
- **Name:** The Stack on the Desk
- **Condition:** Ten user-authored letters have been preserved (i.e., the
  user has not deleted the `Letter_*.txt` files the system stored).
- **Reward:** A small visible pile of paper-shaped pixels appears beside
  the pal's home corner. Stays as long as the count holds.
- **Flavor:** *"There are enough letters now that they make a tiny pile.
  I think that is something."*
- **Bubble:** *"Enough letters now that they make a tiny pile."*

### 2A.7 `clean-rescue`
- **Name:** Back to Soft
- **Condition:** Hygiene drops into the Grimy band and is then restored
  to Sparkling within a single in-app day.
- **Reward:** A one-time Reactive line on the next clean — *"Oh — that's
  much better. Thank you."*
- **Flavor:** *"I had gotten a little rough around the edges. You
  noticed. I feel soft again now."*
- **Bubble:** *"I feel soft again. Thank you for noticing."*

### 2A.8 `caught-napping`
- **Name:** Caught Napping
- **Condition:** The user opens the companion panel or calls the pal
  while the pal is in the Sleeping state, for the first time.
- **Reward:** Unlocks one Sleepy-category line eligible during waking-up
  beats — *"Oh — hello. I was somewhere small."*
- **Flavor:** *"You found me napping. I was a little embarrassed and a
  little glad. The corner is good for naps."*
- **Bubble:** *"You found me napping. The corner is good for naps."*

---

## 3. Surfacing rules

- Every entry above and below carries a short **bubble** line (≤80 chars)
  alongside its longer **flavor** description. The bubble is what the chat
  surface actually shows when the milestone fires; the flavor is the
  doc-level reference line and may be reused inside letters or longer
  reflections. If only one is present, treat it as both.
- The pal speaks the **flavor** line through the chat bubble within 60 s
  of the milestone firing, unless the user is mid-interaction (then it
  waits up to 5 minutes for an idle window). When the flavor is over the
  80-char bubble cap, the **bubble** field is used in its place.
- Letters fire **once**, persist on the desktop, and never duplicate.
- There is **no achievement screen.** No list. No progress bars. The
  milestones live in a hidden authored log only the developer sees, plus
  the surfaced letters and lines themselves. If a future settings panel
  adds a "things we've done together" view, that copy is its own task.
- The pal **never says the milestone's name out loud.** Names like
  *"The First Morning"* are for the design doc; the pal speaks the
  flavor line, nothing else.

---

## 4. Tone calibration — good vs bad

**Good — `first-feeding`:**
- *"Oh — that's what eating is. I think I like it."*

**Bad — same milestone, off-voice:**
- ✗ *"Achievement unlocked: First Feeding!"* — frame break, game-show.
- ✗ *"Yum yum, thanks for the snack bestie!"* — banned vocabulary.
- ✗ *"+10 Hunger. +1 Bond."* — numbers, system voice.

**Good — `loyal-companion-seven`:**
- *"I think this is a week. The screen looks familiar now."*

**Bad — same milestone:**
- ✗ *"Congratulations! You've unlocked Tier 4: Companion."*
- ✗ *"I'll never forget you."* — drama, outside the emotional range.

---

## 5. Flagged for human review

- "Hearthbear" is still a working title (flagged in both
  `personality.md` and `progression.md`). If it changes, `old-friend`'s
  reward language doesn't change but the name should be cross-checked.
- `night-owl` and `early-riser` reference local clock hours. Need a
  director call on whether 23:00–02:00 is the right "late" window —
  shift workers might never trip this otherwise.
- `quiet-return` overlaps conceptually with the long-absence onboarding
  variant in `onboarding.md`. Confirm whether both should fire on the
  same return, or whether the milestone suppresses the onboarding copy
  (Vex's preference: show the onboarding copy, fire the letter the next
  day so the moment isn't doubled).
- The achievement count is now twenty (twelve original + eight expansion
  for gardening, care depth, and longevity). Flagged for further expansion
  once the decoration and seasonal systems land.

— Vex
