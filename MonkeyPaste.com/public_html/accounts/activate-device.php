<?php

require_once __DIR__ . '/../../src/lib/bootstrap.php';

function add_device(string $username, string $device_guid, string $activation_code): int
{
    $acct = find_account_by_username($username);
    if (!$acct) {
        //exit_w_error('Error, user not found');
        redirect_to('error.php');
    }

    $sql = 'SELECT id, activation_code
            FROM device
            WHERE fk_account_id=:fk_account_id AND device_guid=:device_guid';

    $statement = db()->prepare($sql);
    $statement->bindValue(':fk_account_id', $acct['id'], PDO::PARAM_INT);
    $statement->bindValue(':device_guid', $device_guid);
    $statement->execute();

    $device = $statement->fetch(PDO::FETCH_ASSOC);

    if (!$device) {
        //exit_w_error('Error, no device found for '.$device_type.'. Nothing to forget.');
        redirect_to('error.php');
    }
    // verify the password
    if (!password_verify($activation_code, $device['activation_code'])) {
        //exit_w_error('Error, invalid request.');
        redirect_to('error.php');
    }

    $sql2 = 'UPDATE device
            SET active = 1, activated_dt = now()
            WHERE id=:id';

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
        'device_guid' => 'string | required',
        'activation_code' => 'string | required',
    ]);
    if ($errors) {
        redirect_to('error.php');
    }

    $success = add_device($inputs['username'], $inputs['device_guid'], $inputs['activation_code']);
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
    <h1 class="my-5">Hi, <b style="color:green;"><?php echo $inputs["username"]; ?></b>. Your <b style="color:blue;"><?php echo $inputs["device_type"]; ?></b> device has been <b style="color:gold;">added!</b></h1>
    <p>You can close this window now...</p>
</body>
</html>