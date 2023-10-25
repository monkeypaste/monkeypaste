<?php

require_once __DIR__ . '/../../src/lib/bootstrap.php';

function find_unverified_account(string $activation_code, string $email)
{

    $sql = 'SELECT id, username, activation_code, activation_expiry < now() as expired
            FROM account
            WHERE active = 0 AND email=:email';

    $statement = db()->prepare($sql);

    $statement->bindValue(':email', $email);
    $statement->execute();

    $account = $statement->fetch(PDO::FETCH_ASSOC);

    if(!$account) {
        return null;
    }
    
    // already expired, delete the in active account with expired activation code
    if ((int)$account['expired'] === 1) {
        delete_account_by_id($account['id']);
        return null;
    }
    // verify the password
    if(password_verify($activation_code, $account['activation_code'])) {
        return $account;
    }  
}
function delete_account_by_id(int $id, int $active = 0)
{
    $sql = 'DELETE FROM account
            WHERE id =:id and active=:active';

    $statement = db()->prepare($sql);
    $statement->bindValue(':id', $id, PDO::PARAM_INT);
    $statement->bindValue(':active', $active, PDO::PARAM_INT);

    return $statement->execute();
}

function activate_account(int $id): bool
{
    $sql = 'UPDATE account
            SET active = 1, activated_dt = now()
            WHERE id=:id';

    $statement = db()->prepare($sql);
    $statement->bindValue(':id', $id, PDO::PARAM_INT);

    return $statement->execute();
}
$account = NULL;

if (is_get_request()) {

    // sanitize the email & activation code
    [$inputs, $errors] = filter($_GET, [
        'email' => 'string | required | email',
        'activation_code' => 'string | required'
    ]);
    if($errors) {
        redirect_to('error.php');
    }

    $account = find_unverified_account($inputs['activation_code'], $inputs['email']);
    if (!$account || !activate_account($account['id'])) {        
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
    <title>Welcome</title>
    <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css">
    <style>
        body{ font: 14px sans-serif; text-align: center; }

    </style>
</head>
<body>
    <h1 class="my-5">Hi, <b style="color:green;"><?php echo $account["username"]; ?></b>. Activation successful!</h1>
    <p>You can close this window now...</p>
</body>
</html>