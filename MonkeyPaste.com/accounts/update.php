<?php
    require_once "config.php";

    if(empty($_POST["device_guid"]) || 
        empty($_POST["sub_type"]) || 
        empty($_POST["expires_utc_dt"])) {
        echo "Error";
        return;
    }
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
            echo "Oops! Something went wrong. Please try again later.";
        }
        // Close statement
        mysqli_stmt_close($stmt);
    }

?>