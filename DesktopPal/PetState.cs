using System;
using System.IO;
using System.Text.Json;

namespace DesktopPal
{
    public enum PetActionState
    {
        Idle,
        Wandering,
        Watching,
        Sleeping,
        Eating
    }

    public class PetState
    {
        public string Name { get; set; } = "Buddy";
        public DateTime BirthTime { get; set; } = DateTime.Now;
        public DateTime LastSeen { get; set; } = DateTime.Now;
        
        // Stats 0.0 - 100.0
        public double Hunger { get; set; } = 100.0;
        public double Hygiene { get; set; } = 100.0;
        public double Happiness { get; set; } = 100.0;
        public double Energy { get; set; } = 100.0;
        
        public int Level { get; set; } = 1;
        public double Experience { get; set; } = 0;
        public bool IsHatched { get; set; } = false;
        public string ModelName { get; set; } = "local-model";
        public bool VisionEnabled { get; set; } = true;
        public PetActionState CurrentState { get; set; } = PetActionState.Idle;

        public int HotkeyModifier { get; set; } = 3; // Default: Control + Alt (MOD_CONTROL | MOD_ALT)
        public int HotkeyCode { get; set; } = 0x42;   // Default: 'B'

        // First-run gate. Set to true once the user dismisses the onboarding
        // window. Persisted via Save() so onboarding only appears once per
        // installation. See issue #20.
        public bool HasCompletedOnboarding { get; set; } = false;

        private static string SavePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pet_state.json");

        public static PetState Load()
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                var state = JsonSerializer.Deserialize<PetState>(json);
                if (state == null)
                {
                    return new PetState();
                }

                state.UpdateRealTime();
                return state;
            }
            return new PetState();
        }

        public void Save()
        {
            try
            {
                LastSeen = DateTime.Now;
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SavePath, json);
            }
            catch { /* Ignore for now */ }
        }

        public void UpdateRealTime()
        {
            TimeSpan elapsed = DateTime.Now - LastSeen;
            double hours = elapsed.TotalHours;

            if (!IsHatched && elapsed.TotalMinutes > 10)
            {
                IsHatched = true;
            }

            // Stats decay over time
            Hunger = Math.Max(0, Hunger - (hours * 5.0));
            Hygiene = Math.Max(0, Hygiene - (hours * 3.0));
            Happiness = Math.Max(0, Happiness - (hours * 2.0));
            Energy = Math.Max(0, Energy - (hours * 4.0));
            
            LastSeen = DateTime.Now;
        }

        public void Tick()
        {
            if (!IsHatched)
            {
                TimeSpan age = DateTime.Now - BirthTime;
                if (age.TotalMinutes > 1) IsHatched = true; // Fast hatch for testing
                return;
            }

            // Decay per tick (assuming 1 tick per second or so)
            double decayRate = 0.0001; 
            Hunger = Math.Max(0, Hunger - decayRate);
            Hygiene = Math.Max(0, Hygiene - (decayRate * 0.5));
            Happiness = Math.Max(0, Happiness - (decayRate * 0.2));
            
            if (Hunger < 20) Happiness = Math.Max(0, Happiness - decayRate);
        }
    }
}
