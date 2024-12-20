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
using System.Collections.Generic;

namespace ResoniteImGuiLib;

public static class ImGuiLib
{
    public static ImGuiInstance GetOrCreateInstance(ImGuiReady onReady)
    {
        return GetOrCreateInstance("global", onReady);
    }
    public static ImGuiInstance GetOrCreateInstance(string name = "global", ImGuiReady onReady = null)
    {
        return ImGuiInstance.GetOrCreate(name, (gui, isNew) =>
        {
            if (isNew)
            {
                gui._camera = SceneManager.GetActiveScene().GetRootGameObjects().Where(go => go.name == "FrooxEngine").First().GetComponent<FrooxEngineRunner>().OverlayCamera;
                gui.Layout += () =>
                {
                    var io = ImGui.GetIO();
                    ResoniteImGuiLib.WantCapture[name] = (io.WantCaptureMouse, io.WantCaptureKeyboard);
                };
            }

            if (onReady != null) onReady(gui, isNew);
            else gui.enabled = true;
        });
    }
}

public class ResoniteImGuiLib : ResoniteMod
{
    public override string Name => "ResoniteImGuiLib";
    public override string Author => "art0007i";
    public override string Version => "1.1.0";
    public override string Link => "https://github.com/art0007i/ResoniteImGuiLib/";
    public override void OnEngineInit()
    {
        Harmony harmony = new Harmony("me.art0007i.ResoniteImGuiLib");
        harmony.PatchAll();
    }
    internal static Dictionary<string, (bool, bool)> WantCapture = new();

    [HarmonyPatch(typeof(MouseDriver), "UpdateMouse")]
    class CursorUpdatePatch
    {
        public static bool Prefix(Mouse mouse)
        {
            if (WantCapture.Any(x=>x.Value.Item1))
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
            if (WantCapture.Any(x => x.Value.Item2))
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
            if (WantCapture.Any(x => x.Value.Item2))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
