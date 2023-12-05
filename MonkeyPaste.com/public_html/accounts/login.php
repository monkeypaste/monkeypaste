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
function is_device_activated(int $acct_id, string $device_guid): bool
{
    $sql = 'SELECT * FROM device WHERE fk_account_id=:fk_account_id AND device_guid=:device_guid';

    $statement = db()->prepare($sql);
    $statement->bindValue(':fk_account_id', $acct_id, PDO::PARAM_INT);
    $statement->bindValue(':device_guid', $device_guid);
    $statement->execute();

    $device = $statement->fetch(PDO::FETCH_ASSOC);
    if (!$device) {
        exit_w_error('System error');
    }
    return $device['active'] == 1;
}

function is_new_device(int $acct_id, string $device_guid): bool
{
    $sql = 'SELECT id FROM device WHERE fk_account_id=:fk_account_id AND device_guid=:device_guid';

    $statement = db()->prepare($sql);
    $statement->bindValue(':fk_account_id', $acct_id, PDO::PARAM_INT);
    $statement->bindValue(':device_guid', $device_guid);
    $statement->execute();

    return $statement->fetchColumn() === false;
}

function is_new_platform(int $acct_id, string $device_type): bool
{
    $sql = 'SELECT id FROM device WHERE fk_account_id=:fk_account_id AND device_type=:device_type';

    $statement = db()->prepare($sql);
    $statement->bindValue(':fk_account_id', $acct_id, PDO::PARAM_INT);
    $statement->bindValue(':device_type', $device_type);
    $statement->execute();

    return $statement->fetchColumn() === false;
}

function exit_w_forget_device_email_and_err(mixed $acct, string $device_type, int $expiry = DEFAULT_EXPIRY_OFFSET)
{
    // update device record w/ new forget code
    $acct_id = $acct['id'];
    $forget_device_code = generate_activation_code();
    $sql = 'UPDATE device
            SET forget_code=:forget_code, forget_expiry=:forget_expiry
            WHERE fk_account_id=:fk_account_id AND device_type=:device_type';

    $statement = db()->prepare($sql);
    $statement->bindValue(':forget_code', password_hash($forget_device_code, PASSWORD_DEFAULT));
    $statement->bindValue(':forget_expiry', date('Y-m-d H:i:s', time() + $expiry));
    $statement->bindValue(':fk_account_id', $acct_id, PDO::PARAM_INT);
    $statement->bindValue(':device_type', $device_type);

    if (!$statement->execute()) {
        // error
        exit_w_error('device login failed');
    }

    // send forget email
    send_forget_device_email($acct['username'], $acct['email'], $device_type, $forget_device_code);

    // exit w/ error ntf
    exit_w_error('Login Failed. Only 1 device can be associated with your account per platform and you already have a device registered for "' . $device_type . '". To use this device, please click the link in the "Forget Device" email you have just been sent. Then you will be able to login with this device.');
}

function get_forget_device_email_msg_html(string $username, string $forget_url, string $device_type): string
{
    $msg = "Hi " . $username . ", <br><br>This was sent on your behalf because an attempt to login to your MonkeyPaste account was made on an unknown device from an already registered platform.<br><br>Please click <a href='" . $forget_url . "'>here</a> to remove your existing <b>" . $device_type . "</b> device from your account in order to use a new one.<br><br>* The link will expire in 24 hours";
    return $msg;
}

function send_forget_device_email(string $username, string $email, string $device_type, string $forget_code): void
{
    $forget_link = ACCOUNTS_URL . "/forget-device.php?username=$username&device_type=$device_type&forget_device_code=$forget_code";

    $subject = "Did you get a new device?";
    $message = get_forget_device_email_msg_html($username, $forget_link, $device_type);
    // email header

    $headers = "From: " . strip_tags(NO_REPLY_EMAIL_ADDRESS) . "\r\n";
    $headers .= "Reply-To: " . strip_tags(NO_REPLY_EMAIL_ADDRESS) . "\r\n";
    $headers .= "MIME-Version: 1.0\r\n";
    $headers .= "Content-Type: text/html; charset=UTF-8\r\n";

    // send the email
    mail($email, $subject, $message, $headers);
}

function is_initial_device(int $acct_id): bool
{
    $sql = 'SELECT count(id) AS device_count FROM device WHERE fk_account_id=:fk_account_id';

    $statement = db()->prepare($sql);
    $statement->bindValue(':fk_account_id', $acct_id, PDO::PARAM_INT);
    $statement->execute();
    $result = $statement->fetch(PDO::FETCH_ASSOC);
    return $result['device_count'] == 0;
}
function get_add_device_email_msg_html(string $username, string $activate_url, string $device_type): string
{
    $msg = "Hi " . $username . ", <br><br>Please click <a href='" . $activate_url . "'>here</a> to add your <b>" . $device_type . "</b> device to your account.<br><br>* This link will never expire";
    return $msg;
}

