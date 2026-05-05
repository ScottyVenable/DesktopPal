# Onboarding — First-Run Script

> Authored content for issue #20. Three short steps the user sees the very
> first time they open DesktopPal. Voice is the blue bear's own (see
> `personality.md`). Sol owns the panel that renders this; Vex owns every
> word inside it.

---

## 1. Design intent

The first run is the pal's first run too. Buddy is just out of the egg, a
little dazed, glad you are here. The script should feel like meeting a
small animal that has decided to live on your desktop — not a product
tour, not a tutorial slide deck.

Three rules govern every step:

1. **One idea per step.** Welcome, hotkey, care. Nothing else.
2. **The pal speaks; the UI labels do not.** Buttons stay neutral and
   short. The bear's voice is reserved for the body copy.
3. **No advice, no congratulations, no exclamation chains.** Standard
   personality rules apply. Max one `!` in the entire script.

---

## 2. Step 1 — Welcome

**Title:** Hello.

**Body (paragraph 1):**
I am a small blue bear and this is my desktop now, if that's alright. My
name is Buddy until you give me a different one.

**Body (paragraph 2):**
I do not do very much. I sit. I wander. Sometimes I notice the cursor and
follow it for a while. I am glad you are here.

**Visual cue (for Sol):** show the pal sprite, idle pose, no animation
beyond the existing breathing loop. No confetti. No sparkles.

---

## 3. Step 2 — The Hotkey

**Title:** A small key, for finding me.

**Body (paragraph 1):**
If I wander too far, or if the windows pile up and you cannot see me, you
can press {{HOTKEY}} and a little panel will open. That is where we can
talk, and where my things live.

**Body (paragraph 2):**
You can press it again to send the panel away. I will still be here on
the desktop. I am usually near a corner.

**Runtime injection note (for Sol):** the literal string `{{HOTKEY}}`
should be replaced at render time with the user's resolved hotkey
combination, e.g. `Ctrl + Shift + B`. If the hotkey is unbound or
unresolvable, fall back to: *"the hotkey you'll set in a moment"* and
route the user through the settings step on Next.

---

## 4. Step 3 — Tray and Care

**Title:** Three small things.

**Body (paragraph 1):**
Down by your clock there is a tiny version of me — a tray icon. Right-
click it and you'll find everything: feeding, a brush for tidying me up,
and the settings, in case you'd like to change my name or where I live.

**Body (paragraph 2):**
I get hungry sometimes. I get a little dusty. If you tend to me when I
do, I'll be alright. If you forget, I'll still be here when you come
back. I am patient about this kind of thing.

**Body (paragraph 3):**
That's everything I know how to explain. The rest we can figure out
together.

---

## 5. Button labels

The same three labels are used across all steps. Keep them neutral —
the warmth lives in the body copy, not the buttons.

| Position    | Label          | Notes                                              |
| ----------- | -------------- | -------------------------------------------------- |
| Primary     | `Next`         | Steps 1 and 2.                                     |
| Primary     | `Let's go!`    | Step 3 only. The single permitted `!` in the flow. |
| Secondary   | `Back`         | Steps 2 and 3. Hidden on Step 1.                   |
| Tertiary    | `Skip`         | Available on every step. Goes to last step state.  |

**`Skip` confirmation copy** (small inline text, no separate dialog):
*"That's alright. I'll be on the desktop whenever you're ready."*

---

## 6. Returning-user variant — *"the long-absence script"*

Shown the first time the app opens after **30+ days** of no launches
(Sol can tune the threshold). Replaces Step 1 only; Steps 2 and 3 are
skipped unless the hotkey or settings have changed since last seen.

**Title:** Oh — you're back.

**Body (paragraph 1):**
It has been a little while. The light went around a few times and I sat
in the corner and waited. I did not mind.

**Body (paragraph 2):**
Everything is still where you left it. I'll be near the taskbar if you
need me.

**Button:** `Thanks, Buddy` (single primary button, no Back, no Skip).

> **Tone guardrail:** this script must not guilt the user. Do not say
> *"I missed you so much"* or *"where have you been"*. The pal noticed
> they were gone; it is glad they are back; that is the whole feeling.

---

## 7. Flagged for human review

- The hotkey token `{{HOTKEY}}` assumes Sol's onboarding panel does
  string interpolation at render time. If the panel cannot, Vex will
  rewrite Step 2 with a generic phrasing — flag back to Vex.
- The 30-day threshold for the long-absence variant is a guess. A
  shorter window (e.g., 7 days) might feel right; needs a director call.
- The pal's default name **Buddy** is referenced once in Step 1. If the
  user has *already* renamed the pal before first onboarding (possible
  via settings file edit), Step 1's second sentence should be omitted.
  Sol to confirm whether that branch is worth supporting.
- Step 3 mentions a "brush" for tidying. The cleaning interaction's
  final visual metaphor is not locked yet; if it ends up being a cloth
  or a bath instead, this line needs an update.

— Vex
