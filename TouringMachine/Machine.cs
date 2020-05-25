// Copyright Maurice Montag 2020
// All Rights Reserved
// See LICENSE file for more information

using System;
using System.Collections.Generic;

namespace TouringMachine
{
    enum Direction 
    { 
        Left = 0,
        Right = 1
    }

    enum AddressingMode
    {
        Immediate = 0,
        Relative = 1,
        Absolute = 2
    }

    class Machine
    {
        private readonly List<byte> tape;  // in our turing machine the tape only extends out forever in the right direction
        private int index;  // apparently array indexing doesn't support unsigned ints for some reason, that sucks :(.
        public byte MachineState { get; protected set; }
        public bool running;
        private byte flags; // processor flags
            // currently there's just one flag, the LSB is "equal"
        private bool DEBUG = true;
        public Machine()
        {
            tape = new List<byte>(65536);
            index = 512;
            MachineState = 0;
            flags = 0;
            running = true;
        }

        public Machine(byte[] b)
        {
            tape = new List<byte>(new byte[512+b.Length]);  // required to fill everything with zeros by default
                // tbh an array would work just as well
            tape.InsertRange(512, b);
            for(int i = 512; i < tape.Count; i++)
            {
                Console.Write(tape[i].ToString() + " ");
            }
            Console.WriteLine();
            index = 512;
            MachineState = 0;
            flags = 0;
            running = true;
        }

        // perform a single CPU step. 
        public void Step()
        {
            if(running)
            {
                DecodeInstruction(tape[index]);
            }
            // if we're not running, we're halted, and we do nothing
        }

        private void DecodeInstruction(byte b)
        {
            switch (b)
            {
                case 0x1:
                    NonDeterministic();
                    break;
                case 0x2:  // NOP
                    index += 2;
                    break;
                case 0x3:
                    Erase(AddressingMode.Relative);
                    index += 2;
                    break;
                case 0x4:
                    Erase(AddressingMode.Absolute);
                    index += 3;
                    break;
                case 0x5:  // more like branch always
                    Console.WriteLine((sbyte)tape[index + 1]);
                    Jump(AddressingMode.Immediate);
                    break;
                case 0x6:
                    Jump(AddressingMode.Absolute);
                    break;
                case 0x7:  // basically a load command
                    Load(AddressingMode.Immediate);
                    index += 2;
                    break;
                case 0x8:  // basically a load command
                    Load(AddressingMode.Relative);
                    index += 2;
                    break;
                case 0x9:
                    Load(AddressingMode.Absolute);
                    index += 3;
                    break;
                case 0xA:
                    Store(AddressingMode.Relative);
                    index += 2;
                    break;
                case 0xB:
                    Store(AddressingMode.Absolute);
                    index += 3;
                    break;
                case 0xC:
                    IncrementState();
                    index += 1;
                    break;
                case 0xD:
                    DecrementState();
                    index += 1;
                    break;
                case 0xE:
                    Compare(AddressingMode.Immediate);
                    index += 2;
                    break;
                case 0xF:
                    Compare(AddressingMode.Relative);
                    index += 2;
                    break;
                case 0x10:
                    Compare(AddressingMode.Absolute);
                    index += 2;
                    break;
                case 0x11:
                    BranchIfEqual(AddressingMode.Immediate);
                    break;
                case 0x12:
                    BranchIfNotEqual(AddressingMode.Immediate);
                    break;
                case 0xFD:
                    DecodeInstruction(tape[GetMemoryAddress(AddressingMode.Relative)]);
                    index += 2;
                    break;
                case 0xFE:
                    DecodeInstruction(tape[GetMemoryAddress(AddressingMode.Absolute)]);
                    index += 2;
                    break;
                case 0xFF:
                    Halt();
                    break;
                default:
                    throw new InvalidOperationException("Invalid opcode: " + b);
            }
            if (DEBUG)
            {
                Console.WriteLine("Current Opcode is {0:X}",b);
            }
        }

        private void Halt()
        {
            running = false;
        }

        private void Store(AddressingMode addressingMode)
        {
            int idx = GetMemoryAddress(addressingMode);
            tape[idx] = MachineState;

        }

        private void Erase(AddressingMode addressingMode)
        {
            int idx = GetMemoryAddress(addressingMode);
            tape[idx] = 0;
        }

        private void Jump(AddressingMode addressingMode)  // shift the tape head by the given number of cells
        {
            index = GetMemoryAddress(addressingMode);
            if(index < 0)
            {
                throw new InvalidOperationException("index cannot be less than 0");
            }
            if (index > tape.Capacity)
            {
                tape.Capacity *= 2;
            }
            CheckRep();
        }

        private void Compare(AddressingMode addressingMode)
        {
            int idx = GetMemoryAddress(addressingMode);
            if (tape[idx] == MachineState)
            {
                flags |= 0x1;  // set the LSB of flags to 1
            } else
            {
                flags &= 0xFE;  // set the LSB of flags to 0
            }
        }

        private void BranchIfEqual(AddressingMode addressingMode)
        {
            byte relativeBranch = tape[GetMemoryAddress(addressingMode)];
            if((flags & 0x1) == 1)  // if the equals flag is set
            {
                sbyte actualBranch = (sbyte)relativeBranch;
                BranchHelper(actualBranch);
            } else
            {
                index += 2;
            }
        }

        private void BranchIfNotEqual(AddressingMode addressingMode)
        {
            byte relativeBranch = tape[GetMemoryAddress(addressingMode)];
            if ((flags & 0x1) == 0)  // if the equals flag is set
            {
                sbyte actualBranch = (sbyte)relativeBranch;
                BranchHelper(actualBranch);
            } else
            {
                index += 2;
            }
        }

        private void Load(AddressingMode addressingMode)
        {
            int idx = GetMemoryAddress(addressingMode);
            MachineState = tape[idx];
        }

        private void IncrementState()  // increment up the machine state
        {
            MachineState++;
        }

        private void DecrementState()  // decrement the machine state
        {
            MachineState--;
        }

        private void BranchHelper(int relativeBranch)  // helps perform branches
        {
            index += relativeBranch;
            Console.WriteLine("Branching " + relativeBranch);
        }

        private int GetMemoryAddress(AddressingMode addressingMode)
        {
            switch (addressingMode)
            {
                case AddressingMode.Immediate:
                    return index + 1;
                case AddressingMode.Relative:
                    return index + ((sbyte) tape[index + 1]);
                case AddressingMode.Absolute:
                    return tape[index + 1] | (tape[index + 2] << 8);
                default:
                    throw new InvalidOperationException("Invalid Addressing Mode");
            }
        }

        private void NonDeterministic()
        {
            Random rand = new Random();
            int pos = rand.Next(tape.Count);
            tape[pos] = (byte)rand.Next(255);
        }

        private void CheckRep()
        {
            if (index < 0)
            {
                throw new InvalidOperationException("Index cannot be negative");
            }
        }
    }
}
