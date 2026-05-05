using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopPal
{
    // ── Enumerations ─────────────────────────────────────────────────────────────

    public enum PetActionState
    {
        Idle,
        Wandering,
        Watching,
        Sleeping,
        Eating
    }

    // ── Model ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Represents all persistent data for a DesktopPal pet.
    /// Handles JSON serialisation, real-time stat decay, and level progression.
    /// </summary>
    public class PetState
    {
        // ── Constants ────────────────────────────────────────────────────────────
        public const double StatMin = 0.0;
        public const double StatMax = 100.0;

        /// <summary>Decay rate in stat-units per hour for each stat.</summary>
        private const double HungerDecayPerHour = 5.0;
        private const double HygieneDecayPerHour = 3.0;
        private const double HappinessDecayPerHour = 2.0;
        private const double EnergyDecayPerHour = 4.0;

        /// <summary>Additional happiness penalty per hour when hunger is below 20.</summary>
        private const double HappinessPenaltyPerHour = 1.5;

        /// <summary>Experience needed per level (linear).</summary>
        private const double ExperiencePerLevel = 100.0;

        // ── Properties ───────────────────────────────────────────────────────────
        public string Name { get; set; } = "Buddy";
        public DateTime BirthTime { get; set; } = DateTime.Now;
        public DateTime LastSeen { get; set; } = DateTime.Now;

        // Stats – always clamped to [StatMin, StatMax]
        public double Hunger { get; set; } = StatMax;
        public double Hygiene { get; set; } = StatMax;
        public double Happiness { get; set; } = StatMax;
        public double Energy { get; set; } = StatMax;

        public int Level { get; set; } = 1;
        public double Experience { get; set; } = 0;
        public bool IsHatched { get; set; } = false;
        public string ModelName { get; set; } = "local-model";
        public bool VisionEnabled { get; set; } = true;
        public PetActionState CurrentState { get; set; } = PetActionState.Idle;

        // ── Persistence ──────────────────────────────────────────────────────────
        [JsonIgnore]
        private static string SavePath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pet_state.json");

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        /// <summary>
        /// Loads the saved pet state from disk, applies real-time stat decay for the
        /// offline period, and returns the result. If no save file exists or it is
        /// corrupt, a fresh <see cref="PetState"/> is returned.
        /// </summary>
        public static PetState Load()
        {
            if (!File.Exists(SavePath))
            {
                DebugLogger.Info("No save file found – creating a new pet.");
                return new PetState();
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                var state = JsonSerializer.Deserialize<PetState>(json, _jsonOptions);

                if (state is null)
                {
                    DebugLogger.Warning("Deserialised pet state was null – starting fresh.");
                    return new PetState();
                }

                state.UpdateRealTime();
                DebugLogger.Info($"Pet '{state.Name}' loaded. Level {state.Level}, Hunger {state.Hunger:F1}%.");
                return state;
            }
            catch (JsonException ex)
            {
                DebugLogger.Error("Pet save file is corrupt – starting fresh.", ex);
                return new PetState();
            }
            catch (IOException ex)
            {
                DebugLogger.Error("Could not read pet save file.", ex);
                return new PetState();
            }
        }

        /// <summary>Persists the current state to disk.</summary>
        public void Save()
        {
            try
            {
                LastSeen = DateTime.Now;
                string json = JsonSerializer.Serialize(this, _jsonOptions);
                File.WriteAllText(SavePath, json);
                DebugLogger.Debug("Pet state saved.");
            }
            catch (IOException ex)
            {
                DebugLogger.Error("Failed to save pet state.", ex);
            }
            catch (Exception ex)
            {
                DebugLogger.Error("Unexpected error while saving pet state.", ex);
            }
        }

        // ── Real-time decay ──────────────────────────────────────────────────────

        /// <summary>
        /// Calculates how much time has passed since <see cref="LastSeen"/> and
        /// applies proportional stat decay. Called once on startup.
        /// </summary>
        public void UpdateRealTime()
        {
            TimeSpan elapsed = DateTime.Now - LastSeen;
            double hours = Math.Max(0, elapsed.TotalHours);

            DebugLogger.Info($"Applying offline decay for {elapsed.TotalMinutes:F1} minutes ({hours:F3} hours).");

            if (!IsHatched && elapsed.TotalMinutes >= 10)
            {
                IsHatched = true;
                DebugLogger.Info("Pet hatched during offline period!");
            }

            Hunger = Clamp(Hunger - hours * HungerDecayPerHour);
            Hygiene = Clamp(Hygiene - hours * HygieneDecayPerHour);
            Happiness = Clamp(Happiness - hours * HappinessDecayPerHour);
            Energy = Clamp(Energy - hours * EnergyDecayPerHour);

            LastSeen = DateTime.Now;
        }

        // ── Per-tick update ──────────────────────────────────────────────────────

        /// <summary>
        /// Applies a tiny stat decay per game-loop tick (~60 fps / ~1 tick per
        /// CompositionTarget.Rendering frame). Also checks hatch state and levelling.
        /// </summary>
        public void Tick()
        {
            if (!IsHatched)
            {
                TimeSpan age = DateTime.Now - BirthTime;
                if (age.TotalMinutes >= 1)
                {
                    IsHatched = true;
                    DebugLogger.Info("Pet hatched!");
                }
                return;
            }

            // Tiny per-frame decay (~0.006 stat-units per second at 60 fps)
            const double decayRate = 0.0001;

            Hunger = Clamp(Hunger - decayRate);
            Hygiene = Clamp(Hygiene - decayRate * 0.5);
            Happiness = Clamp(Happiness - decayRate * 0.2);

            // Additional happiness penalty when hungry
            if (Hunger < 20)
                Happiness = Clamp(Happiness - decayRate);

            // Level-up check
            if (Experience >= Level * ExperiencePerLevel)
            {
                Level++;
                DebugLogger.Info($"Pet levelled up to {Level}!");
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>Clamps a stat value to [<see cref="StatMin"/>, <see cref="StatMax"/>].</summary>
        public static double Clamp(double value) =>
            Math.Max(StatMin, Math.Min(StatMax, value));

        /// <summary>Adds experience and triggers a level check on the next tick.</summary>
        public void AddExperience(double amount)
        {
            if (amount <= 0) return;
            Experience += amount;
            DebugLogger.Debug($"Added {amount} XP. Total: {Experience:F1}");
        }
    }
}

