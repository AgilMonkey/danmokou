﻿using System;
using System.Collections.Generic;
using Danmokou.Core;
using UnityEngine;

namespace Danmokou.Behavior {
public abstract class RegularUpdater : MonoBehaviour, IRegularUpdater {
    private readonly List<IDeletionMarker> tokens = new List<IDeletionMarker>();
    protected bool Enabled => tokens.Count > 0;

    /// <summary>
    /// Safe to call twice.
    /// </summary>
    protected void EnableUpdates() {
        if (!Enabled) {
            tokens.Add(ETime.RegisterRegularUpdater(this));
            BindListeners();
        }
    }

    protected virtual void BindListeners() { }

    protected void Listen<T>(Events.IEvent<T> ev, Action<T> sub) {
        tokens.Add(ev.Subscribe(sub));
    }
    
    /// <summary>
    /// Invoke the function with the last published value, and then listen to future changes.
    /// </summary>
    protected void ListenInv<T>(Events.IEvent<T> ev, Action<T> sub) {
        if (ev.LastPublished.valid)
            sub(ev.LastPublished.value);
        Listen(ev, sub);
    }
    protected void ListenInv<T>(Events.IEvent<T> ev, Action sub) {
        sub();
        Listen(ev, _ => sub());
    }

    protected void Listen(Events.Event0 ev, Action sub) {
        tokens.Add(ev.Subscribe(sub));
    }
    
    /// <summary>
    /// Invoke the function, and then listen to future changes.
    /// </summary>
    protected void ListenInv(Events.Event0 ev, Action sub) {
        sub();
        Listen(ev, sub);
    }
    
    protected void RegisterDI<T>(T me) where T : class => tokens.Add(DependencyInjection.Register(me));

    protected virtual void OnEnable() => EnableUpdates();

    public abstract void RegularUpdate();
    public virtual void RegularUpdateParallel() { }
    public virtual void FirstFrame() { }
    public virtual int UpdatePriority => UpdatePriorities.DEFAULT;

    public virtual bool UpdateDuringPause => false;


    public virtual void PreSceneClose() { }

    /// <summary>
    /// Safe to call twice.
    /// </summary>
    protected void DisableUpdates() {
        for (int ii = 0; ii < tokens.Count; ++ii) {
            tokens[ii].MarkForDeletion();
        }
        tokens.Clear();
    }

    protected virtual void OnDisable() => DisableUpdates();

    protected void DisableDestroy() {
        DisableUpdates();
        Destroy(gameObject);
    }
}
}
