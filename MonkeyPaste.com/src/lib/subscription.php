<?php
function find_account_by_username(string $username)
{
    $sql = 'SELECT *
            FROM account
            WHERE username=:username';

    $statement = db()->prepare($sql);
    $statement->bindValue(':username', $username, PDO::PARAM_STR);
    $statement->execute();

    return $statement->fetch(PDO::FETCH_ASSOC);
}

function add_subscription(
    string $device_guid,
    string $sub_type,
    bool $is_monthly,
    string $expires_utc_dt): mixed {

    $sql = 'INSERT INTO subscription(device_guid, sub_type, monthly, expires_utc_dt,fk_account_id)
            VALUES (:device_guid,:sub_type,:monthly,:expires_utc_dt, :fk_account_id);';

    $statement = db()->prepare($sql);
    $statement->bindValue(':device_guid', $device_guid);
    $statement->bindValue(':sub_type', $sub_type);
    $statement->bindValue(':monthly', (int) $is_monthly, PDO::PARAM_INT);
    $statement->bindValue(':expires_utc_dt', $expires_utc_dt);
    $statement->bindValue(':fk_account_id', null);
    if ($statement->execute()) {
        return true;
    }
    exit_w_error('error could not add subscription');
}

function has_subscription(int $acct_id, string $device_guid): bool
{
    return find_subscription_by_device_guid($device_guid) != null || find_subscription_by_acct_id($acct_id) != null;

}
function update_subscription(
    int $acct_id,
    string $device_guid,
    string $sub_type,
    bool $is_monthly,
    string $expires_utc_dt) {

    $is_by_device_guid = find_subscription_by_device_guid($device_guid) != null;
    $sql = null;

    if ($is_by_device_guid) {
        // NOTE in case subscription device was forgotten
        // use either device guid or account id...

        $sql = 'UPDATE subscription
            SET sub_type=:sub_type,
                expires_utc_dt=:expires_utc_dt,
                monthly=:is_monthly,
                fk_account_id=:fk_account_id
            WHERE device_guid=:device_guid';

    } else {
        $sql = 'UPDATE subscription
            SET sub_type=:sub_type,
                expires_utc_dt=:expires_utc_dt,
                monthly=:is_monthly
            WHERE fk_account_id=:fk_account_id';
    }

    $statement = db()->prepare($sql);
    $statement->bindValue(':sub_type', $sub_type);
    $statement->bindValue(':expires_utc_dt', date(SYS_DATETIME_FORMAT, strtotime($expires_utc_dt)));
    $statement->bindValue(':is_monthly', (int) $is_monthly, PDO::PARAM_INT);
    $statement->bindValue(':fk_account_id', $acct_id, PDO::PARAM_INT);
    if ($is_by_device_guid) {
        // ensure acct id is set
        $statement->bindValue(':device_guid', $device_guid);
    }

    if ($statement->execute()) {
        return true;
    }
    exit_w_error('error could not update subscription');
}

function find_subscription_by_acct_id(string $acc_id)
{
    // existing subscription, update info
    $sql = 'SELECT *
           FROM subscription
           WHERE device_guid IN (SELECT device_guid FROM device WHERE fk_account_id=:acc_id)
           ORDER BY created_dt DESC
           LIMIT 1';

    $statement = db()->prepare($sql);
    $statement->bindValue(':acc_id', $acc_id, PDO::PARAM_INT);
    $statement->execute();

    return $statement->fetch(PDO::FETCH_ASSOC);
}
function find_subscription_by_device_guid(string $device_guid)
{
    // existing subscription, update info
    $sql = 'SELECT *
           FROM subscription
           WHERE device_guid=:device_guid';

    $statement = db()->prepare($sql);
    $statement->bindValue(':device_guid', $device_guid);
    $statement->execute();

    return $statement->fetch(PDO::FETCH_ASSOC);
}
