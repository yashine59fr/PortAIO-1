﻿using System;
using System.Collections.Generic;

using TargetSelector = PortAIO.TSManager; namespace Mordekaiser
{
    internal class Items
    {
        public enum EnumItemTargettingType
        {
            Ally,
            EnemyHero,
            EnemyObjects
        }

        public enum EnumItemType
        {
            Targeted,
            AoE
        }

        public static Dictionary<string, Tuple<LeagueSharp.Common.Items.Item, EnumItemType, EnumItemTargettingType>>
            ItemDb;

        public Items()
        {
            ItemDb = new Dictionary<string, Tuple<LeagueSharp.Common.Items.Item, EnumItemType, EnumItemTargettingType>>
            {
                {
                    "Tiamat",
                    new Tuple<LeagueSharp.Common.Items.Item, EnumItemType, EnumItemTargettingType>(
                        new LeagueSharp.Common.Items.Item(3077, 450f),
                        EnumItemType.AoE,
                        EnumItemTargettingType.EnemyObjects)
                },
                {
                    "Bilge",
                    new Tuple<LeagueSharp.Common.Items.Item, EnumItemType, EnumItemTargettingType>(
                        new LeagueSharp.Common.Items.Item(3144, 450f),
                        EnumItemType.Targeted, EnumItemTargettingType.EnemyHero)
                },
                {
                    "Blade",
                    new Tuple<LeagueSharp.Common.Items.Item, EnumItemType, EnumItemTargettingType>(
                        new LeagueSharp.Common.Items.Item(3153, 450f),
                        EnumItemType.Targeted,
                        EnumItemTargettingType.EnemyHero)
                },
                {
                    "Hydra",
                    new Tuple<LeagueSharp.Common.Items.Item, EnumItemType, EnumItemTargettingType>(
                        new LeagueSharp.Common.Items.Item(3074, 250f),
                        EnumItemType.AoE,
                        EnumItemTargettingType.EnemyObjects)
                },
                {
                    "Titanic Hydra Cleave",
                    new Tuple<LeagueSharp.Common.Items.Item, EnumItemType, EnumItemTargettingType>(
                        new LeagueSharp.Common.Items.Item(3748, Utils.Player.AutoAttackRange),
                        EnumItemType.AoE,
                        EnumItemTargettingType.EnemyHero)
                },
                {
                    "Randiun",
                    new Tuple<LeagueSharp.Common.Items.Item, EnumItemType, EnumItemTargettingType>(
                        new LeagueSharp.Common.Items.Item(3143, 490f),
                        EnumItemType.AoE,
                        EnumItemTargettingType.EnemyHero)
                },
                {
                    "Hextech",
                    new Tuple<LeagueSharp.Common.Items.Item, EnumItemType, EnumItemTargettingType>(
                        new LeagueSharp.Common.Items.Item(3146, 750f),
                        EnumItemType.Targeted,
                        EnumItemTargettingType.EnemyHero)
                }
            };
        }

        public struct Tuple<TA, TB, TC> : IEquatable<Tuple<TA, TB, TC>>
        {
            private readonly TA item;
            private readonly TB itemType;
            private readonly TC targetingType;

            public Tuple(TA pItem, TB pItemType, TC pTargetingType)
            {
                item = pItem;
                itemType = pItemType;
                targetingType = pTargetingType;
            }

            public TA Item
            {
                get { return item; }
            }

            public TB ItemType
            {
                get { return itemType; }
            }

            public TC TargetingType
            {
                get { return targetingType; }
            }

            public override int GetHashCode()
            {
                return item.GetHashCode() ^ itemType.GetHashCode() ^ targetingType.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }
                return Equals((Tuple<TA, TB, TC>) obj);
            }

            public bool Equals(Tuple<TA, TB, TC> other)
            {
                return other.item.Equals(item) && other.itemType.Equals(itemType) &&
                       other.targetingType.Equals(targetingType);
            }
        }
    }
}