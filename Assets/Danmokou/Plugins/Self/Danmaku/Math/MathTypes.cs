﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DMK.Behavior;
using DMK.Core;
using DMK.Danmaku;
using DMK.DataHoist;
using DMK.Expressions;
using UnityEngine;
using DMK.DMath.Functions;
using DMK.Graphics;
using DMK.Player;
using DMK.Reflection;
using DMK.Scriptables;
using JetBrains.Annotations;
using ExVTP = System.Func<DMK.Expressions.ITexMovement, DMK.Expressions.TEx<float>, DMK.Expressions.TExArgCtx, DMK.Expressions.TExV2, DMK.Expressions.TEx>;
using ExBPY = System.Func<DMK.Expressions.TExArgCtx, DMK.Expressions.TEx<float>>;
using ExTP = System.Func<DMK.Expressions.TExArgCtx, DMK.Expressions.TEx<UnityEngine.Vector2>>;

namespace DMK.DMath {
public class FiringCtx {
    public enum DataType {
        Int,
        Float,
        V2,
        V3,
        RV2
    }
    public readonly Dictionary<int, int> boundInts = new Dictionary<int, int>();
    public readonly Dictionary<int, float> boundFloats = new Dictionary<int, float>();
    public readonly Dictionary<int, Vector2> boundV2s = new Dictionary<int, Vector2>();
    public readonly Dictionary<int, Vector3> boundV3s = new Dictionary<int, Vector3>();
    public readonly Dictionary<int, V2RV2> boundRV2s = new Dictionary<int, V2RV2>();
    public BehaviorEntity? firer; //Note this may be repooled or otherwise destroyed during execution
    
    public PlayerInput? playerController; //For player bullets
    [UsedImplicitly]
    public PlayerInput PlayerController =>
        playerController != null ?
            playerController :
            throw new Exception("FiringCtx does not have a player controller");

    [UsedImplicitly]
    public FireOption OptionFirer {
        get {
            if (firer is FireOption fo)
                return fo;
            throw new Exception("FiringCtx does not have an option firer");
        }
    }

    public CurvedTileRenderLaser? laserController;
    [UsedImplicitly]
    public CurvedTileRenderLaser LaserController => 
        laserController ?? throw new Exception("FiringCtx does not have a laser controller");
    public (int bossDmg, int stageDmg, EffectStrategy eff)? playerFireCfg;
    
    private static readonly Stack<FiringCtx> cache = new Stack<FiringCtx>();
    public static int Allocated { get; private set; }
    public static int Popped { get; private set; }
    public static int Recached { get; private set; }
    public static int Copied { get; private set; }

    public static readonly FiringCtx Empty = new FiringCtx();

    private FiringCtx() { }
    public static FiringCtx New(GenCtx? gcx = null) {
        FiringCtx nCtx;
        if (cache.Count > 0) {
            nCtx = cache.Pop();
            ++Popped;
        } else {
            nCtx = new FiringCtx();
            ++Allocated;
        }
        nCtx.firer = gcx?.exec;
        nCtx.playerController = nCtx.firer switch {
            PlayerInput pi => pi,
            FireOption fo => fo.Player,
            _ => nCtx.playerController
        };
        return nCtx;
    }
    
    private void UploadAddOne(Reflector.ExType ext, string varName, GenCtx gcx) {
        var varId = GetKey(varName);
        if      (ext == Reflector.ExType.Float) 
            boundFloats[varId] = gcx.GetFloatOrThrow(varName);
        else if (ext == Reflector.ExType.V2) 
            boundV2s[varId] = gcx.V2s.GetOrThrow(varName, "GCX V2 values");
        else if (ext == Reflector.ExType.V3) 
            boundV3s[varId] = gcx.V3s.GetOrThrow(varName, "GCX V3 values");
        else if (ext == Reflector.ExType.RV2) 
            boundRV2s[varId] = gcx.RV2s.GetOrThrow(varName, "GCX RV2 values");
        else throw new Exception($"Cannot hoist GCX data {varName}<{ext}>.");
    }
    public void UploadAdd((Reflector.ExType, string)[] boundVars, GenCtx gcx) {
        for (int ii = 0; ii < boundVars.Length; ++ii) {
            var (ext, varNameS) = boundVars[ii];
            UploadAddOne(ext, varNameS, gcx);
        }
        for (int ii = 0; ii < gcx.exposed.Count; ++ii) {
            var (ext, varNameS) = gcx.exposed[ii];
            UploadAddOne(ext, varNameS, gcx);
        }
    }

