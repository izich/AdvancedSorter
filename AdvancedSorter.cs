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
        protected override void Load()
        {
            U.Events.OnPlayerConnected += OnJoin;
        }

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= OnJoin;
        }

        private void OnJoin(UnturnedPlayer player)
        {
            UnturnedChat.Say(player, "Напиши /sort для сортировки");
        }

        // 👉 КОМАНДА вместо UI
        [RocketCommand("sort")]
        public void SortCommand(IRocketPlayer caller, string[] command)
        {
            var player = caller as UnturnedPlayer;
            if (player == null) return;

            Sort(player);
            UnturnedChat.Say(player, "Инвентарь отсортирован");
        }

        public static void Sort(UnturnedPlayer player)
        {
            var inv = player.Player.inventory;
            List<Item> items = new List<Item>();

            // собираем предметы
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

            // очищаем (по-старому API)
            for (byte page = 0; page < PlayerInventory.PAGES; page++)
            {
                inv.removeItems(page, 0);
            }

            // сортировка
            var sorted = items
                .OrderBy(i => i.id)
                .ToList();

            // возвращаем
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
