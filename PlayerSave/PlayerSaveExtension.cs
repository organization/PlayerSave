using fNbt;
using MiNET;
using MiNET.Effects;
using MiNET.Items;
using MiNET.Net;
using MiNET.Utils;
using MiNET.Worlds;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlayerSave
{
    public static class PlayerSaveExtension
    {
        public static void Load(this Player player)
        {
            try
            {
                string path = Config.GetProperty("PluginDirectory", ".\\") + "\\PlayerSave\\players\\" + player.PlayerInfo.Username.ToLower() + ".dat";
                NbtFile file = new NbtFile();

                file.LoadFromFile(path, NbtCompression.ZLib, null);

                NbtCompound nbt = file.RootTag;

                NbtString levelName = nbt["Level"] as NbtString;
                Level level = player.GetServer().LevelManager.Levels.Find(obj =>
                {
                    return obj.LevelName == levelName.Value;
                });
                if (level == null) return;
                NbtList pos = nbt["Pos"] as NbtList;
                NbtList rotation = nbt["Rotation"] as NbtList;
                player.SpawnLevel(level, new PlayerLocation(pos[0].DoubleValue, pos[1].DoubleValue, pos[2].DoubleValue, 0, rotation[0].FloatValue, rotation[1].FloatValue));

                player.HealthManager.Health = (int)nbt["Health"].FloatValue;

                //이펙트 보류
                //NbtList effects = nbt["ActiveEffects"] as NbtList;

                //foreach (NbtTag tag in effects)
                //{
                //    NbtCompound effectNbt = tag as NbtCompound;
                //}

                player.HungerManager.Hunger = nbt["foodlevel"].IntValue;
                player.HungerManager.Saturation = nbt["foodSaturationLevel"].FloatValue;
                player.HungerManager.Exhaustion = nbt["foodExhaustionLevel"].FloatValue;

                player.Experience = nbt["XpP"].FloatValue;
                player.ExperienceLevel = nbt["XpLevel"].IntValue;

                NbtList inventoryList = nbt["Inventory"] as NbtList;

                for (int i = 0; i < inventoryList.Count; i++)
                {
                    NbtCompound invNbt = inventoryList[i] as NbtCompound;
                    byte slot = invNbt["Slot"].ByteValue;
                    if (slot < 100)
                    {
                        if (player.Inventory.Slots.Count > i)
                            player.Inventory.SetInventorySlot(slot, ItemFactory.GetItem(invNbt["id"].ShortValue, invNbt["Damage"].ShortValue, invNbt["Count"].ByteValue));
                    }
                    else
                    {
                        switch (slot)
                        {
                            case 100:
                                player.Inventory.Helmet = ItemFactory.GetItem(invNbt["id"].ShortValue, invNbt["Damage"].ShortValue, invNbt["Count"].ByteValue);
                                break;
                            case 101:
                                player.Inventory.Chest = ItemFactory.GetItem(invNbt["id"].ShortValue, invNbt["Damage"].ShortValue, invNbt["Count"].ByteValue);
                                break;
                            case 102:
                                player.Inventory.Leggings = ItemFactory.GetItem(invNbt["id"].ShortValue, invNbt["Damage"].ShortValue, invNbt["Count"].ByteValue);
                                break;
                            case 103:
                                player.Inventory.Boots = ItemFactory.GetItem(invNbt["id"].ShortValue, invNbt["Damage"].ShortValue, invNbt["Count"].ByteValue);
                                break;
                        }
                    }
                }
                player.Inventory.InHandSlot = nbt["SelectedInventorySlot"].IntValue;
                player.SetGameMode((GameMode)nbt["playerGameType"].IntValue);
            }
            catch (Exception e)
            {
                ConsoleColor col = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.ForegroundColor = col;
            }
        }

        public static void Save(this Player player, bool async = false)
        {
            NbtCompound namedTag = new NbtCompound("");
            namedTag.Add(player.GetNbtPos());
            namedTag.Add(player.GetNbtRotation()); 

            namedTag.Add(player.GetNbtHealth());
            namedTag.Add(player.GetNbtEffects());

            namedTag.Add(player.GetFoodNbt());
            namedTag.Add(player.GetNbtFoodExhaustionLevel());
            namedTag.Add(player.GetNbtFoodSaturationLevel());

            namedTag.Add(player.GetNbtXpLevel());
            namedTag.Add(player.GetNbtXpP());

            namedTag.Add(player.GetNbtInventory());
            namedTag.Add(player.GetNbtSelectedInventorySlot());

            namedTag.Add(player.GetNbtLevel());

            namedTag.Add(player.GetNbtPlayerGameType());

            player.GetServer().SaveOfflinePlayerData(player.PlayerInfo.Username, namedTag, async);
        }

        public static MiNetServer GetServer(this Player player)
        {
            PropertyInfo info = player.GetType().GetProperty("Server", BindingFlags.Instance|BindingFlags.NonPublic);
            return info.GetValue(player) as MiNetServer;
        }

        public static void SaveOfflinePlayerData(this MiNetServer server, string name, NbtCompound nbtTag, bool async = false)
        {
            NbtFile nbt = new NbtFile(nbtTag);
            nbt.BigEndian = true;

            ParameterizedThreadStart threadStart = new ParameterizedThreadStart(obj =>
            {
                try
                {
                    string path = Config.GetProperty("PluginDirectory", ".\\") + "\\PlayerSave\\players\\";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    ((NbtFile)((object[])obj)[0]).SaveToFile(path + ((object[])obj)[1].ToString().ToLower() + ".dat", NbtCompression.ZLib);
                }
                catch (Exception e)
                {
                    ConsoleColor col = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Console.ForegroundColor = col;
                }
            });
            Thread thread = new Thread(threadStart);

            if (async)
            {
                thread.Start(new object[] { nbt, name });
            }
            else
            {
                threadStart(new object[] { nbt, name });
            }
        }

        static NbtInt GetNbtPlayerGameType(this Player player)
        {
            return new NbtInt("playerGameType", (int)player.GameMode);
        }

        static NbtString GetNbtLevel(this Player player)
        {
            return new NbtString("Level", player.Level.LevelName);
        }

        public static NbtCompound NbtSerialize(this Item item, int slot = 1)
        {
            NbtCompound nbt = new NbtCompound();
            nbt.Add(new NbtShort("id", item.Id));
            nbt.Add(new NbtByte("Count", item.Count));
            nbt.Add(new NbtShort("Damage", item.Metadata));

            nbt.Add(new NbtByte("Slot", (byte)slot));

            return nbt;
        }

        static NbtInt GetNbtSelectedInventorySlot(this Player player)
        {
            return new NbtInt("SelectedInventorySlot", player.Inventory.InHandSlot);
        }

        static NbtList GetNbtInventory(this Player player)
        {
            NbtTag[] tags = new NbtTag[104];

            for (int i = 0; i < player.Inventory.Slots.Count; i++)
            {
                tags[i] = player.Inventory.Slots[i].NbtSerialize(i);
            }

            for (int i = player.Inventory.Slots.Count; i < 100; i++)
            {
                tags[i] = new ItemAir().NbtSerialize(i);
            }

            tags[100] = player.Inventory.Helmet.NbtSerialize(100);
            tags[101] = player.Inventory.Chest.NbtSerialize(101);
            tags[102] = player.Inventory.Leggings.NbtSerialize(102);
            tags[103] = player.Inventory.Boots.NbtSerialize(103);


            NbtList nbt = new NbtList("Inventory", tags, NbtTagType.Compound);

            //@TODO EnderChest

            return nbt;
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
            NbtList nbt = new NbtList("ActiveEffects", NbtTagType.Compound);
            foreach (Effect effect in player.Effects.Values)
            {
                NbtCompound nbtCompound = new NbtCompound();
                nbtCompound.Add(new NbtByte("Id", (byte)effect.EffectId));
                nbtCompound.Add(new NbtInt("Duration", effect.Duration));
                nbtCompound.Add(new NbtByte("ShowParticles", (byte)(effect.Particles ? 1 : 0)));
                nbt.Add(nbtCompound);
            }

            return nbt;
        }

        static NbtList GetNbtPos(this Player player)
        {
            NbtList nbt = new NbtList("Pos", NbtTagType.Double);
            nbt.Add(new NbtDouble(player.KnownPosition.X));
            nbt.Add(new NbtDouble(player.KnownPosition.Y));
            nbt.Add(new NbtDouble(player.KnownPosition.Z));
            return nbt;
        }

        static NbtList GetNbtRotation(this Player player)
        {
            NbtList nbt = new NbtList("Rotation", NbtTagType.Float);
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
