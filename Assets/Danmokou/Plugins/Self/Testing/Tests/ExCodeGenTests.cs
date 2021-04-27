﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DMK.Core;
using DMK.Danmaku;
using DMK.Danmaku.Patterns;
using DMK.DataHoist;
using DMK.DMath;
using DMK.DMath.Functions;
using DMK.Expressions;
using DMK.Reflection;
using DMK.SM;
using FastExpressionCompiler;
using NUnit.Framework;
using UnityEngine;
using Ex = System.Linq.Expressions.Expression;
using ExTP = System.Func<DMK.Expressions.TExArgCtx, DMK.Expressions.TEx<UnityEngine.Vector2>>;

namespace DMK.Testing {

public class ExCodeGenTests {
    

    [Test]
    public void TestPrivateHoist() {
        var ex = "sine 13h 0.4 + * 13 &fitr / p ^ dl 1.2".Into<Func<TExArgCtx, TEx<float>>>();
        var exc = GCXFRepo._Fake(ex);
        var _gcx = TExArgCtx.Arg.Make<GenCtx>("gcx", true);
        var tac = new TExArgCtx(_gcx);
        exc(tac).BakeAndCompile<GCXF<float>>(tac, _gcx.expr);
            
        var ex2 = @"sync sun-red/w <2;:> gsr {
	start gcxVar =v2 pxy(1, 2)
} s offset
	:: {
		letVar1 0
		letVar2 (-1 + &letVar1)
	} px &letVar2
	:: {
		letVar1 [&gcxVar].x
		letVar2 (1 + &letVar1)
	} pxy
		@0 publicVar
		ss0 &letVar2".Into<StateMachine>();
        //var s = BakeCodeGenerator.Generated;

    }

    
    public void TestCosSin() {
        var dct = new Dictionary<int, int>();
        var ex = Ex.Constant(dct);

        var block = Ex.Block(new ParameterExpression[0],
            Ex.Call(ex, "Clear", new Type[0]),
            Ex.Constant(true)
        );
        var block2 = Ex.Block(new ParameterExpression[0],
            Ex.Call(ex, "Clear", new Type[0])
        );

        var ifex = Ex.Block(new ParameterExpression[0],
            Ex.IfThenElse(block, block2, block2),
            Ex.Constant(2f));
        var ifexs = ifex.ToCSharpString();
        //if: only the condition needs to be linearized
        //cond: condition, true, false need to be linarized
        
        //the library is kinda broken in that if, if/else statements will always print return statements
        //if the last statement is not a void type...
        //in other words it doesn't dewal with blocks well
        

    }
    

    
}
}