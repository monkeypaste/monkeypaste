<?php

require_once __DIR__ . '/../../src/lib/bootstrap.php';

function forget_device(string $username, string $device_type, string $forget_device_code): int
{
    $acct = find_account_by_username($username);
    if (!$acct) {
        //exit_w_error('Error, user not found');
        redirect_to('error.php');
    }

    $sql = 'SELECT id, forget_code, forget_expiry < now() as expired
            FROM device
            WHERE fk_account_id=:fk_account_id AND device_type=:device_type';

    $statement = db()->prepare($sql);
    $statement->bindValue(':fk_account_id', $acct['id'], PDO::PARAM_INT);
    $statement->bindValue(':device_type', $device_type);
    $statement->execute();

    $device = $statement->fetch(PDO::FETCH_ASSOC);

    if (!$device) {
        //exit_w_error('Error, no device found for '.$device_type.'. Nothing to forget.');
        redirect_to('error.php');
    }

    if ((int) $device['expired'] === 1) {
        // already expired
        //exit_w_error('Error, link has expired. Attempt to login from your unknown '.$device_type.' device again to resend.');
        redirect_to('error.php');
    }
    // verify the password
    if (!password_verify($forget_device_code, $device['forget_code'])) {
        //exit_w_error('Error, invalid request.');
        redirect_to('error.php');
    }

    $sql2 = 'DELETE FROM device
            WHERE id =:id';

    $statement = db()->prepare($sql2);
    $statement->bindValue(':id', $device['id'], PDO::PARAM_INT);

    return $statement->execute();
}

$account = null;

if (is_get_request()) {

    // sanitize the email & activation code
    [$inputs, $errors] = filter($_GET, [
        'username' => 'string | required | username',
        'device_type' => 'string | required',
        'forget_device_code' => 'string | required',
    ]);
    if ($errors) {
        redirect_to('error.php');
    }

    $success = forget_device($inputs['username'], $inputs['device_type'], $inputs['forget_device_code']);
    if (!$success) {
        redirect_to('error.php');
    }
} else {
    redirect_to('error.php');
}
?>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Activate Account - Monkey Paste</title>
    <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css">
    <style>
        body{ font: 14px sans-serif; text-align: center; }

    </style>
</head>
<body>
    <h1 class="my-5">Hi, <b style="color:green;"><?php echo $inputs["username"]; ?></b>. Your <b style="color:blue;"><?php echo $inputs["device_type"]; ?></b> device has been <b style="color:red;">forgotten.</b></h1>
    <p>You can close this window now...</p>
</body>
</html>