    public GenCtx RevertToGCX(BehaviorEntity exec) {
        var gcx = GenCtx.New(exec, V2RV2.Zero);
        foreach (var sk in keyNames) {
            if (boundFloats.ContainsKey(sk.Value))
                gcx.fs[sk.Key] = boundFloats[sk.Value];
            if (boundV2s.ContainsKey(sk.Value))
                gcx.v2s[sk.Key] = boundV2s[sk.Value];
            if (boundV3s.ContainsKey(sk.Value))
                gcx.v3s[sk.Key] = boundV3s[sk.Value];
            if (boundRV2s.ContainsKey(sk.Value))
                gcx.rv2s[sk.Key] = boundRV2s[sk.Value];
        }
        return gcx;
    }

    public FiringCtx Copy() {
        ++Copied;
        var nCtx = New();
        boundInts.CopyInto(nCtx.boundInts);
        boundFloats.CopyInto(nCtx.boundFloats);
        boundV2s.CopyInto(nCtx.boundV2s);
        boundV3s.CopyInto(nCtx.boundV3s);
        boundRV2s.CopyInto(nCtx.boundRV2s);
        nCtx.firer = firer;
        nCtx.playerController = playerController;
        nCtx.laserController = laserController;
        nCtx.playerFireCfg = playerFireCfg;
        return nCtx;
    }

    public void Dispose() {
        if (this == Empty) return;
        boundInts.Clear();
        boundFloats.Clear();
        boundV2s.Clear();
        boundV3s.Clear();
        boundRV2s.Clear();
        firer = null;
        playerController = null;
        laserController = null;
        playerFireCfg = null;
        ++Recached;
        cache.Push(this);
    }

    //Expression methods

    public static DataType FromType<T>() {
        var t = typeof(T);
        if (t == typeof(Vector2))
            return DataType.V2;
        if (t == typeof(Vector3))
            return DataType.V3;
        if (t == typeof(V2RV2))
            return DataType.RV2;
        if (t == typeof(int))
            return DataType.Int;
        else
            return DataType.Float;
    }

    private static TEx Hoisted(TExArgCtx tac, DataType typ, string name, Func<Expression, Expression> constructor) {
        var ex = constructor(exGetKey(name));

        var key_name = tac.Ctx.NameWithSuffix(name);
        var key_assign = FormattableString.Invariant(
            $"var {key_name} = FiringCtx.GetKey(\"{name}\");");
        var replaced = constructor(Expression.Variable(typeof(int), key_name));
        tac.Ctx.HoistedVariables.Add(key_assign);
        tac.Ctx.HoistedReplacements[ex] = replaced;

        return ex;
    }
    
    public static TEx Contains(TExArgCtx tac, DataType typ, string name) =>
        Hoisted(tac, typ, name, key => ExUtils.DictContains(GetDict(tac.BPI.FiringCtx, typ), key));
    public static Expression Contains<T>(TExArgCtx tac, string name) =>
        Hoisted(tac, FromType<T>(), name, key => ExUtils.DictContains(GetDict(tac.BPI.FiringCtx, FromType<T>()), key));
    
    public static TEx GetValue(TExArgCtx tac, DataType typ, string name) =>
        Hoisted(tac, typ, name, key => GetDict(tac.BPI.FiringCtx, typ).DictGet(key));
    public static Expression GetValue<T>(TExArgCtx tac, string name) =>
        Hoisted(tac, FromType<T>(), name, key => GetDict(tac.BPI.FiringCtx, FromType<T>()).DictGet(key));

