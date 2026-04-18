using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using Rocket.Unturned.Events;
using Rocket.Unturned.Chat;
using Rocket.Unturned;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedSorter
{
    public class AdvancedSorterPlugin : RocketPlugin
    {
        private const ushort EFFECT_ID = 7777;

        protected override void Load()
        {
            U.Events.OnPlayerConnected += OnJoin;
            EffectManager.onEffectButtonClicked += OnButtonClicked;
        }

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= OnJoin;
            EffectManager.onEffectButtonClicked -= OnButtonClicked;
        }

        private void OnJoin(UnturnedPlayer player)
        {
            EffectManager.sendUIEffect(
                EFFECT_ID,
                EFFECT_ID,
                player.CSteamID, // ✅ без NetTransport
                true
            );
        }

        private void OnButtonClicked(Player player, string buttonName)
        {
            if (buttonName != "sort_btn") return;

            var uPlayer = UnturnedPlayer.FromPlayer(player);

            Sort(uPlayer);

            UnturnedChat.Say(uPlayer, "Инвентарь отсортирован");
        }

        public static void Sort(UnturnedPlayer player)
        {
            var inv = player.Player.inventory;
            List<Item> items = new List<Item>();

            for (byte page = 0; page < PlayerInventory.PAGES; page++)
            {
                byte count = inv.getItemCount(page);

                for (byte i = 0; i < count; i++)
                {
                    var jar = inv.getItem(page, i);
                    if (jar != null)
                        items.Add(jar.item);
                }
            }

            // очистка (старый API)
            for (byte page = 0; page < PlayerInventory.PAGES; page++)
            {
                inv.removeItems(page, 0);
            }

            var sorted = items
                .OrderBy(i => i.id)
                .ToList();

            foreach (var item in sorted)
            {
                if (!inv.tryAddItem(item, true))
                {
                    player.GiveItem(item.id, item.amount);
                }
            }
        }
    }
}
