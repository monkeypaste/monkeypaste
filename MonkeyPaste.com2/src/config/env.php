<?php
// const DB_HOST = 'localhost';
// const DB_USER = 'monkeypa_tkefauver';
// const DB_PASSWORD = ',)dlyPb@w0h&';
// const DB_NAME = 'monkeypa_mps';
// const APP_URL = 'https://www.monkeypaste.com';

// const DB_HOST = 'localhost';
// const DB_USER = 'root';
// const DB_PASSWORD = '';
// const DB_NAME = 'mp';
// const APP_URL = 'https://localhost';

if(file_exists('/.local')) {
    define('DB_HOST','localhost');
    define('DB_USER','root');
    define('DB_PASSWORD','');
    define('DB_NAME','mp');
    define('APP_URL','https://localhost');
} else {
    define('DB_HOST','localhost');
    define('DB_USER','monkeypa_tkefauver');
    define('DB_PASSWORD',',)dlyPb@w0h&');
    define('DB_NAME','monkeypa_mps');
    define('APP_URL','https://www.monkeypaste.com');
}



?>