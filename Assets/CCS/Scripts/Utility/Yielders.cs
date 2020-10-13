using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

class Yielders
{
    public static bool Enabled = true;
    public static int _internalCounter = 0; // counts how many times the app yields
    static WaitForEndOfFrame _endOfFrame = new WaitForEndOfFrame();
    public static WaitForEndOfFrame EndOfFrame
    {
        get { _internalCounter++; return Enabled ? _endOfFrame : new WaitForEndOfFrame(); }
    }

    static WaitForFixedUpdate _fixedUpdate = new WaitForFixedUpdate();
    public static WaitForFixedUpdate FixedUpdate
    {
        get { _internalCounter++; return Enabled ? _fixedUpdate : new WaitForFixedUpdate(); }
    }

    public static WaitForSeconds GetWaitForSeconds(float seconds)
    {
        _internalCounter++;

        if (!Enabled)
            return new WaitForSeconds(seconds);

        WaitForSeconds wfs;
        if (!_waitForSecondsYielders.TryGetValue(seconds, out wfs))
            _waitForSecondsYielders.Add(seconds, wfs = new WaitForSeconds(seconds));
        return wfs;
    }

    public static void ClearWaitForSeconds()
    {
        _waitForSecondsYielders.Clear();
    }

    static Dictionary<float, WaitForSeconds> _waitForSecondsYielders = new Dictionary<float, WaitForSeconds>(100, new FloatComparer());
}
