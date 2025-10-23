using System;
using NetCord;

namespace ExampleBot;

public static class Utils
{
    public static Color GetRandomColor()
        => new((byte)Random.Shared.Next(0, 256), (byte)Random.Shared.Next(0, 256), (byte)Random.Shared.Next(0, 256));
}