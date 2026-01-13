using System.Collections.Generic;

namespace BalentineV2.Core.StatusBar;

public interface IStatusBarProvider
{
    event EventHandler? Changed;
    IReadOnlyList<StatusBarChipModel> GetChips();
}
