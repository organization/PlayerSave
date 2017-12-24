using fNbt;
using MiNET;
using MiNET.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerSave
{
    public static class PlayerSaveExtension
    {
        public static void Save(this Player player)
        {
            NbtCompound namedTag = new NbtCompound();
            namedTag.Add(player.GetNbtPos());
            namedTag.Add(player.GetNbtRotation());

            namedTag.Add(player.GetNbtHealth());
            namedTag.Add(player.GetNbtEffects());

            namedTag.Add(player.GetFoodNbt());
            namedTag.Add(player.GetNbtFoodExhaustionLevel());
            namedTag.Add(player.GetNbtFoodSaturationLevel());

            namedTag.Add(player.GetNbtXpLevel());
            namedTag.Add(player.GetNbtXpP());

            //https://github.com/pmmp/PocketMine-MP/blob/master/src/pocketmine/entity/Human.php#L482
            //https://github.com/pmmp/PocketMine-MP/blob/master/src/pocketmine/Player.php#L3386
        }

        static NbtList GetNbtInventory(this Player player)
        {
        }

        static NbtFloat GetNbtXpP(this Player player)
        {
            return new NbtFloat("XpP", player.Experience);
        }

        static NbtInt GetNbtXpLevel(this Player player)
        {
            return new NbtInt("XpLevel", (int)player.ExperienceLevel);
        }

        static NbtFloat GetNbtFoodSaturationLevel(this Player player)
        {
            return new NbtFloat("foodSaturationLevel", (float)player.HungerManager.Saturation);
        }

        static NbtFloat GetNbtFoodExhaustionLevel(this Player player)
        {
            return new NbtFloat("foodExhaustionLevel", (float)player.HungerManager.Exhaustion);
        }

        static NbtInt GetFoodNbt(this Player player)
        {
            return new NbtInt("foodlevel", player.HungerManager.Hunger);
        }

        static NbtList GetNbtEffects(this Player player)
        {
            NbtList nbt = new NbtList("ActiveEffects");
            foreach (Effect effect in player.Effects.Values)
            {
                NbtCompound nbtCompound = new NbtCompound();
                nbtCompound.Add(new NbtByte("Id", (byte)effect.EffectId));
                nbtCompound.Add(new NbtInt("Duration", effect.Duration));
                nbtCompound.Add(new NbtByte("ShowParticles", (byte)(effect.Particles ? 1 : 0)));
            }

            return nbt;
        }

        static NbtList GetNbtPos(this Player player)
        {
            NbtList nbt = new NbtList("Pos");
            nbt.Add(new NbtDouble(player.KnownPosition.X));
            nbt.Add(new NbtDouble(player.KnownPosition.Y));
            nbt.Add(new NbtDouble(player.KnownPosition.Z));
            return nbt;
        }

        static NbtList GetNbtRotation(this Player player)
        {
            NbtList nbt = new NbtList("Rotation");
            nbt.Add(new NbtFloat(player.KnownPosition.Yaw));
            nbt.Add(new NbtFloat(player.KnownPosition.Pitch));
            return nbt;
        }

        static NbtFloat GetNbtHealth(this Player player)
        {
            return new NbtFloat("Health", player.HealthManager.Health);
        }
    }
}
