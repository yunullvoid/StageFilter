using RiskOfOptions.Components.Options;
using RoR2.UI;

namespace StageFilter.Lobby.UI;

internal static class Popup
{
    public static void OpenPopUp()
    {
        SimpleDialogBox dialog = SimpleDialogBox.Create();

        dialog.gameObject.AddComponent<RooEscapeRouter>().escapePressed.AddListener(() =>
        {
            if (dialog && dialog.rootObject)
            {
                UnityEngine.Object.Destroy(dialog.rootObject);
            }
        });

        dialog.headerToken = new SimpleDialogBox.TokenParamsPair("POPUP_CANNOT_BLOCK_STAGE_HEADER");
        dialog.descriptionToken = new SimpleDialogBox.TokenParamsPair("POPUP_CANNOT_BLOCK_STAGE_DESCRIPTION");
        dialog.AddCancelButton("POPUP_CANNOT_BLOCK_STAGE_BUTTON");
    }
}
