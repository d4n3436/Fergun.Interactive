using System;
using Discord;

namespace ExampleBot
{
    public static class Utils
    {
        public static Color GetRandomColor()
        {
            var rng = new Random();
            return new Color(rng.Next(0, 256), rng.Next(0, 256), rng.Next(0, 256));
        }
    }
}