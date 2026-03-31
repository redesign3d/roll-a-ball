using System;
using UnityEngine;

public class CountedPrefab : MonoBehaviour
{
    public static int ActiveCount { get; private set; }
    public static event Action<int> OnCountChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        ActiveCount = 0;
        OnCountChanged = null;
    }

    private void OnEnable()
    {
        ActiveCount++;
        OnCountChanged?.Invoke(ActiveCount);
    }

    private void OnDisable()
    {
        ActiveCount = Mathf.Max(0, ActiveCount - 1);
        OnCountChanged?.Invoke(ActiveCount);
    }
}