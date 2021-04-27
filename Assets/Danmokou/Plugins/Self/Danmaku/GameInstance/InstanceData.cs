﻿using System;
using System.Collections.Generic;
using DMK.Behavior;
using DMK.Core;
using DMK.Danmaku;
using DMK.DMath;
using DMK.Player;
using DMK.Scriptables;
using DMK.Services;
using JetBrains.Annotations;

namespace DMK.GameInstance {
public class InstanceData {
    public static bool PowerMechanicEnabled { get; } = false;
    public static bool MeterMechanicEnabled { get; } = true;
    private static int StartLives(InstanceMode mode) {
        if (mode == InstanceMode.CAMPAIGN || mode == InstanceMode.TUTORIAL || mode == InstanceMode.STAGE_PRACTICE) 
            return 7;
        else if (mode.OneLife()) 
            return 1;
        else if (mode == InstanceMode.NULL) 
            return 14;
        else 
            return 1;
    }

    private static int StartBombs(InstanceMode mode) {
        if (mode == InstanceMode.CAMPAIGN || mode == InstanceMode.TUTORIAL || mode == InstanceMode.STAGE_PRACTICE) 
            return 2;
        else if (mode.OneLife()) 
            return 0;
        else 
            return 3;
    }

    private static double StartPower(InstanceMode mode, ShotConfig? shot) {
        if (mode.OneLife() || !PowerMechanicEnabled) 
            return powerMax;
        else if (shot != null) 
            return M.Clamp(powerMin, powerMax, shot.defaultPower);
        else 
            return M.Clamp(powerMin, powerMax, powerDefault);
    }

    private static double StartMeter(InstanceMode mode) {
        if (mode.IsOneCard()) 
            return 0;
        else
            return 0.7;
    }

    public DifficultySettings Difficulty { get; }
    
    private const int defltContinues = 42;
    public const long smallValueItemPoints = 314;
    public const long valueItemPoints = 3142;
    public const decimal smallValueRatio = 0.1m;
    public long MaxScore { get; private set; }
    public long Score { get; private set; }
    private long lastScore;
    public long UIVisibleScore { get; private set; }
    private double remVisibleScoreLerpTime;
    public const double visibleScoreLerpTime = 1f;
    public int Lives { get; private set; }
    public int Bombs { get; private set; }
    public int LifeItems { get; private set; }
    public int NextLifeItems => pointLives.Try(nextItemLifeIndex, 9001);
    public long Graze { get; private set; }
    public double PlayerDamageMultiplier => M.Lerp(0, 3, Difficulty.Counter, 1.20, 1);
    public const double powerMax = 4;
    public const double powerMin = 1;
#if UNITY_EDITOR
    private const double powerDefault = 1000;
#else
    private const double powerDefault = 1;
#endif
    private const double powerDeathLoss = -1;
    private const double powerItemValue = 0.05;
    private const double powerToValueConversion = 2;
    public double Power { get; private set; }
    public int PowerF => (int)Math.Floor(Power);
    public int PowerIndex => PowerF - (int) powerMin;
    public double PIV { get; private set; }
    private double EffectivePIV => PIV + Graze / (double)1337;
    private const double pivPerPPP = 0.01;
    public const double pivFallStep = 0.1;
    public double Faith { get; private set; }
    private double faithLenience;
    public double UIVisibleFaithDecayLenienceRatio { get; private set; }
    private const double faithDecayRate = 0.12;
    public readonly MultiMultiplierD externalFaithDecayMultiplier = new MultiMultiplierD(1, null);
    private double FaithDecayRateMultiplier => (CurrentBoss != null ? 0.666f : 1f) * externalFaithDecayMultiplier.Value;
    private const double faithLenienceFall = 5;
    private const double faithLenienceValue = 0.2;
    private const double faithLeniencePointPP = 0.3;
    private double FaithLenienceGraze => M.Lerp(0, 3, Difficulty.Counter, 0.42, 0.3);
    private const double faithLenienceEnemyDestroy = 0.1;
    private const double faithBoostValue = 0.02;
    private const double faithBoostPointPP = 0.09;
    private double FaithBoostGraze => M.Lerp(0, 3, Difficulty.Counter, 0.033, 0.02);
    
