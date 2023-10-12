<?php
// Include config file
require_once "config.php";
 
// Prepare an insert statement
$sql = "INSERT INTO account (email, password, confirm_key) VALUES (?, ?, ?, ?)";
     
if($stmt = mysqli_prepare($link, $sql)){
    // Bind variables to the prepared statement as parameters
    mysqli_stmt_bind_param($stmt, "sss", $param_email, $param_password, $param_confirm_key);
    
    // Set parameters
    $param_email = $_POST["email"];
    $param_password = password_hash($_POST["password"], PASSWORD_DEFAULT); // Creates a password hash
    $param_confirm_key = com_create_guid();

    // Attempt to execute the prepared statement
    if(mysqli_stmt_execute($stmt)) {            
        // Obtain last inserted id
        $new_account_id = mysqli_insert_id($link);
                    
        $datetime = new DateTime ($_POST["expires_utc_dt"]);
        $datetime = $datetime->format('Y-m-d H:i:s');
        // Prepare an insert statement
        $sql = "INSERT INTO subscription (fk_account_id, device_guid, sub_type, expires_utc_dt) VALUES (?, ?, ?, ?)";

        if($stmt = mysqli_prepare($link, $sql)){
            // Bind variables to the prepared statement as parameters
            mysqli_stmt_bind_param($stmt, "isss", $param_new_account_id, $param_device_guid, $param_sub_type, $param_expires_utc_dt);
            
            // Set parameters
            $param_new_account_id = $new_account_id;
            $param_device_guid = $_POST["device_guid"];
            $param_sub_type = $_POST["sub_type"];
            $param_expires_utc_dt = $datetime;
            
            // Attempt to execute the prepared statement
            if(mysqli_stmt_execute($stmt)) {            
                echo "[SUCCESS]";
            } else {
                echo "[Error]";
            }
            // Close statement
            mysqli_stmt_close($stmt);
        } else {                        
            echo "[Error]";
        }
    } else{
        echo "Oops! Something went wrong. Please try again later.";
    }

    // Close statement
    mysqli_stmt_close($stmt);
}
// Close connection
mysqli_close($link);
?>