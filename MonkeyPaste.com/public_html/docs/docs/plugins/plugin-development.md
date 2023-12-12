# Plugin Development

## Overview

MonkeyPaste plugins use a very simple client/server style request and response convention for all plugin interaction. Where MonkeyPaste is the *client* and the plugin acts as a *server*. Plugins for MonkeyPaste's concerns are *stateless* in nature which keeps the interface as simple and lightweight as possible.

## Getting Started

:::note
For simplicity this assumes you are on Windows (10 or higher) and have an instance of [Visual Studio 2022](https://visualstudio.microsoft.com/vs/community/) already installed. But plugins can be created on Mac or Linux with [VS Code](https://code.visualstudio.com/download) and [OmniSharp](http://www.omnisharp.net/) for free.
:::

Add the MonkeyPaste.Common.Plugin dll from nuget or the cli:
```
dotnet add package MonkeyPaste.Common.Plugin
```

:::info 
Javascript and python plugin wrappers are currently in an alpha-stage of development. Check back at the [repo](https://github.com/monkeypaste) for more updates!
:::

### Minimal Example

#### Code

```c#
using MonkeyPaste.Common.Plugin;

namespace MinimalExample {
    public class MinimalExample : MpIAnalyzeComponent {
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            return new MpAnalyzerPluginResponseFormat() {
                dataObjectLookup = new Dictionary<string, object>() {
                    {"Text", "Hello World!" }
                }
            };
        }
    }
}
```
#### Manifest.json
Every plugin must have a `manifest.json` file included in its bundle. At a minimum it provides basic meta and package information. But will also include rules for the types of content and parameters it can handle.
```json
{
    "title": "Hello World",
    "description": "Outputs 'Hello World' as a new text clip",
    "version": "1.0",
    "author": "Thomas Kefauver",
    "guid": "aa4ceef6-e050-4ed5-b308-7c99942436c3",
    "projectUrl": "https://github.com/codebude/QRCoder/",
    "iconUri": "icon.png",
    "packageType": "Dll",
    "tags": "Core, Qr Code, Text, Image, Link, Converter"
}
```
 Required Fields:

| Field | Detail|
| --- | --- | 
| title | Any name is fine but it must have one |
| guid | A unique id for the plugin that should match the format in the example. I use [this](https://www.guidgenerator.com/online-guid-generator.aspx) online generator but it just needs to be sufficiently unique. |


#### Folder Structure

```
MinimalExample/
    MinimalExample.dll
    manifest.json
    icon.png
```
The only requirements are that the `manifest.json` and plugin assembly (whichever references `MpIAnalyzeComponent` or `MpIAnalyzeComponentAsync`) must be in the root folder and the root folder name must match the plugin assembly name.

#### Testing
Your plugin will be added loaded automatically on startup once the plugin folder (`MinimalExample/`) is in MonkeyPaste's root plugin folder found at `C:\Users\<username>\AppData\Local\MonkeyPaste_DEBUG\Plugins` or by clicking the 📁 button in the Plugin Browser and then restarting the application.

You will get toast notifications of any issues initializing the plugin and some will allow you to fix and retry the errors. 

Beyond loading, debugging can be crudely handled using `errorMessage` or `userNotifications` properties in the `MpAnalyzerPluginResponseFormat` that will be displayed as toast messages.

#### Publishing
For the time being you can fork https://github.com/monkeypaste/mp-plugin-list and do a PR on it by adding your `manifest.json` to the array in `ledger.json`. 

## Samples
(Table of sample plugins)

## Feedback
Feel free to raise an issue at (plugin repo link)



