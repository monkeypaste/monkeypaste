<?php
 
// Include config file
require_once __DIR__ . '/../../src/lib/bootstrap.php';

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

function login(string $username, string $password): bool
{
    $user = find_account_by_username($username);

    // if user found, check the password
    if ($user && password_verify($password, $user['password']) && $user['active'] == 1) {

        // prevent session fixation attack
        session_regenerate_id();

        // set username in the session
        $_SESSION['username'] = $user['username'];
        $_SESSION['user_id']  = $user['id'];

        return true;
    }

    return false;
}
$success = login($_POST['username'], $_POST['password']);

if($success) {
    $success = update_subscription($_POST['device_guid'], $_POST['sub_type'], $_POST['expires_utc_dt']);
    if($success) {
        send_activation_email($_POST['email'],$activation_code);
        echo SUCCESS_MSG;
        exit(0);
    }
}
echo ERROR_MSG;

// // Define variables and initialize with empty values
// $email = $password = "";
// $email_err = $password_err = $login_err = "";
 

// // Check if email is empty
// if(empty(trim($_POST["email"]))){
//     $email_err = "Please enter email.";
// } else{
//     $email = trim($_POST["email"]);
// }

// // Check if password is empty
// if(empty(trim($_POST["password"]))){
//     $password_err = "Please enter your password.";
// } else{
//     $password = trim($_POST["password"]);
// }

// // Validate credentials
// if(empty($email_err) && empty($password_err)){
//     // Prepare a select statement
//     $sql = "SELECT id, email, password, confirm_key FROM account WHERE email = ?";
    
//     if($stmt = mysqli_prepare($link, $sql)){
//         // Bind variables to the prepared statement as parameters
//         mysqli_stmt_bind_param($stmt, "s", $param_email);
        
//         // Set parameters
//         $param_email = $email;
        
//         // Attempt to execute the prepared statement
//         if(mysqli_stmt_execute($stmt)){
//             // Store result
//             mysqli_stmt_store_result($stmt);
            
//             // Check if email exists, if yes then verify password
//             if(mysqli_stmt_num_rows($stmt) == 1){                    
//                 // Bind result variables
//                 mysqli_stmt_bind_result($stmt, $id, $email, $hashed_password, $confirm_key);
//                 if(mysqli_stmt_fetch($stmt)){
//                     if($confirm_key != NULL) {
//                         // account not confirmed yet
//                         echo "Error, please confirm registered email address "
//                     } else if(password_verify($password, $hashed_password)){
//                         // Password is correct, so start a new session
//                         //session_start();
                        
//                         // Store data in session variables
//                         // $_SESSION["loggedin"] = true;
//                         // $_SESSION["id"] = $id;
//                         // $_SESSION["email"] = $email;                            
                        
//                         if(!empty($_POST["device_guid"])) {
//                             // non-browser login

//                             // get device subscription info
//                             $sql = "SELECT sub_type, expires_utc_dt FROM subscription WHERE fk_account_id = ? AND device_guid = ?";
    
//                             if($stmt = mysqli_prepare($link, $sql)){
//                                 mysqli_stmt_bind_param($stmt, "is",$param_accountid, $param_device_guid);
//                                 $param_accountid = $id;
//                                 $param_device_guid = $_POST["device_guid"];
//                                 if(mysqli_stmt_execute($stmt)){
//                                     mysqli_stmt_store_result($stmt);                                        
//                                     if(mysqli_stmt_num_rows($stmt) == 1){            
//                                         // subscription entry found     
//                                         mysqli_stmt_bind_result($stmt, $sub_type, $expires_utc_dt);
//                                         if(mysqli_stmt_fetch($stmt)){
//                                             if($_POST["sub_type"] != $sub_type || $_POST["expires_utc_dt"] != $expires_utc_dt) {
//                                                 // subscription info changed, update record
//                                                 $sub_type = $_POST["sub_type"];
//                                                 $expires_utc_dt = getFormattedDateTimeStr($_POST["expires_utc_dt"]);
                                                    
//                                                 $sql = "UPDATE subscription SET sub_type = '".$sub_type."', expires_utc_dt = '".$expires_utc_dt."' WHERE device_guid = '".$_POST["device_guid"]."'";
//                                                 if(!mysqli_query($link, $sql)){
//                                                     echo "ERROR: Could not able to execute $sql. " . mysqli_error($link);
//                                                     mysqli_stmt_close($stmt);
//                                                     mysqli_close($link);
//                                                     exit(0);    
//                                                 }                                                    
//                                             }
//                                             // return device subscription status
//                                             echo "[SUCCESS]$sub_type,$expires_utc_dt";                                            
//                                             mysqli_stmt_close($stmt);
//                                             mysqli_close($link);
//                                             exit(0);
//                                         }
//                                     } else{
//                                         // Username doesn't exist, display a generic error message
//                                         echo "Invalid client id";
//                                     }
//                                 } else{
//                                     echo "Oops! Something went wrong. Please try again later.";
//                                 }
//                                 // Close statement
//                                 mysqli_stmt_close($stmt);
//                             }
//                         } else {                        
//                             // Redirect user to welcome page
//                             header("location: welcome.php");
//                         }
//                     } else{
//                         // Password is not valid, display a generic error message
//                         echo "Invalid email or password.";
//                     }
//                 }
//             } else{
//                 // Username doesn't exist, display a generic error message
//                 echo "Invalid email or password.";
//             }
//         } else{
//             echo "Oops! Something went wrong. Please try again later.";
//         }
//     }
// }
// mysqli_close($link);
?>