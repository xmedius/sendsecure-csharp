**XM SendSecure** is a collaborative file exchange platform that is both highly secure and simple to use.
It is expressly designed to allow for the secure exchange of sensitive documents via virtual SafeBoxes.

XM SendSecure comes with a **Web API**, which is **RESTful**, uses **HTTPs** and returns **JSON**.

Specific libraries have been published for various languages:
**C#**,
[Java](https://github.com/xmedius/sendsecure-java),
[JavaScript](https://github.com/xmedius/sendsecure-js),
[PHP](https://github.com/xmedius/sendsecure-php),
[Python](https://github.com/xmedius/sendsecure-python)
and
[Ruby](https://github.com/xmedius/sendsecure-ruby).

# sendsecure-csharp

**This library allows you to use the XM SendSecure Web API via C# with .NET.**

With this library, you will be able to:
* Authenticate SendSecure users
* Create new SafeBoxes

# Table of Contents

* [Installation](#installation)
* [Quick Start](#quickstart)
* [Usage](#usage)
* [License](#license)
* [Credits](#credits)

# Installation

## Prerequisites

- .NET version 4.5.2+
- The XM SendSecure solution, provided by [XMedius](https://www.xmedius.com?source=sendsecure-csharp) (demo accounts available on demand)

## Install Package

To use sendsecure-csharp in your C# project, clone or download the SendSecure C# .NET libraries directly from our GitHub repository.

# Quick Start

## Authentication (Retrieving API Token)

Authentication is done using an API Token, which must be first obtained based on SendSecure enterprise account and user credentials.
Here is the minimum code to get such a user-based API Token.

```csharp
using System;
using System.Collections.Generic;
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
                JsonObjects.UserToken tokenDetail = GetUserTokenAsync().GetAwaiter().GetResult();
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

        public static async Task<JsonObjects.UserToken> GetUserTokenAsync()
        {
            JsonObjects.UserToken tokenDetail = await XMedius.SendSecure.Client.GetUserTokenAsync("deathstar", "darthvader", "d@Rk$1De", "DV-TIE/x1", "TIE Advanced x1", "The Force App", new Uri("https://portal.xmedius.com"));

            Console.WriteLine(tokenDetail.Token);
            Console.WriteLine(tokenDetail.UserId);
            return tokenDetail;
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

                string userId = 123456;

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
            var safebox = new XMedius.SendSecure.Helpers.Safebox("darthvader@empire.com");
            safebox.Subject = "Family matters";
            safebox.Message = "Son, you will find attached the evidence.";

            safebox.Attachments.Add(new XMedius.SendSecure.Helpers.Attachment("d:\\Birth_Certificate.pdf", "application/pdf"));

            var recipient = new XMedius.SendSecure.Helpers.Participant("user@example.com");
            var contactMethod = new XMedius.SendSecure.Helpers.ContactMethod { Destination = "555-232-5334", DestinationType = XMedius.SendSecure.Helpers.ContactMethod.DestinationTypeT.CellPhone };
            recipient.GuestOptions.ContactMethods.Add(contactMethod);
            safebox.Participants.Add(recipient);

            XMedius.SendSecure.Client client = new Client(token, userId, "deathstar", new Uri("https://portal.xmedius.com"));
            XMedius.SendSecure.Helpers.Safebox safeboxResponse = await client.SubmitSafeboxAsync(safebox2);

            Console.WriteLine(safeboxResponse.PreviewUrl);
            return safeboxResponse.PreviewUrl;
        }
    }
}
```

# Usage

## Helper Methods

### Get User Token
```
GetUserTokenAsync(string enterpriseAccount, string username, string password, string deviceId, string deviceName, string applicationType, Uri endpoint, string oneTimePassword, CancellationToken cancellationToken)
```
Creates and returns two properties: an API Token and a user ID, for a specific user within a SendSecure enterprise account.
Calling this method again with the exact same params will always return the same Token.

Param             | Definition
------------------|-----------
enterpriseAccount | The SendSecure enterprise account.
username          | The username of a SendSecure user of the current enterprise account.
password          | The password of this user.
deviceId          | The unique ID of the device used to get the Token.
deviceName        | The name of the device used to get the Token.
applicationType   | The type/name of the application used to get the Token ("SendSecure C#" will be used by default if empty).
endpoint          | The URL to the SendSecure service ("https://portal.xmedius.com" will be used by default if empty).
oneTimePassword   | The one-time password of this user (if any).
cancellationToken | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

### Client Object Constructor
```
Client(string apiToken, int userId, string enterpriseAccount, Uri endpoint, string locale)
```

Param             | Definition
------------------|-----------
apiToken          | The API Token to be used for authentication with the SendSecure service.
enterpriseAccount | The SendSecure enterprise account.
endpoint          | The URL to the SendSecure service ("https://portal.xmedius.com" will be used by default if empty).
locale            | The locale in which the server errors will be returned ("en" will be used by default if empty).
userId            | The user ID, which may be used to manage additional objects directly related to the user (e.g. favorites).

### Enterprise Methods

### Get Enterprise Settings
```
EnterpriseSettingsAsync(CancellationToken cancellationToken)
```
Returns all values/properties of the enterprise account's settings specific to SendSecure.

Param             | Definition
------------------|-----------
cancellationToken | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

### Get Default Security Profile
```
DefaultSecurityProfileAsync(string userEmail, CancellationToken cancellationToken)
```
Returns the default security profile (if it has been set) for a specific user, with all its setting values/properties.

Param             | Definition
------------------|-----------
userEmail         | The email address of a SendSecure user of the current enterprise account.
cancellationToken | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

### Get Security Profiles
```
SecurityProfilesAsync(string userEmail, CancellationToken cancellationToken)
```
Returns the list of all security profiles available to a specific user, with all their setting values/properties.

Param             | Definition
------------------|-----------
userEmail         | The email address of a SendSecure user of the current enterprise account.
cancellationToken | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

### Consent Message Group Methods

#### Get Consent Message (in all locales)
```
GetConsentGroupMessagesAsync(int consentGroupId, CancellationToken cancellationToken)
```
Retrieves the consent message (in all available locales) associated to a Security Profile or a SafeBox, among the available consent messages of the current enterprise account.

Param               | Definition
--------------------|-----------
consentGroupId      | The unique ID of the consent group.
cancellationToken   | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

### SafeBox Creation Methods

### Initialize SafeBox
```
InitializeSafeboxAsync(Safebox safebox, CancellationToken cancellationToken)
```
Pre-creates a SafeBox on the SendSecure system and returns the updated [Safebox](#safebox) object with the necessary system parameters filled out (GUID, public encryption key, upload URL).

Param             | Definition
------------------|-----------
safebox           | A [Safebox](#safebox) object to be initialized by the SendSecure system.
cancellationToken | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

### Upload Attachment
```
UploadAttachmentAsync(Safebox safebox, Attachment attachment, CancellationToken cancellationToken)
```
Uploads the specified file as an Attachment of the specified SafeBox and returns the updated [Attachment](#attachment) object with the GUID parameter filled out.

Param             | Definition
------------------|-----------
safebox           | An initialized [Safebox](#safebox) object.
attachment        | An [Attachment](#attachment) object - the file to upload to the SendSecure system.
cancellationToken | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

### Commit SafeBox
```
CommitSafeboxAsync(Safebox safebox, CancellationToken cancellationToken)
```
Finalizes the creation (commit) of the SafeBox on the SendSecure system.
This actually "Sends" the SafeBox with all content and contact info previously specified.

Param             | Definition
------------------|-----------
safebox           | A [Safebox](#safebox) object already initialized, with security profile, recipient(s), subject and message already defined, and attachments already uploaded.
cancellationToken | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

### Submit SafeBox
```
SubmitSafeboxAsync(Safebox safebox, CancellationToken cancellationToken)
```
This high-level method combines the SafeBox initialization, attachment uploads and the SafeBox commit.

Param             | Definition
------------------|-----------
safebox           | A non-initialized [Safebox](#safebox) object with security profile, recipient(s), subject, message and attachments (not yet uploaded) already defined.
cancellationToken | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

### Safebox Methods

#### Reply
```
ReplyAsync(Safebox safebox, Reply reply, CancellationToken cancellationToken)
```
Replies to a specific SafeBox with the content specified through a Reply object.

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
reply                | A [Reply](#reply-object) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Add Time
```
AddTimeAsync(Safebox safebox, int timeValue, SecurityEnums.TimeUnit timeUnit, CancellationToken cancellationToken)
```
Extends the SafeBox duration by the specified amount of time.

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
timeValue            | The time value, according to the specified unit.
timeUnit             | The time unit. Accepted values:```hours```, ```days```, ```weeks```, ```months```.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Close SafeBox
```
CloseSafeboxAsync(Safebox safebox, CancellationToken cancellationToken)
```
Closes the SafeBox immediately, i.e. before its intended expiration. Only available for SafeBoxes in "open" status.

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Delete SafeBox Content
```
DeleteSafeboxContentAsync(Safebox safebox, CancellationToken cancellationToken)
```
Deletes the SafeBox content immediately, i.e. despite the remaining retention period. Only available for SafeBoxes in "closed" status.

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation

#### Mark SafeBox as Read
```
MarkAsReadAsync(Safebox safebox, CancellationToken cancellationToken)
```
Marks as read all messages within the SafeBox.

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Mark SafeBox as Unread
```
MarkAsUnreadAsync(Safebox safebox, CancellationToken cancellationToken)
```
Marks as unread all messages within the SafeBox.

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Mark Message as Read
```
MarkAsReadMessageAsync(Safebox safebox, Message message, CancellationToken cancellationToken)
```
Marks as read a specific message within the SafeBox.

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
message              | A [Message](#message) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Mark Message as Unread
```
MarkAsUnreadMessageAsync(Safebox safebox, Message message, CancellationToken cancellationToken)
```
Marks as unread a specific message within the SafeBox.

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
message              | A [Message](#message) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Get Audit Record PDF
```
AuditRecordPdfAsync(Safebox safebox, CancellationToken cancellationToken)
```
Gets the Audit Record of the SafeBox.

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Get Audit Record PDF URL
```
AuditRecordUrlAsync(Safebox safebox, CancellationToken cancellationToken)
```
Gets the URL of the Audit Record of the SafeBox.

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Get SafeBox Info
```
SafeboxInfoAsync(Safebox safebox, string[] sections, CancellationToken cancellationToken)
```
Gets all information of the SafeBox, regrouped by sections.

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
sections             | The information sections to be retrieved. Accepted values: ```download_activity```, ```event_history```, ```messages```, ```participants```, ```security_options```.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Get SafeBox Participants
```
ParticipantsAsync(Safebox safebox, CancellationToken cancellationToken)
```
Gets the list of all participants of the SafeBox.

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Get SafeBox Messages
```
MessagesAsync(Safebox safebox, CancellationToken cancellationToken)
```
Gets all the messages of the SafeBox.

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Get SafeBox Security Options
```
SecurityOptionsAsync(Safebox safebox, CancellationToken cancellationToken)
```
Gets all the security options of the SafeBox.

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Get SafeBox Download Activity
```
DownloadActivityAsync(Safebox safebox, CancellationToken cancellationToken)
```
Gets all the download activity information of the SafeBox.

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Get SafeBox Event History
```
EventHistoryAsync(Safebox safebox, CancellationToken cancellationToken)
```
Retrieves the complete event history of the SafeBox.

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

### SafeBox Attached Document Methods

#### Get File URL
```
FileUrlAsync(Safebox safebox, Attachment attachment, CancellationToken cancellationToken)
```
Returns the URL of a document contained in a SafeBox (allowing to download it from the File Server).

Param                | Definition
---------------------|-----------
safebox              | A [Safebox](#safebox) object.
attachment           | An [Attachment](#attachment) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

### Participant Management Methods

#### Create Participant
```
CreateParticipantAsync(Safebox safebox, Participant participant, CancellationToken cancellationToken)
```
Creates a new Participant and adds it to the SafeBox.

Param                | Definition
---------------------|-------------
safebox              | A [Safebox](#safebox) object.
participant          | A [Participant](#participant) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Update Participant
```
UpdateParticipantAsync(Safebox safebox, Participant participant, CancellationToken cancellationToken)
```
Updates an existing participant of the specified SafeBox.

Param                | Definition
---------------------|-------------
safebox              | A [Safebox](#safebox) object.
participant          | The updated [Participant](#participant) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Delete Participant's Contact Methods
```
DeleteParticipantContactMethods(Participant participant, Safebox safebox, List<int?> contactMethodsIds, CancellationToken cancellationToken)
```
Deletes one or several contact methods of an existing participant of the specified SafeBox.

Param                | Definition
---------------------|---------------------
safebox              | A [Safebox](#safebox) object.
participant          | A [Participant](#participant) object.
contactMethodIds     | A list of Contact method unique IDs (see [ContactMethod](#contactMethod) object).
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

### Recipient Methods

#### Search Recipient
```
SearchRecipientAsync(string term, CancellationToken cancellationToken)
```
Returns a list of people (e.g. among Favorites and/or Enterprise Users) that match the specified search term, typically for auto-suggestion when adding participants to a new/existing SafeBox. The returned list depends on the *autocomplete* attributes of the Enterprise Settings.

Param                | Definition
---------------------|-----------
term                 | A string intended to match a portion of name, email address or company.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

### SafeBox List (SafeBoxes) Methods

#### Get SafeBox List
```
SafeboxesAsync(Status status, string search, int per_page, int page, CancellationToken cancellationToken)
SafeboxesAsync(string url, CancellationToken cancellationToken)
SafeboxesAsync(CancellationToken cancellationToken)
```
Returns a SafeboxesResponse object containing the count of found SafeBoxes, the previous page URL, the next page URL and a list of [Safebox](#safebox) objects – for the current user account and according to the specified filtering options.

Param                | Definition
---------------------|-----------
url                  | The search URL.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

Available filtering options:
  * ```status```: to filter by SafeBox status: ```in_progress```, ```closed```, ```content_deleted```, ```unread```.
  * ```search```: to search using a term intended to match a portion of SafeBox subject/ID/message, participant email/first name/last name, attached file name/fingerprint.
  * ```per_page```: to split the list in several pages, with 0 < per_page <= 1000 (default is 100).
  * ```page```: to select the page to return.

Example to return the 1st page with 20 SafeBoxes that are open and unread and that contain the word "Luke":
```
SafeboxesAsync("in_progress,unread", "Luke", 20, 1, cancellationToken)
```

### User Methods

#### Get User Settings
```
UserSettingsAsync(CancellationToken cancellationToken)
```
Retrieves all the SendSecure User Settings for the current user account.

Param                | Definition
---------------------|-----------
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Get Favorites
```
FavoritesAsync(CancellationToken cancellationToken)
```
Retrieves all favorites associated to the current user account.

Param                | Definition
---------------------|-----------
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Create Favorite
```
CreateFavoriteAsync(Favorite favorite, CancellationToken cancellationToken)
```
Creates a new favorite for the current user account.

Param                | Definition
---------------------|-----------
favorite             | A [Favorite](#favorite) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Edit Favorite
```
EditFavoriteAsync(Favorite favorite, CancellationToken cancellationToken)
```
Edits an existing favorite associated to the current user account.

Param                | Definition
---------------------|-----------
favorite             | The updated [Favorite](#favorite) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Delete Favorite's Contact Methods
```
DeleteFavoriteContactMethods(Favorite favorite, List<int?> contactMethodsIds, CancellationToken cancellationToken)
```
Deletes one or several contact methods of an existing favorite associated to the current user account.

Param                | Definition
---------------------|-----------
favorite             | A [Favorite](#favorite) object.
contactMethodIds     | A list of contact method unique IDs (see [ContactMethod](#contactMethod) object).
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

#### Delete Favorite
```
DeleteFavoriteAsync(Favorite favorite, CancellationToken cancellationToken)
```
Delete an existing favorite associated to the current user account.

Param                | Definition
---------------------|-----------
favorite             | A [Favorite](#favorite) object.
cancellationToken    | A cancellation token that can be used by other objects or threads to receive notice of cancellation.

## Helper Objects
Here is the alphabetical list of all available objects, with their attributes.

### Attachment

Builds an object to be uploaded to the server as attachment of the SafeBox.
Can be created either with a [File Path](#file) or a [Stream](#stream).
All attributes are mandatory.

#### File Path

Attribute            | Definition
---------------------|-----------
Guid                 | The unique identifier of the attachment (filled by the system once the file is uploaded).
ContentType          | The file Content-type (MIME).
FileName             | The path (full filename) of the file to upload.

#### Stream

Attribute            | Definition
---------------------|-----------
Guid                 | The unique identifier of the attachment (filled by the system once the file is uploaded).
ContentType          | The file Content-type (MIME).
Stream               | The data to upload.
Filename             | The file name.
Size                 | The file size.

### ConsentMessage
Builds an object to retrieve a consent message in a specific locale.
Subset of [ConsentMessageGroup](#consentMessageGroup) (regrouping all locales of a same consent message).
All attributes are read only.

Attribute            | Definition
---------------------|-----------
Locale               | The locale in which the consent message will be returned.
Value                | The text of the consent message.
CreatedAt            | The creation date of the consent message.
UpdatedAt            | The last modification date of the consent message.

### ConsentMessageGroup
Builds an object to retrieve all localized versions of the same consent message.
All attributes are read only.

Attribute            | Definition
---------------------|-----------
Id                   | The unique ID of the consent message group.
Name                 | The name of the consent message group.
CreatedAt            | The creation date of the consent message group.
UpdatedAt            | The last modification date of the consent message group.
ConsentMessages      | The list of [ConsentMessage](#consentMessage) objects (one per available locale).

### ContactMethod
Builds an object to create a phone number destination owned by a participant or a favorite (or retrieve the contact method information).
May be a subset of [GuestOptions](#guestOptions) or [Favorite](#favorite).
Any ContactMethod – plus the email address – will be usable as Security Code delivery means to the participant.

Attribute            | Definition
---------------------|-----------
Destination          | (mandatory) A phone number owned by the participant.
DestinationType      | (mandatory) The phone number's type (i.e. home/cell/office/other).
Id                   | (read only) The unique ID of the contact method.
Verified             | (read only) Indicates whether the contact method was verified by the SendSecure system or not (through authentication mechanism).
CreatedAt            | (read only) The creation date of the contact method.
UpdatedAt            | (read only) The last modification date of the contact method.

### DownloadActivity
Builds an object with all download activity information of all participants of an existing SafeBox.
Subset of [Safebox](#safebox) object.
All attributes are read only.

Attribute            | Definition
---------------------|-----------
Guests               | The list of [DownloadActivityDetail](#downloadActivityDetail) objects associated with each SafeBox participant other than the Owner.
Owner                | The [DownloadActivityDetail](#downloadActivityDetail) object associated with the SafeBox Owner.

### DownloadActivityDetail
Builds an object with all the download activity details for a specific participant of the SafeBox.
Subset of [DownloadActivity](#DownloadActivity).
All attributes are read only.

Attribute            | Definition
---------------------|-----------
Id                   | The unique ID of the download activity detail.
Documents            | The list of [DownloadActivityDocument](#downloadActivityDocument) objects associated with the SafeBox participant.

### DownloadActivityDocument
Builds an object with all the download activity informations for a specific document regarding a specific participant of the SafeBox.
Subset of [DownloadActivityDetail](#downloadActivityDetail).
All attributes are read only.

Attribute            | Definition
---------------------|-----------
Id                   | The unique ID of the download activity document.
DownloadedBytes      | The number of bytes of the document that were actually downloaded.
DownloadedDate       | The date of the download.

### EnterpriseSettings
Builds an object with the SendSecure settings of an Enterprise Account.
All attributes are read only.

Attribute                         | Definition
----------------------------------|--------------------
DefaultSecurityProfileId          | The unique ID of the default security profile of the enterprise.
PdfLanguage                       | The language in which all SafeBox Audit Records are generated.
UsePdfaAuditRecords               | Indicates whether the Audit Records are generated as PDF/A or not.
InternationalDialingPlan          | The country/dialing plan used for formatting national numbers when sending information by phone/SMS.
ExtensionFilter                   | The [ExtensionFilter](#extensionFilter) object associated with the enterprise account.
VirusScanEnabled                  | Indicates whether the virus scan is applied or not when uploading files to SafeBoxes.
MaxFileSizeValue                  | The maximum file size allowed for a SafeBox attachment.
MaxFileSizeUnit                   | The unit of the maximum file size value.
IncludeUsersInAutocomplete        | Indicates whether the users of the enterprise account should be included or not in recipient automatic suggestion.
IncludeFavoritesInAutocomplete    | Indicates whether the favorites of the current user should be included or not in recipient automatic suggestion.
UsersPublicUrl                    | Indicates whether the Personal Secure Links are allowed or not for the users of the enterprise account.
CreatedAt                         | The creation date of the enterprise settings.
UpdatedAt                         | The last modification date of the enterprise settings.

### EventHistory
Builds an object with all Event History information of a SafeBox.
Subset of [Safebox](#safebox) object.
All attributes are read only.

Attribute            | Definition
---------------------|-----------
Type                 | The type of the event.
Date                 | The date of the event.
Metadata             | An object containing all available metadata according to the type of event.
Message              | The complete message describing the event, localized according to the current user locale.

### ExtensionFilter
Builds an object with the list of allowed or forbidden extensions for SafeBox attachments.
Subset of [EnterpriseSettings](#enterpriseSettings).
All attributes are read only.

Attribute            | Definition
---------------------|-----------
Mode                 | Indicates whether the attachments extensions are allowed or forbidden.
List                 | The list of allowed/forbidden extensions for SafeBox attachments.

### Favorite
Builds an object to create a favorite for a user (or retrieve favorite information).

Attribute            | Definition
---------------------|-----------
Email                | (mandatory) The email address of the favorite.
FirstName            | (optional) The first name of the favorite.
LastName             | (optional) The last name of the favorite.
CompanyName          | (optional) The company name of the favorite.
ContactMethods       | (contextual\*) The list of all [ContactMethod](#contactMethod) objects of the favorite.
OrderNumber          | (optional) The ordering number of the favorite among the other favorites.
Id                   | (read only) The unique ID of the favorite.
CreatedAt            | (read only) The creation date of the favorite.
UpdatedAt            | (read only) The last modification date of the favorite.

\* May be mandatory in the case the favorite is added as participant to a SafeBox requiring other contact methods than email.

### GuestOptions
Builds an object to create a subset of additional attributes for the Participant (or retrieve participant information).
Subset of [Participant](#Participant).

Attribute             | Definition
----------------------|-----------
CompanyName           | (optional) The company name of the participant.
Locked                | (optional) Indicates whether the participant access to the SafeBox was revoked or not.
ContactMethods        | (contextual\*) The list of all [ContactMethod](#contactMethod) objects of the participant.
BouncedEmail          | (read only) Indicates if a NDR was received by the system after sending the invitation email to the participant.
FailedLoginAttempts   | (read only) The count of the participant failed login attempts.
Verified              | (read only) Indicated whether the participant email address was verified by the SendSecure system or not (through authentication mechanism).
CreatedAt             | (read only) The creation date of the GuestOptions.
UpdatedAt             | (read only) The last modification date of the GuestOptions.

\* May be mandatory depending on the Security Profile of the SafeBox.

### Message
Builds an object to retrieve a specific message from an existing SafeBox.
Subset of [Safebox](#safebox) object.
All attributes are read only.

Attribute            | Definition
---------------------|-----------
Id                   | The unique ID of the message.
Note                 | The text of the message.
NoteSize             | The size (character count) of the message.
Read                 | Indicates whether the message was read or not.
AuthorId             | The unique ID of the message author.
AuthorType           | The participant type (Owner or other) of the author of the message, regarding the SafeBox.
CreatedAt            | The creation date of the message.
Documents            | The list of all [MessageDocument](#messageDocument) objects representing the attachments of the message.

### MessageDocument
Builds an object to retrieve all information of a specific document (file) from a message within an existing SafeBox.
Subset of [Message](#message) object.
All attributes are read only.

Attribute            | Definition
---------------------|-----------
Id                   | The unique ID of the file.
Name                 | The file name.
Sha                  | The fingerprint (SHA-256) of the file.
Size                 | The file size.
Url                  | The URL of the file.

### Participant
Builds an object to create a participant for the SafeBox (or retrieve participant information).
Subset of [Safebox](#Safebox) object.

Attribute            | Definition
---------------------|-----------
Email                | (mandatory) The email address of the participant.
FirstName            | (optional) The first name of the participant.
LastName             | (optional) The last name of the participant.
GuestOptions         | (optional) The [GuestOptions](#guestOptions) object defining the additional attributes for the participant.
Id                   | (read only) The unique ID of the participant.
Type                 | (read only) The type of the participant (Owner or other) in the SafeBox.
Role                 | (read only) The role of the participant (in terms of permissions) in the SafeBox.
MessageReadCount     | (read only) The count of read messages of the participant.
MessageTotalCount    | (read only) The total count of messages of the participant.

### PersonalSecureLink
Builds an object to retrieve information about the Personal Secure Link of the current user.
Subset of [UserSettings](#UserSettings).
All attributes are read only.

Attribute              | Definition
-----------------------|-----------
Enabled                | Indicates whether the Personal Secure Link of the user is enabled or not.
Url                    | The URL of the Personal Secure Link of the user.
SecurityProfileId      | The ID of the Security Profile used by the Secure Link.

### Reply object
Builds an object to create and post a reply within the SafeBox.

Attribute            | Definition
---------------------|-----------
Message              | (contextual\*) The message of the Reply.
Attachments          | (contextual\*) The list of all [Attachment](#attachment) objects of the Reply.
Consent              | (contextual\*\*) The consent acceptance flag.
DocumentIds         | (auto-filled) The list of the attachment IDs.

\* A message is mandatory if no attachments are provided, and at least one attachment is required if no message is provided.
\*\* Consent acceptance may be mandatory depending on the Security Profile of the SafeBox.

### Safebox
Builds an object to create a new SafeBox or get all information of an existing SafeBox.
Once the SafeBox is created, all attributes are no longer editable.

Attribute                  | Definition
---------------------------|-----------
Guid                       | (auto-filled) The unique ID of the SafeBox (available once the SafeBox is initialized).
UploadUrl                  | (auto-filled) The URL used to upload the SafeBox attachments (available once the SafeBox is initialized). *Note: this attribute is deprecated.*
PublicEncryptionKey        | (auto-filled) The key used to encrypt the SafeBox attachments and/or messages (available once the SafeBox is initialized, only when Double Encryption is enabled).
UserEmail                  | (mandatory) The email address of the creator of the SafeBox.
Subject                    | (optional) The subject of the SafeBox.
Message                    | (contextual\*) The initial message of the SafeBox.
Attachments                | (contextual\*) The list of all [Attachment](#attachment) objects of the SafeBox.
Participants               | (mandatory) The list of all [Participant](#participant) objects of the SafeBox (at least one recipient).
SecurityProfileId          | (optional\*\*) The ID of the Security Profile used to create the Security Options of the SafeBox (see [SecurityProfile](#securityprofile) and [SecurityOptions](#securityoptions) objects).
NotificationLanguage       | (mandatory) The language used for email notifications sent to the recipients.
UserId                     | (read only) The unique ID of the user account of the SafeBox Owner.
EnterpriseId               | (read only) The unique ID of the enterprise account of the SafeBox Owner.
Status                     | (read only) The current status of the SafeBox (life cycle).
SecurityProfileName        | (read only) The name of the Security Profile that was used to create the SafeBox.
UnreadCount                | (read only) The total count of the unread messages within the SafeBox.
DoubleEncryptionStatus     | (read only) The current encryption status of the SafeBox content (i.e. deciphered or key required).
AuditRecordPdf             | (read only) The URL of the Audit Record PDF (available after the SafeBox is closed).
SecureLink                 | (read only) The URL of the Secure Link that was used to create the SafeBox (when applicable).
SecureLinkTitle            | (read only) The Display Name of the Secure Link that was used to create the SafeBox (when applicable).
EmailNotificationEnabled   | (optional) Indicates whether email notifications are enabled for the SafeBox Owner or not (enabled by default, can be disabled for example in a context of SafeBox automated creation by a system).
PreviewUrl                 | (read only) The URL of the SafeBox in the SendSecure Web application.
EncryptionKey              | (read only) The encryption key intended for SafeBox participants (when Double Encryption is enabled). It is returned only once at SafeBox creation and then discarded for security reasons.
CreatedAt                  | (read only) The date on which the SafeBox was created.
UpdatedAt                  | (read only) The date of last modification of the SafeBox.
AssignedAt                 | (read only) The date on which the SafeBox was assigned to the Owner (useful in context of creation via Secure Link).
LatestActivity             | (read only) The date of the latest activity that occurred in the SafeBox.
Expiration                 | (read only) The date on which the SafeBox is expected to auto-close.
ClosedAt                   | (read only) The date on which the SafeBox was closed.
ContentDeletedAt           | (read only) The date on which the content of the SafeBox was deleted.
SecurityOptions            | (read only) The [SecurityOptions](#securityOptions) object, containing the whole set of Security Options of the SafeBox.
Messages                   | (read only) The list of all [Message](#message) objects of the SafeBox.
DownloadActivity           | (read only) The [DownloadActivity](#downloadActivity) object keeping track of all downloads of the SafeBox.
EventHistory               | (read only) The [EventHistory](#eventHistory) object keeping track of all events of the SafeBox.

\* A message is mandatory if no attachments are provided, and at least one attachment is required if no message is provided.
\*\* A Security Profile is always required to create a SafeBox. If no Security Profile ID is specified, the default Security Profile associated to the user will be used.

### SecurityOptions
Builds an object to specify the security options at SafeBox creation, according to the permissions defined in the Security Profile specified in the SafeBox object.
Subset of [Safebox](#safebox) object.
By default, all attribute values are inherited from the Security Profile.
Once the SafeBox is created, all attributes are no longer editable.

Attribute                  | Definition
---------------------------|-----------
ReplyEnabled               | (optional) Indicates whether participants can reply or not to a SafeBox.
GroupReplies               | (optional) Indicates whether the Guest Participants can see each other or not in the SafeBox.
RetentionPeriodType        | (optional) The SafeBox content retention type applied when the SafeBox is closed. Accepted values: ```discard_at_expiration```, ```retain_at_expiration```, ```do_not_discard```.
RetentionPeriodValue       | (optional) The value of the retention period.
RetentionPeriodUnit        | (optional) The unit of the retention period. Accepted values: ```hours```, ```days```, ```weeks```, ```months```, ```years```.
EncryptMessage             | (optional) Indicates whether the messages within the SafeBox will be encrypted or not.
DoubleEncryption           | (optional) Indicates whether Double Encryption is enabled or not.
ExpirationValue            | (contextual\*) The value of the SafeBox open period duration (after which the SafeBox is auto-closed).
ExpirationUnit             | (contextual\*) The unit of the SafeBox open period duration. Accepted values: ```hours```, ```days```, ```weeks```, ```months```.
ExpirationDate             | (contextual\*) The auto-close date of the SafeBox.
ExpirationTime             | (contextual\*) The auto-close time of the SafeBox.
ExpirationTimeZone         | (contextual\*) The time zone of the auto-close date & time of the SafeBox.
SecurityCodeLength         | (read only) The length (number of digits) of the security code sent to participants.
CodeTimeLimit              | (read only) The validity period of the security code once it is sent to the participant.
AllowedLoginAttempts       | (read only) The number of login attempts that are allowed, beyond which the participant access is automatically revoked.
AllowRememberMe            | (read only) Indicates whether the participant can be remembered or not on the device used to access the SafeBox.
AllowSms                   | (read only) Indicates whether the security code can be sent by SMS or not.
AllowVoice                 | (read only) Indicates whether the security code can be sent by voice call or not.
AllowEmail                 | (read only) Indicates whether the security code can be sent by email or not.
TwoFactorRequired          | (read only) Indicates whether a security code is required or not to authenticate the participant.
AutoExtendValue            | (read only) The value of the SafeBox open period auto-extension when a reply is posted near the SafeBox closing date.
AutoExtendUnit             | (read only) The unit of the SafeBox open period auto-extension. Accepted values: ```hours```, ```days```, ```weeks```, ```months```.
AllowManualDelete          | (read only) Indicates whether the content can be manually deleted or not after the SafeBox is closed.
AllowManualClose           | (read only) Indicates whether the SafeBox can be manually closed or not.
EncryptAttachments         | (read only) This attribute is always set to true: attachments are actually always encrypted.
ConsentGroupId             | (read only) The unique ID of the ConsentMessageGroup (see [ConsentMessageGroup](#consentMessageGroup) object).

\* The expiration information (SafeBox auto-close) can be set by specifying either a delay (value + unit) or an actual date (date + time + time zone).

### SecurityProfile
Represents the settings of a Security Profile.
All attributes are read only.
All attributes are composed of two properties: value and modifiable. The value field is as described below and the modifiable field indicates whether the value can be modified or not (let the user choose) at SafeBox creation.

Attribute                  | Definition
---------------------------|-----------
Id                         | The unique ID of the Security Profile.
Name                       | The name of the Security Profile.
Description                | The description of the Security Profile.
CreatedAt                  | The Security Profile creation date.
UpdatedAt                  | The Security Profile last modification date.
AllowedLoginAttempts       | The number of login attempts that are allowed, beyond which the participant access is automatically revoked.
AllowRememberMe            | Indicates whether the participant can be remembered or not on the device used to access the SafeBox.
AllowSms                   | Indicates whether the security code can be sent by SMS or not.
AllowVoice                 | Indicates whether the security code can be sent by voice call or not.
AllowEmail                 | Indicates whether the security code can be sent by email or not.
CodeTimeLimit              | The validity period of the security code once it is sent to the participant.
CodeLength                 | The length (number of digits) of the security code sent to participants.
AutoExtendValue            | The value of the SafeBox open period auto-extension when a reply is posted near the SafeBox closing date.
AutoExtendUnit             | The unit of the SafeBox open period auto-extension. Accepted values: ```hours```, ```days```, ```weeks```, ```months```.
TwoFactorRequired          | Indicates whether a security code is required or not to authenticate the participant.
EncryptAttachments         | This attribute is always set to true: attachments are actually always encrypted.
EncryptMessage             | Indicates whether the messages within the SafeBox will be encrypted or not.
ExpirationValue            | The value of the SafeBox open period duration (after which the SafeBox is auto-closed).
ExpirationUnit             | The unit of the SafeBox open period duration. Accepted values: ```hours```, ```days```, ```weeks```, ```months```.
ReplyEnabled               | Indicates whether participants can reply or not to a SafeBox.
GroupReplies               | Indicates whether the Guest Participants can see each other or not in the SafeBox.
DoubleEncryption           | Indicates whether Double Encryption is enabled or not.
RetentionPeriodType        | The SafeBox content retention type applied when the SafeBox is closed. Accepted values: ```discard_at_expiration```,  ```retain_at_expiration```, ```do_not_discard```.
RetentionPeriodValue       | The value of the retention period.
RetentionPeriodUnit        | The unit of the retention period. Accepted values: ```hours```, ```days```, ```weeks```, ```months```, ```years```.
AllowManualDelete          | Indicates whether the content can be manually deleted or not after the SafeBox is closed.
AllowManualClose           | Indicates whether the SafeBox can be manually closed or not.
AllowForSecureLinks        | Indicates whether the Security Profile can be used or not for Secure Links.
UseCaptcha                 | Indicates whether a verification through Captcha is required for the Secure Link.
VerifyEmail                | Indicates whether the verification of the email of the Secure Link user is required or not.
DistributeKey              | Indicates whether a copy of the participant key will be sent or not by email to the SafeBox Owner when Double Encryption is enabled.
ConsentGroupId             | The unique ID of the ConsentMessageGroup (see [ConsentMessageGroup](#consentMessageGroup) object).

### UserSettings
Builds an object to retrieve the SendSecure options of the current user.
All attributes are read only.

Attribute                  | Definition
---------------------------|-----------
MaskNote                   | Indicates whether the user wants the messages to be masked or not when accessing a SafeBox with message encryption enabled.
OpenFirstTransaction       | Indicates whether the user wants the contents of the first SafeBox in the list to be automatically displayed or not when accessing the SendSecure interface.
MarkAsRead                 | Indicates whether the user wants the unread messages to be automatically marked as read or not when accessing a SafeBox.
MarkAsReadDelay            | The delay (in seconds) after which the messages are automatically marked as read.
RememberKey                | Indicates whether the user accepts or not that the participant key is remembered on the client side to allow subsequent accesses to SafeBoxes having Double Encryption enabled.
DefaultFilter              | The default SafeBox list filter as defined by the user.
RecipientLanguage          | The language in which the user needs the SafeBox recipients to be notified by email and access the SafeBox on their side.
SecureLink                 | The [PersonnalSecureLink](#personalSecureLink) object representing the Personal Secure Link information of the user.
CreatedAt                  | The creation date of the user settings.
UpdatedAt                  | The last modification date of the user settings.

# License

sendsecure-csharp is distributed under [MIT License](https://github.com/xmedius/sendsecure-csharp/blob/master/LICENSE).

# Credits

sendsecure-csharp is developed, maintained and supported by [XMedius Solutions Inc.](https://www.xmedius.com?source=sendsecure-csharp)
The names and logos for sendsecure-csharp are trademarks of XMedius Solutions Inc.

![XMedius Logo](https://s3.amazonaws.com/xmc-public/images/xmedius-site-logo.png)
