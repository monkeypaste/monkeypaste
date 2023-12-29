<?php

require_once __DIR__ . '/../../src/lib/bootstrap.php';

const WIN_VER = '1.0.7.0';

function exit_w_version_resp(string $device_type)
{
    $resp_obj = null;
    switch ($device_type) {
        case 'Windows':
            $resp_obj = ['device_version' => WIN_VER];
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
