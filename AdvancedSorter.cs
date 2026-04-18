using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using Rocket.Unturned.Events;
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
            EffectManager.onEffectButtonClicked += OnEffectButtonClicked;
        }

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= OnJoin;
            EffectManager.onEffectButtonClicked -= OnEffectButtonClicked;
        }

        private void OnJoin(UnturnedPlayer player)
        {
            EffectManager.sendUIEffect(EFFECT_ID, EFFECT_ID, player.CSteamID, true);
        }

        private void OnEffectButtonClicked(Player player, string buttonName)
        {
            if (buttonName != "sort_btn") return;

            var uPlayer = UnturnedPlayer.FromPlayer(player);
            Sort(uPlayer);
            uPlayer.ChatMessage("Инвентарь отсортирован");
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
                    items.Add(inv.getItem(page, i).item);
                }

                inv.removeItems(page);
            }

            // 🔥 стаки
            items = MergeStacks(items);

            // 🧠 сортировка
            var sorted = items
                .OrderBy(i => GetType(i))
                .ThenBy(i => i.id)
                .ToList();

            foreach (var item in sorted)
            {
                if (!inv.tryAddItem(item, true))
                    player.GiveItem(item.id, item.amount);
            }
        }

        private static List<Item> MergeStacks(List<Item> items)
        {
            var grouped = items.GroupBy(i => i.id);
            List<Item> result = new List<Item>();

            foreach (var group in grouped)
            {
                ushort id = group.Key;
                byte total = (byte)group.Sum(x => x.amount);

                var asset = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                byte max = asset.amount;

                while (total > 0)
                {
                    byte take = (byte)(total > max ? max : total);
                    result.Add(new Item(id, take, 100));
                    total -= take;
                }
            }

            return result;
        }

        private static int GetType(Item item)
        {
            var asset = (ItemAsset)Assets.find(EAssetType.ITEM, item.id);

            if (asset is ItemGunAsset) return 0;
            if (asset is ItemMagazineAsset) return 1;
            if (asset is ItemMedicalAsset) return 2;
            if (asset is ItemFoodAsset) return 3;
            if (asset is ItemToolAsset) return 4;

            return 5;
        }
    }
}
