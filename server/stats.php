<?php
ini_set("display_errors", "On");
ini_set("error_reporting", 2047);
if ($_SERVER['HTTP_USER_AGENT'] != 'GODspeed') exit;

function visitor_country()
{
    $client  = @$_SERVER['HTTP_CLIENT_IP'];
    $forward = @$_SERVER['HTTP_X_FORWARDED_FOR'];
    $remote  = $_SERVER['REMOTE_ADDR'];
    $result  = array();
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

	$data = file_get_contents("http://www.geoplugin.net/json.gp?ip=".$ip);
    $ip_data = @json_decode($data);

    if($ip_data && $ip_data->geoplugin_status == 200)
    {
		$result['ip'] = $ip;
        $result['name'] = $ip_data->geoplugin_countryName;
		$result['code'] = $ip_data->geoplugin_countryCode;
		$result['data'] = $data;
    } else {
		$data = file_get_contents("http://freegeoip.net/json/".$ip);
		$ip_data = @json_decode($data);
		if ($ip_data) {
			$result['ip'] = $ip;
			$result['name'] = $ip_data->country_name;
			$result['code'] = $ip_data->country_code;
			$result['data'] = $data;
		}
	}

    return $result;
}

include('db.php');

$country = visitor_country();
$columns = '`country_code`, `country_name`';
$values = "'".$country['code']."','".$country['name']."'";

if ($country['code'] == '') {
	$f = fopen('./ipdata/'.$country['ip'], 'w');
	fwrite($f, $country['data']);
	fclose($f);
}

foreach ($_POST as $k => $v) {
	if ($k == 'command_usage') {
		$rows = explode("\n", trim($v));
		$c = $_POST['client_id'];
		$d = date('Y-m-d H:i:s', trim($_POST['date']));
		foreach($rows as $row) {
			$r = explode('=', trim($row));
			$query = sprintf("INSERT INTO godspeed_command_usage (client_id, date, command, count) VALUES ('%s','%s','%s','%s')", $c, $d, $r[0], $r[1]);
			
			mysql_query($query);
		}
		continue;
	}
	
	if ($k == 'server_usage') {
		$rows = explode("\n", trim($v));
		$c = $_POST['client_id'];
		$d = date('Y-m-d H:i:s', trim($_POST['date']));
		foreach($rows as $row) {
			$r = explode('=', trim($row));
			$query = sprintf("INSERT INTO godspeed_server_usage (client_id, date, server, count) VALUES ('%s','%s','%s','%s')", $c, $d, $r[0], $r[1]);
			
			mysql_query($query);
		}
		continue;
	}

	$columns .= ', `'.mysql_real_escape_string($k).'`';
	$values .= ', ';
	$v = trim($v);
	switch ($k) {
		case 'usage':
		case 'transfer_time':
			$hour = $v / 3600 % 24;
			if ($hour < 10) $hour = '0'.$hour;
			$minute = $v / 60 % 60;
			if ($minute < 10) $minute = '0'.$minute;
			$second = $v % 60;
			if ($second < 10) $second = '0'.$second;		
			$values .= "'$hour:$minute:$second'";
			break;
		case 'date':
			$values .= "'".date('Y-m-d H:i:s', $v)."'";
			break;
		default:
			if (is_numeric($v)) {
				$values .= mysql_real_escape_string($v);
			} else {
				$values .= "'".mysql_real_escape_string($v)."'";
			}
	}
}

$query = "INSERT INTO godspeed_stats ($columns) VALUES ($values)";

mysql_query($query);
mysql_close();

?>