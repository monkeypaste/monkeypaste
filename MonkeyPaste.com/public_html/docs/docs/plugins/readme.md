[![NuGet version (MonkeyPaste.Common.Plugin)](https://img.shields.io/nuget/v/MonkeyPaste.Common.Plugin)](https://www.nuget.org/packages/MonkeyPaste.Common.Plugin/)

## About
MonkeyPaste is a clipboard automation and productivity tool. MonkeyPaste.Common.Plugin allows for extensibility through simple request/response messaging. 


## Getting started


### Prerequisites

- [MonkeyPaste](https://www.monkeypaste.com/download)

## Usage

### Minimal 
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

### Examples

Examples about how to use your package by providing code snippets/example images, or samples links on GitHub if applicable. 

- Provide sample code using code snippets
- Include screenshots, diagrams, or other visual help users better understand how to use your package

## Additional documentation

Provide links to more resources: List links such as detailed documentation, tutorial videos, blog posts, or any other relevant documentation to help users get the most out of your package.

## Feedback

Where and how users can leave feedback?

- Links to a GitHub repository where could open issues, Twitter, a Discord channel, bug tracker, or other platforms where a package consumer can connect with the package author.