Microsoft (R) File Checksum Integrity Verifier V2.05 README file
================================================================

1.What is File Checksum Integrity Verifier (FCIV)?
2.Features.
3.Syntax.
4.Database storage format.
5.Verification.
6.History.


1.What is fciv?
---------------
Fciv is a command line utility that computes and verifies hashes of files.

It computes a MD5 or SHA1 cryptographic hash of the content of the file.
If the file is modified, the hash is different.

With fciv, you can compute hashes of all your sensitive files.
When you suspect that your system has been compromised, you can run a verification to determine which files have been modified.
You can also schedule verifications regularily.

2.Features:
-----------
- Hash algorithm: MD5 , SHA1 or both ( default MD5).
- Display to screen or store hash and filename in a xml file.
- Can recursively browse a directory ( ex fciv.exe c:\ -r ).
- Exception list to specify files or directories that should not be computed.
- Database listing.
- hashes and signature verifications.
- store filename with or without full path.

3.Syntax:
---------
Usage:  fciv.exe [Commands] <Options>

Commands: ( Default -add )

        -add    <file | dir> : Compute hash and send to output (default screen).

                dir options:
                -r       : recursive.
                -type    : ex: -type *.exe.
                -exc file: list of directories that should not be computed.
                -wp      : Without full path name. ( Default store full path)
                -bp      : base path. The base path is removed from the path name of each entry

        -list            : List entries in the database.

        -v               : Verify hashes.
                         : Option: -bp basepath.

        -? -h -help      : Extended Help.

Options:
        -md5 | -sha1 | -both    : Specify hashtype, default md5.
        -xml db                 : Specify database format and name.

To display the MD5 hash of a file, type fciv.exe filename

Compute hashes:
        fciv.exe c:\mydir\myfile.dll
        fciv.exe c:\ -r -exc exceptions.txt -sha1 -xml dbsha.xml
        fciv.exe c:\mydir -type *.exe
        fciv.exe c:\mydir -wp -both -xml db.xml

List hashes stored in database:
        fciv.exe -list -sha1 -xml db.xml

Verifications:
        fciv.exe -v -sha1 -xml db.xml
        fciv.exe -v -bp c:\mydir -sha1 -xml db.xml
        
4.Database storage format:
--------------------------
xml file.

The hash is stored in base 64.
<?xml version="1.0" encoding="utf-8"?>
<FCIV>
	<FILE_ENTRY>
		<name> </name>
		<MD5> </MD5>
		<SHA1> </SHA1>
	</FILE_ENTRY>
</FCIV>	

5.Verification:
---------------
You can build a hash database of your sensitive files and verify them regularily or when you suspect that your system
has been compromised.

It checks each entry stored in the db and verify that the checksum was not modified.

6. History:
-----------
Fciv 1.2 : Added event log.
Fciv 1.21: Fixed bad keyset error on some computers.
Fciv 1.22: Added -type option. Support up to 10 masks. *.exe *.dll ...
Fciv 2.0:  xml as unique storage. Added -both option.
Fciv 2.01: Exit with error code to allow detections of problem in a script.
Fciv 2.02: Improved perfs. When both alg are specified, it's now done in one pass.
Fciv 2.03: Added -wp and -bp options. Fciv now stores full path or relatives paths.
Fciv 2.04: Removed several options to simplify it.
Fciv 2.05: Added success message if the verification did not detect any errors.
