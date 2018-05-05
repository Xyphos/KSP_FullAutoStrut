// 
//  MIT License
//  
//  Copyright (c) 2017 William "Xyphos" Scott
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using KSP.UI.Screens;
using UniLinq;
using UnityEngine;

namespace FullAutoStrut
{
    [KSPAddon(startup: KSPAddon.Startup.EditorAny, once: false)]
    public class FullAutoStrut : MonoBehaviour
    {
        private Texture _appIcon;

        private ApplicationLauncherButton _button;
        private PopupDialog               _dialog;

        [Persistent]
        public bool ApplyChildren;

        [Persistent]
        public bool AutoSelect = true;

        [Persistent]
        public Part.AutoStrutMode AutoStrutMode;

        [Persistent]
        public bool ModEnabled = true;

        [Persistent]
        public float PosX;

        [Persistent]
        public float PosY;

        [Persistent]
        public bool RigidAttachment;

        [Conditional(conditionString: "DEBUG")]
        private static void DebugLog(string m) { print(message: "[FAS] " + m); }

        [Conditional(conditionString: "DEBUG")]
        private static void DebugLog(Exception e) { print(message: "[FAS] " + e); }


        /// <summary>
        ///   Called when this module is deactivated.
        /// </summary>
        public void OnDisable()
        {
            try
            {
                //print("OnDisable()");
                GameEvents.onEditorPartEvent.Remove(evt: OnPartEvent);
            }
            catch (Exception e)
            {
                DebugLog(e: e);
            }
        }

        /// <summary>
        ///   Called when this module is activated.
        /// </summary>
        public void Awake()
        {
            try
            {
                DebugLog(m: "Awake()");

                if (_appIcon == null)
                    _appIcon = GameDatabase.Instance.GetTexture(url: "XyphosAerospace/Plugins/FullAutoStrut/fas", asNormalMap: false);

                GameEvents.onGUIApplicationLauncherReady.Add(evt: OnGuiApplicationLauncherReady);
                GameEvents.onGUIApplicationLauncherUnreadifying.Add(evt: OnGuiApplicationLauncherUnreadifying);
                GameEvents.onEditorPartEvent.Add(evt: OnPartEvent);
            }
            catch (Exception e)
            {
                DebugLog(e: e);
            }
        }


        /// <summary>
        ///   Called when this mondule is destroyed.
        /// </summary>
        public void OnDestroy()
        {
            try
            {
                DebugLog(m: "OnDestroy()");
                GameEvents.onGUIApplicationLauncherReady.Remove(evt: OnGuiApplicationLauncherReady);
                GameEvents.onGUIApplicationLauncherUnreadifying.Remove(evt: OnGuiApplicationLauncherUnreadifying);
            }
            catch (Exception e)
            {
                DebugLog(e: e);
            }
        }

        /// <summary>
        ///   Called when [GUI application launcher ready].
        /// </summary>
        private void OnGuiApplicationLauncherReady()
        {
            try
            {
                DebugLog(m: "OnGUIApplicationLauncherReady()");
                _button = ApplicationLauncher.Instance.AddModApplication(
                                                                         onTrue: OnAppLauncherTrue,
                                                                         onFalse: OnAppLauncherFalse,
                                                                         onHover: null, onHoverOut: null, onEnable: null, onDisable: null,
                                                                         visibleInScenes: ApplicationLauncher.AppScenes.ALWAYS,
                                                                         texture: _appIcon);
            }
            catch (Exception e)
            {
                DebugLog(e: e);
            }
        }

        private void OnAppLauncherFalse()
        {
            if (_dialog == null) return;

            PosX = _dialog.RTrf.position.x;
            PosY = _dialog.RTrf.position.y;
            _dialog.Dismiss();
            _dialog = null;
        }

