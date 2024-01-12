<?php

require_once __DIR__ . '/../../src/lib/bootstrap.php';

$sql = "DELETE FROM device WHERE id>0;DELETE FROM subscription WHERE id>0;";

$statement = db()->prepare($sql);
$statement->execute();

$sql = "DELETE FROM account WHERE id>0;";

$statement = db()->prepare($sql);
$statement->execute();

echo SUCCESS_MSG;