    private const double faithLeniencePhase = 4;
    
    public double Meter { get; private set; }
    public bool MeterEnabled => MeterMechanicEnabled && Difficulty.meterEnabled;
    public bool EnoughMeterToUse => MeterEnabled && Meter >= meterUseThreshold;
    private double MeterBoostGraze => M.Lerp(0, 3, Difficulty.Counter, 0.008, 0.005);
    private const double meterBoostGem = 0.021;
    private const double meterRefillRate = 0.002;
    private const double meterUseRate = 0.314;
    public const double meterUseThreshold = 0.42;
    private const double meterUseInstantCost = 0.042;
    
    public bool MeterInUse { get; private set; }

    public void StartUsingMeter() {
        MeterInUse = true;
        LastMeterStartFrame = ETime.FrameNumber;
    }

    public void StopUsingMeter() {
        MeterInUse = false;
    }
    private double MeterPivPerPPPMultiplier => MeterInUse ? 2 : 1;
    private double MeterScorePerValueMultiplier => MeterInUse ? 2 : 1;
    
    public int Continues { get; private set; }
    public int ContinuesUsed { get; private set; } = 0;
    public bool Continued => ContinuesUsed > 0;
    public int HitsTaken { get; private set; }

    private int nextScoreLifeIndex;
    public long? NextScoreLife => mode.OneLife() ? null : scoreLives.TryN(nextScoreLifeIndex);
    private int nextItemLifeIndex;
    public readonly InstanceMode mode;
    public bool IsCampaign => mode == InstanceMode.CAMPAIGN;
    public bool IsAtleastNormalCampaign => IsCampaign && Difficulty.standard >= FixedDifficulty.Normal;
    
    private PlayerTeam team;
    public PlayerConfig? Player => team.Player;
    public ShotConfig? Shot => team.Shot;
    public Subshot Subshot => team.Subshot;
    public string MultishotString => (Shot != null && Shot.isMultiShot) ? Subshot.Describe() : "";
    
    public CardHistory CardHistory { get; }

    public readonly MultiAdder Lenience = new MultiAdder(0, null);
    public bool Lenient => Lenience.Value > 0;
    public BehaviorEntity? CurrentBoss { get; private set; }
    private ICancellee? CurrentBossCT { get; set; }

    /// <summary>
    /// Only present for campaign-type games
    /// </summary>
    private readonly CampaignConfig? campaign;
    /// <summary>
    /// Present for all games, including "null_campaign" default for unscoped games
    /// </summary>
    public readonly string campaignKey;
    public InstanceRequest? Request { get; }
    public readonly Dictionary<((string, string), int), (int, int)> PreviousSpellHistory;
    
    private static readonly long[] scoreLives = {
        2000000,
        5000000,
        10000000,
        15000000,
        20000000,
        25000000,
        30000000,
        40000000,
        50000000,
        60000000,
        70000000,
        80000000,
        100000000
    };
    private static readonly int[] pointLives = {
        69,
        141,
        224,
        314,
        420,
        618,
        840,
        1084,
        1337,
        1618,
        2048,
        2718,
        3142,
        9001,
        int.MaxValue
    };
    //Miscellaneous stats
    public List<BossConfig> BossesEncountered { get; } = new List<BossConfig>();
    public int EnemiesDestroyed { get; private set; }
    public int TotalFrames { get; private set; }
    public int LastMeterStartFrame { get; private set; }
    public int LastTookHitFrame { get; private set; }
    public int MeterFrames { get; private set; }
    public int SubshotSwitches { get; private set; }
    public int OneUpItemsCollected { get; private set; }

