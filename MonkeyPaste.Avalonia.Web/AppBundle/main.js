﻿import { dotnet } from './dotnet.js'
import { registerAvaloniaModule } from './avalonia.js';

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(true)
    .withApplicationArgumentsFromQuery()
    .create();



await registerAvaloniaModule(dotnetRuntime);

const config = dotnetRuntime.getConfig();

//export dotnetRuntime;

await dotnetRuntime.runMainAndExit(config.mainAssemblyName, [window.location.search]);

