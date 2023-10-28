<?php
 
// Include config file
require_once __DIR__ . '/../../src/lib/bootstrap.php';

function login(string $username, string $password): bool
{

    $account = find_account_by_username($username);

    // if account found, check the password
    if ($account && password_verify($password, $account['password'])) {
        if($account['active'] == 0) {
            exit_w_error('Please confirm account before logging in');
        }
        // prevent session fixation attack
        //session_regenerate_id();
        // $_SESSION['username'] = $account['username'];
        // $_SESSION['user_id']  = $account['id'];

        return true;
    }
    exit_w_error('Login failed');

    return false;
}


$testdata = [
    'username' => 'tkefauver',
    'password' => '*1Password',
    'device_guid' => 'TEST GUID',
    'sub_type' => 'Standard',
    'monthly' => '1',
    'expires_utc_dt' => '11/29/2023 7:00:00 PM',//getFormattedDateTimeStr(date('Y-m-d H:i:s', time() + 1 * 24 * 60 * 60)),
    'detail1' => 'test_detail1',
    'detail2' => 'test_detail2',
    'detail3' => 'test_detail3'
];


$fields = [
    'username' => 'string | required',
    'password' => 'string | required',    
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
        exit_w_error();
    }
}

if ($errors) {
    exit_w_errors($errors);
}
$success = login($inputs['username'], $inputs['password']);

if($success) {
    // login successful
    $resp = add_or_update_subscription($inputs['username'], $inputs['device_guid'], $inputs['sub_type'], $inputs['monthly'] == "1", $inputs['expires_utc_dt'], $inputs['detail1'], $inputs['detail2'], $inputs['detail3']);
    if($resp == NULL) {
        // error
        exit_w_error('Error, please try again later.');
    }
    exit_success($resp);
}

exit_w_error('Unknown error');
?>