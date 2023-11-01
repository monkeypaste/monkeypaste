<?php

require_once __DIR__ . '/../../src/lib/bootstrap.php';

function login(string $username, string $password): mixed
{
    $account = find_account_by_username($username);
    if (!$account) {
        exit_w_errors(['username' => 'User not found.']);
    }
    if ($account['active'] == 0) {
        exit_w_errors(['username' => 'Please confirm account before logging in']);
    }

    if (!password_verify($password, $account['password'])) {
        exit_w_error('Login failed');
    }

    return $account;
}

function is_new_device(string $device_guid): bool
{
    $sql = 'SELECT id FROM device WHERE device_guid=:device_guid';

    $statement = db()->prepare($sql);
    $statement->bindValue(':device_guid', $device_guid);
    $statement->execute();

    return $statement->fetchColumn() === false;
}

function add_account_device(
    int $acct_id,
    string $device_guid,
    string $detail1,
    string $detail2,
    string $detail3) {

    $sql = 'INSERT INTO device (fk_account_id, device_guid, detail1, detail2, detail3)
            VALUES (:fk_account_id, :device_guid, :detail1, :detail2, :detail3)';

    $statement = db()->prepare($sql);
    $statement->bindValue(':fk_account_id', $acct_id, PDO::PARAM_INT);
    $statement->bindValue(':device_guid', $device_guid);
    $statement->bindValue(':detail1', $detail1);
    $statement->bindValue(':detail2', $detail2);
    $statement->bindValue(':detail3', $detail3);
    if ($statement->execute()) {
        return;
    }
    exit_w_error('error could not add device');
}

function create_login_response(string $email, mixed $sub): string
{
    $resp_obj = null;
    if ($sub == null) {
        $resp_obj = [
            'email' => $email,
            'sub_type' => 'Free',
            'monthly' => "0",
            'expires_utc_dt' => getMaxDateTimeStr(),
        ];
    } else {
        $resp_obj = [
            'email' => $email,
            'sub_type' => $sub['sub_type'],
            'monthly' => $sub['monthly'] ? "1" : "0",
            'expires_utc_dt' => getFormattedDateTimeStr($sub['expires_utc_dt']),
        ];
    }

    return json_encode($resp_obj);
}

$testdata_w_sub = [
    'username' => 'tkefauver',
    'password' => '*1Password',
    'device_guid' => 'TEST GUID SUBSCRIPTION',
    'detail1' => 'test_detail1',
    'detail2' => 'test_detail2',
    'detail3' => 'test_detail3',
    'sub_type' => 'Standard',
    'monthly' => '1',
    'expires_utc_dt' => '11/29/2023 7:00:00 PM', //getFormattedDateTimeStr(date('Y-m-d H:i:s', time() + 1 * 24 * 60 * 60)),
];

$testdata_no_sub = [
    'username' => 'tkefauver',
    'password' => '*1Password',
    'device_guid' => 'TEST GUID OTHER',
    'detail1' => 'test_detail1',
    'detail2' => 'test_detail2',
    'detail3' => 'test_detail3',
];

$testdata = $testdata_no_sub;

$fields = [
    'username' => 'string | required',
    'password' => 'string | required',
    'device_guid' => 'string | required',
    'detail1' => 'string | required',
    'detail2' => 'string | required',
    'detail3' => 'string | required',
    'sub_type' => 'string',
    'monthly' => 'string',
    'expires_utc_dt' => 'string | datetime',
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
$account = login($inputs['username'], $inputs['password']);

if ($account == null) {
    exit_w_error('error account not found');
}

// login successful

if (is_new_device($inputs['device_guid'])) {
    // new device
    add_account_device(
        $account['id'],
        $inputs['device_guid'],
        $inputs['detail1'],
        $inputs['detail2'],
        $inputs['detail3']);
}

if ($inputs['sub_type'] != null &&
    $inputs['monthly'] != null &&
    $inputs['expires_utc_dt'] != null) {
    // subscription login, update info
    update_subscription(
        $inputs['device_guid'],
        $inputs['sub_type'],
        $inputs['monthly'] == "1",
        $inputs['expires_utc_dt']);
}
$sub = find_subscription_by_acct_id($account['id']);

exit_w_success(create_login_response($account['email'], $sub));
