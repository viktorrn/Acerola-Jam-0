using System;


public static class Utils
{
    public static float Lerp(float a, float b, float t)
    {
        t = Math.Clamp(t,0,1);
        return a*(1-t) + b*t;
    }

    public static float BinaryLerp(float a, float b, float t)
    {
        if( t < 0.9f )
        {
            return a;
        }

        return b;
    }

    
    public const string WorldPath = "Game/SubViewportContainer/SubViewport/world";
}
