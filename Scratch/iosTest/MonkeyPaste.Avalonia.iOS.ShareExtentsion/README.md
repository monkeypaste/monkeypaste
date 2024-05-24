Add the next code to your app project:

```xml
<ItemGroup>
    <ProjectReference Include="..\MonkeyPaste.Avalonia.iOS.ShareExtentsion\MonkeyPaste.Avalonia.iOS.ShareExtentsion.csproj">
        <IsAppExtension>true</IsAppExtension>
        <IsWatchApp>false</IsWatchApp>
    </ProjectReference>
</ItemGroup>
```