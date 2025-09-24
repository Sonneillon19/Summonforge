namespace RPG.Battle
{
    public static class EffectService
    {
        public static void Apply(EffectDef def, UnitRuntime src, UnitRuntime dst, int duration)
        {
            var inst = new EffectInstance { Def = def, RemainingTurns = duration, Source = src };
            dst.Effects.Add(inst);
        }

        public static void Tick(UnitRuntime u)
        {
            for (int i = u.Effects.Count - 1; i >= 0; i--)
            {
                var e = u.Effects[i];
                e.RemainingTurns--;
                if (e.RemainingTurns <= 0)
                    u.Effects.RemoveAt(i);
            }
        }
    }
}
