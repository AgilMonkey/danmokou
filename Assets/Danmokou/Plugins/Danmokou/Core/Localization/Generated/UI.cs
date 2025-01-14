//----------------------
// <auto-generated>
//     Generated by Bagoum's Localization Utilities CSV Analysis.
//     Github project: https://github.com/Bagoum/localization-utils
// </auto-generated>
//----------------------

using System.Collections.Generic;
using BagoumLib.Culture;
using Danmokou.Core;
using static BagoumLib.Culture.LocalizationRendering;
using static Danmokou.Core.LocalizationRendering;

namespace Danmokou.Core {
public static partial class LocalizedStrings {
	public static partial class UI {
		
		public static readonly LString shaders = new LString("Visual Quality",
			(Locales.JP, "画質"))
			{ ID = "shaders" };
		
		public static readonly LString shaders_low = new LString("Low",
			(Locales.JP, "低画質"))
			{ ID = "shaders_low" };
		
		public static readonly LString shaders_high = new LString("High",
			(Locales.JP, "高画質"))
			{ ID = "shaders_high" };
		
		public static readonly LString resolution = new LString("Resolution",
			(Locales.JP, "解像度"))
			{ ID = "resolution" };
		
		public static readonly LString refresh = new LString("Refresh Rate",
			(Locales.JP, "リフレッシュ速度"))
			{ ID = "refresh" };
		
		public static readonly LString fullscreen = new LString("Fullscreen",
			(Locales.JP, "表示モード"))
			{ ID = "fullscreen" };
		
		public static readonly LString fullscreen_window = new LString("Windowed",
			(Locales.JP, "ウインドウ"))
			{ ID = "fullscreen_window" };
		
		public static readonly LString fullscreen_borderless = new LString("Borderless",
			(Locales.JP, "ボーダレス"))
			{ ID = "fullscreen_borderless" };
		
		public static readonly LString fullscreen_exclusive = new LString("Exclusive",
			(Locales.JP, "フルスクリーン"))
			{ ID = "fullscreen_exclusive" };
		
		public static readonly LString vsync = new LString("VSync",
			(Locales.JP, "垂直同期"))
			{ ID = "vsync" };
		
		public static readonly LString vsync_double = new LString("Double",
			(Locales.JP, "DOUBLE"))
			{ ID = "vsync_double" };
		
		public static readonly LString renderer = new LString("Renderer",
			(Locales.JP, "レンダラー"))
			{ ID = "renderer" };
		
		public static readonly LString renderer_legacy = new LString("Legacy",
			(Locales.JP, "旧型"))
			{ ID = "renderer_legacy" };
		
		public static readonly LString renderer_normal = new LString("Normal",
			(Locales.JP, "新型"))
			{ ID = "renderer_normal" };
		
		public static readonly LString smoothing = new LString("Input Smoothing",
			(Locales.JP, "インプットスムージング"))
			{ ID = "smoothing" };
		
		public static readonly LString controller = new LString("Controller",
			(Locales.JP, "コントローラー"))
			{ ID = "controller" };
		
		public static readonly LString controller_off = new LString("Ignore",
			(Locales.JP, "使わない"))
			{ ID = "controller_off" };
		
		public static readonly LString controller_on = new LString("Enabled",
			(Locales.JP, "使う"))
			{ ID = "controller_on" };
		
		public static readonly LString dialogue_speed = new LString("Dialogue Speed",
			(Locales.JP, "テキスト速度"))
			{ ID = "dialogue_speed" };
		
		public static readonly LString bgm_volume = new LString("BGM Volume",
			(Locales.JP, "ミュージック音量"))
			{ ID = "bgm_volume" };
		
		public static readonly LString sfx_volume = new LString("SFX Volume",
			(Locales.JP, "効果音音量"))
			{ ID = "sfx_volume" };
		
		public static readonly LString hitbox = new LString("Hitbox Display",
			(Locales.JP, "当たり判定表示"))
			{ ID = "hitbox" };
		
		public static readonly LString hitbox_focus = new LString("When Focused",
			(Locales.JP, "低速移動時"))
			{ ID = "hitbox_focus" };
		
