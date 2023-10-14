<?php

require_once __DIR__ . '/../../src/lib/bootstrap.php';


$sql = "DELETE FROM account WHERE id>0;DELETE FROM subscription WHERE id>0;";

$statement = db()->prepare($sql);
$statement->execute();

?>