using System;
using System.Collections.Generic;

namespace DesktopPal
{
    public static class OfflineBrain
    {
        private static Random _random = new Random();

        private static List<string> _idlePhrases = new List<string>
        {
            "Just watching the clouds go by... or your icons.",
            "Is it snack time yet?",
            "You're doing a great job!",
            "I wonder what's inside a folder...",
            "Your desktop is very cozy.",
            "I like it here.",
            "Did you know I can see your mouse? It's like a giant bug!",
            "Wandering is my favorite hobby.",
            "*hums a little tune*",
            "I'm keeping an eye on things for you.",
            "The taskbar is like a long bridge.",
            "I feel lucky to be your buddy.",
            "Everything looks good from down here!",
            "Are we having fun yet?",
            "I'm thinking about trees. And flowers. And snacks.",
            "Don't mind me, just exploring.",
            "You look busy! I'll just sit here.",
            "I wonder if I can climb a window?",
            "My favorite color is... well, green, mostly.",
            "Hope you're having a wonderful day!"
        };

        private static List<string> _hungryPhrases = new List<string>
        {
            "My tummy is rumbling!",
            "Could I have a tiny snack? Please?",
            "Thinking about delicious pixels...",
            "Hungry! Hungry!",
            "I'm so hungry I could eat a shortcut!",
            "Any spare crumbs for a buddy?",
            "Food? Did someone say food?",
            "I'm feeling a bit weak... needs food..."
        };

        private static List<string> _pettingPhrases = new List<string>
        {
            "That feels so nice!",
            "Hehe, that tickles!",
            "You're the best!",
            "Aww, thank you!",
            "*purrs digitally*",
            "More pets please!",
            "I feel so loved!",
            "You have such a kind mouse cursor."
        };

        private static List<string> _callingPhrases = new List<string>
        {
            "Coming!",
            "Wait for me!",
            "I'm on my way!",
            "Did you need me?",
            "Zooming over!",
            "Here I am!"
        };

        public static string GetRandomPhrase(string category)
        {
            List<string> list = category switch
            {
                "Hungry" => _hungryPhrases,
                "Petting" => _pettingPhrases,
                "Calling" => _callingPhrases,
                _ => _idlePhrases
            };

            return list[_random.Next(list.Count)];
        }
    }
}
