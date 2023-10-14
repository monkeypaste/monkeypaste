<?php 

const APP_URL = 'https://www.monkeypaste.com';
const ACCOUNTS_URL = APP_URL.'/accounts';

const NO_REPLY_EMAIL_ADDRESS = 'no-reply@monkeypaste.com';

const SUCCESS_MSG = '[SUCCESS]';
const ERROR_MSG = '[ERROR]';

function println(string $string = '')
{
    print($string . PHP_EOL);
}

function getFormattedDateTimeStr(string $dtStr) {
    $result = new DateTime ($dtStr);
    return $result->format('Y-m-d H:i:s');
}
function is_post_request(): bool
{
    return strtoupper($_SERVER['REQUEST_METHOD']) === 'POST';
}
function is_get_request(): bool
{
    return strtoupper($_SERVER['REQUEST_METHOD']) === 'GET';
}
?>