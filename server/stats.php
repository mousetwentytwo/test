<?php
ini_set("display_errors", "On");
ini_set("error_reporting", 2047);
if ($_SERVER['HTTP_USER_AGENT'] != 'GODspeed') exit;

function visitor_country()
{
    $client  = @$_SERVER['HTTP_CLIENT_IP'];
    $forward = @$_SERVER['HTTP_X_FORWARDED_FOR'];
    $remote  = $_SERVER['REMOTE_ADDR'];
    $result  = "Unknown";
    if(filter_var($client, FILTER_VALIDATE_IP))
    {
        $ip = $client;
    }
    elseif(filter_var($forward, FILTER_VALIDATE_IP))
    {
        $ip = $forward;
    }
    else
    {
        $ip = $remote;
    }

    $ip_data = @json_decode(file_get_contents("http://www.geoplugin.net/json.gp?ip=".$ip));

    if($ip_data && $ip_data->geoplugin_countryName != null)
    {
        $result = $ip_data->geoplugin_countryName;
    }

    return $result;
}

include('db.php');

$columns = '`country`';
$values = "'".visitor_country()."'";

foreach ($_POST as $k => $v) {
	$columns .= ', `'.mysql_real_escape_string($k).'`';
	$values .= ', ';
	$v = trim($v);
	if (is_numeric($v)) {
		$values .= mysql_real_escape_string($v);
	} else {
		$values .= "'".mysql_real_escape_string($v)."'";
	}
}

$query = "INSERT INTO godspeed_stats ($columns) VALUES ($values)";

echo $query;
if (!mysql_query($query)) {
	echo mysql_error();
}

mysql_close();

?>