        private void OnAppLauncherTrue()
        {
            const float Q = 0.5f;
            const float H = 14F;
            OnAppLauncherFalse(); // make sure it's closed

            var components = new List<DialogGUIBase> {new DialogGUIFlexibleSpace()};

            if (!GameSettings.ADVANCED_TWEAKABLES)
            {
                components.Add(item: new DialogGUILabel(message: "'Advanced Tweakables' needs to be enabled.\nClick the button below to enable it."));
                components.Add(item: new DialogGUIButton(optionText: "Enable 'Advanced Tweakables'",
                                                         onSelected: () =>
                                                                     {
                                                                         GameSettings.ADVANCED_TWEAKABLES = true;
                                                                         GameSettings.SaveSettings();
                                                                         OnAppLauncherFalse();
                                                                         OnAppLauncherTrue();
                                                                     }, w: -1f, h: H, dismissOnSelect: false));

                goto SPAWN; // dirty kludge, I know...
            }

            components.Add(item: new DialogGUIToggle(set: ModEnabled,
                                                     lbel: "Enable FAS",
                                                     selected: b =>
                                                               {
                                                                   ModEnabled = b;
                                                                   OnAppLauncherFalse();
                                                                   OnAppLauncherTrue();
                                                               }, w: -1f, h: H));
            if (!ModEnabled)
            {
                components.Add(item: new DialogGUILabel(message: "Full AutoStrut is currently disabled."));
                goto SPAWN; // dirty kludge, I know...
            }

            components.Add(item: new DialogGUIToggle(set: RigidAttachment,
                                                     lbel: "Use Rigid Attachment",
                                                     selected: b => RigidAttachment = b,
                                                     w: -1f, h: H));

            components.Add(item: new DialogGUIFlexibleSpace());

            components.Add(item: new DialogGUIToggleGroup(new DialogGUIToggleButton(set: AutoSelect    == false
                                                                                      && AutoStrutMode == Part.AutoStrutMode.Off,
                                                                                    lbel: "Off",
                                                                                    selected: b =>
                                                                                              {
                                                                                                  if (!b) return;
                                                                                                  AutoSelect    = false;
                                                                                                  AutoStrutMode = Part.AutoStrutMode.Off;
                                                                                              }, w: -1f, h: H),
                                                          new DialogGUIToggleButton(set: AutoSelect    == false
                                                                                      && AutoStrutMode == Part.AutoStrutMode.Root,
                                                                                    lbel: "Root",
                                                                                    selected: b =>
                                                                                              {
                                                                                                  if (!b) return;
                                                                                                  AutoSelect    = false;
                                                                                                  AutoStrutMode = Part.AutoStrutMode.Root;
                                                                                              }, w: -1f, h: H),
                                                          new DialogGUIToggleButton(set: AutoSelect    == false
                                                                                      && AutoStrutMode == Part.AutoStrutMode.Heaviest,
                                                                                    lbel: "Heaviest",
                                                                                    selected: b =>
                                                                                              {
                                                                                                  if (!b) return;
                                                                                                  AutoSelect    = false;
                                                                                                  AutoStrutMode = Part.AutoStrutMode.Heaviest;
                                                                                              }, w: -1f, h: H),
                                                          new DialogGUIToggleButton(set: AutoSelect    == false
                                                                                      && AutoStrutMode == Part.AutoStrutMode.Grandparent,
                                                                                    lbel: "Grandparent",
                                                                                    selected: b =>
                                                                                              {
                                                                                                  if (!b) return;
                                                                                                  AutoSelect    = false;
                                                                                                  AutoStrutMode = Part.AutoStrutMode.Grandparent;
                                                                                              }, w: -1f, h: H),
                                                          new DialogGUIToggleButton(set: AutoSelect,
                                                                                    lbel: "Automatic",
                                                                                    selected: b => AutoSelect = b,
                                                                                    w: -1f, h: H)
                                                          /*,                                                          
                                                           new DialogGUIToggleButton(set: AutoSelect    == false
                                                                                       && AutoStrutMode == Part.AutoStrutMode.ForceRoot,
                                                                                     lbel: "Force Root",
                                                                                     selected: b =>
                                                                                               {
                                                                                                   if (!b) return;
                                                                                                   AutoSelect    = false;
                                                                                                   AutoStrutMode = Part.AutoStrutMode.ForceRoot;
                                                                                               }, w: -1f, h: H),
                                                           new DialogGUIToggleButton(set: AutoSelect    == false
                                                                                       && AutoStrutMode == Part.AutoStrutMode.ForceHeaviest,
                                                                                     lbel: "Force Heaviest",
                                                                                     selected: b =>
                                                                                               {
                                                                                                   if (!b) return;
                                                                                                   AutoSelect    = false;
                                                                                                   AutoStrutMode = Part.AutoStrutMode.ForceHeaviest;
                                                                                               }, w: -1f, h: H),
                                                           new DialogGUIToggleButton(set: AutoSelect    == false
                                                                                       && AutoStrutMode == Part.AutoStrutMode.ForceGrandparent,
                                                                                     lbel: "Force Grandparent",
                                                                                     selected: b =>
                                                                                               {
                                                                                                   if (!b) return;
                                                                                                   AutoSelect    = false;
                                                                                                   AutoStrutMode = Part.AutoStrutMode.ForceGrandparent;
                                                                                               }, w: -1f, h: H)
                                                             //*/
                                                         ));
            components.Add(item: new DialogGUIToggle(set: ApplyChildren,
                                                     lbel: "Re-Apply to child parts",
                                                     selected: b => ApplyChildren = b,
                                                     w: -1f, h: H));


            SPAWN: // dirty kludge, I know...
            _dialog = PopupDialog.SpawnPopupDialog(anchorMin: new Vector2(x: Q, y: Q),
                                                   anchorMax: new Vector2(x: Q, y: Q),
                                                   dialog: new MultiOptionDialog("", "", "Full AutoStrut", HighLogic.UISkin,
                                                                                 new Rect(x: Q, y: Q, width: 200f, height: -1f),
                                                                                 //new DialogGUIFlexibleSpace(),
                                                                                 new DialogGUIVerticalLayout(list: components.ToArray())
                                                                                ),
                                                   persistAcrossScenes: false,
                                                   skin: HighLogic.UISkin,
                                                   isModal: false
                                                  );
        }

