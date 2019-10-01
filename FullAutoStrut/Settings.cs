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
using System.IO;
using UnityEngine;

namespace XyphosAerospace
{
  public class Settings : MonoBehaviour
  {
    private static readonly string SettingsFile = "GameData/" + FullAutoStrut.ModPath + "/FullAutoStrut.settings";

    [Persistent] public bool               ApplyChildren                  = false;
    [Persistent] public bool               AutomaticSameVesselInteraction = true;
    [Persistent] public bool               AutoSelect                     = true;
    [Persistent] public Part.AutoStrutMode AutoStrutMode                  = Part.AutoStrutMode.Off;
    [Persistent] public bool               ModEnabled                     = true;
    [Persistent] public float              PosX                           = 0.5f;
    [Persistent] public float              PosY                           = 0.5f;
    [Persistent] public bool               RigidAttachment                = false;
    [Persistent] public bool               SameVesselInteraction          = false;


    private Settings(bool load)
    {
      try
      {
        if (!load) return;
        if (!File.Exists(path: SettingsFile)) return;

        FullAutoStrut.DebugLog(m: $"Loading Settings: {SettingsFile}");
        ConfigNode.LoadObjectFromConfig(obj: this, node: ConfigNode.Load(fileFullName: SettingsFile));
      }
      catch (Exception e) { FullAutoStrut.DebugLog(m: e); }
    }

    public static Settings Instance { get; private set; } = new Settings(load: true);

    public void Reset() => Instance = new Settings(load: false);

    public static void Save() => ConfigNode.CreateConfigFromObject(obj: Instance, node: new ConfigNode(name: Instance.GetType().Name))
                                           .Save(fileFullName: SettingsFile);
  }
}
