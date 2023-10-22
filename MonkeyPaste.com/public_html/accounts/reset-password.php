<?php

require_once __DIR__ . '/../../src/lib/bootstrap.php';


function find_resetable_account(string $reset_code, string $email)
{

    $sql = 'SELECT id, username, reset_code, reset_expiry < now() as expired
            FROM account
            WHERE email=:email';

    $statement = db()->prepare($sql);

    $statement->bindValue(':email', $email);
    $statement->execute();

    $account = $statement->fetch(PDO::FETCH_ASSOC);

    if ($account) {
        // already expired, delete the in active account with expired reset code
        if ((int)$account['expired'] === 1) {
            exit_w_error("reset expired");
        }
        // verify the password
        if(password_verify($reset_code, $account['reset_code'])) {
            return $account;
        }  
    } 

    return null;
}

function reset_account_password(int $id, string $password): bool
{
    $sql = 'UPDATE account
            SET reset_code = :reset_code, password = :password
            WHERE id=:id';

    $statement = db()->prepare($sql);
    $statement->bindValue(':id', $id, PDO::PARAM_INT);
    $statement->bindValue(':reset_code', NULL);
    $statement->bindValue(':password', password_hash($password, PASSWORD_BCRYPT));

    return $statement->execute();
}

function process_errors($errors, $password) {
    if(!isset($_SESSION)) {
        return;
    }
    $_SESSION["password_err"] = $errors != NULL && array_key_exists('password',$errors) ? $errors['password']:"";
    $_SESSION["password2_err"] = $errors != NULL && array_key_exists('confirm',$errors) ? $errors['confirm']:"";
    $_SESSION["new_password"] = $password;
}

session_start();
if (is_get_request()) 
{    
    $_SESSION["new_password"] = $_SESSION["password_err"] = $_SESSION["password2_err"] = "";

    $fields = [
        'email' => 'string | required',
        'reset_code' => 'string | required'
    ];
    
    $errors = [];
    $inputs = [];
    [$inputs, $errors] = filter($_GET, $fields);

    if ($errors) {
        if(CAN_TEST) {
            printerr($errors);
        }
        exit_w_error("param error");
    }

    $account = find_resetable_account($inputs['reset_code'], $inputs['email']);
    if($account == NULL) {
        exit_w_error("account not found or no reset requested");
    }

    $_SESSION['accid'] = $account['id'];
} else if(is_post_request()) {
    $fields = [
        'password' => 'string | required | secure',
        'confirm' => 'string | required | same: password',
    ];
    
    $errors = [];
    $inputs = [];
    [$inputs, $errors] = filter($_POST, $fields);

    if ($errors) {
        if(CAN_TEST) {
            //printerr($errors);
        }
        process_errors($errors,$inputs['password']);
        //exit_w_error("param error");
    } else {
        // println("about to reset...");
        // var_dump($_SESSION);
        $success = reset_account_password($_SESSION['accid'],$inputs['password']);
        if($success) {
            exit_success();
        }
        process_errors(["password"=>"error","confirm"=>""],$inputs['password']);
    }
    
    
    //exit_w_error("error");

} else {    
    exit_w_error("invalid params");
}

?>
 
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Reset Password</title>
    <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css">
    <style>
        body{ font: 14px sans-serif; }
        .wrapper{ width: 360px; padding: 20px; }
    </style>
</head>
<body>
    <div class="wrapper">
        <h2>Reset Password</h2>
        <p>Please fill out this form to reset your password.</p>
        <form action="<?php echo htmlspecialchars($_SERVER["PHP_SELF"]); ?>" method="post"> 
            <div class="form-group">
                <label>New Password</label>
                <input type="password" name="password" class="form-control <?php echo (!empty($_SESSION["password_err"])) ? 'is-invalid' : ''; ?>" value="<?php echo $_SESSION["new_password"]; ?>">
                <span class="invalid-feedback"><?php echo $_SESSION["password_err"]; ?></span>
            </div>
            <div class="form-group">
                <label>Confirm Password</label>
                <input type="password" name="confirm" class="form-control <?php echo (!empty($_SESSION["password2_err"])) ? 'is-invalid' : ''; ?>">
                <span class="invalid-feedback"><?php echo $_SESSION["password2_err"]; ?></span>
            </div>
            <div class="form-group">
                <input type="submit" class="btn btn-primary" value="Submit">
                <!-- <a class="btn btn-link ml-2" href="welcome.php">Cancel</a> -->
            </div>
        </form>
    </div>    
</body>
</html>