        private void OnGuiApplicationLauncherUnreadifying(GameScenes data)
        {
            try
            {
                DebugLog(m: "OnGuiApplicationLauncherUnreadifying()");

                if (ApplicationLauncher.Ready
                 && _button != null)
                    ApplicationLauncher.Instance.RemoveModApplication(button: _button);

                _button = null;
            }
            catch (Exception e)
            {
                DebugLog(e: e);
            }
        }


        /// <summary>
        ///   Called when a part does something.
        /// </summary>
        /// <param name="ct">The ct.</param>
        /// <param name="p">The p.</param>
        private void OnPartEvent(ConstructionEventType ct, Part p)
        {
            try
            {
                // when removed, reset the strut mode to OFF so it can be properly set again when reattached.
                if (ct == ConstructionEventType.PartDetached)
                {
                    // revert part to default
                    var loadedPart = PartLoader.Instance.loadedParts.First(predicate: part => part.name.Equals(value: p.name));
                    SetAutoStrut(p: p, asm: loadedPart.partPrefab.autoStrutMode);
                    return;
                }

                // if the event isn't detach or attach, stop here.
                if (ct != ConstructionEventType.PartAttached)
                    return;

                DebugLog(m: "OnPartAttached");

                // check for mod enabled
                if (!ModEnabled)
                    return;

                // check for null
                if (p == null)
                {
                    DebugLog(m: "Part is null");
                    return;
                }

                // check if allowed
                if (!p.AllowAutoStruts())
                {
                    DebugLog(m: "AutoStruts is disallowed");
                    return;
                }

                // some parts have their autostrut already set by default, do not override this.
                if (p.autoStrutMode != Part.AutoStrutMode.Off)
                {
                    DebugLog(m: "AutoStrut is already set");
                    return;
                }

                // ignore parts with null parents
                if (p.parent == null)
                {
                    DebugLog(m: "Placed part's parent is null, ignoring.");
                    return;
                }

                // Added Infernal Robotics Comaptibility - Crawl the parts hierarchy to check for robotic parents
                var p2 = p;
                while (p2 != null)
                {
                    if (p2.Modules.Contains(className: "MuMechToggle"))
                    {
                        DebugLog(m: "Placed part is a robotic attachment, ignoring.");
                        return;
                    }

                    p2 = p2.parent; // next itteration
                }

                // old functionality preserved, but now optional
                if (AutoSelect)
                {
                    // if grandparent is null, set to root
                    if (p.parent.parent == null)
                    {
                        SetAutoStrut(p: p, asm: Part.AutoStrutMode.Root);
                        return;
                    }

                    SetAutoStrut(p: p, asm: Part.AutoStrutMode.Grandparent);
                    return;
                }

                // otherwise, use the GUI selection
                SetAutoStrut(p: p, asm: AutoStrutMode);
            }
            catch (Exception e)
            {
                DebugLog(e: e);
            }
        }

        private void SetAutoStrut(Part p, Part.AutoStrutMode asm)
        {
            DebugLog(m: $"Setting AutoStrut to: {asm}");
            p.autoStrutMode   = asm;
            p.rigidAttachment = !RigidAttachment;
            p.ToggleRigidAttachment();
            p.UpdateAutoStrut();


            foreach (var pcp in p.symmetryCounterparts)
            {
                pcp.autoStrutMode   = asm;
                pcp.rigidAttachment = !RigidAttachment;
                pcp.ToggleRigidAttachment();
                pcp.UpdateAutoStrut();
            }

            if (ApplyChildren)
                foreach (var child in p.children)
                    SetAutoStrut(p: child, asm: asm);
        }
    }
}
