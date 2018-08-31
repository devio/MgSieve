
manage-sieve.exe  --connect 10.23.23.143 --ssl-cn imap.contoso.com --caps
manage-sieve.exe  --connect 10.23.23.143 --ssl-cn imap.contoso.com --list
manage-sieve.exe  --connect 10.23.23.143 --ssl-cn imap.contoso.com --upload ..\..\..\sieve-samples\foobar.sieve
manage-sieve.exe  --connect 10.23.23.143 --ssl-cn imap.contoso.com --user sievetest@contoso.com --pass Oelk.olqua2.3 --caps
manage-sieve.exe  --connect 10.23.23.143 --ssl-cn imap.contoso.com --user sievetest@contoso.com --pass Oelk.olqua2.3 --list
manage-sieve.exe  --connect 10.23.23.143 --ssl-cn imap.contoso.com --user sievetest@contoso.com --pass Oelk.olqua23 --caps
manage-sieve.exe  --connect 10.23.23.143 --ssl-cn imap.contoso.com --user sievetest@contoso.com --pass Oelk.olqua23 --list

manage-sieve.exe  --connect 10.23.23.143 --ignore-ssl-warnings --user sievetest@contoso.com --pass Oelk.olqua2.3 --upload nonexistent.script 
manage-sieve.exe  --connect 10.23.23.143 --ignore-ssl-warnings --user sievetest@contoso.com --pass Oelk.olqua2.3 --upload ..\..\..\misc\sieve-samples\invalidscript.sieve
manage-sieve.exe  --connect 10.23.23.143 --ignore-ssl-warnings --user sievetest@contoso.com --pass Oelk.olqua2.3 --upload ..\..\..\misc\sieve-samples\foobar.sieve
manage-sieve.exe  --connect 10.23.23.143 --ignore-ssl-warnings --user sievetest@contoso.com --pass Oelk.olqua2.3 --upload ..\..\..\misc\sieve-samples\hutsefluts.sieve

manage-sieve.exe  --connect 10.23.23.143 --ignore-ssl-warnings --user sievetest@contoso.com --pass Oelk.olqua2.3 --active foononexistent
manage-sieve.exe  --connect 10.23.23.143 --ignore-ssl-warnings --user sievetest@contoso.com --pass Oelk.olqua2.3 --active foobar

manage-sieve.exe  --connect 10.23.23.143 --ignore-ssl-warnings --user sievetest@contoso.com --pass Oelk.olqua2.3 --delete foobar
manage-sieve.exe  --connect 10.23.23.143 --ignore-ssl-warnings --user sievetest@contoso.com --pass Oelk.olqua2.3 --delete nonexistent
manage-sieve.exe  --connect 10.23.23.143 --ignore-ssl-warnings --user sievetest@contoso.com --pass Oelk.olqua2.3 --delete hutsefluts

manage-sieve.exe  --connect 10.23.23.143 --ignore-ssl-warnings --user sievetest@contoso.com --pass Oelk.olqua2.3 --download nonexistent
manage-sieve.exe  --connect 10.23.23.143 --ignore-ssl-warnings --user sievetest@contoso.com --pass Oelk.olqua2.3 --download foobar
manage-sieve.exe  --connect 10.23.23.143 --ignore-ssl-warnings --user sievetest@contoso.com --pass Oelk.olqua2.3 --download validscript

manage-sieve.exe  --connect 10.23.23.143 --ignore-ssl-warnings --user sievetest@contoso.com --pass Oelk.olqua2.3 

