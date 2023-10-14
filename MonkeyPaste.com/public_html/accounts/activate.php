<?php

require_once __DIR__ . '/../../src/lib/bootstrap.php';

function find_unverified_user(string $activation_code, string $email)
{

    $sql = 'SELECT id, activation_code, activation_expiry < now() as expired
            FROM account
            WHERE active = 0 AND email=:email';

    $statement = db()->prepare($sql);

    $statement->bindValue(':email', $email);
    $statement->execute();

    $user = $statement->fetch(PDO::FETCH_ASSOC);

    if ($user) {
        // already expired, delete the in active user with expired activation code
        if ((int)$user['expired'] === 1) {
            delete_user_by_id($user['id']);
            return null;
        }
        // verify the password
        if (password_verify($activation_code, $user['activation_code'])) {
            return $user;
        }
    }

    return null;
}
function delete_user_by_id(int $id, int $active = 0)
{
    $sql = 'DELETE FROM account
            WHERE id =:id and active=:active';

    $statement = db()->prepare($sql);
    $statement->bindValue(':id', $id, PDO::PARAM_INT);
    $statement->bindValue(':active', $active, PDO::PARAM_INT);

    return $statement->execute();
}

function activate_user(int $user_id): bool
{
    $sql = 'UPDATE account
            SET active = 1,
                activated_dt = CURRENT_TIMESTAMP
            WHERE id=:id';

    $statement = db()->prepare($sql);
    $statement->bindValue(':id', $user_id, PDO::PARAM_INT);

    return $statement->execute();
}

if(!is_post_request()) {    
    echo ERROR_MSG;
    exit(0);
}

$user = find_unverified_user($_POST['activation_code'], $_POST['email']);

if ($user && activate_user($user['id'])) {
    echo SUCCESS_MSG;
} else {
    echo ERROR_MSG;
}
?>