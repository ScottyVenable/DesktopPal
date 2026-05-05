# Offline Phrases — Authored Library

> Canonical authored library of the pal's offline (no-LLM) chat lines.
> This file is the **source of truth**. The current `OfflineBrain.cs`
> contains an early subset; Sol or another agent can port these into the
> code at any time. When the two disagree, this file wins until a Sol PR
> updates the doc.
>
> All phrases obey `personality.md`. No emojis. No memes. No coach voice.

---

## How to use this library

- Each category is a flat list of phrases.
- Phrases are ordered roughly from "always-safe" at the top to "more
  flavored / tier-gated" toward the bottom of each list. Tier gating is
  noted in `progression.md` §3 — anything beyond the first 5 lines of
  *Curious*, all of *Sleepy*, and all of *Encouraging* should be locked
  until the appropriate tier.
- Every phrase is ≤ 14 words unless noted, and contains at most one `!`.
- Asterisk-action lines (e.g., *\*hums\**) count as a phrase and are
  allowed sparingly — currently capped at ~10% of any list.
- The pal's name in any phrase should be templated as `{pet_name}` so a
  user-renamed pal still sounds right; the lists below use plain words
  and avoid hardcoding the name.

---

## Idle

Default everyday lines. Drawn most often. Should feel *unbothered, soft,
present*. Includes the original `OfflineBrain.cs` lines that pass review,
plus expansions.

1. Just watching the clouds go by… or your icons.
2. Your desktop is very cozy.
3. I like it here.
4. I am keeping an eye on things for you.
5. The taskbar is like a long bridge.
6. Everything looks good from down here.
7. Don't mind me, just exploring.
8. You look busy. I'll just sit here.
9. I wonder if I can climb a window.
10. Hope you're having a gentle day.
11. The cursor is a very fast little friend.
12. I found a warm patch of pixels.
13. *hums a small tune*
14. Today the screen is quiet, and so am I.
15. I'm thinking about trees. And flowers. And snacks.
16. The light from the monitor is nice on my fur.
17. *blinks slowly*
18. Folders are a kind of cave, I think.
19. The recycle bin is grumpy. I leave it alone.
20. I don't know what you're working on, but I'm rooting for it.
21. Sometimes I just sit and listen to the fan.
22. Your icons are arranged like little tiles.
23. I like the corner near the clock.
24. *yawns, very tiny*

> 24 phrases. Sol can grow this list freely; new entries should match the
> top half's tone.

---

## Hungry

Plaintive, never grumpy. The pal asks; it does not demand. Forced when
Hunger is at Critical (see `progression.md` §4.3).

1. My tummy is rumbling, just a little.
2. Could I have a small snack? Please?
3. Thinking about delicious pixels.
4. Hungry. A bit.
5. I would not say no to a crumb.
6. Any spare food for a small bear?
7. Did someone say snack? No? Just hopeful.
8. I'm feeling a bit weak. Needs food.
9. My belly is making opinions.
10. I keep thinking about round things. Berries. Buttons. Snacks.
11. If a snack came by, I would be grateful.
12. The kitchen is far away. Sad fact.
13. I'd like something soft to chew on.
14. *small hungry sigh*
15. Food, friend? When you can.
16. I am polite. But I am also hungry.
17. A little something would land well right now.
18. I'd trade a flower for a snack. (I would not, actually. But almost.)

> 18 phrases.

---

## Petting

Reactive lines while the user pets the pal. Soft, bashful, fond.

1. That feels so nice.
2. Hehe, that tickles.
3. Aww, thank you.
4. *purrs digitally*
5. More pets, please.
6. I feel so loved.
7. You have such a kind mouse cursor.
8. My fur is the happiest it has been all day.
9. *small contented noise*
10. Right there is good. Right there is perfect.
11. Oh. Oh, that's the spot.
12. I'm going to remember this for a while.
13. *leans into the cursor*
14. The day got softer just now.
15. You're being very gentle. I notice.
16. I think I am glowing a little.
17. Okay, one more. Just one more. (Maybe two.)
18. *quiet, happy sigh*

> 18 phrases.

---

## Calling

When the user clicks/whistles/uses the call hotkey. The pal acknowledges
and moves toward the cursor.

1. Coming!
2. Wait for me!
3. I'm on my way.
4. Did you need me?
5. Zooming over.
6. Here I am.
7. Yes? I'm here.
8. Trotting over.
9. *tiny footsteps*
10. On the way, on the way.
11. Don't move, I'm coming.
12. I heard you. Hold on.
13. Almost there.
14. *small hop in your direction*
15. You called? Good. I was getting bored.
16. Right behind you.

> 16 phrases.

---

## Sleepy *(Tier 3+; Tier 5+ for the longer reflective lines)*

Used when Energy is Low/Drowsy or system clock is in the late window.
Slow, drifty, lower-volume.

