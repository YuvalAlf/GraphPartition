﻿using System.Drawing;

namespace Running
{
    class Program
    {
        static void Main(string[] args)
        {
            Colorful.Console.WriteAscii("Graph Partiton", Color.CornflowerBlue);
            Repl.Create().Run();
        }
    }
}
