using UnityEngine;

namespace com.Plapamaru.Shared.Debugging
{
    public class DebugFPS : MonoBehaviour
    {
        [SerializeField] private float _refreshInterval = 0.5f;
        [SerializeField] private int _warningThreshold = 30;
        [SerializeField] private int _criticalThreshold = 15;

        private GUIStyle _shadowStyle;
        private GUIStyle _textStyle;
        private int _fontSize;
        private float _elapsed;
        private int _frames;
        private int _fps;
        private string _label = "0";

        public int FPS => _fps;

        private void Update()
        {
            _elapsed += Time.unscaledDeltaTime;
            _frames++;
            if (_elapsed < _refreshInterval)
                return;

            _fps = Mathf.RoundToInt(_frames / _elapsed);
            _label = _fps.ToString();
            _frames = 0;
            _elapsed = 0f;
        }

        private void OnGUI()
        {
            EnsureStyles();

            Rect rect = new Rect(0.05f * Screen.width, 0.02f * Screen.height, Screen.width, _fontSize);
            GUI.Label(rect, _label, _shadowStyle);

            float offset = 0.07f * _fontSize;
            rect.x -= offset;
            rect.y -= offset;
            _textStyle.normal.textColor = _fps < _criticalThreshold ? Color.red : _fps < _warningThreshold ? Color.yellow : Color.green;
            GUI.Label(rect, _label, _textStyle);
        }

        private void EnsureStyles()
        {
            int fontSize = Mathf.Max(10, Screen.height * 3 / 100);
            if (_textStyle != null && fontSize == _fontSize)
                return;

            _fontSize = fontSize;
            _shadowStyle = new GUIStyle
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.black }
            };
            _textStyle = new GUIStyle(_shadowStyle);
        }
    }
}
