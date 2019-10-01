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
using System.Reflection;
using System.Text.RegularExpressions;

namespace XyphosAerospace
{
  public static class Extensions
  {
    private static void DebugLog(this Exception e) => FullAutoStrut.DebugLog(m: e);
    private static void DebugLog(object         m) => FullAutoStrut.DebugLog(m: m);


    public static bool ContainsRegex(
        this PartModuleList partModuleList,
        string              pattern,
        RegexOptions        regexOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
      )
    {
      foreach (var partModule in partModuleList)
        if (Regex.IsMatch(input: partModule.moduleName, pattern: pattern, options: regexOptions))
          return true;

      return false;
    }


    /// <summary>
    /// Determines whether this instance is robotic.
    /// </summary>
    /// <param name="p">The p.</param>
    /// <returns>
    ///   <c>true</c> if the specified p is robotic; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>This method is compatible with Infernal Robotics instead of just Stock Robotics.</remarks>
    public static bool IsRoboticCompatible(this Part p) => p.Modules.ContainsRegex(pattern: "^ModuleRobotic(?:Rotation)?Servo|^MuMechToggle");

    public static void DrawAutoStrutLine(this Part p)
      => p.GetType().GetMethod(name: "DrawAutoStrutLine", bindingAttr: BindingFlags.Instance | BindingFlags.IgnoreCase)?.Invoke(obj: p, parameters: null);

    public static void SetSameVesselCollision(this Part p, bool value) => p.Fields[fieldName: "sameVesselCollision"].SetValue(newValue: value, host: p);
  }
}
