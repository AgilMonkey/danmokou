﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DMath;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace Danmaku.DanmakuUI {

public enum NodeState {
    Focused,
    Selected,
    GroupFocused,
    Visible,
    Invisible
}

public class UIScreen {
    public UINode[] top;
    public virtual UINode First => top[0];
    [CanBeNull] public UIScreen calledBy { get; private set; }
    [CanBeNull] public UINode lastCaller { get; private set; }

    public UINode GoToNested(UINode caller, UINode target) {
        lastCaller = caller;
        target.screen.calledBy = this;
        target.screen.onEnter?.Invoke();
        onExit?.Invoke();
        return target;
    }

    public UINode StartingNode => lastCaller ?? top[0];

    [CanBeNull]
    public UINode GoBack() {
        if (calledBy?.StartingNode != null) {
            onExit?.Invoke();
            calledBy.onEnter?.Invoke();
        }
        return calledBy?.StartingNode;
    }

    public void RunPreExit() => onPreExit?.Invoke();

    public UIScreen(params UINode[] nodes) {
        top = nodes.Where(x => x != null).ToArray();
        foreach (var n in top) n.Siblings = top;
        foreach (var n in ListAll()) n.screen = this;
    }

    protected UINode[] AssignNewNodes(UINode[] nodes) {
        top = nodes;
        foreach (var n in top) n.Siblings = top;
        foreach (var n in ListAll()) n.screen = this;
        BuildChildren(cachedBuildMap);
        return top;
    }

    public IEnumerable<UINode> ListAll() => top.SelectMany(x => x.ListAll());
    public bool HasNode(UINode x) => ListAll().Contains(x);

    public void ResetStates() {
        foreach (var n in ListAll()) n.state = NodeState.Invisible;
    }

    public void ApplyStates() {
        foreach (var n in ListAll()) n.ApplyState();
    }

    public void ResetNodes() {
        foreach (var n in ListAll()) n.Reset();
    }

    public VisualElement Bound { get; private set; }
    public List<ScrollView> Lists => Bound.Query<ScrollView>().ToList();
    private Dictionary<Type, VisualTreeAsset> cachedBuildMap;

    public VisualElement Build(Dictionary<Type, VisualTreeAsset> map) {
        cachedBuildMap = map;
        Bound = (overrideBuilder == null ? map[typeof(UIScreen)] : overrideBuilder).CloneTree();
        BuildChildren(map);
        return Bound;
    }
    private void BuildChildren(Dictionary<Type, VisualTreeAsset> map) {
        var lists = Lists;
        foreach (var node in ListAll()) {
            node.Build(map, lists[node.Depth]);
        }
    }

    [CanBeNull] private VisualTreeAsset overrideBuilder;

    public UIScreen With(VisualTreeAsset builder) {
        overrideBuilder = builder;
        return this;
    }
    
    [CanBeNull] private Action onPreExit;
    /// <summary>
    /// This is run on exit transition start
    /// </summary>
    public UIScreen OnPreExit(Action cb) {
        onPreExit = cb;
        return this;
    }
    [CanBeNull] private Action onExit;
    /// <summary>
    /// This is run at exit transition midpoint
    /// </summary>
    public UIScreen OnExit(Action cb) {
        onExit = cb;
        return this;
    }

    [CanBeNull] private Action onEnter;
    public UIScreen OnEnter(Action cb) {
        onEnter = cb;
        return this;
    }
}

public class LazyUIScreen : UIScreen {
    private readonly Func<UINode[]> loader;
    public override UINode First => (top.Length > 0 ? top : AssignNewNodes(loader()))[0];

    public LazyUIScreen(Func<UINode[]> loader) : base() {
        this.loader = loader;
    }
}

public class UINode {
    public readonly UINode[] children;
    [CanBeNull] public UINode Parent { get; private set; }
    public UINode[] Siblings { get; set; } //including self
    [CanBeNull] private UINode[] _sameDepthSiblings;
    public UINode[] SameDepthSiblings =>
        _sameDepthSiblings = _sameDepthSiblings ?? Siblings.Where(s => s.Depth == Depth).ToArray();


    public string Description => descriptor();
    //sorry but there's a use case where i need to modify this in the initializer. see TextInputNode
    protected Func<string> descriptor;
    public UIScreen screen;
    public NodeState state = NodeState.Invisible;

    public UINode(string description, params UINode[] children) : this(() => description, children) { }
    public UINode(Func<string> descriptor, params UINode[] children) {
        this.children = children;
        this.descriptor = descriptor;
        foreach (var c in children) {
            c.Parent = this;
            c.Siblings = children;
        }
    }

    public IEnumerable<UINode> ListAll() => children.SelectMany(n => n.ListAll()).Prepend(this);

    public int Depth => Parent?.ChildDepth ?? 0;
    protected virtual int ChildDepth => 1 + Depth;

    protected static void AssignParentingStates(UINode p) {
        for (; p != null; p = p.Parent) {
            foreach (var x in p.SameDepthSiblings) x.state = NodeState.Visible;
            p.state = NodeState.Selected;
        }
    }
    protected virtual void AssignParentStatesFromSelected() => AssignParentingStates(Parent);
    public virtual void AssignStatesFromSelected() {
        screen.ResetStates();
        foreach (var x in children) x.state = NodeState.Visible;
        foreach (var x in SameDepthSiblings) x.state = NodeState.GroupFocused;
        this.state = NodeState.Focused;
        AssignParentStatesFromSelected();
        screen.ApplyStates();
    }

    private readonly List<string> overrideClasses = new List<string>();
    private readonly List<Action<VisualElement>> overrideInline = new List<Action<VisualElement>>();

    public UINode With(params string[] clss) {
        foreach (var cls in clss) {
            if (!string.IsNullOrWhiteSpace(cls)) overrideClasses.Add(cls);
        }
        return this;
    }

    public UINode With(params Action<VisualElement>[] inline) {
        foreach (var func in inline) {
            if (func != null) overrideInline.Add(func);
        }
        return this;
    }

    private const string disabledClass = "disabled";

    [CanBeNull] private Func<bool> enableCheck;
    public UINode EnabledIf(Func<bool> s) {
        enableCheck = s;
        return this;
    }
    public UINode EnabledIf(bool s) => EnabledIf(() => s);

    public void ApplyState() {
        BindText();
        boundNode.ClearClassList();
        foreach (var c in boundClasses) {
            boundNode.AddToClassList(c);
        }
        boundNode.AddToClassList(ToClass(state));
        foreach (var cls in overrideClasses) {
            boundNode.AddToClassList(cls);
        }
        foreach (var inline in overrideInline) {
            inline(boundNode);
        }
        confirmEnabled = (enableCheck?.Invoke() ?? true);
        if (!confirmEnabled) boundNode.AddToClassList(disabledClass);
    }

    private bool confirmEnabled = true; //otherwise doesn't work with ReturnTo
    
    public virtual void Reset() { }

    [CanBeNull] private Func<UINode> _overrideRight;
    public UINode SetRightOverride(Func<UINode> overr) {
        _overrideRight = overr;
        return this;
    }

    public virtual UINode Right() => _overrideRight?.Invoke() ?? children.Try(0) ?? this;
    
    [CanBeNull] private Func<UINode> _overrideLeft;
    public UINode SetLeftOverride(Func<UINode> overr) {
        _overrideLeft = overr;
        return this;
    }

    [CanBeNull] public virtual UINode CustomEventHandling() => null;
    public virtual UINode Left() => _overrideLeft?.Invoke() ?? Parent ?? this;
    
    [CanBeNull] private Func<UINode> _overrideUp;
    public UINode SetUpOverride(Func<UINode> overr) {
        _overrideUp = overr;
        return this;
    }
    public virtual UINode Up() => _overrideUp?.Invoke() ??
                                  SameDepthSiblings.ModIndex(SameDepthSiblings.IndexOf(this) - 1);
    
    [CanBeNull] private Func<UINode> _overrideDown;
    public UINode SetDownOverride(Func<UINode> overr) {
        _overrideDown = overr;
        return this;
    }
    public virtual UINode Down() => _overrideDown?.Invoke() ?? 
                                    SameDepthSiblings.ModIndex(SameDepthSiblings.IndexOf(this) + 1);

    [CanBeNull] private Func<UINode> _overrideBack;
    public UINode SetBackOverride(Func<UINode> overr) {
        _overrideBack = overr;
        return this;
    }

    public virtual UINode Back() => _overrideBack?.Invoke() ?? screen.calledBy?.StartingNode ?? this;

    [CanBeNull] private Func<bool> _passthrough;
    public bool Passthrough => _passthrough?.Invoke() ?? false;

    public UINode PassthroughIf(Func<bool> passthrough) {
        _passthrough = passthrough;
        return this;
    }

    [CanBeNull] private Action<UINode> _onVisit = null;

    [CanBeNull] private Action<UINode> _onLeave = null;

    public UINode SetOnVisit(Action<UINode> onVisit) {
        _onVisit = onVisit;
        return this;
    }
    public void OnVisit(UINode prev) {
        _onVisit?.Invoke(prev);
    }
    public UINode SetOnLeave(Action<UINode> onLeave) {
        _onLeave = onLeave;
        return this;
    }
    public void OnLeave(UINode prev) {
        _onLeave?.Invoke(prev);
    }

    protected virtual (bool success, UINode target) _Confirm() => _overrideConfirm?.Invoke() ?? (false, this);

    [CanBeNull] private Func<(bool, UINode)> _overrideConfirm;
    public UINode SetConfirmOverride(Func<(bool, UINode)> overr) {
        _overrideConfirm = overr;
        return this;
    }

    public (bool success, UINode target) Confirm() {
        if (confirmEnabled) {
            var (success, target) = _Confirm();
            if (!success || target == null || screen.HasNode(target)) return (success, target);
            else return (true, screen.GoToNested(this, target));
        } else return (false, this);
    }

    public (bool success, UINode target) Confirm_DontNest() =>
        confirmEnabled ? _Confirm() : (false, this);

    protected const string NodeClass = "node";

    [CanBeNull] private Func<bool?> _visible;

    public UINode VisibleIf(Func<bool?> visible) {
        _visible = visible;
        _passthrough = _passthrough ?? (() => _visible?.Invoke() == false);
        return this;
    }
    private string ToClass(NodeState s) {
        var visOverride = _visible?.Invoke();
        if (visOverride == false) return "invisible";
        if (s == NodeState.Focused) return "focus";
        else if (s == NodeState.Selected) return "selected";
        else if (s == NodeState.GroupFocused || visOverride == true) return "group";
        else if (s == NodeState.Invisible) return "invisible";
        else if (s == NodeState.Visible) return "visible";
        throw new Exception($"Couldn't resolve nodeState {s}");
    }

    public VisualElement Bound => bound;
    public VisualElement BoundN => boundNode;
    protected VisualElement bound;
    protected VisualElement boundNode;
    private ScrollView scroll;
    private string[] boundClasses;

    public void ScrollTo() => scroll.ScrollTo(bound);
    
    protected virtual void BindText() => bound.Q<Label>().text = Description;

    protected VisualElement BindScroll(ScrollView scroller) {
        (scroll = scroller).Add(bound);
        return bound;
    }

    public VisualElement Build(Dictionary<Type, VisualTreeAsset> map, ScrollView scroller) {
        CloneTree(map);
        BindText();
        return BindScroll(scroller);
    }

    protected void CloneTree(Dictionary<Type, VisualTreeAsset> map) {
        bound = (overrideBuilder == null ? map.SearchByType(this) : overrideBuilder).CloneTree();
        boundNode = bound.Q<VisualElement>(null, NodeClass);
        boundClasses = boundNode.GetClasses().ToArray();
    }

    [CanBeNull] private VisualTreeAsset overrideBuilder;

    public UINode With(VisualTreeAsset builder) {
        overrideBuilder = builder;
        return this;
    }
    
    protected List<int> CacheCurrent() {
        List<int> revInds = new List<int>();
        var c = this;
        while (c != null) {
            revInds.Add(c.Siblings.IndexOf(c));
            c = c.Parent ?? c.screen.calledBy?.lastCaller;
        }
        revInds.Reverse();
        return revInds;
    }
}

public class NavigateUINode : UINode {
    
    public NavigateUINode(string description, params UINode[] children) : base(description, children) {}
    public NavigateUINode(Func<string> description, params UINode[] children) : base(description, children) {}
    protected override (bool success, UINode target) _Confirm() {
        //default going right
        var n = Right();
        return n != this ? (true, n) : base._Confirm();
    }
}

public class CacheNavigateUINode : NavigateUINode {
    private readonly Action<List<int>> cacher;
    public CacheNavigateUINode(Action<List<int>> cacher, string description, params UINode[] children) :
        base(description, children) {
        this.cacher = cacher;
    }
    public CacheNavigateUINode(Action<List<int>> cacher, Func<string> description, params UINode[] children) :
        base(description, children) {
        this.cacher = cacher;
    }
    protected override (bool success, UINode target) _Confirm() {
        cacher(CacheCurrent());
        return base._Confirm();
    }

    public override UINode Right() {
        cacher(CacheCurrent());
        return base.Right();
    }
}

public class TransferNode : UINode {

    private readonly UIScreen screen_target;

    public TransferNode(UIScreen target, string description, params UINode[] children) : base(description, children) {
        this.screen_target = target;
    }

    protected override (bool success, UINode target) _Confirm() {
        return (true, screen_target.First);
    }
}

public class FuncNode : UINode {

    protected readonly Func<bool> target;
    protected readonly UINode next;

    public FuncNode(Func<bool> target, string description, bool returnSelf=false, params UINode[] children) : base(description, children) {
        this.target = target;
        this.next = returnSelf ? this : null;
    }
    public FuncNode(Func<bool> target, Func<string> description, bool returnSelf=false, params UINode[] children) : base(description, children) {
        this.target = target;
        this.next = returnSelf ? this : null;
    }

    public FuncNode(Action target, string description, bool returnSelf = false, params UINode[] children) : this(() => {
        target();
        return true;
    }, description, returnSelf, children) { }

    public FuncNode(Action target, string description, UINode next, params UINode[] children) : this(() => {
        target();
        return true;
    }, description, true, children) {
        this.next = next;
    }

    protected override (bool success, UINode target) _Confirm() => (target(), next);
}

public class OpenUrlNode : FuncNode {

    public OpenUrlNode(string site, string description) : base(() => Application.OpenURL(site), description, true) { }
}

public class ConfirmFuncNode : FuncNode {
    private bool isConfirm;
    public ConfirmFuncNode(Action target, string description, bool returnSelf=false, params UINode[] children) : base(target, description, returnSelf, children) { }
    public ConfirmFuncNode(Func<bool> target, string description, bool returnSelf=false, params UINode[] children) : base(target, description, returnSelf, children) { }

    private void SetConfirm(bool newConfirm) {
        isConfirm = newConfirm;
    }
    protected override void BindText() {
        bound.Q<Label>().text = isConfirm ? "Are you sure?" : Description;
    }
    public override void Reset() {
        SetConfirm(false);
        base.Reset();
    }
    public override UINode Back() {
        SetConfirm(false);
        return base.Back();
    }
    public override UINode Left() {
        SetConfirm(false);
        return base.Left();
    }
    public override UINode Right() {
        SetConfirm(false);
        return base.Right();
    }
    public override UINode Up() {
        SetConfirm(false);
        return base.Up();
    }
    public override UINode Down() {
        SetConfirm(false);
        return base.Down();
    }

    protected override (bool success, UINode target) _Confirm() {
        if (isConfirm) {
            SetConfirm(false);
            return base._Confirm();
        } else {
            SetConfirm(true);
            return (true, this);
        }
    }
}

public class PassthroughNode : UINode {
    public PassthroughNode(string description) : base(description) {
        PassthroughIf(() => true);
    }
    public PassthroughNode(Func<string> description) : base(description) { 
        PassthroughIf(() => true);
    }
}
public class NavigateOptionNodeLR : OptionNodeLR<UINode> {
    private UINode currentVisible;
    protected override Action<UINode> OnChange => node => currentVisible = node;
    protected override int ChildDepth => Depth; //Children will show on this layer with invisibility
    
    public NavigateOptionNodeLR(string description, UINode[] values) : 
        base(description, null, values.Select(u => (u.Description, u)).ToArray(), values[0], values) {
        currentVisible = values[0];
        foreach (var c in children) c.With("invisible");
    }

    public override void AssignStatesFromSelected() {
        currentVisible.AssignStatesFromSelected();
        state = NodeState.Focused;
        screen.ApplyStates();
    }

    public override UINode Down() => currentVisible;
}

public class DelayOptionNodeLR<T> : UINode {
    private readonly Action<T> onChange;
    private readonly Func<(string key, T val)[]> values;
    private int index;
    
    public DelayOptionNodeLR(string description, Action<T> onChange, Func<(string, T)[]> values) : base(description) {
        this.onChange = onChange;
        this.values = values;
        index = 0;
    }

    public override UINode Left() {
        var v = values();
        if (v.Length > 0) index = M.Mod(v.Length, index - 1);
        AssignValueText(v);
        onChange(v[index].val);
        return this;
    }
    public override UINode Right() {
        var v = values();
        if (v.Length > 0) index = M.Mod(v.Length, index + 1);
        AssignValueText(v);
        onChange(v[index].val);
        return this;
    }

    private void AssignValueText((string key, T val)[] vals) {
        bound.Q<Label>("Value").text = vals.Try(index, ("None", default)).key;
    }
    protected override void BindText() {
        bound.Q<Label>("Key").text = Description;
        AssignValueText(values());
    }
}

public class DynamicOptionNodeLR2<T> : UINode {
    private readonly Action<T> onChange;
    private readonly Func<T[]> values;
    private readonly Action<T, VisualElement, bool> binder;
    private readonly VisualTreeAsset objectTree;
    private int index;
    private VisualElement[] boundChildren = new VisualElement[0];
    
    public DynamicOptionNodeLR2(string description, VisualTreeAsset objectTree, Action<T> onChange, Func<T[]> values, Action<T, VisualElement, bool> binder) : base(description) {
        this.onChange = onChange;
        this.values = values;
        this.binder = binder;
        this.objectTree = objectTree;
        index = 0;
    }

    public void ResetIndex() => index = 0;

    public override UINode Left() {
        var v = values();
        if (v.Length > 0) {
            index = M.Mod(v.Length, index - 1);
            onChange(v[index]);
        }
        AssignValueText();
        return this;
    }
    public override UINode Right() {
        var v = values();
        if (v.Length > 0) {
            index = M.Mod(v.Length, index + 1);
            onChange(v[index]);
        }
        AssignValueText();
        return this;
    }

    private void AssignValueText() {
        var v = values();
        if (v.Length != boundChildren.Length) index = 0;
        foreach (var bc in boundChildren) childContainer.Remove(bc);
        boundChildren = v.Select((x, i) => {
            VisualElement t = objectTree.CloneTree();
            childContainer.Add(t);
            binder(x, t, i == index);
            return t;
        }).ToArray();
    }

    private VisualElement childContainer;
    protected override void BindText() {
        childContainer = bound.Q("LR2ChildContainer");
        bound.Q<Label>("Key").text = Description;
        AssignValueText();
    }
}

public class DynamicOptionNodeLR<T> : UINode {
    private readonly Action<T> onChange;
    private readonly Func<(string key, T val)[]> values;
    private int index;
    public int Index => index = M.Clamp(0, values().Length - 1, index);
    public T Value => values()[Index].val;

    public DynamicOptionNodeLR(string description, Action<T> onChange, Func<(string, T)[]> values, T defaulter, params UINode[] children) : base(description, children) {
        this.onChange = onChange;
        this.values = values;
        index = this.values().Enumerate().FirstOrDefault(x => x.Item2.val.Equals(defaulter)).Item1;
    }

    public override UINode Left() {
        var v = values();
        if (v.Length > 0) {
            index = M.Mod(v.Length, index - 1);
            onChange(v[index].val);
            AssignValueText();
        }
        return this;
    }
    public override UINode Right() {
        var v = values();
        if (v.Length > 0) {
            index = M.Mod(v.Length, index + 1);
            onChange(v[index].val);
            AssignValueText();
        }
        return this;
    }

    protected override void BindText() {
        bound.Q<Label>("Key").text = Description;
        AssignValueText();
    }
    private void AssignValueText() {
        bound.Q<Label>("Value").text = values()[Index].key;
    }
}
public class OptionNodeLR<T> : UINode {
    private readonly Action<T> onChange;
    protected virtual Action<T> OnChange => onChange;
    private readonly (string key, T val)[] values;
    private int index;
    public T Value => values[index].val;
    
    public OptionNodeLR(string description, Action<T> onChange, (string, T)[] values, T defaulter, params UINode[] children) : base(description, children) {
        this.onChange = onChange;
        this.values = values;
        index = this.values.Enumerate().First(x => x.Item2.val.Equals(defaulter)).Item1;
    }
    public OptionNodeLR(string description, Action<T> onChange, T[] values, T defaulter, params UINode[] children) : base(description, children) {
        this.onChange = onChange;
        this.values = values.Select(x => (x.ToString(), x)).ToArray();
        index = this.values.Enumerate().First(x => x.Item2.val.Equals(defaulter)).Item1;
    }

    public override UINode Left() {
        index = M.Mod(values.Length, index - 1);
        AssignValueText();
        OnChange(Value);
        return this;
    }
    public override UINode Right() {
        index = M.Mod(values.Length, index + 1);
        AssignValueText();
        OnChange(Value);
        return this;
    }

    protected override void BindText() {
        bound.Q<Label>("Key").text = Description;
        AssignValueText();
    }
    private void AssignValueText() {
        bound.Q<Label>("Value").text = values[index].key;
    }
}

public class TextInputNode : UINode {
    public string DataWIP { get; private set; } = "";
    private int cursorIdx = 0;
    private int bdCursorIdx => Math.Min(cursorIdx, DataWIP.Length);
    private string DisplayWIP => DataWIP.Insert(bdCursorIdx, "|");

    public TextInputNode(string title) : base((Func<string>) null) {
        descriptor = () => $"{title}: {DisplayWIP}";
    }

    private static readonly string[] alphanumeric = 
        "abcdefghijklmnopqrstuvwxyz0123456789".Select(x => x.ToString()).ToArray();
    
    public override UINode CustomEventHandling() {
        foreach (var kc in alphanumeric) {
            if (Input.GetKeyDown(kc)) {
                DataWIP = DataWIP.Insert(bdCursorIdx, (Input.GetKey(KeyCode.LeftShift) || 
                                                       Input.GetKey(KeyCode.RightShift)) ? kc.ToUpper() : kc);
                ++cursorIdx;
                return this;
            }
        }
        if (Input.GetKeyDown(KeyCode.Backspace)) {
            DataWIP =
                ((cursorIdx > 1) ? DataWIP.Substring(0, cursorIdx - 1) : "") +
                ((cursorIdx < DataWIP.Length) ? DataWIP.Substring(cursorIdx) : "");
            cursorIdx = Math.Max(0, cursorIdx - 1);
            return this;
        } else if (Input.GetKeyDown(KeyCode.Return)) {
            return Parent;
        } else return null;
    }

    public override UINode Left() {
        cursorIdx = Math.Max(0, cursorIdx - 1);
        return this;
    }
    public override UINode Right() {
        cursorIdx = Math.Min(DataWIP.Length, cursorIdx + 1);
        return this;
    }
    
}

}