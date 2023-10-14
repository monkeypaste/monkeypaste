<?php

session_start();

require __DIR__ . '/inc/flash.php';
require __DIR__ . '/inc/header.php';

flash('greeting', 'Hi there', FLASH_INFO);

echo '<a href="page2.php" title="Go To Page 2">Go To Page 2</a>';

require __DIR__ . '/inc/footer.php';
