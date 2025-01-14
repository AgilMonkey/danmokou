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
	public static partial class TestContent1 {
		
		public static string pickup_gold(object arg0, object arg1) => Localization.Locale.Value switch {
			Locales.JP => Render(Localization.Locale.Value, new[] {
				"{0}",
				"が金貨を",
				JP_COUNTER(arg1, "枚"),
				"拾いました",
			}, arg0, arg1),
			_ => Render(Localization.Locale.Value, new[] {
				"{0}",
				" picked up ",
				"{1}",
				" gold ",
				PLURAL(arg1, "coin", "coins"),
			}, arg0, arg1),
		};
		
		public static LString pickup_gold_ls(object arg0, object arg1) => new LString(Render(null, new[] {
				"{0}",
				" picked up ",
				"{1}",
				" gold ",
				PLURAL(arg1, "coin", "coins"),
			}, arg0, arg1),
			(Locales.JP, Render(Locales.JP, new[] {
				"{0}",
				"が金貨を",
				JP_COUNTER(arg1, "枚"),
				"拾いました",
			}, arg0, arg1)))
			{ ID = "pickup_gold" };
		
		public static string escape_example(object arg0, object arg1) => Localization.Locale.Value switch {
			Locales.JP => Render(Localization.Locale.Value, new[] {
				"{0}",
				"が金貨を",
				"{{",
				"$JP_COUNTER(1, 枚)",
				"}}",
				"拾いました",
			}, arg0, arg1),
			_ => Render(Localization.Locale.Value, new[] {
				"{0}",
				" picked up ",
				"{{",
				"1",
				"}}",
				" gold ",
				PLURAL(arg1, "coin", "coins"),
			}, arg0, arg1),
		};
		
		public static LString escape_example_ls(object arg0, object arg1) => new LString(Render(null, new[] {
				"{0}",
				" picked up ",
				"{{",
				"1",
				"}}",
				" gold ",
				PLURAL(arg1, "coin", "coins"),
			}, arg0, arg1),
			(Locales.JP, Render(Locales.JP, new[] {
				"{0}",
				"が金貨を",
				"{{",
				"$JP_COUNTER(1, 枚)",
				"}}",
				"拾いました",
			}, arg0, arg1)))
			{ ID = "escape_example" };
		
	}
}
}
