using MelonLoader;
using PlagueGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Max_The_Sex_Slave.RealFeel_API;
using UnityEngine;
using Object = UnityEngine.Object;
using Libraries;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(Max_The_Sex_Slave.Slave), "Max The Sex Slave", "1.0", "Kannya")]
[assembly: MelonGame("T-Hoodie Draws", "Max_The_Elf_DEMO")]

namespace Max_The_Sex_Slave
{
    public class Slave : MelonMod
    {
        private static float CumBarAmount = 0f;
        private static bool InfHealth = false;
        private static bool NoCum = false;
        private static bool NoTaken = false;
        private static bool RealFeelSupport = false;
        private static bool NoStruggle = false;

        public static bool RealFeelPulseMode = false;

        private static MaxControllerRevamp MaxCon;
        private static MaxAttacks MaxAttac;
        private static GameplayManager GamePlayMan;
        private static UI ui;

        public static float RangeConv(float input, float MinPossibleInput, float MaxPossibleInput, float MinConv, float MaxConv)
        {
            return (input - MinPossibleInput) * (MaxConv - MinConv) / (MaxPossibleInput - MinPossibleInput) + MinConv;
        }

        public override void OnApplicationStart()
        {
            MelonLogger.Msg(";)");

            vibrator = new Vibrator();

            HarmonyInstance.Patch(typeof(MaxControllerRevamp).GetMethod(nameof(MaxControllerRevamp.ReceiveDamage), AccessTools.all), new HarmonyMethod(typeof(Slave).GetMethod(nameof(PreReceiveDamage), BindingFlags.NonPublic | BindingFlags.Static)));

            HarmonyInstance.Patch(typeof(MaxControllerRevamp).GetMethod(nameof(MaxControllerRevamp.ClearStruggle), AccessTools.all), new HarmonyMethod(typeof(Slave).GetMethod(nameof(PreClearFuck), BindingFlags.NonPublic | BindingFlags.Static)));

            HarmonyInstance.Patch(typeof(MaxControllerRevamp).GetMethod(nameof(MaxControllerRevamp.ClearMasturbate), AccessTools.all), new HarmonyMethod(typeof(Slave).GetMethod(nameof(PreClearFuck), BindingFlags.NonPublic | BindingFlags.Static)));

            HarmonyInstance.Patch(typeof(MaxAnimationCallbacks).GetMethod(nameof(MaxAnimationCallbacks.receiveFuckCaller), AccessTools.all), new HarmonyMethod(typeof(Slave).GetMethod(nameof(PreReceiveFuck), BindingFlags.NonPublic | BindingFlags.Static)));

            HarmonyInstance.Patch(typeof(GameplayManager).GetMethod(nameof(GameplayManager.AddCum), AccessTools.all), new HarmonyMethod(typeof(Slave).GetMethod(nameof(CumAmount), BindingFlags.NonPublic | BindingFlags.Static)));

            HarmonyInstance.Patch(typeof(GameplayManager).GetMethod(nameof(GameplayManager.SetCum), AccessTools.all), new HarmonyMethod(typeof(Slave).GetMethod(nameof(CumAmount), BindingFlags.NonPublic | BindingFlags.Static)));

            HarmonyInstance.Patch(typeof(MaxControllerRevamp).GetMethod(nameof(MaxControllerRevamp.SetEnemy), AccessTools.all), null, new HarmonyMethod(typeof(Slave).GetMethod(nameof(PostSetEnemy), BindingFlags.NonPublic | BindingFlags.Static)));

            //HarmonyInstance.Patch(typeof(MaxControllerRevamp).GetMethod(nameof(MaxControllerRevamp.ResetEnemy), AccessTools.all), null, new HarmonyMethod(typeof(Slave).GetMethod(nameof(PostResetEnemy), BindingFlags.NonPublic | BindingFlags.Static)));

            MelonCoroutines.Start(DoAutoPassword());

            IEnumerator DoAutoPassword()
            {
                var PasswordHandler = GameObject.Find("Canvas")?.transform?.Find("SubMenus/Password")?.GetComponent<PasswordHandler>();

                while (PasswordHandler?.InputFieldComponent == null)
                {
                    yield return new WaitForSeconds(0.1f);

                    PasswordHandler = GameObject.Find("Canvas")?.transform?.Find("SubMenus/Password")?.GetComponent<PasswordHandler>();
                }

                var DB = Database.Instance;

                PasswordHandler.InputFieldComponent.text = DB.passwords.Last();

                PasswordHandler.ApplyPassword();

                MelonLogger.Msg("Auto-Unlocked Levels!");
            }

            LoadUI();
        }

