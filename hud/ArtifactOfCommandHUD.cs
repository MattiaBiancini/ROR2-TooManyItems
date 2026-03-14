using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RoR2ItemInfo.hud
{
    public class ArtifactOfCommandHUD : BaseHUD
    {
        protected override string GetPanelName() => "ArtifactOfCommandInfoPanel";
        protected override float GetDefaultWidth() => 400f;

        public override void Init()
        {
            base.Init();
            On.RoR2.UI.PickupPickerPanel.OnCreateButton += OnPickupPickerCreateButton;
        }

        private void OnPickupPickerCreateButton(
            On.RoR2.UI.PickupPickerPanel.orig_OnCreateButton orig,
            RoR2.UI.PickupPickerPanel self,
            int index,
            RoR2.UI.MPButton button)
        {
            orig(self, index, button);

            try
            {
                // Otteniamo le opzioni del picker tramite reflection
                var optionsField = typeof(RoR2.UI.PickupPickerPanel).GetField("pickupOptions",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                
                if (optionsField == null) return;

                var options = optionsField.GetValue(self) as RoR2.PickupPickerController.Option[];
                if (options == null || index >= options.Length) return;

                #pragma warning disable CS0618
                var pickupDef = PickupCatalog.GetPickupDef(options[index].pickupIndex);
                #pragma warning restore CS0618
                if (pickupDef == null) return;

                var data = ResolveFromPickupDef(pickupDef);
                if (data == null) return;

                // Assicuriamoci che il pannello sia nel Canvas corretto per essere visibile
                EnsurePanelOnTop(self);

                var trigger = button.gameObject.GetComponent<EventTrigger>()
                           ?? button.gameObject.AddComponent<EventTrigger>();

                var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                enterEntry.callback.AddListener(_ =>
                {
                    ShowPanel(getInfoText(data));
                });
                trigger.triggers.Add(enterEntry);

                var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                exitEntry.callback.AddListener(_ =>
                {
                    HidePanel();
                });
                trigger.triggers.Add(exitEntry);
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError($"ArtifactOfCommandHUD error: {e}");
            }
        }

        private void EnsurePanelOnTop(RoR2.UI.PickupPickerPanel pickerPanel)
        {
            if (_panel == null) return;

            // Il menu del Command ha un suo Canvas. Dobbiamo spostare il nostro pannello lì
            // per evitare che finisca "sotto" il menu.
            var pickerCanvas = pickerPanel.GetComponentInParent<Canvas>();
            if (pickerCanvas != null && _panel.transform.parent != pickerCanvas.transform)
            {
                _panel.transform.SetParent(pickerCanvas.transform, false);
                
                // Riposizioniamo a destra del menu
                var rect = _panel.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.anchoredPosition = new Vector2(350f, 0f);
            }
        }

        private static ItemData? ResolveFromPickupDef(PickupDef pickupDef)
        {
            string? token = null;
            if (pickupDef.itemIndex != ItemIndex.None)
                token = ItemCatalog.GetItemDef(pickupDef.itemIndex)?.nameToken;
            else if (pickupDef.equipmentIndex != EquipmentIndex.None)
                token = EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex)?.nameToken;

            if (token != null && ItemDatabase.ByToken.TryGetValue(token, out var data))
                return data;
            return null;
        }
    }
}
