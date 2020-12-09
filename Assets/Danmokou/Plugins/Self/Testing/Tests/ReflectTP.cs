﻿using NUnit.Framework;
using DMK.DMath;
using DMK.DMath.Functions;
using DMK.Expressions;
using UnityEngine;
using static DMK.Reflection.Compilers;
using static DMK.DMath.Functions.Parametrics;
using static DMK.DMath.Functions.FXYRepo;
using static DMK.DMath.Functions.BPYRepo;
using static DMK.DMath.Functions.BPRV2Repo;
using static DMK.Testing.TAssert;
using static DMK.DMath.Functions.ExM;
using static DMK.DMath.Functions.ExMV2;
using static DMK.DMath.Functions.ExMRV2;
using static DMK.DMath.Functions.ExMConversions;

namespace DMK.Testing {

    public static class ReflectTP {
        
        [Test]
        public static void TPivot() {
            var pivoter = TP(Pivot(BPYRepo.T(), _ => 3f, bp => PolarToXY(1f, bp.t.Mul(90f)), 
                PX(bp => Mul<float>(bp.t, 2f))));
            var bpi = ParametricInfo.WithRandomId(Vector2.down * 100f, 0);
            bpi.t = 0f;
            TAssert.VecEq(pivoter(bpi), Vector2.right);
            bpi.t = 2f;
            TAssert.VecEq(pivoter(bpi), Vector2.left);
            bpi.t = 3f;
            TAssert.VecEq(pivoter(bpi), Vector2.down);
            bpi.t = 4f;
            TAssert.VecEq(pivoter(bpi), new Vector2(2f, -1f));
            bpi.t = 5f;
            TAssert.VecEq(pivoter(bpi), new Vector2(4f, -1f));
        }

        [Test]
        public static void TestConvert() {
            var pivoter = TP(bp => RV2ToXY(V2V2F(PX(BPYRepo.T())(bp), PY(BPYRepo.P())(bp), BPYRepo.X()(bp))));
            var bpi = new ParametricInfo(Vector2.left * 30, 4, 0, 3.5f);
            VecEq(pivoter(bpi), new V2RV2(3.5f, 0, 0, 4, -30).TrueLocation);
        }
        
        private static Vector2 V2(float x, float y) => new Vector2(x, y);

        [Test]
        public static void TCircle() {
            var circ = TP(x => Circle(4f, 3f, x.t));
            var dcirc = TP(x => DCircle(4f, 3f, x.t));
            var bpi = new ParametricInfo(Vector2.down, 0, 0, 0f);
            VecEq(circ(bpi), V2(3f, 0));
            VecEq(circ(bpi.CopyWithT(1f)), V2(0, 3f));
            VecEq(dcirc(bpi.CopyWithT(1f)), V2(-3f * M.TAU/4f, 0f));
            VecEq(circ(bpi.CopyWithT(2.5f)), V2(-Mathf.Sqrt(4.5f), -Mathf.Sqrt(4.5f))); //3^2 = 2 * sqrt(4.5)^2
        }
    }
}