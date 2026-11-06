using System;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace CameraLocation
{
    /// <summary>Screen corner for the readout. The order matters: the settings window maps its drop-down index to it.</summary>
    public enum Corner
    {
        TopLeft = 0,
        TopRight = 1,
        BottomLeft = 2,
        BottomRight = 3,
    }

    /// <summary>Kept in Settings.xml in the mod folder.</summary>
    public class Settings
    {
        /// <summary>Master switch for the readout.</summary>
        public bool Enabled = true;

        /// <summary>Which corner of the screen the readout sits in.</summary>
        public Corner Location = Corner.TopLeft;

        /// <summary>Distance from the left or right screen edge, in pixels.</summary>
        public int MarginX = 12;

        /// <summary>Distance from the top or bottom screen edge, in pixels.</summary>
        public int MarginY = 12;

        /// <summary>Text size in pixels.</summary>
        public int FontSize = 16;

        /// <summary>Opacity of the dark box behind the text, 0 (none) to 1.</summary>
        public float Background = 0.45f;

        /// <summary>Digits after the decimal point, 0 to 2.</summary>
        public int Decimals = 0;

        /// <summary>Show the camera height as well as the map position.</summary>
        public bool ShowHeight = true;

        /// <summary>Show which camera is active (first person, strategy, dispatcher, fly).</summary>
        public bool ShowCameraName = true;

        /// <summary>Opens the mod's own settings window: an Input System binding path, empty for none.</summary>
        public string SettingsHotkey = "<Keyboard>/f8";

        /// <summary>Hold Ctrl with SettingsHotkey (the default, so the key cannot clash with the game's own).</summary>
        public bool SettingsHotkeyCtrl = true;

        internal string Describe()
        {
            var c = CultureInfo.InvariantCulture;
            return "enabled=" + Enabled + ";corner=" + Location + ";margin=" + MarginX + "," + MarginY + ";font=" + FontSize
                + ";background=" + Background.ToString("0.##", c) + ";decimals=" + Decimals
                + ";height=" + ShowHeight + ";cameraName=" + ShowCameraName;
        }

        /// <summary>Keeps every value in a range the overlay can draw.</summary>
        internal void Clamp()
        {
            if (!Enum.IsDefined(typeof(Corner), Location)) Location = Corner.TopLeft;
            MarginX = Math.Max(0, Math.Min(400, MarginX));
            MarginY = Math.Max(0, Math.Min(400, MarginY));
            FontSize = Math.Max(8, Math.Min(48, FontSize));
            Background = Math.Max(0f, Math.Min(1f, Background));
            Decimals = Math.Max(0, Math.Min(2, Decimals));
        }

        /// <summary>Writes the file through a temporary copy, so a failed write cannot leave half a file.</summary>
        internal void Save(string path)
        {
            string temp = path + ".tmp";
            using (var stream = File.Create(temp)) new XmlSerializer(typeof(Settings)).Serialize(stream, this);
            if (File.Exists(path)) File.Delete(path);
            File.Move(temp, path);
        }

        /// <summary>Reads the file, or returns defaults (and writes them out when there is no file yet).</summary>
        internal static Settings Load(string path, out string problem)
        {
            problem = null;
            var serializer = new XmlSerializer(typeof(Settings));
            if (File.Exists(path))
            {
                try
                {
                    using (var stream = File.OpenRead(path))
                    {
                        var s = (Settings)serializer.Deserialize(stream);
                        s.Clamp();
                        return s;
                    }
                }
                catch (Exception e)
                {
                    // Leave the user's file alone and run on defaults.
                    problem = "could not read " + path + ", using defaults: " + e.Message;
                    return new Settings();
                }
            }
            var settings = new Settings();
            try
            {
                using (var stream = File.Create(path)) serializer.Serialize(stream, settings);
            }
            catch (Exception e)
            {
                problem = "could not write " + path + ": " + e.Message;
            }
            return settings;
        }
    }
}
