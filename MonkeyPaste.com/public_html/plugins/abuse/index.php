<?php

require_once __DIR__ . '/../../../src/lib/bootstrap.php';

function send_abuse_report(string $plugin_guid, string $description, string $reply_email) {
     $subject = "$plugin_guid,$reply_email";
     $message = $description;

     $headers = "From: " . strip_tags(NO_REPLY_EMAIL_ADDRESS) . "\r\n";
     $headers .= "Reply-To: " . strip_tags(NO_REPLY_EMAIL_ADDRESS) . "\r\n";
     $headers .= "MIME-Version: 1.0\r\n";
     $headers .= "Content-Type: text/html; charset=UTF-8\r\n";

     // send the email
     mail(ABUSE_EMAIL_ADDRESS, $subject, $message, $headers);
}

function reset_validation() {
     $_SESSION['plugin_guid'] = '';
     $_SESSION['email'] = '';
     $_SESSION['description'] = '';
     $_SESSION['email_err'] = '';
     $_SESSION['description_err'] = '';
}

session_start();
if (is_get_request()) {
     // form opened
     reset_validation();

     [$inputs, $errors] = filter($_GET, [
          'id' => 'string | required',
     ]);
     if ($errors) {
          redirect_to('error.php');
     }
     $_SESSION['plugin_guid'] = $inputs['id'];
} else if (is_post_request()) {
     // form submitted
     [$inputs, $errors] = filter($_POST, [
          'plugin_guid' => 'string | required',
          'email' => 'string | email',
          'description' => 'string | required | between: 1, 5000',
     ]);

     if ($errors) {
          if (array_key_exists("plugin_guid", $errors)) {
               redirect_to('error.php');
          }

          if (array_key_exists("email", $errors)) {
               $_SESSION['email_err'] = $errors['email'];
          } else {
               unset($_SESSION['email_err']);
          }
          if (array_key_exists("description", $errors)) {
               $_SESSION['description_err'] = $errors['description'];
          } else {
               unset($_SESSION['description_err']);
          }
          $_SESSION['email'] = $inputs['email'];
          $_SESSION['description'] = $inputs['description'];
     } else {
          if (array_key_exists("email", $inputs)) {
               $_SESSION['email'] = $inputs['email'];
          }
          send_abuse_report($inputs['plugin_guid'], $inputs['description'], $inputs['email']);
          redirect_to('confirmed.php');
     }
} else {
     redirect_to('error.php');
}
?>
<!DOCTYPE html>
<html lang="en">

<head>
     <meta charset="UTF-8">
     <title>Report Abuse</title>
     <link rel="shortcut icon" href="lock.ico" />
     <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css">
     <style>
          body {
               font: 14px sans-serif;
          }

          .wrapper {
               width: 360px;
               padding: 20px;
          }

          textarea {
               resize: auto;
          }
     </style>
</head>

<body>
     <div class="wrapper">
          <h2>Report Abuse</h2>
          <form action="<?php echo htmlspecialchars($_SERVER["PHP_SELF"]); ?>" method="post">
               <input type="hidden" value="1" name="XDEBUG_SESSION" />
               <input type="hidden" value="<?php echo $_SESSION["plugin_guid"]; ?>" name="plugin_guid" />
               <div class="form-group">
                    <label for="description">Please tell us in as few words as possible what's wrong:</label>
                    <textarea id="description" name="description" rows="10" cols="50" class="form-control <?php echo (!empty($_SESSION["description_err"])) ? 'is-invalid' : ''; ?>" value="<?php echo $_SESSION["description"]; ?>"></textarea>
                    <span class="invalid-feedback"><?php echo $_SESSION["description_err"]; ?></span>
               </div>
               <div class="form-group">
                    <label for="email">Your email (optional):</label>
                    <input type="text" id="email" name="email" class="form-control <?php echo (!empty($_SESSION["email_err"])) ? 'is-invalid' : ''; ?>" value="<?php echo $_SESSION["email"]; ?>">
                    <span class="invalid-feedback"><?php echo $_SESSION["email_err"]; ?></span>
               </div>
               <div class="form-group">
                    <input type="submit" class="btn btn-primary" value="Submit">
               </div>
          </form>
     </div>
</body>

</html>