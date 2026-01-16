//using HarmonyLib;
//using System;
//using System.Reflection;
//using System.Collections.Generic;
//using Vintagestory.API.Client;
//using Vintagestory.API.Common;
//using Vintagestory.API.Config;
//using Cairo;
//using Vintagestory.Client.NoObf;
//using Vintagestory.Client;

//namespace SaplingTreeParams2.Systems
//{
//    [HarmonyPatch(typeof(GuiCompositeSettings), "ComposerHeader")]
//    public class SaplingTreeParamGUI
//    {
//        private static bool saplingConfigTab;
//        private static ILogger log;

//        static string[,] headerStrings = {
//            {"setting-graphics-header",       "graphics"           ,"OnGraphicsOptions"},
//            {"setting-mouse-header",          "mouse"              ,"OnMouseOptions"},
//            {"setting-controls-header",       "controls"           ,"OnControlOptions"},
//            {"setting-accessibility-header",  "accessibility"      ,"OnAccessibilityOptions"},
//            {"setting-sound-header",          "sounds"             ,"OnSoundOptions"},
//            {"setting-interface-header",      "interface"          ,"OnInterfaceOptions"},
//            {"setting-dev-header",            "developer"          ,"OnDeveloperOptions"},
//            {"setting-saplig-config-header",  "saplingConfig"      ,""}
//        };

//        // had to copy these from the original code
//        private static ElementBounds[] elementButtonBounds = new ElementBounds[] {
//            ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0),
//            ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0),
//            ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0),
//            ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0),
//            ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0),
//            ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0),
//            ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0),
//            ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0)
//        };

//        private static ElementBounds backButtonBounds = ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0);

//        [HarmonyPrefix]
//        private static bool ComposerHeaderPatch(
//            ref GuiCompositeSettings __instance,
//            ref IGameSettingsHandler ___handler,
//            ref GuiComposer __result,
//            string dialogName,
//            string currentTab
//            )
//        {
//            GuiCompositeSettings nonRefInst = __instance;

//            Type type = typeof(GuiCompositeSettings);
//            CairoFont fnt = CairoFont.ButtonText();

//            FieldInfo onMainScreenFI = type.GetField("onMainscreen", BindingFlags.NonPublic | BindingFlags.Instance);
//            bool onMainScreen = (bool)onMainScreenFI.GetValue(__instance);

//            GuiComposer composerHeader;
//            if (onMainScreen)
//            {
//                int width2 = ScreenManager.Platform.WindowSize.Width;
//                int height = ScreenManager.Platform.WindowSize.Height;
//                ElementBounds containerBounds = ElementBounds.Fixed(0.0, 0.0, 950.0, 740.0);

//                for (int i = 0; i < elementButtonBounds.Length; i++)
//                {
//                    elementButtonBounds[i].ParentBounds = containerBounds;
//                }

//                composerHeader = ___handler.dialogBase(
//                    dialogName + "main",
//                    containerBounds.fixedWidth,
//                    containerBounds.fixedHeight
//                 ).BeginChildElements(containerBounds);


//                for (int i = 0; i < headerStrings.Length; i++)
//                {
//                    if (i != 6)
//                    {
//                        composerHeader.AddToggleButton(
//                            Lang.Get(headerStrings[i, 0], Array.Empty<object>()),
//                            fnt,
//                            (bool on) =>
//                            {
//                                if (i != 7)
//                                    type.GetMethod(headerStrings[i, 2], BindingFlags.NonPublic | BindingFlags.Instance).Invoke(nonRefInst, new object[] { on });
//                                else OnSaplingConfig(on);
//                            },
//                            elementButtonBounds[i],
//                            headerStrings[i, 1]
//                        );
//                    }
//                    else
//                    {
//                        composerHeader.AddIf(ClientSettings.DeveloperMode)
//                        .AddToggleButton(
//                            Lang.Get(headerStrings[i, 0], Array.Empty<object>()),
//                            fnt,
//                            (bool on) => type.GetMethod(headerStrings[i, 2], BindingFlags.NonPublic | BindingFlags.Instance).Invoke(nonRefInst, new object[] { on }),
//                            elementButtonBounds[i],
//                            headerStrings[i, 1]
//                            )
//                        .EndIf();
//                    }
//                }

//            }
//            else
//            {
//                ElementBounds dlgBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterFixed).WithFixedPosition(0.0, 75.0);

//                double width = backButtonBounds.fixedX + backButtonBounds.fixedWidth + 35.0;

//                dlgBounds.horizontalSizing = ElementSizing.Fixed;
//                dlgBounds.fixedWidth = width;

//                ElementBounds bgBounds = new ElementBounds().WithSizing(ElementSizing.FitToChildren).WithFixedPadding(GuiStyle.ElementToDialogPadding);
//                bgBounds.horizontalSizing = ElementSizing.Fixed;
//                bgBounds.fixedWidth = width - 2.0 * GuiStyle.ElementToDialogPadding;

//                for (int i = 0; i < elementButtonBounds.Length; i++)
//                {
//                    elementButtonBounds[i].ParentBounds = bgBounds;
//                }
//                backButtonBounds.ParentBounds = bgBounds;

//                composerHeader = ___handler.GuiComposers
//                    .Create(dialogName + "ingame", dlgBounds)
//                    .AddShadedDialogBG(bgBounds, false, 5.0, 0.75f)
//                    .AddStaticCustomDraw(bgBounds,
//                    (Context ctx, ImageSurface surface, ElementBounds bounds) =>
//                    {
//                        ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.1);
//                        GuiElement.RoundRectangle(ctx, GuiElement.scaled(5.0) + bounds.bgDrawX, GuiElement.scaled(5.0) + bounds.bgDrawY, bounds.OuterWidth - GuiElement.scaled(10.0), GuiElement.scaled(75.0), 1.0);
//                        ctx.Fill();
//                    }).BeginChildElements();

//            }

//            return true;
//        }

//        public static void UpdateButtonBounds(GuiCompositeMainMenuLeft inst)
//        {
//            CairoFont cairoFont = CairoFont.ButtonText();
//            Type cairoFontType = typeof(CairoFont);
//            MethodInfo GetTextExtentsMI = cairoFontType.GetMethod("GetTextExtents", BindingFlags.NonPublic | BindingFlags.Instance);

//            List<double> widthList = new List<double>();

//            for (int i = 0; i < 8; i++)
//            {
//                widthList.Add(
//                    (double)GetTextExtentsMI.Invoke(inst, new object[] { Lang.Get(headerStrings[i, 0], Array.Empty<object>()) })
//                        .GetType()
//                        .GetField("Width", BindingFlags.NonPublic | BindingFlags.Instance)
//                        .GetValue(inst) / (double)ClientSettings.GUIScale + 15.0
//                );
//            }

//            double backWidth = (double)GetTextExtentsMI.Invoke(inst, new object[] { Lang.Get("general-back", Array.Empty<object>()) })
//                .GetType()
//                .GetField("Width", BindingFlags.NonPublic | BindingFlags.Instance)
//                .GetValue(inst) / (double)ClientSettings.GUIScale + 15.0;
//        }


//        private static void OnSaplingConfig(bool on)
//        {

//        }


//    }

//}
