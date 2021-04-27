﻿using System;
using System.Linq;
using DMK.Behavior;
using DMK.Core;
using DMK.UI.XML;
using static DMK.UI.XML.XMLUtils;

namespace DMK.UI {
public abstract class SubmenuHandler : CoroutineRegularUpdater {
    public abstract UIScreen Initialize(XMLMainMenu menu);
}

public abstract class IndexedSubmenuHandler : SubmenuHandler {
    protected abstract int NumOptions { get; }
    protected virtual int DefaultOption => 0;
    protected XMLMainMenu Menu { get; private set; } = null!;
    
    public override UIScreen Initialize(XMLMainMenu menu) {
        Menu = menu;
        HideOnExit();
        var opt = new OptionNodeLR<int>(LocalizedString.Empty, SetIndex, NumOptions.Range().ToArray(), DefaultOption);
        return new UIScreen(opt
                .With(hideClass)
                .SetUpOverride(() => opt.Left())
                .SetDownOverride(() => opt.Right())
                .SetConfirmOverride(() => Activate(opt.Value))
            )
            .OnPreEnter(() => OnPreEnter(opt.Value))
            .OnPreExit(OnPreExit)
            .OnEnter(() => Show(opt.Value, true))
            .OnExit(HideOnExit);
    }

    protected virtual void SetIndex(int index) => Show(index, false);
    
    protected virtual void OnPreExit() { }
    protected virtual void OnPreEnter(int index) { }
    protected abstract void HideOnExit();

    protected abstract void Show(int index, bool isOnEnter);

    protected abstract (bool success, UINode? nxt) Activate(int index);
}
}