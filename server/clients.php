<?php
ini_set("display_errors", "On");
ini_set("error_reporting", 2047);
if ($_SERVER['HTTP_USER_AGENT'] != 'GODspeed') exit;

include('db.php');

switch (trim($_POST['participates'])) {
	case 'yes':
		$v = 'true';
		break;
	case 'no':
		$v = 'false';
		break;
	default:
		echo 'Invalid value.';
		exit;
}

$query = sprintf("INSERT INTO godspeed_clients (`client_id`,`date`,`participates`) VALUES ('%s','%s',%s)", mysql_real_escape_string($_POST['client_id']), date('Y-m-d H:i:s', $_POST['date']), $v);

echo $query."\r\n";
if (!mysql_query($query)) {
	echo mysql_error()."\r\n";
}

mysql_close();

?>