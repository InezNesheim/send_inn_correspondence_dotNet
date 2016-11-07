# Send in Correspondence Service

##Installation guide

To install this windows service perform the following:

1. Start up the command prompt (CMD) with administrator rights.
2. Type c:\windows\microsoft.net\framework\v4.0.30319\installutil.exe [your windows service path to exe]
3. Press return and thats that!

##Run the Service

This service is setup to run on manual start, therefore you need to start the service manually after the installation.

1. Open Service explorer
2. Find SendCorrespondenceService
3. Start the service
