using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Persisted record for a single garden plot (issue #3). Position is in
    /// WPF device-independent pixels relative to the primary work area at
    /// save time. <see cref="LastTransition"/> anchors the lifecycle clock so
    /// state can advance offline using the same elapsed-real-time pattern as
    /// the pet's stat decay.
    /// </summary>
    public class GardenPlotData
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public double X { get; set; }
        public double Y { get; set; }
        public GardenPlotState State { get; set; } = GardenPlotState.Empty;
        public DateTime LastTransition { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Lightweight world-state container persisted alongside <see cref="PetState"/>.
    /// Lives inside the same save file as a forward-compatible field so older
    /// saves without a World node still load (System.Text.Json defaults to a
    /// fresh empty instance). Full split into a separate world_state.json is
    /// scoped in docs/design/world-state.md and is out of MVP scope.
    /// </summary>
    public class WorldState
    {
        public int SchemaVersion { get; set; } = 1;
        public List<GardenPlotData> Plots { get; set; } = new();
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

        // Persistent world objects (issue #3). Older saves that predate the
        // gardening loop will deserialize this as a fresh empty WorldState.
        public WorldState World { get; set; } = new WorldState();

        // Save path now resolves under %LOCALAPPDATA%\DesktopPal\ via the
        // Paths helper. The previous location (next to the .exe) is no longer
        // writable once the app is packaged into Program Files / MSIX. Paths
        // also runs a one-shot legacy migration on first use. Path-only
        // change in this revision; JSON schema is unchanged.
        private static string SavePath => Paths.PetStatePath;

        public static PetState Load()
        {
            Paths.EnsureInitialized();

            if (File.Exists(SavePath))
            {
                try
                {
                    string json = File.ReadAllText(SavePath);
                    var state = JsonSerializer.Deserialize<PetState>(json);
                    if (state == null)
                    {
                        return new PetState();
                    }

                    // Backwards-compat: older saves predate the World field.
                    if (state.World == null)
                    {
                        state.World = new WorldState();
                    }

                    state.UpdateRealTime();
                    return state;
                }
                catch (Exception ex)
                {
                    Logging.Error("PetState", "Failed to load pet_state.json; starting fresh.", ex);
                    return new PetState();
                }
            }
            return new PetState();
        }

        public void Save()
        {
            try
            {
                Paths.EnsureInitialized();
                LastSeen = DateTime.Now;
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SavePath, json);
            }
            catch (Exception ex)
            {
                Logging.Warn("PetState", "Save failed", ex);
            }
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

            // Advance any active garden plots using the same offline-real-time
            // pattern. A plot may need to step forward more than once if the
            // user was away long enough to skip a stage.
            if (World != null)
            {
                AdvancePlots(DateTime.Now);
            }

            LastSeen = DateTime.Now;
        }

        /// <summary>
        /// Walks every plot and rolls its state forward to whatever <paramref name="now"/>
        /// implies. Bloom is a terminal state (waits for harvest); Empty is
        /// inert. Each transition resets <see cref="GardenPlotData.LastTransition"/>
        /// so the next stage is timed from the moment the previous one
        /// "completed" rather than from when the user came back.
        /// </summary>
        public void AdvancePlots(DateTime now)
        {
            foreach (var plot in World.Plots)
            {
                // Loop because a long absence can skip Sprout entirely.
                while (true)
                {
                    if (plot.State == GardenPlotState.Seeded
                        && now - plot.LastTransition >= GardenConstants.SeededToSproutDelay)
                    {
                        plot.LastTransition = plot.LastTransition + GardenConstants.SeededToSproutDelay;
                        plot.State = GardenPlotState.Sprout;
                        continue;
                    }
                    if (plot.State == GardenPlotState.Sprout
                        && now - plot.LastTransition >= GardenConstants.SproutToBloomDelay)
                    {
                        plot.LastTransition = plot.LastTransition + GardenConstants.SproutToBloomDelay;
                        plot.State = GardenPlotState.Bloom;
                        continue;
                    }
                    break;
                }
            }
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