		public static readonly LString hitbox_always = new LString("Always Show",
			(Locales.JP, "常に"))
			{ ID = "hitbox_always" };
		
		public static readonly LString backgrounds = new LString("Backgrounds",
			(Locales.JP, "背景"))
			{ ID = "backgrounds" };
		
		public static readonly LString screenshake = new LString("Screenshake",
			(Locales.JP, "画像振動"))
			{ ID = "screenshake" };
		
		public static readonly LString unpause = new LString("Unpause",
			(Locales.JP, "再開"))
			{ ID = "unpause" };
		
		public static readonly LString to_menu = new LString("Return to Menu",
			(Locales.JP, "タイトルに戻る"))
			{ ID = "to_menu" };
		
		public static readonly LString restart = new LString("Restart",
			(Locales.JP, "やり直す"))
			{ ID = "restart" };
		
		public static readonly LString save_replay = new LString("Save Replay",
			(Locales.JP, "リプレイを保存する"))
			{ ID = "save_replay" };
		
		public static readonly LString are_you_sure = new LString("Are you sure?",
			(Locales.JP, "本当に？"))
			{ ID = "are_you_sure" };
		
		public static readonly LString to_desktop = new LString("Quit to Desktop",
			(Locales.JP, "終了"))
			{ ID = "to_desktop" };
		
		public static readonly LString pause_header = new LString("Pause",
			(Locales.JP, "一時停止"))
			{ ID = "pause_header" };
		
		public static readonly LString replay_view = new LString("View",
			(Locales.JP, "再演"))
			{ ID = "replay_view" };
		
		public static readonly LString delete = new LString("Delete",
			(Locales.JP, "削除"))
			{ ID = "delete" };
		
		public static string practice_stage(object arg0) => Localization.Locale.Value switch {
			Locales.JP => Render(Localization.Locale.Value, new[] {
				"{0}",
				"面",
			}, arg0),
			_ => Render(Localization.Locale.Value, new[] {
				"Stage ",
				"{0}",
			}, arg0),
		};
		
		public static LString practice_stage_ls(object arg0) => new LString(Render(null, new[] {
				"Stage ",
				"{0}",
			}, arg0),
			(Locales.JP, Render(Locales.JP, new[] {
				"{0}",
				"面",
			}, arg0)))
			{ ID = "practice_stage" };
		
		public static readonly LString practice_fullstage = new LString("Full Stage",
			(Locales.JP, "全面"))
			{ ID = "practice_fullstage" };
		
		public static string practice_stage_section(object arg0) => Localization.Locale.Value switch {
			Locales.JP => Render(Localization.Locale.Value, new[] {
				"第",
				"{0}",
				"幕",
			}, arg0),
			_ => Render(Localization.Locale.Value, new[] {
				"Stage Section ",
				"{0}",
			}, arg0),
		};
		
		public static LString practice_stage_section_ls(object arg0) => new LString(Render(null, new[] {
				"Stage Section ",
				"{0}",
			}, arg0),
			(Locales.JP, Render(Locales.JP, new[] {
				"第",
				"{0}",
				"幕",
			}, arg0)))
			{ ID = "practice_stage_section" };
		
		public static readonly LString practice_midboss = new LString("Midboss",
			(Locales.JP, "中ボス"))
			{ ID = "practice_midboss" };
		
		public static readonly LString practice_endboss = new LString("Endboss",
			(Locales.JP, "ラスボス"))
			{ ID = "practice_endboss" };
		
		public static readonly LString practice_dialogue = new LString("Dialogue",
			(Locales.JP, "会話"))
			{ ID = "practice_dialogue" };
		
		public static string challenge_day_intro(object arg0) => Localization.Locale.Value switch {
			Locales.JP => Render(Localization.Locale.Value, new[] {
				"{0}",
				"紹介",
			}, arg0),
			_ => Render(Localization.Locale.Value, new[] {
				"{0}",
				" Intro",
			}, arg0),
		};
		
		public static LString challenge_day_intro_ls(object arg0) => new LString(Render(null, new[] {
				"{0}",
				" Intro",
			}, arg0),
			(Locales.JP, Render(Locales.JP, new[] {
				"{0}",
				"紹介",
			}, arg0)))
			{ ID = "challenge_day_intro" };
		
