# MgSieve - A .Net ManageSieve Client and Library

MgSieve is a .Net Library for the ManageSieve protocol, written in C#. A simple command line interface is also included. It allows the remote management of Sieve Scripts, according to [RFC5804](https://tools.ietf.org/html/rfc5804).

## Using the Command Line Program

A list of all available command line options is available by using the -h switch:

```
> manage-sieve.exe -h
[*] mgsieve - .Net ManageSieve-Client
[*]   Copyright 2018, Thorsten Schroeder
[*]   Information and updates on GitHub
[*] Usage: manage-sieve.exe
  -h, -?, --help             Halp!
  -v, --verbose              Increase verbosity level
      --connect=VALUE        Connect to server
      --port=VALUE           Connect to port (default: 4190)
      --ssl-cn=VALUE         Server X509Certificate common name
      --upload, --upload-file=VALUE
                             Upload file to server
      --user, --username=VALUE
                             Username on Server
      --pass, --password=VALUE
                             Password for account (not recommended)
      --active, --script-set-active=VALUE
                             Set script active
      --delete, --script-delete=VALUE
                             Delete script from server
      --download, --script-download=VALUE
                             Download script from server
      --ignore-ssl-warnings  Ignore SSL warnings, but you should rather use --
                               ssl-cn=
      --list, --list-scripts List all scripts on server
      --caps, --capabilities Print server capabilities
```

If you provide the ManageSieve port on your server via a different network interface with a different hostname or IP address, you might run into SSL-certificate validation issues, because the `--connect=HOSTNAME` might not match the common name in your X509 certificate. In this case you can use the switch `--ignore-ssl-warnings` (which is not recommended), but rather use the `--ssl-cn` parameter, to provide the proper CN that matches your X509 certificate. 

It is possible to pass the username's password via command line parameter. This is not recommended as well, but provided in case you need to manage sieve scripts without user-interaction. Passing a password via commandline has quite a few disadvantages: 1.) It is visible to other users via the process list of the operating system, 2.) It is probably written to some command line history in plaintext, and 3.) If you start the client with the password parameter from a batch-file, the password is stored in cleartext right within the batch-file.

If you do not pass a password via command line, the program will ask you for a password right after start.

## Using the MgSieve Library

I'll provide a nuget package soon on nuget.org. The library allows you to use any function that is defined [RFC5804](https://tools.ietf.org/html/rfc5804). Error-handling is done with exception handlers, so most of the functions that could fail should be wrapped in try-catch clauses.

## General Information

ManageSieve uses STARTTLS to initiate an authenticated and secure channel between client and server. By default, the SSL handshake will fail safely. If you're using an invalid certificate or if you know what you're doing, you could use the `--ignore-ssl-warnings` switch or the `StartTls(verify=false)` constructor to connect anyway.

Authentication information (PLAIN over TLS) will only be sent, if you successfully initiated a TLS-session before.

By 2018/08/31, the library is not yet fully compliant with [RFC5804](https://tools.ietf.org/html/rfc5804). The library is currently not comliant with the process described in [section 1.8](https://tools.ietf.org/html/rfc5804#section-1.8) of the RFC:

>   Before opening the TCP connection, the ManageSieve client first MUST
>   resolve the Domain Name System (DNS) hostname associated with the
>   receiving entity and determine the appropriate TCP port for
>   communication with the receiving entity.

It will be available in a new release, soon.

Currently, DIGEST-MD5 and GSSAPI authentication methods are not implemented yet.

## Contact

Author of MgSieve library and command line client is Thorsten Schroeder (@devio). If you have any questions, don't hesitate to contact me directly.
This software is released under the Beer-Ware-License (see below).

## License

```
 Copyright (c) 2018, MgSieve .Net Client
    Thorsten Schroeder 
 
 All rights reserved.
 
 This file is part of MgSieve.
 
 "THE BEER-WARE LICENSE" (Revision 42):
 Thorsten Schroeder wrote this file. As long as you
 retain this notice you can do whatever you want with this stuff. If we meet
 some day, and you think this stuff is worth it, you can buy me a beer in
 return. Thorsten Schroeder.
 
 NON-MILITARY-USAGE CLAUSE
 Redistribution and use in source and binary form for military use and
 military research is not permitted. Infringement of these clauses may
 result in publishing the source code of the utilizing applications and
 libraries to the public. As this software is developed, tested and
 reviewed by *international* volunteers, this clause shall not be refused
 due to the matter of *national* security concerns.
 
 THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 ARE DISCLAIMED. IN NO EVENT SHALL THE DDK PROJECT BE LIABLE FOR ANY DIRECT,
 INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```
