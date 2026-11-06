using System;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace CameraLocation
{
    /// <summary>
    /// Draws the camera's map position in a corner of the screen. The text is rebuilt ten times a second (not every
    /// frame) and drawn with Unity's immediate-mode GUI, which sits on top of the game's own interface and never
    /// takes a click.
    /// </summary>
    internal sealed class CameraOverlay : MonoBehaviour
    {
        private const float RefreshSeconds = 0.1f;

        private static readonly GUIContent Content = new GUIContent();

        private string _text = "";
        private float _next;
        private bool _failed;
        private GUIStyle _style;
        private int _styleSize;

        private void Update()
        {
            if (!Main.Active || _failed || Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + RefreshSeconds;
            try
            {
                _text = Main.Settings.Enabled ? Read(Main.Settings) : "";
            }
            catch (Exception e)
            {
                // One log line, then stop: a broken readout must never cost frames or spam the log.
                _failed = true;
                _text = "";
                Main.Mod.Logger.LogException(e);
            }
        }

        /// <summary>The text for the current camera, or "" when there is none (main menu, map loading).</summary>
        private static string Read(Settings s)
        {
            CameraSelector selector = CameraSelector.shared;
            if (selector == null) return "";
            Vector3 p = selector.CurrentCameraPosition;
            if (p == Vector3.zero) return "";   // the game returns zero while no camera is selected

            string format = s.Decimals <= 0 ? "0" : (s.Decimals == 1 ? "0.0" : "0.00");
            CultureInfo c = CultureInfo.InvariantCulture;
            var sb = new StringBuilder(64);
            if (s.ShowCameraName)
            {
                sb.Append(NameOf(selector.CurrentCameraIdentifier)).Append(" camera").Append('\n');
            }
            sb.Append("X ").Append(p.x.ToString(format, c));
            sb.Append("   Z ").Append(p.z.ToString(format, c));
            if (s.ShowHeight) sb.Append("   Height ").Append(p.y.ToString(format, c));
            return sb.ToString();
        }

        private static string NameOf(CameraSelector.CameraIdentifier id)
        {
            switch (id)
            {
                case CameraSelector.CameraIdentifier.FirstPerson: return "First-person";
                case CameraSelector.CameraIdentifier.Strategy: return "Strategy";
                case CameraSelector.CameraIdentifier.Dispatcher: return "Dispatcher";
                case CameraSelector.CameraIdentifier.Fly: return "Fly";
                default: return id.ToString();
            }
        }

        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint || _text.Length == 0 || !Main.Active) return;
            try
            {
                Settings s = Main.Settings;
                bool right = s.Location == Corner.TopRight || s.Location == Corner.BottomRight;
                bool bottom = s.Location == Corner.BottomLeft || s.Location == Corner.BottomRight;

                // GUI.skin can only be used inside OnGUI, so the style is made here and rebuilt when the size changes.
                if (_style == null || _styleSize != s.FontSize)
                {
                    _style = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = s.FontSize,
                        wordWrap = false,
                        richText = false,
                        padding = new RectOffset(8, 8, 4, 4),
                    };
                    _style.normal.textColor = Color.white;
                    _styleSize = s.FontSize;
                }
                _style.alignment = right ? TextAnchor.UpperRight : TextAnchor.UpperLeft;

                Content.text = _text;
                Vector2 size = _style.CalcSize(Content);
                float x = right ? Screen.width - s.MarginX - size.x : s.MarginX;
                float y = bottom ? Screen.height - s.MarginY - size.y : s.MarginY;
                var rect = new Rect(x, y, size.x, size.y);

                if (s.Background > 0f)
                {
                    Color old = GUI.color;
                    GUI.color = new Color(0f, 0f, 0f, s.Background);
                    GUI.DrawTexture(rect, Texture2D.whiteTexture);
                    GUI.color = old;
                }
                GUI.Label(rect, _text, _style);
            }
            catch (Exception e)
            {
                _failed = true;
                _text = "";
                Main.Mod.Logger.LogException(e);
            }
        }
    }
}
