<?php
	$debug = isset($_GET['debug']);
	if ($debug) {
		ini_set("display_errors", "On");
		ini_set("error_reporting", 2047);
	}

	include("../db.php");
	include("charts.php");

	$name = './charts/servers.png';
		
	$query = "SELECT action `key`, count(action) `value` FROM `godspeed_install`
              group by action
              order by count(action) desc";
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}

	$keys = array();
	$values = array();

	$j = 0;
	$o = 0;
	while ($row = mysql_fetch_assoc($rs)) {
		$k = $row['key'];
		$v = $row['value'];
		if ($k == 'I') $k = 'Install ('.$v.')';
		if ($k == 'U') $k = 'Uninstall ('.$v.')';
		array_push($keys, $k);
		array_push($values, $v);
		$j++;
	} 
	
	$data = array(
		"keys" => $keys, 
		"values" => $values
	);
	
	
	$title = 'Install';

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