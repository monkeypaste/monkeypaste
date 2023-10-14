<?php
require_once __DIR__ . '/../../src/lib/bootstrap.php';

function generate_activation_code(): string
{
    return bin2hex(random_bytes(16));
}

function send_activation_email(string $email, string $activation_code): void
{
    // create the activation link
    $activation_link = ACCOUNTS_URL."/activate.php?email=$email&activation_code=$activation_code";

    // set email subject & body
    $subject = 'Please activate your account';
    $message = <<<MESSAGE
            Hi,
            Please click the following link to activate your account:
            $activation_link
            MESSAGE;
    // email header
    $header = 'From:' . NO_REPLY_EMAIL_ADDRESS;

    // send the email
    mail($email, $subject, nl2br($message), $header);

}

function register_user(string $username, string $email, string $password, string $activation_code, int $expiry = 1 * 24  * 60 * 60, bool $admin = false): bool
{    
   $sql = 'INSERT INTO account(username, email, password, admin, activation_code, activation_expiry)
            VALUES(:username, :email, :password, :admin, :activation_code, :activation_expiry)';

    $statement = db()->prepare($sql);

    $statement->bindValue(':username', $username);
    $statement->bindValue(':email', $email);
    $statement->bindValue(':password', password_hash($password, PASSWORD_BCRYPT));
    $statement->bindValue(':admin', (int)$admin, PDO::PARAM_INT);
    $statement->bindValue(':activation_code', password_hash($activation_code, PASSWORD_DEFAULT));
    $statement->bindValue(':activation_expiry', date('Y-m-d H:i:s',  time() + $expiry));

    return $statement->execute();
}

if(!is_post_request()) {
    
    echo ERROR_MSG;
    exit(0);
}

$errors = [];
$inputs = [];

$fields = [
    // 'username' => 'string | required | alphanumeric | between: 3, 25 | unique: account, username',
    // 'email' => 'email | required | email | unique: account, email',
    // 'password' => 'string | required | secure',
    // 'device_guid' => 'string | required',
    // 'sub_type' => 'string | required',
    // 'detail1' => 'string | required',
    // 'detail2' => 'string | required',
    // 'detail3' => 'string | required'
    'username' => 'string',
    'email' => 'email',
    'password' => 'string',
    'device_guid' => 'string',
    'sub_type' => 'string',
    'expires_utc_dt' => 'string',
    'detail1' => 'string',
    'detail2' => 'string',
    'detail3' => 'string'
];

[$inputs, $errors] = filter($_POST, $fields);

if ($errors) {
    echo ERROR_MSG;
    exit(0);
}

$activation_code = generate_activation_code();
//println(explode(',',$inputs));
$username = $inputs['username'];
$email = $inputs['email'];
$password = $inputs['password'];
$success = register_user($username, $email, $password, $activation_code);
if($success) {
    // account created, add subscription 
    $new_account_id = db()->lastInsertId();
    $success = add_subscription($new_account_id, $inputs['device_guid'], $inputs['sub_type'], $inputs['expires_utc_dt'], $inputs['detail1'], $inputs['detail2'], $inputs['detail3']);
    if($success) {
        send_activation_email($inputs['email'],$activation_code);
        echo SUCCESS_MSG;
        exit(0);
    }
}

echo ERROR_MSG;
exit(0);
// if($stmt = mysqli_prepare($link, $sql)){
//     // Bind variables to the prepared statement as parameters
//     mysqli_stmt_bind_param($stmt, 'sssss', $username, $email, $password, $activation_code, $expiry);
    
//     // Set parameters
//     $param_email = $_POST['email'];
//     $param_password = password_hash($_POST['password'], PASSWORD_DEFAULT); // Creates a password hash
//     $param_confirm_key = com_create_guid();

//     // Attempt to execute the prepared statement
//     if(mysqli_stmt_execute($stmt)) {            
//         // Obtain last inserted id
//         $new_account_id = mysqli_insert_id($link);
                    
//         $datetime = new DateTime ($_POST['expires_utc_dt']);
//         $datetime = $datetime->format('Y-m-d H:i:s');
//         // Prepare an insert statement
//         $sql = 'INSERT INTO subscription (fk_account_id, device_guid, sub_type, expires_utc_dt) VALUES (?, ?, ?, ?)';

//         if($stmt = mysqli_prepare($link, $sql)){
//             // Bind variables to the prepared statement as parameters
//             mysqli_stmt_bind_param($stmt, 'isss', $param_new_account_id, $param_device_guid, $param_sub_type, $param_expires_utc_dt);
            
//             // Set parameters
//             $param_new_account_id = $new_account_id;
//             $param_device_guid = $_POST['device_guid'];
//             $param_sub_type = $_POST['sub_type'];
//             $param_expires_utc_dt = $datetime;
            
//             // Attempt to execute the prepared statement
//             if(mysqli_stmt_execute($stmt)) {            
//                 echo '[SUCCESS]';
//             } else {
//                 echo '[Error]';
//             }
//             // Close statement
//             mysqli_stmt_close($stmt);
//         } else {                        
//             echo '[Error]';
//         }
//     } else{
//         echo 'Oops! Something went wrong. Please try again later.';
//     }

//     // Close statement
//     mysqli_stmt_close($stmt);
// }
// // Close connection
// mysqli_close($link);
?>