    public static Expression SetValue(TExArgCtx tac, DataType typ, string name, Expression val) =>
        Hoisted(tac, typ, name, key => GetDict(tac.BPI.FiringCtx, typ).DictSet(key, val));
    public static Expression SetValue<T>(TExArgCtx tac, string name, Expression val) =>
        Hoisted(tac, FromType<T>(), name, key => GetDict(tac.BPI.FiringCtx, FromType<T>()).DictSet(key, val));
    
    public static Expression GetDict(Expression fctx, DataType typ) => typ switch {
        DataType.RV2 => fctx.Field("boundRV2s"),
        DataType.V3 => fctx.Field("boundV3s"),
        DataType.V2 => fctx.Field("boundV2s"),
        DataType.Int => fctx.Field("boundInts"),
        _ => fctx.Field("boundFloats")
    };
    
    
    public static void ClearNames() {
        keyNames.Clear();
        lastKey = 0;
    }

    private static Expression exGetKey(string name) => Expression.Constant(GetKey(name));
    public static int GetKey(string name) {
        if (keyNames.TryGetValue(name, out var res)) return res;
        keyNames[name] = lastKey;
        return lastKey++;
    }

    private static readonly Dictionary<string, int> keyNames = new Dictionary<string, int>();
    private static int lastKey = 0;
    
}
/// <summary>
/// A struct containing the input required for a parametric equation.
/// </summary>
public struct ParametricInfo {
    public static ParametricInfo Zero = new ParametricInfo(Vector2.zero, 0, 0, 0);
    /// <summary>Random ID</summary>
    public readonly uint id;
    /// <summary>Firing index</summary>
    public readonly int index;
    /// <summary>Global position</summary>
    public Vector2 loc;
    /// <summary>Life-time (with minor adjustment)</summary>
    public float t;
    /// <summary>Context containing additional bound variables</summary>
    public FiringCtx ctx;

    public static ParametricInfo WithRandomId(Vector2 position, int findex, float t) => new ParametricInfo(position, findex, RNG.GetUInt(), t);
    public static ParametricInfo WithRandomId(Vector2 position, int findex) => WithRandomId(position, findex, 0f);

    public ParametricInfo(in Movement mov, int findex = 0, uint? id = null, float t = 0, FiringCtx? ctx = null) : 
        this(mov.rootPos, findex, id, t, ctx) { }
    public ParametricInfo(Vector2 position, int findex = 0, uint? id = null, float t = 0, FiringCtx? ctx = null) {
        loc = position;
        index = findex;
        this.id = id ?? RNG.GetUInt();
        this.t = t;
        this.ctx = ctx ?? FiringCtx.New();
    }

    public ParametricInfo Rehash() => new ParametricInfo(loc, index, RNG.Rehash(id), t, ctx);
    public ParametricInfo CopyWithT(float newT) => new ParametricInfo(loc, index, id, newT, ctx);

    public ParametricInfo CopyCtx(uint newId) => new ParametricInfo(loc, index, newId, t, ctx.Copy());
    
    /// <summary>
    /// Flips the position around an X or Y axis.
    /// </summary>
    /// <param name="y">Iff true, flips Y values around an X axis. Else, flips X values around a Y axis.</param>
    /// <param name="around">Location of the axis.</param>
    public void FlipSimple(bool y, float around) {
        if (y) {
            loc.y = 2 * around - loc.y;
        } else {
            loc.x = 2 * around - loc.x;
        }
    }

