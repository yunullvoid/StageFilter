using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RoR2;
using RoR2.UI;
using StageFilter.Stage;
using StageFilter.Stage.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StageFilter.Lobby.UI;

// Adapted from CENI by Joaosnake
// https://github.com/Jaosnake/CENI/blob/main/ProperSave/LobbyUI.cs
internal static class ListUI
{
    private const float DialogWidth = 640f;
    private const float ButtonHeight = 86f;

    private const float PageButtonHeight = 40f;
    private const float PageButtonWidth = 100f;

    private const float CancelButtonHeight = 50f;
    private const float CancelButtonWidth = 220f;

    static public int actualPage = 0;

    public static void ShowBanListDialog(List<Page<StageInfo>> banList)
    {
        if (banList is null || banList.Count == 0) return;

        List<Page<StageInfo>> allPages = banList
            .OrderBy(page => page.Number)
            .ThenBy(page => page.Items.Count)
            .ToList();

        actualPage = Mathf.Clamp(actualPage, 0, allPages.Count - 1);

        var dialog = SimpleDialogBox.Create(EventSystem.current as MPEventSystem);
        if (dialog is null)
        {
            StageFilter.Logger.LogWarning("Failed to create ban list dialog!");
            return;
        }

        dialog.headerToken = new SimpleDialogBox.TokenParamsPair { token = "BANLIST_DIALOG_HEADER" };
        dialog.descriptionToken = new SimpleDialogBox.TokenParamsPair { token = Language.GetStringFormatted("BANLIST_DIALOG_DESCRIPTION", [(int)allPages[actualPage].Items[0].StageSet]) };

        foreach (StageInfo item in allPages[actualPage].Items)
        {
            dialog.AddActionButton(() => { }, "PLACEHOLDER", destroyDialog: false);

            SetLastButtonContent(
                dialog,
                Language.GetStringFormatted(item.NameToken),
                StageDatabase.EnabledSpriteList[item.ID]
            );
        }

        bool canGoBack = actualPage > 0;
        bool canGoNext = actualPage < allPages.Count - 1;

        MPButton previousPageButton = dialog.AddActionButton(() => NavigatePage(dialog, allPages, -1, canGoBack), "<", destroyDialog: false);
        previousPageButton.interactable = canGoBack;
        previousPageButton.enabled = canGoBack;

        dialog.AddCancelButton("SUBMENU_BACK");

        MPButton nextPageButton = dialog.AddActionButton(() => NavigatePage(dialog, allPages, 1, canGoNext), ">", destroyDialog: false);
        nextPageButton.interactable = canGoNext;
        nextPageButton.enabled = canGoNext;

        AddBottomNavigationRow(dialog);
        AddCloseWatcher(dialog, onCancel: null, onPrev: () => NavigatePage(dialog, allPages, -1, canGoBack), onNext: () => NavigatePage(dialog, allPages, 1, canGoNext));

        StageFilter.Instance.StartCoroutine(DelayedDialogLayout(dialog));
    }

    private static void NavigatePage(SimpleDialogBox dialog, List<Page<StageInfo>> allPages, int delta, bool isActive)
    {
        if (allPages is null || allPages.Count == 0 || !isActive) return;

        actualPage += delta;

        var root = GetDialogRoot(dialog);
        if (root is not null)
        {
            UnityEngine.Object.Destroy(root);
        }

        ShowBanListDialog(BanList.pages);
    }

