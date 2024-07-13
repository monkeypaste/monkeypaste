#!/bin/bash
# from https://github.com/ios-control/ios-deploy/issues/588#issuecomment-2042088605
#set -x


###############################################################################

function show_help()
{
  cat <<EOF
Usage:
  $(basename $0) [options]

  options
    -h              - show help and exit

  example:
    ./ios-ebee-deploy.sh      -b "/home/user/Projects/app_project/build_app/Release-iPhoneOS/iPhoneApp.app"
                              -i "iPhone UUID"
                              -p 'password' !!! <-- PLEASE KEEP CARE TO USE '' if password contains special chars --> !!!
                              -l "/home/user/logfile.out"

EOF
}

###############################################################################

while getopts b:i:p:l:h arg; do
  case $arg in
    h)
      show_help
      exit 0
      ;;
    b)
      app_path=${OPTARG}
      ;;
    i)
      device_id=${OPTARG}
      ;;
    p)
      sudo_pw=${OPTARG}
      ;;
    l)
      log_output=${OPTARG}
      ;;
    *)
      if [ $arg == ":" ]; then
        echo "ERROR: Option \"-$OPTARG\" requires an argument."
      else
        echo "ERROR: $arg Unknown option \"-$OPTARG\"."
      fi
      show_help
      exit 1
      ;;
  esac
done

if [ -z "$app_path" ]; then
 echo "Parameter -b is required. See -h for help."
 exit 1
fi

if [ -z "$device_id" ]; then
 echo "Parameter -i was not provided. Use devicectl to find first available iOS device."
 device_id=$(echo `xcrun devicectl list devices | awk '/available \(/ {print($6)}'`)
 if [ "$device_id" != "" ]; then
   echo "Using device id: $device_id"
 else
   echo "Warning: Cannot detect device id! Maybe XCode is running?"
 fi
fi

rm -rf tmp.o
rm -rf lldb.o
rm -rf install.o
rm -rf dynamic_data.sh
rm -rf lldb.commands


device_connected=$(echo `xcrun devicectl list devices | awk '/connected / {print($5)}' | awk -F'.' '{print($1)}'`)
if [ "$device_connected" != "" ]; then
 echo "ERROR: Possible opened XCode instance detected. Please close XCode an restart this script! Make sure selected iPhone is not in 'connected' state (xcrun devicectl list devices)."
 exit 1
fi


curl -sS "127.0.0.1:49151"
exit_code=$?
echo ""
if [ $exit_code -ne 0 ]; then
  if [ $exit_code -eq 56 ]; then
    echo "Tunnel-Server for getting rsd info is running but not reachable! Killing..."
    process_id=$(echo `echo ${sudo_pw} | sudo -S lsof -i:49151 | tail -n1 | awk -F' ' '{print ($2)}'`)
    echo ${sudo_pw} | sudo -S kill -9 ${process_id}
    sleep 5
  fi
fi


server_started=$(echo `nc -vz 127.0.0.1 49151 2>&1 | awk -F ' ' '{print $7}'`)
if [ ${server_started} == "succeeded!" ]; thenfo
  echo "Server already running! Try to get rsd info string."
else
  echo ${sudo_pw} | sudo -S python3 -m pymobiledevice3 remote tunneld > server_start.o &
  sleep 10
fi

device_hostname=$(echo `xcrun devicectl list devices | awk '/available \(/ {print($5)}' | awk -F'.' '{print($1)}'`)
rsd_output="$(echo `curl -s -H "Accept: application/json" 127.0.0.1:49151 | jq -r --arg var "$device_hostname" '.[$var][0]'`) $(echo `curl -s -H "Accept: application/json" 127.0.0.1:49151 | jq -r --arg var "$device_hostname" '.[$var][1]'`)"

echo "Using $rsd_output for --rsd option"

xcrun devicectl device install app --device ${device_id}  ${app_path} > install.o
installation_url=$(cat install.o | grep 'installationURL' | awk -F 'file://' '{print $2}' | tr -d '\r\n' | sed 's/\/$//')

python3 -m pymobiledevice3 developer debugserver start-server --rsd ${rsd_output} > lldb.o &
sleep 5
connection_details=$(cat lldb.o  | grep -o 'connect://\[.*\]:[0-9]*')

echo "app_path=${app_path}" >> dynamic_data.sh
echo "remote_app_path=${installation_url}" >> dynamic_data.sh
echo "connection_details=${connection_details}" >> dynamic_data.sh

source $(dirname $0)/lldb.sh

if [ -z "$log_output" ]; then
 lldb -s lldb.commands

# | grep -v "\(lldb\)" &> ${app_path}/../../output.log
else
 lldb -s lldb.commands | grep -v "\(lldb\)" &> ${log_output}
fi