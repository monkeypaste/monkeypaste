<?php

if (file_exists(__DIR__ . '/.local')) {
    define('DB_HOST', 'localhost');
    define('DB_USER', 'root');
    define('DB_PASSWORD', '');
    define('DB_NAME', 'mp');
    define('APP_URL', 'https://localhost');
    define('CAN_TEST', true);
} else {
    define('DB_HOST', 'localhost');
    define('DB_USER', 'monkeypa_tkefauver');
    define('DB_PASSWORD', ',)dlyPb@w0h&');
    define('DB_NAME', 'monkeypa_mp');
    define('APP_URL', 'https://www.monkeypaste.com');
    define('CAN_TEST', false);
}
