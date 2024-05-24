<?php

require_once __DIR__ . '/../../src/lib/bootstrap.php';

const LINUX_VER = '1.0.18.0';
const WIN_VER = '1.0.19.0';
const MAC_VER = '1.0.19.0';
const ANDROID_VER = '1.0.19.0';
const IOS_VER = '1.0.19.0';

function exit_w_version_resp(string $device_type) {
     $resp_obj = null;
     switch ($device_type) {
          case 'Windows':
               $resp_obj = ['device_version' => WIN_VER];
               break;
          case 'Mac':
               $resp_obj = ['device_version' => MAC_VER];
               break;
          case 'Linux':
               $resp_obj = ['device_version' => LINUX_VER];
               break;
          case 'Android':
               $resp_obj = ['device_version' => ANDROID_VER];
               break;
          case 'Ios':
               $resp_obj = ['device_version' => IOS_VER];
               break;
          default:
               exit_w_error('unknown device type');
     }
     exit_w_success(json_encode($resp_obj));
}

$testdata = [
     'device_type' => 'Windows',
];

$fields = [
     'device_type' => 'string | required',
];

$errors = [];
$inputs = [];

if (is_post_request()) {
     [$inputs, $errors] = filter($_POST, $fields);
} else {
     if (CAN_TEST && isset($testdata)) {
          [$inputs, $errors] = filter($testdata, $fields);
     } else {
          exit_w_error();
     }
}

if ($errors) {
     exit_w_errors($errors);
}

exit_w_version_resp($inputs['device_type']);
