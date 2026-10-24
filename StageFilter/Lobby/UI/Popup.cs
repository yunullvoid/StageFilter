using RoR2.UI;
using UnityEngine;
using System;

namespace StageFilter.Lobby.UI;

internal static class Popup
{
    public static void OpenPopUp()
    {
        SimpleDialogBox dialog = SimpleDialogBox.Create();

        var watcher = dialog.rootObject.AddComponent<PopUpCloseWatcher>();
        watcher.dialog = dialog;

        dialog.headerToken = new SimpleDialogBox.TokenParamsPair("POPUP_CANNOT_BLOCK_STAGE_HEADER");
        dialog.descriptionToken = new SimpleDialogBox.TokenParamsPair("POPUP_CANNOT_BLOCK_STAGE_DESCRIPTION");
        dialog.AddCancelButton("POPUP_CANNOT_BLOCK_STAGE_BUTTON");
    }

    private class PopUpCloseWatcher : MonoBehaviour
    {
        public SimpleDialogBox dialog;
        public Action onCancel;
        private float delay = 0.4f;
        private bool closing;

        private void Update()
        {
            if (closing) return;

            delay -= Time.unscaledDeltaTime;
            if (delay > 0f) return;

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                closing = true;
                var target = dialog.rootObject ?? dialog.gameObject;
                onCancel?.Invoke();
                UnityEngine.Object.Destroy(target);
            }
        }
    }
}
