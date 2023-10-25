<?php
require_once __DIR__ . '/../../src/lib/bootstrap.php';


function get_activate_email_msg_html($username, $activation_url): string 
{
    $msg = "Hi ".$username.", <br>Please click <a href='".$activation_url."'>here</a> to activate your account!<br><br>* The link will expire in 24 hours";
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
    'confirm' => '*1Password',
];

$fields = [
    'username' => 'string | required | alphanumeric | between: 3, 25 | unique: account, username',
    'email' => 'email | required | email | unique: account, email',
    'password' => 'string | required | secure',
    'confirm' => 'string | required | secure | same: password',
    'agree' => 'string | required'
];

$messages = [
    'confirm' => [
        'required' => 'Please enter the password again',
        'same' => 'The password does not match'
    ],
    'agree' => [
        'required' => 'You need to agree to the term of services to register'
    ]
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
        exit_w_error();
    }
}


if ($errors) {
    exit_w_errors($errors);
}

$activation_code = generate_activation_code();
$success = register_user($inputs['username'], $inputs['email'], $inputs['password'], $activation_code);
if ($success) {
    send_activation_email($inputs['username'], $inputs['email'], $activation_code);
    exit_success();
}

exit_w_error();
?>