    public InstanceData(InstanceMode mode, InstanceRequest? req = null, long? maxScore = null) {
        this.Request = req;
        //Minor hack to avoid running the SaveData static constructor in the editor during type initialization
        PreviousSpellHistory = (req == null) ? 
            new Dictionary<((string, string), int), (int, int)>() :
            SaveData.r.GetCampaignSpellHistory();
        
        this.mode = mode;
        this.Difficulty = req?.metadata.difficulty ?? GameManagement.defaultDifficulty;
        this.MaxScore = maxScore ?? 9001;
        campaign = req?.lowerRequest.Resolve(cr => cr.campaign.campaign, _ => null!, _ => null!, _ => null!);
        campaignKey = req?.lowerRequest.Resolve(cr => cr.Key, b => b.boss.campaign.Key, s => s.Campaign.key,
            s => s.stage.campaign.Key) ?? "null_campaign";
        team = req?.metadata.team ?? PlayerTeam.Empty;
        if (campaign != null) {
            Lives = campaign.startLives > 0 ? campaign.startLives : StartLives(mode);
        } else {
            Lives = StartLives(mode);
        }
        Lives = Difficulty.startingLives ?? Lives;
        Bombs = StartBombs(mode);
        Power = StartPower(mode, team.Shot);
        CardHistory = new CardHistory();
        this.Score = 0;
        this.PIV = 1;
        Meter = StartMeter(mode);
        nextScoreLifeIndex = 0;
        nextItemLifeIndex = 0;
        remVisibleScoreLerpTime = 0;
        lastScore = 0;
        UIVisibleScore = 0;
        LifeItems = 0;
        Faith = 1f;
        faithLenience = 0f;
        UIVisibleFaithDecayLenienceRatio = 0f;
        Continues = mode.OneLife() ? 0 : defltContinues;
        HitsTaken = 0;
        EnemiesDestroyed = 0;
        Graze = 0;
        CurrentBoss = null;
        MeterInUse = false;
    }
    
    
    public void SetSubshot(Subshot newSubshot) {
        if (team.Shot == null || !team.Shot.isMultiShot)
            UselessPowerupCollected.Proc();
        if (team.Subshot == newSubshot)
            return;
        team.Subshot = newSubshot;
        if (team.Shot != null && team.Shot.isMultiShot)
            ++SubshotSwitches;
        PlayerInput.RequestShotUpdate.Publish((Shot, Subshot));
        CampaignDataUpdated.Proc();
    }

    /// <summary>
    /// Has no effect if the provided player/shot must exist in the team config.
    /// Returns true iff it exists in the team config.
    /// </summary>
    public bool SetPlayer(PlayerConfig player, ShotConfig shot, Subshot? subshot = null) {
        var ind = team.players.IndexOf(x => x == (player, shot));
        if (ind > -1) {
            team.Subshot = subshot ?? team.Subshot;
            team.SelectedIndex = ind;
            PlayerInput.RequestPlayerUpdate.Publish(player);
            PlayerInput.RequestShotUpdate.Publish((shot, team.Subshot));
            CampaignDataUpdated.Proc();
            return true;
        } else {
            return false;
        }
    }

    public bool TryContinue() {
        if (Continues > 0) {
            //We can allow continues in replays! But in the current impl, the watcher will have to press continue.
            //Replayer.Cancel();
            --Continues;
            ++ContinuesUsed;
            Score = lastScore = UIVisibleScore = nextItemLifeIndex = nextScoreLifeIndex = LifeItems = 0;
            CardHistory.Clear();//Partial game is saved when lives=0. Don't double on captures.
            PIV = 1;
            Meter = StartMeter(mode);
            if (campaign != null) {
                Lives = campaign.startLives > 0 ? campaign.startLives : StartLives(mode);
            } else {
                Lives = StartLives(mode);
            }
            Bombs = StartBombs(mode);
            remVisibleScoreLerpTime = Faith = faithLenience = 0;
            CampaignDataUpdated.Proc();
            return true;
        } else return false;
    }

