<?php

const ACCOUNTS_URL = APP_URL . '/accounts';

const NO_REPLY_EMAIL_ADDRESS = 'no-reply@monkeypaste.com';

const SUCCESS_MSG = '[SUCCESS]';
const ERROR_MSG = '[ERROR]';

const TOMORROW_TICKS = 1 * 24 * 60 * 60;
const DEFAULT_EXPIRY_OFFSET = TOMORROW_TICKS;

const SYS_DATETIME_FORMAT = 'Y-m-d H:i:s';

function generate_activation_code(): string
{
    return bin2hex(random_bytes(16));
}
function println(string $string = '')
{
    echo $string . "<br>";
}

function getMaxDateTimeStr()
{
    // from https: //stackoverflow.com/a/3953378

    $dt = new DateTime('1st January 2999');
    $dt->add(DateInterval::createFromDateString('+1 day'));
    return $dt->format(SYS_DATETIME_FORMAT); // 2999-01-02 00:00:00
}

function getFormattedDateTimeStr($dtStr)
{
    if ($dtStr == null) {
        // use now
        $dtStr = date('Y-m-d H:i:s', time());
    }
    $dt = new DateTime($dtStr);
    return $dt->format(SYS_DATETIME_FORMAT);
}
function printerr($err_dict)
{
    foreach ($err_dict as $key => $value) {
        println($value);
    }
}
function is_post_request(): bool
{
    return strtoupper($_SERVER['REQUEST_METHOD']) === 'POST';
}
function is_get_request(): bool
{
    return strtoupper($_SERVER['REQUEST_METHOD']) === 'GET';
}
function exit_w_success($msg = "")
{
    echo SUCCESS_MSG . $msg;
    exit(0);
}

function exit_w_error($msg = "")
{
    echo ERROR_MSG . " " . $msg;
    exit(0);
}
function exit_w_errors(array $errors)
{
    echo json_encode($errors);
    exit(0);
}

function redirect_to(string $url): void
{
    header('Location:' . $url);
    exit;
}
function redirect_with(string $url, array $items): void
{
    foreach ($items as $key => $value) {
        $_SESSION[$key] = $value;
    }

    redirect_to($url);
}
function redirect_with_message(string $url, string $message, string $type = FLASH_SUCCESS)
{
    flash('flash_' . uniqid(), $message, $type);
    redirect_to($url);

}
