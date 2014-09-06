<?php
	$debug = isset($_GET['debug']);
	if ($debug) {
		ini_set("display_errors", "On");
		ini_set("error_reporting", 2047);
	}

	include("../db.php");
	include("charts.php");

	$name = './charts/servers.png';
		
	$query = "SELECT server `key`, sum(count) `value` FROM `godspeed_server_usage`
	          where server != ''
              group by server
              order by sum(count) desc";
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}

	$keys = array();
	$values = array();

	$j = 0;
	$o = 0;
	while ($row = mysql_fetch_assoc($rs)) {
		if ($j < 7) {
			$k = $row['key'];
			if ($k == null || $k == '') $k = '<empty>';
			if (isset($keyr[$k])) $k = $keyr[$k];
			array_push($keys, $k);
			array_push($values, $row['value']);
			$j++;
		} else {
			$o += $row['value'];
		}
	} 
	if ($o > 0) {
		array_push($keys, 'Others');
		array_push($values, $o);
	}
	
	$data = array(
		"keys" => $keys, 
		"values" => $values
	);
	
	
	$title = 'FTP Servers';

	pie($data, $title, $name);

	$fp = fopen($name, 'rb'); 
	
	if (!$debug) {
		header("Content-Type: image/png");
		header("Content-Length: " . filesize($name));
		fpassthru($fp);
	} else {
		echo $query;
		print_r($data);
	}
?>