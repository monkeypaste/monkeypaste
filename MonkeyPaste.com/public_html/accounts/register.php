<?php
require_once __DIR__ . '/../../src/lib/bootstrap.php';


function get_activate_email_msg_html($username, $activation_url): string 
{
    $msg = "Hi ".$username.", <br>Please click <a href='".$activation_url."'>here</a> to activate your account!<br><br>Thank you :)";
    return $msg;
}

function send_activation_email(string $username, string $email, string $activation_code): void
{
    $activation_link = ACCOUNTS_URL . "/activate.php?email=$email&activation_code=$activation_code";

    $subject = "Please activate your MonkeyPaste account";
    $message = get_activate_email_msg_html($username, $activation_link);
    // email header

    $headers  = "From: " . strip_tags(NO_REPLY_EMAIL_ADDRESS) . "\r\n";
    $headers .= "Reply-To: " . strip_tags(NO_REPLY_EMAIL_ADDRESS) . "\r\n";
    $headers .= "MIME-Version: 1.0\r\n";
    $headers .= "Content-Type: text/html; charset=UTF-8\r\n";

    // send the email
    mail($email, $subject, $message, $headers);
}


function register_user(string $username, string $email, string $password, string $activation_code, int $expiry = 1 * 24 * 60 * 60, bool $admin = false): bool
{
    $sql = 'INSERT INTO account(username, email, password, admin, activation_code, activation_expiry)
            VALUES(:username, :email, :password, :admin, :activation_code, :activation_expiry)';

    $statement = db()->prepare($sql);

    $statement->bindValue(':username', $username);
    $statement->bindValue(':email', $email);
    $statement->bindValue(':password', password_hash($password, PASSWORD_BCRYPT));
    $statement->bindValue(':admin', (int) $admin, PDO::PARAM_INT);
    $statement->bindValue(':activation_code', password_hash($activation_code, PASSWORD_DEFAULT));
    $statement->bindValue(':activation_expiry', date('Y-m-d H:i:s', time() + $expiry));

    return $statement->execute();
}

$testdata = [
    'username' => 'tkefauver',
    'email' => "tkefauver@gmail.com",
    'password' => '*1Password',
    'device_guid' => 'TEST GUID',
    'sub_type' => 'Standard',
    'monthly' => '0',
    'expires_utc_dt' => getFormattedDateTimeStr(date('Y-m-d H:i:s', time() + 1 * 24 * 60 * 60)),
    'detail1' => 'test_detail1',
    'detail2' => 'test_detail2',
    'detail3' => 'test_detail3'
];

$fields = [
    'username' => 'string | required | alphanumeric | between: 3, 25 | unique: account, username',
    'email' => 'email | required | email | unique: account, email',
    'password' => 'string | required | secure',
    'password2' => 'string | required | secure | same: password',
    'device_guid' => 'string | required',
    'sub_type' => 'string | required',
    'monthly' => 'string | required',
    'expires_utc_dt' => 'string | required | datetime',
    'detail1' => 'string | required',
    'detail2' => 'string | required',
    'detail3' => 'string | required'
];

$errors = [];
$inputs = [];

if (is_post_request()) 
{    
    [$inputs, $errors] = filter($_POST, $fields);
} else {
    if(CAN_TEST && isset($testdata)) {
        [$inputs, $errors] = filter($testdata, $fields);
    } else {        
        echo ERROR_MSG;
        exit(0);
    }
}


if ($errors) {
    exit_w_errors($errors);
}

$activation_code = generate_activation_code();
$success = register_user($inputs['username'], $inputs['email'], $inputs['password'], $activation_code);
if ($success) {
    // account created, add subscription 
    $new_account_id = db()->lastInsertId();
    $success = add_subscription($new_account_id, $inputs['device_guid'], $inputs['sub_type'], $inputs['monthly'] == "1", $inputs['expires_utc_dt'], $inputs['detail1'], $inputs['detail2'], $inputs['detail3']);
    if ($success) {
        send_activation_email($inputs['username'], $inputs['email'], $activation_code);
        echo SUCCESS_MSG;
        exit(0);
    }
}

echo ERROR_MSG;
exit(0);
?>