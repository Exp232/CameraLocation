using System;
using System.IO;
using System.Text.RegularExpressions;
using Railroader.Modding;
using UnityEngine;

namespace CameraLocation
{
    /// <summary>
    /// Entry point for the game's own mod loader: Info.json lists this assembly under "assemblies", and the game
    /// creates this class and calls OnModLoad once at start-up. It must stay public with a parameterless constructor.
    /// </summary>
    public sealed class ModEntryPoint : IModEntryPoint
    {
        public void OnModLoad(IModContext context)
        {
            Main.Load(context);
        }

        public void OnModUnload()
        {
            Main.Shutdown();
        }
    }

    internal sealed class ModLogger
    {
        private readonly IModLogger _log;

        internal ModLogger(IModLogger log)
        {
            _log = log;
        }

        public void Log(string message) => _log.Info(message);
        public void Warning(string message) => _log.Warning(message);
        public void Error(string message) => _log.Error(message);
        public void LogException(Exception e) => _log.Error(e, e.Message);
    }

    internal sealed class ModInfo
    {
        public string Id;
        public string Version;
    }

    /// <summary>What the mod knows about itself: log, version (from Info.json) and folder.</summary>
    internal sealed class ModHandle
    {
        public ModLogger Logger;
        public ModInfo Info;
        public string Path;
    }

    public static class Main
    {
        internal static ModHandle Mod;
        internal static Settings Settings;

        /// <summary>True while the mod is running.</summary>
        internal static bool Active;

        internal static string SettingsPath => Path.Combine(Mod.Path, "Settings.xml");

        private static GameObject _runner;

        internal static void Load(IModContext context)
        {
            Mod = new ModHandle
            {
                Logger = new ModLogger(context.Log),
                Info = new ModInfo { Id = context.ModIdentifier, Version = ReadVersion(context.ModDirectory) },
                Path = context.ModDirectory,
            };

            Settings = Settings.Load(SettingsPath, out string problem);
            if (problem != null) Mod.Logger.Warning("Settings: " + problem);

            try
            {
                _runner = new GameObject("CameraLocation.Runner");
                UnityEngine.Object.DontDestroyOnLoad(_runner);
                _runner.AddComponent<CameraOverlay>();
                _runner.AddComponent<SettingsHotkey>();
            }
            catch (Exception e)
            {
                Mod.Logger.Warning("Runner could not be set up: " + e.Message);
                return;
            }
            Active = true;

            // Puts this mod's page into the Mod Settings Hub when that is installed (found by reflection, in either
            // load order); without it the mod opens its own window with the hotkey.
            HubLink.Register(context.ModIdentifier, "Camera Location", Mod.Info.Version, SettingsUi.BuildPage);
        }

        internal static void Shutdown()
        {
            Active = false;
            HubLink.Unregister();
            if (_runner != null)
            {
                UnityEngine.Object.Destroy(_runner);
                _runner = null;
            }
        }

        private static string ReadVersion(string modDirectory)
        {
            try
            {
                string json = File.ReadAllText(Path.Combine(modDirectory, "Info.json"));
                Match m = Regex.Match(json, "\"version\"\\s*:\\s*\"([^\"]*)\"", RegexOptions.IgnoreCase);
                if (m.Success) return m.Groups[1].Value;
            }
            catch (Exception)
            {
            }
            return "?";
        }
    }
}
