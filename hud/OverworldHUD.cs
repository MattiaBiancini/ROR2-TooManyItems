using RoR2;
using RoR2.UI;
using UnityEngine;

namespace RoR2ItemInfo.hud
{
    public class OverworldHUD : BaseHUD
    {
        private string? _lastItemId;
        private GameObject? _currentTarget;

        public override void Init()
        {
            base.Init();
            On.RoR2.Interactor.FindBestInteractableObject += OnFindBestInteractableObject;
            On.RoR2.UI.HUD.Update += OnHUDUpdate;
        }

        protected override string GetPanelName() => "OverworldItemInfoPanel";

        protected override void OnHUDUpdate(On.RoR2.UI.HUD.orig_Update orig, RoR2.UI.HUD self)
        {
            base.OnHUDUpdate(orig, self);

            // Se non abbiamo un target in questo frame, nascondiamo il pannello
            if (_currentTarget == null)
                HidePanel();
            
            _currentTarget = null;
        }

        private GameObject? OnFindBestInteractableObject(
            On.RoR2.Interactor.orig_FindBestInteractableObject orig,
            RoR2.Interactor self,
            Ray raycastRay,
            float maxRaycastDistance,
            Vector3 overlapPosition,
            float overlapRadius)
        {
            // Aumentiamo leggermente la distanza per rendere il tooltip più reattivo
            var target = orig(self, raycastRay, maxRaycastDistance * 2f, overlapPosition, overlapRadius);

            try
            {
                if (target == null) return target;

                var data = GetItemDataFromTarget(target);
                if (data == null) 
                { 
                    HidePanel(); 
                    return target; 
                }

                _currentTarget = target;

                // Se è lo stesso oggetto di prima, assicuriamoci solo che sia attivo
                if (_lastItemId == data.ItemId && _panel != null)
                {
                    _panel.SetActive(true);
                    return target;
                }

                _lastItemId = data.ItemId;
                ShowPanel(getInfoText(data));
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError($"OverworldHUD error: {e}");
            }

            return target;
        }

        private static ItemData? GetItemDataFromTarget(GameObject target)
        {
            #pragma warning disable CS0618
            var pickup = target.GetComponent<GenericPickupController>();
            if (pickup != null)
            {
                var pickupDef = PickupCatalog.GetPickupDef(pickup.pickupIndex);
                if (pickupDef != null) return ResolveFromPickupDef(pickupDef);
            }

            var shop = target.GetComponent<ShopTerminalBehavior>();
            if (shop != null)
            {
                var pickupDef = PickupCatalog.GetPickupDef(shop.CurrentPickupIndex());
                if (pickupDef != null) return ResolveFromPickupDef(pickupDef);
            }
            #pragma warning restore CS0618

            return null;
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
