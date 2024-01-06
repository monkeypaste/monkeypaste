<?php

require_once __DIR__ . '/../../src/lib/bootstrap.php';

function exit_w_info_resp(string $plugin_guid, string $version, bool $is_installing)
{
    $resp_obj = null;

    $sql = 'SELECT *
            FROM plugin
            WHERE plugin_guid=:plugin_guid';

    $statement = db()->prepare($sql);

    $statement->bindValue(':plugin_guid', $plugin_guid);
    $success = $statement->execute();

    $plugin = $statement->fetch(PDO::FETCH_ASSOC);

    $install_count = 0;
    $publish_dt = getFormattedDateTimeStr(null);
    $cur_ver = '1.0.0';

    if ($plugin) {
        $install_count = $plugin['install_count'];
        $publish_dt = $plugin['publish_dt'];
        $cur_ver = $plugin['ver'];
    } else {
        // add new plugin
        if (!add_plugin($plugin_guid, $version, $publish_dt)) {
            exit_w_error('error adding plugin ' . $plugin_guid . $version);
        }
    }
    $ver_comp_result = version_compare($version, $cur_ver);
    if ($ver_comp_result > 0) {
        // new version detected, update publish date
        $publish_dt = getFormattedDateTimeStr(null);

        if (!update_ver($plugin_guid, $version, $publish_dt)) {
            exit_w_error('error updating plugin ' . $plugin_guid . $version);
        }
    }
    if ($is_installing) {
        // increment install count
        $install_count = $install_count + 1;
        if (!update_install_count($plugin_guid, $install_count)) {
            exit_w_error('error incrementing plugin ' . $plugin_guid . $version);
        }
    }
    exit_w_success(json_encode([
        'install_count' => $install_count,
        'publish_dt' => $publish_dt,
    ]));
}

function update_install_count(string $plugin_guid, int $install_count): bool
{
    $sql = 'UPDATE plugin
            SET install_count=:install_count
            WHERE plugin_guid=:plugin_guid';

    $statement = db()->prepare($sql);
    $statement->bindValue(':install_count', $install_count, PDO::PARAM_INT);
    $statement->bindValue(':plugin_guid', $plugin_guid);

    return $statement->execute();
}

function update_ver(string $plugin_guid, string $ver, string $publish_dt): bool
{
    $sql = 'UPDATE plugin
            SET ver=:ver, publish_dt=:publish_dt
            WHERE plugin_guid=:plugin_guid';

    $statement = db()->prepare($sql);
    $statement->bindValue(':ver', $ver);
    $statement->bindValue(':publish_dt', $publish_dt);
    $statement->bindValue(':plugin_guid', $plugin_guid);

    return $statement->execute();
}
function add_plugin(string $plugin_guid, string $ver, string $publish_dt): bool
{

    $sql = 'INSERT INTO plugin(plugin_guid, ver, publish_dt)
            VALUES (:plugin_guid, :ver, :publish_dt);';

    $statement = db()->prepare($sql);
    $statement->bindValue(':plugin_guid', $plugin_guid);
    $statement->bindValue(':ver', $ver);
    $statement->bindValue(':publish_dt', $publish_dt);

    return $statement->execute();
}

$testdata = [
    'plugin_guid' => '4950',
    'version' => '1.0.0',
    'is_install' => '0',
];

$fields = [
    'plugin_guid' => 'string | required',
    'version' => 'string | required',
    'is_install' => 'string | required',
];

$errors = [];
$inputs = [];

if (is_post_request()) {
    [$inputs, $errors] = filter($_POST, $fields);
} else {
    if (CAN_TEST && isset($testdata)) {
        [$inputs, $errors] = filter($testdata, $fields);
    } else {
        exit_w_error();
    }
}

if ($errors) {
    exit_w_errors($errors);
}

exit_w_info_resp($inputs['plugin_guid'], $inputs['version'], $inputs['is_install'] == '1');
