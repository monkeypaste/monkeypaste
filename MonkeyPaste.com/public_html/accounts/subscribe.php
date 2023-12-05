<?php

require_once __DIR__ . '/../../src/lib/bootstrap.php';

$testdata = [
    'device_guid' => 'TEST GUID SUBSCRIPTION',
    'sub_type' => 'Standard',
    'monthly' => '1',
    'expires_utc_dt' => getFormattedDateTimeStr(date('Y-m-d H:i:s', time() + DEFAULT_EXPIRY_OFFSET)),
];

$fields = [
    'device_guid' => 'string | required | unique: subscription, device_guid',
    'sub_type' => 'string | required',
    'monthly' => 'string | required',
    'expires_utc_dt' => 'string | required | datetime',
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

$success =
    add_subscription(
    $inputs['device_guid'],
    $inputs['sub_type'],
    $inputs['monthly'] == "1",
    $inputs['expires_utc_dt']);

if ($success) {
    exit_w_success();
}
exit_w_error('error could not add subscription');
