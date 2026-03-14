using BepInEx;
using BepInEx.Logging;
using RoR2ItemInfo.hud;

namespace RoR2ItemInfo
{
    [BepInPlugin("me.garnet14.tooManyItems", "Too Many Items", "1.0.0")]
	public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log = null!;

		private static BaseHUD? _overworldHUD;
		private static BaseHUD? _commandHUD;

        private void Awake()
		{
			Log = Logger;

			ItemDatabase.Load();

			_overworldHUD = new OverworldHUD();
			_overworldHUD.Init();

			_commandHUD = new ArtifactOfCommandHUD();
			_commandHUD.Init();

			Log.LogInfo("RoR2 Item Info loaded!");

		}

    }
}