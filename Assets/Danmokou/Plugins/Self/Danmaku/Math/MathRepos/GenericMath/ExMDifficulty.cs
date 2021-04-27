﻿using DMK.Core;
using DMK.Expressions;
using Ex = System.Linq.Expressions.Expression;
using static DMK.Expressions.ExMHelpers;
using tfloat = DMK.Expressions.TEx<float>;
using static DMK.Core.GameManagement;

namespace DMK.DMath.Functions {
/// <summary>
/// See <see cref="ExM"/>. This class contains functions related to difficulty control.
/// </summary>
[Reflect]
public static class ExMDifficulty {
    //Note: difficulty is parsed statically at script compile time (when not using EXBAKE_SAVE/LOAD),
    //whereas rank (future) will always be dynamic.
    
#if !EXBAKE_SAVE && !EXBAKE_LOAD
    /// <summary>
    /// Get the difficulty multiplier. 1 is easy, ~2.3 is lunatic. POSITIVE values outside this range are possible.
    /// </summary>
    public static tfloat D() => Ex.Constant(Difficulty.Value);
    /// <summary>
    /// Get the difficulty counter. 1 is easy, 4 is lunatic.
    /// </summary>
    public static tfloat Dc() => Ex.Constant(Difficulty.Counter);
    /// <summary>
    /// Get the difficulty counter relative to lunatic. -3 is easy, 0 is lunatic.
    /// </summary>
    public static tfloat DcL() => Ex.Constant(Difficulty.Counter - FixedDifficulty.Lunatic.Counter());
    
    /// <summary>
    /// Get the difficulty multiplier centered on normal.
    /// </summary>
    public static tfloat DN() => Ex.Constant(Difficulty.Value / FixedDifficulty.Normal.Value());
    /// <summary>
    /// Get the difficulty multiplier centered on hard.
    /// </summary>
    public static tfloat DH() => Ex.Constant(Difficulty.Value / FixedDifficulty.Hard.Value());
    /// <summary>
    /// Get the difficulty multiplier centered on lunatic.
    /// </summary>
    public static tfloat DL() => Ex.Constant(Difficulty.Value / FixedDifficulty.Lunatic.Value());

    private static tfloat ResolveD3(tfloat n, tfloat h, tfloat u) =>
        Difficulty.Value < FixedDifficulty.Normal.Value() ? n :
        Difficulty.Value < FixedDifficulty.Lunatic.Value() ? h :
        u;

    /// <summary>
    /// Return -2 if the difficulty is less than Normal,
    /// else 0 if less than Lunatic,
    /// else 2.
    /// </summary>
    /// <returns></returns>
    public static tfloat D3d2() => ResolveD3(EN2, E0, E2);
    /// <summary>
    /// Return -1 if the difficulty is less than Normal,
    /// else 0 if less than Lunatic,
    /// else 1.
    /// </summary>
    /// <returns></returns>
    public static tfloat D3d1() => ResolveD3(EN1, E0, E1);
#else

    public static tfloat D() => Ex.Property(null, typeof(GameManagement), "Difficulty").Field("Value");
    public static tfloat Dc() => Ex.Property(null, typeof(GameManagement), "Difficulty").Field("Counter");
    
    public static tfloat DcL() => Dc().Sub(ExC(FixedDifficulty.Lunatic.Counter()));
    
    public static tfloat DN() => D().Div(ExC(FixedDifficulty.Normal.Value()));
    public static tfloat DH() => D().Div(ExC(FixedDifficulty.Hard.Value()));
    public static tfloat DL() => D().Div(ExC(FixedDifficulty.Lunatic.Value()));

    private static tfloat ResolveD3(tfloat n, tfloat h, tfloat u) =>
        Ex.Condition(D().LT(ExC(FixedDifficulty.Normal.Value())), n,
            Ex.Condition(D().LT(ExC(FixedDifficulty.Lunatic.Value())), h, u)
        );

    /// <summary>
    /// Return -2 if the difficulty is less than Normal,
    /// else 0 if less than Lunatic,
    /// else 2.
    /// </summary>
    /// <returns></returns>
    public static tfloat D3d2() => ResolveD3(EN2, E0, E2);
    /// <summary>
    /// Return -1 if the difficulty is less than Normal,
    /// else 0 if less than Lunatic,
    /// else 1.
    /// </summary>
    /// <returns></returns>
    public static tfloat D3d1() => ResolveD3(EN1, E0, E1);
    
    
#endif
    
    
    
    
}
}