    public void Dispose() {
        ctx.Dispose();
        //Prevents double dispose
        ctx = FiringCtx.Empty;
    }
}

//Note: ref mov/ in dT/ ref bpi/ out delta are significant optimizations.
// (I don't know why in float is so significant. Probably because in the SimpleBullet case
// it's read from the same memory location for all bullets within a pool. That would be good cache performance.)
//ref bpi is used over in bpi because there are methods on bpi (copyWithP, copyWithT, etc) that
// would trigger defensive struct copies. (Methods and properties both trigger defensive copies.)
//ref mov is used for the same reason, though no such methods/properties currently exist.

/// <summary>
/// A function that converts ParametricInfo into a possibly-rotated Cartesian coordinate.
/// </summary>
public delegate void CoordF(float cos, float sin, ParametricInfo bpi, out Vector2 vec);
/// <summary>
/// A function that converts ParametricInfo into a possibly-rotated Cartesian coordinate
/// representing the next position that the Velocity struct should take with a timestep of dT.
/// </summary>
public delegate void VTP(ref Movement vel, in float dT, ref ParametricInfo bpi, out Vector2 delta);
/// <summary>
/// A function that converts ParametricInfo into a possibly-rotated Cartesian coordinate
/// representing the next position that the LaserVelocity struct should take with a timestep of dT
/// and a laser lifetime of lT.
/// </summary>
public delegate void LVTP(ref LaserMovement vel, in float dT, in float lT, ref ParametricInfo bpi, out Vector2 delta);


public readonly struct RootedVTP {
    public readonly GCXF<Vector2> root;
    public readonly GCXU<VTP> path;

    public RootedVTP(GCXF<Vector2> root, GCXU<VTP> path) {
        this.root = root;
        this.path = path;
    }

    public RootedVTP(GCXF<Vector2> root, ExVTP path) : this(root, Compilers.GCXU(path)) { }

    public RootedVTP(ExBPY x, ExBPY y, ExVTP path) : this(Parametrics.PXY(x, y), path) { }
    public RootedVTP(ExTP root, ExVTP path) : this(Compilers.GCXF(root), Compilers.GCXU(path)) { }
    public RootedVTP(float x, float y, ExVTP path) : this(_ => new Vector2(x, y), path) { }
}

/// <summary>
/// A function that converts ParametricInfo into a Vector2.
/// </summary>
public delegate Vector2 TP(ParametricInfo bpi);
/// <summary>
/// A function that converts a SimpleBullet into a Vector2.
/// </summary>
public delegate Vector2 SBV2(ref BulletManager.SimpleBullet sb);

/// <summary>
/// A function that converts ParametricInfo into a Vector3.
/// </summary>
public delegate Vector3 TP3(ParametricInfo bpi);

/// <summary>
/// A function that converts ParametricInfo into a Vector4.
/// </summary>
public delegate Vector4 TP4(ParametricInfo bpi);

/// <summary>
/// A function that converts ParametricInfo into a float.
/// </summary>
public delegate float BPY(ParametricInfo bpi);
/// <summary>
/// A function that converts a SimpleBullet into a float.
/// </summary>
public delegate float SBF(ref BulletManager.SimpleBullet sb);

/// <summary>
/// A function that converts ParametricInfo into a V2RV2.
/// </summary>
public delegate V2RV2 BPRV2(ParametricInfo bpi);

/// <summary>
/// A function that converts ParametricInfo into a boolean.
/// </summary>
public delegate bool Pred(ParametricInfo bpi);
/// <summary>
/// A function that converts ParametricInfo and a laser lifetime into a Vector2.
/// </summary>
public delegate bool LPred(ParametricInfo bpi, float lT);

/// <summary>
/// A wrapper type used for functions that operate over a GCX.
/// </summary>
/// <typeparam name="T">Return object type (eg. float, v2, rv2)</typeparam>
public delegate T GCXF<T>(GenCtx gcx);

/// <summary>
/// A wrapper type used to upload values from a GCX to private data hoisting before providing a delegate to a new object.
/// </summary>
/// <typeparam name="Fn">Delegate type (eg. TP, BPY, Pred)</typeparam>
public delegate Fn GCXU<Fn>(GenCtx gcx, FiringCtx fctx);

//Note: we don't use ref SB because some operations, like deletion and time modification,
//require access to sbc, ii.
/// <summary>
/// A bullet control function performing some operation on a SimpleBullet.
/// </summary>
public delegate void SBCF(BulletManager.AbsSimpleBulletCollection sbc, int ii, ParametricInfo bpi);

/// <summary>
/// A pool control function performing some operation on a simple bullet pool.
/// </summary>
public delegate void SPCF(string pool);

}