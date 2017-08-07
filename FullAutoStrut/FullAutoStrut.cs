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

using UnityEngine;

namespace FullAutoStrut
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class FullAutoStrut : MonoBehaviour
    {
        /// <summary>
        ///     Called when this module is activated.
        /// </summary>
        public void Awake()
        {
            //print("Start()");
            GameEvents.onEditorPartEvent.Add(OnPartEvent);
            //GameEvents.onEditorShipModified.Add(OnShipModified);
        }

        /// <summary>
        ///     Called when this module is deactivated.
        /// </summary>
        public void OnDisable()
        {
            //print("OnDisable()");
            GameEvents.onEditorPartEvent.Remove(OnPartEvent);
            //GameEvents.onEditorShipModified.Remove(OnShipModified);
        }

        /*
        /// <summary>
        ///     Called when [ship modified].
        /// </summary>
        /// <param name="sc">The sc.</param>
        private static void OnShipModified(ShipConstruct sc)
        {
            if (sc.parts.Count != 1)
                return;

            sc.parts[0].autoStrutMode = Part.AutoStrutMode.Heaviest;
            sc.parts[0].UpdateAutoStrut();
        }
        //*/

        /// <summary>
        ///     Called when a part is attached.
        /// </summary>
        /// <param name="ct">The ct.</param>
        /// <param name="p">The p.</param>
        private static void OnPartEvent(ConstructionEventType ct, Part p)
        {
            if (ct != ConstructionEventType.PartAttached)
                return;

            //print("OnPartAttached");

            if (p == null)
            {
                //print("Part is null");
                return;
            }

            if (!p.AllowAutoStruts())
            {
                //print("AutoStruts is disallowed");
                return;
            }

            if (p.autoStrutMode != Part.AutoStrutMode.Off)
            {
                //print("AutoStrut is already set");
                return;
            }

            if (p.parent == null)
            {
                //print("part's parent is null");
                return;
            }

            if (p.parent.parent == null)
            {
                //print("part's grandparent is null"); // root part?
                p.autoStrutMode = Part.AutoStrutMode.Root;
                p.UpdateAutoStrut();
                return;
            }

            //print("Defaulting AutoStrut to GrandParent");
            p.autoStrutMode = Part.AutoStrutMode.Grandparent;
            p.UpdateAutoStrut();
        }
    }
}
