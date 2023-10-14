<?php
/* Database credentials. Assuming you are running MySQL
server with default setting (user 'root' with no password) */

const DB_HOST = 'localhost';
const DB_USER = 'monkeypa_tkefauver';
const DB_PASSWORD = ',)dlyPb@w0h&';
const DB_NAME = 'monkeypa_mps';
 
/**
 * Connect to the database and returns an instance of PDO class
 * or false if the connection fails
 *
 * @return PDO
 */
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
/* Attempt to connect to MySQL database */
// $link = mysqli_connect(DB_SERVER, DB_USERNAME, DB_PASSWORD, DB_NAME);
 
// // Check connection
// if($link === false){
//     die("ERROR: Could not connect. " . mysqli_connect_error());
// }


// function db() {
//     try{
//         db() = new PDO("mysql:host=" . DB_SERVER . ";dbname=" . DB_NAME, DB_USERNAME, DB_PASSWORD);
//         // Set the PDO error mode to exception
//         db()->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
//         return db();
//     } catch(PDOException $e){
//         die("ERROR: Could not connect. " . $e->getMessage());
//     }
//     return NULL;
// }

?>