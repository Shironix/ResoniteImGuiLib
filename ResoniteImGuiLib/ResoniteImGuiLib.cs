using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;
using Elements.Core;
using ImGuiNET;
using UnityEngine;
using ImGuiUnityInject;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityFrooxEngineRunner;

namespace ResoniteImGuiLib;

public static class ImGuiLib
{
    public static ImGuiInstance GetOrCreateInstance(ImGuiReady onReady)
    {
        return GetOrCreateInstance("global", onReady);
    }
    public static ImGuiInstance GetOrCreateInstance(string name = "global", ImGuiReady onReady = null)
    {
        return ImGuiInstance.GetOrCreate((gui, isNew) =>
        {
            if (isNew) gui._camera = SceneManager.GetActiveScene().GetRootGameObjects().Where(go => go.name == "FrooxEngine").First().GetComponent<FrooxEngineRunner>().OverlayCamera;

            if (onReady != null) onReady(gui, isNew);
            else gui.enabled = true;
        });
    }
}

public class ResoniteImGuiLib : ResoniteMod
{
    public override string Name => "ResoniteImGuiLib";
    public override string Author => "art0007i";
    public override string Version => "1.0.0";
    public override string Link => "https://github.com/art0007i/ResoniteImGuiLib/";
    public override void OnEngineInit()
    {
        Harmony harmony = new Harmony("me.art0007i.ResoniteImGuiLib");
        harmony.PatchAll();
    }

    [HarmonyPatch(typeof(MouseDriver), "UpdateMouse")]
    class CursorUpdatePatch
    {
        public static bool Prefix(Mouse mouse)
        {
            var io = ImGui.GetIO();
            if (io.WantCaptureMouse)
            {
                mouse.LeftButton.UpdateState(false);
                mouse.RightButton.UpdateState(false);
                mouse.MiddleButton.UpdateState(false);
                mouse.MouseButton4.UpdateState(false);
                mouse.MouseButton5.UpdateState(false);
                mouse.DirectDelta.UpdateValue(float2.Zero, Time.deltaTime);
                mouse.ScrollWheelDelta.UpdateValue(float2.Zero, Time.deltaTime);
                mouse.NormalizedScrollWheelDelta.UpdateValue(float2.Zero, Time.deltaTime);

                var cursor = ImGui.GetMouseCursor();
                Cursor.visible = cursor != ImGuiMouseCursor.None;
                Cursor.lockState = CursorLockMode.None;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KeyboardDriver), "Current_onTextInput")]
    class KeyboardDeltaPatch
    {
        public static bool Prefix()
        {
            if (ImGui.GetIO().WantCaptureKeyboard)
            {
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(KeyboardDriver), "GetKeyState")]
    class KeyboardStatePatch
    {
        public static bool Prefix(ref bool __result)
        {
            if (ImGui.GetIO().WantCaptureKeyboard)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
