﻿using System;
using UnityEngine;

public abstract class Ref<T> {
    public bool useConstant = true;
    public T constVal;

    public static implicit operator T(Ref<T> t) {
        return t.Get();
    }

    public T Get() {
        return useConstant ? constVal : GetRef();
    }
    public bool Set(T t) {
        if (useConstant) {
            return false;
        }
        GetRef().Set(t);
        return true;
    }

    public abstract SO<T> GetRef();
}

[Serializable]
public class RFloat : Ref<float> {
    public SOFloat refVal;
    public override SO<float> GetRef() { return refVal; }
}

[Serializable]
public class RInt : Ref<int> {
    public SOInt refVal;
    public override SO<int> GetRef() { return refVal; }
}

[Serializable]
public class RBool : Ref<bool> {
    public SOBool refVal;
    public override SO<bool> GetRef() { return refVal; }
}

[Serializable]
public class RString : Ref<string> {
    public SOString refVal;
    public override SO<string> GetRef() => refVal;
}

[Serializable]
public class RColor : Ref<Color> {
    public SOColor refVal;
    public override SO<Color> GetRef() => refVal;
}
[Serializable]
public class RColor2 : Ref<Color2> {
    public SOColor2 refVal;
    public override SO<Color2> GetRef() => refVal;
}