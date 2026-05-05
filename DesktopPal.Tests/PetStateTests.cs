using System;
using System.IO;
using DesktopPal;
using Xunit;

namespace DesktopPal.Tests
{
    /// <summary>
    /// Unit tests for <see cref="PetState"/> covering stat clamping, decay,
    /// level progression, and JSON persistence.
    /// </summary>
    public class PetStateTests : IDisposable
    {
        // ── Helpers ──────────────────────────────────────────────────────────────
        private readonly string _tempDir;

        public PetStateTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        // ── Clamp ────────────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(50.0, 50.0)]
        [InlineData(100.0, 100.0)]
        [InlineData(-10.0, 0.0)]
        [InlineData(110.0, 100.0)]
        public void Clamp_ReturnsCorrectBounds(double input, double expected)
        {
            Assert.Equal(expected, PetState.Clamp(input));
        }

        // ── AddExperience ────────────────────────────────────────────────────────

        [Fact]
        public void AddExperience_IncrementsExperience()
        {
            var state = new PetState();
            state.AddExperience(10.0);
            Assert.Equal(10.0, state.Experience);
        }

        [Fact]
        public void AddExperience_NegativeAmount_DoesNothing()
        {
            var state = new PetState();
            state.AddExperience(-5.0);
            Assert.Equal(0.0, state.Experience);
        }

        [Fact]
        public void AddExperience_ZeroAmount_DoesNothing()
        {
            var state = new PetState();
            state.AddExperience(0.0);
            Assert.Equal(0.0, state.Experience);
        }

        // ── Tick – hatching ──────────────────────────────────────────────────────

        [Fact]
        public void Tick_BeforeHatch_DoesNotDecayStats()
        {
            var state = new PetState { IsHatched = false, BirthTime = DateTime.Now };
            double hungerBefore = state.Hunger;
            state.Tick();
            Assert.Equal(hungerBefore, state.Hunger);
        }

        [Fact]
        public void Tick_AfterOneMinute_HatchesPet()
        {
            var state = new PetState
            {
                IsHatched = false,
                BirthTime = DateTime.Now.AddMinutes(-2)
            };
            state.Tick();
            Assert.True(state.IsHatched);
        }

        // ── Tick – stat decay ────────────────────────────────────────────────────

        [Fact]
        public void Tick_AfterHatch_DecaysHunger()
        {
            var state = new PetState { IsHatched = true, Hunger = 50.0 };
            state.Tick();
            Assert.True(state.Hunger < 50.0);
        }

        [Fact]
        public void Tick_HungerAtZero_StaysAtZero()
        {
            var state = new PetState { IsHatched = true, Hunger = 0.0 };
            state.Tick();
            Assert.Equal(0.0, state.Hunger);
        }

        [Fact]
        public void Tick_LowHunger_AcceleratesHappinessDecay()
        {
            // Happiness should decay faster when Hunger < 20
            var stateHungry = new PetState { IsHatched = true, Hunger = 5.0, Happiness = 50.0 };
            var stateNormal = new PetState { IsHatched = true, Hunger = 80.0, Happiness = 50.0 };

            stateHungry.Tick();
            stateNormal.Tick();

            Assert.True(stateHungry.Happiness < stateNormal.Happiness,
                "Happy decay should be faster when Hunger < 20.");
        }

        // ── Level progression ────────────────────────────────────────────────────

        [Fact]
        public void Tick_SufficientExperience_LevelsUp()
        {
            // Need Experience >= Level * 100 for level-up.
            var state = new PetState { IsHatched = true, Level = 1, Experience = 100.0 };
            state.Tick();
            Assert.Equal(2, state.Level);
        }

        // ── UpdateRealTime ───────────────────────────────────────────────────────

        [Fact]
        public void UpdateRealTime_ZeroElapsed_DoesNotDecay()
        {
            var state = new PetState { LastSeen = DateTime.Now, Hunger = 80.0 };
            state.UpdateRealTime();
            // At most a tiny epsilon change due to DateTime.Now drift
            Assert.True(state.Hunger > 79.9);
        }

        [Fact]
        public void UpdateRealTime_TwoHoursOffline_DecaysStats()
        {
            var state = new PetState
            {
                LastSeen = DateTime.Now.AddHours(-2),
                Hunger = 100.0,
                Hygiene = 100.0,
                Happiness = 100.0,
                Energy = 100.0
            };
            state.UpdateRealTime();

            // 2 h * 5.0/h = 10 units of hunger lost
            Assert.Equal(90.0, state.Hunger, precision: 1);
            // 2 h * 3.0/h = 6 units of hygiene lost
            Assert.Equal(94.0, state.Hygiene, precision: 1);
        }

        [Fact]
        public void UpdateRealTime_TenMinutesOffline_HatchesPet()
        {
            var state = new PetState
            {
                IsHatched = false,
                LastSeen = DateTime.Now.AddMinutes(-11)
            };
            state.UpdateRealTime();
            Assert.True(state.IsHatched);
        }

        [Fact]
        public void UpdateRealTime_StatsCannotGoBelowZero()
        {
            var state = new PetState
            {
                LastSeen = DateTime.Now.AddHours(-100), // extreme
                Hunger = 100.0,
                Hygiene = 100.0,
                Happiness = 100.0,
                Energy = 100.0
            };
            state.UpdateRealTime();

            Assert.Equal(0.0, state.Hunger);
            Assert.Equal(0.0, state.Hygiene);
            Assert.Equal(0.0, state.Happiness);
            Assert.Equal(0.0, state.Energy);
        }

        // ── Default values ───────────────────────────────────────────────────────

        [Fact]
        public void NewPetState_HasSensibleDefaults()
        {
            var state = new PetState();
            Assert.Equal("Buddy", state.Name);
            Assert.Equal(1, state.Level);
            Assert.Equal(0.0, state.Experience);
            Assert.False(state.IsHatched);
            Assert.Equal(100.0, state.Hunger);
            Assert.Equal(100.0, state.Happiness);
            Assert.Equal(100.0, state.Hygiene);
            Assert.Equal(100.0, state.Energy);
        }
    }
}
