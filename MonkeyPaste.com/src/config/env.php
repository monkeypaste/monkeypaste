<?php

require_once __DIR__ . '/secrets.php';

// knowing if server is local or remote decided by presence of empty '.local' file
// which isn't synced to the remote server

if (file_exists(__DIR__ . '/.local')) {
     define('DB_HOST', 'localhost');
     define('DB_USER', 'root');
     define('DB_PASSWORD', '');
     define('DB_NAME', 'mp');
     define('APP_URL', 'https://localhost');
     define('CAN_TEST', true);
} else {
     define('DB_HOST', DB_HOST_REMOTE);
     define('DB_USER', DB_USER_REMOTE);
     define('DB_PASSWORD', DB_PASSWORD_REMOTE);
     define('DB_NAME', DB_NAME_REMOTE);
     define('APP_URL', 'https://www.monkeypaste.com');
     define('CAN_TEST', false);
}
