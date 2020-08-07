
# SyncPro CloudOS API Library and demo Crestron device

>  This collection includes several classes to make development for the SyncPro's CloudOS easier for Crestron S#Pro and S# developers.

To develop for the SyncPro API you need to be a SyncPro Partner. If you wish to become a partner, please contact us at partners@syncpro.io.

### Tested with the following SW versions
* Crestron DB - 200.00.004.00  
* Device DB - 200.00.015.00  
* MS VS2008 - 9.0.30728.1 SP
* Crestron SIMPL # Pro - v2.000.0058


### Overview

- Api.cs - This it the main class, which implements the SyncPro API calls. For more information about the CloudOS api, please browse
	to the partners portal at https://partners.syncpro.io. 

- DataStore.cs - This is class is designed to help developers read and write credentials and configuraitions files.

- Settings.cs - Configure your debug mode as well as the hub you're working with (production or staging)

- Lib\DeviceConfig.cs - This is a basic device configuration file. Other devices' config files should inherit from this class and extend it.

- Lib\Exceptions.cs - Set of custom exceptions for the library.

- Lib\FileMethods.cs - Set of file methods to help developers save and read files to\from the local file system.

- Lib\Loggin.cs - Some simple methods to log exceptions, errors and notices.

- Lib\ServerResponse - A set of server responses classes per the API.

- Lib\SharedObjects - A generic set of shared objects (that didn't make sense to pur anywhere else...:))

- Lib\TelemetryMessage - A basic telemetry message class. Other devices' telemetry messages should extend this one.

- Lib\WebMethods - Some core webmethods to Get and Post data from\to the server= This collection includes several classes to make development for the SyncPro's CloudOS easier for Crestron S#Pro and S# developers.

