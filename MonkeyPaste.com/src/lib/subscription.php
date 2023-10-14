<?php

function find_account_by_username(string $username)
{
    $sql = 'SELECT id, username, password
            FROM account
            WHERE username=:username';

    $statement = db()->prepare($sql);
    $statement->bindValue(':username', $username, PDO::PARAM_STR);
    $statement->execute();

    return $statement->fetch(PDO::FETCH_ASSOC);
}
function find_account_by_email(string $email)
{
    $sql = 'SELECT *
            FROM account
            WHERE email=:email';

    $statement = db()->prepare($sql);
    $statement->bindValue(':email', $email);
    $statement->execute();

    return $statement->fetch(PDO::FETCH_ASSOC);
}

function update_subscription(string $device_guid,string $sub_type,string $expires_utc_dt)
{
   // existing subscription, update info
   $sql = 'UPDATE subscription
            SET sub_type=:sub_type,
                expires_utc_dt=:expires_utc_dt
            WHERE device_guid=:device_guid';

    $statement = db()->prepare($sql);
    $statement->bindValue(':sub_type', $sub_type);
    $statement->bindValue(':expires_utc_dt', $expires_utc_dt);
    $statement->bindValue(':device_guid', $device_guid);

    return $statement->execute();
}

function add_subscription(int $accid, string $device_guid, string $sub_type, string $expires_utc_dt, string $detail1, string $detail2, string $detail3)
{
    $sql = 'INSERT INTO subscription(fk_account_id, device_guid, sub_type, expires_utc_dt, detail1, detail2, detail3)
                                VALUES(:accid, :device_guid, :sub_type, :expires_utc_dt, :detail1, :detail2, :detail3)';

    $statement = db()->prepare($sql);

    $statement->bindValue(':accid', $accid, PDO::PARAM_INT);
    $statement->bindValue(':device_guid', $device_guid);
    $statement->bindValue(':sub_type', $sub_type);
    $statement->bindValue(':expires_utc_dt', getFormattedDateTimeStr($expires_utc_dt));
    $statement->bindValue(':detail1', $detail1);
    $statement->bindValue(':detail2', $detail2);
    $statement->bindValue(':detail3', $detail3);

    return $statement->execute();
}

?>