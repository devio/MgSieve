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
using System.Security;
using System.Text;
using System.Threading.Tasks;
using ManageSieve;
using NDesk.Options;

namespace MgSieve_Client
{
    class Program
    {
        static uint param_verbose = 0;
        static String param_server = null;
        static UInt16 param_port = 4190;
        static String param_sslcn = null;
        static String param_fileupload = null;
        static String param_username = null;
        static String param_password = null;
        static String param_scriptsetactive = null;
        static String param_scriptdelete = null;
        static String param_scriptdownload = null;

        static Boolean param_ignoresslerror = false;
        static Boolean param_listscripts = false;
        static Boolean param_capabilities = false;
        static Boolean param_showhelp = false;

        static void usage(OptionSet p)
        {
            Console.WriteLine("[*] MgSieve - .Net ManageSieve-Client");
            Console.WriteLine("[*]   Copyright 2018, Thorsten Schroeder");
            Console.WriteLine("[*]   Information and updates on GitHub");

            Console.WriteLine("[*] Usage: {0}", System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.Split('\\').Last());
            p.WriteOptionDescriptions(Console.Out);

            Environment.Exit(0);
        }

        static int Main(string[] args)
        {
            List<string> extra;
            var p = new OptionSet() {
               { "h|?|help", "Halp!", v => param_showhelp = v != null },
               { "v|verbose", "Increase verbosity level", v => { if (v != null) ++param_verbose; } },
               { "connect=", "Connect to server", v => param_server = v },
               { "port=", "Connect to port (default: 4190)", v => param_port = UInt16.Parse(v) },
               { "ssl-cn=", "Server X509Certificate common name", v => param_sslcn = v },
               { "upload=|upload-file=", "Upload file to server", v => param_fileupload = v },
               { "user=|username=", "Username on Server", v => param_username = v },
               { "pass=|password=", "Password for account (not recommended)", v => param_password = v },
               { "active=|script-set-active=", "Set script active", v => param_scriptsetactive = v },
               { "delete=|script-delete=", "Delete script from server", v => param_scriptdelete = v },
               { "download=|script-download=", "Download script from server", v => param_scriptdownload = v },
               { "ignore-ssl-warnings", "Ignore SSL warnings, but you should rather use --ssl-cn=", v => param_ignoresslerror = v != null },
               { "list|list-scripts", "List all scripts on server", v => param_listscripts = v != null },
               { "caps|capabilities", "Print server capabilities", v => param_capabilities = v != null },

            };

            Boolean is_authenticated = false;
            MgSieve sieveclient;

            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException)
            {
                Console.WriteLine("Try '--help' for more information.");
                return -1;
            }

            if (param_showhelp)
                usage(p);

            // check params

            if (param_server == null || (param_username == null && !param_capabilities))
            {
                usage(p);
            }

            if (param_verbose > 1)
            {
                dump_params();
            }

