// Copyright Maurice Montag 2020
// All Rights Reserved
// See LICENSE file for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Assembler
{
    class Assembler
    {
        public static void main(string[] args)
        {
            //if(args.Length == 0)
            //{
            //    Console.WriteLine("Please input a file name");
            //    return;
            //}
            //IEnumerable<string> fileLines = File.ReadLines(args[0]);
            Console.WriteLine("Input Filename of assembly");
            string fileName = Console.ReadLine();
            IEnumerable<string> fileLines = File.ReadLines(fileName);
            // we'll treat any lines that contain "//" as comments, so we use linq to remove them
            IEnumerable<string> filtered = fileLines.Where(line => !line.StartsWith("//"));
            // we don't support labels, because we're mean
            List<byte> programBytes = new List<byte>();  // create a List of bytes to hold our opcodes
            foreach(string line in filtered)
            {
                byte[] temp = GetOpcodeFromAsm(line);
                foreach(byte b in temp)  // for multibyte opcodes
                {
                    programBytes.Add(b);
                }
            }
            File.WriteAllBytes(Path.GetFileNameWithoutExtension(fileName) + ".bin", programBytes.ToArray());
            // write bytes to bin file
        }

        private static byte[] GetOpcodeFromAsm(string asmLine)
        {
            string asmCode = Regex.Replace(asmLine, "//" + ".+", string.Empty).Trim();
            byte secondArgument;  // for when we need relative jumps
            byte thirdArgument; // for when we need absolute addressing
            switch (asmCode.Substring(0,3))  // we'll do something special for the ones that have relative/immediate values
            {
                
                case "NDT":
                    return new byte[]{ 0x1 };
                case "NOP":
                    return new byte[]{ 0x2 };
                case "ERS":
                    
                    if (asmCode.Substring(4, 1) == "#")
                    {
                        // relative
                        secondArgument = (byte)sbyte.Parse(asmCode.Substring(5));
                        return new byte[] { 0x3, secondArgument };
                    }
                    if (asmCode.Substring(4, 1) == "@")
                    {
                        // absolute
                        secondArgument = byte.Parse(asmCode.Substring(5,2));
                        thirdArgument = byte.Parse(asmCode.Substring(7,2));
                        return new byte[] { 0x4, thirdArgument, secondArgument};  // encode in little endian
                    }
                    else
                    {
                        throw new ArgumentException("Probably didn't get the substring right");
                    }
                case "JMP":
                   
                    if (asmCode.Substring(4, 1) == "#")
                    {
                        // relative
                        secondArgument = (byte)sbyte.Parse(asmCode.Substring(5));
                        return new byte[] { 0x5, secondArgument };
                    }
                    if (asmCode.Substring(4, 1) == "@")
                    {
                        // absolute
                        secondArgument = byte.Parse(asmCode.Substring(5, 2));
                        thirdArgument = byte.Parse(asmCode.Substring(7, 2));
                        return new byte[] { 0x6, thirdArgument, secondArgument };  // encode in little endian
                    }
                    else
                    {
                        throw new ArgumentException("Probably didn't get the substring right");
                    }
                case "LOD":
                    
                    // just as well
                    if (asmCode.Substring(4,1) == "$")
                    {
                        // immediate
                        secondArgument = (byte)sbyte.Parse(asmCode.Substring(5));
                        return new byte[] { 0x7, secondArgument };
                    } 
                    if(asmCode.Substring(4,1) == "#")
                    {
                        // relative
                        secondArgument = (byte)sbyte.Parse(asmCode.Substring(5));
                        return new byte[] { 0x8, secondArgument };
                    }
                    if (asmCode.Substring(4, 1) == "@")
                    {
                        // absolute
                        secondArgument = byte.Parse(asmCode.Substring(5, 2));
                        thirdArgument = byte.Parse(asmCode.Substring(7, 2));
                        return new byte[] { 0x9, thirdArgument, secondArgument };
                    }
                    else
                    {
                        throw new ArgumentException("Probably didn't get the substring right");
                    }
                case "STR":
                    if (asmCode.Substring(4, 1) == "#")
                    {
                        // relative
                        secondArgument = (byte)sbyte.Parse(asmCode.Substring(5));
                        return new byte[] { 0xA, secondArgument };
                    }
                    if (asmCode.Substring(4, 1) == "@")
                    {
                        // absolute
                        secondArgument = byte.Parse(asmCode.Substring(5, 2));
                        thirdArgument = byte.Parse(asmCode.Substring(7, 2));
                        return new byte[] { 0xB, thirdArgument, secondArgument };
                    }
                    else
                    {
                        throw new ArgumentException("Probably didn't get the substring right");
                    }
                case "INC":
                    return new byte[] { 0xC };
                case "DEC":
                    return new byte[] { 0xD };
                case "CMP":
                    secondArgument = (byte)sbyte.Parse(asmCode.Substring(5));
                    if (asmCode.Substring(4, 1) == "$")
                    {
                        // immediate
                        return new byte[] { 0xE, secondArgument };
                    }
                    if (asmCode.Substring(4, 1) == "#")
                    {
                        // relative
                        return new byte[] { 0xF, secondArgument };
                    }
                    if (asmCode.Substring(4, 1) == "@")
                    {
                        // absolute
                        secondArgument = byte.Parse(asmCode.Substring(5, 2));
                        thirdArgument = byte.Parse(asmCode.Substring(7, 2));
                        return new byte[] { 0x10, thirdArgument, secondArgument };
                    }
                    else
                    {
                        throw new ArgumentException("Probably didn't get the substring right");
                    }
                case "BEQ":
                    secondArgument = (byte) sbyte.Parse(asmCode.Substring(5));
                    return new byte[] { 0x11, secondArgument};
                case "BNE":
                    secondArgument = (byte) sbyte.Parse(asmCode.Substring(5));
                    return new byte[] { 0x12, secondArgument};
                case "DEI":
                    if (asmCode.Substring(4, 1) == "#")
                    {
                        // relative
                        secondArgument = byte.Parse(asmCode.Substring(5));
                        return new byte[] { 0xFD, secondArgument };
                    }
                    if (asmCode.Substring(4, 1) == "@")
                    {
                        // absolute
                        secondArgument = byte.Parse(asmCode.Substring(5, 2));
                        thirdArgument = byte.Parse(asmCode.Substring(7, 2));
                        return new byte[] { 0xFE, thirdArgument, secondArgument };
                    }
                    else
                    {
                        throw new ArgumentException("Probably didn't get the substring right");
                    }
                case "HAL":
                    return new byte[] { 0xFF };
                default:
                    throw new ArgumentException("Invalid Assembly code " + asmCode);
            }
        }
    }
}
