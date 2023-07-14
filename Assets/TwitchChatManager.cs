using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TwitchChatManager : MonoBehaviour
{
    private Twitch client;

    [SerializeField] private List<UnityEvent> outcomes;

    [SerializeField]
    private ulong[] counters;

    [SerializeField]
    [Min(0)]
    private float timeLimit;

    private Coroutine routine;

    void Start()
    {
        client = new Twitch();
        client.OnChat = OnChat;
    }

    private void OnDisable()
    {
        client.ExplicitlyDestroy();
    }
    
    private void OnChat(string msg)
    {
        try
        {
            var num = Convert.ToInt32(msg);
            if (num <= 0 || num > counters.Length) return;

            counters[num - 1]++;
        }
        catch { }
    }

    public void JoinChannel(string name)
    {
        client.JoinChannel(name);
        if (routine != null) return;
        routine = StartCoroutine(timer(timeLimit));
    }

    private void Reset()
    {
        for (var i = 0; i < counters.Length; i++)
        {
            counters[i] = 0;
        }
    }

    private IEnumerator timer(float limit)
    {
        yield return new WaitForSeconds(limit);

        var max = counters.Max();
        
        if (max == 0) goto skip;

        var index = Array.IndexOf(counters, max);

        if (index > outcomes.Count)
        {
            Debug.LogError("Lengths are mismatched");
            yield break;
        }
        
        outcomes[index]?.Invoke();
        
        skip:
        
        Reset();
        StartCoroutine(timer(limit));
    }

    public void DebugFunction1()
    {
        Debug.Log("Received! 1");
    }
    
    public void DebugFunction2()
    {
        Debug.Log("Received! 2");
    }
    
    public void DebugFunction3()
    {
        Debug.Log("Received! 3");
    }
}
