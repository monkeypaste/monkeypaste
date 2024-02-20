<?php

require_once __DIR__ . '/../../../src/lib/bootstrap.php';

session_start();

?>
<!DOCTYPE html>
<html lang="en">

<head>
     <meta charset="UTF-8">
     <title>Report Submitted</title>
     <link rel="shortcut icon" href="check.ico" />
     <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css">
     <style>
          body {
               font: 14px sans-serif;
               text-align: center;
          }
     </style>
</head>

<body>
     <h1 class="my-5">Thank you. Your report has been <b style="color:blue;">submitted</b>.</h1>
     <h2>It will be reviewed <b>soon</b><?php echo (empty($_SESSION["email"])) ? '...' : ' and we will contact you at <span style="color:magenta;">' . $_SESSION["email"] . '</span> if necssary...'; ?></h2>
     <p>You can close this window now...</p>
</body>

</html>
<?php session_destroy(); ?>