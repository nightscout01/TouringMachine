// Copyright Maurice Montag 2020
// All Rights Reserved
// See LICENSE file for more information

using System;
using System.IO;

namespace TouringMachine
{
    class Helper
    {
        public static void Main()
        {
            byte[] progROM = File.ReadAllBytes("Adding.bin");
            Machine m = new Machine(progROM);  // program starts executing at index 0
            while (true)
            {
                Console.WriteLine("Current Machine state is: " + m.MachineState);
                Console.ReadLine();  // for single stepping through the assembly
                m.Step();
            }
        }
    }
}
