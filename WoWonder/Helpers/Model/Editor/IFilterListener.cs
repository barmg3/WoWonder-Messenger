using WoWonder.NiceArt.Models;

namespace WoWonder.Helpers.Model.Editor
{
    public interface IFilterListener
    {
        void OnFilterSelected(PhotoFilter photoFilter);
    }
}