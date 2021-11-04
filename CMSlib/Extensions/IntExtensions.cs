﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CMSlib.Extensions
{
    public static class IntExtensions
    {
        public static BinaryInt ToBinary(this int i)
        {
            return new(i);
        }

        public static int ToPower(this int num, int power)
        {
            int startingNum = num;
            if (power == 0) return 1;
            for (int i = 0; i < power - 1; i++)
            {
                num *= startingNum;
            }

            return num;
        }

        public static int NumOfDigits(this int num)
        {
            int returns = 0;
            for (int i = 1; num / i > 0; i *= 10.PlusPlusThisToo(ref returns)) ;
            return returns;
        }

        public static IEnumerable<int> GetDigits(this int num)
        {
            if (num == 0)
            {
                yield return 0;
                yield break;
            }

            int i = (int) Math.Pow(10, (int) Math.Ceiling(Math.Log10(num + 1)) - 1);
            for (; i > 0; i /= 10)
            {
                yield return num / i;
                num = num % i;
            }
        }

        private static int PlusPlusThisToo(this int num, ref int toPlusPlus)
        {
            toPlusPlus++;
            return num;
        }

        public static TaskAwaiter GetAwaiter(this int num)
        {
            return Task.Delay(num).GetAwaiter();
        }

        public static int Modulus(this int operand, int num)
        {
            int result = operand % num;
            return result < 0 ? result + num : result;
        }

        public static void KeyPress(this byte num)
        {
            keybd_event(num, 0, 0x0001, (IntPtr) 0);
            keybd_event(num, 0, 0x0001 | 0x0002, (IntPtr) 0);
        }

        public static void HoldKey(this byte num) => keybd_event(num, 0, 0x0001, (IntPtr) 0);

        public static void ReleaseKey(this byte num) => keybd_event(num, 0, 0x0001 | 0x0002, (IntPtr) 0);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte key, byte scan, uint flags, IntPtr exInfo);
    }

    public struct BinaryInt
    {
        private readonly bool[] _bits;

        public BinaryInt(int base10)
        {
            _bits = new bool[32];
            for (int i = _bits.Length - 1; i >= 0; i--)
            {
                if (base10 / 2.ToPower(i) > 0)
                {
                    _bits[_bits.Length - i - 1] = true;
                    base10 -= 2.ToPower(i);
                }
                else
                {
                    _bits[_bits.Length - i - 1] = false;
                }
            }
        }


        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(_bits.Length);
            foreach (bool bit in _bits)
            {
                builder.Append(bit ? 1 : 0);
            }

            return builder.ToString();
        }

        public bool BitAt(int position)
        {
            if (position >= _bits.Length || position < 0)
            {
                throw new ArgumentException("Position must be a valid bit index, between 0 and 31, inclusive");
            }

            return _bits[position];
        }

        /// <summary>
        /// Gets the bit at the specific index
        /// </summary>
        /// <param name="index">The index to get the bit at, from 0 - 31</param>
        public bool this[int index]
        {
            get { return BitAt(index); }
        }
    }
}