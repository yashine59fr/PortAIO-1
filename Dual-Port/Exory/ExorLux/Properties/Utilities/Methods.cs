using EloBuddy;
using EloBuddy.SDK;
using LeagueSharp;
using LeagueSharp.SDK;

using TargetSelector = PortAIO.TSManager; namespace ExorAIO.Champions.Lux
{
    /// <summary>
    ///     The methods class.
    /// </summary>
    internal class Methods
    {
        /// <summary>
        ///     Sets the methods.
        /// </summary>
        public static void Initialize()
        {
            Game.OnUpdate += Lux.OnUpdate;
		    GameObject.OnCreate += Lux.OnCreate;
            GameObject.OnDelete += Lux.OnDelete;
            Events.OnGapCloser += Lux.OnGapCloser;
            Obj_AI_Base.OnProcessSpellCast += Lux.OnProcessSpellCast;
            LeagueSharp.Common.LSEvents.BeforeAttack += Lux.OnAction;
        }
    }
}