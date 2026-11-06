using System;
using System.Collections.Generic;
using TMPro;
using UI.Builder;
using UI.Windows;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CameraLocation
{
    /// <summary>
    /// The mod's in-game settings, built with the game's own panel builder. With the Mod Settings Hub installed the
    /// page is shown there; without it, Ctrl+F8 (see SettingsHotkey in Settings.xml) opens the mod's own window.
    /// Every change takes effect at once and is written back to Settings.xml.
    /// </summary>
    internal static class SettingsUi
    {
        internal const string WindowId = "exp232.cameralocation.settings";

        private static readonly List<string> CornerNames = new List<string>
        {
            "Top left", "Top right", "Bottom left", "Bottom right",
        };

        internal static void Toggle()
        {
            var windows = WindowManager.Shared;
            if (windows == null) return;   // main menu: the window system is not up yet
            EnsureRegistered(windows);
            windows.ToggleWindow(WindowId);
        }

        // The game forgets custom windows when a map unloads, so register again whenever the definition is gone.
        private static void EnsureRegistered(WindowManager windows)
        {
            if (windows.HasDefinition(WindowId)) return;
            windows.RegisterCustomWindow(new CustomWindowDefinition
            {
                Identifier = WindowId,
                Title = "Camera Location " + Main.Mod.Info.Version,
                Config = WindowConfig.Resizable(new Vector2Int(480, 460), new Vector2Int(380, 260)),
                BuilderAction = Build,
            });
        }

        // The window as the mod opens it itself: the page inside a scroll view.
        private static void Build(UIPanelBuilder builder)
        {
            try
            {
                builder.VScrollView(BuildPage);
            }
            catch (Exception e)
            {
                Main.Mod.Logger.LogException(e);
            }
        }

        /// <summary>The page itself, without a scroll view, so it also fits into the Mod Settings Hub window.</summary>
        internal static void BuildPage(UIPanelBuilder page)
        {
            try
            {
                var s = Main.Settings;
                page.AddTitle("Camera Location", "CameraLocation " + Main.Mod.Info.Version);

                page.AddSection("Readout", b =>
                {
                    b.AddFieldToggle("Show camera location", () => s.Enabled, v => { s.Enabled = v; Changed(); });
                    b.AddField("Corner", b.AddDropdown(CornerNames, (int)s.Location, i =>
                    {
                        s.Location = (Corner)Mathf.Clamp(i, 0, 3);
                        Changed();
                    }));
                    b.AddFieldToggle("Show height", () => s.ShowHeight, v => { s.ShowHeight = v; Changed(); });
                    b.AddFieldToggle("Show which camera", () => s.ShowCameraName, v => { s.ShowCameraName = v; Changed(); });
                    Slider(b, "Decimals", () => s.Decimals, v => s.Decimals = (int)v, 0, 2, 1, v => ((int)v).ToString());
                    Note(b, "X and Z are the camera's position on the map, Height is how far above the map's zero level it is (the game's own coordinates).");
                });

                page.AddSection("Look", b =>
                {
                    Slider(b, "Text size", () => s.FontSize, v => s.FontSize = (int)v, 8, 48, 1, v => ((int)v) + " px");
                    Slider(b, "Distance from side edge", () => s.MarginX, v => s.MarginX = (int)v, 0, 400, 1, v => ((int)v) + " px");
                    Slider(b, "Distance from top/bottom edge", () => s.MarginY, v => s.MarginY = (int)v, 0, 400, 1, v => ((int)v) + " px");
                    Slider(b, "Background", () => s.Background, v => s.Background = v, 0f, 1f, 0.05f, v => Mathf.RoundToInt(v * 100f) + " %");
                });

                page.AddSection("", b =>
                {
                    b.AddButton("Reset these to defaults", ResetDefaults);
                    Note(b, "Saved to Settings.xml as you change them. The readout only draws text on the screen; it never changes the game.");
                });
            }
            catch (Exception e)
            {
                Main.Mod.Logger.LogException(e);
            }
        }

        // Explanatory text: smaller and dimmer than the controls, so the sliders stay in view.
        private static void Note(UIPanelBuilder b, string text)
        {
            b.AddLabel(text, (TMP_Text t) =>
            {
                t.fontSize *= 0.75f;
                t.alpha = 0.8f;
            });
        }

        private static void Slider(UIPanelBuilder b, string label, Func<float> get, Action<float> set, float min, float max, float step, Func<float, string> format)
        {
            b.AddField(label, b.AddSliderQuantized(
                get,
                () => format(get()),
                v => set(Mathf.Clamp(Mathf.Round(v / step) * step, min, max)),
                step, min, max,
                v => Changed()));
        }

        private static void ResetDefaults()
        {
            var d = new Settings();
            var s = Main.Settings;
            s.Enabled = d.Enabled;
            s.Location = d.Location;
            s.MarginX = d.MarginX;
            s.MarginY = d.MarginY;
            s.FontSize = d.FontSize;
            s.Background = d.Background;
            s.Decimals = d.Decimals;
            s.ShowHeight = d.ShowHeight;
            s.ShowCameraName = d.ShowCameraName;
            Changed();
        }

        // A setting changed: keep it in range and save it.
        private static void Changed()
        {
            Main.Settings.Clamp();
            try
            {
                Main.Settings.Save(Main.SettingsPath);
                Main.Mod.Logger.Log("Settings saved: " + Main.Settings.Describe());
            }
            catch (Exception e)
            {
                Main.Mod.Logger.Warning("Could not save settings: " + e.Message);
            }
        }
    }

    /// <summary>Listens for the settings hotkey. Lives on its own object for as long as the mod is loaded.</summary>
    internal sealed class SettingsHotkey : MonoBehaviour
    {
        private InputAction _action;

        private void OnEnable()
        {
            try
            {
                string key = Main.Settings.SettingsHotkey;
                if (string.IsNullOrWhiteSpace(key)) return;

                _action = new InputAction("CameraLocation.Settings", InputActionType.Button);
                if (Main.Settings.SettingsHotkeyCtrl)
                {
                    _action.AddCompositeBinding("ButtonWithOneModifier")
                        .With("Modifier", "<Keyboard>/ctrl")
                        .With("Button", key);
                }
                else
                {
                    _action.AddBinding(key);
                }
                _action.performed += OnPerformed;
                _action.Enable();
            }
            catch (Exception e)
            {
                Main.Mod.Logger.Warning("Settings hotkey not available: " + e.Message);
                _action = null;
            }
        }

        private void OnPerformed(InputAction.CallbackContext context)
        {
            if (!Main.Active) return;
            try
            {
                SettingsUi.Toggle();
            }
            catch (Exception e)
            {
                Main.Mod.Logger.LogException(e);
            }
        }

        private void OnDisable()
        {
            if (_action == null) return;
            _action.performed -= OnPerformed;
            _action.Disable();
            _action.Dispose();
            _action = null;
        }
    }
}