		public static string challenge_day_end(object arg0) => Localization.Locale.Value switch {
			Locales.JP => Render(Localization.Locale.Value, new[] {
				"{0}",
				"結末",
			}, arg0),
			_ => Render(Localization.Locale.Value, new[] {
				"{0}",
				" End",
			}, arg0),
		};
		
		public static LString challenge_day_end_ls(object arg0) => new LString(Render(null, new[] {
				"{0}",
				" End",
			}, arg0),
			(Locales.JP, Render(Locales.JP, new[] {
				"{0}",
				"結末",
			}, arg0)))
			{ ID = "challenge_day_end" };
		
		public static string challenge_day_card(object arg0, object arg1) => Localization.Locale.Value switch {
			Locales.JP => Render(Localization.Locale.Value, new[] {
				"{0}",
				"-",
				"{1}",
			}, arg0, arg1),
			_ => Render(Localization.Locale.Value, new[] {
				"{0}",
				" ",
				"{1}",
			}, arg0, arg1),
		};
		
		public static LString challenge_day_card_ls(object arg0, object arg1) => new LString(Render(null, new[] {
				"{0}",
				" ",
				"{1}",
			}, arg0, arg1),
			(Locales.JP, Render(Locales.JP, new[] {
				"{0}",
				"-",
				"{1}",
			}, arg0, arg1)))
			{ ID = "challenge_day_card" };
		
		public static readonly LString practice_m_boss = new LString("Boss Practice")
			{ ID = "practice_m_boss" };
		
		public static readonly LString practice_m_stage = new LString("Stage Practice")
			{ ID = "practice_m_stage" };
		
		public static readonly LString practice_m_campaign = new LString("Campaign")
			{ ID = "practice_m_campaign" };
		
		public static readonly LString practice_m_scene = new LString("Scene Challenge")
			{ ID = "practice_m_scene" };
		
		public static readonly LString practice_m_whichstage = new LString("Stage",
			(Locales.JP, "面"))
			{ ID = "practice_m_whichstage" };
		
		public static readonly LString practice_m_whichphase = new LString("Phase",
			(Locales.JP, "フェーズ"))
			{ ID = "practice_m_whichphase" };
		
		public static readonly LString practice_m_whichboss = new LString("Boss Practice",
			(Locales.JP, "ボス"))
			{ ID = "practice_m_whichboss" };
		
		public static readonly LString practice_type = new LString("Game Mode",
			(Locales.JP, "モード"))
			{ ID = "practice_type" };
		
		public static readonly LString practice_campaign = new LString("Campaign",
			(Locales.JP, "シナリオ"))
			{ ID = "practice_campaign" };
		
		public static readonly LString shotsel_player = new LString("PLAYER",
			(Locales.JP, "機体"))
			{ ID = "shotsel_player" };
		
		public static readonly LString shotsel_shot = new LString("SHOT",
			(Locales.JP, "装備"))
			{ ID = "shotsel_shot" };
		
		public static readonly LString shotsel_support = new LString("ABILITY",
			(Locales.JP, "能力"))
			{ ID = "shotsel_support" };
		
		public static string shotsel_type(object arg0) => Localization.Locale.Value switch {
			Locales.JP => Render(Localization.Locale.Value, new[] {
				"タイプ",
				"{0}",
			}, arg0),
			_ => Render(Localization.Locale.Value, new[] {
				"Type ",
				"{0}",
			}, arg0),
		};
		
		public static LString shotsel_type_ls(object arg0) => new LString(Render(null, new[] {
				"Type ",
				"{0}",
			}, arg0),
			(Locales.JP, Render(Locales.JP, new[] {
				"タイプ",
				"{0}",
			}, arg0)))
			{ ID = "shotsel_type" };
		
		public static string shotsel_variant(object arg0) => Localization.Locale.Value switch {
			Locales.JP => Render(Localization.Locale.Value, new[] {
				"バリアント",
				"{0}",
			}, arg0),
			_ => Render(Localization.Locale.Value, new[] {
				"Variant ",
				"{0}",
			}, arg0),
		};
		
