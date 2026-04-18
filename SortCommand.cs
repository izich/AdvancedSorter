using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedSorter
{
    public class SortCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "sort";
        public string Help => "Сортировка инвентаря";
        public string Syntax => "/sort";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string>() { "sort.use" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = (UnturnedPlayer)caller;

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

            // очищаем
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

            player.ChatMessage("Инвентарь отсортирован");
        }
    }
}
