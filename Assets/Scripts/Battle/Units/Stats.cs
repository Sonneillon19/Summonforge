using System;
using UnityEngine;

namespace RPG.Battle
{
    [Serializable]
    public struct Stats
    {
        public int HP;
        public int ATK;
        public int DEF;
        public int SPD;
        public float CritRate;   // 0..1
        public float CritDmg;    // 0..1 (extra)
        public float Res;        // 0..1
        public float Acc;        // 0..1

        public static Stats operator +(Stats a, Stats b)
        {
            return new Stats
            {
                HP = a.HP + b.HP,
                ATK = a.ATK + b.ATK,
                DEF = a.DEF + b.DEF,
                SPD = a.SPD + b.SPD,
                CritRate = a.CritRate + b.CritRate,
                CritDmg = a.CritDmg + b.CritDmg,
                Res = a.Res + b.Res,
                Acc = a.Acc + b.Acc
            };
        }
    }
}
