using System;
using System.Collections;
using System.Collections.Generic;
using AOT;
using RawNative;

public unsafe class Twitch
{
    public static Twitch Instance;
    
    private void* runtime;

    public Action<string> OnChat;

    public Twitch()
    {
        if (Instance != null)
        {
            return;
        }
        
        runtime = RawTwitch.init_runtime(RawListener);
        Instance = this;
    }

    ~Twitch()
    {
        if (runtime == null) return;

        ExplicitlyDestroy();
    }
    
    [MonoPInvokeCallback(typeof(RawTwitch.init_runtime_callback_delegate))]
    private static void RawListener(byte* str)
    {
        var msg = new string((sbyte*)str);
        RawTwitch.free_string(str);

        Instance?.OnChat?.Invoke(msg);
    }

    public void JoinChannel(string channelName)
    {
        if (runtime == null)
        {
            throw new InvalidOperationException("Runtime has been destroyed");
        }
        
        var str = String.Copy(channelName);
        fixed (char* p = str)
        {
            RawTwitch.join_channel(runtime, (ushort*)p, str.Length);
        }
    }

    public void ExplicitlyDestroy()
    {
        if (runtime == null)
        {
            throw new InvalidOperationException("Runtime has already been destroyed");
        }
        RawTwitch.free_handle(runtime);
        runtime = null;
    }
}