function send_add_device_email(string $username, string $email, string $device_type, string $device_guid, string $activation_code): void
{
    $activate_link = ACCOUNTS_URL . "/activate-device.php?username=$username&device_type=$device_type&device_guid=$device_guid&activation_code=$activation_code";

    $subject = "Confirm added device";
    $message = get_add_device_email_msg_html($username, $activate_link, $device_type);
    $headers = "From: " . strip_tags(NO_REPLY_EMAIL_ADDRESS) . "\r\n";
    $headers .= "Reply-To: " . strip_tags(NO_REPLY_EMAIL_ADDRESS) . "\r\n";
    $headers .= "MIME-Version: 1.0\r\n";
    $headers .= "Content-Type: text/html; charset=UTF-8\r\n";

    // send the email
    mail($email, $subject, $message, $headers);
}

function add_account_device(
    bool $is_first_device,
    mixed $acct,
    string $device_guid,
    string $device_type,
    string $machine_name,
    string $detail1,
    string $detail2,
    string $detail3) {

    $sql = 'INSERT INTO device (fk_account_id, device_guid, device_type, machine_name, detail1, detail2, detail3, active, activation_code, activated_dt)
            VALUES (:fk_account_id, :device_guid, :device_type, :machine_name, :detail1, :detail2, :detail3, :active, :activation_code, :activated_dt)';

    $statement = db()->prepare($sql);
    $statement->bindValue(':fk_account_id', $acct['id'], PDO::PARAM_INT);
    $statement->bindValue(':active', $is_first_device ? 1 : 0, PDO::PARAM_INT);
    $statement->bindValue(':device_guid', $device_guid);
    $statement->bindValue(':device_type', $device_type);
    $statement->bindValue(':machine_name', $machine_name);
    $statement->bindValue(':detail1', $detail1);
    $statement->bindValue(':detail2', $detail2);
    $statement->bindValue(':detail3', $detail3);
    $statement->bindValue(':detail3', $detail3);

    $activation_code = null;
    if ($is_first_device) {
        $statement->bindValue(':activated_dt', date('Y-m-d H:i:s', time()));
        $statement->bindValue(':activation_code', null);
    } else {
        $activation_code = generate_activation_code();
        $statement->bindValue(':activated_dt', null);
        $statement->bindValue(':activation_code', password_hash($activation_code, PASSWORD_DEFAULT));
    }
    if ($statement->execute()) {
        if ($activation_code != null) {
            // new non-existing platform, send confirm email and exit
            // send add email
            send_add_device_email($acct['username'], $acct['email'], $device_type, $device_guid, $activation_code);
            // exit w/ error ntf
            exit_w_error('New device login detected. To use this device, please click the link in the "Add Device" email you have just been sent. Then you will be able to login with this device.');
        }
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
    'device_type' => 'Linux',
    'machine_name' => 'TEST MACHINE NAME W/ SUB',
    'detail1' => 'test_detail1',
    'detail2' => 'test_detail2',
    'detail3' => 'test_detail3',
    'sub_type' => 'Standard',
    'monthly' => '1',
    'expires_utc_dt' => getFormattedDateTimeStr(date('Y-m-d H:i:s', time() + DEFAULT_EXPIRY_OFFSET)),
];

$testdata_no_sub = [
    'username' => 'tkefauver',
    'password' => '*1Password',
    'device_guid' => 'TEST GUID OTHER1',
    'device_type' => 'Windows',
    'machine_name' => 'TEST MACHINE NAME NO SUB',
    'detail1' => 'test_detail1',
    'detail2' => 'test_detail2',
    'detail3' => 'test_detail3',
];

$testdata = $testdata_w_sub;

$fields = [
    'username' => 'string | required',
    'password' => 'string | required',
    'device_guid' => 'string | required',
    'device_type' => 'string | required',
    'machine_name' => 'string | required',
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

if (is_new_device($account['id'], $inputs['device_guid'])) {
    // new device
    if (!is_new_platform($account['id'], $inputs['device_type'])) {
        // existing platform, send forget email and exit
        exit_w_forget_device_email_and_err($account, $inputs['device_type']);
    }

    add_account_device(
        is_initial_device($account['id']),
        $account,
        $inputs['device_guid'],
        $inputs['device_type'],
        $inputs['machine_name'],
        $inputs['detail1'],
        $inputs['detail2'],
        $inputs['detail3']);
} else if (!is_device_activated($account['id'], $inputs['device_guid'])) {
    exit_w_error('Error, please confirm this device in the email that was sent. You may need to check your spam folder.');
}
$has_sub = has_subscription($account['id'], $inputs['device_guid']);
if ($has_sub &&
    $inputs['sub_type'] != null &&
    $inputs['monthly'] != null &&
    $inputs['expires_utc_dt'] != null) {
    // subscription login, update info
    update_subscription(
        $account['id'],
        $inputs['device_guid'],
        $inputs['sub_type'],
        $inputs['monthly'] == "1",
        $inputs['expires_utc_dt']);
}
$sub = find_subscription_by_acct_id($account['id']);

exit_w_success(create_login_response($account['email'], $sub));
