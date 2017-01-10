**XMediusSENDSECURE (SendSecure)** is a collaborative file exchange platform that is both highly secure and simple to use.
It is expressly designed to allow for the secured exchange of sensitive documents via virtual SafeBoxes.

SendSecure comes with a **Web API**, which is **RESTful**, uses **HTTPs** and returns **JSON**.

Specific libraries have been published for various languages:
**C#**,
[Java](https://github.com/xmedius/sendsecure-java),
[JavaScript](https://github.com/xmedius/sendsecure-js),
[PHP](https://github.com/xmedius/sendsecure-php),
[Python](https://github.com/xmedius/sendsecure-python)
and
[Ruby](https://github.com/xmedius/sendsecure-ruby).

# sendsecure-csharp

**This library allows you to use the SendSecure Web API via C# with .NET.**

With this library, you will be able to:
* Authenticate SendSecure users
* Create new SafeBoxes

# Table of Contents

* [Installation](#installation)
* [Quick Start](#quickstart)
* [Usage](#usage)
* [License](#license)
* [Credits](#credits)

<a name="installation"></a>
# Installation

## Prerequisites

- .NET version 4.5.2+
- The SendSecure service, provided by [XMedius](https://www.xmedius.com/en/products?source=sendsecure-csharp) (demo accounts available on demand)

## Install Package

To use sendsecure-csharp in your C# project, clone or download the SendSecure C# .NET libraries directly from our GitHub repository.

<a name="quickstart"></a>
# Quick Start

## Authentication (Retrieving API Token)

Authentication is done using an API Token, which must be first obtained based on SendSecure enterprise account and user credentials.
Here is the minimum code to get such a user-based API Token.

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMedius.SendSecure;

namespace SendSecureClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string token = GetUserTokenAsync().GetAwaiter().GetResult();
            }
            catch (XMedius.SendSecure.Exceptions.SendSecureException e)
            {
                Console.WriteLine(e.Message);
                if(!String.IsNullOrEmpty(e.Response))
                {
                    Console.WriteLine("Server response: " + e.Response);
                }
            }
        }

        public static async Task<string> GetUserTokenAsync()
        {
            string token = await XMedius.SendSecure.Client.GetUserTokenAsync("deathstar", "darthvader", "d@Rk$1De", "DV-TIE/x1", "TIE Advanced x1", "The Force App", new Uri("https://portal.xmedius.com"));

            Console.WriteLine(token);
            return token;
        }
    }
}
```

## SafeBox Creation  (Using SafeBox Helper Class)

Here is the minimum required code to create a SafeBox – with 1 recipient, a subject, a message and 1 attachment.
This example uses the user's *default* security profile (which requires to be set in the account).

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMedius.SendSecure;

namespace SendSecureClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string token = "USER|1d495165-4953-4457-8b5b-4fcf801e621a";

                string previewUrl = SubmitSafeboxAsync(token).GetAwaiter().GetResult();
            }
            catch (XMedius.SendSecure.Exceptions.SendSecureException e)
            {
                Console.WriteLine(e.Message);
                if(!String.IsNullOrEmpty(e.Response))
                {
                    Console.WriteLine("Server response: " + e.Response);
                }
            }
        }

        public static async Task<string> SubmitSafeboxAsync(string token)
        {
            var safebox2 = new XMedius.SendSecure.Helpers.Safebox("darthvader@empire.com");
            safebox2.Subject = "Family matters";
            safebox2.Message = "Son, you will find attached the evidence.";

            safebox2.Attachments.Add(new XMedius.SendSecure.Helpers.Attachment("d:\\Birth_Certificate.pdf"));

            var recipient = new XMedius.SendSecure.Helpers.Recipient("lukeskywalker@rebels.com");
            var contactMethod = new XMedius.SendSecure.Helpers.ContactMethod { Destination = "555-232-5334", DestinationType = XMedius.SendSecure.Helpers.ContactMethod.DestinationTypeT.CellPhone };
            recipient.ContactMethods.Add(contactMethod);
            safebox2.Recipients.Add(recipient);

            XMedius.SendSecure.Client client = new Client(token, "deathstar", new Uri("https://portal.xmedius.com"));
            XMedius.SendSecure.Helpers.SafeboxResponse safeboxResponse = await client.SubmitSafeboxAsync(safebox2);

            Console.WriteLine(safeboxResponse.PreviewUrl);
            return safeboxResponse.PreviewUrl;
        }
    }
}
```

<a name="usage"></a>
# Usage

## Helper Methods

### Get User Token
```
GetUserTokenAsync(enterpriseAccount, username, password, deviceId, deviceName, applicationType, endpoint, oneTimePassword, cancellationToken)
```
Creates and returns an API Token for a specific user within a SendSecure enterprise account.
Calling this method again with the exact same params will always return the same Token.

Param             | Definition
------------------|-----------
enterpriseAccount | The SendSecure enterprise account
username          | The username of a SendSecure user of the current enterprise account
password          | The password of this user
deviceId          | The unique ID of the device used to get the Token 
deviceName        | The name of the device used to get the Token
applicationType   | The type/name of the application used to get the Token ("SendSecure C#" will be used by default if empty)
endpoint          | The URL to the SendSecure service ("https://portal.xmedius.com" will be used by default if empty)
oneTimePassword   | The one-time password of this user (if any)
cancellationToken | A cancellation token that can be used by other objects or threads to receive notice of cancellation

### Client Object Constructor
```
Client(apiToken, enterpriseAccount, endpoint, locale)
```

Param             | Definition
------------------|-----------
apiToken          | The API Token to be used for authentication with the SendSecure service
enterpriseAccount | The SendSecure enterprise account
endpoint          | The URL to the SendSecure service ("https://portal.xmedius.com" will be used by default if empty)
locale            | The locale in which the server errors will be returned ("en" will be used by default if empty)

### Get Enterprise Settings
```
EnterpriseSettingsAsync(cancellationToken)
```
Returns all values/properties of the enterprise account's settings specific to SendSecure.

Param             | Definition
------------------|-----------
cancellationToken | A cancellation token that can be used by other objects or threads to receive notice of cancellation

### Get Default Security Profile
```
DefaultSecurityProfileAsync(userEmail, cancellationToken)
```
Returns the default security profile (if it has been set) for a specific user, with all its setting values/properties.

Param             | Definition
------------------|-----------
userEmail         | The email address of a SendSecure user of the current enterprise account
cancellationToken | A cancellation token that can be used by other objects or threads to receive notice of cancellation

### Get Security Profiles
```
SecurityProfilesAsync(userEmail, cancellationToken)
```
Returns the list of all security profiles available to a specific user, with all their setting values/properties.

Param             | Definition
------------------|-----------
userEmail         | The email address of a SendSecure user of the current enterprise account
cancellationToken | A cancellation token that can be used by other objects or threads to receive notice of cancellation

### Initialize SafeBox
```
InitializeSafeboxAsync(safebox, cancellationToken)
```
Pre-creates a SafeBox on the SendSecure system and returns the updated [Safebox](#safebox) object with the necessary system parameters filled out (GUID, public encryption key, upload URL).

Param             | Definition
------------------|-----------
safebox           | A [Safebox](#safebox) object to be initialized by the SendSecure system
cancellationToken | A cancellation token that can be used by other objects or threads to receive notice of cancellation

### Upload Attachment
```
UploadAttachmentAsync(safebox, attachment, cancellationToken)
```
Uploads the specified file as an Attachment of the specified SafeBox and returns the updated [Attachment](#attachment) object with the GUID parameter filled out.

Param             | Definition
------------------|-----------
safebox           | An initialized [Safebox](#safebox) object 
attachment        | An [Attachment](#attachment) object - the file to upload to the SendSecure system
cancellationToken | A cancellation token that can be used by other objects or threads to receive notice of cancellation

### Commit SafeBox
```
CommitSafeboxAsync(safebox, cancellationToken)
```
Finalizes the creation (commit) of the SafeBox on the SendSecure system.
This actually "Sends" the SafeBox with all content and contact info previously specified.

Param             | Definition
------------------|-----------
safebox           | A [Safebox](#safebox) object already initialized, with security profile, recipient(s), subject and message already defined, and attachments already uploaded. 
cancellationToken | A cancellation token that can be used by other objects or threads to receive notice of cancellation

### Submit SafeBox
```
SubmitSafeboxAsync(safebox, cancellationToken)
```
This method is a high-level combo that initializes the SafeBox, uploads all attachments and commits the SafeBox.

Param             | Definition
------------------|-----------
safebox           | A non-initialized [Safebox](#safebox) object with security profile, recipient(s), subject, message and attachments (not yet uploaded) already defined. 
cancellationToken | A cancellation token that can be used by other objects or threads to receive notice of cancellation


## Helper Modules

<a name="safebox"></a>
### Safebox

Builds an object to send all the necessary information to create a new Safebox.

Attribute            | Definition
---------------------|-----------
Guid                 | The unique identifier of the SafeBox (filled by the system once the SafeBox is initialized).
UploadUrl            | The URL used to upload the SafeBox attachments (filled by the system once the SafeBox is initialized).
PublicEncryptionKey  | The key used to encrypt the SafeBox attachments and/or messages (filled by the system once the SafeBox is initialized).
UserEmail            | The email address of the creator of the SafeBox (mandatory).
Subject              | The subject of the SafeBox (optional).
Message              | The initial message of the SafeBox (optional if the SafeBox has at least one attachment).
Recipients           | The list of all [Recipient](#recipient) objects of the SafeBox (mandatory, at least one recipient).
Attachments          | The list of all [Attachment](#attachment) objects of the SafeBox (optional if the SafeBox has a message).
SecurityProfile      | The SecurityProfile object defining security options for the SafeBox (mandatory).

<!-- NotificationLanguage | The language used for email notifications sent to the recipients (optional, English by default).
MISSING -->

### SafeboxResponse

Represents the response to the successful creation of a SafeBox in the SendSecure system.
All attributes are filled by the system once the SafeBox is successfully created (sent).

Attribute            | Definition
---------------------|-----------
Guid                 | The unique identifier of the SafeBox.
PreviewUrl           | The URL to access the SafeBox in the SendSecure Web Portal.
EncryptionKey        | The key that may be required to decrypt the SafeBox content (only if Double Encryption is enabled in the Security Profile).

<a name="recipient"></a>
### Recipient

Builds an object to create a recipient for the SafeBox.

Attribute            | Definition
---------------------|-----------
Email                | The email address of the recipient (mandatory).
FirstName            | The first name of the recipient (optional).
LastName             | The last name of the recipient (optional).
CompanyName          | The company name of the recipient (optional).
ContactMethods       | The list of all [ContactMethod](#contactmethod) objects of the recipient (may be mandatory depending on the Security Profile of the SafeBox).

<a name="contactmethod"></a>
### ContactMethod

Builds an object to create a phone number destination owned by the recipient (as part of the Recipient object attributes).
Any ContactMethod – plus the recipient's email address – will be usable as Security Code delivery means to the recipient.
All attributes are mandatory.

Attribute            | Definition
---------------------|-----------
Destination          | A phone number owned by the recipient.
DestinationType      | The phone number's type (i.e. home/cell/office/other).


<a name="attachment"></a>
### Attachment

Builds an object to be uploaded to the server as attachment of the SafeBox.
Can be created either with a [File Path](#file) or a [Stream](#stream).
All attributes are mandatory.

<a name="filepath"></a>
#### File Path

Attribute            | Definition
---------------------|-----------
Guid                 | The unique identifier of the attachment (filled by the system once the file is uploaded).
ContentType          | The file Content-type (MIME). 
FileName             | The path (full filename) of the file to upload.

<a name="stream"></a>
#### Stream

Attribute            | Definition
---------------------|-----------
Guid                 | The unique identifier of the attachment (filled by the system once the file is uploaded).
ContentType          | The file Content-type (MIME). 
Stream               | The data to upload.
Filename             | The file name.
Size                 | The file size.

### SecurityProfile

Represents the settings of a Security Profile.
The use of specific attributes of this object rather takes place in advanced scenarios.
To know all available attributes, please look in the library.

### EnterpriseSettings

Represents the SendSecure settings of an Enterprise Account.
The use of specific attributes of this object rather takes place in advanced scenarios.
To know all available attributes, please look in the library.

### ExtensionFilter

Represents the list of allowed/forbidden extensions for SafeBox attachments (part of the EnterpriseSettings).
The use of specific attributes of this object rather takes place in advanced scenarios.
To know all available attributes, please look in the library.

<a name="license"></a>
# License

sendsecure-csharp is distributed under [MIT License](https://github.com/xmedius/sendsecure-csharp/blob/master/LICENSE).

<a name="credits"></a>
# Credits

sendsecure-csharp is developed, maintained and supported by [XMedius Solutions Inc.](https://www.xmedius.com?source=sendsecure-csharp)
The names and logos for sendsecure-csharp are trademarks of XMedius Solutions Inc.

![XMedius Logo](https://s3.amazonaws.com/xmc-public/images/xmedius-site-logo.png)
