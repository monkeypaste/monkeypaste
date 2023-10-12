<?php 

function println(string $string = '')
{
    print($string . PHP_EOL);
}

function getFormattedDateTimeStr(string $dtStr) {
    $result = new DateTime ($dtStr);
    return $result->format('Y-m-d H:i:s');
}

?>