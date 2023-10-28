<?php
/* Database credentials. Assuming you are running MySQL
server with default setting (user 'root' with no password) */

require_once __DIR__ . '/env.php';
 
function db(): PDO
{
    static $pdo;
    // if the connection is not initialized
    // connect to the database
    if (!$pdo) {
        $pdo = new PDO(
            sprintf("mysql:host=%s;dbname=%s;charset=UTF8", DB_HOST, DB_NAME),
            DB_USER,
            DB_PASSWORD,
            [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION]
        );
    }
    return $pdo;
}

?>