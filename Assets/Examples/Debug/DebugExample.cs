using BeauUtil.Debugger;
using UnityEngine;

public class DebugExample : MonoBehaviour
{
    public DMMenuUI menuUI;
    public Transform slide;

    private bool m_AdvancedLogging;

    private void Awake()
    {
        DMInfo subMenu = new DMInfo("Gameplay");
        subMenu.AddText("Useless Text", () => Time.timeSinceLevelLoad.ToString());
        subMenu.SetMinWidth(200);

        DMInfo rootMenu = new DMInfo("Debug", 8);
        rootMenu
            .AddSubmenu(subMenu)
            .AddButton("Log Debug Text", () => print("some debug text"))
            .AddButton("Log More Debug Text", () => print("some more debug text"))
            .AddToggle("Enable Advanced Logging", () => m_AdvancedLogging, (b) => m_AdvancedLogging = b)
                .AddButton("Advanced Log 1", () => print("advanced log!!1!"), () => m_AdvancedLogging, 1)
                .AddButton("Advanced Log 2", () => print("advanced log 2!!1!"), () => m_AdvancedLogging, 1)
            .AddDivider()
            .AddSlider("Slide X", () => slide.position.x, (f) => slide.position = new Vector3(f, slide.position.y, slide.position.z), -10, 10, 0, "{0:0.0}", () => m_AdvancedLogging)
            .AddDivider()
            .AddText("Frame Count", () => Time.frameCount.ToString());

        menuUI.GotoMenu(rootMenu);
    }

    private void LateUpdate()
    {
        menuUI.UpdateElements();
    }
}