		public static LString shotsel_variant_ls(object arg0) => new LString(Render(null, new[] {
				"Variant ",
				"{0}",
			}, arg0),
			(Locales.JP, Render(Locales.JP, new[] {
				"バリアント",
				"{0}",
			}, arg0)))
			{ ID = "shotsel_variant" };
		
		public static string shotsel_multi(object arg0) => Localization.Locale.Value switch {
			Locales.JP => Render(Localization.Locale.Value, new[] {
				"可変ショット",
				"{0}",
			}, arg0),
			_ => Render(Localization.Locale.Value, new[] {
				"Multishot ",
				"{0}",
			}, arg0),
		};
		
		public static LString shotsel_multi_ls(object arg0) => new LString(Render(null, new[] {
				"Multishot ",
				"{0}",
			}, arg0),
			(Locales.JP, Render(Locales.JP, new[] {
				"可変ショット",
				"{0}",
			}, arg0)))
			{ ID = "shotsel_multi" };
		
		public static readonly LString shotsel_prefix = new LString("Shot:",
			(Locales.JP, "ショット"))
			{ ID = "shotsel_prefix" };
		
		public static readonly LString shotsel_multi_prefix = new LString("Multishot:",
			(Locales.JP, "可変ショット"))
			{ ID = "shotsel_multi_prefix" };
		
		public static readonly LString shotsel_support_prefix = new LString("Support:",
			(Locales.JP, "スキル"))
			{ ID = "shotsel_support_prefix" };
		
		public static string death_continue(object arg0) => Localization.Locale.Value switch {
			Locales.JP => Render(Localization.Locale.Value, new[] {
				"続ける（後",
				"{0}",
				"回）",
			}, arg0),
			_ => Render(Localization.Locale.Value, new[] {
				"Continue [",
				"{0}",
				"]",
			}, arg0),
		};
		
		public static LString death_continue_ls(object arg0) => new LString(Render(null, new[] {
				"Continue [",
				"{0}",
				"]",
			}, arg0),
			(Locales.JP, Render(Locales.JP, new[] {
				"続ける（後",
				"{0}",
				"回）",
			}, arg0)))
			{ ID = "death_continue" };
		
		public static readonly LString death_header = new LString("YOU DIED",
			(Locales.JP, "落命"))
			{ ID = "death_header" };
		
		public static readonly LString difficulty_easy = new LString("Easy")
			{ ID = "difficulty_easy" };
		
		public static readonly LString difficulty_normal = new LString("Normal")
			{ ID = "difficulty_normal" };
		
		public static readonly LString difficulty_hard = new LString("Hard")
			{ ID = "difficulty_hard" };
		
		public static readonly LString difficulty_lunatic = new LString("Lunatic")
			{ ID = "difficulty_lunatic" };
		
		public static readonly LString difficulty_custom = new LString("Custom")
			{ ID = "difficulty_custom" };
		
		public static readonly LString play_game = new LString("Start!")
			{ ID = "play_game" };
		
		public static readonly LString main_gamestart = new LString("Play!")
			{ ID = "main_gamestart" };
		
		public static readonly LString main_main = new LString("Main Scenario")
			{ ID = "main_main" };
		
		public static readonly LString main_extra = new LString("Extra Stage")
			{ ID = "main_extra" };
		
		public static readonly LString main_lang = new LString("Language")
			{ ID = "main_lang" };
		
		public static readonly LString main_stageprac = new LString("Stage Practice")
			{ ID = "main_stageprac" };
		
		public static readonly LString main_bossprac = new LString("Boss Practice")
			{ ID = "main_bossprac" };
		
		public static readonly LString main_scores = new LString("Scores")
			{ ID = "main_scores" };
		
		public static readonly LString main_stats = new LString("Stats")
			{ ID = "main_stats" };
		
		public static readonly LString main_achievements = new LString("Achievements")
			{ ID = "main_achievements" };
		
		public static readonly LString main_musicroom = new LString("Music Room")
			{ ID = "main_musicroom" };
		
		public static readonly LString main_replays = new LString("Replays")
			{ ID = "main_replays" };
		
		public static readonly LString main_tutorial = new LString("Tutorial")
			{ ID = "main_tutorial" };
		
		public static readonly LString main_options = new LString("Options")
			{ ID = "main_options" };
		
