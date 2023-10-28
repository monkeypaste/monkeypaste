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

(nuget install)
(hello world)

## Parameters