            try
            {
                if (param_sslcn == null)
                {
                    sieveclient = new MgSieve(param_server, param_port);
                }
                else
                {
                    sieveclient = new MgSieve(param_server, param_port, param_sslcn);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[e] connect failed: " + e.Message);
                return 1;
            }

            sieveclient.verbose = param_verbose;

            sieveclient.ReadBanner();
            sieveclient.Capability();

            try
            {
                if (param_ignoresslerror == true)
                {
                    // establish tls connection w/o verifying authenticity of the server
                    sieveclient.StartTls(false);
                }
                else
                {
                    // establish tls connection and verify authenticity of the server
                    sieveclient.StartTls(true);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[e] TLS handshake failed: " + ex.Message);
                Console.ResetColor();

                Environment.Exit(-1);
            }

            // dump caps w/o auth
            if (param_username == null && param_capabilities)
            {
                printCapabilities();
                Environment.Exit(-1);
            }

            try
            {
                authenticate();
            }
            catch (MgSieveException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[e] authenticate() exception: " + ex.Message);
                Console.ResetColor();

                Environment.Exit(-1);
            }

            if (param_capabilities)
            {
                printCapabilities();
            }

            if (param_listscripts)
            {
                List<String> scripts = sieveclient.ListScripts();
                scripts.ForEach((x) => Console.WriteLine("  - {0}", x));
            }

            if (param_fileupload != null)
            {
                Console.WriteLine("[-] uploading file {0}", param_fileupload);

                try
                {
                    sieveclient.HaveSpace(param_fileupload);
                    sieveclient.CheckScript(param_fileupload);
                    sieveclient.PutScript(param_fileupload);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[e] file upload failed: " + ex.Message);
                    Console.ResetColor();
                    Environment.Exit(0);
                }

            }

            if (param_scriptdownload != null)
            {
                String scriptcontent;
                Console.WriteLine("[-] downloading file {0}", param_scriptdownload);

                try
                {
                    scriptcontent = sieveclient.GetScript(param_scriptdownload);
                    Console.WriteLine("[-] Script content:");
                    Console.WriteLine(scriptcontent);
                    // TODO: save script content to file
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[e] file download failed: " + ex.Message);
                    Console.ResetColor();
                    Environment.Exit(0);
                }
            }

            if (param_scriptdelete != null)
            {
                Console.WriteLine("[-] removing script {0}", param_scriptdelete);

                try
                {
                    sieveclient.DeleteScript(param_scriptdelete);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[e] script delete failed: " + ex.Message);
                    Console.ResetColor();
                    Environment.Exit(0);
                }
            }

            if (param_scriptsetactive != null)
            {
                Console.WriteLine("[-] activating script {0}", param_scriptsetactive);
                try
                {
                    sieveclient.SetActive(param_scriptsetactive);
                    sieveclient.ListScripts();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[e] script setactive failed: " + ex.Message);
                    Console.ResetColor();
                    Environment.Exit(0);
                }
            }

            try
            {
                sieveclient.Logout();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[e] Logout failed: " + ex.Message);
                Console.ResetColor();
                Environment.Exit(0);
            }

            return 0;

            // ******* local functions ***********************************************

            Boolean authenticate()
            {
                String passwd = param_password;

                if (!sieveclient.IsConnectionEncrypted())
                {
                    return false;
                }

                if (param_password.Equals(""))
                {
                    Console.Write("Enter password for {0}: ", param_username);
                    passwd = GetConsolePassword();
                    Console.WriteLine();
                }

                is_authenticated = sieveclient.Authenticate(param_username, passwd.ToString());

                return is_authenticated;
            }

            void printCapabilities()
            {
                List<String> caps = sieveclient.GetCapabilities();
                Console.WriteLine("[-] Server capabilities:");
                caps.ForEach((x) => Console.WriteLine("  - {0}", x));
            }

            void dump_params()
            {
                Console.WriteLine("[c] param_server             = {0}", param_server);
                Console.WriteLine("[c] param_port               = {0}", param_port);
                Console.WriteLine("[c] param_sslcn              = {0}", param_sslcn);
                Console.WriteLine("[c] param_fileupload         = {0}", param_fileupload);
                Console.WriteLine("[c] param_username           = {0}", param_username);
                Console.WriteLine("[c] param_scriptsetactive    = {0}", param_scriptsetactive);
                Console.WriteLine("[c] param_scriptdelete       = {0}", param_scriptdelete);
                Console.WriteLine("[c] param_scriptdownload     = {0}", param_scriptdownload);
                Console.WriteLine("[c] param_verbose            = {0}", param_verbose);
                Console.WriteLine("[c] param_ignoresslerror     = {0}", param_ignoresslerror);
                Console.WriteLine("[c] param_listscripts        = {0}", param_listscripts);
                Console.WriteLine("[c] param_capabilities       = {0}", param_capabilities);
            }
        } // end of main()

        private static string GetConsolePassword()
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                if (cki.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
                        Console.Write("\b\0\b");
                        sb.Length--;
                    }

                    continue;
                }

                Console.Write('*');
                sb.Append(cki.KeyChar);
            }

            return sb.ToString();
        }
    }
}
