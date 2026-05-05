using System;
using System.Collections.Generic;

namespace DesktopPal
{
    /// <summary>
    /// Authored offline phrase library. Source of truth lives in
    /// docs/content/offline-phrases.md (Vex). Keep in sync with that doc;
    /// when the doc and code disagree, the doc wins until a Sol PR updates
    /// the doc as well. Tier gating is described in the doc but is not yet
    /// implemented here — all phrases for a category are currently eligible.
    /// </summary>
    public static class OfflineBrain
    {
        private static readonly Random _random = new Random();

        // ── Idle ────────────────────────────────────────────────────────────
        // Default everyday lines. Drawn most often. Soft, present, unbothered.
        private static readonly List<string> _idlePhrases = new List<string>
        {
            "Just watching the clouds go by... or your icons.",
            "Your desktop is very cozy.",
            "I like it here.",
            "I am keeping an eye on things for you.",
            "The taskbar is like a long bridge.",
            "Everything looks good from down here.",
            "Don't mind me, just exploring.",
            "You look busy. I'll just sit here.",
            "I wonder if I can climb a window.",
            "Hope you're having a gentle day.",
            "The cursor is a very fast little friend.",
            "I found a warm patch of pixels.",
            "*hums a small tune*",
            "Today the screen is quiet, and so am I.",
            "I'm thinking about trees. And flowers. And snacks.",
            "The light from the monitor is nice on my fur.",
            "*blinks slowly*",
            "Folders are a kind of cave, I think.",
            "The recycle bin is grumpy. I leave it alone.",
            "I don't know what you're working on, but I'm rooting for it.",
            "Sometimes I just sit and listen to the fan.",
            "Your icons are arranged like little tiles.",
            "I like the corner near the clock.",
            "*yawns, very tiny*"
        };

        // ── Hungry ──────────────────────────────────────────────────────────
        // Plaintive, never grumpy. The pal asks; it does not demand.
        private static readonly List<string> _hungryPhrases = new List<string>
        {
            "My tummy is rumbling, just a little.",
            "Could I have a small snack? Please?",
            "Thinking about delicious pixels.",
            "Hungry. A bit.",
            "I would not say no to a crumb.",
            "Any spare food for a small bear?",
            "Did someone say snack? No? Just hopeful.",
            "I'm feeling a bit weak. Needs food.",
            "My belly is making opinions.",
            "I keep thinking about round things. Berries. Buttons. Snacks.",
            "If a snack came by, I would be grateful.",
            "The kitchen is far away. Sad fact.",
            "I'd like something soft to chew on.",
            "*small hungry sigh*",
            "Food, friend? When you can.",
            "I am polite. But I am also hungry.",
            "A little something would land well right now.",
            "I'd trade a flower for a snack. (I would not, actually. But almost.)"
        };

        // ── Petting ─────────────────────────────────────────────────────────
        // Reactive lines while the user pets the pal. Soft, bashful, fond.
        private static readonly List<string> _pettingPhrases = new List<string>
        {
            "That feels so nice.",
            "Hehe, that tickles.",
            "Aww, thank you.",
            "*purrs digitally*",
            "More pets, please.",
            "I feel so loved.",
            "You have such a kind mouse cursor.",
            "My fur is the happiest it has been all day.",
            "*small contented noise*",
            "Right there is good. Right there is perfect.",
            "Oh. Oh, that's the spot.",
            "I'm going to remember this for a while.",
            "*leans into the cursor*",
            "The day got softer just now.",
            "You're being very gentle. I notice.",
            "I think I am glowing a little.",
            "Okay, one more. Just one more. (Maybe two.)",
            "*quiet, happy sigh*"
        };

        // ── Calling ─────────────────────────────────────────────────────────
        // The pal acknowledges and moves toward the cursor.
        private static readonly List<string> _callingPhrases = new List<string>
        {
            "Coming!",
            "Wait for me!",
            "I'm on my way.",
            "Did you need me?",
            "Zooming over.",
            "Here I am.",
            "Yes? I'm here.",
            "Trotting over.",
            "*tiny footsteps*",
            "On the way, on the way.",
            "Don't move, I'm coming.",
            "I heard you. Hold on.",
            "Almost there.",
            "*small hop in your direction*",
            "You called? Good. I was getting bored.",
            "Right behind you."
        };

