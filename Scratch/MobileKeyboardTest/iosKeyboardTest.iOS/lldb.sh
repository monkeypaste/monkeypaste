source dynamic_data.sh

rm -rf lldb.commands

echo "platform select remote-ios" >> lldb.commands
echo "target create $app_path" >> lldb.commands
echo "script lldb.target.module[0].SetPlatformFileSpec(lldb.SBFileSpec('$remote_app_path'))" >> lldb.commands
echo "script old_debug = lldb.debugger.GetAsync()" >> lldb.commands
echo "script lldb.debugger.SetAsync(True)" >> lldb.commands
echo "process connect $connection_details" >> lldb.commands
echo "script lldb.debugger.SetAsync(old_debug)" >> lldb.commands
echo "process launch" >> lldb.commands
echo "exit" >> lldb.commands