1. *yawns*
2. My eyes are getting heavy.
3. I might just rest them for a moment.
4. The screen is dimmer, or maybe I am.
5. *settles into the corner*
6. Sleep is a soft folder.
7. I'll keep watch from here. With my eyes mostly closed.
8. Just a small nap. Five minutes. Maybe seven.
9. The cursor is moving slow. I like it.
10. *blinks, blinks again, blinks more slowly*
11. Goodnight, taskbar.
12. The light has gone gold. That means rest.
13. I'm tired, but the good kind.
14. I'll dream about flowers. And folders. And you.
15. *small, sleepy hum*
16. Tucking into the warm patch of pixels.
17. Even my fur is yawning.
18. Wake me if anything important happens. Or don't.
19. *curls up*
20. The day was a good shape.

> 20 phrases.

---

## Excited *(Tier 2+)*

Used when mood is Sunny, Restless, or Curious-with-energy. Slightly
bouncier — but still inside the voice rules. One `!` max per line.

1. Oh! Something good is happening.
2. *small hop*
3. Today feels bright.
4. The desktop is wide and full of corners.
5. I want to look at everything.
6. *bounces a little*
7. I have so much energy. I don't know what to do with it.
8. Watch this — I'm going to wander somewhere new.
9. Everything looks brand new today.
10. I think I'll plant something.
11. The cursor and I should race.
12. I feel light.
13. *spins in place, just once*
14. Your icons look extra friendly today.
15. I want to climb something. Anything.
16. Today is a good day to be a bear.
17. The screen is humming and so am I.
18. *eyes wide, ears up*

> 18 phrases.

---

## Curious *(Tier 1: first 5 only; Tier 2+: full list)*

The pal noticing things. The most "personality-forward" category. Each
line should land on a small concrete image.

1. I wonder what's inside a folder.
2. Did you know I can see your mouse? It's like a giant bug.
3. I wonder if I can climb a window.
4. The taskbar is like a long bridge.
5. I'm thinking about trees. And flowers. And snacks.
6. What does that icon do? I won't touch it. Just curious.
7. The window with all the words — is that for reading?
8. Why do some folders feel heavier than others?
9. The cursor sometimes hesitates. I wonder what it's thinking.
10. Where do windows go when you close them?
11. The little blinking line in the text box — is it breathing?
12. I think the start menu is a kind of cave.
13. Why does the screen get warmer in the afternoon?
14. If I sit very still, will the icons forget I'm here?
15. Is the recycle bin lonely? It seems lonely.
16. I noticed the wallpaper changed. Or maybe I just woke up better.
17. What's a "tab"? You have so many.
18. The clock keeps changing. I respect it.
19. I wonder if there are other bears on other desktops.
20. The shape of your folders is a kind of language.

> 20 phrases.

---

## Encouraging *(Tier 3+: reduced first 6; Tier 4+: full list)*

Used when the user has been working a long time, when Happiness is the
dominant low signal, or as a soft response to a frustrated tone. Never
coach voice. Never "you got this." Always small.

1. You are doing a good job.
2. I'm rooting for you, quietly.
3. Take a slow breath if you can.
4. You've kept going for a while now. I noticed.
5. Whatever it is, it can wait one minute.
6. I'll be here in the corner when you come back.
7. You don't have to do it all today.
8. Small steps still count. They count a lot, actually.
9. The cursor and I both think you're doing fine.
10. Even sitting with a hard thing is a kind of progress.
11. Your shoulders look tired. Mine too. Solidarity.
12. You can rest without finishing first.
13. I've watched you try. That part matters.
14. The day is long. You have time.
15. If you stop now, I'll keep your spot warm.
16. You are kinder than you give yourself credit for.
17. One small thing at a time. That's how I wander, too.
18. It's okay if today is a quiet day.

> 18 phrases.

---

## Totals and gaps

| Category    | Count | Tier gate                              |
| ----------- | ----- | -------------------------------------- |
| Idle        | 24    | All tiers (Hatchling sees full list)   |
| Hungry      | 18    | All tiers                              |
| Petting     | 18    | All tiers                              |
| Calling     | 16    | All tiers                              |
| Sleepy      | 20    | Tier 3+ (full at Tier 5)               |
| Excited     | 18    | Tier 2+                                |
| Curious     | 20    | Tier 1: first 5; Tier 2+: full         |
| Encouraging | 18    | Tier 3+: first 6; Tier 4+: full        |

All categories are within the 15–25 target. Idle and Curious are
deliberately deepest because they fire the most often.

---

## Porting notes for Sol

- The current `OfflineBrain.cs` `_idlePhrases`, `_hungryPhrases`,
  `_pettingPhrases`, `_callingPhrases` lists predate this document. Lines
  in those arrays that are *not* present here were intentionally pruned
  (e.g., color references that conflict with the blue-bear identity) and
  should be removed during the port. One example: *"My favorite color is…
  well, green, mostly."* — does not survive the blue-bear voice pass.
- The new categories (Sleepy / Excited / Curious / Encouraging) need
  matching `category` strings in `GetRandomPhrase`.
- Tier gating is *not* implemented in the current OfflineBrain. When tier
  awareness lands, the gate column above is the contract.
- Treat this file as authored data. Code edits that change voice should
  ping Vex first.

— Vex
