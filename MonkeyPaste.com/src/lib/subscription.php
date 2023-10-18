<?php
function is_free_sub(string $sub_type):bool {
    return ctype_lower($sub_type) == "free";
}
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

function find_paid_subscription_by_username(string $username)
{
   // existing subscription, update info
   $sql = 'SELECT * FROM subscription WHERE sub_type != :free_sub_type AND fk_account_id = (SELECT id FROM account WHERE username=:username)';

    $statement = db()->prepare($sql);
    $statement->bindValue(':free_sub_type', "Free");
    $statement->bindValue(':username', $username);
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

function get_subscription_response($sub): string {

    $resp_obj = [
      'sub_type' => $sub['sub_type'],  
      'monthly' => $sub['monthly'] ? "1":"0",  
      'expires_utc_dt' => getFormattedDateTimeStr($sub['expires_utc_dt']),  
    ];
    return json_encode($resp_obj);
}

function add_or_update_subscription(
    string $username, 
    string $device_guid,
    string $sub_type, 
    bool $is_monthly, 
    string $expires_utc_dt,  
    string $detail1, 
    string $detail2, 
    string $detail3): string {
    $device_sub = find_subscription_by_device_guid($device_guid);
    
    if($device_sub == NULL) {
        
        $acc = find_account_by_username($username);
        if($acc == NULL) {
            return NULL;
        }
        $add_success = add_subscription($acc['id'],$device_guid,$sub_type,$is_monthly,$expires_utc_dt,$detail1,$detail2,$detail3);
        if(!$add_success) {
            return null;
        }
        $device_sub = find_subscription_by_device_guid($device_guid);
    } else {
        $update_success = update_subscription($username,$device_guid,$sub_type,$is_monthly,$expires_utc_dt);
        if(!$update_success) {
            return null;
        }
    }
    
    $paid_sub = find_paid_subscription_by_username($username);
    if($paid_sub != null) {
        // return paid info
        return get_subscription_response($paid_sub);
    }
    // return this devices free info
    return get_subscription_response($device_sub);
}

function update_subscription(string $username, string $device_guid,string $sub_type, bool $is_monthly, string $expires_utc_dt):string
{    
    $sql = 'UPDATE subscription
        SET sub_type=:sub_type,
            expires_utc_dt=:expires_utc_dt,
            monthly=:is_monthly
        WHERE device_guid=:device_guid';

    $statement = db()->prepare($sql);
    $statement->bindValue(':sub_type', $sub_type);
    $statement->bindValue(':expires_utc_dt', $expires_utc_dt);
    $statement->bindValue(':device_guid', $device_guid);
    $statement->bindValue(':is_monthly', (int)$is_monthly, PDO::PARAM_INT);

    return $statement->execute();
}

function add_subscription(int $accid, string $device_guid, string $sub_type, bool $is_monthly, string $expires_utc_dt, string $detail1, string $detail2, string $detail3)
{
    println('add called');
    $sql = 'INSERT INTO subscription(fk_account_id, device_guid, sub_type, monthly, expires_utc_dt, detail1, detail2, detail3)
                                VALUES(:accid, :device_guid, :sub_type, :is_monthly, :expires_utc_dt, :detail1, :detail2, :detail3)';

    $statement = db()->prepare($sql);

    $statement->bindValue(':accid', $accid, PDO::PARAM_INT);
    $statement->bindValue(':device_guid', $device_guid);
    $statement->bindValue(':sub_type', $sub_type);
    $statement->bindValue(':is_monthly', (int)$is_monthly, PDO::PARAM_INT);
    $statement->bindValue(':expires_utc_dt', getFormattedDateTimeStr($expires_utc_dt));
    $statement->bindValue(':detail1', $detail1);
    $statement->bindValue(':detail2', $detail2);
    $statement->bindValue(':detail3', $detail3);

    return $statement->execute();
}

?>