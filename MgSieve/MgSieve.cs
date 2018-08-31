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
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ManageSieve
{
    public class MgSieve
    {
        private String m_sieveserver = null;
        private UInt16 m_sieveport = 4190;
        private TCPClient m_client = null;
        private List<String> m_capabilities_raw;
        public uint verbose = 0;

        public MgSieve(String server, UInt16 port, String tls_server_cn)
        {
            this.m_sieveserver = server;
            this.m_sieveport = port;
            this.m_capabilities_raw = new List<string>();

            try
            {
                this.m_client = new TCPClient(server, port);
                this.m_client.tls_server_cn = tls_server_cn;
                this.m_client.Connect();
            }
            catch (Exception ex)
            {
                this.m_client = null;
                throw ex;
            }

            if (this.verbose > 1)
            {
                this.m_client.debug = true;
            }

        }

        public MgSieve(String server, UInt16 port)
        {
            this.m_sieveserver = server;
            this.m_sieveport = port;
            this.m_capabilities_raw = new List<string>();

            try
            {
                this.m_client = new TCPClient(server, port);
                this.m_client.tls_server_cn = server;
                this.m_client.Connect();
            }
            catch (Exception ex)
            {
                this.m_client = null;
                throw ex;
            }
        }


        ~MgSieve()
        {
            if (!Object.ReferenceEquals(null, this.m_client))
            {
                try { this.m_client.Close(); } catch { };
            }
        }

        public Boolean IsConnectionEncrypted()
        {
            return this.m_client.IsEncrypted();
        }

        public Boolean Logout()
        {
            Boolean result = false;

            int len = this.m_client.Send("LOGOUT\r\n");
            String line = this.m_client.ReadLine();

            if (line.StartsWith("OK"))
                result = true;
            else
                throw new MgSieveException("Logout failed - " + line);

            return result;
        }

        public List<String> GetCapabilities()
        {
            return this.m_capabilities_raw;
        }

        public Boolean Capability()
        {
            Boolean result = true;

            // reset current caps...
            this.m_capabilities_raw = new List<string>();

            int len = this.m_client.Send("CAPABILITY\r\n");
            String line = this.m_client.ReadLine();

            while (!line.StartsWith("OK"))
            {
                line = line.TrimEnd();
                if (!line.Equals(""))
                {
                    this.m_capabilities_raw.Add(line);
                }
                line = this.m_client.ReadLine();
            }

            return result;
        }

        public Boolean HaveSpace(String path)
        {
            Boolean result = false;
            String scriptname = Path.GetFileNameWithoutExtension(path);

            if (!File.Exists(path))
            {
                throw new MgSieveException(String.Format("File {0} not found.", path));
            }

            FileInfo fi = new FileInfo(path);
            long size = fi.Length;

            String command = String.Format("HAVESPACE \"{0}\" {1}\r\n", scriptname, size);

            int len = this.m_client.Send(command);
            String line = this.m_client.ReadLine();

            if (line.StartsWith("OK"))
            {
                result = true;
            }
            else
            {
                line = line.TrimEnd();
                throw new MgSieveException("HaveSpace failed - " + line);
            }
            return result;
        }

        public Boolean DeleteScript(String script)
        {
            Boolean result = false;
            String scriptname = Path.GetFileNameWithoutExtension(script);
            int len = this.m_client.Send(String.Format("DELETESCRIPT \"{0}\"\r\n", scriptname));
            String line = this.m_client.ReadLine();

            if (line.StartsWith("OK"))
            {
                result = true;
            }
            else
            {
                line = line.TrimEnd();
                throw new MgSieveException("DeleteScript failed - " + line);
            }
            return result;
        }

        public Boolean RenameScript(String path_old, String path_new)
        {
            Boolean result = false;

            String scriptname = Path.GetFileNameWithoutExtension(path_old);
            String scriptnew = Path.GetFileNameWithoutExtension(path_new);

            int len = this.m_client.Send(String.Format("RENAMESCRIPT \"{0}\" \"{1}\"\r\n", scriptname, scriptnew));
            String line = this.m_client.ReadLine();

            if (line.StartsWith("OK"))
            {
                result = true;
            }
            else
            {
                line = line.TrimEnd();
                throw new MgSieveException("RenameScript failed - " + line);
            }

            return result;
        }

        public Boolean CheckScript(String path)
        {
            Boolean result = false;

            String scriptname = Path.GetFileNameWithoutExtension(path);

            String command;
            String file_content;

            if (!File.Exists(path))
            {
                throw new MgSieveException(String.Format("File {0} not found.", path));
            }

            file_content = File.ReadAllText(path);
            command = string.Format("CHECKSCRIPT {{{0}+}}\r\n", file_content.Length);

            int len = 0;

            len = this.m_client.Send(command + file_content + "\r\n");

            String line = this.m_client.ReadLine();
            List<String> full = this.GetFullResponse(line);

            if (line.StartsWith("OK"))
            {
                result = true;
            }
            else
            {
                throw new MgSieveException("CheckScript failed - " + String.Join("", full));
            }
            return result;
        }

        public Boolean Noop()
        {
            return this.Noop("");
        }

        public Boolean Noop(String text)
        {
            Boolean result = false;

            String command = "NOOP\r\n";

            if (!text.TrimEnd().Equals(""))
            {
                command = string.Format("NOOP \"{0}\"\r\n", text);
            }

            this.m_client.Send(command);

            String line = this.m_client.ReadLine();
            List<String> full = this.GetFullResponse(line);

            if (line.StartsWith("OK"))
            {
                result = true;
            }
            else
            {
                throw new MgSieveException("Noop failed - " + String.Join("", full));
            }
            return result;
        }

        public Boolean Unauthenticate()
        {
            // The UNAUTHENTICATE command has no parameters.

            int len = this.m_client.Send("UNAUTHENTICATE\r\n");
            String line = this.m_client.ReadLine();

            throw new NotImplementedException("UNAUTHENTICATE");
        }

        public void ReadBanner()
        {
            String line = this.m_client.ReadLine().TrimEnd();

            // discard cached capability information
            this.m_capabilities_raw = new List<string>();

            while (!line.StartsWith("OK"))
            {
                line = line.TrimEnd();
                if (!line.Equals(""))
                {
                    this.m_capabilities_raw.Add(line);
                }
                line = this.m_client.ReadLine();
            }
        }

        // throws exception, if tls/ssl handshake failed or tls is unavailable
        public Boolean StartTls()
        {
            return this.StartTls(true); // do not ignore cert issues by default
        }

        public Boolean StartTls(Boolean verify)
        {
            int len = 0;
            Boolean result = false;

            len = this.m_client.Send("STARTTLS\r\n");

            String line = this.m_client.ReadLine().TrimEnd();

            if (line.StartsWith("NO") || line.StartsWith("BYE"))
            {
                throw new MgSieveException(line);
            }

            result = this.m_client.StartTLS(verify);

            line = this.m_client.ReadLine().TrimEnd();

            // discard cached capability information
            this.m_capabilities_raw = new List<string>();

            while (!line.StartsWith("OK"))
            {
                line = line.TrimEnd();
                if (!line.Equals(""))
                {
                    this.m_capabilities_raw.Add(line);
                }
                line = this.m_client.ReadLine();
            }

            return result;
        }

        // throws exception, if auth failed.
        public Boolean Authenticate(String user, String password)
        {
            Boolean result = false;
            byte[] u = Encoding.UTF8.GetBytes(user);
            byte[] p = Encoding.UTF8.GetBytes(password);
            byte[] auth = new byte[1 + u.Length + 1 + p.Length];
            String authstring;
            int len = 0;

            Buffer.BlockCopy(u, 0, auth, 1, u.Length);
            Buffer.BlockCopy(p, 0, auth, 2 + u.Length, p.Length);

            auth[0] = 0;
            auth[1 + u.Length] = 0;

            authstring = "AUTHENTICATE \"PLAIN\" \"" + Convert.ToBase64String(auth) + "\"\r\n";

            len = this.m_client.Send(authstring);

            String line = this.m_client.ReadLine();

            if (line.StartsWith("OK"))
            {
                result = true;
            }
            else
            {
                result = false;
                throw new MgSieveException(line);
            }
            return result;
        }

        public List<string> ListScripts()
        {
            String command = "LISTSCRIPTS\r\n";
            List<string> scripts = new List<string>();

            int len = 0;

            len = this.m_client.Send(command);

            String line = this.m_client.ReadLine();

            while (!line.StartsWith("OK"))
            {
                line = line.TrimEnd();
                if (!line.Equals(""))
                {
                    scripts.Add(line);
                }
                line = this.m_client.ReadLine();
            }

            return scripts;
        }

        public Boolean PutScript(String path)
        {
            String scriptname = Path.GetFileNameWithoutExtension(path);

            Boolean result = false;
            String command;
            String file_content;

            if (!File.Exists(path))
            {
                throw new MgSieveException(String.Format("File {0} not found.", path));
            }

            file_content = File.ReadAllText(path);
            command = string.Format("PUTSCRIPT \"{0}\" {{{1}+}}\r\n", scriptname, file_content.Length);

            int len = 0;

            len = this.m_client.Send(command + file_content + "\r\n");

            String line = this.m_client.ReadLine();
            List<String> full = this.GetFullResponse(line);

            if (line.StartsWith("OK"))
            {
                result = true;
            }
            else
            {
                throw new MgSieveException("Putscript failed - " + String.Join("", full));
            }

            return result;
        }

        private int ReadSize()
        {
            int result = 0;

            Boolean got_line = false;
            int i = 0;

            byte[] str_size = new byte[1024];
            byte[] line = new byte[1024];
            byte[] b = new byte[1];

            while (!got_line)
            {
                this.m_client.Read(b);
                line[i] = b[0];

                if ((line[i] == '\n') && (line[i - 1] == '\r'))
                {
                    got_line = true;
                    i = i - 1;
                    line[i] = 0x00;
                }
                else
                {
                    i += 1;
                }
            }

            var str = System.Text.Encoding.Default.GetString(line);

            if (str.StartsWith("{"))
            {
                string[] p = str.Split('{', '}');
                result = int.Parse(p[1]);
            }

            return result;
        }

        private List<String> GetFullResponse(String response)
        {
            List<String> result = new List<string>();

            result.Add(response);

            if (response.Contains("{"))
            {
                string[] p = response.Split('{', '}');
                int len = int.Parse(p[1]);

                byte[] data = new byte[len + 2]; // XXX don't understand why dovecot adds another \r\n. Does not comply with RFC5804
                len = this.m_client.Read(data);
                var str = System.Text.Encoding.Default.GetString(data);
                result.Add(str);
            }

            return result;
        }

        public String GetScript(String path)
        {
            String scriptname = Path.GetFileNameWithoutExtension(path);

            String command = String.Format("GETSCRIPT \"{0}\"\r\n", scriptname);
            int len = 0;

            len = this.m_client.Send(command);

            String line = this.m_client.ReadLine();

            if (line.StartsWith("NO"))
            {
                throw new MgSieveException("GetScript failed - " + line.TrimEnd());
            }

            List<String> full = this.GetFullResponse(line);

            do
            {
                line = this.m_client.ReadLine();
            } while (line.TrimEnd().Equals(""));

            if (line.StartsWith("OK"))
            {
                full.RemoveAt(0);
                return String.Join("", full);
            }
            else
            {
                throw new MgSieveException("GetScript failed - " + line.TrimEnd());
            }
        }

        public Boolean SetActive(String path)
        {

            int len = 0;
            String scriptname = Path.GetFileNameWithoutExtension(path);

            String command = String.Format("SETACTIVE \"{0}\"\r\n", scriptname);

            Console.Write("[d] Sending command: {0}", command);
            len = this.m_client.Send(command);

            String line = this.m_client.ReadLine();
            line = line.TrimEnd();

            if (line.StartsWith("NO"))
            {
                throw new MgSieveException("SetActive failed - " + line);
            }

            return true;
        }
    }

    [Serializable()]
    public class MgSieveException : System.Exception
    {
        public MgSieveException() : base() { }
        public MgSieveException(string message) : base(message)
        {
        }
        public MgSieveException(string message, System.Exception inner) : base(message, inner)
        {
        }

        protected MgSieveException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }

    class TCPClient
    {
        private String m_host = null;
        private UInt16 m_port = 0;
        private TcpClient m_tcpcli = null;
        private NetworkStream m_stream = null;
        private SslStream m_sslstream = null;
        private Boolean m_is_encrypted;
        private Boolean m_ignore_sslauth;

        public String tls_server_cn = null;
        public Boolean debug = false;

        public TCPClient(String host, UInt16 port)
        {
            m_host = host;
            this.tls_server_cn = host;
            m_port = port;
            m_is_encrypted = false;
            m_ignore_sslauth = false;
        }

        ~TCPClient()
        {
            this.m_is_encrypted = false;

            if (this.m_sslstream != null)
            {
                this.m_sslstream.Close();
            }

            if (this.m_tcpcli != null)
            {
                this.m_tcpcli.Close();
            }
        }

        public Boolean IsEncrypted()
        {
            return this.m_is_encrypted;
        }

        public void IgnoreSslErrors(Boolean value)
        {
            this.m_ignore_sslauth = value;
        }

        public Boolean Connect()
        {
            this.m_tcpcli = new TcpClient(this.m_host, this.m_port);
            this.m_stream = this.m_tcpcli.GetStream();
            return true;
        }

        public Boolean Close()
        {
            this.m_is_encrypted = false;
            this.m_sslstream.Close();
            this.m_tcpcli.Close();
            return true;
        }

        public String ReadLine()
        {
            List<byte> line = new List<byte>();

            Boolean got_line = false;
            int i = 0;

            byte[] b = new byte[1];

            while (!got_line)
            {
                if (this.m_is_encrypted == true)
                {
                    this.m_sslstream.Read(b, 0, 1);
                }
                else
                {
                    this.m_stream.Read(b, 0, 1);
                }

                line.Add(b[0]);

                if ((b[0] == '\n') && (line[i - 1] == '\r'))
                {
                    got_line = true;
                    i = i - 1;
                    line.RemoveAt(i);
                }
                else if (b[0] == '\n')
                {
                    got_line = true;

                    line.RemoveAt(i);
                }
                else
                {
                    i += 1;
                }
            }
            return System.Text.Encoding.Default.GetString(line.ToArray()).TrimEnd();
        }

        public int Read(byte[] data)
        {
            int result = 0;
            if (this.m_is_encrypted == true)
            {
                result = this.m_sslstream.Read(data, 0, data.Length);
            }
            else
            {
                result = this.m_stream.Read(data, 0, data.Length);
            }
            return result;
        }

        public int Send(byte[] data)
        {
            int result = 0;
            if (this.m_is_encrypted == true)
            {
                this.m_sslstream.Write(data, 0, data.Length);
                this.m_sslstream.Flush();
            }
            else
            {
                this.m_stream.Write(data, 0, data.Length);
            }

            return result;
        }

        public int Send(String str)
        {
            int result = 0;
            byte[] buf = Encoding.ASCII.GetBytes(str);

            if (this.m_is_encrypted == true)
            {
                this.m_sslstream.Write(buf, 0, buf.Length);
                this.m_sslstream.Flush();
            }
            else
            {
                this.m_stream.Write(buf, 0, buf.Length);
            }

            return result;
        }

        private static Hashtable m_certificateErrors = new Hashtable();

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // Do not allow this client to communicate with unauthenticated servers.
            throw new TcpClientException(String.Format("SSL Certificate error - {0}", sslPolicyErrors));
        }

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool DoNotValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            return true; // ignore ssl errors.
        }

        public Boolean StartTLS()
        {
            return this.StartTLS(true); // do not ignore cert issues by default
        }

        public Boolean StartTLS(Boolean verify)
        {
            if (verify == false)
            {
                // ignore ssl errors...
                this.m_sslstream = new SslStream(
                    this.m_stream,
                    false,
                    new RemoteCertificateValidationCallback(DoNotValidateServerCertificate),
                    null
                    );
            }
            else
            {
                // do NOT ignore ssl errors
                try
                {
                    this.m_sslstream = new SslStream(
                        this.m_stream,
                        false,
                        new RemoteCertificateValidationCallback(ValidateServerCertificate),
                        null
                        );
                }
                catch (Exception e)
                {
                    this.Close();
                    throw e;
                }
            }

            // The server name must match the name on the server certificate.
            try
            {
                this.m_sslstream.AuthenticateAsClient(this.tls_server_cn);
            }
            catch (AuthenticationException e)
            {
                this.Close();
                throw e;
            }

            this.m_is_encrypted = this.m_sslstream.IsEncrypted;

            return this.m_is_encrypted;
        }
    }

    [Serializable()]
    public class TcpClientException : System.Exception
    {
        public TcpClientException() : base() { }
        public TcpClientException(string message) : base(message)
        {
        }
        public TcpClientException(string message, System.Exception inner) : base(message, inner)
        {
        }

        protected TcpClientException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }
}
