// 
//  MIT License
// 
//  Copyright (c) 2017-2019 William "Xyphos" Scott (TheGreatXyphos@gmail.com)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in all
//   copies or substantial portions of the Software.
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
using System.Reflection;
using System.Text.RegularExpressions;
using KSP.UI.Screens;
using UnityEngine;

namespace XyphosAerospace
{
  [KSPAddon(startup: KSPAddon.Startup.EditorAny, once: false)]
  public class FullAutoStrut : MonoBehaviour
  {
    internal static readonly string AssemblyVersion = GetAssemblyVersion();

    internal static string ModPath = Regex.Replace(
        input: Assembly.GetExecutingAssembly().CodeBase,
        pattern: "^.+GameData/(.+)FullAutoStrut\\.dll$",
        replacement: @"$1",
        options: RegexOptions.IgnoreCase
      );


    private Texture                   _appIcon;
    private DialogGUIToggleButton     _applyChildren;
    private ApplicationLauncherButton _button;
    private PopupDialog               _dialog;

    /// <summary>
    ///   Gets the assembly version.
    /// </summary>
    /// <returns></returns>
    private static string GetAssemblyVersion()
    {
      var assembly        = Assembly.GetExecutingAssembly();
      var fileVersionInfo = FileVersionInfo.GetVersionInfo(fileName: assembly.Location);
      return fileVersionInfo.FileVersion;
    }


    /// <summary>
    ///   Logs debugging information. (DEBUG builds only)
    /// </summary>
    /// <param name="m">The m.</param>
    [Conditional(conditionString: "DEBUG")]
    internal static void DebugLog(object m) => print(message: "[Full AutoStrut] " + m);

    /// <summary>
    ///   Called when this module is deactivated.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public void OnDisable()
    {
      try
      {
        DebugLog(m: "OnDisable()");
        GameEvents.onEditorPartEvent.Remove(evt: OnPartEvent);
        Settings.Save();
      }
      catch (Exception e)
      {
        DebugLog(m: e);
      }
    }

    /// <summary>
    ///   Called when this module is activated.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public void Awake()
    {
      try
      {
        DebugLog(m: "Awake()");

        var iconPath = ModPath + "fas";
        DebugLog(m: $"iconPath: {iconPath}");

        if (_appIcon == null) _appIcon = GameDatabase.Instance.GetTexture(url: iconPath, asNormalMap: false);

        GameEvents.onGUIApplicationLauncherReady.Add(evt: OnGuiApplicationLauncherReady);
        GameEvents.onGUIApplicationLauncherUnreadifying.Add(evt: OnGuiApplicationLauncherUnreadifying);
        GameEvents.onEditorPartEvent.Add(evt: OnPartEvent);
      }
      catch (Exception e)
      {
        DebugLog(m: e);
      }
    }


