// File: UI/Navigation/IActivatablePage.cs
using System.Threading.Tasks;

namespace BalentineV2.UI.Navigation;

public interface IActivatablePage
{
    Task OnActivatedAsync();   // ekrana geldi (ContentHost’a basıldı)
    void OnDeactivated();      // ekrandan gitti
}