		public static readonly LString main_quit = new LString("Quit")
			{ ID = "main_quit" };
		
		public static readonly LString main_twitter = new LString("Twitter (Browser)")
			{ ID = "main_twitter" };
		
		public static readonly LString replay_name = new LString("Name",
			(Locales.JP, "リプレイ名"))
			{ ID = "replay_name" };
		
		public static readonly LString replay_save = new LString("Save",
			(Locales.JP, "セーブ"))
			{ ID = "replay_save" };
		
		public static readonly LString replay_saved = new LString("Saved!",
			(Locales.JP, "セーブ完了！"))
			{ ID = "replay_saved" };
		
		public static readonly LString scores_nocampaign = new LString("Finish a campaign to view scores.")
			{ ID = "scores_nocampaign" };
		
		public static readonly LString stats_allcampaigns = new LString("All Campaigns",
			(Locales.JP, "すべて"))
			{ ID = "stats_allcampaigns" };
		
		public static readonly LString stats_seldifficulty = new LString("Difficulty",
			(Locales.JP, "難易度"))
			{ ID = "stats_seldifficulty" };
		
		public static readonly LString stats_alldifficulty = new LString("All Difficulties",
			(Locales.JP, "すべて"))
			{ ID = "stats_alldifficulty" };
		
		public static readonly LString stats_selplayer = new LString("Player",
			(Locales.JP, "機体"))
			{ ID = "stats_selplayer" };
		
		public static readonly LString stats_allplayers = new LString("All Players",
			(Locales.JP, "すべて"))
			{ ID = "stats_allplayers" };
		
		public static readonly LString stats_selshot = new LString("Shot",
			(Locales.JP, "装備"))
			{ ID = "stats_selshot" };
		
		public static readonly LString stats_allshots = new LString("All Shots",
			(Locales.JP, "すべて"))
			{ ID = "stats_allshots" };
		
		public static readonly LString stats_allruns = new LString("Runs Total",
			(Locales.JP, "プレイ回"))
			{ ID = "stats_allruns" };
		
		public static readonly LString stats_complete = new LString("Runs Completed",
			(Locales.JP, "クリア回"))
			{ ID = "stats_complete" };
		
		public static readonly LString stats_1cc = new LString("Runs 1CCed",
			(Locales.JP, "1CCクリア回"))
			{ ID = "stats_1cc" };
		
		public static readonly LString stats_deaths = new LString("Total Deaths",
			(Locales.JP, "被弾数"))
			{ ID = "stats_deaths" };
		
		public static readonly LString stats_favday = new LString("Favorite Day",
			(Locales.JP, "プレイしがちの曜日"))
			{ ID = "stats_favday" };
		
		public static readonly LString stats_favplayer = new LString("Favorite Player",
			(Locales.JP, "使いがちの機体"))
			{ ID = "stats_favplayer" };
		
		public static readonly LString stats_favshot = new LString("Favorite Shot",
			(Locales.JP, "使いがちの装備"))
			{ ID = "stats_favshot" };
		
		public static readonly LString stats_highestscore = new LString("Highest Score",
			(Locales.JP, "ハイスコア"))
			{ ID = "stats_highestscore" };
		
		public static readonly LString stats_capturerate = new LString("Card Capture Rate",
			(Locales.JP, "スペルカードキャプチャ率"))
			{ ID = "stats_capturerate" };
		
		public static readonly LString stats_bestcard = new LString("Easiest Card Capture",
			(Locales.JP, "上手なスペルカード"))
			{ ID = "stats_bestcard" };
		
		public static readonly LString stats_worstcard = new LString("Hardest Card Capture",
			(Locales.JP, "下手なスペルカード"))
			{ ID = "stats_worstcard" };
		
		public static readonly LString stats_avgtime = new LString("Average Game Length",
			(Locales.JP, "平均ゲーム時間"))
			{ ID = "stats_avgtime" };
		
		public static readonly LString stats_totaltime = new LString("Total Play Time",
			(Locales.JP, "総計プレイ時間"))
			{ ID = "stats_totaltime" };
		
		public static readonly LString achievements_locked = new LString("???")
			{ ID = "achievements_locked" };
		
	}
}
}
