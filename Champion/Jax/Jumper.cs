using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using LeagueSharp.Common;
using SharpDX;
using Spell = LeagueSharp.Common.Spell;
using TargetSelector = PortAIO.TSManager;

namespace JaxQx
{
    internal class Jumper
    {
        
        public static Vector2 testSpellCast;
        public static Vector2 testSpellProj;

        public static AIHeroClient Player = ObjectManager.Player;
        public static Spell Q;

        public static float lastward;
        public static float last;

        public static int getJumpWardId()
        {
            int[] wardIds = {3340, 3350, 3205, 3207, 2049, 2045, 2044, 3361, 3154, 3362, 3160, 2043, (int)ItemId.Sightstone, (int)ItemId.Warding_Totem_Trinket, (int)ItemId.Vision_Ward, (int)ItemId.Sightstone, (int)ItemId.Trackers_Knife, (int)ItemId.Trackers_Knife_Enchantment_Cinderhulk, (int)ItemId.Trackers_Knife_Enchantment_Devourer, (int)ItemId.Trackers_Knife_Enchantment_Runic_Echoes, (int)ItemId.Trackers_Knife_Enchantment_Sated_Devourer, (int)ItemId.Trackers_Knife_Enchantment_Warrior};
            foreach (var id in wardIds)
            {
                if (Item.HasItem(id) && Item.CanUseItem(id))
                    return id;
            }
            return -1;
        }

        public static void moveTo(Vector2 Pos)
        {
            EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, Pos.To3D());
        }

        public static void wardJump(Vector2 pos)
        {
            Q = new Spell(SpellSlot.Q, 700);

            if (!Q.IsReady())
                return;

            var wardIs = false;

            if (!InDistance(pos, Player.ServerPosition.LSTo2D(), Q.Range))
            {
                pos = Player.ServerPosition.LSTo2D() + Vector2.Normalize(pos - Player.ServerPosition.LSTo2D())*600;
            }

            if (!Q.IsReady())
                return;

            foreach (var ally in ObjectManager.Get<Obj_AI_Base>().Where(ally => ally.IsAlly && !(ally is Obj_AI_Turret) && InDistance(pos, ally.ServerPosition.LSTo2D(), 200)))
            {
                wardIs = true;
                moveTo(pos);
                if (InDistance(Player.ServerPosition.LSTo2D(), ally.ServerPosition.LSTo2D(), Q.Range + ally.BoundingRadius))
                {
                    if (last < Environment.TickCount)
                    {
                        Q.Cast(ally);
                        last = Environment.TickCount + 2000;
                    }
                    else return;
                }
                return;
            }
            Polygon pol;
            if ((pol = Program.map.getInWhichPolygon(pos)) != null)
            {
                if (InDistance(pol.getProjOnPolygon(pos), Player.ServerPosition.LSTo2D(), Q.Range) && !wardIs &&
                    InDistance(pol.getProjOnPolygon(pos), pos, 250))
                {
                    putWard(pos);
                }
            }
            else if (!wardIs)
            {
                putWard(pos);
            }
        }

        public static bool putWard(Vector2 pos)
        {
            int wardItem;
            if ((wardItem = getJumpWardId()) != -1)
            {
                foreach (var slot in Player.InventoryItems.Where(slot => slot.Id == (ItemId) wardItem))
                {
                    if (lastward < Environment.TickCount)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(slot.SpellSlot, pos.To3D());
                        lastward = Environment.TickCount + 2000;
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }


        public static bool InDistance(Vector2 pos1, Vector2 pos2, float distance)
        {
            var dist2 = Vector2.DistanceSquared(pos1, pos2);
            return dist2 <= distance*distance;
        }
    }
}