    public (int success, int total)? LookForSpellHistory(string bossKey, int phaseIndex) {
        var key = ((campaignKey, bossKey), phaseIndex);
        return PreviousSpellHistory.TryGetValue(key, out var rate) ? rate : ((int, int)?)null;
    }


    /// <summary>
    /// Delta should be negative.
    /// </summary>
    public bool TryConsumeBombs(int delta) {
        if (Bombs + delta >= 0) {
            Bombs += delta;
            CampaignDataUpdated.Proc();
            return true;
        }
        return false;
    }

    public void SwapLifeScore(long score, bool usePIVMultiplier) {
        AddLives(-1, false);
        if (usePIVMultiplier) score = (long) (score * PIV);
        AddScore(score);
        LifeSwappedForScore.Proc();
        CampaignDataUpdated.Proc();
    }
    public void AddLives(int delta, bool asHit = true) {
        //if (mode == CampaignMode.NULL) return;
        Log.Unity($"Adding player lives: {delta}");
        if (delta < 0 && asHit) {
            ++HitsTaken;
            LastTookHitFrame = ETime.FrameNumber;
            Bombs = Math.Max(Bombs, StartBombs(mode));
            AddPower(powerDeathLoss);
            Meter = 1;
            PlayerTookHit.Proc();
        }
        if (delta < 0 && mode.OneLife()) Lives = 0;
        else Lives = Math.Max(0, Lives + delta);
        if (Lives == 0) {
            //Record failure
            if (Request?.Saveable == true) {
                //Special-case boss practice handling
                if (Request.lowerRequest.Resolve(_ => null, 
                        b => (BossPracticeRequest?) b, _ => null, _ => null).Try(out var bpr)) {
                    CardHistory.Add(new CardRecord() {
                        campaign = bpr.boss.campaign.Key,
                        boss = bpr.boss.boss.key,
                        phase = bpr.phase.index,
                        stars = 0,
                        hits = 1,
                        method = null
                    });
                }
                SaveData.r.RecordGame(new InstanceRecord(Request, this, false));
            }
            EngineStateManager.HandlePlayerDeath();
        }
        CampaignDataUpdated.Proc();
    }

    /// <summary>
    /// Don't use this in the main campaign-- it will interfere with stats
    /// </summary>
    public void SetLives(int to) => AddLives(to - Lives, false);

    private void AddFaith(double delta) => Faith = M.Clamp(0, 1, Faith + delta * Difficulty.faithAcquireMultiplier);
    private void AddFaithLenience(double time) => faithLenience = Math.Max(faithLenience, time);
    public void ExternalLenience(double time) => AddFaithLenience(time);
    private void AddMeter(double delta) {
        var belowThreshold = !EnoughMeterToUse;
        Meter = M.Clamp(0, 1, Meter + delta * Difficulty.meterAcquireMultiplier);
        if (belowThreshold && EnoughMeterToUse && !MeterInUse) {
            MeterNowUsable.Proc();
        }
    }

    public void RefillMeterFrame(PlayerInput.PlayerState state) {
        double rate = 0;
        if (state == PlayerInput.PlayerState.NORMAL) rate = meterRefillRate;
        //meter use handled under TryUseMeterFrame
        AddMeter(rate * ETime.FRAME_TIME);
    }

    public bool TryStartMeter() {
        if (EnoughMeterToUse) {
            Meter -= meterUseInstantCost;
            return true;
        } else return false;
    }

    public bool TryUseMeterFrame() {
        var consume = meterUseRate * Difficulty.meterUsageMultiplier * ETime.FRAME_TIME;
        if (Meter >= consume) {
            Meter -= consume;
            return true;
        } else {
            Meter = 0;
            return false;
        }
    }