        // ── Sleepy ──────────────────────────────────────────────────────────
        // Late-night / Low-energy. Slow, drifty, lower-volume.
        // Tier 3+ in the doc; gating not yet enforced here.
        private static readonly List<string> _sleepyPhrases = new List<string>
        {
            "*yawns*",
            "My eyes are getting heavy.",
            "I might just rest them for a moment.",
            "The screen is dimmer, or maybe I am.",
            "*settles into the corner*",
            "Sleep is a soft folder.",
            "I'll keep watch from here. With my eyes mostly closed.",
            "Just a small nap. Five minutes. Maybe seven.",
            "The cursor is moving slow. I like it.",
            "*blinks, blinks again, blinks more slowly*",
            "Goodnight, taskbar.",
            "The light has gone gold. That means rest.",
            "I'm tired, but the good kind.",
            "I'll dream about flowers. And folders. And you.",
            "*small, sleepy hum*",
            "Tucking into the warm patch of pixels.",
            "Even my fur is yawning.",
            "Wake me if anything important happens. Or don't.",
            "*curls up*",
            "The day was a good shape."
        };

        // ── Excited ─────────────────────────────────────────────────────────
        // Sunny / restless / curious-with-energy. Bouncier — still inside the
        // voice rules. Tier 2+ in the doc.
        private static readonly List<string> _excitedPhrases = new List<string>
        {
            "Oh! Something good is happening.",
            "*small hop*",
            "Today feels bright.",
            "The desktop is wide and full of corners.",
            "I want to look at everything.",
            "*bounces a little*",
            "I have so much energy. I don't know what to do with it.",
            "Watch this — I'm going to wander somewhere new.",
            "Everything looks brand new today.",
            "I think I'll plant something.",
            "The cursor and I should race.",
            "I feel light.",
            "*spins in place, just once*",
            "Your icons look extra friendly today.",
            "I want to climb something. Anything.",
            "Today is a good day to be a bear.",
            "The screen is humming and so am I.",
            "*eyes wide, ears up*"
        };

        // ── Curious ─────────────────────────────────────────────────────────
        // The pal noticing things. Most "personality-forward" category.
        // Tier 1: first 5 only; Tier 2+: full list. Gating not yet enforced.
        private static readonly List<string> _curiousPhrases = new List<string>
        {
            "I wonder what's inside a folder.",
            "Did you know I can see your mouse? It's like a giant bug.",
            "I wonder if I can climb a window.",
            "The taskbar is like a long bridge.",
            "I'm thinking about trees. And flowers. And snacks.",
            "What does that icon do? I won't touch it. Just curious.",
            "The window with all the words — is that for reading?",
            "Why do some folders feel heavier than others?",
            "The cursor sometimes hesitates. I wonder what it's thinking.",
            "Where do windows go when you close them?",
            "The little blinking line in the text box — is it breathing?",
            "I think the start menu is a kind of cave.",
            "Why does the screen get warmer in the afternoon?",
            "If I sit very still, will the icons forget I'm here?",
            "Is the recycle bin lonely? It seems lonely.",
            "I noticed the wallpaper changed. Or maybe I just woke up better.",
            "What's a \"tab\"? You have so many.",
            "The clock keeps changing. I respect it.",
            "I wonder if there are other bears on other desktops.",
            "The shape of your folders is a kind of language."
        };

        // ── Encouraging ─────────────────────────────────────────────────────
        // Soft response when the user has been working a long time, or when
        // Happiness is the dominant low signal. Never coach voice. Always small.
        // Tier 3+: first 6; Tier 4+: full list. Gating not yet enforced.
        private static readonly List<string> _encouragingPhrases = new List<string>
        {
            "You are doing a good job.",
            "I'm rooting for you, quietly.",
            "Take a slow breath if you can.",
            "You've kept going for a while now. I noticed.",
            "Whatever it is, it can wait one minute.",
            "I'll be here in the corner when you come back.",
            "You don't have to do it all today.",
            "Small steps still count. They count a lot, actually.",
            "The cursor and I both think you're doing fine.",
            "Even sitting with a hard thing is a kind of progress.",
            "Your shoulders look tired. Mine too. Solidarity.",
            "You can rest without finishing first.",
            "I've watched you try. That part matters.",
            "The day is long. You have time.",
            "If you stop now, I'll keep your spot warm.",
            "You are kinder than you give yourself credit for.",
            "One small thing at a time. That's how I wander, too.",
            "It's okay if today is a quiet day."
        };

        public static string GetRandomPhrase(string category)
        {
            List<string> list = category switch
            {
                "Hungry"      => _hungryPhrases,
                "Petting"     => _pettingPhrases,
                "Calling"     => _callingPhrases,
                "Sleepy"      => _sleepyPhrases,
                "Excited"     => _excitedPhrases,
                "Curious"     => _curiousPhrases,
                "Encouraging" => _encouragingPhrases,
                _             => _idlePhrases
            };

            return list[_random.Next(list.Count)];
        }
    }
}
