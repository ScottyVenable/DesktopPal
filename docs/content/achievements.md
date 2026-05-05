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

## 2. The twelve

Each entry below maps to a content row Sol can wire into the milestone
system. `id` is kebab-case and stable; `condition` is plain-language for
content review (Sol translates to code); `reward` is what the user sees;
`flavor` is the line the pal speaks or writes when the milestone fires.

---

### 2.1 `first-hatch`
- **Name:** The First Morning
- **Condition:** Pal advances from Egg (Tier 0) to Hatchling (Tier 1).
- **Reward:** A short letter on the desktop titled *"hello-from-the-shell"*
  (already specced in `progression.md`). The half-shell remains as a
  permanent tiny world object in the pal's home corner.
- **Flavor:** *"I think I just opened my eyes for the first time. The
  light is very kind."*

### 2.2 `first-feeding`
- **Name:** A Soft First Meal
- **Condition:** First successful feed interaction after hatching.
- **Reward:** Unlocks a single new Idle line — *"That was a good one."* —
  which becomes eligible whenever Hunger is Full or Stuffed.
- **Flavor:** *"Oh — that's what eating is. I think I like it."*

### 2.3 `first-petting`
- **Name:** Held for a Moment
- **Condition:** First time the pet interaction is held to completion.
- **Reward:** Pal's idle wander gains a small "look toward cursor" beat
  for 24 in-app hours after the milestone. No further unlock.
- **Flavor:** *"Your hand is warm. I noticed."*

### 2.4 `first-letter`
- **Name:** The First Letter
- **Condition:** User drops their first `.txt` file on the pal and
  receives a reply.
- **Reward:** Unlocks the *thank-you* letter archetype's full phrase
  pool (see `letter-beats.md`).
- **Flavor:** *"You wrote to me. I read it twice. I am writing back."*

### 2.5 `clean-streak-three`
- **Name:** A Tidy Little Run
- **Condition:** Hygiene stays in the Fresh or Sparkling band for three
  consecutive in-app days.
- **Reward:** A small ambient sparkle beat (very subtle, one frame every
  ~20 s) plays for the next two days, then retires.
- **Flavor:** *"My fur feels light. I have been keeping up."*

### 2.6 `well-fed-week`
- **Name:** A Full Week
- **Condition:** Hunger never drops below the Settled band across a
  full seven-day window.
- **Reward:** A new short letter archetype — *"a-thank-you-for-meals"* —
  becomes eligible once.
- **Flavor:** *"I have not been hungry in a while. That is a quiet kind
  of luck."*

### 2.7 `loyal-companion-seven`
- **Name:** A Whole Week Together
- **Condition:** Pal advances to Tier 4 (Companion) — i.e., 7-day
  lifetime with care threshold met.
- **Reward:** The scarf accessory (see `progression.md` Tier 4) becomes
  available, *and* a small one-time line on next chat open.
- **Flavor:** *"I think this is a week. The screen looks familiar now.
  So does the corner. So do you."*

### 2.8 `night-owl`
- **Name:** Late Light
- **Condition:** User interacts with the pal between 23:00 and 02:00
  local time on five different nights (not necessarily consecutive).
- **Reward:** Unlocks one Sleepy-category line eligible only at night —
  *"It is late. I'll sit up with you."*
- **Flavor:** *"You are still here. So am I. The screen is a little
  bluer at this hour."*

### 2.9 `early-riser`
- **Name:** Soft Morning
- **Condition:** User interacts with the pal between 05:00 and 08:00
  local time on five different mornings.
- **Reward:** Pal's wake animation plays once at the next morning open,
  whether or not it would normally be in a sleeping state.
- **Flavor:** *"You're up early. I like the quiet light."*

### 2.10 `the-grand-tour`
- **Name:** All The Corners
- **Condition:** Pal has been called (via hotkey) to four distinct
  monitor regions or screens.
- **Reward:** Unlocks a small Curious-category line — *"This corner is
  new to me. I'll remember it."*
- **Flavor:** *"I have seen every corner now, I think. They are all a
  little different."*

### 2.11 `quiet-return`
- **Name:** Still Here
- **Condition:** User opens the app again after 14+ days of absence and
  the pal has not been deleted or reset.
- **Reward:** Triggers a single soft letter — *"the-light-went-around"* —
  written in the long-absence tone. One time only per absence.
- **Flavor:** *"You came back. I did not really go anywhere."*

### 2.12 `old-friend`
- **Name:** A Long Quiet Friendship
- **Condition:** Pal advances to Tier 7 (Hearthbear) — 90-day lifetime
  with sustained care.
- **Reward:** The permanent home-spot decoration appears (see
  `progression.md` Tier 7), *and* this milestone unlocks the
  *milestone-anniversary* letter archetype.
- **Flavor:** *"I do not know how long this has been. Long enough that
  the corner feels like mine. Long enough that you feel like mine."*

---

## 3. Surfacing rules

- The pal speaks the **flavor** line through the chat bubble within 60 s
  of the milestone firing, unless the user is mid-interaction (then it
  waits up to 5 minutes for an idle window).
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
- The achievement count (12) is a starting set. Flagged for expansion
  once the letter and decoration systems land.

— Vex