    private void AddPower(double delta) {
        if (!PowerMechanicEnabled) return;
        var prevFloor = Math.Floor(Power);
        var prevCeil = Math.Ceiling(Power);
        var prevPower = Power;
        Power = M.Clamp(powerMin, powerMax, Power + delta);
        //1.95 is effectively 1, 2.00 is effectively 2
        if (Power < prevFloor) PowerLost.Proc();
        if (prevPower < prevCeil && Power >= prevCeil) {
            if (Power >= powerMax) PowerFull.Proc();
            else PowerGained.Proc();
        }
        CampaignDataUpdated.Proc();
    }

    /// <summary>
    /// Delta should be negative.
    /// </summary>
    public bool TryConsumePower(double delta) {
        if (!PowerMechanicEnabled) return false;
        if (Power + delta >= powerMin) {
            AddPower(delta);
            return true;
        } else return false;
    }

    private void FullPower() {
        Power = powerMax;
        PowerFull.Proc();
    }
    public void AddPowerItems(int delta) {
        if (!PowerMechanicEnabled || Power >= powerMax) {
            AddValueItems((int)(delta * powerToValueConversion), 1);
        } else AddPower(delta * powerItemValue);
    }

    public void AddFullPowerItems(int _) {
        FullPower();
    }
    public void AddValueItems(int delta, double multiplier) {
        AddFaith(delta * faithBoostValue);
        AddFaithLenience(faithLenienceValue);
        double bonus = MeterScorePerValueMultiplier;
        long scoreDelta = (long) Math.Round(delta * valueItemPoints * bonus * EffectivePIV * multiplier);
        AddScore(scoreDelta);
        Events.ScoreItemHasReceived.Publish((scoreDelta, bonus > 1));
    }
    public void AddSmallValueItems(int delta, double multiplier) {
        AddFaith(delta * faithBoostValue * 0.1);
        AddFaithLenience(faithLenienceValue * 0.1);
        double bonus = MeterScorePerValueMultiplier;
        long scoreDelta = (long) Math.Round(delta * smallValueItemPoints * bonus * EffectivePIV * multiplier);
        AddScore(scoreDelta);
        Events.ScoreItemHasReceived.Publish((scoreDelta, bonus > 1));
    }
    public void AddGraze(int delta) {
        Graze += delta;
        AddFaith(delta * FaithBoostGraze);
        AddFaithLenience(FaithLenienceGraze);
        AddMeter(delta * MeterBoostGraze);
        Counter.GrazeProc(delta);
        CampaignDataUpdated.Proc();
    }

    public void AddPointPlusItems(int delta) {
        PIV += pivPerPPP * MeterPivPerPPPMultiplier * delta;
        AddFaith(delta * faithBoostPointPP);
        AddFaithLenience(faithLeniencePointPP);
        CampaignDataUpdated.Proc();
    }

    public void AddGems(int delta) {
        AddMeter(delta * meterBoostGem);
    }

    public void AddOneUpItem() {
        ++OneUpItemsCollected;
        LifeExtend();
    }

    private void LifeExtend() {
        ++Lives;
        AnyExtendAcquired.Proc();
        CampaignDataUpdated.Proc();
    }

    public void PhaseEnd(PhaseCompletion pc) {
        if (pc.props.phaseType?.IsCard() == true && pc.props.Boss != null && pc.CaptureStars.Try(out var captured)) {
            CardHistory.Add(new CardRecord() {
                campaign = campaignKey,
                boss = pc.props.Boss.key,
                phase = pc.props.Index,
                stars = captured,
                hits = pc.hits,
                method = pc.clear
            });
            CardHistoryUpdated.Proc();
        }
        if (pc.props.phaseType?.IsPattern() ?? false) AddFaithLenience(faithLeniencePhase);

        PhaseCompleted.Publish(pc);
    }
    
