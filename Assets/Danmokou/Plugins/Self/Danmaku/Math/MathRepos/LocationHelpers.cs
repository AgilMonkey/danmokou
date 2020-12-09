﻿using System.Linq.Expressions;
using UnityEngine;
using Ex = System.Linq.Expressions.Expression;
using ExBPY = System.Func<DMK.Expressions.TExPI, DMK.Expressions.TEx<float>>;
using ExTP = System.Func<DMK.Expressions.TExPI, DMK.Expressions.TEx<UnityEngine.Vector2>>;
using ExPred = System.Func<DMK.Expressions.TExPI, DMK.Expressions.TEx<bool>>;
using DMK.Behavior;
using DMK.Core;
using DMK.DataHoist;
using DMK.Expressions;
using static DMK.DMath.Functions.ExM;
using tfloat = DMK.Expressions.TEx<float>;
using tv2 = DMK.Expressions.TEx<UnityEngine.Vector2>;
using ev2 = DMK.Expressions.EEx<UnityEngine.Vector2>;


namespace DMK.DMath.Functions {
public static partial class ExMPred {
//return (loc.x >= left && loc.x <= right && loc.y >= bot && loc.y <= top);
    public static TEx<bool> OnScreen(EEx<Vector2> loc) => EEx.ResolveV2(loc, l =>
            l.x.GT(LocationHelpers.left)
            .And(l.x.LT(LocationHelpers.right))
            .And(l.y.GT(LocationHelpers.bot))
            .And(l.y.LT(LocationHelpers.top)));
    public static TEx<bool> OnScreenBy(EEx<float> by, EEx<Vector2> loc) => EEx.ResolveV2(loc, by, (l, f) =>
            l.x.GT(LocationHelpers.left.Sub(f))
            .And(l.x.LT(LocationHelpers.right.Add(f)))
            .And(l.y.GT(LocationHelpers.bot.Sub(f)))
            .And(l.y.LT(LocationHelpers.top.Add(f))));

    public static TEx<bool> OffScreen(TEx<Vector2> loc) => Not(OnScreen(loc));
    
    public static TEx<bool> OffScreenBy(TEx<float> f, TEx<Vector2> loc) => Not(OnScreenBy(f, loc));

}
public static partial class ExM {
    public static TEx<float> YMin() => LocationHelpers.bot;
    public static TEx<float> YMax() => LocationHelpers.top;
    public static TEx<float> XMin() => LocationHelpers.left;
    public static TEx<float> XMax() => LocationHelpers.right;
    [Alias("ymin-")]
    public static TEx<float> YMinMinus1() => LocationHelpers.bot.Sub(1);
    [Alias("ymax+")]
    public static TEx<float> YMaxPlus1() => LocationHelpers.top.Add(1);
    [Alias("xmin-")]
    public static TEx<float> XMinMinus1() => LocationHelpers.left.Sub(1);
    [Alias("xmax+")]
    public static TEx<float> XMaxPlus1() => LocationHelpers.right.Add(1);
    public static TEx<float> XWidth() => LocationHelpers.width;
    public static TEx<float> YHeight() => LocationHelpers.height;
    
    private static readonly ExFunction GetEnemyVisiblePlayer =
        ExUtils.Wrap(typeof(LocationHelpers), "GetEnemyVisiblePlayer");
    
    /// <summary>
    /// Get the location of the player as visible to enemies.
    /// </summary>
    /// <returns></returns>
    public static TEx<Vector2> LPlayer() => GetEnemyVisiblePlayer.Of();

    public static TEx<Vector2> LBEH(BEHPointer beh) => Ex.Constant(beh).Field("beh").Field("bpi").Field("loc");
    
    private static readonly ExFunction distToWall =
        ExUtils.Wrap(typeof(LocationHelpers), "DistToWall", typeof(Vector2), typeof(Vector2));
    private static readonly ExFunction toWall =
        ExUtils.Wrap(typeof(LocationHelpers), "ToWall", typeof(Vector2), typeof(Vector2));

    public static tfloat DistToWall(tv2 from, tv2 dir) => distToWall.Of(from, dir);
    public static tv2 ToWall(tv2 from, tv2 dir) => toWall.Of(from, dir);
}

public static partial class BPYRepo {
    
    
    /// <summary>
    /// Returns Atan(Player.Loc - this.Loc) in degrees.
    /// </summary>
    public static ExBPY AngleToPlayer() => AngleTo(x => ExM.LPlayer());
    /// <summary>
    /// Returns Atan(loc - this.Loc) in degrees.
    /// </summary>
    public static ExBPY AngleTo(ExTP loc) => bpi => ATan(Sub(loc(bpi), bpi.loc));
    
    /// <summary>
    /// Returns the x-position of the left/right wall that the location is closer to.
    /// </summary>
    public static ExBPY ToLR(ExTP loc) => bpi =>
        Ex.Condition(new TExV2(loc(bpi)).x.LT0(), LocationHelpers.left, LocationHelpers.right);
    /// <summary>
    /// Returns the x-position of the left/right wall that the location is farther from.
    /// </summary>
    public static ExBPY ToRL(ExTP loc) => bpi =>
        Ex.Condition(new TExV2(loc(bpi)).x.LT0(), LocationHelpers.right, LocationHelpers.left);
}
public static partial class Parametrics {

    public static ExTP LNearestEnemy() => b => {
        var loc = new TExV2();
        return Ex.Block(new ParameterExpression[] { loc },
            Ex.IfThen(Ex.Not(Enemy.findNearest.Of(b.loc, loc)),
                loc.Is(Ex.Constant(new Vector2(0f, 50f)))
            ),
            loc
        );
    };
    public static ExTP LSaveNearestEnemy() => b => {
        Ex data = DataHoisting.GetClearableDictInt();
        var eid_in = ExUtils.V<int?>();
        var eid = ExUtils.V<int>();
        var loc = new TExV2();
        return Ex.Block(new[] { eid_in, eid, loc },
            eid_in.Is(Ex.Condition(ExUtils.DictContains<uint, int>(data, b.id),
                    data.DictGet(b.id).As<int?>(),
                    Ex.Constant(null).As<int?>())
            ),
            Ex.IfThenElse(Enemy.findNearestSave.Of(b.loc, eid_in, eid, loc),
                data.DictSet(b.id, eid),
                loc.Is(Ex.Constant(new Vector2(0f, 50f)))
            ),
            loc
        );
    };

    /*
    /// <summary>
    /// Find the location such that a ray fired from the source would bounce
    /// against a horizontal wall at {x, y} and hit the target.
    /// </summary>
    /// <param name="y"></param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static ExTP BounceY(ExBPY y, ExTP source, ExTP target) {
        var src = TExV2.Variable();
        var v2 = TExV2.Variable();
        var yw = ExUtils.VFloat();
        return bpi => Ex.Block(new[] {src, v2, yw},
            Ex.Assign(src, source(bpi)),
            Ex.Assign(yw, Ex.Subtract(y(bpi), src.y)),
            Ex.Assign(v2, Ex.Subtract(target(bpi), src)),
            ExUtils.AddAssign(src.x, v2.x.Mul(yw).Div(yw.Add(yw.Sub(v2.y)))),
            ExUtils.AddAssign(src.y, yw),
            src
        );
    }*/
}
}
