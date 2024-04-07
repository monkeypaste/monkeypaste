## Warning! 

Many of these scripts:
1. are not used or out-of-date. 
2. Contain absolute paths you may need to adjust for you needs. Just search for 'tkefauver'.

## Remarks

### Windows
- When changing build configurations I suggest running [reset_all.bat](./scripts/windows/reset_all.bat) which deletes all bin/obj folders from solution. Then restarting Visual Studio.
- Sometimes debugging Drag-and-Drop can freeze Visual Studio and the app, running [kill_mpav_processes_manual.bat](./scripts/windows/kill_mpav_processes_manual.bat) will unfreeze it.
- All other Windows scripts are currently unused and now handled using the Ledgerizer console app

### Mac
- I highly suggest remote debugging from Windows (and disabling 'Just My Code' in options) and avoid using VS Code/Visual Studio for Mac. Avalonia has a great tutorial for setting up remote debugging found [here](https://github.com/AvaloniaUI/Avalonia/wiki/Remotely-debugging-AvaloniaUI-on-Linux-OSX).
- The [debug.sh](./scripts/mac/sugarwv/debug.sh) is designed for remote debugging but you can use [bundle-osx-x64.sh](./scripts/mac/sugarwv/bundle-osx-x64.sh) independantly.