    private void AddScore(long delta) {
        lastScore = UIVisibleScore;
        Score += delta;
        MaxScore = Math.Max(MaxScore, Score);
        if (NextScoreLife.Try(out var next) && Score >= next) {
            ++nextScoreLifeIndex;
            LifeExtend();
            ScoreExtendAcquired.Proc();
            CampaignDataUpdated.Proc();
        }
        remVisibleScoreLerpTime = visibleScoreLerpTime;
        //updated in RegUpd
    }
    public void AddLifeItems(int delta) {
        LifeItems += delta;
        if (nextItemLifeIndex < pointLives.Length && LifeItems >= pointLives[nextItemLifeIndex]) {
            ++nextItemLifeIndex;
            LifeExtend();
            ItemExtendAcquired.Proc();
        }
        CampaignDataUpdated.Proc();
    }

    public void DestroyNormalEnemy() {
        ++EnemiesDestroyed;
        AddFaithLenience(faithLenienceEnemyDestroy);
    }

    public void _RegularUpdate() {
        ++TotalFrames;
        if (MeterInUse)
            ++MeterFrames;
        if (CurrentBossCT?.Cancelled == true) {
            CloseBoss();
        }
        if (remVisibleScoreLerpTime > 0) {
            remVisibleScoreLerpTime -= ETime.FRAME_TIME;
            if (remVisibleScoreLerpTime <= 0) UIVisibleScore = Score;
            else UIVisibleScore = (long) M.LerpU(lastScore, Score, 1 - remVisibleScoreLerpTime / visibleScoreLerpTime);
            CampaignDataUpdated.Proc();
        }
        UIVisibleFaithDecayLenienceRatio = M.LerpU(UIVisibleFaithDecayLenienceRatio, 
            Math.Min(1f, faithLenience / 3f), 6f * ETime.FRAME_TIME);
        if (PlayerInput.PlayerActive && !Lenient && EngineStateManager.IsRunning) {
            if (faithLenience > 0) {
                faithLenience = Math.Max(0, faithLenience - ETime.FRAME_TIME);
            } else if (Faith > 0) {
                Faith = Math.Max(0, Faith - ETime.FRAME_TIME * 
                    faithDecayRate * FaithDecayRateMultiplier * Difficulty.faithDecayMultiplier);
            } else if (PIV > 1) {
                PIV = Math.Max(1, PIV - pivFallStep);
                Faith = 0.5f;
                faithLenience = faithLenienceFall;
                CampaignDataUpdated.Proc();
            }
        }
    }


    public void SetCurrentBoss(BossConfig cfg, BehaviorEntity boss, ICancellee bossCT) {
        if (CurrentBossCT != null) CloseBoss();
        BossesEncountered.Add(cfg);
        CurrentBoss = boss;
        CurrentBossCT = bossCT;
    }

    private void CloseBoss() {
        if (CurrentBossCT != null) {
            CurrentBoss = null;
            CurrentBossCT = null;
        } else Log.UnityError("You tried to close a boss section when no boss exists.");
    }

    public static readonly Events.Event0 UselessPowerupCollected = new Events.Event0();
    public static readonly Events.Event0 CampaignDataUpdated = new Events.Event0();
    public static readonly Events.Event0 PlayerTookHit = new Events.Event0();
    public static readonly Events.Event0 CardHistoryUpdated = new Events.Event0();
    public static readonly Events.Event0 MeterNowUsable = new Events.Event0();
    public static readonly Events.Event0 PowerLost = new Events.Event0();
    public static readonly Events.Event0 PowerGained = new Events.Event0();
    public static readonly Events.Event0 PowerFull = new Events.Event0();
    public static readonly Events.Event0 AnyExtendAcquired = new Events.Event0();
    public static readonly Events.Event0 ItemExtendAcquired = new Events.Event0();
    public static readonly Events.Event0 ScoreExtendAcquired = new Events.Event0();
    public static readonly Events.IEvent<PhaseCompletion> PhaseCompleted = new Events.Event<PhaseCompletion>();
    public static readonly Events.Event0 LifeSwappedForScore = new Events.Event0();

#if UNITY_EDITOR
    public void SetPower(double x) => Power = x;
    #endif
}


}