<?php

require_once __DIR__ . '/../../src/lib/bootstrap.php';

function exit_w_default_resp()
{
    exit_w_success(json_encode([
        'install_count' => 0,
        'publish_dt' => '',
    ]));
}
function get_plugin_by_guid(string $plugin_guid): mixed
{
    $sql = 'SELECT *
                FROM plugin
                WHERE plugin_guid=:plugin_guid';

    $statement = db()->prepare($sql);

    $statement->bindValue(':plugin_guid', $plugin_guid);
    $statement->execute();

    return $statement->fetch(PDO::FETCH_ASSOC);
}

function exit_w_add_or_update(string $plugin_guid, string $req_version)
{
    $plugin = get_plugin_by_guid($plugin_guid);
    if ($plugin) {
        // update

        if (version_compare($req_version, $plugin['ver']) > 0) {
            // updating version so change publish date
            $publish_dt = getFormattedDateTimeStr(null);

            if (update_ver($plugin_guid, $req_version, $publish_dt)) {
                exit_w_success("plugin '$plugin_guid' version '$req_version' UPDATED");
            } else {
                exit_w_error("error updating plugin '$plugin_guid' with version '$req_version'");
            }
        }
    } else {
        // add
        if (add_plugin($plugin_guid, $req_version)) {
            exit_w_success("plugin '$plugin_guid' version '$req_version' ADDED");
        } else {
            exit_w_error("error updating plugin '$plugin_guid' with version '$req_version'");
        }
    }
    exit_w_success();
}
function exit_w_info_resp(string $plugin_guid, bool $is_installing)
{
    $plugin = get_plugin_by_guid($plugin_guid);
    if (!$plugin) {
        exit_w_default_resp();
    }

    $install_count = $plugin['install_count'];

    if ($is_installing) {
        // increment install count
        $install_count = $install_count + 1;
        if (!update_install_count($plugin_guid, $install_count)) {
            exit_w_error('error incrementing plugin ' . $plugin_guid);
        }
    }
    exit_w_success(json_encode([
        'install_count' => $install_count,
        'publish_dt' => $plugin['publish_dt'],
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
function add_plugin(string $plugin_guid, string $ver): bool
{
    $sql = 'INSERT INTO plugin(plugin_guid, ver, publish_dt)
            VALUES (:plugin_guid, :ver, :publish_dt);';

    $statement = db()->prepare($sql);
    $statement->bindValue(':plugin_guid', $plugin_guid);
    $statement->bindValue(':ver', $ver);
    $statement->bindValue(':publish_dt', getFormattedDateTimeStr(null));

    return $statement->execute();
}

$testdata = [
    'plugin_guid' => '4950 400444',
    'version' => '0.9.0',
    'is_install' => '0',
    'add_phrase' => "Im the big T pot check me out",
];

$fields = [
    'plugin_guid' => 'string | required',
    'version' => 'string',
    'is_install' => 'string',
    'add_phrase' => 'string',
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

if (array_key_exists('add_phrase', $inputs) &&
    array_key_exists('version', $inputs) &&
    $inputs['add_phrase'] == "Im the big T pot check me out") {
    exit_w_add_or_update($inputs['plugin_guid'], $inputs['version']);
}
exit_w_info_resp($inputs['plugin_guid'], $inputs['is_install'] == '1');
