using BeauUtil;
using BeauUtil.Debugger;
using UnityEngine;

public class DebugExample : MonoBehaviour
{
    public DMMenuUI menuUI;
    public Transform slide;
    public Camera cam;

    private bool m_AdvancedLogging;
    [SerializeField, Range(8, 16)] private int m_MyInt = 8;

    private void Awake()
    {
        DMInfo subMenu = new DMInfo("Gameplay");
        subMenu.AddText("Useless Text", () => Time.timeSinceLevelLoad.ToString());
        subMenu.SetMinWidth(200);

        SceneHelper.LoadingState loadingState = SceneHelper.GetLoadingState(SceneHelper.ActiveScene());
        string guid = SceneHelper.GetGUID(SceneHelper.ActiveScene());

        DMInfo rootMenu = new DMInfo("Debug", 8);
        rootMenu
            .AddButton("Log Debug Text", () => print("some debug text"))
            .AddButton("Log More Debug Text", () => print("some more debug text"))
            .AddToggle("Enable Advanced Logging", () => m_AdvancedLogging, (b) => m_AdvancedLogging = b)
                .AddButton("Advanced Log 1", () => print("advanced log!!1!"), () => m_AdvancedLogging, 1)
                .AddButton("Advanced Log 2", () => print("advanced log 2!!1!"), () => m_AdvancedLogging, 1)
            .AddDivider()
            .AddPageBreak()
            .AddSlider("Slide X", () => slide.position.x, (f) => slide.position = new Vector3(f, slide.position.y, slide.position.z), -10, 10, 0, "{0:0.0}", () => m_AdvancedLogging)
            .AddDivider()
            .AddSubmenu(subMenu)
            .AddDivider()
            .AddText("Frame Count", (sb) => sb.AppendNoAlloc(Time.frameCount))
            .AddText("Scene GUID", () => guid)
            .AddSelector("Some Selector", () => m_MyInt, (i) => m_MyInt = i, new int[] { 8, 10, 9 }, new string[] { "Zero", "One", "Two" })
            .AddSelector("Camera Clear Flags", () => (int) cam.clearFlags, (i) => cam.clearFlags = (CameraClearFlags) i, new int[] { (int) CameraClearFlags.Skybox, (int) CameraClearFlags.SolidColor, (int) CameraClearFlags.Depth, (int) CameraClearFlags.Nothing }, new string[] { "Skybox", "Solid Color", "Depth", "Nothing" });

        DMInfo newMenu = new DMInfo("Gameplay");
        newMenu.AddButton("Useless Button", () => { Debug.Log("useless!"); });

        DMInfo.MergeSubmenu(rootMenu, newMenu);

        menuUI.GotoMenu(rootMenu);
    }

    private void LateUpdate()
    {
        menuUI.UpdateElements();

        menuUI.SubmitCommand(GetCommand());
    }

    static private DMMenuUI.NavigationCommand GetCommand()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            return DMMenuUI.NavigationCommand.MoveArrowUp;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            return DMMenuUI.NavigationCommand.MoveArrowDown;
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            return DMMenuUI.NavigationCommand.Back;
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            return DMMenuUI.NavigationCommand.SelectArrow;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                return DMMenuUI.NavigationCommand.DecreaseSlider;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                return DMMenuUI.NavigationCommand.IncreaseSlider;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            return DMMenuUI.NavigationCommand.PrevPage;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            return DMMenuUI.NavigationCommand.NextPage;
        }

        return DMMenuUI.NavigationCommand.None;
    }
}