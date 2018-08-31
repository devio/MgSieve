/*
 * Copyright (c) 2018, MgSieve .Net Client
 *    Thorsten Schroeder 
 *
 * All rights reserved.
 *
 * This file is part of MgSieve.
 *
 * "THE BEER-WARE LICENSE" (Revision 42):
 * Thorsten Schroeder wrote this file. As long as you
 * retain this notice you can do whatever you want with this stuff. If we meet
 * some day, and you think this stuff is worth it, you can buy me a beer in
 * return. Thorsten Schroeder.
 *
 * NON-MILITARY-USAGE CLAUSE
 * Redistribution and use in source and binary form for military use and
 * military research is not permitted. Infringement of these clauses may
 * result in publishing the source code of the utilizing applications and
 * libraries to the public. As this software is developed, tested and
 * reviewed by *international* volunteers, this clause shall not be refused
 * due to the matter of *national* security concerns.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE DDK PROJECT BE LIABLE FOR ANY DIRECT,
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
 */
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mgsieve
{

    public static class Helper
    {
        public static String GetSizeString(UInt64 size)
        {
            String s;

            if (size > (UInt64)1024 * 1024 * 1024 * 1024 * 1024 * 99)
            {
                s = String.Format("{0} PiB", size / 1024 / 1024 / 1024 / 1024 / 1024);
            }
            else if (size > (UInt64)1024 * 1024 * 1024 * 1024 * 1024)
            {
                s = String.Format("{0} PiB", (size / 1024.0 / 1024 / 1024 / 1024 / 1024).ToString("0.00"));
            }
            else if (size > (UInt64)1024 * 1024 * 1024 * 1024 * 99)
            {
                s = String.Format("{0} TiB", size / 1024 / 1024 / 1024 / 1024);
            }
            else if (size > (UInt64)1024 * 1024 * 1024 * 1024)
            {
                s = String.Format("{0} TiB", (size / 1024.0 / 1024 / 1024 / 1024));
            }
            else if (size > (UInt64)1024 * 1024 * 1024 * 99)
            {
                s = String.Format("{0} GiB", size / 1024 / 1024 / 1024);
            }
            else if (size > (UInt64)1024 * 1024 * 1024)
            {
                s = String.Format("{0} GiB", (size / 1024.0 / 1024 / 1024));
            }
            else if (size > (UInt64)1024 * 1024 * 99)
            {
                s = String.Format("{0} MiB", size / 1024 / 1024);
            }
            else if (size > (UInt64)1024 * 1024)
            {
                s = String.Format("{0} MiB", (size / 1024.0 / 1024));
            }
            else if (size > (UInt64)1024)
            {
                s = String.Format("{0} KiB", size / 1024);
            }
            else
            {
                s = String.Format("{0} bytes", size);
            }

            return s;
        }

        public static void Hexdump(byte[] payload, int slen, int offset)
        {

            int i = 0;
            int pi = 0;
            int gap, len;
            //byte ch;
            int pi2 = 0;

            len = 16;
            //ch = payload[pi];

            do
            {

                Console.Write("{0,5:X5}   ", offset);

                /* hex */
                if (slen < len)
                {
                    len = slen;
                }

                for (i = 0; i < len; i++)
                {

                    Console.Write("{0,2:X2} ", payload[pi]);
                    pi += 1;

                    /* print extra space after 8th byte for visual aid */
                    if (i == 7)
                    {
                        Console.Write(' ');
                    }
                }
                /* print space to handle line less than 8 bytes */
                if (len < 8)
                {
                    Console.Write(' ');
                }

                /* fill hex gap with spaces if not full line */
                if (len < 16)
                {
                    gap = 16 - len;
                    for (i = 0; i < gap; i++)
                    {
                        Console.Write("   ");
                    }
                }

                Console.Write("   ");

                /* ascii (if printable) */
                for (i = 0; i < len; i++)
                {

                    if (isPrint(payload[pi2]))
                    {
                        //Console.Write("{0:C}", payload[pi2]);

                        Console.Write(Convert.ToChar(payload[pi2]));

                    }
                    else
                    {
                        Console.Write('.');
                    }
                    pi2 += 1;
                }

                Console.WriteLine("");

                slen -= 16;
                offset += len;
            } while (slen > 0);


            return;
        }

        ///CharINDec-is the character in ascii
        ///returns true or false.
        ///is char is printable ascii then returns true and if it's not then false
        private static bool isPrint(int CharINDec)
        {
            if (CharINDec >= 32 && CharINDec <= 126)
                return true;
            else
                return false;
        }


    }



}
