﻿using System;
using System.Collections;
using System.Collections.Generic;
using DMK.Behavior;
using DMK.Core;
using DMK.Danmaku;
using DMK.GameInstance;
using DMK.Player;
using DMK.Scriptables;
using JetBrains.Annotations;
using DMK.SM;
using static DMK.GameInstance.Challenge;

namespace DMK.Services {
public interface IChallengeManager {
    float? BossTimeoutOverride([CanBeNull] BossConfig bc);
    void TrackChallenge(IChallengeRequest cr, Action<InstanceRecord> onSucess);
    void SetupBossPhase(SMHandoff smh);
    void LinkBoss(BehaviorEntity exec);
    IEnumerable<AyaPhoto> ChallengePhotos { get; }

    ChallengeManager.Restrictions Restriction { get; }
}

public class ChallengeManager : CoroutineRegularUpdater, IChallengeManager {
    private readonly List<AyaPhoto> challengePhotos = new List<AyaPhoto>();
    public IEnumerable<AyaPhoto> ChallengePhotos => challengePhotos;

    private PhaseCompletion? completion = null;
    [CanBeNull] private IChallengeRequest tracking = null;
    [CanBeNull] private BehaviorEntity Exec { get; set; }

    protected override void BindListeners() {
        base.BindListeners();
        Listen(InstanceData.PhaseCompleted, pc => {
            if (pc.clear != PhaseClearMethod.CANCELLED && pc.props.phaseType != null && pc.exec == Exec)
                completion = pc;
        });
        Listen(AyaCamera.PhotoTaken, photo => {
            //There are no restrictions on what type of challenge may receive a photo
            if (tracking != null && photo.success) {
                challengePhotos.Add(photo.photo);
            }
        });
        RegisterDI<IChallengeManager>(this);
    }

    private void OnDestroy() {
        CleanupState();
    }

    private void CleanupState() {
        completion = null;
        Exec = null;
        tracking = null;
        Restriction = new Restrictions();
    }

    public float? BossTimeoutOverride([CanBeNull] BossConfig bc) =>
        (tracking?.ControlsBoss(bc) == true) ? Restriction.TimeoutOverride : null;



    public class Restrictions {
        public static readonly Restrictions Default = new Restrictions();
        public readonly bool HorizAllowed = true;
        public readonly bool VertAllowed = true;

        public readonly bool FocusAllowed = true;
        public readonly bool FocusForced = false;

        public readonly float? TimeoutOverride = null;

        public Restrictions() { }

        public Restrictions(Challenge[] cs) {
            foreach (var c in cs) {
                if (c is NoHorizC)
                    HorizAllowed = false;
                else if (c is NoVertC)
                    VertAllowed = false;
                else if (c is NoFocusC)
                    FocusAllowed = false;
                else if (c is AlwaysFocusC)
                    FocusForced = true;
                else if (c is DestroyTimedC dtc)
                    TimeoutOverride = dtc.time;
            }
        }
    }

    public Restrictions Restriction { get; private set; } = new Restrictions();

    public void SetupBossPhase(SMHandoff smh) {
        if (smh.Exec != Exec || tracking == null) return;
        var cs = tracking.Challenges;
        for (int ii = 0; ii < cs.Length; ++ii) cs[ii].SetupPhase(smh);
    }

    public void LinkBoss(BehaviorEntity exec) {
        if (tracking == null) throw new Exception("Cannot link BEH when no challenge is tracked");
        Log.Unity($"Linked boss {exec.ID} to challenge {tracking.Description}");
        tracking.Start(Exec = exec);
    }

    public void TrackChallenge(IChallengeRequest cr, Action<InstanceRecord> onSuccess) {
        Log.Unity($"Tracking challenge {cr.Description}");
        CleanupState();
        tracking = cr;
        Restriction = new Restrictions(cr.Challenges);
        challengePhotos.Clear();
        cr.Initialize();
        RunDroppableRIEnumerator(TrackChallenges(cr, onSuccess));
    }

    private void ChallengeFailed(IChallengeRequest cr, TrackingContext ctx) {
        cr.OnFail(ctx);
        CleanupState();
    }

    private void ChallengeSuccess(IChallengeRequest cr, TrackingContext ctx) {
        if (cr.OnSuccess(ctx)) CleanupState();
    }

    //This is not controlled by smh.cT because its scope is the entire segment over which the challenge executes,
    //not just the boss phase. In the case of BPoHC stage events, this scope is the phase cT of the stage section.
    private IEnumerator TrackChallenges(IChallengeRequest cr, Action<InstanceRecord> onSuccess) {
        while (Exec == null) yield return null;
        var challenges = cr.Challenges;
        var ctx = new TrackingContext(Exec, this, onSuccess);

        for (; completion == null; ctx.t += ETime.FRAME_TIME) {
            for (int ii = 0; ii < challenges.Length; ++ii) {
                if (!challenges[ii].FrameCheck(ctx)) {
                    ChallengeFailed(cr, ctx);
                    yield break;
                }
            }
            yield return null;
        }
        for (int ii = 0; ii < challenges.Length; ++ii) {
            if (!challenges[ii].EndCheck(ctx, completion.Value)) {
                ChallengeFailed(cr, ctx);
                yield break;
            }
        }
        ChallengeSuccess(cr, ctx);
    }

    public struct TrackingContext {
        public readonly BehaviorEntity exec;
        public readonly ChallengeManager cm;
        public readonly Action<InstanceRecord> onSuccess;
        public float t;

        public TrackingContext(BehaviorEntity exec, ChallengeManager cm, Action<InstanceRecord> onSuccess) {
            this.exec = exec;
            this.cm = cm;
            this.onSuccess = onSuccess;
            this.t = 0;
        }
    }
}
}