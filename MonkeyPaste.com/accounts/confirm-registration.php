<?php
require_once "config.php";

// Prepare a select statement
$sql = "SELECT * FROM account WHERE id = ? AND confirm_key = ?";
        
if($stmt = mysqli_prepare($link, $sql)){
    mysqli_stmt_bind_param($stmt, "is", $param_accountid, $param_confirm_key);
    $param_accountid = $_GET["accountid"];
    $param_confirm_key = $_GET["confirm_key"];
    if(mysqli_stmt_execute($stmt)){
        mysqli_stmt_store_result($stmt);
        
        if(mysqli_stmt_num_rows($stmt) == 1){
            // valid confirmation
            $sql = "UPDATE account SET confirm_key = NULL WHERE id = '".$_GET["accountid"]."'";
            if(mysqli_query($link, $sql)){
                echo "Confirmation successful!";

            } else{
                echo "ERROR: Could not able to execute $sql. " . mysqli_error($link);
                mysqli_stmt_close($stmt);
                mysqli_close($link);
                exit(0);    
            }   
        } else{
            echo "Error, no confirmation found"
        }
    } else{
        echo "Oops! Something went wrong. Please try again later.";
    }

    // Close statement
    mysqli_stmt_close($stmt);
}
?>