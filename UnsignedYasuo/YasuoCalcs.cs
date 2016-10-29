﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace UnsignedYasuo
{

    class YasuoCalcs
    {
        public static AIHeroClient Yasuo = Program.Yasuo;
        public static float YasuoLastEStartTime = 0;

        public static bool IsDashing()
        {
            if ((Program.E.State == SpellState.Surpressed && !Yasuo.HasBuffOfType(BuffType.Suppression)) || Game.Time - YasuoLastEStartTime <= Program.E.CastDelay / 1000)
                return true;
            return false;
        }

        public static double Q(Obj_AI_Base target)
        {
            if(Yasuo.FlatCritChanceMod >= 1 && !Yasuo.HasItem(ItemId.Infinity_Edge))
                return Yasuo.CalculateDamageOnUnit(target, DamageType.Physical,
                    (float)(new double[] { 0, 20, 40, 60, 80, 100 }[Program.Q.Level] + (Yasuo.TotalAttackDamage * 1.5f)));
            else if (Yasuo.FlatCritChanceMod >= 1 && Yasuo.HasItem(ItemId.Infinity_Edge))
                return Yasuo.CalculateDamageOnUnit(target, DamageType.Physical, 
                    (float)(new double[] { 0, 20, 40, 60, 80, 100 }[Program.Q.Level] + (Yasuo.TotalAttackDamage * 1.87f)));
            else
                return Yasuo.CalculateDamageOnUnit(target, DamageType.Physical,
                    (float)(new double[] { 0, 20, 40, 60, 80, 100 }[Program.Q.Level] + Yasuo.TotalAttackDamage));
        }
        public static double E(Obj_AI_Base target)
        {
            double dmgModifier = 1;
            if (Yasuo.HasBuff("yasuodashscalar"))
                dmgModifier += Yasuo.GetBuff("yasuodashscalar").Count * 0.25f;

            return Yasuo.CalculateDamageOnUnit(target, DamageType.Magical,
                (50 + (20 * Program.E.Level)) + (0.6f * Yasuo.FlatMagicDamageMod));
        }
        public static float Ignite(Obj_AI_Base target)
        {
            return ((10 + (4 * Yasuo.Level)) * 5) - ((target.HPRegenRate / 2) * 5);
        }
        public static bool IsUnderTurret(Vector3 position)
        {
            Obj_AI_Turret closestTurret = EntityManager.Turrets.Enemies.Where(a =>
                a.IsInRange(position, a.AttackRange + 35)
                && !a.IsDead).OrderBy(a => a.Distance(position)).FirstOrDefault();

            if (closestTurret == null)
                return false;
            return true;
        }
        public static bool IsInFountain(Vector3 position, GameObjectTeam team)
        {
            float fountainRange = 1050;
            Vector3 vec3 = (team == GameObjectTeam.Order) ? new Vector3(363, 426, 182) : new Vector3(14340, 14390, 172);

            return position.IsInRange(vec3, fountainRange);
        }
        public static bool ERequirements(Obj_AI_Base unit, bool EUNDERTURRET)
        {
            //not in fountain
            if (!IsInFountain(GetDashingEnd(unit), unit.Team) &&
                //can be e'd
                !unit.HasBuff("YasuoDashWrapper") &&
                (!GetDashingEnd(unit).IsUnderTower() || EUNDERTURRET)
                )
                return true;

            return false;
        }
        public static bool WillQBeReady()
        {
            if (Math.Max(0, Yasuo.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires - Game.Time) <= 0.20f && Program.Q.IsLearned)
                return true;
            else
                return false;
        }
        public static float GetQReadyTime()
        {
            return Math.Max(0, Yasuo.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires - Game.Time);
        }
        public static int GetQReadyTimeInt()
        {
            return (int)(GetQReadyTime() * 1000);
        }
        public static Vector3 GetDashingEnd(Obj_AI_Base target)
        {
            if (target == null || target.Position == Vector3.Zero)
                return Vector3.Zero;

            Vector3 endPosition = Yasuo.Position.Extend(target, Program.E.Range).To3D() + new Vector3(0, 0, Yasuo.Position.Z);
            return endPosition;
        }
    }
}
