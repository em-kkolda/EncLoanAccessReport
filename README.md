# Encompass Loan Access Report Tool
This tool produces a report of all Encompass users in a system and the number of Loans that the user can access (read). The tool is meant to 
provide system and security administrators of an Encompass system with a means to audit their users' access. Run periodically, it can help
your organization avoid "access creep" and adhere to a least privileges approach to data access.

## Command-line Options
Once compiled, you can run the tool on the command line as follows:

```
PROMPT> encloanaccessreport -u <sa_uid> -p <sa_pwd> -s <server_url> -o <output_file>
```

The `sa_uid` and `sa_pwd` parameters are the credentials for an Encompass user with Super Administrator privileges. A highly privileged
account is required by this tool in order to "impersonate" other users and determine their level of visibility.

The `server_url` is the URL of yoru Encompass Server, which typically has the format `https://<iid>.ea.elliemae.net$<iid>`, where
`scid` is your Encompass Instance ID (e.g. "BE11112222").

The `output_file` parameter is optional and can be used to direct the output to a file. Otherwise, the output will be written to
stdout (i.e. the console window).

## SDK Licensing
This tool leverages the Encompass SDK, which is an integration tool available to you as part of your Encompass license. However,
using the SDK requires that the machine on which the program will run be registered using an SDK Key generated for your organization.
If you do not already have an SDK Key, contact your Account Represetative for more information.