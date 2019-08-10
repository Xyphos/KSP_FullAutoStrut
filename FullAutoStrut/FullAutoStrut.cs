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

namespace FullAutoStrut
{
  [KSPAddon(startup: KSPAddon.Startup.EditorAny, once: false)]
  public class FullAutoStrut : MonoBehaviour
  {
    private Texture                   _appIcon;
    private bool                      _applyChildren;
    private bool                      _autoSelect = true;
    private Part.AutoStrutMode        _autoStrutMode;
    private ApplicationLauncherButton _button;
    private PopupDialog               _dialog;
    private bool                      _modEnabled = true;
    private bool                      _rigidAttachment;


    [Conditional(conditionString: "DEBUG")] private static void DebugLog(string m) => print(message: "[FAS] " + m);

    [Conditional(conditionString: "DEBUG")] private static void DebugLog(Exception e) => print(message: "[FAS] " + e);


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

        var iconPath = Regex.Replace(
            input: Assembly.GetExecutingAssembly().CodeBase,
            pattern: "^.+GameData/(.+)FullAutoStrut\\.dll$",
            replacement: @"$1fas",
            options: RegexOptions.IgnoreCase
          );

        DebugLog(m: $"iconPath: {iconPath}");

        if (_appIcon == null) _appIcon = GameDatabase.Instance.GetTexture(url: iconPath, asNormalMap: false);

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
        DebugLog(e: e);
      }
    }

    /// <summary>
    ///   Called when [application launcher false].
    /// </summary>
    private void OnAppLauncherFalse()
    {
      if (_dialog == null) return;
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
      const float h = 18F;
      OnAppLauncherFalse(); // make sure it's closed

      var components = new List<DialogGUIBase> {new DialogGUIFlexibleSpace()};

      if (!GameSettings.ADVANCED_TWEAKABLES)
      {
        components.Add(item: new DialogGUILabel(message: "'Advanced Tweakables' needs to be enabled.\nClick the button below to enable it."));
        components.Add(
            item: new DialogGUIHorizontalLayout(
                new DialogGUISpace(v: -1f),
                new DialogGUIButton(
                    optionText: "Enable 'Advanced Tweakables'",
                    onSelected: () =>
                    {
                      GameSettings.ADVANCED_TWEAKABLES = true;
                      GameSettings.SaveSettings();
                      OnAppLauncherFalse();
                      OnAppLauncherTrue();
                    },
                    w: 250f,
                    h: -1f,
                    dismissOnSelect: false
                  ),
                new DialogGUISpace(v: -1f)
              )
          );
        goto SPAWN; // dirty kludge, I know...
      }

      components.Add(
          item: new DialogGUIToggle(
              set: _modEnabled,
              lbel: "Enable FAS",
              selected: b =>
              {
                _modEnabled = b;
                OnAppLauncherFalse();
                OnAppLauncherTrue();
              },
              w: -1f,
              h: h
            )
        );
      if (!_modEnabled)
      {
        components.Add(item: new DialogGUILabel(message: "Full AutoStrut is currently disabled."));
        goto SPAWN; // dirty kludge, I know...
      }

      components.Add(item: new DialogGUIToggle(set: _rigidAttachment, lbel: "Use Rigid Attachment", selected: b => _rigidAttachment = b, w: -1f, h: h));

      components.Add(item: new DialogGUIFlexibleSpace());

      components.Add(
          item: new DialogGUIToggleGroup(
              new DialogGUIToggleButton(
                  set: _autoSelect == false && _autoStrutMode == Part.AutoStrutMode.Off,
                  lbel: "Off",
                  selected: b =>
                  {
                    if (!b) return;
                    _autoSelect    = false;
                    _autoStrutMode = Part.AutoStrutMode.Off;
                  },
                  w: -1f,
                  h: h
                ),
              new DialogGUIToggleButton(
                  set: _autoSelect == false && _autoStrutMode == Part.AutoStrutMode.Root,
                  lbel: "Root",
                  selected: b =>
                  {
                    if (!b) return;
                    _autoSelect    = false;
                    _autoStrutMode = Part.AutoStrutMode.Root;
                  },
                  w: -1f,
                  h: h
                ),
              new DialogGUIToggleButton(
                  set: _autoSelect == false && _autoStrutMode == Part.AutoStrutMode.Heaviest,
                  lbel: "Heaviest",
                  selected: b =>
                  {
                    if (!b) return;
                    _autoSelect    = false;
                    _autoStrutMode = Part.AutoStrutMode.Heaviest;
                  },
                  w: -1f,
                  h: h
                ),
              new DialogGUIToggleButton(
                  set: _autoSelect == false && _autoStrutMode == Part.AutoStrutMode.Grandparent,
                  lbel: "Grandparent",
                  selected: b =>
                  {
                    if (!b) return;
                    _autoSelect    = false;
                    _autoStrutMode = Part.AutoStrutMode.Grandparent;
                  },
                  w: -1f,
                  h: h
                ),
              new DialogGUIToggleButton(set: _autoSelect, lbel: "Automatic", selected: b => _autoSelect = b, w: -1f, h: h)
            )
        );
      components.Add(item: new DialogGUIToggle(set: _applyChildren, lbel: "Re-Apply to child parts", selected: b => _applyChildren = b, w: -1f, h: h));

      //components.Add(item: new DialogGUIFlexibleSpace());
      components.Add(
          item: new DialogGUIButton(
              optionText: "Apply to all",
              onSelected: () => SetAutoStrut(p: EditorLogic.RootPart, asm: _autoStrutMode, applyChildren: true),
              EnabledCondition: null,
              w: -1f,
              h: -1f,
              dismissOnSelect: false
            )
        );

      SPAWN: // dirty kludge, I know...
      components.Add(item: new DialogGUISpace(v: h));
      components.Add(
          item: new DialogGUIHorizontalLayout(
              new DialogGUISpace(v: -1f),
              new DialogGUIButton(optionText: "Close", onSelected: OnAppLauncherFalse, EnabledCondition: null, w: -1f, h: -1f, dismissOnSelect: false),
              new DialogGUISpace(v: -1f)
            )
        );

      _dialog = PopupDialog.SpawnPopupDialog(
          anchorMin: new Vector2(x: q, y: q),
          anchorMax: new Vector2(x: q, y: q),
          dialog: new MultiOptionDialog(
              name: "FULL~AUTO~STRUT",
              msg: string.Empty,
              windowTitle: "Full AutoStrut",
              skin: HighLogic.UISkin,
              rct: new Rect(x: 0.5f, y: 0.5f, width: 200f, height: -1f),
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
          DebugLog(m: "OnPartAttached (removed)");
          // revert part to default
          // ReSharper disable once SwitchStatementMissingSomeCases
          switch (p.autoStrutMode)
          {
            case Part.AutoStrutMode.Grandparent:
            case Part.AutoStrutMode.Heaviest:
            case Part.AutoStrutMode.Root:
              SetAutoStrut(p: p, asm: Part.AutoStrutMode.Off, applyChildren: true);
              break;
          }

          return;
        }

        // if the event isn't detach or attach, stop here.
        if (ct != ConstructionEventType.PartAttached) return;

        DebugLog(m: "OnPartAttached");

        // check for mod enabled
        if (!_modEnabled) return;

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
          if (p2.Modules.ContainsRegex(pattern: "^ModuleRobotic(?:Rotation)?Servo|^MuMechToggle"))
          {
            DebugLog(m: "Placed part is a robotic attachment, ignoring.");
            return;
          }

          p2 = p2.parent; // next itteration
        }

        SetAutoStrut(p: p, asm: _autoStrutMode, applyChildren: _applyChildren);
      }
      catch (Exception e)
      {
        DebugLog(e: e);
      }
    }

    /// <summary>
    ///   Sets the automatic strut.
    /// </summary>
    /// <param name="p">The p.</param>
    /// <param name="asm">The asm.</param>
    /// <param name="applyChildren">if set to <c>true</c> [apply children].</param>
    private void SetAutoStrut(Part p, Part.AutoStrutMode asm, bool applyChildren)
    {
      if (p == null) return;

      // automatic override
      if (_autoSelect)
        asm = p.parent == null
                ? Part.AutoStrutMode.Off
                : p.parent.parent == null
                  ? Part.AutoStrutMode.Root
                  : Part.AutoStrutMode.Grandparent;

      DebugLog(m: $"Setting AutoStrut to: {asm}");
      p.autoStrutMode   = asm;
      p.rigidAttachment = _rigidAttachment;
      p.ApplyRigidAttachment();
      p.UpdateAutoStrut();

      foreach (var pcp in p.symmetryCounterparts)
      {
        if (pcp == null) continue;
        pcp.autoStrutMode   = asm;
        pcp.rigidAttachment = _rigidAttachment;
        pcp.ApplyRigidAttachment();
        pcp.UpdateAutoStrut();
      }

      // ReSharper disable once InvertIf
      if (applyChildren)
        foreach (var child in p.children)
          if (child != null)
            SetAutoStrut(p: child, asm: asm, applyChildren: true);
    }
  }
}