    private static void AddBottomNavigationRow(SimpleDialogBox dialog)
    {
        var container = dialog?.buttonContainer;
        if (container is null) return;

        var buttons = container.GetComponentsInChildren<MPButton>(includeInactive: true).ToList();
        if (buttons.Count < 3) return;

        var navButtons = buttons.Skip(Math.Max(0, buttons.Count - 3)).Take(3).ToList();

        var rowObject = new GameObject("Bottom Nav Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        var row = rowObject.GetComponent<RectTransform>();
        row.SetParent(container, false);

        var rowLayout = rowObject.GetComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 18f;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = false;
        rowLayout.childControlHeight = false;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;

        SetLayoutElement(rowObject, DialogWidth, 56f);
        SetCenteredRect(row, DialogWidth, 56f);

        foreach (var button in navButtons)
        {
            button.transform.SetParent(row, false);
        }

        SetLayoutElement(navButtons[0].gameObject, PageButtonWidth, PageButtonHeight);
        SetLayoutElement(navButtons[1].gameObject, CancelButtonWidth, CancelButtonHeight);
        SetLayoutElement(navButtons[2].gameObject, PageButtonWidth, PageButtonHeight);

        var prevRect = navButtons[0].GetComponent<RectTransform>();
        var cancelRect = navButtons[1].GetComponent<RectTransform>();
        var nextRect = navButtons[2].GetComponent<RectTransform>();

        prevRect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, PageButtonWidth);
        prevRect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, PageButtonHeight);
        cancelRect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CancelButtonWidth);
        cancelRect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CancelButtonHeight);
        nextRect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, PageButtonWidth);
        nextRect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, PageButtonHeight);
        // Add LB/RB glyphs to the page navigation buttons.
        try
        {
            var previousPageGlyph = CreateGlyph(navButtons[0].transform, "UITabLeft");
            var nextPageGlyph = CreateGlyph(navButtons[2].transform, "UITabRight");

            var root = GetDialogRoot(dialog);
            var glyphController = root.GetComponent<PageGlyphVisibilityController>()
                ?? root.AddComponent<PageGlyphVisibilityController>();
            glyphController.eventSystem = EventSystem.current as MPEventSystem;
            glyphController.prevPageGlyph = previousPageGlyph;
            glyphController.nextPageGlyph = nextPageGlyph;
        }
        catch (Exception ex)
        {
            StageFilter.Logger.LogWarning("Failed to add page button glyphs.");
            StageFilter.Logger.LogError(ex);
        }
    }

    private static InputBindingDisplayController CreateGlyph(Transform parent, string actionName)
    {
        var glyphObject = new GameObject($"{actionName} Glyph", typeof(RectTransform));
        var glyphRect = glyphObject.GetComponent<RectTransform>();
        glyphRect.SetParent(parent, false);
        glyphRect.anchorMin = new Vector2(0f, 0.5f);
        glyphRect.anchorMax = new Vector2(0f, 0.5f);
        glyphRect.pivot = new Vector2(0f, 0.5f);
        glyphRect.anchoredPosition = new Vector2((actionName == "UITabRight" ? 80f : -12f), 0f);
        glyphRect.sizeDelta = new Vector2(40f, 40f);

        var glyphText = glyphObject.AddComponent<HGTextMeshProUGUI>();
        glyphText.fontSize = 18f;
        glyphText.alignment = TMPro.TextAlignmentOptions.Center;

        var glyph = glyphObject.AddComponent<InputBindingDisplayController>();
        glyph.actionName = actionName;
        glyph.useExplicitInputSource = true;
        glyph.explicitInputSource = MPEventSystem.InputSource.Gamepad;
        glyphObject.SetActive(true);

        return glyph;
    }

    private static void SetLastButtonContent(SimpleDialogBox dialog, string text, Sprite mapPreview)
    {
        try
        {
            var container = dialog.buttonContainer;
            if (container is null || container.childCount == 0)
            {
                return;
            }

            var button = container.GetChild(container.childCount - 1);
            if (mapPreview is not null)
            {
                AddMapPreviewToButton(button, text, mapPreview);
            }

            foreach (var languageController in button.GetComponentsInChildren<LanguageTextMeshController>())
            {
                languageController.enabled = false;
            }

            var label = button.GetComponentInChildren<HGTextMeshProUGUI>();
            if (label is not null)
            {
                label.text = text;
                label.enableWordWrapping = true;
                label.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
                label.fontSize = 18f;

                var rect = label.GetComponent<RectTransform>();
                if (rect is not null && mapPreview is not null)
                {
                    rect.offsetMin = new Vector2(rect.offsetMin.x + 78f, rect.offsetMin.y);
                }
            }
        }
        catch (Exception ex)
        {
            StageFilter.Logger.LogWarning("Failed to set button content.");
            StageFilter.Logger.LogError(ex);
        }
    }

    private static void AddMapPreviewToButton(Transform button, string mapToken,Sprite mapPreview)
    {
        var mappreviewObject = new GameObject($"{mapToken} Map Preview", typeof(RectTransform), typeof(RawImage));
        var rect = mappreviewObject.GetComponent<RectTransform>();
        rect.SetParent(button, false);
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(12f, 0f);
        rect.sizeDelta = new Vector2(64f, 64f);

        var image = mappreviewObject.GetComponent<RawImage>();
        image.texture = mapPreview.texture;
        image.color = Color.white;
        image.raycastTarget = false;
    }

    private static IEnumerator DelayedDialogLayout(SimpleDialogBox dialog)
    {
        yield return null;

        ApplyDialogLayout(dialog);
    }

    private static void ApplyDialogLayout(SimpleDialogBox dialog)
    {
        try
        {
            var container = dialog.buttonContainer;
            if (container is null)
            {
                return;
            }

            foreach (var layout in container.GetComponents<LayoutGroup>())
            {
                layout.enabled = false;
                UnityEngine.Object.DestroyImmediate(layout, true);
            }

            var vertical = container.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.spacing = 8f;
            vertical.childAlignment = TextAnchor.UpperCenter;
            vertical.childControlWidth = false;
            vertical.childControlHeight = false;
            vertical.childForceExpandWidth = false;
            vertical.childForceExpandHeight = false;

            for (var i = 0; i < container.childCount; i++)
            {
                var child = container.GetChild(i);
                var rect = child.GetComponent<RectTransform>();
                rect?.sizeDelta = new Vector2(DialogWidth, ButtonHeight);

                SetLayoutElement(child.gameObject, DialogWidth, ButtonHeight);
            }

            var fitter = container.GetComponent<ContentSizeFitter>() ?? container.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            WrapDialogNavigation(container);
            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
        }
        catch (Exception ex)
        {
            StageFilter.Logger.LogWarning("Failed to apply dialog layout.");
            StageFilter.Logger.LogError(ex);
        }
    }

    private static void WrapDialogNavigation(RectTransform container)
    {
        var buttons = container.GetComponentsInChildren<MPButton>().ToList();
        if (buttons.Count == 0) return;

        for (var i = 0; i < buttons.Count; i++)
        {
            var navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = buttons[(i - 1 + buttons.Count) % buttons.Count],
                selectOnDown = buttons[(i + 1) % buttons.Count]
            };
            navigation.selectOnLeft = navigation.selectOnUp;
            navigation.selectOnRight = navigation.selectOnDown;
            buttons[i].navigation = navigation;
        }
    }

    private static DialogCloseWatcher AddCloseWatcher(SimpleDialogBox dialog, Action onCancel = null, Action onPrev = null, Action onNext = null)
    {
        var watcher = GetDialogRoot(dialog).AddComponent<DialogCloseWatcher>();
        watcher.dialog = dialog;
        watcher.onCancel = onCancel;
        watcher.onPrev = onPrev;
        watcher.onNext = onNext;
        return watcher;
    }

    private static GameObject GetDialogRoot(SimpleDialogBox dialog)
    {
        return dialog.rootObject ?? dialog.gameObject;
    }

    private static void SetCenteredRect(RectTransform rect, float width, float height)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetLayoutElement(GameObject target, float width, float height)
    {
        var element = target.GetComponent<LayoutElement>() ?? target.AddComponent<LayoutElement>();
        ConfigureLayoutElement(element, width, height);
    }

    private static void ConfigureLayoutElement(LayoutElement element, float width, float height)
    {
        element.minWidth = width;
        element.preferredWidth = width;
        element.minHeight = height;
        element.preferredHeight = height;
        element.flexibleWidth = 0f;
        element.flexibleHeight = 0f;
    }

    private class DialogCloseWatcher : MonoBehaviour
    {
        public SimpleDialogBox dialog;
        public Action onCancel;
        public Action onPrev;
        public Action onNext;
        private float delay = 0.4f;
        private bool closing;

        private void Update()
        {
            if (closing) return;

            delay -= Time.unscaledDeltaTime;
            if (delay > 0f) return;

            // LB (JoystickButton4) -> previous page
            if (Input.GetKeyDown(KeyCode.JoystickButton4))
            {
                try
                {
                    onPrev?.Invoke();
                }
                catch (Exception ex)
                {
                    StageFilter.Logger.LogWarning("Error invoking onPrev action!");
                    StageFilter.Logger.LogError(ex);
                }
            }

            // RB (JoystickButton5) -> next page
            if (Input.GetKeyDown(KeyCode.JoystickButton5))
            {
                try
                {
                    onNext?.Invoke();
                }
                catch (Exception ex)
                {
                    StageFilter.Logger.LogWarning("Error invoking onNext action!");
                    StageFilter.Logger.LogError(ex);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                closing = true;
                var target = dialog.rootObject ?? dialog.gameObject;
                onCancel?.Invoke();
                UnityEngine.Object.Destroy(target);
            }
        }
    }

    private class PageGlyphVisibilityController : MonoBehaviour
    {
        public MPEventSystem eventSystem;
        public InputBindingDisplayController prevPageGlyph;
        public InputBindingDisplayController nextPageGlyph;

        private void Update()
        {
            bool gamepad = eventSystem?.currentInputSource == MPEventSystem.InputSource.Gamepad;
            SetGlyphVisible(prevPageGlyph, gamepad);
            SetGlyphVisible(nextPageGlyph, gamepad);
        }

        private static void SetGlyphVisible(InputBindingDisplayController glyph, bool visible)
        {
            if (glyph is not null)
            {
                var glyphText = glyph.GetComponent<HGTextMeshProUGUI>();
                glyphText?.enabled = visible;
            }
        }
    }
}
