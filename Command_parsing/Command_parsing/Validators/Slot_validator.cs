﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Validators
{
    public class Slot_validator : Validator
    {
        private readonly HashSet<string> slots_1_13 = new() { "container.0", "container.1", "container.2", "container.3", "container.4", "container.5", "container.6", "container.7", "container.8", "container.9", "container.10", "container.11", "container.12", "container.13", "container.14", "container.15", "container.16", "container.17", "container.18", "container.19", "container.20", "container.21", "container.22", "container.23", "container.24", "container.25", "container.26", "container.27", "container.28", "container.29", "container.30", "container.31", "container.32", "container.33", "container.34", "container.35", "container.36", "container.37", "container.38", "container.39", "container.40", "container.41", "container.42", "container.43", "container.44", "container.45", "container.46", "container.47", "container.48", "container.49", "container.50", "container.51", "container.52", "container.53", "enderchest.0", "enderchest.1", "enderchest.2", "enderchest.3", "enderchest.4", "enderchest.5", "enderchest.6", "enderchest.7", "enderchest.8", "enderchest.9", "enderchest.10", "enderchest.11", "enderchest.12", "enderchest.13", "enderchest.14", "enderchest.15", "enderchest.16", "enderchest.17", "enderchest.18", "enderchest.19", "enderchest.20", "enderchest.21", "enderchest.22", "enderchest.23", "enderchest.24", "enderchest.25", "enderchest.26", "horse.0", "horse.1", "horse.2", "horse.3", "horse.4", "horse.5", "horse.6", "horse.7", "horse.8", "horse.9", "horse.10", "horse.11", "horse.12", "horse.13", "horse.14", "hotbar.0", "hotbar.1", "hotbar.2", "hotbar.3", "hotbar.4", "hotbar.5", "hotbar.6", "hotbar.7", "hotbar.8", "inventory.0", "inventory.1", "inventory.2", "inventory.3", "inventory.4", "inventory.5", "inventory.6", "inventory.7", "inventory.8", "inventory.9", "inventory.10", "inventory.11", "inventory.12", "inventory.13", "inventory.14", "inventory.15", "inventory.16", "inventory.17", "inventory.18", "inventory.19", "inventory.20", "inventory.21", "inventory.22", "inventory.23", "inventory.24", "inventory.25", "inventory.26", "villager.0", "villager.1", "villager.2", "villager.3", "villager.4", "villager.5", "villager.6", "villager.7", "armor.chest", "armor.feet", "armor.head", "armor.legs", "horse.armor", "horse.chest", "horse.saddle", "weapon", "weapon.mainhand", "weapon.offhand" };
        private readonly HashSet<string> slots_1_20_5 = new() {"player.cursor","player.crafting.0","player.crafting.1","player.crafting.2","player.crafting.3","contents"};
        private readonly HashSet<string> multiple_slots = new() { "armor.*", "container.*", "horse.*", "hotbar.*", "inventory.*", "player.crafting.*", "villager.*", "weapon.*" };

        private readonly bool has_1_20_5;
        public Slot_validator(bool has_1_20_5)
        {
            this.has_1_20_5 = has_1_20_5;
        }

        public override void Validate(Command command, object external_data, string validator_params, out string error)
        {
            string slot = (string)external_data;

            if (slots_1_13.Contains(slot))
            {
                error = "";
                return;
            }

            if(has_1_20_5 && slots_1_20_5.Contains(slot))
            {
                error = "";
                return;
            }

            if(validator_params == "multiple")
            {
                if(multiple_slots.Contains(slot))
                {
                    error = "";
                    return;
                }
            }

            error = slot + " is not a valid slot";
            return;
        }
    }
}