        //private static enemyInfo CurrentEnemy;

        private static void PostSetEnemy(ref GameObject __0)
        {
            if (NoStruggle)
            {
                return;
            }

            //CurrentEnemy = __0.transform.parent?.gameObject.GetComponent<enemyInfo>();

            // Auto Zoom
            var ZoomController = Object.FindObjectOfType<ZoomControl>();

            typeof(ZoomControl).GetField("m_zoomedIn", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ZoomController, false);
            ZoomController.zoomCamWrapper();
        }

        //private static void PostResetEnemy()
        //{
        //    CurrentEnemy = null;
        //}

        private static bool PreClearFuck()
        {
            if (NoStruggle)
            {
                return true;
            }

            // Auto Zoom
            var ZoomController = Object.FindObjectOfType<ZoomControl>();

            typeof(ZoomControl).GetField("m_zoomedIn", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ZoomController, true);
            ZoomController.zoomCamWrapper();

            vibrator.Vibrate(0);

            return true;
        }

        private static bool PreReceiveFuck()
        {
            if (RealFeelSupport)
            {
                var Pleasure = GamePlayMan.GetCum() + (float)typeof(MaxControllerRevamp).GetField("currentEnemyDamage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(MaxCon);

                MelonLogger.Msg($"Pleasure: {Pleasure}");

                var ScaledPleasure = RangeConv(Pleasure, 1f, 100f, 1f, 20f);

                if (ScaledPleasure > 20f)
                {
                    ScaledPleasure = 20f;
                }

                //if (!RealFeelPulse)
                //{
                //    GUI.Label(new Rect(75, 120, 175, 100), $"Raw Pleasure: {Pleasure}, Pleasure: {ScaledPleasure}");

                //    vibrator.Vibrate((int)ScaledPleasure);
                //}
                //else
                //GUI.Label(new Rect(75, 120, 175, 100), $"Raw Pleasure: {Pleasure}, Pleasure: {ScaledPleasure}");

                vibrator.Vibrate((int)ScaledPleasure);
            }

            return true;
        }

        private static bool PreReceiveDamage(ref bool __0) // shake
        {
            return !NoCum;
        }

        private static bool CumAmount(ref float __0, ref bool __1) // cum, shake - 108 max
        {
            return !NoCum;
        }

        private static GameObject TogglePrefab = null;
        private static void LoadUI()
        {
            if (new AssetBundleLib() is var Bundle && Bundle.LoadBundle(Properties.Resources.toggle)) // This If Also Checks If It Successfully Loaded As To Prevent Further Exceptions
            {
                TogglePrefab = Bundle.Load<GameObject>("Toggle.prefab");

                MelonLogger.Msg("Loaded Bundle!");
            }
            else
            {
                MelonLogger.Error($"Failed Loading Bundle: {Bundle.error}");
            }
        }

        private static IEnumerator CreateUI()
        {
            MelonLogger.Msg("Waiting To Init UI..");

            // Adjustments
            var Options = GameObject.Find("Game_Canvas").transform.Find("UI/SubWindows/options/");

            if (Options == null)
            {
                MelonLogger.Msg("Not Found.");
            }

            while (Options == null)
            {
                MelonLogger.Msg("Not Found..");

                yield return new WaitForEndOfFrame();

                Options = GameObject.Find("Game_Canvas").transform.Find("UI/SubWindows/options/");
            }

            MelonLogger.Msg("UI Found.");

            // Move Sliders Up
            //Options.Find("Slider").localPosition = new Vector3(0f, 156f, 0f);
            //Options.Find("Title (1)").localPosition = new Vector3(-298.33f, 224f, 0f);

            //Options.Find("Slider (1)").localPosition = new Vector3(0f, -2f, 0f);
            //Options.Find("Title (2)").localPosition = new Vector3(-298.33f, 64f, 0f);

            //MelonLogger.Msg("Adjustments Done.");

            var toggle = Object.Instantiate(TogglePrefab).transform;
            toggle.SetParent(Options);
            //toggle.localPosition = new Vector3(17f, -67f, 0f);
            toggle.localPosition = new Vector3(17f, -150f, 0f);
            toggle.localScale = new Vector3(1.668443f, 1.953655f, 0.7199998f);

            toggle.GetComponentInChildren<Text>().text = "RealFeel Support";

            MelonLogger.Msg("Toggle Created.");

            var _event = toggle.GetComponent<Toggle>().onValueChanged = new Toggle.ToggleEvent();
            toggle.GetComponent<Toggle>().isOn = RealFeelSupport;
            _event.AddListener((val) =>
            {
                RealFeelSupport = val;

                vibrator.Vibrate(0);

                MelonLogger.Msg($"Changed RealFeel Support To: {RealFeelSupport}!");
            });

            MelonLogger.Msg("Created UI");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            MelonLogger.Msg($"Loaded Scene: {buildIndex}-{sceneName}");

            MaxCon = Resources.FindObjectsOfTypeAll<MaxControllerRevamp>().FirstOrDefault();
            MaxAttac = Resources.FindObjectsOfTypeAll<MaxAttacks>().FirstOrDefault();
            GamePlayMan = Resources.FindObjectsOfTypeAll<GameplayManager>().FirstOrDefault();
            ui = Resources.FindObjectsOfTypeAll<UI>().FirstOrDefault();

            if (buildIndex != 0 && buildIndex != 1)
            {
                MelonCoroutines.Start(CreateUI());

                //Enemies = Resources.FindObjectsOfTypeAll<enemyInfo>().DistinctBy(o => o.enemyIdx).ToArray();

                //if (Enemies.Length > 0)
                //{
                //    MaxDamage = (Enemies.Min(o => o.damage), Enemies.Max(o => o.damage));

                //    MelonLogger.Msg($"Found {Enemies.Length} Enemy Variations In Level, Min Damage: {MaxDamage.Item1}, Min Damage: {MaxDamage.Item2}.");

                //    MelonLogger.Msg($"Enemy IDs: {string.Join(", ", Enemies.Select(o => o.gameObject.name.Replace("Dummy", "")))}");
                //}
                //else
                //{
                //    MelonLogger.Msg("No Enemies Found.");
                //}
            }
        }

        private static Vibrator vibrator;

        public override void OnGUI()
        {
            MaxCon ??= Resources.FindObjectsOfTypeAll<MaxControllerRevamp>().FirstOrDefault();
            MaxAttac ??= Resources.FindObjectsOfTypeAll<MaxAttacks>().FirstOrDefault();
            GamePlayMan ??= Resources.FindObjectsOfTypeAll<GameplayManager>().FirstOrDefault();
            ui ??= Resources.FindObjectsOfTypeAll<UI>().FirstOrDefault();

            if (MaxCon == null || MaxAttac == null || GamePlayMan == null || ui == null)
            {
                return;
            }

            if (InfHealth)
            {
                GamePlayMan.SetCorruptionLevel((int)CumBarAmount);
                
                var corruption = ((int)typeof(UI).GetField("corruptionLevel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ui));

                if (corruption < ui.corruptionLevels.Count)
                {
                    for (var index = 0; index < ui.corruptionLevels.Count; index++)
                    {
                        var level = ui.corruptionLevels[index];

                        if (index > (corruption - 1))
                        {
                            level.color = new Color(1f, 1f, 1f, 0.298f);
                        }
                    }

                    if (corruption - 1 >= 0)
                    {
                        ui.corruptionLevels[corruption - 1].color = Color.white;
                    }
                }

                typeof(UI).GetField("specialCount", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ui, 3);
                MaxAttac.TotalSpecials = int.MaxValue;
            }

            if (NoTaken && !GameplayManager.m_timerPaused)
            {
                GameplayManager.m_timerPaused = true;
            }

            if (NoStruggle)
            {
                MaxCon.ClearStruggle(false, 0);
            }

            var ListOfButtonsx = new List<KeyValuePair<Tuple<string, string, ButtonType, bool>, Action<string, int, float, bool>>>();/*Cache The List So You Can Append Numerous Things To It First, And Keep Your Code Clean*/

            ListOfButtonsx.Add(new KeyValuePair<Tuple<string, string, ButtonType, bool>, Action<string, int, float, bool>>(new Tuple<string, string, ButtonType, bool>(/*Button Text*/"Clear Struggle", /*ToolTip Text*/"Gets You Free From Being Raped!", /*Button Type*/ButtonType.Toggle, /*Default Toggle State*/NoStruggle), /*Delegate To Execute On Button Select/Toggle*/delegate (string a, int b, float c, bool d)
            {
                NoStruggle = d;
            }));

            ListOfButtonsx.Add(new KeyValuePair<Tuple<string, string, ButtonType, bool>, Action<string, int, float, bool>>(new Tuple<string, string, ButtonType, bool>(/*Button Text*/"Infinite Health", /*ToolTip Text*/"Inability To Be Fully Corrupted", /*Button Type*/ButtonType.Toggle, /*Default Toggle State*/InfHealth), /*Delegate To Execute On Button Select/Toggle*/delegate (string a, int b, float c, bool d)
            {
                InfHealth = d;
            }));

            ListOfButtonsx.Add(new KeyValuePair<Tuple<string, string, ButtonType, bool>, Action<string, int, float, bool>>(new Tuple<string, string, ButtonType, bool>(/*Button Text*/"No Cumming", /*ToolTip Text*/"Inability To Cum", /*Button Type*/ButtonType.Toggle, /*Default Toggle State*/NoCum), /*Delegate To Execute On Button Select/Toggle*/delegate (string a, int b, float c, bool d)
            {
                NoCum = d;
            }));

            ListOfButtonsx.Add(new KeyValuePair<Tuple<string, string, ButtonType, bool>, Action<string, int, float, bool>>(new Tuple<string, string, ButtonType, bool>(/*Button Text*/"No Taken Time", /*ToolTip Text*/"Inability To Be Taken", /*Button Type*/ButtonType.Toggle, /*Default Toggle State*/NoTaken), /*Delegate To Execute On Button Select/Toggle*/delegate (string a, int b, float c, bool d)
            {
                NoTaken = d;
            }));

            ListOfButtonsx.Add(new KeyValuePair<Tuple<string, string, ButtonType, bool>, Action<string, int, float, bool>>(new Tuple<string, string, ButtonType, bool>(/*Button Text*/"Corruption Level For Inf Health", /*ToolTip Text*/"Choose Your Corruption!", /*Button Type*/ButtonType.Slider, /*Default Toggle State*/false), /*Delegate To Execute On Button Select/Toggle*/delegate (string a, int b, float c, bool d)
            {
                CumBarAmount = c;
            }));

            ListOfButtonsx.Add(new KeyValuePair<Tuple<string, string, ButtonType, bool>, Action<string, int, float, bool>>(new Tuple<string, string, ButtonType, bool>(/*Button Text*/"RealFeel Pulse Mode", /*ToolTip Text*/"Changes The RealFeel Vibration System To Pulse At The Raising Intensity, Then Back To 0, Instead Of Raising A Constant Vibration Level.", /*Button Type*/ButtonType.Toggle, /*Default Toggle State*/RealFeelPulseMode), /*Delegate To Execute On Button Select/Toggle*/delegate (string a, int b, float c, bool d)
            {
                RealFeelPulseMode = d;
            }));

            PlagueGUI.PlagueGUI.DropDown(/*The Position And Scale Of The DropDown*/new Rect(25, 25, 300, 25), /*The Main DropDown Expand Button Text*/"Sex Slave's Options", ListOfButtonsx, false);
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
