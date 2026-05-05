using System;
using System.IO;

namespace DesktopPal
{
    /// <summary>
    /// Resolves on-disk locations for persistent application state.
    /// 
    /// All persistent files live under <c>%LOCALAPPDATA%\DesktopPal\</c> so the
    /// app can be installed to a read-only location (Program Files / MSIX)
    /// without breaking saves. See <c>docs/design/persistence.md</c>.
    /// 
    /// On first access this class also runs a one-shot migration that copies
    /// any legacy <c>pet_state.json</c> sitting next to the executable into
    /// the new data root. The legacy file is best-effort deleted afterwards;
    /// failure to delete is not fatal. Migration only runs when no file
    /// already exists at the new path, so it is safe to call repeatedly.
    /// </summary>
    public static class Paths
    {
        private const string AppFolderName = "DesktopPal";
        private const string PetStateFileName = "pet_state.json";
        private const string LogSource = "Paths";

        private static readonly object _gate = new object();
        private static bool _initialized;

        /// <summary>
        /// Per-user, writable data root: <c>%LOCALAPPDATA%\DesktopPal\</c>.
        /// </summary>
        public static string DataRoot { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppFolderName);

        /// <summary>
        /// Canonical location for the pet save file under the new data root.
        /// </summary>
        public static string PetStatePath { get; } = Path.Combine(DataRoot, PetStateFileName);

        /// <summary>
        /// Legacy save location next to the executable. Read-only after
        /// migration; never written.
        /// </summary>
        public static string LegacyPetStatePath { get; } = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            PetStateFileName);

        /// <summary>
        /// Ensures the data root exists and runs the one-shot legacy
        /// migration. Idempotent and safe to call from any persistence
        /// entry point. Never throws.
        /// </summary>
        public static void EnsureInitialized()
        {
            if (_initialized) return;
            lock (_gate)
            {
                if (_initialized) return;
                try
                {
                    Directory.CreateDirectory(DataRoot);
                    Logging.Info(LogSource, "Data root resolved: " + DataRoot);
                    MigrateLegacyPetState();
                }
                catch (Exception ex)
                {
                    Logging.Error(LogSource, "Failed to initialize data root", ex);
                }
                finally
                {
                    // Mark initialized even on failure so we don't spam logs.
                    _initialized = true;
                }
            }
        }

        private static void MigrateLegacyPetState()
        {
            try
            {
                bool legacyExists = File.Exists(LegacyPetStatePath);
                bool newExists = File.Exists(PetStatePath);

                if (!legacyExists)
                {
                    return;
                }

                if (newExists)
                {
                    Logging.Info(LogSource,
                        "Legacy pet_state.json present next to exe but new save already exists; leaving legacy file untouched.");
                    return;
                }

                File.Copy(LegacyPetStatePath, PetStatePath, overwrite: false);
                Logging.Info(LogSource,
                    "Migrated legacy pet_state.json from '" + LegacyPetStatePath + "' to '" + PetStatePath + "'.");

                // Best-effort delete of the legacy file. If it fails (locked,
                // permission, AV), leave it in place — the new path is the
                // source of truth from this launch onward.
                try
                {
                    File.Delete(LegacyPetStatePath);
                    Logging.Info(LogSource, "Removed legacy pet_state.json after successful migration.");
                }
                catch (Exception delEx)
                {
                    Logging.Warn(LogSource,
                        "Migration copy succeeded but legacy file delete failed; leaving legacy file in place.",
                        delEx);
                }
            }
            catch (Exception ex)
            {
                Logging.Error(LogSource, "Legacy pet_state.json migration failed", ex);
            }
        }
    }
}
