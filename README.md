# Send in Correspondence Service

##Installation guide

To install this windows service perform the following:

1. Start up the command prompt (CMD) with administrator rights.
2. Type c:\windows\microsoft.net\framework\v4.0.30319\installutil.exe [your windows service path to exe]
3. Press return and thats that!

Edit the SendCorrespondenceService.exe.config file to specify the From and ToPath, ToPathFailed variables. These indicate which folder the service is watching for new files.

##Run the Service

This service is setup to run on manual start, therefore you need to start the service manually after the installation.

1. Open Service explorer
2. Find SendCorrespondenceService
3. Right click on the service, open properties. On the Log On tab choose: Local System account as user and check the Allow service to interact with desktop box
4. Start the service

