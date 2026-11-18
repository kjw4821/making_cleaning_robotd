using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

internal sealed class RobotRunHud : MonoBehaviour
{
    private RobotController robot;
    private Font font;
    private UnityEngine.UI.Text status;
    private UnityEngine.UI.Button run;
    private UnityEngine.UI.Button step;
    private UnityEngine.UI.Button pause;

    public void Initialize(RobotController controller)
    {
        robot = controller;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var root = new GameObject("Robot HUD", typeof(Canvas),
            typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        root.transform.SetParent(transform, false);
        root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800, 600);

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var input = new GameObject("Robot HUD Input", typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            input.transform.SetParent(root.transform, false);
        }

        var panel = new GameObject("Run Controls", typeof(RectTransform), typeof(UnityEngine.UI.Image),
            typeof(UnityEngine.UI.HorizontalLayoutGroup));
        panel.transform.SetParent(root.transform, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(0f, 54f);
        panel.GetComponent<UnityEngine.UI.Image>().color = new Color(0.08f, 0.12f, 0.18f, 0.97f);

        var layout = panel.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 8, 8);
        layout.spacing = 6;
        layout.childForceExpandWidth = false;

        status = MakeLabel(panel.transform, "Status");
        status.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().flexibleWidth = 1;
        run = MakeButton(panel.transform, "Run", robot.StartAutomaticRules);
        pause = MakeButton(panel.transform, "Pause", robot.PauseAutomaticRules);
        step = MakeButton(panel.transform, "Step", () => robot.TryRunRuleStep());
        MakeButton(panel.transform, "Reset", () => robot.ResetToStart());
        MakeButton(panel.transform, "Faster", () => robot.AdjustRuleInterval(-0.05f));
        MakeButton(panel.transform, "Slower", () => robot.AdjustRuleInterval(0.05f));
        LateUpdate();
    }

    private void LateUpdate()
    {
        string progress = robot.IsCleared ? "Cleared" : $"Dust left: {robot.RemainingDust}";
        string display = $"{progress}   |   Delay: {robot.RuleInterval:0.00}s";
        if (status.text != display)
            status.text = display;

        bool canRun = robot.isActiveAndEnabled && !robot.IsCleared;
        run.interactable = canRun && !robot.IsAutomatic;
        pause.interactable = robot.IsAutomatic;
        step.interactable = canRun && !robot.IsAutomatic && !robot.IsBusy;
    }

    private UnityEngine.UI.Button MakeButton(Transform parent, string title, UnityEngine.Events.UnityAction action)
    {
        var item = new GameObject(title, typeof(RectTransform), typeof(UnityEngine.UI.Image),
            typeof(UnityEngine.UI.Button), typeof(UnityEngine.UI.LayoutElement));
        item.transform.SetParent(parent, false);
        item.GetComponent<UnityEngine.UI.LayoutElement>().preferredWidth = 70;
        var button = item.GetComponent<UnityEngine.UI.Button>();
        button.targetGraphic = item.GetComponent<UnityEngine.UI.Image>();
        button.onClick.AddListener(action);

        var label = MakeLabel(item.transform, title);
        label.text = title;
        label.color = new Color(0.08f, 0.12f, 0.18f);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    private UnityEngine.UI.Text MakeLabel(Transform parent, string name)
    {
        var item = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Text));
        item.transform.SetParent(parent, false);
        var label = item.GetComponent<UnityEngine.UI.Text>();
        label.font = font;
        label.fontSize = 15;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;
        return label;
    }
}