    /// <summary>
    ///   Called when this module is destroyed.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
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
        DebugLog(m: e);
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
            onHover: null,
            onHoverOut: null,
            onEnable: null,
            onDisable: null,
            visibleInScenes: ApplicationLauncher.AppScenes.ALWAYS,
            texture: _appIcon
          );
      }
      catch (Exception e)
      {
        DebugLog(m: e);
      }
    }

    /// <summary>
    ///   Called when [application launcher false].
    /// </summary>
    private void OnAppLauncherFalse()
    {
      DebugLog(m: "OnAppLauncherFalse()");
      if (_dialog == null) return;
      var rt = _dialog.GetComponent<RectTransform>().position;
      Settings.Instance.PosX = rt.x / GameSettings.UI_SCALE / Screen.width  + 0.5f;
      Settings.Instance.PosY = rt.y / GameSettings.UI_SCALE / Screen.height + 0.5f;

      _dialog.Dismiss();
      _dialog = null;
      _button.SetFalse(makeCall: false); // this is needed in case the "close" button was clicked instead of the app button.
    }

    /// <summary>
    ///   Called when [application launcher true].
    /// </summary>
    private void OnAppLauncherTrue()
    {
      const float q = 0.5f;
      const float w = 500f;
      const float h = 28f;
      const float v = 14f;

      DebugLog(m: "OnAppLauncherTrue()");

      OnAppLauncherFalse(); // make sure it's closed

      var components = new List<DialogGUIBase> {new DialogGUISpace(v: v)};
      var btm        = new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace());


      if (!GameSettings.ADVANCED_TWEAKABLES)
      {
        components.Add(
            item: new DialogGUIHorizontalLayout(new DialogGUILabel(message: "'Advanced Tweakables' needs to be enabled.\nClick the button below to enable it."))
          );

        components.Add(
            item: new DialogGUIHorizontalLayout(
                new DialogGUIButton(
                    optionText: "Enable 'Advanced Tweakables'",
                    onSelected: () =>
                    {
                      GameSettings.ADVANCED_TWEAKABLES = true;
                      GameSettings.SaveSettings();
                      OnAppLauncherFalse();
                      OnAppLauncherTrue();
                    },
                    w: -1f,
                    h: h,
                    dismissOnSelect: false
                  ),
                new DialogGUISpace(v: -1f)
              )
          );
        goto SPAWN; // dirty kludge, I know...
      }

      var top = new DialogGUIHorizontalLayout(
          new DialogGUISpace(v: v),
          new DialogGUIToggleButton(
              set: Settings.Instance.ModEnabled,
              lbel: "Enable Full AutoStrut",
              selected: b =>
              {
                Settings.Instance.ModEnabled = b;
                OnAppLauncherFalse();
                OnAppLauncherTrue();
              },
              w: -1f,
              h: h
            )
        );

      components.Add(item: top);

      if (!Settings.Instance.ModEnabled)
      {
        components.Add(
            item: new DialogGUIHorizontalLayout(
                new DialogGUIFlexibleSpace(),
                new DialogGUILabel(message: "Full AutoStrut is currently disabled."),
                new DialogGUIFlexibleSpace()
              )
          );
        components.Add(item: new DialogGUISpace(v: v));
        top.AddChild(child: new DialogGUISpace(v: v));
        goto SPAWN; // dirty kludge, I know...
      }

      top.AddChild(
          child: new DialogGUIToggleButton(
              set: Settings.Instance.RigidAttachment,
              lbel: "Use Rigid Attachment",
              selected: b => Settings.Instance.RigidAttachment = b,
              w: -1f,
              h: h
            )
        );

      _applyChildren = new DialogGUIToggleButton(
          set: Settings.Instance.ApplyChildren,
          lbel: "Re-Apply to child parts",
          selected: b => Settings.Instance.ApplyChildren = b,
          w: -1f,
          h: h
        );

      top.AddChild(child: _applyChildren);
      top.AddChild(child: new DialogGUISpace(v: v));

      components.Add(item: new DialogGUISpace(v: v));
      components.Add(
          item: new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace(), new DialogGUIBox(message: "AutoStrut", w: w, h: h), new DialogGUIFlexibleSpace())
        );

      components.Add(
          item: new DialogGUIHorizontalLayout(
              new DialogGUISpace(v: v),
              new DialogGUIToggleGroup(
                  new DialogGUIToggleButton(
                      set: Settings.Instance.AutoSelect == false && Settings.Instance.AutoStrutMode == Part.AutoStrutMode.Off,
                      lbel: "Off",
                      selected: b =>
                      {
                        if (!b) return;
                        Settings.Instance.AutoSelect    = false;
                        Settings.Instance.AutoStrutMode = Part.AutoStrutMode.Off;
                      },
                      w: -1f,
                      h: h
                    ),
                  new DialogGUIToggleButton(
                      set: Settings.Instance.AutoSelect == false && Settings.Instance.AutoStrutMode == Part.AutoStrutMode.Root,
                      lbel: "Root",
                      selected: b =>
                      {
                        if (!b) return;
                        Settings.Instance.AutoSelect    = false;
                        Settings.Instance.AutoStrutMode = Part.AutoStrutMode.Root;
                      },
                      w: -1f,
                      h: h
                    ),
                  new DialogGUIToggleButton(
                      set: Settings.Instance.AutoSelect == false && Settings.Instance.AutoStrutMode == Part.AutoStrutMode.Heaviest,
                      lbel: "Heaviest",
                      selected: b =>
                      {
                        if (!b) return;
                        Settings.Instance.AutoSelect    = false;
                        Settings.Instance.AutoStrutMode = Part.AutoStrutMode.Heaviest;
                      },
                      w: -1f,
                      h: h
                    ),
                  new DialogGUIToggleButton(
                      set: Settings.Instance.AutoSelect == false && Settings.Instance.AutoStrutMode == Part.AutoStrutMode.Grandparent,
                      lbel: "Grandparent",
                      selected: b =>
                      {
                        if (!b) return;
                        Settings.Instance.AutoSelect    = false;
                        Settings.Instance.AutoStrutMode = Part.AutoStrutMode.Grandparent;
                      },
                      w: -1f,
                      h: h
                    ),
                  new DialogGUIToggleButton(set: Settings.Instance.AutoSelect, lbel: "Automatic", selected: b => Settings.Instance.AutoSelect = b, w: -1f, h: h)
                ),
              new DialogGUISpace(v: v)
            )
        );

      components.Add(item: new DialogGUISpace(v: v));

      components.Add(item: new DialogGUIHorizontalLayout(new DialogGUIBox(message: "Same Vessel Interaction", w: w, h: h)));
      components.Add(
          item: new DialogGUIHorizontalLayout(
              new DialogGUISpace(v: v),
              new DialogGUIToggleGroup(
                  new DialogGUIToggleButton(
                      set: Settings.Instance.AutomaticSameVesselInteraction == false && Settings.Instance.SameVesselInteraction == false,
                      lbel: "Off",
                      selected: b =>
                      {
                        if (!b) return;
                        Settings.Instance.AutomaticSameVesselInteraction = false;
                        Settings.Instance.SameVesselInteraction          = false;
                      },
                      w: -1F,
                      h: h
                    ),
                  new DialogGUIToggleButton(
                      set: Settings.Instance.AutomaticSameVesselInteraction == false && Settings.Instance.SameVesselInteraction,
                      lbel: "On",
                      selected: b =>
                      {
                        if (!b) return;
                        Settings.Instance.AutomaticSameVesselInteraction = false;
                        Settings.Instance.SameVesselInteraction          = true;
                      },
                      w: -1F,
                      h: h
                    ),
                  new DialogGUIToggleButton(
                      set: Settings.Instance.AutomaticSameVesselInteraction,
                      lbel: "Automatic",
                      selected: b =>
                      {
                        Settings.Instance.AutomaticSameVesselInteraction = b;
                        Settings.Instance.SameVesselInteraction          = false;
                      },
                      w: -1F,
                      h: h
                    )
                ),
              new DialogGUISpace(v: v)
            )
        );


      components.Add(item: new DialogGUISpace(v: v));


      btm.AddChild(
          child: new DialogGUIButton(
              optionText: "Apply to all",
              onSelected: () => SetPartOptions(
                  p: EditorLogic.RootPart,
                  autoStrutMode: Settings.Instance.AutoStrutMode,
                  isRoboticHierarchy: EditorLogic.RootPart.IsRoboticCompatible(),
                  applyChildren: true
                ),
              EnabledCondition: null,
              w: -1f,
              h: h,
              dismissOnSelect: false
            )
        );

      btm.AddChild(child: new DialogGUISpace(v: v));
      btm.AddChild(
          child: new DialogGUIButton(
              optionText: "Reset Settings",
              onSelected: () =>
              {
                Settings.Instance.Reset();
                OnAppLauncherFalse();
                OnAppLauncherTrue();
              },
              EnabledCondition: null,
              w: -1f,
              h: h,
              dismissOnSelect: false
            )
        );
      btm.AddChild(child: new DialogGUISpace(v: v));

      SPAWN: // dirty kludge, I know...
      btm.AddChild(
          child: new DialogGUIButton(optionText: "Close", onSelected: OnAppLauncherFalse, EnabledCondition: null, w: -1f, h: h, dismissOnSelect: false)
        );
      btm.AddChild(child: new DialogGUIFlexibleSpace());

      components.Add(item: btm);
      components.Add(item: new DialogGUISpace(v: v));

      _dialog = PopupDialog.SpawnPopupDialog(
          anchorMin: new Vector2(x: q, y: q),
          anchorMax: new Vector2(x: q, y: q),
          dialog: new MultiOptionDialog(
              name: "FULL~AUTO~STRUT",
              msg: string.Empty,
              windowTitle: "Full AutoStrut v" + AssemblyVersion,
              skin: HighLogic.UISkin,
              rct: new Rect(x: Settings.Instance.PosX * GameSettings.UI_SCALE, y: Settings.Instance.PosY * GameSettings.UI_SCALE, width: w, height: -1f),
              options: new DialogGUIVerticalLayout(list: components.ToArray())
            ),
          persistAcrossScenes: false,
          skin: HighLogic.UISkin,
          isModal: false
        );
    }

    /// <summary>
    ///   Called when [GUI application launcher unreadifying].
    /// </summary>
    /// <param name="data">The data.</param>
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
        DebugLog(m: e);
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
          DebugLog(m: "OnPartDetached");
          ResetPartOptions(p: p, applyChildren: true, applySymmetry: true);
          return;
        }

        // if the event isn't detach or attach, stop here.
        if (ct != ConstructionEventType.PartAttached) return;
        DebugLog(m: "OnPartAttached");

        // check for mod enabled
        if (!Settings.Instance.ModEnabled) return;

        /* TODO:
        // automatically toggle off "Re-Apply to Children" after re-placing parent part
        if (Settings.Instance.ApplyChildren)
        {
          DebugLog("HERE");
          Settings.Instance.ApplyChildren = false;
          _applyChildren.toggle.group.SetAllTogglesOff();
        }
        */

        // check for null
        if (p == null)
        {
          DebugLog(m: "Part is null");
          return;
        }

        // check if AutoStruts are allowed
        if (!p.AllowAutoStruts())
        {
          DebugLog(m: "AutoStruts is disallowed");
          return;
        }

        // check for robotics
        var isRobotic = false;
        for (var p2 = p; p2 != null && (isRobotic = p2.IsRoboticCompatible()) == false; p2 = p2.parent) { }

        // some parts have their AutoStrut already set by default, do not override this if not placed on robotics.
        if (!isRobotic
         && p.autoStrutMode != Part.AutoStrutMode.Off)
        {
          DebugLog(m: "AutoStrut is already set");
          return;
        }

        SetPartOptions(p: p, autoStrutMode: Settings.Instance.AutoStrutMode, isRoboticHierarchy: isRobotic, applyChildren: true);
      }
      catch (Exception e)
      {
        DebugLog(m: e);
      }
    }

    /// <summary>
    ///   Sets the part options.
    /// </summary>
    /// <param name="p">The p.</param>
    /// <param name="autoStrutMode">The automatic strut mode.</param>
    /// <param name="isRoboticHierarchy">if set to <c>true</c> [is robotic hierarchy].</param>
    /// <param name="applyChildren">if set to <c>true</c> [apply to all children].</param>
    private void SetPartOptions(Part p, Part.AutoStrutMode autoStrutMode, bool isRoboticHierarchy, bool applyChildren)
    {
      if (p == null) return;

      if (isRoboticHierarchy)
        autoStrutMode = Part.AutoStrutMode.Off;
      else if (Settings.Instance.AutoSelect)
        autoStrutMode = p.parent == null
                          ? Part.AutoStrutMode.Off
                          : p.parent.parent == null
                            ? Part.AutoStrutMode.Root
                            : Part.AutoStrutMode.Grandparent;

      var sameVesselInteraction = Settings.Instance.SameVesselInteraction | (isRoboticHierarchy && Settings.Instance.AutomaticSameVesselInteraction);

      ApplyPartOptions(p: p, autoStrutMode: autoStrutMode, sameVesselInteraction: sameVesselInteraction, applySymmetry: true);

      // ReSharper disable once InvertIf
      if (applyChildren)
        foreach (var child in p.children)
          SetPartOptions(p: child, autoStrutMode: autoStrutMode, isRoboticHierarchy: isRoboticHierarchy, applyChildren: true);
    }

    /// <summary>
    ///   Applies the part options.
    /// </summary>
    /// <param name="p">The p.</param>
    /// <param name="autoStrutMode">The automatic strut mode.</param>
    /// <param name="sameVesselInteraction">if set to <c>true</c> [same vessel interaction].</param>
    /// <param name="applySymmetry">if set to <c>true</c> [apply symmetry].</param>
    private void ApplyPartOptions(Part p, Part.AutoStrutMode autoStrutMode, bool sameVesselInteraction, bool applySymmetry)
    {
      p.autoStrutMode = autoStrutMode;
      p.UpdateAutoStrut();
      p.rigidAttachment = Settings.Instance.RigidAttachment;
      p.ApplyRigidAttachment();
      p.SetSameVesselCollision(value: sameVesselInteraction);

      // ReSharper disable once InvertIf
      if (applySymmetry)
        foreach (var symmetryCounterpart in p.symmetryCounterparts)
          ApplyPartOptions(p: symmetryCounterpart, autoStrutMode: autoStrutMode, sameVesselInteraction: sameVesselInteraction, applySymmetry: false);
    }


    /// <summary>
    ///   Resets the part options.
    /// </summary>
    /// <param name="p">The p.</param>
    /// <param name="applyChildren">if set to <c>true</c> [apply children].</param>
    /// <param name="applySymmetry">if set to <c>true</c> [apply symmetry].</param>
    private void ResetPartOptions(Part p, bool applyChildren, bool applySymmetry)
    {
      if (p == null) return;
      var autoStrutMode = p.partInfo.partPrefab.autoStrutMode;
      DebugLog(m: $"Reverting AutoStrut to: {autoStrutMode}");

      p.autoStrutMode = autoStrutMode;
      p.UpdateAutoStrut();
      p.rigidAttachment = p.partInfo.partPrefab.rigidAttachment;
      p.ApplyRigidAttachment();
      p.SetSameVesselCollision(value: p.partInfo.partPrefab.sameVesselCollision);

      if (applySymmetry)
        foreach (var counterpart in p.symmetryCounterparts)
          ResetPartOptions(p: counterpart, applyChildren: applyChildren, applySymmetry: false);

      // ReSharper disable once InvertIf
      if (applyChildren)
        foreach (var child in p.children)
          ResetPartOptions(p: child, applyChildren: true, applySymmetry: true);
    }
  }
}
