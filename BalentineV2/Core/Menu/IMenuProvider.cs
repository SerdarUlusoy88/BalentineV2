using System.Collections.Generic;

namespace BalentineV2.Core.Menu;

public interface IMenuProvider
{
    IReadOnlyList<MenuItemModel> GetMenu();

    // Geriye dönük uyumluluk: bazı yerlerde BuildMenu çağrılmış olabilir
    IReadOnlyList<MenuItemModel> BuildMenu();
}
