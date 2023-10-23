<?php

require_once __DIR__ . '/../../src/lib/bootstrap.php';

function get_reset_password_email_msg_html($username, $reset_url): string 
{
    $msg = "Hi ".$username.", <br>Please click <a href='".$reset_url."'>here</a> to reset your password.";
    return $msg;
}

function send_reset_email(string $username, string $email, string $reset_code)
{
    $reset_link = ACCOUNTS_URL . "/reset-password.php?email=$email&reset_code=$reset_code";

    $subject = "Reset your MonkeyPaste account password";
    $message = get_reset_password_email_msg_html($username, $reset_link);
    // email header

    $headers  = "From: " . strip_tags(NO_REPLY_EMAIL_ADDRESS) . "\r\n";
    $headers .= "Reply-To: " . strip_tags(NO_REPLY_EMAIL_ADDRESS) . "\r\n";
    $headers .= "MIME-Version: 1.0\r\n";
    $headers .= "Content-Type: text/html; charset=UTF-8\r\n";

    // send the email
    mail($email, $subject, $message, $headers);
}

function reset_account_password(int $id, string $reset_code, int $expiry = 1 * 24 * 60 * 60): bool
{
    $sql = 'UPDATE account
            SET reset_code = :reset_code, reset_expiry = :reset_expiry
            WHERE id=:id';

    $statement = db()->prepare($sql);
    $statement->bindValue(':id', $id, PDO::PARAM_INT);
    $statement->bindValue(':reset_code', password_hash($reset_code, PASSWORD_DEFAULT));
    $statement->bindValue(':reset_expiry', date('Y-m-d H:i:s', time() + $expiry));
    return $statement->execute();
}

$testdata = [
    'username' => "tombo",
];

$fields = [
    'username' => 'string | required',
];

$errors = [];
$inputs = [];

if (is_post_request()) 
{    
    [$inputs, $errors] = filter($_POST, $fields);
} else if(CAN_TEST && isset($testdata)) {
    [$inputs, $errors] = filter($testdata, $fields);
} else {    
    exit_w_error("invalid params");
}

if ($errors) {
    if(CAN_TEST) {
        printerr($errors);
    }
    exit_w_error("param error");
}

$account = find_account_by_username($inputs['username']);
if($account == NULL) {
    exit_w_error("account not found.");
}
if($account['active'] === 0) {
    exit_w_error("account not activated");
}
$reset_code = generate_activation_code();
$success = reset_account_password($account['id'],$reset_code);
if(!$success) {
    exit_w_error("error");
}
send_reset_email($account['username'],$account['email'],$reset_code);
exit_success();

?>
