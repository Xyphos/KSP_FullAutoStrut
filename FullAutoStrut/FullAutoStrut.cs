/*
 MIT License
 
 Copyright (c) 2017 William "Xyphos" Scott
 
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in all
 copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
 */

using System;
using System.Diagnostics;
using UnityEngine;

namespace FullAutoStrut
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class FullAutoStrut : MonoBehaviour
    {
        [Conditional("DEBUG")]
        private void Log(string m)
        {
            print(m);
        }


        /// <summary>
        ///     Called when this module is activated.
        /// </summary>
        public void Awake()
        {
            try
            {
                Log("[FAS] Awake()");
                GameEvents.onEditorPartEvent.Add(OnPartEvent);
            }
            catch (Exception e)
            {
                print("[FAS] " + e);
            }
        }

        /// <summary>
        ///     Called when this module is deactivated.
        /// </summary>
        public void OnDisable()
        {
            try
            {
                //print("OnDisable()");
                GameEvents.onEditorPartEvent.Remove(OnPartEvent);
            }
            catch (Exception e)
            {
                print("[FAS] " + e);
            }
        }

        /// <summary>
        ///     Called when a part does something.
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
                    switch (p.autoStrutMode)
                    {
                        case Part.AutoStrutMode.Grandparent:
                        case Part.AutoStrutMode.Root:
                        case Part.AutoStrutMode.Heaviest:
                            SetAS(p, Part.AutoStrutMode.Off);
                            break;
                    }

                    return;
                }

                // if the event isn't detach or attach, stop here.
                if (ct != ConstructionEventType.PartAttached)
                    return;

                Log("[FAS] OnPartAttached");

                // check for null
                if (p == null)
                {
                    Log("[FAS] Part is null");
                    return;
                }

                // check if allowed
                if (!p.AllowAutoStruts())
                {
                    Log("[FAS] AutoStruts is disallowed");
                    return;
                }

                // some parts have their autostrut already set by default, do not override this.
                if (p.autoStrutMode != Part.AutoStrutMode.Off)
                {
                    Log("[FAS] AutoStrut is already set");
                    return;
                }

                // ignore parts with null parents
                if (p.parent == null)
                {
                    Log("[FAS] Placed part's parent is null, ignoring.");
                    return;
                } 



                // Added Infernal Robotics Comaptibility - Crawl the parts hierarchy to check for robotic parents
                var p2 = p;
                while (p2 != null)
                {
                    if (p2.Modules.Contains("MuMechToggle"))
                    {
                        Log("[FAS] Placed part is a robotic attachment, ignoring.");
                        return;
                    }

                    p2 = p2.parent; // next itteration
                }
                

                // if grandparent is null, set to root
                if (p.parent.parent == null)
                {
                    SetAS(p, Part.AutoStrutMode.Root);
                    return;
                } 

                SetAS(p, Part.AutoStrutMode.Grandparent);
            }
            catch (Exception e)
            {
                print("[FAS] " + e);
            }            
        }

        private void SetAS(Part p, Part.AutoStrutMode asm)
        {
            Log(string.Format("[FAS] Setting AutoStrut to: {0}", asm));
            p.autoStrutMode = asm;
            p.UpdateAutoStrut();           

            foreach (var pcp in p.symmetryCounterparts)
            {
                pcp.autoStrutMode = asm;
                pcp.UpdateAutoStrut();
            }
        }
    }
}
