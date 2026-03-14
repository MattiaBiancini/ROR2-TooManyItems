using System.Text;
using System.Collections.Generic;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2ItemInfo.hud
{
    public abstract class BaseHUD
    {
        protected GameObject? _panel;
        protected HGTextMeshProUGUI? _panelText;
        protected HUD? _hud;
        protected virtual float GetDefaultWidth() => 360f;

        public virtual void Init()
        {
            On.RoR2.UI.HUD.Awake += OnHUDAwake;
        }

        private void OnHUDAwake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);
            _hud = self;
            CreatePanel();
        }

        protected virtual void OnHUDUpdate(On.RoR2.UI.HUD.orig_Update orig, RoR2.UI.HUD self)
        {
            orig(self);
            if (_panel != null && !_panel)
            {
                _panel = null;
                _panelText = null;
            }
        }

        protected virtual void CreatePanel(float posX = 8f, float posY = 0f, float width = 360f, float height = 200f)
        {
            if (_hud == null) return;

            var upperLeft = _hud.transform.Find("MainContainer/MainUIArea/SpringCanvas/UpperLeftCluster");
            if (upperLeft == null) return;

            _panel = GameObject.Instantiate(upperLeft.gameObject, upperLeft.parent);
            _panel.name = GetPanelName();

            CleanupPanel();
            SetupPositionAndSize(posX, posY, width, height);
            SetupText();

            _panel.SetActive(false);
        }

        protected abstract string GetPanelName();

        protected virtual void CleanupPanel()
        {
            if (_panel == null) return;

            var children = new List<GameObject>();
            foreach (Transform child in _panel.transform)
                children.Add(child.gameObject);
            foreach (var child in children)
                GameObject.DestroyImmediate(child);

            var layout = _panel.GetComponent<VerticalLayoutGroup>();
            if (layout != null) GameObject.DestroyImmediate(layout);

			var fitter = _panel.GetComponent<ContentSizeFitter>();
			if (fitter != null) GameObject.DestroyImmediate(fitter);

            var canvas = _panel.GetComponent<Canvas>();
            if (canvas != null) GameObject.DestroyImmediate(canvas);

            var image = _panel.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0.75f);
            }
        }

        protected virtual void SetupPositionAndSize(float posX, float posY, float width, float height)
        {
            if (_panel == null) return;
            var rect = _panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(posX, posY);
            rect.sizeDelta = new Vector2(width, height);
        }

        protected virtual void SetupText()
        {
            if (_panel == null || _hud == null) return;

            var textObj = new GameObject("PanelText");
            textObj.transform.SetParent(_panel.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(12f, 12f);
            textRect.offsetMax = new Vector2(-12f, -12f);

            _panelText = textObj.AddComponent<HGTextMeshProUGUI>();
            _panelText.fontSize = 14f;
            _panelText.enableWordWrapping = true;
            _panelText.color = Color.white;

            var existingText = _hud.GetComponentInChildren<HGTextMeshProUGUI>();
            if (existingText != null)
                _panelText.font = existingText.font;
        }

        protected virtual string getInfoText(ItemData data)
        {
            var sb = new StringBuilder();
            sb.Append($"<color=#FFFFFF><b>    {data.Name}</b></color>");
            sb.Append($"<color=#CDCDCD>\n{data.Description}</color>");

            if (data.Stats != null && data.Stats.Count > 0)
            {
				sb.Append("\n\n<color=#61CCE8><b>    Stats</b></color>");

                foreach (var stat in data.Stats)
                {
                    sb.Append($"\n<color=#E5C962>{stat.Stat}</color>: {stat.Value}");
                    if (!string.IsNullOrEmpty(stat.Add))
                        sb.Append($"<color=#CDCDCD>  ({stat.Add} per stack)</color>");
                }

            }
            return sb.ToString();
        }

        public virtual void ShowPanel(string content)
		{
			if (_panel == null || _panelText == null) return;

			_panel.SetActive(true);

			_panelText.text = content;
			
			_panelText.ForceMeshUpdate();
			
			float panelHeight = _panelText.preferredHeight + 24f;
			
			var rect = _panel.GetComponent<RectTransform>();
			rect.sizeDelta = new Vector2(GetDefaultWidth(), panelHeight);

			LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
		}

        public virtual void HidePanel()
        {
            if (_panel != null && _panel.activeSelf)
                _panel.SetActive(false);
        }
    }
}
